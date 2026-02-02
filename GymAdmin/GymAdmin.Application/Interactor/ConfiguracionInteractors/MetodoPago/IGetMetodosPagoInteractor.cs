using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;

public interface IGetMetodosPagoInteractor
{
    Task<PagedResult<MetodoPagoDto>> ExecuteAsync(GetMetodoPagoRequest request, CancellationToken ct = default);
}
