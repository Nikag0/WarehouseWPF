using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WMS.Application;
using WMS.Application.DTO;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using WMS.Infrastructure;
using WMS.Infrastructure.Context;
using Xunit;
using Assert = Xunit.Assert;

namespace WMS.Test.IntegrationTests.Services;

// Проверяют выдачу и приёмку целиком: сервис, репозитории и транзакцию на реальной БД (SQLite в памяти)
public class StockServiceIntegrationTests : IntegrationTestBase
{
    private Component _component = null!;
    private Rack _rack = null!;
    private Cell _cell = null!;
    private Cell _secondCell = null!;

    private async Task SeedCatalogAsync()
    {
        _component = Component.Create("ART-001", "Резистор 10 кОм", "Yageo", null, 5);
        _rack = new Rack(1, 2, RackType.R1);
        _cell = new Cell(_rack.Id, 3, 1);
        _secondCell = new Cell(_rack.Id, 4, 1);

        await using var db = CreateContext();
        db.Component.Add(_component);
        db.Rack.Add(_rack);
        db.Cell.AddRange(_cell, _secondCell);
        await db.SaveChangesAsync();
    }

    private async Task<Stock> SeedStockAsync(Cell cell, int quantity)
    {
        var stock = Stock.Create(_component.Id, _rack.Id, cell.Id, quantity);

        await using var db = CreateContext();
        db.Stock.Add(stock);
        await db.SaveChangesAsync();

        return stock;
    }

    private StockService CreateService()
    {
        var holder = new DbContextHolder();

        return new StockService(
            new ComponentRepository(holder),
            new StockRepository(holder),
            new HistoryRepository(holder),
            NullLogger<StockService>.Instance,
            new UnitOfWorkFactory(new TestDbContextFactory(DbOptions), holder));
    }

    private ReceiptItemDto ReceiptFor(Cell cell, int quantity) =>
        new(_component.Id, _rack.Id, "rack", cell.Id, "cell", quantity);

    private IssueStockDto IssueFor(Stock stock, int quantity) =>
        new(stock.Id, quantity, stock.RackId, "rack", stock.CellId, "cell", stock.ComponentId, "component");

    // -------- Приёмка --------

    [Fact]
    public async Task ReceiveAsync_EmptyCell_CreatesStockAndHistory()
    {
        await SeedCatalogAsync();

        await CreateService().ReceiveAsync(ReceiptFor(_cell, 5), "Иванов И. И.", "Поставка");

        await using var db = CreateContext();
        var stock = await db.Stock.SingleAsync();
        Assert.Equal(5, stock.Quantity);
        Assert.Equal(_cell.Id, stock.CellId);

        var history = await db.OperationHistory.SingleAsync();
        Assert.Equal(OperationType.Receipt, history.Type);
        Assert.Equal(_component.Name, history.ComponentName);
        Assert.Equal("Иванов И. И.", history.Operator);
        Assert.Equal("Поставка", history.Comment);
        Assert.Equal(0, history.PreviousQuantity);
        Assert.Equal(5, history.QuantityChanged);
        Assert.Equal(5, history.NewQuantity);
    }

    [Fact]
    public async Task ReceiveAsync_ExistingStock_AddsToSameStock()
    {
        await SeedCatalogAsync();
        await SeedStockAsync(_cell, 3);

        await CreateService().ReceiveAsync(ReceiptFor(_cell, 4), "Оператор");

        await using var db = CreateContext();
        var stock = await db.Stock.SingleAsync();
        Assert.Equal(7, stock.Quantity);

        var history = await db.OperationHistory.SingleAsync();
        Assert.Equal(3, history.PreviousQuantity);
        Assert.Equal(7, history.NewQuantity);
    }

    [Fact]
    public async Task ReceiveAsync_SameComponentInAnotherCell_CreatesSeparateStock()
    {
        await SeedCatalogAsync();
        await SeedStockAsync(_cell, 3);

        await CreateService().ReceiveAsync(ReceiptFor(_secondCell, 2), "Оператор");

        await using var db = CreateContext();
        var quantities = await db.Stock.OrderBy(s => s.Quantity).Select(s => s.Quantity).ToListAsync();
        Assert.Equal(new[] { 2, 3 }, quantities);
    }

    [Fact]
    public async Task ReceiveAsync_UnknownComponent_ThrowsAndSavesNothing()
    {
        await SeedCatalogAsync();
        var dto = ReceiptFor(_cell, 5) with { ComponentId = Guid.NewGuid() };

        await Assert.ThrowsAsync<BusinessException>(() => CreateService().ReceiveAsync(dto, "Оператор"));

        await using var db = CreateContext();
        Assert.False(await db.Stock.AnyAsync());
        Assert.False(await db.OperationHistory.AnyAsync());
    }

    [Xunit.Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task ReceiveAsync_NotPositiveQuantity_ThrowsAndSavesNothing(int quantity)
    {
        await SeedCatalogAsync();

        await Assert.ThrowsAsync<BusinessException>(() => CreateService().ReceiveAsync(ReceiptFor(_cell, quantity), "Оператор"));

        await using var db = CreateContext();
        Assert.False(await db.Stock.AnyAsync());
        Assert.False(await db.OperationHistory.AnyAsync());
    }

    // -------- Выдача --------

    [Fact]
    public async Task IssueAsync_PartOfStock_DecreasesQuantityAndWritesHistory()
    {
        await SeedCatalogAsync();
        var stock = await SeedStockAsync(_cell, 10);

        await CreateService().IssueAsync(new[] { IssueFor(stock, 4) }, "Петров П. П.", "На сборку");

        await using var db = CreateContext();
        Assert.Equal(6, (await db.Stock.SingleAsync()).Quantity);

        var history = await db.OperationHistory.SingleAsync();
        Assert.Equal(OperationType.Issue, history.Type);
        Assert.Equal(10, history.PreviousQuantity);
        Assert.Equal(4, history.QuantityChanged);
        Assert.Equal(6, history.NewQuantity);
        Assert.Equal("На сборку", history.Comment);

        // Название и место берутся из БД, а не из переданного DTO
        Assert.Equal(_component.Name, history.ComponentName);
        Assert.Equal(MappingExtensions.CodeToDisplay(_rack.Column, _rack.Row), history.RackName);
        Assert.Equal(MappingExtensions.CodeToDisplay(_cell.Column, _cell.Row), history.CellName);
    }

    [Fact]
    public async Task IssueAsync_WholeStock_DeletesStockAndKeepsHistory()
    {
        await SeedCatalogAsync();
        var stock = await SeedStockAsync(_cell, 5);

        await CreateService().IssueAsync(new[] { IssueFor(stock, 5) }, "Оператор");

        await using var db = CreateContext();
        Assert.False(await db.Stock.AnyAsync());

        var history = await db.OperationHistory.SingleAsync();
        Assert.Equal(0, history.NewQuantity);
    }

    [Fact]
    public async Task IssueAsync_SeveralStocks_UpdatesAllInOneOperation()
    {
        await SeedCatalogAsync();
        var first = await SeedStockAsync(_cell, 10);
        var second = await SeedStockAsync(_secondCell, 3);

        await CreateService().IssueAsync(new[] { IssueFor(first, 2), IssueFor(second, 3) }, "Оператор");

        await using var db = CreateContext();
        var remaining = await db.Stock.SingleAsync();
        Assert.Equal(first.Id, remaining.Id);
        Assert.Equal(8, remaining.Quantity);
        Assert.Equal(2, await db.OperationHistory.CountAsync());
    }

    [Fact]
    public async Task IssueAsync_MoreThanAvailable_ThrowsAndChangesNothing()
    {
        await SeedCatalogAsync();
        var stock = await SeedStockAsync(_cell, 3);

        await Assert.ThrowsAsync<BusinessException>(() =>
            CreateService().IssueAsync(new[] { IssueFor(stock, 4) }, "Оператор"));

        await using var db = CreateContext();
        Assert.Equal(3, (await db.Stock.SingleAsync()).Quantity);
        Assert.False(await db.OperationHistory.AnyAsync());
    }

    [Fact]
    public async Task IssueAsync_OneOfSeveralItemsFails_NothingIsSaved()
    {
        await SeedCatalogAsync();
        var first = await SeedStockAsync(_cell, 10);
        var second = await SeedStockAsync(_secondCell, 1);

        // Первая позиция корректна, вторая — больше остатка. Выдача должна откатиться целиком.
        await Assert.ThrowsAsync<BusinessException>(() =>
            CreateService().IssueAsync(new[] { IssueFor(first, 5), IssueFor(second, 2) }, "Оператор"));

        await using var db = CreateContext();
        Assert.Equal(10, (await db.Stock.SingleAsync(s => s.Id == first.Id)).Quantity);
        Assert.Equal(1, (await db.Stock.SingleAsync(s => s.Id == second.Id)).Quantity);
        Assert.False(await db.OperationHistory.AnyAsync());
    }

    [Fact]
    public async Task IssueAsync_StockNotFound_ThrowsAndChangesNothing()
    {
        await SeedCatalogAsync();
        var stock = await SeedStockAsync(_cell, 10);
        var missing = IssueFor(stock, 1) with { StockId = Guid.NewGuid() };

        await Assert.ThrowsAsync<BusinessException>(() =>
            CreateService().IssueAsync(new[] { IssueFor(stock, 1), missing }, "Оператор"));

        await using var db = CreateContext();
        Assert.Equal(10, (await db.Stock.SingleAsync()).Quantity);
        Assert.False(await db.OperationHistory.AnyAsync());
    }

    [Fact]
    public async Task IssueAsync_EmptyList_Throws()
    {
        await SeedCatalogAsync();

        await Assert.ThrowsAsync<BusinessException>(() =>
            CreateService().IssueAsync(Array.Empty<IssueStockDto>(), "Оператор"));
    }

    [Fact]
    public async Task ReceiveThenIssue_AllStockIssued_ReceiveAgainCreatesNewStock()
    {
        await SeedCatalogAsync();
        var service = CreateService();

        await service.ReceiveAsync(ReceiptFor(_cell, 5), "Оператор");

        Guid stockId;
        await using (var db = CreateContext())
            stockId = (await db.Stock.SingleAsync()).Id;

        await service.IssueAsync(new[] { new IssueStockDto(stockId, 5, _rack.Id, "rack", _cell.Id, "cell", _component.Id, "component") }, "Оператор");
        await service.ReceiveAsync(ReceiptFor(_cell, 2), "Оператор");

        await using var verify = CreateContext();
        Assert.Equal(2, (await verify.Stock.SingleAsync()).Quantity);
        Assert.Equal(3, await verify.OperationHistory.CountAsync());
    }
}
