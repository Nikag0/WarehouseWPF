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
    private HistoryService CreateService()
    {
        var holder = new DbContextHolder();

        return new HistoryService(
            new HistoryRepository(holder),
            new UnitOfWorkFactory(new TestDbContextFactory(DbOptions), holder));
    }

    // Время операции задаётся при создании, поэтому для теста переписываем его напрямую в БД
    private async Task SeedOperationAsync(DateTime occurredAtUtc, string componentName)
    {
        var operation = OperationHistory.Create(
            OperationType.Receipt, "Оператор", null,
            Guid.NewGuid(), componentName,
            Guid.NewGuid(), "А-1", Guid.NewGuid(), "А-3",
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
