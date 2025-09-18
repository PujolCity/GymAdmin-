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

        services.AddTransient<IGetPlanesMembresiaInteractor, GetPlanesMembresiaInteractor>();
        services.AddTransient<ICreateOrUpdatePlanInteractor, CreateOrUpdatePlanInteractor>();
        services.AddTransient<IDeletePlanMembresiaInteractor, DeletePlanMembresiaInteractor>();

        services.AddTransient<IGetPagosInteractor, GetPagosInteractor>();
        services.AddTransient<IGetMetodosPagoInteractor, GetMetodosPagoInteractor>();
        services.AddTransient<IGetSociosLookupInteractor, GetSociosLookupInteractor>();
        services.AddTransient<ICreatePagoInteractor, CreatePagoInteractor>();
        services.AddTransient<IAnularPagoInteractor, AnularPagoInteractor>();

        services.AddTransient<ICreateAsistenciaInteractor, CreateAsistenciaInteractor>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<ISocioService, SocioService>();
        services.AddTransient<IPlanMembresiaService, PlanMembresiaService>();
        services.AddTransient<IPagosServices, PagosServices>();

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
        var serilogCfg = configuration.GetOptions<SerilogConfig>(nameof(SerilogConfig));

        Serilog.Debugging.SelfLog.Enable(m => System.Diagnostics.Debug.WriteLine("[Serilog] " + m));

        services.AddSingleton(sp =>
        {
            var paths = sp.GetRequiredService<IAppPaths>();
            Directory.CreateDirectory(Path.GetDirectoryName(paths.LogFilePattern)!);

            var serilogCfg = configuration.GetOptions<SerilogConfig>("SerilogConfig");
            var minLevel = Enum.TryParse<LogEventLevel>(serilogCfg.MinimumLevel, true, out var lvl)
                ? lvl : LogEventLevel.Information;

            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(minLevel)
                .WriteTo.Debug()
                .WriteTo.File(
                    path: paths.LogFilePattern,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: serilogCfg.RetainedFileCountLimit,
                    outputTemplate: serilogCfg.FileOutputTemplate,
                    shared: true)
                .CreateLogger();

            Log.Logger = logger;
            return logger;
        });

        services.AddLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddSerilog(Log.Logger, dispose: true); 
        });

        return services;
    }

    private static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // SqliteConfig lo podés seguir usando para nombres si querés,
        // pero la ruta sale de IAppPaths.DbFile
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
