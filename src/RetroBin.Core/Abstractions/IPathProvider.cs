namespace RetroBin.Core.Abstractions;

public interface IPathProvider
{
    string AppDataRoot { get; }
    string DatabasePath { get; }
    string EmulatorsRoot { get; }
    string CoverCacheRoot { get; }
}