using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.DTOs.SociosDto;

public sealed class GetSociosRequest
{
    public string? Texto { get; init; }
    public StatusFilter Status { get; init; } = StatusFilter.Todos;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string SortBy { get; init; }
    public bool SortDesc { get; init; }
}