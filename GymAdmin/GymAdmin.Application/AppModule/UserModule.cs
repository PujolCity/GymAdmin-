using FluentValidation;
using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Applications.Interfaces.ValidacionesUI;
using GymAdmin.Applications.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace GymAdmin.Applications.AppModule;

/// <summary>
/// Module for user-related services.
/// </summary>
public static class UserModule
{
    public static IServiceCollection AddUserValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SocioCreateDtoValidator>();
        services.AddTransient<IValidator<SocioCreateDto>, SocioCreateDtoValidator>();
        services.AddSingleton<IValidationUIService, ValidationUIService>();

        return services;
    }
}