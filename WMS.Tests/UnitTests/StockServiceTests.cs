using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using WMS.Application.Abstractions;
using WMS.Application.Services;
using WMS.Domain.ExceptionControl;
using WMS.Domain;

namespace WMS.Test.UnitTests;

public class StockServiceTests
{
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

        // Component создаём через фабричный метод
        var component = Component.Create(
            article: "RES-001",
            name: "Resistor",
            manufacturer: "Vishay",
            expirationDate: null,
            minQuantity: 10);

        // Подменяем Id через рефлексию или приватный сеттер, если он есть.
        // Если Id только get; private set — можно использовать reflection:
        typeof(Component)
            .GetProperty(nameof(Component.Id))!
            .SetValue(component, componentId);

        compRepo
            .Setup(r => r.GetByIdAsync(componentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(component);

        stockRepo
            .Setup(r => r.GetByLocationAsync(componentId, rackId, cellId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock)null);

        uowMock.SetupGet(u => u.Components).Returns(compRepo.Object);
        uowMock.SetupGet(u => u.Stocks).Returns(stockRepo.Object);
        uowMock.SetupGet(u => u.History).Returns(historyRepo.Object);

        // Конструктор StockService принимает репозитории + UoW
        var service = new StockService(
            compRepo.Object,
            stockRepo.Object,
            historyRepo.Object,
            loggerMock.Object,
            uowMock.Object);

        // Record с позиционными параметрами
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
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Component)null);

        uowMock.SetupGet(u => u.Components).Returns(compRepo.Object);

        var service = new StockService(
            compRepo.Object,
            Mock.Of<IStockRepository>(),
            Mock.Of<IHistoryRepository>(),
            Mock.Of<ILogger<StockService>>(),
            uowMock.Object);

        var dto = new ReceiptItemDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);

        // Явно указываем Xunit.Assert, чтобы не было конфликта с NUnit
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
            Mock.Of<IHistoryRepository>(),
            Mock.Of<ILogger<StockService>>(),
            Mock.Of<IUnitOfWork>());

        await Xunit.Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.ReceiveAsync(null, "Ivanov"));
    }
}