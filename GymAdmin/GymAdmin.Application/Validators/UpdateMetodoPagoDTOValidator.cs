using FluentValidation;
using GymAdmin.Applications.DTOs.MetodosPagoDto;

namespace GymAdmin.Applications.Validators;

public class UpdateMetodoPagoDTOValidator : AbstractValidator<MetodoPagoDto>
{
    public UpdateMetodoPagoDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del método de pago es obligatorio y debe ser mayor que cero.");
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del método de pago es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre del método de pago no puede exceder los 100 caracteres.");
     }
}
