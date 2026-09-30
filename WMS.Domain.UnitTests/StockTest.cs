using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain.ExceptionControl;
using Xunit;

namespace WMS.Domain.UnitTests
{
    public class StockTest
    {
        private const int ValidQuantity = 100;

        [Fact]
        public void Create_WithValidData_ShouldCreateStockWithCorrectProperties()
        {
            // Arrange
            var componentId = Guid.NewGuid();
            var rackId = Guid.NewGuid();
            var cellId = Guid.NewGuid();

            // Act
            var stock = Stock.Create(componentId, rackId, cellId, ValidQuantity);

            // Assert
            stock.Id.Should().NotBe(Guid.Empty); 
            stock.ComponentId.Should().Be(componentId);
            stock.RackId.Should().Be(rackId);
            stock.CellId.Should().Be(cellId);
            stock.Quantity.Should().Be(ValidQuantity);
        }

        [Fact] 
        public void Create_WithEmptyComponentId_ShouldThrowBusinessException()
        {
            // Act
            Action act = () => Stock.Create(
                Guid.Empty,
                Guid.NewGuid(),
                Guid.NewGuid(),
                ValidQuantity);

            // Assert
            act.Should().Throw<BusinessException>()
               .WithMessage("ComponentId не задан");
        }

        [Fact]
        public void Create_WithEmptyRackId_ShouldThrowBusinessException()
        {
            // Act
            Action act = () => Stock.Create(
                Guid.NewGuid(),
                Guid.Empty,
                Guid.NewGuid(),
                ValidQuantity);

            // Assert
            act.Should().Throw<BusinessException>()
               .WithMessage("RackId не задан");
        }

        [Fact]
        public void Create_WithEmptyCellId_ShouldThrowBusinessException()
        {
            // Act
            Action act = () => Stock.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Empty,
                ValidQuantity);

            // Assert
            act.Should().Throw<BusinessException>()
               .WithMessage("CellId не задан");
        }

        [Xunit.Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Create_WithNegativeQuantity_ShouldThrowBusinessException(int invalidQuantity)
        {
            Action act = () => Stock.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                invalidQuantity);

            act.Should().Throw<BusinessException>();
        }
    }
}
