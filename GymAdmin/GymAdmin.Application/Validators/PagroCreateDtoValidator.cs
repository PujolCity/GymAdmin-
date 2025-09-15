using FluentValidation;
using GymAdmin.Applications.DTOs.PagosDto;

namespace GymAdmin.Applications.Validators;

public class PagroCreateDtoValidator : AbstractValidator<PagoCreateDto>
{
    public PagroCreateDtoValidator()
    {
        RuleFor(p => p.SocioId)
            .GreaterThan(0).WithMessage("El ID del socio debe ser mayor que cero.");
        RuleFor(p => p.PlanMembresiaId)
            .GreaterThan(0).WithMessage("El ID del plan de membresía debe ser mayor que cero.");
        RuleFor(p => p.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor que cero.");
        RuleFor(p => p.MetodoPagoId)
            .GreaterThan(0).WithMessage("El ID del método de pago debe ser mayor que cero.");
        RuleFor(p => p.CreditosAsignados)
            .GreaterThanOrEqualTo(0).WithMessage("Los créditos asignados no pueden ser negativos.");
        RuleFor(p => p.FechaVencimiento)
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de vencimiento debe ser una fecha futura.");
    }
}
