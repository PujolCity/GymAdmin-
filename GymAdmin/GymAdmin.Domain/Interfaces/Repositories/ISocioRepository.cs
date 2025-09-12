using GymAdmin.Domain.Entities;

namespace GymAdmin.Domain.Interfaces.Repositories;

public interface ISocioRepository : IRepository<Socio>
{
    Task<Socio?> GetSocioByDni(string dni);
}
