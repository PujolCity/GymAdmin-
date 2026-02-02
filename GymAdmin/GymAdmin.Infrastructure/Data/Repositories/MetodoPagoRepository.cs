using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Infrastructure.Data.Repositories;

public class MetodoPagoRepository : Repository<MetodoPago>, IMetodoPagoRepository
{
    public MetodoPagoRepository(GymAdminDbContext context) : base(context){}

    public async Task<MetodoPago?> GetNeighborUpAsync(int ordenActual, CancellationToken ct)
    {
        return await _context.MetodosPago
            .AsTracking()
            .Where(x => !x.IsDeleted && x.Orden < ordenActual)
            .OrderByDescending(x => x.Orden)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MetodoPago?> GetNeighborDownAsync(int ordenActual, CancellationToken ct)
    {
        return await _context.MetodosPago
            .AsTracking()
            .Where(x => !x.IsDeleted && x.Orden > ordenActual)
            .OrderBy(x => x.Orden)
            .FirstOrDefaultAsync(ct);
    }

    public async Task SwapOrdenAsync(int idA, int idB, CancellationToken ct)
    {
        var a = await _context.MetodosPago
        .AsTracking()
        .FirstAsync(x => x.Id == idA && !x.IsDeleted, ct);

        var b = await _context.MetodosPago
            .AsTracking()
            .FirstAsync(x => x.Id == idB && !x.IsDeleted, ct);

        var tmp = a.Orden;
        a.Orden = b.Orden;
        b.Orden = tmp;
    }

    public async Task<int> GetMaxOrden(CancellationToken ct)
    {
        return await _context.MetodosPago
       .AsNoTracking()
       .Where(mp => !mp.IsDeleted)
       .Select(mp => (int?)mp.Orden)
       .MaxAsync(ct)
       .ContinueWith(t => t.Result ?? 0, ct);
    }

    public Task CompactOrdenAfterDeleteAsync(int ordenEliminado, CancellationToken ct)
    {
        return _context.MetodosPago
            .Where(x => !x.IsDeleted && x.Orden > ordenEliminado)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Orden, x => x.Orden - 1), ct);
    }
}
