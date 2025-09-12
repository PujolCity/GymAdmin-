using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Infrastructure.Data.Repositories;

public class SocioRepository : Repository<Socio>, ISocioRepository
{
    public SocioRepository(GymAdminDbContext context) : base(context){}

    public async Task<Socio?> GetSocioByDni(string dni)
    {
        var dniHash = _context._cryptoService.ComputeHash(dni);
        return await _context.Socios.FirstOrDefaultAsync(s => s.DniHash == dniHash);
    }
}
