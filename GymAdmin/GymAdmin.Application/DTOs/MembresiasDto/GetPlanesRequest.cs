using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.DTOs.MembresiasDto;

public class GetPlanesRequest
{
    public string? Texto { get; set; }
    public StatusFilter Status { get; set; } = StatusFilter.Todos;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; init; } = "Nombre";
    public bool SortDesc { get; init; }
}