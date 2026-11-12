using FluentAssertions;
using NUnit.Framework;
using WMS.Domain.ExceptionControl;

namespace WMS.Domain.UnitTests
{
    [TestFixture]
    public class OperationHistoryTests
    {
        private static OperationHistory Create(
            OperationType type = OperationType.Receipt,
            string operatorName = "Иванов И. И.",
            string? comment = "Комментарий",
            string componentName = "Резистор",
            string rackName = "А-1",
            string cellName = "Б-2",
            int previousQuantity = 10,
            int quantityChanged = 5,
            int newQuantity = 15)
        {
            return OperationHistory.Create(
                type, operatorName, comment,
                componentName, rackName, cellName,
                previousQuantity, quantityChanged, newQuantity);
        }

        [Test]
        public void Create_Receipt_WithValidData_ShouldSetProperties()
        {
            var history = Create();

            history.Id.Should().NotBe(Guid.Empty);
            history.Type.Should().Be(OperationType.Receipt);
            history.Operator.Should().Be("Иванов И. И.");
            history.Comment.Should().Be("Комментарий");
            history.ComponentName.Should().Be("Резистор");
            history.RackName.Should().Be("А-1");
            history.CellName.Should().Be("Б-2");
            history.PreviousQuantity.Should().Be(10);
            history.QuantityChanged.Should().Be(5);
            history.NewQuantity.Should().Be(15);
            history.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void Create_Issue_DownToZero_ShouldSucceed()
        {
            var history = Create(type: OperationType.Issue, previousQuantity: 5, quantityChanged: 5, newQuantity: 0);

            history.NewQuantity.Should().Be(0);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Create_WithEmptyComment_ShouldStoreNull(string? comment)
        {
            Create(comment: comment).Comment.Should().BeNull();
        }

        [Test]
        public void Create_ShouldTrimTextFields()
        {
            var history = Create(operatorName: " Иванов И. И. ", comment: " текст ", componentName: " Резистор ");

            history.Operator.Should().Be("Иванов И. И.");
            history.Comment.Should().Be("текст");
            history.ComponentName.Should().Be("Резистор");
        }

        [Test]
        public void Create_WithTooLongComment_ShouldThrowBusinessException()
        {
            var act = () => Create(comment: new string('к', OperationHistory.CommentMaxLength + 1));

            act.Should().Throw<BusinessException>()
                .WithMessage($"Комментарий не может быть длиннее {OperationHistory.CommentMaxLength} символов");
        }

        [Test]
        public void Create_WithUnknownType_ShouldThrowBusinessException()
        {
            var act = () => Create(type: (OperationType)0);

            act.Should().Throw<BusinessException>().WithMessage("Неизвестный тип операции");
        }

        [TestCase("")]
        [TestCase(" ")]
        public void Create_WithEmptyOperator_ShouldThrowBusinessException(string operatorName)
        {
            var act = () => Create(operatorName: operatorName);

            act.Should().Throw<BusinessException>().WithMessage("Оператор не указан");
        }

        [Test]
        public void Create_WithEmptyComponentName_ShouldThrowBusinessException()
        {
            var act = () => Create(componentName: " ");

            act.Should().Throw<BusinessException>().WithMessage("Название компонента не указано");
        }

        [Test]
        public void Create_WithEmptyRackName_ShouldThrowBusinessException()
        {
            var act = () => Create(rackName: "");

            act.Should().Throw<BusinessException>().WithMessage("Название стеллажа не указано");
        }

        [Test]
        public void Create_WithEmptyCellName_ShouldThrowBusinessException()
        {
            var act = () => Create(cellName: "");

            act.Should().Throw<BusinessException>().WithMessage("Название ячейки не указано");
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void Create_WithNonPositiveChange_ShouldThrowBusinessException(int quantityChanged)
        {
            var act = () => Create(quantityChanged: quantityChanged, newQuantity: 10 + quantityChanged);

            act.Should().Throw<BusinessException>().WithMessage("Изменение количества должно быть больше нуля");
        }

        [Test]
        public void Create_WithNegativeQuantity_ShouldThrowBusinessException()
        {
            var act = () => Create(type: OperationType.Issue, previousQuantity: 2, quantityChanged: 5, newQuantity: -3);

            act.Should().Throw<BusinessException>().WithMessage("Количество не может быть отрицательным");
        }

        [TestCase(OperationType.Receipt, 10, 5, 14)]
        [TestCase(OperationType.Issue, 10, 5, 15)]
        public void Create_WithInconsistentQuantities_ShouldThrowBusinessException(
            OperationType type, int previous, int changed, int @new)
        {
            var act = () => Create(type: type, previousQuantity: previous, quantityChanged: changed, newQuantity: @new);

            act.Should().Throw<BusinessException>().WithMessage("Количество после операции не сходится с изменением");
        }
    }
}
