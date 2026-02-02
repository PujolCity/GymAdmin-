
using FluentValidation;
using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Applications.Extensions;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;

public class CreateMetodoPagoInteractor : ICreateMetodoPagoInteractor
{
    private readonly IMetodoPagoService _metodoPagoService;
    private readonly IValidator<MetodoPagoCreateDTO> _validator;

    public CreateMetodoPagoInteractor(IMetodoPagoService metodoPagoService,
        IValidator<MetodoPagoCreateDTO> validator)
    {
        _metodoPagoService = metodoPagoService;
        _validator = validator;
    }

    public async Task<Result<MetodoPagoDto>> ExecuteAsync(MetodoPagoCreateDTO metodoPagoCreateDTO, CancellationToken ct = default)
    {
        var validationResult = _validator.Validate(metodoPagoCreateDTO);
        if (!validationResult.IsValid)
            return Result<MetodoPagoDto>.Fail(validationResult.Errors.ToErrorMessages());

        var metodoPago = metodoPagoCreateDTO.ToMetodoPago();

        var result = await _metodoPagoService.CreateAsync(metodoPago, ct);

        if (result.IsSuccess)
            return Result<MetodoPagoDto>.Ok(result.Value.ToMetodoPagoDto());

        return Result<MetodoPagoDto>.Fail(result.Errors);
    }
}
