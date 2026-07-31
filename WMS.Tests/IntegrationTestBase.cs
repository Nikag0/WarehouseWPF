using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure;
using Xunit;

namespace WMS.Test;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private SqliteConnection _connection;

    protected DbContextOptions<AppDbContext> DbOptions { get; private set; }

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        DbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(DbOptions);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}