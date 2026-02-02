using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAndChangePropertiesToPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AjusteImporte",
                table: "Pagos",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoFinal",
                table: "Pagos",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TipoAjusteAplicado",
                table: "Pagos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Ninguno");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorAjusteAplicado",
                table: "Pagos",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_MontoFinal",
                table: "Pagos",
                column: "MontoFinal");

            migrationBuilder.Sql("UPDATE Pagos SET MontoFinal = Precio WHERE MontoFinal IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagos_MontoFinal",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "AjusteImporte",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MontoFinal",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "TipoAjusteAplicado",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ValorAjusteAplicado",
                table: "Pagos");
        }
    }
}
