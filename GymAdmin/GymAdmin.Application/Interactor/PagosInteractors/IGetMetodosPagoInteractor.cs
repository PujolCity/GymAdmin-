using GymAdmin.Applications.DTOs.MetodosDePagoDto;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public interface IGetMetodosPagoInteractor
{
    Task<List<MetodoPagoDto>> ExecuteAsync(CancellationToken ct = default);
}
