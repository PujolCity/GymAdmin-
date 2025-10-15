using AutoUpdaterDotNET;
using GymAdmin.Applications.AppModule;
using GymAdmin.Desktop.ConfigStartup;
using GymAdmin.Infrastructure.Config.Extensions;
using GymAdmin.Infrastructure.Config.Options;
using GymAdmin.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.IO;
using System.Windows;

namespace GymAdmin.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder() // appsettings.json, logging, env, etc.
             .ConfigureAppConfiguration((hostingContext, cfg) =>
             {
                 var env = hostingContext.HostingEnvironment; 
                 cfg.SetBasePath(AppContext.BaseDirectory);
                 cfg.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(); 
             })
            .ConfigureServices((ctx, services) =>
            {
                // Infra/DI propios
                services.ConfigureDesktopInfrastructure(ctx.Configuration);
                services.AddUserValidators();
                services.ConfigureUIDesktop(ctx.Configuration);
                services.AddTransient<MainWindow>();

                // Options: bind Updates
                services.Configure<InstallerConfig>(ctx.Configuration.GetSection(nameof(InstallerConfig)));
            })
            .UseSerilog()
            .Build();

        // Inicializar DB si aplica (migraciones/seed)
        using (var scope = _host.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            initializer.InitializeAsync().GetAwaiter().GetResult();

            // Leer Options y lanzar AutoUpdater con el feed correcto
            var updateOpts = scope.ServiceProvider.GetRequiredService<IOptions<InstallerConfig>>().Value;
            var feedUrl = updateOpts.FeedUrl ?? string.Empty;

            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.Mandatory = false;
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.ShowRemindLaterButton = false;

            // fallback por si faltara config
            AutoUpdater.Start(string.IsNullOrWhiteSpace(feedUrl)
                ? "https://pujolcity.github.io/GymAdmin-/update.xml"
                : feedUrl);
        }

        // Mostrar ventana principal
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        Serilog.Log.CloseAndFlush();
        base.OnExit(e);
    }
}