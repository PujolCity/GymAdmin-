using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;

public class DeleteMetodoPagoInteractor : IDeleteMetodoPagoInteractor
{
    private readonly IMetodoPagoService _metodoPagoService;

    public DeleteMetodoPagoInteractor(IMetodoPagoService metodoPagoService)
    {
        _metodoPagoService = metodoPagoService;
    }

    public async Task<Result> ExecuteAsync(MetodoPagoDto metodoPagoDto, CancellationToken ct = default)
    {
        var metodoPago = metodoPagoDto.ToMetodoPago();

        var result = await _metodoPagoService.DeleteAsync(metodoPago, ct);  
        if(result.IsSuccess)
            return Result.Ok();

        return Result.Fail(result.Errors);
    }
}
