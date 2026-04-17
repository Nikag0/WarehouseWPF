using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnAndRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_racks_Row_RackNum",
                table: "racks");

            migrationBuilder.DropIndex(
                name: "IX_cells_RackId_Line_Column",
                table: "cells");

            migrationBuilder.RenameColumn(
                name: "RackNum",
                table: "racks",
                newName: "Column");

            migrationBuilder.RenameColumn(
                name: "Line",
                table: "cells",
                newName: "Row");

            migrationBuilder.CreateIndex(
                name: "IX_racks_Column_Row",
                table: "racks",
                columns: new[] { "Column", "Row" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cells_RackId_Column_Row",
                table: "cells",
                columns: new[] { "RackId", "Column", "Row" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_racks_Column_Row",
                table: "racks");

            migrationBuilder.DropIndex(
                name: "IX_cells_RackId_Column_Row",
                table: "cells");

            migrationBuilder.RenameColumn(
                name: "Column",
                table: "racks",
                newName: "RackNum");

            migrationBuilder.RenameColumn(
                name: "Row",
                table: "cells",
                newName: "Line");

            migrationBuilder.CreateIndex(
                name: "IX_racks_Row_RackNum",
                table: "racks",
                columns: new[] { "Row", "RackNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cells_RackId_Line_Column",
                table: "cells",
                columns: new[] { "RackId", "Line", "Column" },
                unique: true);
        }
    }
}
