using Microsoft.EntityFrameworkCore;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Infrastructure;
using WMS.Infrastructure.Context;
using Xunit;
using Assert = Xunit.Assert;

namespace WMS.Test.IntegrationTests.Services;

public class HistoryServiceIntegrationTests : IntegrationTestBase
{
    // Часовой пояс склада
    private static readonly TimeZoneInfo Gmt7 =
        TimeZoneInfo.CreateCustomTimeZone("GMT+7", TimeSpan.FromHours(7), "GMT+7", "GMT+7");

    // По умолчанию UTC, чтобы результат тестов не зависел от часового пояса компьютера, на котором они запущены
    private HistoryService CreateService(TimeZoneInfo? timeZone = null)
    {
        var holder = new DbContextHolder();

        return new HistoryService(
            new HistoryRepository(holder),
            new UnitOfWorkFactory(new TestDbContextFactory(DbOptions), holder),
            timeZone ?? TimeZoneInfo.Utc);
    }

    // Операции вокруг местных суток 2 ноября по GMT+7 (местные сутки = 1 ноября 17:00 UTC … 2 ноября 17:00 UTC)
    private async Task SeedAroundLocalDayAsync()
    {
        await SeedOperationAsync(new DateTime(2026, 11, 1, 16, 30, 0, DateTimeKind.Utc), "1 ноября 23:30 местного");
        await SeedOperationAsync(new DateTime(2026, 11, 1, 18, 30, 0, DateTimeKind.Utc), "2 ноября 01:30 местного");
        await SeedOperationAsync(new DateTime(2026, 11, 2, 16, 30, 0, DateTimeKind.Utc), "2 ноября 23:30 местного");
        await SeedOperationAsync(new DateTime(2026, 11, 2, 17, 30, 0, DateTimeKind.Utc), "3 ноября 00:30 местного");
    }

    [Fact]
    public async Task GetFilteredAsync_Gmt7_ShowsLocalTimeAndFiltersByLocalDay()
    {
        await SeedAroundLocalDayAsync();
        var day = new DateTime(2026, 11, 2);

        var result = await CreateService(Gmt7).GetFilteredAsync(null, day, day, null, 100, CancellationToken.None);

        // Сортировка от новых к старым
        Assert.Equal(new[] { "2 ноября 23:30 местного", "2 ноября 01:30 местного" }, result.Select(r => r.ComponentName));
        Assert.Equal(new DateTime(2026, 11, 2, 23, 30, 0), result[0].OccurredAt);
        Assert.Equal(new DateTime(2026, 11, 2, 1, 30, 0), result[1].OccurredAt);
    }

    [Fact]
    public async Task DeleteHistoryAsync_Gmt7_RemovesOnlyOperationsOfLocalDay()
    {
        await SeedAroundLocalDayAsync();
        var day = new DateTime(2026, 11, 2);

        await CreateService(Gmt7).DeleteHistoryAsync(day, day);

        await using var db = CreateContext();
        var remaining = await db.OperationHistory.OrderBy(o => o.OccurredAt).Select(o => o.ComponentName).ToListAsync();
        Assert.Equal(new[] { "1 ноября 23:30 местного", "3 ноября 00:30 местного" }, remaining);
    }

    [Fact]
    public async Task ExportCsvAsync_Gmt7_WritesLocalTime()
    {
        await SeedAroundLocalDayAsync();
        var day = new DateTime(2026, 11, 2);
        var filePath = Path.Combine(Path.GetTempPath(), $"history-test-{Guid.NewGuid()}.csv");

        try
        {
            await CreateService(Gmt7).ExportCsvAsync(filePath, day, day);

            var lines = await File.ReadAllLinesAsync(filePath);

            Assert.Equal(3, lines.Length);
            Assert.Contains("02.11.2026 01:30", lines[1]);
            Assert.Contains("02.11.2026 23:30", lines[2]);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    // Время операции задаётся при создании, поэтому для теста переписываем его напрямую в БД
    private async Task SeedOperationAsync(DateTime occurredAtUtc, string componentName)
    {
        var operation = OperationHistory.Create(
            OperationType.Receipt, "Оператор", null,
            componentName, "А-1", "А-3",
            0, 5, 5);

        await using var db = CreateContext();
        db.OperationHistory.Add(operation);
        db.Entry(operation).Property(o => o.OccurredAt).CurrentValue = occurredAtUtc;
        await db.SaveChangesAsync();
    }

    private async Task SeedThreeDaysAsync()
    {
        await SeedOperationAsync(new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc), "До периода");
        await SeedOperationAsync(new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc), "В периоде 1");
        await SeedOperationAsync(new DateTime(2026, 11, 3, 23, 30, 0, DateTimeKind.Utc), "В периоде 2");
        await SeedOperationAsync(new DateTime(2026, 11, 4, 0, 30, 0, DateTimeKind.Utc), "После периода");
    }

    [Fact]
    public async Task DeleteHistoryAsync_RemovesOnlyOperationsInPeriod()
    {
        await SeedThreeDaysAsync();

        await CreateService().DeleteHistoryAsync(new DateTime(2026, 11, 2), new DateTime(2026, 11, 3));

        await using var db = CreateContext();
        var remaining = await db.OperationHistory.OrderBy(o => o.OccurredAt).Select(o => o.ComponentName).ToListAsync();
        Assert.Equal(new[] { "До периода", "После периода" }, remaining);
    }

    [Fact]
    public async Task ExportCsvAsync_WritesExactlyOperationsThatDeleteWouldRemove()
    {
        await SeedThreeDaysAsync();
        var filePath = Path.Combine(Path.GetTempPath(), $"history-test-{Guid.NewGuid()}.csv");

        try
        {
            await CreateService().ExportCsvAsync(filePath, new DateTime(2026, 11, 2), new DateTime(2026, 11, 3));

            var lines = await File.ReadAllLinesAsync(filePath);

            // Заголовок и две операции из периода
            Assert.Equal(3, lines.Length);
            Assert.Contains("Компонент", lines[0]);
            Assert.Contains("В периоде 1", lines[1]);
            Assert.Contains("В периоде 2", lines[2]);
            Assert.DoesNotContain(lines, l => l.Contains("До периода") || l.Contains("После периода"));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ExportCsvAsync_EmptyPeriod_WritesOnlyHeader()
    {
        await SeedThreeDaysAsync();
        var filePath = Path.Combine(Path.GetTempPath(), $"history-test-{Guid.NewGuid()}.csv");

        try
        {
            await CreateService().ExportCsvAsync(filePath, new DateTime(2026, 10, 1), new DateTime(2026, 10, 2));

            Assert.Single(await File.ReadAllLinesAsync(filePath));
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
