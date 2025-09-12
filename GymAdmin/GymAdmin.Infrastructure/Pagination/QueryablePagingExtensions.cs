using GymAdmin.Domain.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GymAdmin.Infrastructure.Pagination;

public static class QueryablePagingExtensions
{
    public static async Task<PagedResult<T>> PaginateAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 500);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = size
        };
    }

    public static async Task<PagedResult<TOut>> PaginateSelectAsync<TIn, TOut>(
        this IQueryable<TIn> query,
        int pageNumber,
        int pageSize,
        Expression<Func<TIn, TOut>> selector,
        CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 500);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(selector) // proyección a DTO/anon type
            .ToListAsync(ct);

        return new PagedResult<TOut>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = size
        };
    }
}