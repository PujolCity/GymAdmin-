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
    IRepository<MetodoPago> MetodoPagoRepo { get; }

    Task<int> CommitAsync(CancellationToken ct = default);
    void Rollback();
}
