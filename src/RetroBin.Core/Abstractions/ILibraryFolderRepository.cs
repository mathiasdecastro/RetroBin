using RetroBin.Core.Models;

namespace RetroBin.Core.Abstractions;

public interface ILibraryFolderRepository
{
    Task<IReadOnlyList<LibraryFolder>> GetAllAsync(CancellationToken ct = default);
    Task<LibraryFolder> AddAsync(string path, string? platformId, CancellationToken ct = default);
    Task RemoveAsync(Guid id, CancellationToken ct = default);
}