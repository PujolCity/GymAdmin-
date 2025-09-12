using FluentValidation;
using GymAdmin.Applications.DTOs.SociosDto;

namespace GymAdmin.Applications.Validators;

public class SocioCreateDtoValidator : AbstractValidator<SocioCreateDto>
{
    public SocioCreateDtoValidator()
    {
        RuleFor(s => s.Dni)
            .NotEmpty().WithMessage("El DNI es obligatorio.")
            .Matches(@"^\d{7,8}").WithMessage("El DNI debe tener entre 7 y 8 dígitos.");

        RuleFor(s => s.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres.");
       
        RuleFor(s => s.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(50).WithMessage("El apellido no puede exceder los 50 caracteres.");
        
        //RuleFor(x => x.Email)
        //     .NotEmpty().WithMessage("Email es requerido")
        //     .EmailAddress().WithMessage("Formato de email inválido")
        //     .Must(BeAValidEmailDomain).WithMessage("El dominio del email no es válido.");
    }

    // Método opcional para validar el dominio del email (ejemplo: rechazar dominios temporales)
    private bool BeAValidEmailDomain(string email)
    {
        var domainsToBlock = new[] { "temp.com", "fake.org" }; // Ejemplo: lista de dominios no permitidos
        var domain = email.Split('@').LastOrDefault();
        return domain != null && !domainsToBlock.Contains(domain);
    }
}
