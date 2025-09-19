using GymAdmin.Applications.DTOs.MetodosDePagoDto;
using GymAdmin.Applications.Mapppers;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Services;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public class GetMetodosPagoInteractor : IGetMetodosPagoInteractor
{
    private readonly IPagosServices _pagosServices;

    public GetMetodosPagoInteractor(IPagosServices pagosServices)
    {
        _pagosServices = pagosServices;
    }

    public async Task<List<MetodoPagoDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var result = await _pagosServices.GetMetodosPagoAsync(true, ct);
        var metodosPago = result.IsSuccess
            ? result.Value.Select(m => m.ToMetodoPagoDto()).ToList()
            : [];

        return metodosPago;
    }
}
