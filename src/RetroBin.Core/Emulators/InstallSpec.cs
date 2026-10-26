namespace RetroBin.Core.Emulators;

public sealed record InstallSpec
{
    public required string Type { get; init; }

    public string? Repo { get; init; }
    public string? Url { get; init; }
    public string? AssetPattern { get; init; }
    public required string Exe { get; init; }
    public string? PortableMarker { get; init; }
}