using FluentAssertions;
using NUnit.Framework;
using WMS.Domain.ExceptionControl;

namespace WMS.Domain.UnitTests
{
    [TestFixture]
    public class OperatorTests
    {
        private const string ValidSurnameName = "Иванов";
        private const string ValidName = "Иван";
        private const string ValidPatronymic = "Иванович";

        private static Operator CreateValidOperator()
        {
            return Operator.Create(
                ValidSurnameName,
                ValidName,
                ValidPatronymic);
        }

        [Test]
        public void Create_WithValidData_ShouldCreateOperatorWithCorrectProperties()
        {
            var now = DateTime.UtcNow;

            var @operator = CreateValidOperator();

            @operator.Id.Should().NotBe(Guid.Empty);
            @operator.Surname.Should().Be(ValidSurnameName);
            @operator.Name.Should().Be(ValidName);
            @operator.Patronymic.Should().Be(ValidPatronymic);
            @operator.IsDeleted.Should().BeFalse();

            @operator.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            @operator.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Create_WithInvalidSurname_ShouldThrowBusinessException(string? invalidSurname)
        {
            var act = () => Operator.Create(
                invalidSurname,
                ValidName,
                ValidPatronymic);

            act.Should().Throw<BusinessException>()
                .WithMessage("Фамилия не задана");
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Create_WithInvalidName_ShouldThrowBusinessException(string? invalidName)
        {
            var act = () => Operator.Create(
                ValidSurnameName,
                invalidName,
                ValidPatronymic);

            act.Should().Throw<BusinessException>()
                .WithMessage("Имя не задано");
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Create_WithInvalidPatronymic_ShouldThrowBusinessException(string? invalidPatronymic)
        {
            var act = () => Operator.Create(
                ValidSurnameName,
                ValidName,
                invalidPatronymic);

            act.Should().Throw<BusinessException>()
                .WithMessage("Отчество не задано");
        }

        [Test]
        public void Update_WithValidData_ShouldUpdateOperatorProperties()
        {
            var @operator = CreateValidOperator();
            var originalCreatedAt = @operator.CreatedAt;
            var originalUpdatedAt = @operator.UpdatedAt;

            System.Threading.Thread.Sleep(100);

            var newSurname = "Петров";
            var newName = "Пётр";
            var newPatronymic = "Петрович";

            @operator.Update(newSurname, newName, newPatronymic);

            @operator.Surname.Should().Be(newSurname);
            @operator.Name.Should().Be(newName);
            @operator.Patronymic.Should().Be(newPatronymic);
            @operator.CreatedAt.Should().Be(originalCreatedAt);
            @operator.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        [Test]
        public void Update_ShouldTrimWhitespace()
        {
            var @operator = CreateValidOperator();

            @operator.Update("  Петров  ", "  Пётр  ", "  Петрович  ");

            @operator.Surname.Should().Be("Петров");
            @operator.Name.Should().Be("Пётр");
            @operator.Patronymic.Should().Be("Петрович");
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Update_WithInvalidSurname_ShouldThrowBusinessException(string? invalidSurname)
        {
            var @operator = CreateValidOperator();

            var act = () => @operator.Update(
                invalidSurname,
                ValidName,
                ValidPatronymic);

            act.Should().Throw<BusinessException>()
                .WithMessage("Фамилия не задана");
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Update_WithInvalidName_ShouldThrowBusinessException(string? invalidName)
        {
            var @operator = CreateValidOperator();

            var act = () => @operator.Update(
                ValidSurnameName,
                invalidName,
                ValidPatronymic);

            act.Should().Throw<BusinessException>()
                .WithMessage("Имя не задано");
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Update_WithInvalidPatronymic_ShouldThrowBusinessException(string? invalidPatronymic)
        {
            var @operator = CreateValidOperator();

            var act = () => @operator.Update(
                ValidSurnameName,
                ValidName,
                invalidPatronymic);

            act.Should().Throw<BusinessException>()
                .WithMessage("Отчество не задано");
        }

        [Test]
        public void Delete_ShouldMarkOperatorAsDeleted()
        {
            var @operator = CreateValidOperator();

            @operator.Delete();

            @operator.IsDeleted.Should().BeTrue();
        }

        [Test]
        public void Delete_WhenAlreadyDeleted_ShouldNotThrowException()
        {
            var @operator = CreateValidOperator();
            @operator.Delete();

            var act = () => @operator.Delete();

            act.Should().NotThrow();
            @operator.IsDeleted.Should().BeTrue();
        }

        [Test]
        public void Delete_WhenAlreadyDeleted_ShouldRemainDeleted()
        {
            var @operator = CreateValidOperator();
            @operator.Delete();

            @operator.Delete();

            @operator.IsDeleted.Should().BeTrue();
        }

        [Test]
        public void FullName_ShouldReturnFormattedString()
        {
            var @operator = CreateValidOperator();

            var fullName = @operator.FullName;

            fullName.Should().Be("Иванов И. И.");
        }

        [Test]
        public void FullName_ShouldUseFirstLettersOfNameAndPatronymic()
        {
            var @operator = Operator.Create("Сидоров", "Сергей", "Сергеевич");

            var fullName = @operator.FullName;

            fullName.Should().Be("Сидоров С. С.");
        }

        [Test]
        public void FullName_ShouldUpdateAfterUpdate()
        {
            var @operator = CreateValidOperator();

            @operator.Update("Петров", "Пётр", "Петрович");

            @operator.FullName.Should().Be("Петров П. П.");
        }

        [Test]
        public void Create_WithLongNames_ShouldSucceed()
        {
            var longName = new string('А', 100);

            var @operator = Operator.Create(longName, longName, longName);

            @operator.Surname.Should().Be(longName);
            @operator.Name.Should().Be(longName);
            @operator.Patronymic.Should().Be(longName);
        }
    }
}
