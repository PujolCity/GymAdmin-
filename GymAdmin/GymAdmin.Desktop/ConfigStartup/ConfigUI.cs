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
        return services;
    }
    
    private static IServiceCollection AddWindows(this IServiceCollection services)
    {
        services.AddTransient<MainWindow>();
        return services;
    }
}
