using Microsoft.Data.Sqlite;
using RetroBin.Infrastructure.Data;

namespace RetroBin.Infrastructure.Tests;

public sealed class TestDatabase : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"retrobin-test-{Guid.NewGuid():N}.db");

    public SqliteConnectionFactory Factory { get; }

    public TestDatabase()
    {
        Factory = new SqliteConnectionFactory(_path);
        DatabaseMigrator.Migrate(Factory);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var f in new[] { _path, _path + "-wal", _path + "-shm", _path + "-journal" })
            if (File.Exists(f)) File.Delete(f);
    }
}