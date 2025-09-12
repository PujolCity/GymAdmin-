using FluentValidation;
using GymAdmin.Applications.DTOs.MembresiasDto;

namespace GymAdmin.Applications.Validators;

public class PlanMembresiaCreateEditValidator : AbstractValidator<PlanMembresiaDto>
{
    public PlanMembresiaCreateEditValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Creditos).GreaterThan(0);
        RuleFor(x => x.DiasValidez).GreaterThan(0);
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0);
    }
}
