using GymAdmin.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GymAdmin.Infrastructure.Data.Repositories;

internal sealed class EfCoreTransaction : ITransaction
{
    private readonly IDbContextTransaction _inner;
    public EfCoreTransaction(IDbContextTransaction inner) => _inner = inner;
    public Task CommitAsync(CancellationToken ct = default) => _inner.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => _inner.RollbackAsync(ct);
    public ValueTask DisposeAsync() => _inner.DisposeAsync();
}