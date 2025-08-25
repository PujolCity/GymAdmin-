using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GymAdminDbContext _context;

    public IRepository<Pago> PagosRepo { get; }
    public IRepository<PlanMembresia> MembresiaRepo { get; } 
    public IRepository<Miembro> MiembroRepo { get; }
    public IRepository<User> UserRepo { get; }
    public IRepository<Asistencia> AsistenciaRepo { get; }
    public IRepository<SystemConfig> SystemConfigRepo { get; }

    public UnitOfWork(GymAdminDbContext context,
        IRepository<SystemConfig> systemConfigRepo,
        IRepository<Asistencia> asistenciaRepo,
        IRepository<User> userRepo,
        IRepository<Miembro> miembroRepo,
        IRepository<PlanMembresia> membresiaRepo,
        IRepository<Pago> pagosRepo)
    {
        _context = context;
        SystemConfigRepo = systemConfigRepo;
        AsistenciaRepo = asistenciaRepo;
        UserRepo = userRepo;
        MiembroRepo = miembroRepo;
        MembresiaRepo = membresiaRepo;
        PagosRepo = pagosRepo;
    }

    public async Task<int> CommitAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Rollback();
            throw new Exception("Error al guardar cambios", ex);
        }
    }

    public void Rollback()
    {
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
