namespace RetroBin.Core.Models;

public sealed record GameEntry(Game Game, GameMetadata? Metadata)
{
    public string DisplayTitle => Metadata?.Title ?? Path.GetFileNameWithoutExtension(Game.FilePath);
}