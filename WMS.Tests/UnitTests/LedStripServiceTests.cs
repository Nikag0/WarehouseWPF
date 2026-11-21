using Moq;
using WMS.Application;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Application.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace WMS.Test.UnitTests;

public class LedStripServiceTests
{
    private const byte CreateSectorCommand = 0xF0;
    private const byte ColorCommand = 0xEF;
    private const byte GpioCommand = 0xFC;

    private static readonly ControllerEndpoint RowController = new("10.0.0.31", 5000, 1);
    private static readonly ControllerEndpoint LampController = new("10.0.0.33", 5000, 2);

    // Без пауз, чтобы тесты шли мгновенно. Маски — как в appsettings.json
    private static readonly LightingOptions Options = new()
    {
        Enabled = true,
        TcpTimeout = TimeSpan.FromSeconds(3),
        RowStepDelay = TimeSpan.Zero,
        ResultHoldTime = TimeSpan.Zero,
        CheckSectorsOnInit = false,
        SectorCheckDelay = TimeSpan.Zero,
        LampMasks = new Dictionary<LampSignal, byte>
        {
            [LampSignal.Off] = LightingOptions.ParseLampMask("1111", LampSignal.Off),
            [LampSignal.Yellow] = LightingOptions.ParseLampMask("0000", LampSignal.Yellow),
            [LampSignal.Green] = LightingOptions.ParseLampMask("0101", LampSignal.Green),
            [LampSignal.Red] = LightingOptions.ParseLampMask("0010", LampSignal.Red),
        }
    };

    private static byte Mask(LampSignal signal) => Options.GetLampMask(signal);

    private static SectorInitDTO Sector(ControllerEndpoint controller, Guid? lampId, byte index = 0) =>
        new(ControllerId(controller), controller, 1, index, 0, 10, lampId);

    private static Guid ControllerId(ControllerEndpoint controller) =>
        new(controller.DeviceAddress, 0, 0, new byte[8]);

    private static SectorColorDTO Cell(ControllerEndpoint? lamp) =>
        new(RowController, 1, 3, 255, 0, 0, 200, lamp);

    // Записывает все отправленные пакеты; контроллеры из списка «молчат»
    private sealed class FakeSender : ITcpPacketSender
    {
        private readonly HashSet<string> _silentIps;

        public FakeSender(params string[] silentIps) => _silentIps = silentIps.ToHashSet();

        public List<(string Ip, byte[] Packet)> Sent { get; } = new();

        public List<(string Ip, byte[] Packet)> WithCommand(byte command) =>
            Sent.Where(p => p.Packet[2] == command).ToList();

        public Task<bool> SendAsync(string ip, int port, byte[] packet, CancellationToken ct = default)
        {
            Sent.Add((ip, packet));
            return Task.FromResult(!_silentIps.Contains(ip));
        }
    }

    private static LedStripService CreateService(
        ITcpPacketSender sender,
        IReadOnlyList<SectorInitDTO>? sectors = null,
        IReadOnlyList<SignalLampDto>? lamps = null,
        SectorColorDTO? cell = null,
        IReadOnlyList<SectorCheckDto>? checkSectors = null,
        LightingOptions? options = null)
    {
        var repository = new Mock<ISectorRepository>();
        repository
            .Setup(r => r.GetAllSectorsForInitializationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectors ?? Array.Empty<SectorInitDTO>());
        repository
            .Setup(r => r.GetSignalLampsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lamps ?? Array.Empty<SignalLampDto>());
        repository
            .Setup(r => r.GetByCellIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cell);
        repository
            .Setup(r => r.GetAllSectorsForCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkSectors ?? Array.Empty<SectorCheckDto>());

        return new LedStripService(repository.Object, sender, options ?? Options);
    }

    // -------- Проверка зон --------

    private static readonly LightingOptions CheckOptions = new()
    {
        Enabled = true,
        TcpTimeout = Options.TcpTimeout,
        RowStepDelay = TimeSpan.Zero,
        ResultHoldTime = TimeSpan.Zero,
        CheckSectorsOnInit = true,
        SectorCheckDelay = TimeSpan.Zero,
        LampMasks = Options.LampMasks
    };

    // -------- Подсветка отключена --------

    private static readonly LightingOptions DisabledOptions = new()
    {
        Enabled = false,
        TcpTimeout = Options.TcpTimeout,
        RowStepDelay = TimeSpan.Zero,
        ResultHoldTime = TimeSpan.Zero,
        CheckSectorsOnInit = true,
        SectorCheckDelay = TimeSpan.Zero,
        LampMasks = Options.LampMasks
    };

    [Fact]
    public async Task InitializeAsync_LightingDisabled_ReportsSuccessWithoutSending()
    {
        // Все контроллеры молчат — при включённой подсветке это были бы ошибки
        var sender = new FakeSender(RowController.Ip, LampController.Ip);
        var lamp = new SignalLampDto(Guid.NewGuid(), "Ряд А", LampController);
        var service = CreateService(sender,
            sectors: new[] { Sector(RowController, lamp.Id) },
            lamps: new[] { lamp },
            options: DisabledOptions);

        var report = await service.InitializeAsync();

        Assert.True(report.IsSuccess);
        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task HighlightAndClearCell_LightingDisabled_ReturnTrueWithoutSending()
    {
        var sender = new FakeSender(RowController.Ip, LampController.Ip);
        var service = CreateService(sender, cell: Cell(LampController), options: DisabledOptions);

        Assert.True(await service.HighlightCellAsync(Guid.NewGuid()));
        Assert.True(await service.ClearCellAsync(Guid.NewGuid()));
        Assert.Empty(sender.Sent);
    }

    // Индекс зоны совпадает с «номером» ячейки в порядке обхода — по нему проверяется очерёдность
    private static SectorCheckDto CheckSector(byte index, int rackColumn, int rackRow, int cellColumn, int cellRow, Guid? lampId = null) =>
        new(RowController, 1, index, lampId, rackColumn, rackRow, cellColumn, cellRow);

    [Fact]
    public async Task InitializeAsync_CheckDisabled_DoesNotLightSectors()
    {
        var sender = new FakeSender();
        var service = CreateService(sender,
            sectors: new[] { Sector(RowController, lampId: null) },
            checkSectors: new[] { CheckSector(0, 1, 1, 1, 1) });

        await service.InitializeAsync();

        Assert.Empty(sender.WithCommand(ColorCommand));
    }

    [Fact]
    public async Task InitializeAsync_CheckEnabled_LightsSectorsRackByRackFloorByFloorLeftToRight()
    {
        var sender = new FakeSender();

        // Перемешано специально; ожидаемый порядок — по индексу 0…5
        var checkSectors = new[]
        {
            CheckSector(5, rackColumn: 1, rackRow: 2, cellColumn: 1, cellRow: 1), // стеллаж Б-1
            CheckSector(3, rackColumn: 1, rackRow: 1, cellColumn: 1, cellRow: 2), // стеллаж А-1, этаж 2, слева
            CheckSector(1, rackColumn: 1, rackRow: 1, cellColumn: 2, cellRow: 1), // стеллаж А-1, этаж 1, справа
            CheckSector(4, rackColumn: 2, rackRow: 1, cellColumn: 1, cellRow: 1), // стеллаж А-2
            CheckSector(0, rackColumn: 1, rackRow: 1, cellColumn: 1, cellRow: 1), // стеллаж А-1, этаж 1, слева
            CheckSector(2, rackColumn: 1, rackRow: 1, cellColumn: 3, cellRow: 1), // стеллаж А-1, этаж 1, ещё правее
        };
        var service = CreateService(sender,
            sectors: new[] { Sector(RowController, lampId: null) },
            checkSectors: checkSectors,
            options: CheckOptions);

        var report = await service.InitializeAsync();

        Assert.True(report.IsSuccess);

        // Каждая зона: включить, затем выключить
        var colorPackets = sender.WithCommand(ColorCommand);
        Assert.Equal(12, colorPackets.Count);
        Assert.Equal(new byte[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 }, colorPackets.Select(p => p.Packet[4]));
    }

    [Fact]
    public async Task InitializeAsync_CheckFailsForSector_ReportsPlaceAndLeavesLampRed()
    {
        var lamp = new SignalLampDto(Guid.NewGuid(), "Ряд А", LampController);

        // Контроллер принимает настройку зон, но не отвечает на команду цвета
        var sender = new SilentOnColorSender();
        var service = CreateService(sender,
            sectors: new[] { Sector(RowController, lamp.Id) },
            lamps: new[] { lamp },
            checkSectors: new[] { CheckSector(0, 1, 1, 2, 1, lamp.Id) },
            options: CheckOptions);

        var report = await service.InitializeAsync();

        var error = Assert.Single(report.Errors);
        Assert.Contains("стеллаж А-1", error);
        Assert.Contains("ячейка А-2", error);
        Assert.Equal(Mask(LampSignal.Red), sender.Sent.Last(p => p.Packet[2] == GpioCommand).Packet[3]);
    }

    private sealed class SilentOnColorSender : ITcpPacketSender
    {
        public List<(string Ip, byte[] Packet)> Sent { get; } = new();

        public Task<bool> SendAsync(string ip, int port, byte[] packet, CancellationToken ct = default)
        {
            Sent.Add((ip, packet));
            return Task.FromResult(packet[2] != ColorCommand);
        }
    }

    // -------- Инициализация --------

    [Fact]
    public async Task InitializeAsync_NoSectorsInDatabase_ReturnsNotConfiguredWithoutSending()
    {
        var sender = new FakeSender();

        var report = await CreateService(sender).InitializeAsync();

        Assert.False(report.IsConfigured);
        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task InitializeAsync_AllControllersAnswer_LampGoesOffYellowGreenOff()
    {
        var lamp = new SignalLampDto(Guid.NewGuid(), "Ряд А", LampController);
        var sender = new FakeSender();
        var service = CreateService(sender,
            sectors: new[] { Sector(RowController, lamp.Id, 0), Sector(RowController, lamp.Id, 1) },
            lamps: new[] { lamp });

        var report = await service.InitializeAsync();

        Assert.True(report.IsSuccess);
        Assert.Equal(2, sender.WithCommand(CreateSectorCommand).Count(p => p.Ip == RowController.Ip));

        // Лампа управляется своим контроллером, а не контроллером зон ряда
        var lampPackets = sender.WithCommand(GpioCommand);
        Assert.All(lampPackets, p => Assert.Equal(LampController.Ip, p.Ip));
        Assert.Equal(
            new[] { Mask(LampSignal.Off), Mask(LampSignal.Yellow), Mask(LampSignal.Green), Mask(LampSignal.Off) },
            lampPackets.Select(p => p.Packet[3]));
    }

    [Fact]
    public async Task InitializeAsync_RowLampTurnsYellowBeforeItsSectorsAreSent()
    {
        var lamp = new SignalLampDto(Guid.NewGuid(), "Ряд А", LampController);
        var sender = new FakeSender();
        var service = CreateService(sender,
            sectors: new[] { Sector(RowController, lamp.Id) },
            lamps: new[] { lamp });

        await service.InitializeAsync();

        var yellowIndex = sender.Sent.FindIndex(p => p.Packet[2] == GpioCommand && p.Packet[3] == Mask(LampSignal.Yellow));
        var sectorIndex = sender.Sent.FindIndex(p => p.Packet[2] == CreateSectorCommand);
        Assert.True(yellowIndex >= 0 && yellowIndex < sectorIndex);
    }

    [Fact]
    public async Task InitializeAsync_RowControllerSilent_ReportsOnceSkipsItsSectorsAndLeavesLampRed()
    {
        var lamp = new SignalLampDto(Guid.NewGuid(), "Ряд А", LampController);
        var sender = new FakeSender(RowController.Ip);
        var service = CreateService(sender,
            sectors: new[]
            {
                Sector(RowController, lamp.Id, 0),
                Sector(RowController, lamp.Id, 1),
                Sector(RowController, lamp.Id, 2)
            },
            lamps: new[] { lamp });

        var report = await service.InitializeAsync();

        Assert.True(report.IsConfigured);
        Assert.False(report.IsSuccess);
        var error = Assert.Single(report.Errors);
        Assert.Contains(RowController.Ip, error);

        // После первой неудачи контроллеру больше ничего не отправляется — один таймаут вместо трёх
        Assert.Single(sender.WithCommand(CreateSectorCommand));

        // Лампа ряда исправна и в конце горит красным
        Assert.Equal(Mask(LampSignal.Red), sender.WithCommand(GpioCommand).Last().Packet[3]);
    }

    [Fact]
    public async Task InitializeAsync_LampControllerSilent_ReportsLampOnce()
    {
        var lamp = new SignalLampDto(Guid.NewGuid(), "Ряд А", LampController);
        var sender = new FakeSender(LampController.Ip);
        var service = CreateService(sender,
            sectors: new[] { Sector(RowController, lamp.Id) },
            lamps: new[] { lamp });

        var report = await service.InitializeAsync();

        var error = Assert.Single(report.Errors);
        Assert.Contains(LampController.Ip, error);

        // Зоны ряда всё равно настроены — контроллер ряда исправен
        Assert.Single(sender.WithCommand(CreateSectorCommand));

        // Молчащей лампе отправлена только первая команда
        Assert.Single(sender.WithCommand(GpioCommand));
    }

    [Fact]
    public async Task InitializeAsync_SectorsWithoutLamp_AreStillSent()
    {
        var sender = new FakeSender();
        var service = CreateService(sender, sectors: new[] { Sector(RowController, lampId: null) });

        var report = await service.InitializeAsync();

        Assert.True(report.IsSuccess);
        Assert.Single(sender.WithCommand(CreateSectorCommand));
        Assert.Empty(sender.WithCommand(GpioCommand));
    }

    // -------- Подсветка ячейки --------

    [Fact]
    public async Task HighlightCellAsync_SendsColorToRowControllerAndGreenToLampController()
    {
        var sender = new FakeSender();
        var service = CreateService(sender, cell: Cell(LampController));

        Assert.True(await service.HighlightCellAsync(Guid.NewGuid()));

        Assert.Equal(RowController.Ip, Assert.Single(sender.WithCommand(ColorCommand)).Ip);

        var lampPacket = Assert.Single(sender.WithCommand(GpioCommand));
        Assert.Equal(LampController.Ip, lampPacket.Ip);
        Assert.Equal(Mask(LampSignal.Green), lampPacket.Packet[3]);
    }

    [Fact]
    public async Task ClearCellAsync_TurnsLampOff()
    {
        var sender = new FakeSender();
        var service = CreateService(sender, cell: Cell(LampController));

        Assert.True(await service.ClearCellAsync(Guid.NewGuid()));

        Assert.Equal(Mask(LampSignal.Off), Assert.Single(sender.WithCommand(GpioCommand)).Packet[3]);
    }

    [Fact]
    public async Task HighlightCellAsync_CellWithoutSector_ReturnsTrueWithoutSending()
    {
        var sender = new FakeSender();

        Assert.True(await CreateService(sender, cell: null).HighlightCellAsync(Guid.NewGuid()));
        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task HighlightCellAsync_LampControllerSilent_ReturnsFalse()
    {
        var sender = new FakeSender(LampController.Ip);
        var service = CreateService(sender, cell: Cell(LampController));

        Assert.False(await service.HighlightCellAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task HighlightCellAsync_RowWithoutLamp_SendsOnlyColor()
    {
        var sender = new FakeSender();
        var service = CreateService(sender, cell: Cell(lamp: null));

        Assert.True(await service.HighlightCellAsync(Guid.NewGuid()));
        Assert.Single(sender.Sent);
    }

    // -------- Настройки лампы --------

    [Fact]
    public void ParseLampMask_PinsLeftToRight_SetsMatchingBits()
    {
        // pin0 = 0, pin1 = 1, pin2 = 0, pin3 = 1
        Assert.Equal(0b1010, LightingOptions.ParseLampMask("0101", LampSignal.Green));
        Assert.Equal(0b1111, LightingOptions.ParseLampMask("1111", LampSignal.Off));
        Assert.Equal(0, LightingOptions.ParseLampMask("0000", LampSignal.Yellow));
    }

    [Xunit.Theory]
    [InlineData("010")]
    [InlineData("01010")]
    [InlineData("01a1")]
    [InlineData("")]
    public void ParseLampMask_InvalidValue_Throws(string pins)
    {
        Assert.Throws<InvalidOperationException>(() => LightingOptions.ParseLampMask(pins, LampSignal.Red));
    }

    // -------- ШИМ --------

    [Fact]
    public void CalculatePwm_AnyColorAndBrightness_DoesNotThrow()
    {
        // Раньше таблица гаммы была короче 256 значений и яркие цвета падали с IndexOutOfRangeException
        for (var value = 0; value <= 255; value++)
        {
            for (var brightness = 0; brightness <= 255; brightness++)
            {
                LedStripService.CalculatePwm((byte)value, (byte)value, (byte)value, (byte)brightness);
            }
        }
    }

    [Fact]
    public void CalculatePwm_MaxColorAndBrightness_GivesNearlyFullPwm()
    {
        var (r, g, b) = LedStripService.CalculatePwm(255, 255, 255, 255);

        Assert.True(r >= 250);
        Assert.Equal(r, g);
        Assert.Equal(r, b);
    }

    [Fact]
    public void CalculatePwm_ZeroColor_IsOff()
    {
        Assert.Equal(((byte)0, (byte)0, (byte)0), LedStripService.CalculatePwm(0, 0, 0, 150));
    }

    [Fact]
    public void CalculatePwm_BrighterInput_NeverGivesDimmerOutput()
    {
        byte previous = 0;

        for (var value = 0; value <= 255; value++)
        {
            var (r, _, _) = LedStripService.CalculatePwm((byte)value, 0, 0, 150);
            Assert.True(r >= previous);
            previous = r;
        }
    }
}
