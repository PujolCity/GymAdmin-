using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;

namespace GymAdmin.Domain.Interfaces.Services;

public interface IMetodoPagoService
{
    Task<PagedResult<MetodoPago>> GetAllAsync(PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default);
    Task<Result<MetodoPago?>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result> AddAsync(MetodoPago metodoPago, CancellationToken ct);
    Task<Result> UpdateAsync(MetodoPago metodoPago, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);
}
