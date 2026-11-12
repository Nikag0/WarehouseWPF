using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Помеченные удалёнными записи удаляем окончательно, иначе после удаления колонки они снова появятся в программе.
            // Нулевые остатки удалённых компонентов мешают удалению из-за внешнего ключа. Ненулевых быть не должно:
            // если они есть, миграция упадёт на внешнем ключе и данные не пострадают.
            migrationBuilder.Sql(@"DELETE FROM ""operators"" WHERE ""IsDeleted"" = TRUE;");
            migrationBuilder.Sql(@"DELETE FROM ""stocks"" WHERE ""Quantity"" = 0 AND ""ComponentId"" IN (SELECT ""Id"" FROM ""components"" WHERE ""IsDeleted"" = TRUE);");
            migrationBuilder.Sql(@"DELETE FROM ""components"" WHERE ""IsDeleted"" = TRUE;");

            migrationBuilder.DropIndex(
                name: "IX_components_Article",
                table: "components");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "operators");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "components");

            migrationBuilder.CreateIndex(
                name: "IX_components_Article",
                table: "components",
                column: "Article",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_components_Article",
                table: "components");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "operators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "components",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_components_Article",
                table: "components",
                column: "Article",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
