using GymAdmin.Applications.Interactor.PagosInteractors;
using GymAdmin.Applications.Interactor.PlanesMembresia;
using GymAdmin.Applications.Interactor.SociosInteractors;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
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
        services.AddLoggingConfiguration(configuration);
        services.AddDatabaseConfiguration(configuration);
        services.AddRepositories();
        services.AddServices();
        services.AddInteractors();

        // Servicio Encriptado
        services.AddSingleton<ICryptoService>(sp =>
        {
            var cryptoService = new AesCryptoService(); // Archivo local donde se guarda/lee el secret
            return cryptoService;
        });

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

        return services;
    }

    private static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var serilogConfig = configuration.GetOptions<SerilogConfig>("SerilogConfig");

        var logFolder = Path.GetDirectoryName(serilogConfig.LogFilePath);
        if (!Directory.Exists(logFolder))
            Directory.CreateDirectory(logFolder);

        Log.Logger = new LoggerConfiguration()
         .MinimumLevel.Is(Enum.Parse<LogEventLevel>(serilogConfig.MinimumLevel))
         .WriteTo.Debug()
         .WriteTo.Console(outputTemplate: serilogConfig.ConsoleOutputTemplate)
         .WriteTo.File(
             path: serilogConfig.LogFilePath,
             rollingInterval: RollingInterval.Day,
             retainedFileCountLimit: serilogConfig.RetainedFileCountLimit,
             outputTemplate: serilogConfig.FileOutputTemplate,
             shared: true)
         .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        return services;
    }

    private static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var sqliteConfig = configuration.GetOptions<SqliteConfig>("SqliteConfig");

        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));

        var dataFolder = Path.Combine(projectRoot, sqliteConfig.NameFolder);
        Directory.CreateDirectory(dataFolder);

        var dbPath = Path.Combine(dataFolder, sqliteConfig.NameDataBase);
        var connectionString = $"Data Source={dbPath}";

        services.AddDbContext<GymAdminDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(GymAdminDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
