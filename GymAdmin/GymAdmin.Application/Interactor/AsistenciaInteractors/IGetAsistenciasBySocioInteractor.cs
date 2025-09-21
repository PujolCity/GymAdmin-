using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.AsistenciaInteractors;

public interface IGetAsistenciasBySocioInteractor
{
    Task<PagedResult<AsistenciaDto>> ExecuteAsync(GetAsistenciasBySocioRequest request, CancellationToken ct = default);
}
