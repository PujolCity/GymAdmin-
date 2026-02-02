using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;

namespace GymAdmin.Domain.Interfaces.Services;

public interface IMetodoPagoService
{
    Task<PagedResult<MetodoPago>> GetAllAsync(PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default);
    Task<Result<MetodoPago>> CreateAsync(MetodoPago metodoPago, CancellationToken ct);
    Task<Result> UpdateAsync(MetodoPago metodoPago, CancellationToken ct);
    Task<Result> DeleteAsync(MetodoPago metodoPago, CancellationToken ct);
    Task<Result> MoveUpAsync(int metodoPagoId, CancellationToken ct);
    Task<Result> MoveDownAsync(int metodoPagoId, CancellationToken ct);
}
