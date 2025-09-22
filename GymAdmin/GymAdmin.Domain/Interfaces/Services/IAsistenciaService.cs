using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;

namespace GymAdmin.Domain.Interfaces.Services;

public interface IAsistenciaService
{
    Task<Result> RegistrarAsync(Asistencia asistencia, CancellationToken ct = default);
    Task<Result> DeleteAsync(Asistencia asistencia, CancellationToken ct = default);
    Task<Result> UpdateAsync(Asistencia asistencia, CancellationToken ct = default);
    Task<PagedResult<Asistencia>> GetAsistenciasBySocioAsync(AsistenciaFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default);
}
