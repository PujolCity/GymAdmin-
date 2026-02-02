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

            migrationBuilder.Sql("""
            WITH ordered AS (
                SELECT Id,
                       ROW_NUMBER() OVER (
                           ORDER BY 
                               CASE WHEN IFNULL(Nombre,'') = '' THEN 1 ELSE 0 END,
                               Nombre
                       ) AS rn
                FROM MetodosPago
            )
            UPDATE MetodosPago
            SET Orden = (SELECT rn FROM ordered WHERE ordered.Id = MetodosPago.Id)
            WHERE IFNULL(Orden, 0) = 0;
        """);
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

            migrationBuilder.Sql("""
                    UPDATE MetodosPago
                    SET Orden = 0;
                """);
        }
    }
}
