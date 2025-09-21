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
        
    }
}
