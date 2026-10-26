namespace RetroBin.Core.Emulators;

public sealed record Requirement
{
    public required string Type { get; init; }
    public string? Platform { get; init; }
    public required string Hint { get; init; }
}