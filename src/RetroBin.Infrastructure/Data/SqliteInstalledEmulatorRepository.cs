using Dapper;
using RetroBin.Core.Abstractions;
using RetroBin.Core.Models;

namespace RetroBin.Infrastructure.Data;

public sealed class SqliteInstalledEmulatorRepository(SqliteConnectionFactory factory) : IInstalledEmulatorRepository
{
    private const string Select =
        "SELECT EmulatorId, Version, InstallPath, IsActive, InstalledAt FROM InstalledEmulators";

    public async Task<IReadOnlyList<InstalledEmulator>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = factory.Open();
        var rows = await db.QueryAsync<Row>(new CommandDefinition(
            Select + " ORDER BY EmulatorId, InstalledAt DESC;", cancellationToken: ct));
        return rows.Select(ToModel).ToList();
    }

    public async Task<InstalledEmulator?> GetActiveAsync(string emulatorId, CancellationToken ct = default)
    {
        using var db = factory.Open();
        var row = await db.QuerySingleOrDefaultAsync<Row>(new CommandDefinition(
            Select + " WHERE EmulatorId = @emulatorId AND IsActive = 1;",
            new { emulatorId }, cancellationToken: ct));
        return row is null ? null : ToModel(row);
    }

    public async Task RegisterInstallAsync(InstalledEmulator emulator, CancellationToken ct = default)
    {
        using var db = factory.Open();
        using var tx = db.BeginTransaction();

        await db.ExecuteAsync(new CommandDefinition(
            "UPDATE InstalledEmulators SET IsActive = 0 WHERE EmulatorId = @EmulatorId;",
            new { emulator.EmulatorId }, tx, cancellationToken: ct));

        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO InstalledEmulators (EmulatorId, Version, InstallPath, IsActive, InstalledAt)
            VALUES (@EmulatorId, @Version, @InstallPath, 1, @InstalledAt)
            ON CONFLICT(EmulatorId, Version) DO UPDATE SET
                InstallPath = excluded.InstallPath,
                IsActive = 1;
            """,
            new
            {
                emulator.EmulatorId,
                emulator.Version,
                emulator.InstallPath,
                InstalledAt = DbFormat.ToDb(emulator.InstalledAt)
            }, tx, cancellationToken: ct));

        tx.Commit();
    }

    public async Task SetActiveVersionAsync(string emulatorId, string version, CancellationToken ct = default)
    {
        using var db = factory.Open();
        using var tx = db.BeginTransaction();

        var exists = await db.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) FROM InstalledEmulators WHERE EmulatorId = @emulatorId AND Version = @version;",
            new { emulatorId, version }, tx, cancellationToken: ct));
        if (exists == 0)
            throw new InvalidOperationException($"Version {version} de {emulatorId} introuvable.");

        await db.ExecuteAsync(new CommandDefinition(
            "UPDATE InstalledEmulators SET IsActive = (Version = @version) WHERE EmulatorId = @emulatorId;",
            new { emulatorId, version }, tx, cancellationToken: ct));

        tx.Commit();
    }

    public async Task RemoveAsync(string emulatorId, string version, CancellationToken ct = default)
    {
        using var db = factory.Open();
        await db.ExecuteAsync(new CommandDefinition(
            "DELETE FROM InstalledEmulators WHERE EmulatorId = @emulatorId AND Version = @version;",
            new { emulatorId, version }, cancellationToken: ct));
    }

    private static InstalledEmulator ToModel(Row r) =>
        new(r.EmulatorId, r.Version, r.InstallPath, r.IsActive != 0, DbFormat.FromDb(r.InstalledAt));

    private sealed class Row
    {
        public string EmulatorId { get; set; } = "";
        public string Version { get; set; } = "";
        public string InstallPath { get; set; } = "";
        public long IsActive { get; set; }
        public string InstalledAt { get; set; } = "";
    }
}