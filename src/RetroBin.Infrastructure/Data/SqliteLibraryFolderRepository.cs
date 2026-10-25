using Dapper;
using RetroBin.Core.Abstractions;
using RetroBin.Core.Models;

namespace RetroBin.Infrastructure.Data;

public sealed class SqliteLibraryFolderRepository(SqliteConnectionFactory factory) : ILibraryFolderRepository
{
    public async Task<IReadOnlyList<LibraryFolder>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = factory.Open();
        var rows = await db.QueryAsync<Row>(new CommandDefinition(
            "SELECT Id, Path, PlatformId, AddedAt FROM LibraryFolders ORDER BY Path COLLATE NOCASE;",
            cancellationToken: ct));
        return rows.Select(ToModel).ToList();
    }

    public async Task<LibraryFolder> AddAsync(string path, string? platformId, CancellationToken ct = default)
    {
        using var db = factory.Open();
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT OR IGNORE INTO LibraryFolders (Id, Path, PlatformId, AddedAt)
            VALUES (@Id, @Path, @PlatformId, @AddedAt);
            """,
            new
            {
                Id = DbFormat.ToDb(Guid.NewGuid()),
                Path = path,
                PlatformId = platformId,
                AddedAt = DbFormat.ToDb(DateTimeOffset.UtcNow)
            }, cancellationToken: ct));

        var row = await db.QuerySingleAsync<Row>(new CommandDefinition(
            "SELECT Id, Path, PlatformId, AddedAt FROM LibraryFolders WHERE Path = @Path;",
            new { Path = path }, cancellationToken: ct));
        return ToModel(row);
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct = default)
    {
        using var db = factory.Open();
        await db.ExecuteAsync(new CommandDefinition(
            "DELETE FROM LibraryFolders WHERE Id = @Id;", new { Id = DbFormat.ToDb(id) }, cancellationToken: ct));
    }

    private static LibraryFolder ToModel(Row r) =>
        new(Guid.Parse(r.Id), r.Path, r.PlatformId, DbFormat.FromDb(r.AddedAt));

    private sealed class Row
    {
        public string Id { get; set; } = "";
        public string Path { get; set; } = "";
        public string? PlatformId { get; set; }
        public string AddedAt { get; set; } = "";
    }
}