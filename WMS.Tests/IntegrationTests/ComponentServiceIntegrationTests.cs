using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
using WMS.Infrastructure;
using WMS.Infrastructure.Context;
using Xunit;
using Assert = Xunit.Assert;
using Theory = Xunit.TheoryAttribute;

namespace WMS.Test.IntegrationTests.Services;

// Проверка уникальности артикула: дубль должен давать понятное BusinessException, а не ошибку уникального индекса БД
public class ComponentServiceIntegrationTests : IntegrationTestBase
{
    private Component _existing = null!;

    private async Task SeedAsync()
    {
        _existing = Component.Create("ART-001", "Резистор 10 кОм", "Yageo", null, 5);

        await using var db = CreateContext();
        db.Component.Add(_existing);
        await db.SaveChangesAsync();
    }

    private ComponentService CreateService()
    {
        var holder = new DbContextHolder();

        return new ComponentService(
            new ComponentRepository(holder),
            new StockRepository(holder),
            new UnitOfWorkFactory(new TestDbContextFactory(DbOptions), holder),
            NullLogger<ComponentService>.Instance);
    }

    [Theory]
    [InlineData("ART-001 ")]
    [InlineData("  art-001")]
    public async Task AddAsync_DuplicateArticleWithSpacesOrCase_ThrowsBusinessException(string article)
    {
        await SeedAsync();

        var dto = new ComponentEditDto(Guid.Empty, article, "Другой", "Другой", null, 1);

        await Assert.ThrowsAsync<BusinessException>(() => CreateService().AddAsync(dto));

        await using var db = CreateContext();
        Assert.Equal(1, await db.Component.CountAsync());
    }

    [Fact]
    public async Task AddAsync_ArticleWithSpaces_SavesTrimmed()
    {
        await SeedAsync();

        await CreateService().AddAsync(new ComponentEditDto(Guid.Empty, " ART-002 ", "Конденсатор", "Murata", null, 1));

        await using var db = CreateContext();
        Assert.True(await db.Component.AnyAsync(c => c.Article == "ART-002"));
    }

    [Fact]
    public async Task UpdateAsync_ArticleOfAnotherComponentWithSpaces_ThrowsAndKeepsOriginal()
    {
        await SeedAsync();
        var other = Component.Create("ART-002", "Конденсатор", "Murata", null, 1);
        await using (var db = CreateContext())
        {
            db.Component.Add(other);
            await db.SaveChangesAsync();
        }

        var dto = new ComponentEditDto(other.Id, "ART-001 ", other.Name, other.Manufacturer, null, 1);

        await Assert.ThrowsAsync<BusinessException>(() => CreateService().UpdateAsync(dto));

        await using var check = CreateContext();
        Assert.Equal("ART-002", (await check.Component.SingleAsync(c => c.Id == other.Id)).Article);
    }

    [Fact]
    public async Task UpdateAsync_SameArticle_IsNotTreatedAsDuplicate()
    {
        await SeedAsync();

        var dto = new ComponentEditDto(_existing.Id, " art-001 ", "Новое название", "Yageo", null, 5);

        await CreateService().UpdateAsync(dto);

        await using var db = CreateContext();
        var saved = await db.Component.SingleAsync();
        Assert.Equal("art-001", saved.Article);
        Assert.Equal("Новое название", saved.Name);
    }
}
