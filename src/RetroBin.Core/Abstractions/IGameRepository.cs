using RetroBin.Core.Models;

namespace RetroBin.Core.Abstractions;

public interface IGameRepository
{
    Task<IReadOnlyList<GameEntry>> GetAllAsync(CancellationToken ct = default);
    Task<GameEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Guid> UpsertScannedAsync(Game game, CancellationToken ct = default);

    Task SetCrc32Async(Guid id, string crc32, CancellationToken ct = default);
    Task SetMissingAsync(Guid id, bool isMissing, CancellationToken ct = default);
    Task SetEmulatorOverrideAsync(Guid id, string? emulatorId, CancellationToken ct = default);
    Task SaveMetadataAsync(GameMetadata metadata, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}