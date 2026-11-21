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
               .WithMessage("Компонент не указан");
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
               .WithMessage("Стеллаж не указан");
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
               .WithMessage("Ячейка не указана");
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

        // -------- Дробные количества --------

        [Fact]
        public void ReceiveAndIssue_FractionalQuantities_AreExact()
        {
            var stock = Stock.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0);

            stock.Receive(0.1m);
            stock.Receive(0.2m);
            stock.Quantity.Should().Be(0.3m);

            stock.Issue(0.3m);
            stock.Quantity.Should().Be(0m);
        }

        [Fact]
        public void Create_WithFractionalInitialQuantity_ShouldKeepValue()
        {
            var stock = Stock.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2.125m);

            stock.Quantity.Should().Be(2.125m);
        }

        [Fact]
        public void Receive_WithMoreThanThreeDecimalPlaces_ShouldThrowAndKeepQuantity()
        {
            var stock = Stock.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1m);

            Action act = () => stock.Receive(0.0001m);

            act.Should().Throw<BusinessException>();
            stock.Quantity.Should().Be(1m);
        }

        [Fact]
        public void Issue_WithMoreThanThreeDecimalPlaces_ShouldThrowAndKeepQuantity()
        {
            var stock = Stock.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1m);

            Action act = () => stock.Issue(0.1234m);

            act.Should().Throw<BusinessException>();
            stock.Quantity.Should().Be(1m);
        }

        [Fact]
        public void Create_WithMoreThanThreeDecimalPlaces_ShouldThrow()
        {
            Action act = () => Stock.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1.0005m);

            act.Should().Throw<BusinessException>();
        }
    }
}
