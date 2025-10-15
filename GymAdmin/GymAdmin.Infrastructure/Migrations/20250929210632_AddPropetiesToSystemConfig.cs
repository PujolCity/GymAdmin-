using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropetiesToSystemConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "SystemConfigs",
                newName: "TelefonoEncrypted");

            migrationBuilder.DropColumn(
                name: "DiasValidezCredito",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "SystemConfigs");

            migrationBuilder.RenameColumn(
                name: "ExpiracionAutomaticaCredito",
                table: "SystemConfigs",
                newName: "IncluirNombreEnExport");

            migrationBuilder.AddColumn<int>(
                name: "BackupRetentionCount",
                table: "SystemConfigs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarpetaBackups",
                table: "SystemConfigs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CarpetaBase",
                table: "SystemConfigs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CuitEncrypted",
                table: "SystemConfigs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailContacto",
                table: "SystemConfigs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrefijoArchivos",
                table: "SystemConfigs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "GymAdmin_");

            migrationBuilder.AlterColumn<string>(
                        name: "TelefonoEncrypted",
                        table: "SystemConfigs",
                        type: "TEXT",
                        maxLength: 500,
                        nullable: true,
                        oldClrType: typeof(string),
                        oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoBackupAt",
                table: "SystemConfigs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppEncrypted",
                table: "SystemConfigs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackupRetentionCount",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "CarpetaBackups",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "CarpetaBase",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "CuitEncrypted",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "EmailContacto",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "PrefijoArchivos",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "TelefonoEncrypted",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "UltimoBackupAt",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "WhatsAppEncrypted",
                table: "SystemConfigs");

            migrationBuilder.RenameColumn(
                name: "IncluirNombreEnExport",
                table: "SystemConfigs",
                newName: "ExpiracionAutomaticaCredito");

            migrationBuilder.AddColumn<int>(
                name: "DiasValidezCredito",
                table: "SystemConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.RenameColumn(
                name: "TelefonoEncrypted",
                table: "SystemConfigs",
                newName: "Telefono");
        }
    }
}
