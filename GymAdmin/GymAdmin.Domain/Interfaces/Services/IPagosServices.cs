using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;

namespace GymAdmin.Domain.Interfaces.Services;

public interface IPagosServices
{
    Task<Result<List<MetodoPago>>> GetMetodosPagoAsync(bool isActive = true, CancellationToken ct = default);
    Task<Result> CrearPagoAsync(Pagos pago, CancellationToken ct = default);
    Task<Result> AnularPagoAsync(int pagoId, CancellationToken ct = default);
    Task<PagedResult<Pagos>> GetAllAsync(PagosFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default);

}
