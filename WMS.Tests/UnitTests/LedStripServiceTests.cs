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
    private static SectorInitDTO Sector(string ip) => new(1, 1, 0, 0, 10, ip, 5000);

    private static (LedStripService Service, Mock<ITcpPacketSender> Sender) CreateService(params SectorInitDTO[] sectors)
    {
        var repository = new Mock<ISectorRepository>();
        repository
            .Setup(r => r.GetAllSectorsForInitializationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectors);

        var sender = new Mock<ITcpPacketSender>();

        return (new LedStripService(repository.Object, sender.Object), sender);
    }

    [Fact]
    public async Task InitializeSectorsAsync_NoSectorsInDatabase_ReturnsNotConfiguredWithoutSending()
    {
        var (service, sender) = CreateService();

        var result = await service.InitializeSectorsAsync();

        Assert.Equal(ControllersInitResult.NotConfigured, result);
        sender.Verify(
            s => s.SendAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task InitializeSectorsAsync_AllControllersAnswer_ReturnsSuccess()
    {
        var (service, sender) = CreateService(Sector("10.0.0.1"), Sector("10.0.0.2"));
        sender
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Assert.Equal(ControllersInitResult.Success, await service.InitializeSectorsAsync());
    }

    [Fact]
    public async Task InitializeSectorsAsync_ControllerDoesNotAnswer_ReturnsFailed()
    {
        var (service, sender) = CreateService(Sector("10.0.0.1"));
        sender
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Assert.Equal(ControllersInitResult.Failed, await service.InitializeSectorsAsync());
    }

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
