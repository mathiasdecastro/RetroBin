namespace RetroBin.Core.Models;

public sealed record Game
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FilePath { get; init; }
    public required string PlatformId { get; init; }
    public long FileSize { get; init; }
    public string? Crc32 { get; init; }
    public string? EmulatorOverrideId { get; init; }
    public bool IsMissing { get; init; }
    public DateTimeOffset AddedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}