using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.DTOs.PagosDto;

public class GetPagosRequest
{
    public string? Texto { get; set; }
    public StatusPagosFilter Status { get; set; } = StatusPagosFilter.Todos;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; init; } = "FechaPago";
    public bool SortDesc { get; init; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}
