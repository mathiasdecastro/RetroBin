using Dapper;
using RetroBin.Core.Models;
using RetroBin.Infrastructure.Data;

namespace RetroBin.Infrastructure.Tests;

public class DatabaseTests : IDisposable
{
    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private static Game NewGame(string path = @"D:\ROMs\NES\truc_final_v2.nes") =>
        new() { FilePath = path, PlatformId = "nes", FileSize = 40976 };

    [Fact]
    public void Migrate_twice_is_idempotent_and_sets_version()
    {
        DatabaseMigrator.Migrate(_db.Factory);
        DatabaseMigrator.Migrate(_db.Factory);

        using var c = _db.Factory.Open();
        Assert.Equal(1, c.ExecuteScalar<long>("PRAGMA user_version;"));
    }

    [Fact]
    public async Task Upsert_same_path_keeps_original_id()
    {
        var repo = new SqliteGameRepository(_db.Factory);

        var first = await repo.UpsertScannedAsync(NewGame());
        var second = await repo.UpsertScannedAsync(NewGame() with { Id = Guid.NewGuid() });

        Assert.Equal(first, second);
        Assert.Single(await repo.GetAllAsync());
    }

    [Fact]
    public async Task Metadata_is_joined_and_survives_rescan()
    {
        var repo = new SqliteGameRepository(_db.Factory);
        var id = await repo.UpsertScannedAsync(NewGame());
        await repo.SaveMetadataAsync(new GameMetadata(id, "Super Mario Bros.", null, "manual", DateTimeOffset.UtcNow));

        await repo.UpsertScannedAsync(NewGame());

        var entry = await repo.GetByIdAsync(id);
        Assert.Equal("Super Mario Bros.", entry!.Metadata!.Title);
        Assert.Equal("Super Mario Bros.", entry.DisplayTitle);
    }

    [Fact]
    public async Task Game_without_metadata_falls_back_to_file_name()
    {
        var repo = new SqliteGameRepository(_db.Factory);
        var id = await repo.UpsertScannedAsync(NewGame());

        var entry = await repo.GetByIdAsync(id);
        Assert.Null(entry!.Metadata);
        Assert.Equal("truc_final_v2", entry.DisplayTitle);
    }

    [Fact]
    public async Task Deleting_game_cascades_to_metadata()
    {
        var repo = new SqliteGameRepository(_db.Factory);
        var id = await repo.UpsertScannedAsync(NewGame());
        await repo.SaveMetadataAsync(new GameMetadata(id, "X", null, "manual", DateTimeOffset.UtcNow));

        await repo.DeleteAsync(id);

        using var c = _db.Factory.Open();
        Assert.Equal(0, c.ExecuteScalar<long>("SELECT COUNT(*) FROM GameMetadata;"));
    }

    [Fact]
    public async Task Missing_flag_is_cleared_when_file_is_found_again()
    {
        var repo = new SqliteGameRepository(_db.Factory);
        var id = await repo.UpsertScannedAsync(NewGame());
        await repo.SetMissingAsync(id, true);
        Assert.True((await repo.GetByIdAsync(id))!.Game.IsMissing);

        await repo.UpsertScannedAsync(NewGame());
        Assert.False((await repo.GetByIdAsync(id))!.Game.IsMissing);
    }

    [Fact]
    public async Task Installing_new_version_makes_it_the_only_active_one()
    {
        var repo = new SqliteInstalledEmulatorRepository(_db.Factory);
        var now = DateTimeOffset.UtcNow;

        await repo.RegisterInstallAsync(new InstalledEmulator("mesen2", "1.0", @"C:\e\1.0", true, now));
        await repo.RegisterInstallAsync(new InstalledEmulator("mesen2", "1.1", @"C:\e\1.1", true, now.AddMinutes(1)));

        Assert.Equal("1.1", (await repo.GetActiveAsync("mesen2"))!.Version);

        await repo.SetActiveVersionAsync("mesen2", "1.0");
        Assert.Equal("1.0", (await repo.GetActiveAsync("mesen2"))!.Version);
        Assert.Equal(1, (await repo.GetAllAsync()).Count(e => e.IsActive));
    }

    [Fact]
    public async Task Folders_are_unique_by_path()
    {
        var repo = new SqliteLibraryFolderRepository(_db.Factory);

        var a = await repo.AddAsync(@"D:\ROMs\NES", "nes");
        var b = await repo.AddAsync(@"D:\ROMs\NES", "nes");

        Assert.Equal(a.Id, b.Id);
        Assert.Single(await repo.GetAllAsync());
    }

    [Fact]
    public async Task Platform_default_emulator_can_be_set_and_changed()
    {
        var repo = new SqlitePlatformSettingsRepository(_db.Factory);

        Assert.Null(await repo.GetDefaultEmulatorAsync("nes"));
        await repo.SetDefaultEmulatorAsync("nes", "mesen2");
        await repo.SetDefaultEmulatorAsync("nes", "mesen2-alt");
        Assert.Equal("mesen2-alt", await repo.GetDefaultEmulatorAsync("nes"));
    }
}