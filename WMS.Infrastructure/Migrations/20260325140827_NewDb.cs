using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Article = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MinQuantity = table.Column<int>(type: "integer", nullable: false),
                    IsDelet = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_components", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Operator = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "operators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Surname = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Patronymic = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "racks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Row = table.Column<int>(type: "integer", nullable: false),
                    RackNum = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_racks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "operation_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CellId = table.Column<Guid>(type: "uuid", nullable: false),
                    RackId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityBefore = table.Column<int>(type: "integer", nullable: false),
                    QuantityAfter = table.Column<int>(type: "integer", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_operation_items_operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RackId = table.Column<Guid>(type: "uuid", nullable: false),
                    Line = table.Column<int>(type: "integer", nullable: false),
                    Column = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cells_racks_RackId",
                        column: x => x.RackId,
                        principalTable: "racks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RackId = table.Column<Guid>(type: "uuid", nullable: false),
                    CellId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stocks_cells_CellId",
                        column: x => x.CellId,
                        principalTable: "cells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stocks_components_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stocks_racks_RackId",
                        column: x => x.RackId,
                        principalTable: "racks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cells_RackId_Line_Column",
                table: "cells",
                columns: new[] { "RackId", "Line", "Column" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_components_Article",
                table: "components",
                column: "Article",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operation_items_OperationId",
                table: "operation_items",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_operators_Surname_Name_Patronymic",
                table: "operators",
                columns: new[] { "Surname", "Name", "Patronymic" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_racks_Row_RackNum",
                table: "racks",
                columns: new[] { "Row", "RackNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stocks_CellId",
                table: "stocks",
                column: "CellId");

            migrationBuilder.CreateIndex(
                name: "IX_stocks_ComponentId_RackId_CellId",
                table: "stocks",
                columns: new[] { "ComponentId", "RackId", "CellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stocks_RackId",
                table: "stocks",
                column: "RackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operation_items");

            migrationBuilder.DropTable(
                name: "operators");

            migrationBuilder.DropTable(
                name: "stocks");

            migrationBuilder.DropTable(
                name: "operations");

            migrationBuilder.DropTable(
                name: "cells");

            migrationBuilder.DropTable(
                name: "components");

            migrationBuilder.DropTable(
                name: "racks");
        }
    }
}
