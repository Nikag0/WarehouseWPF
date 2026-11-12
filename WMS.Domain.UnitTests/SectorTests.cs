using FluentAssertions;
using NUnit.Framework;
using WMS.Domain.ExceptionControl;
using WMS.Domain.LedStrip;

namespace WMS.Domain.UnitTests
{
    [TestFixture]
    public class SectorTests
    {
        [Test]
        public void Create_WithValidDiodes_ShouldSetRange()
        {
            var sector = new Sector(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, 10, 20);

            sector.StartDiode.Should().Be(10);
            sector.EndDiode.Should().Be(20);
        }

        [Test]
        public void Create_WithStartGreaterThanEnd_ShouldThrowBusinessException()
        {
            var act = () => new Sector(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, 20, 10);

            act.Should().Throw<BusinessException>()
                .WithMessage("Начальный диод сектора не может быть больше конечного");
        }
    }
}
