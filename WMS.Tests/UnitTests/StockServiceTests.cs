using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain.ExceptionControl;
using WMS.Domain;
using WMS.Application.DTO;

namespace WMS.Test.UnitTests;

public class StockServiceTests
{
    #region ReceiveAsync Tests

    [Fact]
    public async Task ReceiveAsync_NewStock_CallsSaveChangesExactlyOnce()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<StockService>>();
        var compRepo = new Mock<IComponentRepository>();
        var stockRepo = new Mock<IStockRepository>();
        var historyRepo = new Mock<IHistoryRepository>();

        var componentId = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var cellId = Guid.NewGuid();

        var component = Component.Create(
            article: "RES-001",
            name: "Resistor",
            manufacturer: "Vishay",
            expirationDate: null,
            minQuantity: 10);

        typeof(Component)
            .GetProperty(nameof(Component.Id))!
            .SetValue(component, componentId);

        compRepo
            .Setup(r => r.GetByIdAsync(componentId))
            .ReturnsAsync(component);

        stockRepo
            .Setup(r => r.GetByLocationAsync(componentId, rackId, cellId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock)null);

        uowMock.SetupGet(u => u.Components).Returns(compRepo.Object);
        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);
        uowMock.SetupGet(u => u.History).Returns(historyRepo.Object);

        var service = new StockService(
            compRepo.Object,
            stockRepo.Object,
            loggerMock.Object,
            uowMock.Object);

        var dto = new ReceiptItemDto(componentId, rackId, cellId, 10);

        // Act
        await service.ReceiveAsync(dto, "Ivanov");

        // Assert
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        stockRepo.Verify(r => r.Add(
            It.Is<Stock>(s => s.Quantity == 10 && s.RackId == rackId),
            It.IsAny<CancellationToken>()), Times.Once);

        historyRepo.Verify(r => r.Add(It.IsAny<History>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReceiveAsync_ComponentNotFound_NeverCallsSaveChanges()
    {
        var uowMock = new Mock<IUnitOfWork>();
        var compRepo = new Mock<IComponentRepository>();

        compRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Component)null);

        uowMock.SetupGet(u => u.Components).Returns(compRepo.Object);

        var service = new StockService(
            compRepo.Object,
            Mock.Of<IStockRepository>(),
            Mock.Of<ILogger<StockService>>(),
            uowMock.Object);

        var dto = new ReceiptItemDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);

        await Xunit.Assert.ThrowsAsync<BusinessException>(() =>
            service.ReceiveAsync(dto, "Ivanov"));

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReceiveAsync_NullItem_ThrowsArgumentNullException()
    {
        var service = new StockService(
            Mock.Of<IComponentRepository>(),
            Mock.Of<IStockRepository>(),
            Mock.Of<ILogger<StockService>>(),
            Mock.Of<IUnitOfWork>());

        await Xunit.Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.ReceiveAsync(null, "Ivanov"));
    }

    [Fact]
    public async Task ReceiveAsync_ExistingStock_UpdatesQuantity()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<StockService>>();
        var compRepo = new Mock<IComponentRepository>();
        var stockRepo = new Mock<IStockRepository>();
        var historyRepo = new Mock<IHistoryRepository>();

        var componentId = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var cellId = Guid.NewGuid();

        var component = Component.Create(
            article: "RES-002",
            name: "Capacitor",
            manufacturer: "Kemet",
            expirationDate: null,
            minQuantity: 20);

        typeof(Component)
            .GetProperty(nameof(Component.Id))!
            .SetValue(component, componentId);

        var existingStock = Stock.Create(componentId, rackId, cellId, 50);

        compRepo
            .Setup(r => r.GetByIdAsync(componentId))
            .ReturnsAsync(component);

        stockRepo
            .Setup(r => r.GetByLocationAsync(componentId, rackId, cellId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStock);

        uowMock.SetupGet(u => u.Components).Returns(compRepo.Object);
        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);
        uowMock.SetupGet(u => u.History).Returns(historyRepo.Object);

        var service = new StockService(
            compRepo.Object,
            stockRepo.Object,
            loggerMock.Object,
            uowMock.Object);

        var dto = new ReceiptItemDto(componentId, rackId, cellId, 30);

        // Act
        await service.ReceiveAsync(dto, "Operator");

        // Assert
        stockRepo.Verify(r => r.Update(
            It.Is<Stock>(s => s.Quantity == 80),
            It.IsAny<CancellationToken>()), Times.Once);

        stockRepo.Verify(r => r.Add(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Never);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReceiveAsync_WithComment_IncludesCommentInHistory()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<StockService>>();
        var compRepo = new Mock<IComponentRepository>();
        var stockRepo = new Mock<IStockRepository>();
        var historyRepo = new Mock<IHistoryRepository>();

        var componentId = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var cellId = Guid.NewGuid();

        var component = Component.Create(
            article: "RES-003",
            name: "Inductor",
            manufacturer: "Murata",
            expirationDate: null,
            minQuantity: 15);

        typeof(Component)
            .GetProperty(nameof(Component.Id))!
            .SetValue(component, componentId);

        compRepo
            .Setup(r => r.GetByIdAsync(componentId))
            .ReturnsAsync(component);

        stockRepo
            .Setup(r => r.GetByLocationAsync(componentId, rackId, cellId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock)null);

        uowMock.SetupGet(u => u.Components).Returns(compRepo.Object);
        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);
        uowMock.SetupGet(u => u.History).Returns(historyRepo.Object);

        var service = new StockService(
            compRepo.Object,
            stockRepo.Object,
            loggerMock.Object,
            uowMock.Object);

        var dto = new ReceiptItemDto(componentId, rackId, cellId, 25);
        var comment = "Emergency restock due to low inventory";

        // Act
        await service.ReceiveAsync(dto, "Manager", comment);

        // Assert
        historyRepo.Verify(r => r.Add(
            It.Is<History>(h => h.Comment == comment && h.Type == OperationType.Receipt),
            It.IsAny<CancellationToken>()), Times.Once);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReceiveAsync_InvalidQuantity_ThrowsBusinessException()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var compRepo = new Mock<IComponentRepository>();
        var stockRepo = new Mock<IStockRepository>();
        var historyRepo = new Mock<IHistoryRepository>();

        var componentId = Guid.NewGuid();
        var component = Component.Create("TEST-001", "Test Component", "Manufacturer", null, 5);
        typeof(Component).GetProperty(nameof(Component.Id))!.SetValue(component, componentId);

        compRepo
            .Setup(r => r.GetByIdAsync(componentId))
            .ReturnsAsync(component);

        stockRepo
            .Setup(r => r.GetByLocationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock)null);

        uowMock.SetupGet(u => u.Components).Returns(compRepo.Object);
        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);
        uowMock.SetupGet(u => u.History).Returns(historyRepo.Object);

        var service = new StockService(
            compRepo.Object,
            stockRepo.Object,
            Mock.Of<ILogger<StockService>>(),
            uowMock.Object);

        var dto = new ReceiptItemDto(componentId, Guid.NewGuid(), Guid.NewGuid(), 0);

        // Act & Assert
        await Xunit.Assert.ThrowsAsync<BusinessException>(() =>
            service.ReceiveAsync(dto, "Operator"));

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region IssueAsync Tests

    [Fact]
    public async Task IssueAsync_ValidItems_CallsSaveChangesExactlyOnce()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<StockService>>();
        var stockRepo = new Mock<IStockRepository>();
        var historyRepo = new Mock<IHistoryRepository>();

        var stockId = Guid.NewGuid();
        var componentId = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var cellId = Guid.NewGuid();

        var stock = Stock.Create(componentId, rackId, cellId, 50);
        typeof(Stock)
            .GetProperty(nameof(Stock.Id))!
            .SetValue(stock, stockId);

        stockRepo
            .Setup(r => r.GetByIdAsync(stockId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);

        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);
        uowMock.SetupGet(u => u.History).Returns(historyRepo.Object);

        var service = new StockService(
            Mock.Of<IComponentRepository>(),
            stockRepo.Object,
            loggerMock.Object,
            uowMock.Object);

        var items = new List<IssueItemDto> { new IssueItemDto(stockId, 20) }.AsReadOnly();

        // Act
        await service.IssueAsync(items, "Dispatcher");

        // Assert
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        stockRepo.Verify(r => r.Update(
            It.Is<Stock>(s => s.Quantity == 30 && s.Id == stockId),
            It.IsAny<CancellationToken>()), Times.Once);

        historyRepo.Verify(r => r.Add(It.IsAny<History>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IssueAsync_ReducesStockToZero_DeletesStock()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<StockService>>();
        var stockRepo = new Mock<IStockRepository>();
        var historyRepo = new Mock<IHistoryRepository>();

        var stockId = Guid.NewGuid();
        var componentId = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var cellId = Guid.NewGuid();

        var stock = Stock.Create(componentId, rackId, cellId, 20);
        typeof(Stock)
            .GetProperty(nameof(Stock.Id))!
            .SetValue(stock, stockId);

        stockRepo
            .Setup(r => r.GetByIdAsync(stockId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);

        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);
        uowMock.SetupGet(u => u.History).Returns(historyRepo.Object);

        var service = new StockService(
            Mock.Of<IComponentRepository>(),
            stockRepo.Object,
            loggerMock.Object,
            uowMock.Object);

        var items = new List<IssueItemDto> { new IssueItemDto(stockId, 20) }.AsReadOnly();

        // Act
        await service.IssueAsync(items, "Dispatcher");

        // Assert
        stockRepo.Verify(r => r.Delete(
            It.Is<Stock>(s => s.Quantity == 0 && s.Id == stockId),
            It.IsAny<CancellationToken>()), Times.Once);

        stockRepo.Verify(r => r.Update(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Never);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IssueAsync_StockNotFound_NeverCallsSaveChanges()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<StockService>>();
        var stockRepo = new Mock<IStockRepository>();

        stockRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock)null);

        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);

        var service = new StockService(
            Mock.Of<IComponentRepository>(),
            stockRepo.Object,
            loggerMock.Object,
            uowMock.Object);

        var items = new List<IssueItemDto> { new IssueItemDto(Guid.NewGuid(), 10) }.AsReadOnly();

        // Act & Assert
        await Xunit.Assert.ThrowsAsync<BusinessException>(() =>
            service.IssueAsync(items, "Dispatcher"));

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IssueAsync_EmptyItemsList_ThrowsBusinessException()
    {
        var service = new StockService(
            Mock.Of<IComponentRepository>(),
            Mock.Of<IStockRepository>(),
            Mock.Of<ILogger<StockService>>(),
            Mock.Of<IUnitOfWork>());

        var items = new List<IssueItemDto>().AsReadOnly();

        await Xunit.Assert.ThrowsAsync<BusinessException>(() =>
            service.IssueAsync(items, "Dispatcher"));
    }

    [Fact]
    public async Task IssueAsync_MultipleItems_UpdatesAllAndCallsSaveChangesOnce()
    {
        // Arrange
        var uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<StockService>>();
        var stockRepo = new Mock<IStockRepository>();
        var historyRepo = new Mock<IHistoryRepository>();

        var stock1Id = Guid.NewGuid();
        var stock2Id = Guid.NewGuid();
        var componentId1 = Guid.NewGuid();
        var componentId2 = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var cellId1 = Guid.NewGuid();
        var cellId2 = Guid.NewGuid();

        var stock1 = Stock.Create(componentId1, rackId, cellId1, 50);
        var stock2 = Stock.Create(componentId2, rackId, cellId2, 30);

        typeof(Stock).GetProperty(nameof(Stock.Id))!.SetValue(stock1, stock1Id);
        typeof(Stock).GetProperty(nameof(Stock.Id))!.SetValue(stock2, stock2Id);

        stockRepo
            .Setup(r => r.GetByIdAsync(stock1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock1);

        stockRepo
            .Setup(r => r.GetByIdAsync(stock2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock2);

        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);
        uowMock.SetupGet(u => u.History).Returns(historyRepo.Object);

        var service = new StockService(
            Mock.Of<IComponentRepository>(),
            stockRepo.Object,
            loggerMock.Object,
            uowMock.Object);

        var items = new List<IssueItemDto>
        {
            new IssueItemDto(stock1Id, 20),
            new IssueItemDto(stock2Id, 10)
        }.AsReadOnly();

        // Act
        await service.IssueAsync(items, "Dispatcher");

        // Assert
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify both stocks were updated
        stockRepo.Verify(r => r.Update(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Verify history record was created with both items
        historyRepo.Verify(r => r.Add(
            It.Is<History>(h => h.Items.Count() == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
