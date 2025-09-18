using FluentValidation;
using GymAdmin.Applications.DTOs;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Applications.Extensions;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public class AnularPagoInteractor : IAnularPagoInteractor
{
    private readonly IPagosServices _pagoService;
    private readonly IValidator<BaseDeleteRequest> _validator;
    
    public AnularPagoInteractor(IPagosServices pagoService,
        IValidator<BaseDeleteRequest> validator)
    {
        _pagoService = pagoService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(PagoDto pagoDto, CancellationToken ct = default)
    {
        var requestWrapper = new BaseDeleteRequest { IdToDelete = pagoDto.Id };
        var validationResult = await _validator.ValidateAsync(requestWrapper, ct);

        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.ToErrorMessages());
        }

        var pagoToAnular = requestWrapper.ToPagos();

        var result = await _pagoService.AnularPagoAsync(pagoToAnular, ct);
        if (result.IsSuccess)
            return Result.Ok();
        return Result.Fail(result.Errors);
    }
}
