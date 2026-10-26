using System.Text.Json;

namespace RetroBin.Core.Emulators;

public static class EmulatorProfileParser
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static EmulatorProfile Parse(string json)
    {
        EmulatorProfile? profile;
        try
        {
            profile = JsonSerializer.Deserialize<EmulatorProfile>(json, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Profil d'émulateur illisible : {ex.Message}", ex);
        }

        if (profile is null)
            throw new InvalidDataException("Profil d'émulateur vide.");

        Validate(profile);
        return profile;
    }

    private static void Validate(EmulatorProfile p)
    {
        if (string.IsNullOrWhiteSpace(p.Id))
            throw new InvalidDataException("Profil sans 'id'.");
        if (string.IsNullOrWhiteSpace(p.Name))
            throw new InvalidDataException($"Profil '{p.Id}' : 'name' manquant.");
        if (p.Platforms.Count == 0)
            throw new InvalidDataException($"Profil '{p.Id}' : aucune plateforme.");

        foreach (var platform in p.Platforms)
        {
            if (!p.Extensions.TryGetValue(platform, out var exts) || exts.Count == 0)
                throw new InvalidDataException($"Profil '{p.Id}' : aucune extension pour '{platform}'.");

            foreach (var ext in exts)
                if (!ext.StartsWith('.'))
                    throw new InvalidDataException(
                        $"Profil '{p.Id}' : l'extension '{ext}' doit commencer par un point.");
        }

        foreach (var (os, install) in p.Install)
        {
            switch (install.Type)
            {
                case "github-release" when string.IsNullOrWhiteSpace(install.Repo):
                    throw new InvalidDataException($"Profil '{p.Id}' ({os}) : 'repo' manquant.");
                case "direct-url" when string.IsNullOrWhiteSpace(install.Url):
                    throw new InvalidDataException($"Profil '{p.Id}' ({os}) : 'url' manquant.");
                case "github-release" or "direct-url":
                    break;
                default:
                    throw new InvalidDataException(
                        $"Profil '{p.Id}' ({os}) : type d'installation inconnu '{install.Type}'.");
            }
        }
    }
}