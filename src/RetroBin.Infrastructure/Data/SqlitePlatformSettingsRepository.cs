using Dapper;
using RetroBin.Core.Abstractions;

namespace RetroBin.Infrastructure.Data;

public sealed class SqlitePlatformSettingsRepository(SqliteConnectionFactory factory) : IPlatformSettingsRepository
{
    public async Task<string?> GetDefaultEmulatorAsync(string platformId, CancellationToken ct = default)
    {
        using var db = factory.Open();
        return await db.ExecuteScalarAsync<string?>(new CommandDefinition(
            "SELECT DefaultEmulatorId FROM PlatformSettings WHERE PlatformId = @platformId;",
            new { platformId }, cancellationToken: ct));
    }

    public async Task SetDefaultEmulatorAsync(string platformId, string? emulatorId, CancellationToken ct = default)
    {
        using var db = factory.Open();
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO PlatformSettings (PlatformId, DefaultEmulatorId) VALUES (@platformId, @emulatorId)
            ON CONFLICT(PlatformId) DO UPDATE SET DefaultEmulatorId = excluded.DefaultEmulatorId;
            """,
            new { platformId, emulatorId }, cancellationToken: ct));
    }
}