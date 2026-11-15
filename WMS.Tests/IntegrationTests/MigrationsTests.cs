using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure.Context;
using Xunit;
using Assert = Xunit.Assert;

namespace WMS.Test.IntegrationTests;

public class MigrationsTests
{
    // Если модель (сущности и конфигурации) изменили, а миграцию не создали,
    // на складе программа накатит старую схему и упадёт при первом обращении к новой колонке или таблице.
    // Проверка не подключается к БД: модель сравнивается со снимком последней миграции.
    [Fact]
    public void Model_HasNoChangesWithoutMigration()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_check")
            .Options;

        using var context = new AppDbContext(options);

        Assert.False(
            context.Database.HasPendingModelChanges(),
            "Модель изменилась, но миграция не создана. Выполните: dotnet ef migrations add <Название>");
    }
}
