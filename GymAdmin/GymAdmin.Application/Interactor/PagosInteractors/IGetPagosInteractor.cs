using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public interface IGetPagosInteractor
{ 
    Task<PagedResult<PagoDto>> ExecuteAsync(GetPagosRequest request, CancellationToken cancellationToken = default);
}
