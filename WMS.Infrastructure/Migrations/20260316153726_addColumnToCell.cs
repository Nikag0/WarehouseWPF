using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addColumnToCell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cells_Row_Rack_Line",
                table: "cells");

            migrationBuilder.CreateIndex(
                name: "IX_cells_Row_Rack_Line_Column",
                table: "cells",
                columns: new[] { "Row", "Rack", "Line", "Column" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cells_Row_Rack_Line_Column",
                table: "cells");

            migrationBuilder.CreateIndex(
                name: "IX_cells_Row_Rack_Line",
                table: "cells",
                columns: new[] { "Row", "Rack", "Line" },
                unique: true);
        }
    }
}
