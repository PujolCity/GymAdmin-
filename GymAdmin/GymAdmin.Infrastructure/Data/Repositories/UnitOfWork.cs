using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GymAdminDbContext _context;

    public IRepository<Pagos> PagosRepo { get; }
    public IRepository<PlanesMembresia> MembresiaRepo { get; } 
    public ISocioRepository SocioRepo { get; }
    public IRepository<User> UserRepo { get; }
    public IRepository<Asistencia> AsistenciaRepo { get; }
    public IRepository<SystemConfig> SystemConfigRepo { get; }

    public UnitOfWork(GymAdminDbContext context,
        IRepository<SystemConfig> systemConfigRepo,
        IRepository<Asistencia> asistenciaRepo,
        IRepository<User> userRepo,
        ISocioRepository socioRepo,
        IRepository<PlanesMembresia> membresiaRepo,
        IRepository<Pagos> pagosRepo)
    {
        _context = context;
        SystemConfigRepo = systemConfigRepo;
        AsistenciaRepo = asistenciaRepo;
        UserRepo = userRepo;
        SocioRepo = socioRepo;
        MembresiaRepo = membresiaRepo;
        PagosRepo = pagosRepo;
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.SaveChangesAsync(ct);
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
