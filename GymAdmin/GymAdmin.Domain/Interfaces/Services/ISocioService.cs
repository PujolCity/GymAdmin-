using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;

namespace GymAdmin.Domain.Interfaces.Services;

public interface ISocioService
{
    Task<Result> CreateAsync(Socio socio, CancellationToken ct = default);
    Task<Result> DeleteAsync(Socio socio, CancellationToken ct = default);
    Task<Result> UpdateAsync(Socio socio, CancellationToken ct = default);
    Task<PagedResult<Socio>> GetAllAsync(PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default);
    Task<List<Socio>> GetAllForLookupAsync(CancellationToken ct = default);
    Task<Result<Socio>> GetSocioByIdAsync(int socioId, CancellationToken ct = default);
}
