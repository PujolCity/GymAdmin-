using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Pagination;

public sealed class PaginationFilter
{
    public string? Texto { get; init; }    
    public StatusFilter Status { get; init; } = StatusFilter.Todos;
}