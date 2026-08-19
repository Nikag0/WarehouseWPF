using Xunit;
using Moq;
using FluentAssertions;
using WMS.Domain.ExceptionControl;

namespace WMS.Domain.UnitTests
{
    public class ComponentTests
    {
        private const string ValidArticle = "ART-001";
        private const string ValidName = "Резистор";
        private const string ValidManufacturer = "Завод";
        private const int ValidMinQuantity= 10;

        private static Component CreateValidComponent()
        {
            return Component.Create(
                ValidArticle,
                ValidName,
                ValidManufacturer,
                null,
                ValidMinQuantity);
        }

        [Fact]
        public void Create_WithValidData_ShouldCreateComponentWithCorrectProperties()
        {
            var now = DateTime.UtcNow;

            var component = CreateValidComponent();

            component.Id.Should().NotBe(Guid.Empty);
            component.Article.Should().Be(ValidArticle);
            component.Name.Should().Be(ValidName);
            component.Manufacturer.Should().Be(ValidManufacturer);
            component.ExpirationDate.Should().Be(null);
            component.MinQuantity.Should().Be(ValidMinQuantity);
            component.IsDeleted.Should().BeFalse();

            component.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            component.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        }

        [Xunit.Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithInvalidArticle_ShouldThrowBusinessException(string invalidArticle)
        {
            Action act = () => Component.Create(
                invalidArticle,
                ValidName,
                ValidManufacturer,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                5);

            act.Should().Throw<BusinessException>()
               .WithMessage("Артикул не может быть пустым");
        }

        [Xunit.Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithInvalidName_ShouldThrowBusinessException(string invalidName)
        {
            Action act = () => Component.Create(
                ValidArticle,
                invalidName,
                ValidManufacturer,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                5);

            act.Should().Throw<BusinessException>()
               .WithMessage("Название не может быть пустым");
        }

        [Xunit.Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithInvalidManufacturer_ShouldThrowBusinessException(string invalidManufacturer)
        {
            Action act = () => Component.Create(
                ValidArticle,
                ValidName,
                invalidManufacturer,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                5);

            act.Should().Throw<BusinessException>()
               .WithMessage("Производитель не может быть пустым");
        }

        [Xunit.Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        public void Create_WithInvalidMinQuantity_ShouldThrowBusinessException(int invalidMinQuantity)
        {
            Action act = () => Component.Create(
                ValidArticle,
                ValidName,
                ValidName,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                invalidMinQuantity);

            act.Should().Throw<BusinessException>()
               .WithMessage("Минимальный остаток не может быть отрицательным или равным нулю");
        }

        [Fact]
        public void Update_WithValidData_ShouldUpdatePropertiesAndTimestamp()
        {
            var component = CreateValidComponent();
            var beforeUpdate = component.UpdatedAt;

            Thread.Sleep(10);

            var newExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60));

            component.Update("NEW-ART", "Новое имя", "Новый производитель", newExpirationDate, 15);

            component.Article.Should().Be("NEW-ART");
            component.Name.Should().Be("Новое имя");
            component.Manufacturer.Should().Be("Новый производитель");
            component.ExpirationDate.Should().Be(newExpirationDate);
            component.MinQuantity.Should().Be(15);
            component.UpdatedAt.Should().BeAfter(beforeUpdate);
        }

        [Fact]
        public void Update_WithEmptyArticle_ShouldThrowBusinessException()
        {
            var component = CreateValidComponent();

            Action act = () => component.Update(
                "",
                ValidName,
                ValidManufacturer,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                5);

            act.Should().Throw<BusinessException>()
               .WithMessage("Артикул не может быть пустым");
        }

        [Fact]
        public void Update_WithEmptyName_ShouldThrowBusinessException()
        {
            var component = CreateValidComponent();

            Action act = () => component.Update(
                ValidArticle,
                "",
                ValidManufacturer,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                5);

            act.Should().Throw<BusinessException>()
               .WithMessage("Название не может быть пустым");
        }

        [Fact]
        public void Update_WithEmptyManufacturer_ShouldThrowBusinessException()
        {
            var component = CreateValidComponent();

            Action act = () => component.Update(
                ValidArticle,
                ValidName,
                "",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                5);

            act.Should().Throw<BusinessException>()
               .WithMessage("Производитель не может быть пустым");
        }

        [Fact]
        public void Update_WithEmptyMinQuantity_ShouldThrowBusinessException()
        {
            var component = CreateValidComponent();

            Action act = () => component.Update(
                ValidArticle,
                ValidName,
                ValidManufacturer,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                -1);

            act.Should().Throw<BusinessException>()
               .WithMessage("Минимальный остаток не может быть отрицательным или равным нулю");
        }

        [Fact]
        public void Delete_WhenNotDeleted_ShouldSetIsDeletedTrue()
        {
            var component = CreateValidComponent();

            component.Delete();

            component.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_WhenAlreadyDeleted_ShouldNotThrow()
        {
            var component = CreateValidComponent();
            component.Delete();

            Action act = () => component.Delete();

            act.Should().NotThrow();
            component.IsDeleted.Should().BeTrue();
        }
    }
}
