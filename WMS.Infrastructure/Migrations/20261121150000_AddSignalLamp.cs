using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Migrations
{
    /// <summary>
    /// Сигнальные лампы рядов. Лампа подключена к GPIO одного контроллера (signal_lamp.MicrocontrollerId),
    /// а контроллеры ряда ссылаются на свою лампу (microcontroller.SignalLampId).
    /// Таблица заполняется вручную командой разработки.
    /// </summary>
    public partial class AddSignalLamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SignalLampId",
                table: "microcontroller",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "signal_lamp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MicrocontrollerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_signal_lamp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_signal_lamp_microcontroller_MicrocontrollerId",
                        column: x => x.MicrocontrollerId,
                        principalTable: "microcontroller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_microcontroller_SignalLampId",
                table: "microcontroller",
                column: "SignalLampId");

            migrationBuilder.CreateIndex(
                name: "IX_signal_lamp_MicrocontrollerId",
                table: "signal_lamp",
                column: "MicrocontrollerId");

            migrationBuilder.AddForeignKey(
                name: "FK_microcontroller_signal_lamp_SignalLampId",
                table: "microcontroller",
                column: "SignalLampId",
                principalTable: "signal_lamp",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_microcontroller_signal_lamp_SignalLampId",
                table: "microcontroller");

            migrationBuilder.DropTable(
                name: "signal_lamp");

            migrationBuilder.DropIndex(
                name: "IX_microcontroller_SignalLampId",
                table: "microcontroller");

            migrationBuilder.DropColumn(
                name: "SignalLampId",
                table: "microcontroller");
        }
    }
}
