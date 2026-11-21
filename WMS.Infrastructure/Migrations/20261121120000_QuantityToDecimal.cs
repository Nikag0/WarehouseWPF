using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <summary>
    /// Количества переводятся в numeric(18,3): до трёх знаков после запятой без ошибок округления double.
    /// Минимальный остаток компонента тоже становится дробным.
    /// </summary>
    public partial class QuantityToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Значения double округляются до 3 знаков, чтобы "хвосты" вида 0.10000000000000009 не перешли в новый тип
            migrationBuilder.Sql(@"ALTER TABLE stock ALTER COLUMN ""Quantity"" TYPE numeric(18,3) USING round(""Quantity""::numeric, 3);");
            migrationBuilder.Sql(@"ALTER TABLE operation_history ALTER COLUMN ""PreviousQuantity"" TYPE numeric(18,3) USING round(""PreviousQuantity""::numeric, 3);");
            migrationBuilder.Sql(@"ALTER TABLE operation_history ALTER COLUMN ""QuantityChanged"" TYPE numeric(18,3) USING round(""QuantityChanged""::numeric, 3);");
            migrationBuilder.Sql(@"ALTER TABLE operation_history ALTER COLUMN ""NewQuantity"" TYPE numeric(18,3) USING round(""NewQuantity""::numeric, 3);");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinQuantity",
                table: "component",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Quantity",
                table: "stock",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<double>(
                name: "PreviousQuantity",
                table: "operation_history",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<double>(
                name: "QuantityChanged",
                table: "operation_history",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<double>(
                name: "NewQuantity",
                table: "operation_history",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            // Дробный минимальный остаток при откате округляется до целого
            migrationBuilder.Sql(@"ALTER TABLE component ALTER COLUMN ""MinQuantity"" TYPE integer USING ceil(""MinQuantity"")::integer;");
        }
    }
}
