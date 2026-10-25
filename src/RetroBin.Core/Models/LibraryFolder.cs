namespace RetroBin.Core.Models;

public sealed record LibraryFolder(Guid Id, string Path, string? PlatformId, DateTimeOffset AddedAt);