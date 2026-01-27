using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_cell_stocks",
                table: "cell_stocks");

            migrationBuilder.RenameTable(
                name: "cell_stocks",
                newName: "stocks");

            migrationBuilder.RenameIndex(
                name: "IX_cell_stocks_ComponentId_CellId",
                table: "stocks",
                newName: "IX_stocks_ComponentId_CellId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stocks",
                table: "stocks",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_stocks",
                table: "stocks");

            migrationBuilder.RenameTable(
                name: "stocks",
                newName: "cell_stocks");

            migrationBuilder.RenameIndex(
                name: "IX_stocks_ComponentId_CellId",
                table: "cell_stocks",
                newName: "IX_cell_stocks_ComponentId_CellId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cell_stocks",
                table: "cell_stocks",
                column: "Id");
        }
    }
}
