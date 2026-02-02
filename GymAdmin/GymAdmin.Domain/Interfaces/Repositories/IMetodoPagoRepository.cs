using GymAdmin.Domain.Entities;

namespace GymAdmin.Domain.Interfaces.Repositories;

public interface IMetodoPagoRepository : IRepository<MetodoPago>
{
    Task<MetodoPago?> GetByIdAsync(int id, CancellationToken ct);
    Task<MetodoPago?> GetNeighborUpAsync(int ordenActual, CancellationToken ct);
    Task<MetodoPago?> GetNeighborDownAsync(int ordenActual, CancellationToken ct);
    Task SwapOrdenAsync(int idA, int idB, CancellationToken ct);
    Task<int> GetMaxOrden(CancellationToken ct);
    Task CompactOrdenAfterDeleteAsync(int ordenEliminado, CancellationToken ct);
}
