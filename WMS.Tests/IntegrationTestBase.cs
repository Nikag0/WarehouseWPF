using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure.Context;
using Xunit;

namespace WMS.Test;

// Каждый тест получает свою пустую БД SQLite в памяти. Она живёт, пока открыто соединение.
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private SqliteConnection _connection = null!;

    protected DbContextOptions<AppDbContext> DbOptions { get; private set; } = null!;

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

    // Отдельный контекст для подготовки и проверки данных, чтобы не видеть объекты из памяти сервиса
    protected AppDbContext CreateContext() => new(DbOptions);

    protected sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDbContextFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public AppDbContext CreateDbContext() => new(_options);
    }
}
