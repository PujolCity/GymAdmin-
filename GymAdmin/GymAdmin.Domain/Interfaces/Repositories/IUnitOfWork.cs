using GymAdmin.Domain.Entities;

namespace GymAdmin.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Pago> PagosRepo { get; }
    IRepository<PlanMembresia> MembresiaRepo { get; }
    IRepository<Miembro> MiembroRepo { get; }
    IRepository<User> UserRepo { get; }
    IRepository<Asistencia> AsistenciaRepo { get; }
    IRepository<SystemConfig> SystemConfigRepo { get; }

    Task<int> CommitAsync();
    void Rollback();
}
