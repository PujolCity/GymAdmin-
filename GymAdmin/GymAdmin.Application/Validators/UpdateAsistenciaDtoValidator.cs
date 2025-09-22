using FluentValidation;
using GymAdmin.Applications.DTOs.Asistencia;

namespace GymAdmin.Applications.Validators;

public class UpdateAsistenciaDtoValidator : AbstractValidator<AsistenciaDto>
{
    public UpdateAsistenciaDtoValidator()
    {
        RuleFor(a => a.Id)
            .GreaterThan(0).WithMessage("El Id debe ser un número positivo.");
        RuleFor(a => a.SocioId)
            .GreaterThan(0).WithMessage("El Id del socio debe ser un número positivo.");
        RuleFor(a => a.Entrada)
            .NotEmpty().WithMessage("La fecha es obligatoria.");
    }
}
