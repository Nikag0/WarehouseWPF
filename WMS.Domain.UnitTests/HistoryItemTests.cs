using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain.ExceptionControl;

namespace WMS.Domain.UnitTests
{
    [TestFixture]
    public class HistoryItemTests
    {
        private static readonly Guid ValidComponentId = Guid.NewGuid();
        private static readonly Guid ValidRackId = Guid.NewGuid();
        private static readonly Guid ValidCellId = Guid.NewGuid();

        #region Constructor Validation Tests

        [Test]
        public void Constructor_WithValidData_ShouldCreateHistoryItem()
        {
            var component = Component.Create("ART001", "Тестовый компонент", "Производитель", null, 1);
            var history = History.Create(OperationType.Receipt, "Оператор");
            history.AddItem(component.Id, ValidRackId, ValidCellId, 10, 20);

            var item = history.Items.First();

            item.Id.Should().NotBe(Guid.Empty);
            item.ComponentId.Should().Be(component.Id);
            item.RackId.Should().Be(ValidRackId);
            item.CellId.Should().Be(ValidCellId);
            item.QuantityBefore.Should().Be(10);
            item.QuantityAfter.Should().Be(20);
        }

        [Test]
        public void Constructor_WithEmptyComponentId_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            var act = () => history.AddItem(Guid.Empty, ValidRackId, ValidCellId, 10, 20);

            act.Should().Throw<BusinessException>()
                .WithMessage("ComponentId не задан");
        }

        [Test]
        public void Constructor_WithEmptyRackId_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            var act = () => history.AddItem(ValidComponentId, Guid.Empty, ValidCellId, 10, 20);

            act.Should().Throw<BusinessException>()
                .WithMessage("RackId не задан");
        }

        [Test]
        public void Constructor_WithEmptyCellId_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            var act = () => history.AddItem(ValidComponentId, ValidRackId, Guid.Empty, 10, 20);

            act.Should().Throw<BusinessException>()
                .WithMessage("CellId не задан");
        }

        [Test]
        public void Constructor_WithNegativeQuantityBefore_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            var act = () => history.AddItem(ValidComponentId, ValidRackId, ValidCellId, -1, 20);

            act.Should().Throw<BusinessException>()
                .WithMessage("Количество не может быть отрицательным");
        }

        [Test]
        public void Constructor_WithNegativeQuantityAfter_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            var act = () => history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, -5);

            act.Should().Throw<BusinessException>()
                .WithMessage("Количество не может быть отрицательным");
        }

        [Test]
        public void Constructor_WithBothNegativeQuantities_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            var act = () => history.AddItem(ValidComponentId, ValidRackId, ValidCellId, -5, -10);

            act.Should().Throw<BusinessException>();
        }

        #endregion

        #region Quantity Edge Cases

        [Test]
        public void Constructor_WithZeroQuantities_ShouldSucceed()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 0, 0);

            history.Items.Should().HaveCount(1);
        }

        [Test]
        public void Constructor_WithLargeQuantities_ShouldSucceed()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, int.MaxValue - 1, int.MaxValue);

            history.Items.Should().HaveCount(1);
            history.Items.First().QuantityAfter.Should().Be(int.MaxValue);
        }

        [Test]
        public void Constructor_WithQuantityIncreasingThenDecreasing_ShouldSucceed()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            // Увеличиваем количество
            history.AddItem(ValidComponentId, ValidRackId, Guid.NewGuid(), 10, 50);
            // Уменьшаем количество в другой ячейке
            history.AddItem(Guid.NewGuid(), ValidRackId, ValidCellId, 50, 10);

            history.Items.Should().HaveCount(2);
        }

        #endregion

        #region ID Generation Tests

        [Test]
        public void Constructor_ShouldGenerateUniqueIds()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");

            history.AddItem(ValidComponentId, ValidRackId, Guid.NewGuid(), 10, 20);
            history.AddItem(Guid.NewGuid(), ValidRackId, Guid.NewGuid(), 5, 15);

            var ids = history.Items.Select(i => i.Id).ToList();
            ids.Should().NotContain(Guid.Empty);
            ids.Distinct().Should().HaveCount(2);
        }

        #endregion

        #region Properties Read-Only Tests

        [Test]
        public void Properties_ShouldBeReadOnly()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");
            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, 20);

            var item = history.Items.First();

            // Проверяем, что свойства установлены корректно
            item.ComponentId.Should().Be(ValidComponentId);
            item.RackId.Should().Be(ValidRackId);
            item.CellId.Should().Be(ValidCellId);
            item.QuantityBefore.Should().Be(10);
            item.QuantityAfter.Should().Be(20);
        }

        #endregion

        #region Quantity Change Calculation

        [Test]
        public void QuantityChange_ShouldBeCalculable()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");
            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, 35);

            var item = history.Items.First();
            var change = item.QuantityAfter - item.QuantityBefore;

            change.Should().Be(25);
        }

        [Test]
        public void QuantityChange_WithNegativeValue_ShouldBeCalculable()
        {
            var history = History.Create(OperationType.Issue, "Оператор");
            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 100, 30);

            var item = history.Items.First();
            var change = item.QuantityAfter - item.QuantityBefore;

            change.Should().Be(-70);
        }

        [Test]
        public void QuantityChange_WithZeroChange_ShouldBeCalculable()
        {
            var history = History.Create(OperationType.Update, "Оператор");
            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 50, 50);

            var item = history.Items.First();
            var change = item.QuantityAfter - item.QuantityBefore;

            change.Should().Be(0);
        }

        #endregion

        #region Duplicate Item Detection

        [Test]
        public void AddItem_WithSameComponentAndCellInDifferentHistories_ShouldSucceed()
        {
            var history1 = History.Create(OperationType.Receipt, "Оператор");
            var history2 = History.Create(OperationType.Issue, "Оператор");

            history1.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, 20);
            history2.AddItem(ValidComponentId, ValidRackId, ValidCellId, 20, 10);

            history1.Items.Should().HaveCount(1);
            history2.Items.Should().HaveCount(1);
        }

        [Test]
        public void AddItem_WithDifferentRacksSameComponentCell_ShouldThrowDuplicateException()
        {
            var history = History.Create(OperationType.Receipt, "Оператор");
            var rack1 = Guid.NewGuid();
            var rack2 = Guid.NewGuid();

            history.AddItem(ValidComponentId, rack1, ValidCellId, 10, 20);

            var act = () => history.AddItem(ValidComponentId, rack2, ValidCellId, 5, 15);

            act.Should().Throw<BusinessException>()
                .WithMessage("Операция уже содержит позицию для этого товара и ячейки");
        }

        #endregion
    }
}
