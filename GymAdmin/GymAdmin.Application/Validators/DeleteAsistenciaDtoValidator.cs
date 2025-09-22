using FluentValidation;
using GymAdmin.Applications.DTOs.Asistencia;

namespace GymAdmin.Applications.Validators;

public class DeleteAsistenciaDtoValidator : AbstractValidator<DeleteAsistenciaDto>
{
    public DeleteAsistenciaDtoValidator()
    {
        RuleFor(a => a.Id)
            .GreaterThan(0).WithMessage("El Id debe ser un número positivo.");
        RuleFor(a => a.SocioId)
            .GreaterThan(0).WithMessage("El SocioId debe ser un número positivo.");
    }
}
