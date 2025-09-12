using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;

namespace GymAdmin.Domain.Interfaces.Services;

public interface IPlanMembresiaService
{
    Task<Result> CreateAsync(PlanesMembresia plan, CancellationToken ct = default);
    Task<Result> UpdateAsync(PlanesMembresia plan, CancellationToken ct = default);
    Task<Result> DeleteAsync(PlanesMembresia plan, CancellationToken ct = default);
    Task<PagedResult<PlanesMembresia>> GetAllAsync(PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default);
}
