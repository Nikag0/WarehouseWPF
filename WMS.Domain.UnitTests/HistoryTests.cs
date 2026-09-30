using FluentAssertions;
using NUnit.Framework;
using WMS.Domain.ExceptionControl;

namespace WMS.Domain.UnitTests
{
    [TestFixture]
    public class HistoryTests
    {
        private const string ValidOperatorName = "Иванов И. И.";
        private const string ValidComment = "Тестовый комментарий";

        private static readonly Guid ValidComponentId = Guid.NewGuid();
        private static readonly Guid ValidRackId = Guid.NewGuid();
        private static readonly Guid ValidCellId = Guid.NewGuid();

        #region Create Tests

        [Test]
        public void Create_WithValidData_ShouldCreateHistory()
        {
            var now = DateTime.UtcNow;

            var history = History.Create(OperationType.Receipt, ValidOperatorName, ValidComment);

            history.Id.Should().NotBe(Guid.Empty);
            history.Type.Should().Be(OperationType.Receipt);
            history.Operator.Should().Be(ValidOperatorName);
            history.Comment.Should().Be(ValidComment);
            history.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            history.Items.Should().BeEmpty();
        }

        [Test]
        public void Create_WithoutComment_ShouldCreateHistoryWithEmptyComment()
        {
            var history = History.Create(OperationType.Issue, ValidOperatorName);

            history.Comment.Should().Be(string.Empty);
        }

        [Test]
        public void Create_WithNullComment_ShouldCreateHistoryWithEmptyComment()
        {
            var history = History.Create(OperationType.Update, ValidOperatorName, null);

            history.Comment.Should().Be(string.Empty);
        }

        [Test]
        public void Create_ShouldSupportAllOperationTypes()
        {
            var operationTypes = new[] 
            { 
                OperationType.Receipt, 
                OperationType.Issue, 
                OperationType.AddNew, 
                OperationType.Update, 
                OperationType.Delete 
            };

            foreach (var opType in operationTypes)
            {
                var history = History.Create(opType, ValidOperatorName);
                history.Type.Should().Be(opType);
            }
        }

        [Test]
        public void Create_ShouldGenerateUniqueIds()
        {
            var history1 = History.Create(OperationType.Receipt, ValidOperatorName);
            var history2 = History.Create(OperationType.Receipt, ValidOperatorName);

            history1.Id.Should().NotBe(history2.Id);
        }

        [TestCase("")]
        [TestCase(null)]
        public void Create_WithInvalidOperatorName_ShouldThrowBusinessException(string? invalidOperator)
        {
            var act = () => History.Create(OperationType.Receipt, invalidOperator);

            act.Should().Throw<BusinessException>()
                .WithMessage("Имя оператора не указано");
        }

        #endregion

        #region AddItem Tests

        [Test]
        public void AddItem_WithValidData_ShouldAddItemToHistory()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);

            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, 20);

            history.Items.Should().HaveCount(1);
            var item = history.Items.First();
            item.ComponentId.Should().Be(ValidComponentId);
            item.RackId.Should().Be(ValidRackId);
            item.CellId.Should().Be(ValidCellId);
            item.QuantityBefore.Should().Be(10);
            item.QuantityAfter.Should().Be(20);
        }

        [Test]
        public void AddItem_WithMultipleItems_ShouldAddAllItems()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);
            var componentIds = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToList();

            foreach (var componentId in componentIds)
            {
                history.AddItem(componentId, ValidRackId, ValidCellId, 0, 10);
            }

            history.Items.Should().HaveCount(3);
            history.Items.Select(i => i.ComponentId).Should().ContainInOrder(componentIds);
        }

        [Test]
        public void AddItem_WithZeroQuantities_ShouldAddItem()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);

            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 0, 0);

            history.Items.Should().HaveCount(1);
        }

        [Test]
        public void AddItem_WithDuplicateComponentAndCell_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);
            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, 20);

            var act = () => history.AddItem(ValidComponentId, Guid.NewGuid(), ValidCellId, 5, 15);

            act.Should().Throw<BusinessException>()
                .WithMessage("Операция уже содержит позицию для этого товара и ячейки");
        }

        [Test]
        public void AddItem_WithDifferentComponentSameCell_ShouldAddItem()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);
            var componentId1 = Guid.NewGuid();
            var componentId2 = Guid.NewGuid();

            history.AddItem(componentId1, ValidRackId, ValidCellId, 10, 20);
            history.AddItem(componentId2, ValidRackId, ValidCellId, 5, 15);

            history.Items.Should().HaveCount(2);
        }

        [Test]
        public void AddItem_WithSameComponentDifferentCell_ShouldAddItem()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);
            var cellId1 = Guid.NewGuid();
            var cellId2 = Guid.NewGuid();

            history.AddItem(ValidComponentId, ValidRackId, cellId1, 10, 20);
            history.AddItem(ValidComponentId, ValidRackId, cellId2, 5, 15);

            history.Items.Should().HaveCount(2);
        }

        #endregion

        #region Validate Tests

        [Test]
        public void Validate_WithItems_ShouldNotThrowException()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);
            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, 20);

            var act = () => history.Validate();

            act.Should().NotThrow();
        }

        [Test]
        public void Validate_WithoutItems_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);

            var act = () => history.Validate();

            act.Should().Throw<BusinessException>()
                .WithMessage("Операция не может быть пустой");
        }

        [Test]
        public void Validate_WithMultipleItems_ShouldNotThrowException()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);
            for (int i = 0; i < 5; i++)
            {
                history.AddItem(Guid.NewGuid(), ValidRackId, Guid.NewGuid(), i, i + 1);
            }

            var act = () => history.Validate();

            act.Should().NotThrow();
        }

        #endregion

        #region Items Read-Only Tests

        [Test]
        public void Items_ShouldReturnReadOnlyCollection()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);
            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, 20);

            var items = history.Items;

            items.Should().BeAssignableTo<IReadOnlyCollection<HistoryItem>>();
            items.Should().HaveCount(1);
        }

        #endregion

        #region Operator Name Edge Cases

        [Test]
        public void Create_WithVeryLongOperatorName_ShouldSucceed()
        {
            var longOperatorName = new string('А', 1000);

            var history = History.Create(OperationType.Receipt, longOperatorName);

            history.Operator.Should().Be(longOperatorName);
        }

        [Test]
        public void Create_WithSpecialCharactersInOperatorName_ShouldSucceed()
        {
            var specialName = "Оператор-123! @Тест (ФИО)";

            var history = History.Create(OperationType.Receipt, specialName);

            history.Operator.Should().Be(specialName);
        }

        [Test]
        public void Create_WithWhitespaceOnlyOperatorName_ShouldSucceed()
        {
            var whitespaceOperatorName = "   ";

            var history = History.Create(OperationType.Receipt, whitespaceOperatorName);

            history.Operator.Should().Be(whitespaceOperatorName);
        }

        #endregion

        #region Comment Edge Cases

        [Test]
        public void Create_WithVeryLongComment_ShouldSucceed()
        {
            var longComment = new string('А', 5000);

            var history = History.Create(OperationType.Receipt, ValidOperatorName, longComment);

            history.Comment.Should().Be(longComment);
        }

        [Test]
        public void Create_WithWhitespaceComment_ShouldSetComment()
        {
            var whitespaceComment = "   \t\n   ";

            var history = History.Create(OperationType.Receipt, ValidOperatorName, whitespaceComment);

            history.Comment.Should().Be(whitespaceComment);
        }

        #endregion

        #region Timestamp Tests

        [Test]
        public void Create_ShouldSetOccurredAtToUtcNow()
        {
            var timeBefore = DateTime.UtcNow;

            var history = History.Create(OperationType.Receipt, ValidOperatorName);

            var timeAfter = DateTime.UtcNow;
            history.OccurredAt.Should().BeOnOrAfter(timeBefore);
            history.OccurredAt.Should().BeOnOrBefore(timeAfter);
        }

        [Test]
        public void Create_ShouldUseUtcTimeNotLocal()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);

            history.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void Create_MultipleInstances_ShouldHaveDifferentTimestamps()
        {
            var history1 = History.Create(OperationType.Receipt, ValidOperatorName);
            var history2 = History.Create(OperationType.Receipt, ValidOperatorName);

            history1.OccurredAt.Should().NotBe(history2.OccurredAt);
        }

        #endregion

        #region AddItem Quantity Calculation

        [Test]
        public void AddItem_CanCalculateQuantityChange()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);

            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 10, 20);

            var item = history.Items.First();
            var quantityChange = item.QuantityAfter - item.QuantityBefore;
            quantityChange.Should().Be(10);
        }

        [Test]
        public void AddItem_WithNegativeChange_ShouldTrackCorrectly()
        {
            var history = History.Create(OperationType.Issue, ValidOperatorName);

            history.AddItem(ValidComponentId, ValidRackId, ValidCellId, 100, 50);

            var item = history.Items.First();
            var quantityChange = item.QuantityAfter - item.QuantityBefore;
            quantityChange.Should().Be(-50);
        }

        #endregion

        #region AddItem With Empty GUIDs

        [Test]
        public void AddItem_WithEmptyComponentId_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);

            var act = () => history.AddItem(Guid.Empty, ValidRackId, ValidCellId, 10, 20);

            act.Should().Throw<BusinessException>();
        }

        [Test]
        public void AddItem_WithEmptyCellId_ShouldThrowBusinessException()
        {
            var history = History.Create(OperationType.Receipt, ValidOperatorName);

            var act = () => history.AddItem(ValidComponentId, ValidRackId, Guid.Empty, 10, 20);

            act.Should().Throw<BusinessException>();
        }

        #endregion
    }
}
