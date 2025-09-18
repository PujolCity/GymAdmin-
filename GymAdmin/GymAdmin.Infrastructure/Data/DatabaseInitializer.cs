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

            if (await _context.Socios.AnyAsync(ct))
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

        // — Socios (DNI cifrado + hash)
        var nombres = new[] { "Lucas", "Sofía", "Mateo", "Valentina", "Benjamín", "Martina", "Santiago", "Lucía", "Thiago", "Emma" };
        var apellidos = new[] { "Gómez", "Fernández", "López", "Díaz", "Martínez", "Pérez", "Sosa", "Romero", "Álvarez", "Torres" };
        var rnd = new Random(20250915); // determinístico

        var socios = new List<Socio>();
        for (int i = 0; i < 25; i++)
        {
            var nombre = nombres[rnd.Next(nombres.Length)];
            var ape = apellidos[rnd.Next(apellidos.Length)];
            var dni = (30000000 + rnd.Next(0, 9_999_999)).ToString();

            var socio = new Socio
            {
                Nombre = nombre,
                Apellido = ape,
                FechaRegistro = DateTime.UtcNow.AddDays(-rnd.Next(10, 90)),
                CreditosRestantes = 0,
                TotalCreditosComprados = 0,
                ExpiracionMembresia = null,
                Dni = dni // NotMapped, para que la entidad se auto-procese
            };

            // Cifrado + hash (usa tus métodos)
            socio.HandleEncryption(_crypto);

            socios.Add(socio);
        }
        _context.Socios.AddRange(socios);
        await _context.SaveChangesAsync(ct);

        // — Pagos por socio (1..2 pagos recientes)
        var pagos = new List<Pagos>();
        foreach (var s in socios)
        {
            int pagosCount = 1 + rnd.Next(0, 2);
            DateTime? ultimoVenc = null;

            for (int k = 0; k < pagosCount; k++)
            {
                var plan = planes[rnd.Next(planes.Length)];
                var metodo = metodos[rnd.Next(metodos.Length)];

                // Fecha de pago: últimos 45 días
                var fechaPagoLocal = DateTime.Now.AddDays(-rnd.Next(0, 45)).AddHours(rnd.Next(0, 23));
                var baseDate = fechaPagoLocal.Date;
                var vencLocal = baseDate.AddDays(plan.DiasValidez).AddDays(1).AddTicks(-1);
                var vencUtc = DateTime.SpecifyKind(vencLocal, DateTimeKind.Local).ToUniversalTime();

                var pago = new Pagos
                {
                    SocioId = s.Id,
                    PlanMembresiaId = plan.Id,
                    Precio = plan.Precio, // si no tenés Precio, asigná un valor fijo
                    FechaPago = fechaPagoLocal.ToUniversalTime(),
                    Observaciones = "Pago demo",
                    CreditosAsignados = plan.Creditos,
                    FechaVencimiento = vencUtc,
                    MetodoPagoId = metodo.Id,
                    Estado = EstadoPago.Pagado
                };
                pagos.Add(pago);

                // Política: reset
                s.CreditosRestantes = plan.Creditos;
                s.TotalCreditosComprados += plan.Creditos;
                s.ExpiracionMembresia = vencUtc;
                ultimoVenc = vencUtc;
            }

            // — Sembrar algunas asistencias (0..3) sin exceder créditos
            var usos = rnd.Next(0, Math.Min(3, s.CreditosRestantes));
            for (int u = 0; u < usos; u++)
            {
                // día de asistencia dentro de la vigencia actual (si existe)
                DateTime entradaLocal;
                if (ultimoVenc.HasValue)
                {
                    var venLocal = ultimoVenc.Value.ToLocalTime().Date;
                    var desdeLocal = DateTime.Now.Date.AddDays(-7);
                    entradaLocal = desdeLocal.AddDays(rnd.Next(0, Math.Max(1, (venLocal - desdeLocal).Days + 1)))
                                              .AddHours(9 + rnd.Next(0, 10));
                }
                else
                {
                    entradaLocal = DateTime.Now.Date.AddDays(-rnd.Next(0, 7)).AddHours(9 + rnd.Next(0, 10));
                }

                var asistencia = new Asistencia
                {
                    SocioId = s.Id,
                    Entrada = DateTime.SpecifyKind(entradaLocal, DateTimeKind.Local).ToUniversalTime(),
                    SeUsoCredito = true
                };

                // descontar saldo coherente con tu dominio
                s.UsarCredito();

                _context.Asistencias.Add(asistencia);
            }
        }

        _context.Pagos.AddRange(pagos);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Seed completado: {Socios} socios, {Planes} planes, {Metodos} métodos, {Pagos} pagos.",
            socios.Count, planes.Length, metodos.Length, pagos.Count);
    }
}