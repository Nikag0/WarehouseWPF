using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCells : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Row = table.Column<int>(type: "integer", nullable: false),
                    Rack = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cells", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stocks_CellId",
                table: "stocks",
                column: "CellId");

            migrationBuilder.CreateIndex(
                name: "IX_cells_Row_Rack_Position",
                table: "cells",
                columns: new[] { "Row", "Rack", "Position" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_stocks_cells_CellId",
                table: "stocks",
                column: "CellId",
                principalTable: "cells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stocks_components_ComponentId",
                table: "stocks",
                column: "ComponentId",
                principalTable: "components",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stocks_cells_CellId",
                table: "stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_stocks_components_ComponentId",
                table: "stocks");

            migrationBuilder.DropTable(
                name: "cells");

            migrationBuilder.DropIndex(
                name: "IX_stocks_CellId",
                table: "stocks");
        }
    }
}
