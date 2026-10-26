using RetroBin.Core.Emulators;

namespace RetroBin.Infrastructure.Emulators;

public sealed class JsonEmulatorProfileLoader(string profilesFolder) : IEmulatorProfileLoader
{
    public IReadOnlyList<EmulatorProfile> LoadAll()
    {
        if (!Directory.Exists(profilesFolder))
            return [];

        var profiles = new List<EmulatorProfile>();
        foreach (var file in Directory.EnumerateFiles(profilesFolder, "*.json").Order())
        {
            try
            {
                profiles.Add(EmulatorProfileParser.Parse(File.ReadAllText(file)));
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidDataException($"{Path.GetFileName(file)} : {ex.Message}", ex);
            }
        }

        var duplicate = profiles.GroupBy(p => p.Id).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new InvalidDataException($"Deux profils ont le même id : '{duplicate.Key}'.");

        return profiles;
    }
}