using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly GymAdminDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly ICryptoService _crypto;

    public DatabaseInitializer(GymAdminDbContext context,
        ILogger<DatabaseInitializer> logger,
        ICryptoService crypto)
    {
        _context = context;
        _logger = logger;
        _crypto = crypto;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Inicializando base de datos...");

            // Verificar migraciones pendientes
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(ct);
            _logger.LogInformation("Migraciones pendientes: {Count}", pendingMigrations.Count());

            // Aplicar migraciones
            await _context.Database.MigrateAsync(ct);
            _logger.LogInformation("Migraciones aplicadas");

            // Verificar tablas creadas
            await VerifyDatabaseStructureAsync(ct);

            if (await _context.MetodosPago.AnyAsync(ct))
            {
                _logger.LogInformation("Seed omitido: ya existen socios en la base.");
                return;
            }
            //seed
            await SeedAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inicializando base de datos");
            throw;
        }
    }

    private async Task VerifyDatabaseStructureAsync(CancellationToken ct = default)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";

        using var reader = await command.ExecuteReaderAsync(ct);

        var tables = new List<string>();
        while (await reader.ReadAsync(ct))
        {
            tables.Add(reader.GetString(0));
        }

        _logger.LogInformation("Tablas en la BD: {Tables}", string.Join(", ", tables));
    }


    private async Task SeedAsync(CancellationToken ct)
    {
        _logger.LogInformation("Iniciando seed de datos de demo…");

        // — Métodos de pago
        var metodos = new[]
        {
            new MetodoPago { Nombre = "Efectivo", IsActive = true },
            new MetodoPago { Nombre = "Tarjeta",  IsActive = true },
            new MetodoPago { Nombre = "Transferencia", IsActive = true },
            new MetodoPago { Nombre = "Mercado Pago", IsActive = true },
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

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Seed completado: {Planes} planes, {Metodos} métodos.",
            planes.Length, metodos.Length);
    }
}