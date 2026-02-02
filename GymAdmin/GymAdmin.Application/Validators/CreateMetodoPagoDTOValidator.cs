using FluentValidation;
using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.Validators;

public class CreateMetodoPagoDTOValidator : AbstractValidator<MetodoPagoCreateDTO>
{
    public CreateMetodoPagoDTOValidator()
    {
        RuleFor(x => x.ValorAjuste)
            .GreaterThan(0).WithMessage("El Valor Ajuste obligatorio y debe ser mayor que cero.")
            .When(x => x.TipoAjuste != TipoAjusteSaldo.Ninguno);
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El Nombre es obligatorio.");
    }
}
