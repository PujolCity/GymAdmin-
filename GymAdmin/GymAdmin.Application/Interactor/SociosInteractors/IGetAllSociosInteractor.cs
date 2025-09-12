using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public interface IGetAllSociosInteractor
{
    Task<PagedResult<SocioDto>> ExecuteAsync(GetSociosRequest request, CancellationToken ct = default);
}
