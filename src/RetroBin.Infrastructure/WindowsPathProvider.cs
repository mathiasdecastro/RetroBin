using RetroBin.Core.Abstractions;

namespace RetroBin.Infrastructure;

public sealed class WindowsPathProvider : IPathProvider
{
    public string AppDataRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RetroBin");

    public string DatabasePath => Path.Combine(AppDataRoot, "retrobin.db");
    public string EmulatorsRoot => Path.Combine(AppDataRoot, "emulators");
    public string CoverCacheRoot => Path.Combine(AppDataRoot, "covers");
}