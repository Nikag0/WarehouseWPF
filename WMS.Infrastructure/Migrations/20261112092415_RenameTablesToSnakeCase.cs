using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cells_racks_RackId",
                table: "cells");

            migrationBuilder.DropForeignKey(
                name: "FK_sector_cells_CellId",
                table: "sector");

            migrationBuilder.DropForeignKey(
                name: "FK_stocks_cells_CellId",
                table: "stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_stocks_components_ComponentId",
                table: "stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_stocks_racks_RackId",
                table: "stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stocks",
                table: "stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_racks",
                table: "racks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_operators",
                table: "operators");

            migrationBuilder.DropPrimaryKey(
                name: "PK_operationhistory",
                table: "operationhistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_components",
                table: "components");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cells",
                table: "cells");

            migrationBuilder.RenameTable(
                name: "stocks",
                newName: "stock");

            migrationBuilder.RenameTable(
                name: "racks",
                newName: "rack");

            migrationBuilder.RenameTable(
                name: "operators",
                newName: "operator");

            migrationBuilder.RenameTable(
                name: "operationhistory",
                newName: "operation_history");

            migrationBuilder.RenameTable(
                name: "components",
                newName: "component");

            migrationBuilder.RenameTable(
                name: "cells",
                newName: "cell");

            migrationBuilder.RenameIndex(
                name: "IX_stocks_RackId",
                table: "stock",
                newName: "IX_stock_RackId");

            migrationBuilder.RenameIndex(
                name: "IX_stocks_ComponentId_RackId_CellId",
                table: "stock",
                newName: "IX_stock_ComponentId_RackId_CellId");

            migrationBuilder.RenameIndex(
                name: "IX_stocks_CellId",
                table: "stock",
                newName: "IX_stock_CellId");

            migrationBuilder.RenameIndex(
                name: "IX_racks_Column_Row",
                table: "rack",
                newName: "IX_rack_Column_Row");

            migrationBuilder.RenameIndex(
                name: "IX_operators_Surname_Name_Patronymic",
                table: "operator",
                newName: "IX_operator_Surname_Name_Patronymic");

            migrationBuilder.RenameIndex(
                name: "IX_components_Article",
                table: "component",
                newName: "IX_component_Article");

            migrationBuilder.RenameIndex(
                name: "IX_cells_RackId_Column_Row",
                table: "cell",
                newName: "IX_cell_RackId_Column_Row");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stock",
                table: "stock",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_rack",
                table: "rack",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_operator",
                table: "operator",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_operation_history",
                table: "operation_history",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_component",
                table: "component",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cell",
                table: "cell",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_cell_rack_RackId",
                table: "cell",
                column: "RackId",
                principalTable: "rack",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sector_cell_CellId",
                table: "sector",
                column: "CellId",
                principalTable: "cell",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_cell_CellId",
                table: "stock",
                column: "CellId",
                principalTable: "cell",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_component_ComponentId",
                table: "stock",
                column: "ComponentId",
                principalTable: "component",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_rack_RackId",
                table: "stock",
                column: "RackId",
                principalTable: "rack",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cell_rack_RackId",
                table: "cell");

            migrationBuilder.DropForeignKey(
                name: "FK_sector_cell_CellId",
                table: "sector");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_cell_CellId",
                table: "stock");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_component_ComponentId",
                table: "stock");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_rack_RackId",
                table: "stock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stock",
                table: "stock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_rack",
                table: "rack");

            migrationBuilder.DropPrimaryKey(
                name: "PK_operator",
                table: "operator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_operation_history",
                table: "operation_history");

            migrationBuilder.DropPrimaryKey(
                name: "PK_component",
                table: "component");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cell",
                table: "cell");

            migrationBuilder.RenameTable(
                name: "stock",
                newName: "stocks");

            migrationBuilder.RenameTable(
                name: "rack",
                newName: "racks");

            migrationBuilder.RenameTable(
                name: "operator",
                newName: "operators");

            migrationBuilder.RenameTable(
                name: "operation_history",
                newName: "operationhistory");

            migrationBuilder.RenameTable(
                name: "component",
                newName: "components");

            migrationBuilder.RenameTable(
                name: "cell",
                newName: "cells");

            migrationBuilder.RenameIndex(
                name: "IX_stock_RackId",
                table: "stocks",
                newName: "IX_stocks_RackId");

            migrationBuilder.RenameIndex(
                name: "IX_stock_ComponentId_RackId_CellId",
                table: "stocks",
                newName: "IX_stocks_ComponentId_RackId_CellId");

            migrationBuilder.RenameIndex(
                name: "IX_stock_CellId",
                table: "stocks",
                newName: "IX_stocks_CellId");

            migrationBuilder.RenameIndex(
                name: "IX_rack_Column_Row",
                table: "racks",
                newName: "IX_racks_Column_Row");

            migrationBuilder.RenameIndex(
                name: "IX_operator_Surname_Name_Patronymic",
                table: "operators",
                newName: "IX_operators_Surname_Name_Patronymic");

            migrationBuilder.RenameIndex(
                name: "IX_component_Article",
                table: "components",
                newName: "IX_components_Article");

            migrationBuilder.RenameIndex(
                name: "IX_cell_RackId_Column_Row",
                table: "cells",
                newName: "IX_cells_RackId_Column_Row");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stocks",
                table: "stocks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_racks",
                table: "racks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_operators",
                table: "operators",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_operationhistory",
                table: "operationhistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_components",
                table: "components",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cells",
                table: "cells",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_cells_racks_RackId",
                table: "cells",
                column: "RackId",
                principalTable: "racks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sector_cells_CellId",
                table: "sector",
                column: "CellId",
                principalTable: "cells",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_stocks_racks_RackId",
                table: "stocks",
                column: "RackId",
                principalTable: "racks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
