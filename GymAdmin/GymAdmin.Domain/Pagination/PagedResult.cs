namespace GymAdmin.Domain.Pagination;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public readonly record struct Paging(int PageNumber, int PageSize);

public readonly record struct Sorting(string SortBy, bool Desc);