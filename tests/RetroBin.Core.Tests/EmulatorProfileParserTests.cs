using RetroBin.Core.Emulators;

namespace RetroBin.Core.Tests;

public class EmulatorProfileParserTests
{
    private const string Valid = """
        {
          "id": "test-emu",
          "name": "Test Emu",
          "platforms": ["nes", "gbc"],
          "extensions": {
            "nes": [".nes"],
            "gbc": [".gb", ".gbc"]
          },
          "install": {
            "windows": {
              "type": "github-release",
              "repo": "owner/repo",
              "assetPattern": "windows.*\\.zip",
              "exe": "Emu.exe",
              "portableMarker": "portable.txt"
            }
          },
          "launch": {
            "windows": { "args": "{rom}", "fullscreenArgs": "--fullscreen {rom}" }
          },
          "requirements": [
            { "type": "bios", "platform": "nes", "hint": "Place le BIOS ici" }
          ]
        }
        """;

    [Fact]
    public void Parses_a_valid_profile()
    {
        var p = EmulatorProfileParser.Parse(Valid);

        Assert.Equal("test-emu", p.Id);
        Assert.Equal(new[] { "nes", "gbc" }, p.Platforms);
        Assert.Equal(new[] { ".gb", ".gbc" }, p.Extensions["gbc"]);
        Assert.Equal("owner/repo", p.Install["windows"].Repo);
        Assert.Equal("portable.txt", p.Install["windows"].PortableMarker);
        Assert.Equal("{rom}", p.Launch["windows"].Args);
        Assert.Single(p.Requirements);
    }

    [Fact]
    public void Rejects_profile_without_id()
    {
        var json = Valid.Replace("\"id\": \"test-emu\",", "");
        Assert.Throws<InvalidDataException>(() => EmulatorProfileParser.Parse(json));
    }

    [Fact]
    public void Rejects_extension_without_dot()
    {
        var json = Valid.Replace("\".nes\"", "\"nes\"");
        Assert.Throws<InvalidDataException>(() => EmulatorProfileParser.Parse(json));
    }

    [Fact]
    public void Rejects_platform_without_extensions()
    {
        var json = Valid.Replace("\"gbc\": [\".gb\", \".gbc\"]", "\"other\": [\".x\"]");
        Assert.Throws<InvalidDataException>(() => EmulatorProfileParser.Parse(json));
    }

    [Fact]
    public void Rejects_github_release_without_repo()
    {
        var json = Valid.Replace("\"repo\": \"owner/repo\",", "");
        Assert.Throws<InvalidDataException>(() => EmulatorProfileParser.Parse(json));
    }

    [Fact]
    public void Rejects_unknown_install_type()
    {
        var json = Valid.Replace("github-release", "magic");
        Assert.Throws<InvalidDataException>(() => EmulatorProfileParser.Parse(json));
    }

    [Fact]
    public void Rejects_invalid_json()
    {
        Assert.Throws<InvalidDataException>(() => EmulatorProfileParser.Parse("{ pas du json"));
    }
}