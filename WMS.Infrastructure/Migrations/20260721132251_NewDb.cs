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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_components", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "microcontroller",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ip = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    DeviceAddress = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_microcontroller", x => x.Id);
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
                    Patronymic = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Column = table.Column<int>(type: "integer", nullable: false),
                    Row = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_racks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "strip",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MicrocontrollerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StripNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_strip", x => x.Id);
                    table.ForeignKey(
                        name: "FK_strip_microcontroller_MicrocontrollerId",
                        column: x => x.MicrocontrollerId,
                        principalTable: "microcontroller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operation_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CellId = table.Column<Guid>(type: "uuid", nullable: false),
                    RackId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityBefore = table.Column<int>(type: "integer", nullable: false),
                    QuantityAfter = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_operation_items_components_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Column = table.Column<int>(type: "integer", nullable: false),
                    Row = table.Column<int>(type: "integer", nullable: false)
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
                name: "sector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StripId = table.Column<Guid>(type: "uuid", nullable: false),
                    CellId = table.Column<Guid>(type: "uuid", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    StartDiode = table.Column<int>(type: "integer", nullable: false),
                    EndDiode = table.Column<int>(type: "integer", nullable: false),
                    R = table.Column<byte>(type: "smallint", nullable: false),
                    G = table.Column<byte>(type: "smallint", nullable: false),
                    B = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sector", x => x.Id);
                    table.CheckConstraint("CK_Sector_Range", "\"StartDiode\" <= \"EndDiode\"");
                    table.ForeignKey(
                        name: "FK_sector_cells_CellId",
                        column: x => x.CellId,
                        principalTable: "cells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sector_strip_StripId",
                        column: x => x.StripId,
                        principalTable: "strip",
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
                name: "IX_cells_RackId_Column_Row",
                table: "cells",
                columns: new[] { "RackId", "Column", "Row" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_components_Article",
                table: "components",
                column: "Article",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operation_items_ComponentId",
                table: "operation_items",
                column: "ComponentId");

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
                name: "IX_racks_Column_Row",
                table: "racks",
                columns: new[] { "Column", "Row" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sector_CellId",
                table: "sector",
                column: "CellId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sector_StripId_Index",
                table: "sector",
                columns: new[] { "StripId", "Index" },
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

            migrationBuilder.CreateIndex(
                name: "IX_strip_MicrocontrollerId_StripNumber",
                table: "strip",
                columns: new[] { "MicrocontrollerId", "StripNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operation_items");

            migrationBuilder.DropTable(
                name: "operators");

            migrationBuilder.DropTable(
                name: "sector");

            migrationBuilder.DropTable(
                name: "stocks");

            migrationBuilder.DropTable(
                name: "operations");

            migrationBuilder.DropTable(
                name: "strip");

            migrationBuilder.DropTable(
                name: "cells");

            migrationBuilder.DropTable(
                name: "components");

            migrationBuilder.DropTable(
                name: "microcontroller");

            migrationBuilder.DropTable(
                name: "racks");
        }
    }
}
