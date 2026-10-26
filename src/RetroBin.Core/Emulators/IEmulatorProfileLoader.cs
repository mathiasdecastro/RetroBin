namespace RetroBin.Core.Emulators;

public interface IEmulatorProfileLoader
{
    IReadOnlyList<EmulatorProfile> LoadAll();
}