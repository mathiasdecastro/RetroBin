using Dapper;
using RetroBin.Core.Abstractions;
using RetroBin.Core.Models;

namespace RetroBin.Infrastructure.Data;

public sealed class SqliteGameRepository(SqliteConnectionFactory factory) : IGameRepository
{
    private const string SelectEntries = """
        SELECT g.Id, g.FilePath, g.PlatformId, g.FileSize, g.Crc32, g.EmulatorOverrideId,
               g.IsMissing, g.AddedAt, g.UpdatedAt,
               m.Title, m.CoverPath, m.Source, m.UpdatedAt AS MetaUpdatedAt
        FROM Games g
        LEFT JOIN GameMetadata m ON m.GameId = g.Id
        """;

    public async Task<IReadOnlyList<GameEntry>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = factory.Open();
        var rows = await db.QueryAsync<Row>(new CommandDefinition(
            SelectEntries + " ORDER BY COALESCE(m.Title, g.FilePath) COLLATE NOCASE;",
            cancellationToken: ct));
        return rows.Select(ToEntry).ToList();
    }

    public async Task<GameEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var db = factory.Open();
        var row = await db.QuerySingleOrDefaultAsync<Row>(new CommandDefinition(
            SelectEntries + " WHERE g.Id = @Id;",
            new { Id = DbFormat.ToDb(id) }, cancellationToken: ct));
        return row is null ? null : ToEntry(row);
    }

    public async Task<Guid> UpsertScannedAsync(Game game, CancellationToken ct = default)
    {
        using var db = factory.Open();
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO Games (Id, FilePath, PlatformId, FileSize, Crc32, EmulatorOverrideId, IsMissing, AddedAt, UpdatedAt)
            VALUES (@Id, @FilePath, @PlatformId, @FileSize, @Crc32, @EmulatorOverrideId, 0, @AddedAt, @UpdatedAt)
            ON CONFLICT(FilePath) DO UPDATE SET
                PlatformId = excluded.PlatformId,
                FileSize   = excluded.FileSize,
                IsMissing  = 0,
                UpdatedAt  = excluded.UpdatedAt;
            """,
            new
            {
                Id = DbFormat.ToDb(game.Id),
                game.FilePath,
                game.PlatformId,
                game.FileSize,
                game.Crc32,
                game.EmulatorOverrideId,
                AddedAt = DbFormat.ToDb(game.AddedAt),
                UpdatedAt = DbFormat.ToDb(game.UpdatedAt)
            }, cancellationToken: ct));

        var id = await db.ExecuteScalarAsync<string>(new CommandDefinition(
            "SELECT Id FROM Games WHERE FilePath = @FilePath;", new { game.FilePath }, cancellationToken: ct));
        return Guid.Parse(id!);
    }

    public Task SetCrc32Async(Guid id, string crc32, CancellationToken ct = default) =>
        ExecAsync("UPDATE Games SET Crc32 = @Value, UpdatedAt = @Now WHERE Id = @Id;",
            new { Id = DbFormat.ToDb(id), Value = crc32, Now = DbFormat.ToDb(DateTimeOffset.UtcNow) }, ct);

    public Task SetMissingAsync(Guid id, bool isMissing, CancellationToken ct = default) =>
        ExecAsync("UPDATE Games SET IsMissing = @Value, UpdatedAt = @Now WHERE Id = @Id;",
            new { Id = DbFormat.ToDb(id), Value = isMissing ? 1 : 0, Now = DbFormat.ToDb(DateTimeOffset.UtcNow) }, ct);

    public Task SetEmulatorOverrideAsync(Guid id, string? emulatorId, CancellationToken ct = default) =>
        ExecAsync("UPDATE Games SET EmulatorOverrideId = @Value, UpdatedAt = @Now WHERE Id = @Id;",
            new { Id = DbFormat.ToDb(id), Value = emulatorId, Now = DbFormat.ToDb(DateTimeOffset.UtcNow) }, ct);

    public Task SaveMetadataAsync(GameMetadata metadata, CancellationToken ct = default) =>
        ExecAsync("""
            INSERT INTO GameMetadata (GameId, Title, CoverPath, Source, UpdatedAt)
            VALUES (@GameId, @Title, @CoverPath, @Source, @UpdatedAt)
            ON CONFLICT(GameId) DO UPDATE SET
                Title = excluded.Title,
                CoverPath = excluded.CoverPath,
                Source = excluded.Source,
                UpdatedAt = excluded.UpdatedAt;
            """,
            new
            {
                GameId = DbFormat.ToDb(metadata.GameId),
                metadata.Title,
                metadata.CoverPath,
                metadata.Source,
                UpdatedAt = DbFormat.ToDb(metadata.UpdatedAt)
            }, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default) =>
        ExecAsync("DELETE FROM Games WHERE Id = @Id;", new { Id = DbFormat.ToDb(id) }, ct);

    private async Task ExecAsync(string sql, object param, CancellationToken ct)
    {
        using var db = factory.Open();
        await db.ExecuteAsync(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    private static GameEntry ToEntry(Row r)
    {
        var game = new Game
        {
            Id = Guid.Parse(r.Id),
            FilePath = r.FilePath,
            PlatformId = r.PlatformId,
            FileSize = r.FileSize,
            Crc32 = r.Crc32,
            EmulatorOverrideId = r.EmulatorOverrideId,
            IsMissing = r.IsMissing != 0,
            AddedAt = DbFormat.FromDb(r.AddedAt),
            UpdatedAt = DbFormat.FromDb(r.UpdatedAt)
        };

        var metadata = r.Title is null
            ? null
            : new GameMetadata(game.Id, r.Title, r.CoverPath, r.Source ?? "unknown",
                DbFormat.FromDb(r.MetaUpdatedAt ?? r.UpdatedAt));

        return new GameEntry(game, metadata);
    }

    private sealed class Row
    {
        public string Id { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string PlatformId { get; set; } = "";
        public long FileSize { get; set; }
        public string? Crc32 { get; set; }
        public string? EmulatorOverrideId { get; set; }
        public long IsMissing { get; set; }
        public string AddedAt { get; set; } = "";
        public string UpdatedAt { get; set; } = "";
        public string? Title { get; set; }
        public string? CoverPath { get; set; }
        public string? Source { get; set; }
        public string? MetaUpdatedAt { get; set; }
    }
}