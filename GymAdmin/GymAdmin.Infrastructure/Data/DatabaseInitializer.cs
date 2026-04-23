using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Infrastructure.Backup;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly GymAdminDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IMigrationSafetyBackup _migrationSafetyBackup;
    private readonly IBackupService _backupService;

    public DatabaseInitializer(GymAdminDbContext context,
        ILogger<DatabaseInitializer> logger,
        IMigrationSafetyBackup migrationSafetyBackup,
        IBackupService backupService)
    {
        _context = context;
        _logger = logger;
        _migrationSafetyBackup = migrationSafetyBackup;
        _backupService = backupService;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        string? backupPath = null;
        string? dbPath = null;

        try
        {
            _logger.LogInformation("Inicializando base de datos...");

            _logger.LogInformation("Se limpian backups viejos");
            await _backupService.CleanupOldBackupsAsync(ct);

            // Verificar migraciones pendientes
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(ct);
            _logger.LogInformation("Migraciones pendientes: {Count}", pendingMigrations.Count());

            if (pendingMigrations.Any())
            {
                dbPath = GetSqliteDbPath();

                backupPath = _migrationSafetyBackup.CreatePreMigrationBackup(dbPath);
                _logger.LogInformation("Backup pre-migración creado en: {BackupPath}", backupPath);

                // Aplicar migraciones
                await _context.Database.MigrateAsync(ct);
                _logger.LogInformation("Migraciones aplicadas");
            }
            else
            {
                _logger.LogInformation("No hay migraciones pendientes. No se genera backup pre-migración.");
            }

            _logger.LogInformation("Crando Backup diario...");
            await _backupService.CreateDailyBackupAsync(ct);

            // Verificar tablas creadas
            await VerifyDatabaseStructureAsync(ct);

            if (await _context.MetodosPago.AnyAsync(ct))
            {
                _logger.LogInformation("Seed omitido: ya existen MetodosPago en la base.");
                return;
            }
            //seed
            await SeedAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inicializando base de datos");

            // Si hubo backup y conocemos la DB, restauramos
            if (!string.IsNullOrWhiteSpace(backupPath) && !string.IsNullOrWhiteSpace(dbPath))
            {
                try
                {
                    // Cerramos conexión
                    await _context.Database.CloseConnectionAsync();

                    _migrationSafetyBackup.Restore(backupPath, dbPath);
                    _logger.LogWarning("Base de datos restaurada desde backup: {BackupPath}", backupPath);
                }
                catch (Exception restoreEx)
                {
                    _logger.LogError(restoreEx, "Falló la restauración del backup pre-migración");
                }
            }

            throw;
        }
    }

    private string GetSqliteDbPath()
    {
        var connection = _context.Database.GetDbConnection();

        if (connection is SqliteConnection sqliteConnection)
            return sqliteConnection.DataSource;

        throw new InvalidOperationException("La conexión actual no es SQLite.");
    }

    private async Task VerifyDatabaseStructureAsync(CancellationToken ct = default)
    {
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;

        try
        {
            if (shouldClose)
                await _context.Database.OpenConnectionAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";

            var tables = new List<string>();

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                tables.Add(reader.GetString(0));
            }

            _logger.LogDebug("Tablas en la BD: {Tables}", string.Join(", ", tables));
        }
        finally
        {
            if (shouldClose)
                await _context.Database.CloseConnectionAsync();
        }
    }


    private async Task SeedAsync(CancellationToken ct)
    {
        _logger.LogInformation("Iniciando seed de datos de demo…");

        // — Métodos de pago
        var metodos = new[]
        {
            new MetodoPago { Nombre = "Efectivo", IsActive = true, Orden = 1 },
            new MetodoPago { Nombre = "Tarjeta",  IsActive = true , Orden = 2},
            new MetodoPago { Nombre = "Transferencia", IsActive = true, Orden = 3 },
            new MetodoPago { Nombre = "Mercado Pago", IsActive = true, Orden = 4 },
        };
        _context.MetodosPago.AddRange(metodos);
        await _context.SaveChangesAsync(ct);

        // — Planes (ajustá propiedades a tu entidad real)
        var planes = new[]
        {
            new PlanesMembresia { Nombre = "Mensual",    Creditos = 12, DiasValidez = 30, Precio = 10000m, IsActive = true },
            new PlanesMembresia { Nombre = "Quincenal",  Creditos = 6,  DiasValidez = 15, Precio = 6000m,  IsActive = true },
            new PlanesMembresia { Nombre = "Diario",     Creditos = 1,  DiasValidez = 1,  Precio = 1500m,  IsActive = true },
            new PlanesMembresia { Nombre = "Libre",      Creditos = 30, DiasValidez = 30, Precio = 18000m, IsActive = true },
        };
        _context.PlanesMembresia.AddRange(planes);
        await _context.SaveChangesAsync(ct);

        //await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Seed completado: {Planes} planes, {Metodos} métodos.",
            planes.Length, metodos.Length);
    }
}