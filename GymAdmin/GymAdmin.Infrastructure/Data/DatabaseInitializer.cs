using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly GymAdminDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(GymAdminDbContext context,
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Inicializando base de datos...");

            // Verificar migraciones pendientes
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            _logger.LogInformation("Migraciones pendientes: {Count}", pendingMigrations.Count());

            // Aplicar migraciones
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Migraciones aplicadas");

            // Verificar tablas creadas
            await VerifyDatabaseStructureAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inicializando base de datos");
            throw;
        }
    }

    private async Task VerifyDatabaseStructureAsync()
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";

        using var reader = await command.ExecuteReaderAsync();

        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        _logger.LogInformation("Tablas en la BD: {Tables}", string.Join(", ", tables));
    }
}