namespace RetroBin.Core.Emulators;

public sealed record LaunchSpec
{
    public required string Args { get; init; }

    public string? FullscreenArgs { get; init; }
}