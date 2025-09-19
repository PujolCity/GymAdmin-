using FluentValidation;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Applications.Extensions;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public class CreatePagoInteractor : ICreatePagoInteractor
{
    private readonly IPagosServices _pagosServices;
    private readonly IValidator<PagoCreateDto> _validator;

    public CreatePagoInteractor(IPagosServices pagosServices, 
        IValidator<PagoCreateDto> validator)
    {
        _pagosServices = pagosServices;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(PagoCreateDto pagoCreateDto, CancellationToken ct = default)
    {
        var validationResult = _validator.Validate(pagoCreateDto);
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.Errors.ToErrorMessages());
        
        var pago = pagoCreateDto.ToPagos();

        var result = await _pagosServices.CrearPagoAsync(pago, ct);
        
        if (result.IsSuccess)
            return Result.Ok();

        return Result.Fail(result.Errors);
    }
}
