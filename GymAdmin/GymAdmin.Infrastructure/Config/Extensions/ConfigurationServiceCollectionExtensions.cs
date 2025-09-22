using GymAdmin.Applications.Interactor.AsistenciaInteractors;
using GymAdmin.Applications.Interactor.PagosInteractors;
using GymAdmin.Applications.Interactor.PlanesMembresia;
using GymAdmin.Applications.Interactor.SociosInteractors;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Infrastructure.Config.FolderConfig;
using GymAdmin.Infrastructure.Config.InitializationExtensions;
using GymAdmin.Infrastructure.Config.Options;
using GymAdmin.Infrastructure.Data;
using GymAdmin.Infrastructure.Data.Repositories;
using GymAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace GymAdmin.Infrastructure.Config.Extensions;

public static class ConfigurationServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDesktopInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PathsConfig>(configuration.GetSection(nameof(PathsConfig)));
        services.AddSingleton<IAppPaths, AppPaths>();

        services.AddLoggingConfiguration(configuration);
        services.AddDatabaseConfiguration(configuration);
        services.AddRepositories();
        services.AddServices();
        services.AddInteractors();

        // Servicio Encriptado
        services.AddSingleton<ICryptoService, AesCryptoService>();

        return services;
    }

    private static IServiceCollection AddInteractors(this IServiceCollection services)
    {
        services.AddTransient<ISocioCreateInteractor, SocioCreateInteractor>();
        services.AddTransient<IGetAllSociosInteractor, GetAllSociosInteractor>();
        services.AddTransient<IDeleteSocioInteractor, DeleteSocioInteractor>();
        services.AddTransient<IUpdateSocioInteractor, UpdateSocioInteractor>();
        services.AddTransient<IGetSocioByIdInteractor, GetSocioByIdInteractor>();

        services.AddTransient<IGetPlanesMembresiaInteractor, GetPlanesMembresiaInteractor>();
        services.AddTransient<ICreateOrUpdatePlanInteractor, CreateOrUpdatePlanInteractor>();
        services.AddTransient<IDeletePlanMembresiaInteractor, DeletePlanMembresiaInteractor>();

        services.AddTransient<IGetPagosInteractor, GetPagosInteractor>();
        services.AddTransient<IGetMetodosPagoInteractor, GetMetodosPagoInteractor>();
        services.AddTransient<IGetSociosLookupInteractor, GetSociosLookupInteractor>();
        services.AddTransient<ICreatePagoInteractor, CreatePagoInteractor>();
        services.AddTransient<IAnularPagoInteractor, AnularPagoInteractor>();

        services.AddTransient<ICreateAsistenciaInteractor, CreateAsistenciaInteractor>();
        services.AddTransient<IGetAsistenciasBySocioInteractor, GetAsistenciasBySocioInteractor>();
        services.AddTransient<IUpdateAsistenciaInteractor, UpdateAsistenciaInteractor>();
        services.AddTransient<IDeleteAsistenciaInteractor, DeleteAsistenciaInteractor>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<ISocioService, SocioService>();
        services.AddTransient<IPlanMembresiaService, PlanMembresiaService>();
        services.AddTransient<IPagosServices, PagosServices>();
        services.AddTransient<IAsistenciaService, AsistenciaService>();
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISocioRepository, SocioRepository>();
        services.AddScoped<ITransaction, EfCoreTransaction>();

        return services;
    }

    private static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        Serilog.Debugging.SelfLog.Enable(m => System.Diagnostics.Debug.WriteLine("[Serilog] " + m));

        // 1) Leer config
        var serilogCfg = configuration.GetSection("SerilogConfig").Get<SerilogConfig>();
        var pathsCfg = configuration.GetSection("PathsConfig").Get<PathsConfig>();

        // 2) Armar ruta absoluta: %MyDocuments%\GymAdmin\Logs\log-.txt
        var root = ExpandRoot(pathsCfg.Root);
        var logsDir = Path.Combine(root, pathsCfg.LogsDir ?? "Logs");
        Directory.CreateDirectory(logsDir);
        var logFilePattern = Path.Combine(logsDir, pathsCfg.LogFilePattern ?? "log-.txt");

        // 3) Nivel mínimo
        var minLevel = Enum.TryParse<Serilog.Events.LogEventLevel>(serilogCfg.MinimumLevel, true, out var lvl)
            ? lvl : Serilog.Events.LogEventLevel.Information;

        // 4) Crear logger AHORA (no en una lambda diferida)
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(minLevel)
            .WriteTo.Debug(outputTemplate: serilogCfg.ConsoleOutputTemplate)
            .WriteTo.File(
                path: logFilePattern,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: serilogCfg.RetainedFileCountLimit,
                outputTemplate: serilogCfg.FileOutputTemplate,
                shared: true)
            .CreateLogger();

        Log.Logger = logger;
        services.AddSingleton<Serilog.ILogger>(logger);

        services.AddLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddSerilog(Log.Logger, dispose: true);
        });

        Log.Information("Logger inicializado. Archivo: {LogFile}", logFilePattern);

        return services;
    }

    private static string ExpandRoot(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        if (root.StartsWith("%MyDocuments%", StringComparison.OrdinalIgnoreCase))
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var tail = root.Substring("%MyDocuments%".Length).TrimStart('\\', '/');
            return Path.Combine(docs, tail);
        }

        return Path.GetFullPath(Environment.ExpandEnvironmentVariables(root));
    }

    private static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GymAdminDbContext>((sp, options) =>
        {
            var paths = sp.GetRequiredService<IAppPaths>();
            var connectionString = $"Data Source={paths.DbFile}";

            options.UseSqlite(connectionString, sqlite =>
            {
                sqlite.MigrationsAssembly(typeof(GymAdminDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<DatabaseInitializer>();
        return services;
    }
}
