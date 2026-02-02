using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CambioEstadoPagoAString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Pagos",
                type: "TEXT",
                nullable: false,
                defaultValue: "Pagado",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Pagos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "Pagado");
        }
    }
}
