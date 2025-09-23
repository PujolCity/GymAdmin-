using FluentValidation;
using GymAdmin.Applications.DTOs.Asistencia;

namespace GymAdmin.Applications.Validators;

public class CreateAsistenciaDtoValidation : AbstractValidator<CreateAsistenciaDto>
{
    public CreateAsistenciaDtoValidation()
    {
        RuleFor(x => x.IdSocio)
            .GreaterThan(0).WithMessage("El ID del socio es obligatorio y debe ser mayor que cero.");
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha de la asistencia es obligatoria.");
    }
}
