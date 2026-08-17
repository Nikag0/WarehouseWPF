using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Infrastructure;
using Xunit;

namespace WMS.Test.IntegrationTests.Services;

public class StockServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task ReceiveAsync_WhenHistoryFails_StockMustNotBePersisted()
    {
        // Arrange
        var component = Component.Create("ART-001", "Test Component", "Test Mfg", null, 1);
        var rack = new Rack(1, 1, RackType.R1); // <-- замените RackType.Standard на ваше значение
        var cell = new Cell(rack.Id, 1, 1);

        using (var seed = new AppDbContext(DbOptions))
        {
            seed.Components.Add(component);
            seed.Racks.Add(rack);
            seed.Cells.Add(cell);
            await seed.SaveChangesAsync();
        }

        using var context = new AppDbContext(DbOptions);

        var compRepo = new ComponentRepository(context, Mock.Of<ILogger<ComponentRepository>>());
        var stockRepo = new StockRepository(context, Mock.Of<ILogger<StockRepository>>());
        var opRepo = Mock.Of<IOperatorRepository>();

        // Ломаем History: имитируем добавление в Change Tracker, но потом бросаем исключение
        var historyMock = new Mock<IHistoryRepository>();
        historyMock
            .Setup(r => r.Add(It.IsAny<History>(), It.IsAny<CancellationToken>()))
            .Callback<History, CancellationToken>((h, _) => context.History.Add(h))
            .Throws(new InvalidOperationException("History DB is down"));

        var uow = new UnitOfWork(context, compRepo, stockRepo, historyMock.Object, opRepo);
        var service = new StockService(
            compRepo,
            stockRepo,
            historyMock.Object,
            Mock.Of<ILogger<StockService>>(),
            uow);

        var dto = new ReceiptItemDto(component.Id, rack.Id, cell.Id, 5);

        // Act
        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ReceiveAsync(dto, "TestOperator"));

        // Assert: проверяем через НОВЫЙ DbContext, иначе увидим "грязный" объект из памяти
        using var verify = new AppDbContext(DbOptions);
        var stockInDb = await verify.Stocks.FirstOrDefaultAsync();
        var historyInDb = await verify.History.FirstOrDefaultAsync();

        Xunit.Assert.Null(stockInDb);
        Xunit.Assert.Null(historyInDb);
    }

    [Fact]
    public async Task ReceiveAsync_Success_StockAndHistoryPersistedInDatabase()
    {
        var component = Component.Create("ART-002", "Capacitor", "Murata", null, 5);
        var rack = new Rack(2, 1, RackType.R1); // <-- замените на ваше значение
        var cell = new Cell(rack.Id, 1, 2);

        using (var seed = new AppDbContext(DbOptions))
        {
            seed.Components.Add(component);
            seed.Racks.Add(rack);
            seed.Cells.Add(cell);
            await seed.SaveChangesAsync();
        }

        using var context = new AppDbContext(DbOptions);

        var compRepo = new ComponentRepository(context, Mock.Of<ILogger<ComponentRepository>>());
        var stockRepo = new StockRepository(context, Mock.Of<ILogger<StockRepository>>());
        var opRepo = Mock.Of<IOperatorRepository>();
        var historyMock = new Mock<IHistoryRepository>();
        historyMock
            .Setup(r => r.Add(It.IsAny<History>(), It.IsAny<CancellationToken>()))
            .Callback<History, CancellationToken>((h, _) => context.History.Add(h));

        var uow = new UnitOfWork(context, compRepo, stockRepo, historyMock.Object, opRepo);
        var service = new StockService(
            compRepo,
            stockRepo,
            historyMock.Object,
            Mock.Of<ILogger<StockService>>(),
            uow);

        var dto = new ReceiptItemDto(component.Id, rack.Id, cell.Id, 7);

        // Act
        await service.ReceiveAsync(dto, "Ivanov");

        // Assert
        using var verify = new AppDbContext(DbOptions);
        var stock = await verify.Stocks.FirstOrDefaultAsync();
        var history = await verify.History.Include(h => h.Items).FirstOrDefaultAsync();

        Xunit.Assert.NotNull(stock);
        Xunit.Assert.Equal(7, stock.Quantity);
        Xunit.Assert.Equal(rack.Id, stock.RackId);

        Xunit.Assert.NotNull(history);
        Xunit.Assert.Single(history.Items);
    }

    [Fact]
    public async Task ReceiveAsync_ExistingStock_IncreasesQuantity()
    {
        var componentId = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var cellId = Guid.NewGuid();

        using (var seed = new AppDbContext(DbOptions))
        {
            seed.Components.Add(Component.Create("ART-002", "Capacitor", "Murata", null, 5));
            seed.Stocks.Add(Stock.Create(componentId, rackId, cellId, 5));
            await seed.SaveChangesAsync();
        }

        using var context = new AppDbContext(DbOptions);

        var compRepo = new ComponentRepository(context, Mock.Of<ILogger<ComponentRepository>>());
        var stockRepo = new StockRepository(context, Mock.Of<ILogger<StockRepository>>());
        var opRepo = Mock.Of<IOperatorRepository>();
        var historyMock = new Mock<IHistoryRepository>();
        historyMock
            .Setup(r => r.Add(It.IsAny<History>(), It.IsAny<CancellationToken>()))
            .Callback<History, CancellationToken>((h, _) => context.History.Add(h));
        var uow = new UnitOfWork(context, compRepo, stockRepo, historyMock.Object, opRepo);

        var service = new StockService(
            compRepo,
            stockRepo,
            historyMock.Object,
            Mock.Of<ILogger<StockService>>(),
            uow);

        await service.ReceiveAsync(new ReceiptItemDto
        (
            componentId,
            rackId,
            cellId,
            3
        ), "Sidorov");

        using var verify = new AppDbContext(DbOptions);
        var stock = await verify.Stocks.SingleAsync();

        Xunit.Assert.Equal(8, stock.Quantity); // 5 + 3
    }
}