using GymAdmin.Domain.Entities;

namespace GymAdmin.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Pagos> PagosRepo { get; }
    IRepository<PlanesMembresia> MembresiaRepo { get; }
    ISocioRepository SocioRepo { get; }
    IRepository<User> UserRepo { get; }
    IRepository<Asistencia> AsistenciaRepo { get; }
    IRepository<SystemConfig> SystemConfigRepo { get; }
    IMetodoPagoRepository MetodoPagoRepo { get; }

    Task<int> CommitAsync(CancellationToken ct = default);
    void Rollback();

    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
}
