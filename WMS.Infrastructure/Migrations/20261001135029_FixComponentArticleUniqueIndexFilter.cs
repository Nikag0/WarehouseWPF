using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixComponentArticleUniqueIndexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_components_Article",
                table: "components");

            migrationBuilder.CreateIndex(
                name: "IX_components_Article",
                table: "components",
                column: "Article",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_components_Article",
                table: "components");

            migrationBuilder.CreateIndex(
                name: "IX_components_Article",
                table: "components",
                column: "Article",
                unique: true);
        }
    }
}
