using RetroBin.Core.Models;

namespace RetroBin.Core.Abstractions;

public interface IInstalledEmulatorRepository
{
    Task<IReadOnlyList<InstalledEmulator>> GetAllAsync(CancellationToken ct = default);
    Task<InstalledEmulator?> GetActiveAsync(string emulatorId, CancellationToken ct = default);

    Task RegisterInstallAsync(InstalledEmulator emulator, CancellationToken ct = default);

    Task SetActiveVersionAsync(string emulatorId, string version, CancellationToken ct = default);
    Task RemoveAsync(string emulatorId, string version, CancellationToken ct = default);
}