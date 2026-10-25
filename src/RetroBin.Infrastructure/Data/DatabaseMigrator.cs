using Dapper;

namespace RetroBin.Infrastructure.Data;

public static class DatabaseMigrator
{
    private static readonly string[] Migrations =
    [
        """
        CREATE TABLE LibraryFolders (
          Id         TEXT PRIMARY KEY,
          Path       TEXT NOT NULL UNIQUE,
          PlatformId TEXT NULL,
          AddedAt    TEXT NOT NULL
        );

        CREATE TABLE Games (
          Id                 TEXT PRIMARY KEY,
          FilePath           TEXT NOT NULL UNIQUE,
          PlatformId         TEXT NOT NULL,
          FileSize           INTEGER NOT NULL,
          Crc32              TEXT NULL,
          EmulatorOverrideId TEXT NULL,
          IsMissing          INTEGER NOT NULL DEFAULT 0,
          AddedAt            TEXT NOT NULL,
          UpdatedAt          TEXT NOT NULL
        );
        CREATE INDEX IX_Games_PlatformId ON Games(PlatformId);
        CREATE INDEX IX_Games_Crc32 ON Games(Crc32);

        CREATE TABLE GameMetadata (
          GameId    TEXT PRIMARY KEY REFERENCES Games(Id) ON DELETE CASCADE,
          Title     TEXT NOT NULL,
          CoverPath TEXT NULL,
          Source    TEXT NOT NULL,
          UpdatedAt TEXT NOT NULL
        );

        CREATE TABLE InstalledEmulators (
          EmulatorId  TEXT NOT NULL,
          Version     TEXT NOT NULL,
          InstallPath TEXT NOT NULL,
          IsActive    INTEGER NOT NULL DEFAULT 0,
          InstalledAt TEXT NOT NULL,
          PRIMARY KEY (EmulatorId, Version)
        );

        CREATE TABLE PlatformSettings (
          PlatformId        TEXT PRIMARY KEY,
          DefaultEmulatorId TEXT NULL
        );
        """
    ];

    public static void Migrate(SqliteConnectionFactory factory)
    {
        using var connection = factory.Open();
        var current = (int)connection.ExecuteScalar<long>("PRAGMA user_version;");

        for (var i = current; i < Migrations.Length; i++)
        {
            using var tx = connection.BeginTransaction();
            connection.Execute(Migrations[i], transaction: tx);
            connection.Execute($"PRAGMA user_version = {i + 1};", transaction: tx);
            tx.Commit();
        }
    }
}