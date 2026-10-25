namespace RetroBin.Core.Abstractions;

public interface IPlatformSettingsRepository
{
    Task<string?> GetDefaultEmulatorAsync(string platformId, CancellationToken ct = default);
    Task SetDefaultEmulatorAsync(string platformId, string? emulatorId, CancellationToken ct = default);
}