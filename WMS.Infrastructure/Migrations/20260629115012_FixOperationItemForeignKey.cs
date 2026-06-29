using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOperationItemForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "OperationId",
                table: "operation_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_operation_items_ComponentId",
                table: "operation_items",
                column: "ComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_operation_items_components_ComponentId",
                table: "operation_items",
                column: "ComponentId",
                principalTable: "components",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_operation_items_components_ComponentId",
                table: "operation_items");

            migrationBuilder.DropIndex(
                name: "IX_operation_items_ComponentId",
                table: "operation_items");

            migrationBuilder.AlterColumn<Guid>(
                name: "OperationId",
                table: "operation_items",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
