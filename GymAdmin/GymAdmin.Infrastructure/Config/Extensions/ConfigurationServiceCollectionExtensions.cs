using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Infrastructure.Config.InitializationExtensions;
using GymAdmin.Infrastructure.Config.Options;
using GymAdmin.Infrastructure.Data;
using GymAdmin.Infrastructure.Data.Repositories;
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

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    private static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var serilogConfig = configuration.GetOptions<SerilogConfig>("SerilogConfig");

        Log.Logger = new LoggerConfiguration()
         .MinimumLevel.Is(Enum.Parse<LogEventLevel>(serilogConfig.MinimumLevel))
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
