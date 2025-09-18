using GymAdmin.Applications.AppModule;
using GymAdmin.Desktop.ConfigStartup;
using GymAdmin.Infrastructure.Config.Extensions;
using GymAdmin.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.SetBasePath(Directory.GetCurrentDirectory());
                cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((ctx, services) =>
            {
                services.ConfigureDesktopInfrastructure(ctx.Configuration);
                services.AddUserValidators();
                services.ConfigureUIDesktop(ctx.Configuration);
                services.AddTransient<MainWindow>();
            })
            .UseSerilog()
            .Build();

        // Inicializar DB si aplica (migraciones/seed)
        using (var scope = _host.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            initializer.InitializeAsync().GetAwaiter().GetResult();
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