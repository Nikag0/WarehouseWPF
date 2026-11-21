using Microsoft.EntityFrameworkCore;
using WMS.Application.DTO;
using WMS.Domain;
using WMS.Domain.LedStrip;
using WMS.Infrastructure;
using WMS.Infrastructure.Context;
using Xunit;
using Assert = Xunit.Assert;

namespace WMS.Test.IntegrationTests;

// Проверяют, что запросы подсветки правильно находят лампу ряда, подключённую к другому контроллеру
public class SectorRepositoryIntegrationTests : IntegrationTestBase
{
    private Cell _cell = null!;
    private Microcontroller _rowController = null!;
    private Microcontroller _lampController = null!;
    private SignalLamp _lamp = null!;

    // Сущности подсветки заполняются вручную и не имеют публичных конструкторов, поэтому значения задаются через EF
    private static T Create<T>(AppDbContext db, object values) where T : class
    {
        var entity = (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;
        var entry = db.Entry(entity);

        foreach (var property in values.GetType().GetProperties())
            entry.Property(property.Name).CurrentValue = property.GetValue(values);

        entry.State = EntityState.Added;
        return entity;
    }

    private async Task SeedAsync(bool withLamp)
    {
        var rack = new Rack(1, 1, RackType.R1);
        _cell = new Cell(rack.Id, 1, 1);

        await using var db = CreateContext();
        db.Rack.Add(rack);
        db.Cell.Add(_cell);

        _lampController = Create<Microcontroller>(db, new { Id = Guid.NewGuid(), Ip = "10.0.0.33", Port = 5000, DeviceAddress = (byte)2 });

        _lamp = Create<SignalLamp>(db, new { Id = Guid.NewGuid(), Name = "Ряд А", MicrocontrollerId = _lampController.Id });

        _rowController = Create<Microcontroller>(db, new
        {
            Id = Guid.NewGuid(),
            Ip = "10.0.0.31",
            Port = 5000,
            DeviceAddress = (byte)1,
            SignalLampId = withLamp ? _lamp.Id : (Guid?)null
        });

        var strip = Create<Strip>(db, new { Id = Guid.NewGuid(), MicrocontrollerId = _rowController.Id, StripNumber = (byte)1 });

        db.Sector.Add(new Sector(Guid.NewGuid(), strip.Id, _cell.Id, 3, 0, 10));

        await db.SaveChangesAsync();
    }

    private SectorRepository CreateRepository() => new(new TestDbContextFactory(DbOptions));

    [Fact]
    public async Task GetByCellIdAsync_LampOnAnotherController_ReturnsBothControllers()
    {
        await SeedAsync(withLamp: true);

        var cell = await CreateRepository().GetByCellIdAsync(_cell.Id);

        Assert.NotNull(cell);
        Assert.Equal(new ControllerEndpoint("10.0.0.31", 5000, 1), cell.Controller);
        Assert.Equal(new ControllerEndpoint("10.0.0.33", 5000, 2), cell.Lamp);
        Assert.Equal(3, cell.Index);
    }

    [Fact]
    public async Task GetByCellIdAsync_ControllerWithoutLamp_ReturnsNullLamp()
    {
        await SeedAsync(withLamp: false);

        var cell = await CreateRepository().GetByCellIdAsync(_cell.Id);

        Assert.NotNull(cell);
        Assert.Null(cell.Lamp);
    }

    [Fact]
    public async Task GetAllSectorsForInitializationAsync_ReturnsLampOfController()
    {
        await SeedAsync(withLamp: true);

        var sector = Assert.Single(await CreateRepository().GetAllSectorsForInitializationAsync());

        Assert.Equal(_rowController.Id, sector.ControllerId);
        Assert.Equal(_lamp.Id, sector.SignalLampId);
        Assert.Equal("10.0.0.31", sector.Controller.Ip);
    }

    [Fact]
    public async Task GetAllSectorsForCheckAsync_ReturnsRackAndCellPosition()
    {
        await SeedAsync(withLamp: true);

        var sector = Assert.Single(await CreateRepository().GetAllSectorsForCheckAsync());

        // SeedAsync: стеллаж (1, 1), ячейка (1, 1), зона 3
        Assert.Equal(1, sector.RackColumn);
        Assert.Equal(1, sector.RackRow);
        Assert.Equal(1, sector.CellColumn);
        Assert.Equal(1, sector.CellRow);
        Assert.Equal(3, sector.Index);
        Assert.Equal(_lamp.Id, sector.SignalLampId);
        Assert.Equal("10.0.0.31", sector.Controller.Ip);
    }

    [Fact]
    public async Task GetSignalLampsAsync_ReturnsControllerTheLampIsWiredTo()
    {
        await SeedAsync(withLamp: true);

        var lamp = Assert.Single(await CreateRepository().GetSignalLampsAsync());

        Assert.Equal("Ряд А", lamp.Name);
        Assert.Equal(new ControllerEndpoint("10.0.0.33", 5000, 2), lamp.Controller);
    }
}
