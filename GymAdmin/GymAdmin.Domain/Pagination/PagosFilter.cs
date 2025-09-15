using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Pagination;

public sealed class PagosFilter
{
    public string? Texto { get; init; }
    public StatusPagosFilter Status { get; init; } = StatusPagosFilter.Todos;
    public DateTime? FechaDesde { get; init; }
    public DateTime? FechaHasta { get; init; }
}
