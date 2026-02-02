using FluentValidation;
using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Applications.Extensions;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;

public class MoveDownInteractor : IMoveDownInteractor
{
    private readonly IMetodoPagoService _metodoPagoService;
    private readonly IValidator<MetodoPagoDto> _validator;

    public MoveDownInteractor(IMetodoPagoService metodoPagoService,
        IValidator<MetodoPagoDto> validator)
    {
        _metodoPagoService = metodoPagoService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(MetodoPagoDto metodoPagoDto, CancellationToken ct)
    {
        var validationResult = _validator.Validate(metodoPagoDto);
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.Errors.ToErrorMessages());
        var metodoPago = metodoPagoDto.ToMetodoPago();

        var result = await _metodoPagoService.MoveDownAsync(metodoPago.Id, ct);
        if (result.IsSuccess)
            return Result.Ok();

        return Result.Fail(result.Errors);
    }
}
