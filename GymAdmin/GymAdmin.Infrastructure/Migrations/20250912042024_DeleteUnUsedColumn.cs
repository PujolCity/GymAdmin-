using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteUnUsedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasPorSemana",
                table: "PlanesMembresia");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiracionMembresia",
                table: "Socios",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiracionMembresia",
                table: "Socios",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiasPorSemana",
                table: "PlanesMembresia",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
