namespace RetroBin.Core.Models;

public sealed record InstalledEmulator(
    string EmulatorId,
    string Version,
    string InstallPath,
    bool IsActive,
    DateTimeOffset InstalledAt);