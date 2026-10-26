namespace RetroBin.Core.Emulators;

public sealed record EmulatorProfile
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    public IReadOnlyList<string> Platforms { get; init; } = [];

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Extensions { get; init; }
        = new Dictionary<string, IReadOnlyList<string>>();

    public IReadOnlyDictionary<string, InstallSpec> Install { get; init; }
        = new Dictionary<string, InstallSpec>();
    public IReadOnlyDictionary<string, LaunchSpec> Launch { get; init; }
        = new Dictionary<string, LaunchSpec>();

    public IReadOnlyList<Requirement> Requirements { get; init; } = [];
}