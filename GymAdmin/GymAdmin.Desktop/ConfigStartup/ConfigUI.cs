using GymAdmin.Desktop.ViewModels;
using GymAdmin.Desktop.ViewModels.Asistencia;
using GymAdmin.Desktop.ViewModels.Configuracion;
using GymAdmin.Desktop.ViewModels.Dialogs;
using GymAdmin.Desktop.ViewModels.Membresias;
using GymAdmin.Desktop.ViewModels.Pagos;
using GymAdmin.Desktop.ViewModels.Socios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymAdmin.Desktop.ConfigStartup;

public static class ConfigUI
{
    public static IServiceCollection ConfigureUIDesktop(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddViewModels();
        services.AddWindows();
        return services;
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<InicioViewModel>();
        services.AddTransient<ConfigViewModel>();
        services.AddTransient<PagosViewModel>();
        services.AddTransient<SociosViewModel>();
        services.AddTransient<AddSocioViewModel>();
        services.AddTransient<PlanesMembresiaViewModel>();
        services.AddTransient<AddEditPlanViewModel>();
        services.AddTransient<ConfirmDialogViewModel>();
        services.AddTransient<AddPagoViewModel>();
        services.AddTransient<VerPagoViewModel>();
        services.AddTransient<EditarSocioViewModel>();
        services.AddTransient<EditAsistenciaDialogViewModel>();
        services.AddTransient<ConfigViewModel>();

        return services;
    }

    private static IServiceCollection AddWindows(this IServiceCollection services)
    {
        services.AddTransient<MainWindow>();
        return services;
    }
}
