namespace RetroBin.Core.Models;

public sealed record GameMetadata(
    Guid GameId,
    string Title,
    string? CoverPath,
    string Source,
    DateTimeOffset UpdatedAt);