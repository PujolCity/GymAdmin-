using GymAdmin.Applications.DTOs;
using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Domain.Entities;

namespace GymAdmin.Applications.Mapppers;

public static class PlanesMembresiaMapper
{
    public static PlanesMembresia ToPlanMembresia(this PlanMembresiaDto planDto)
    {
        return new PlanesMembresia()
        {
            Id = planDto.Id,
            Nombre = planDto.Nombre,
            Descripcion = planDto.Descripcion,
            Precio = planDto.Precio,
            DiasValidez = planDto.DiasValidez,
            Creditos = planDto.Creditos,
            IsActive = planDto.IsActive
        };
    }

    public static PlanMembresiaDto ToPlanMembresiaDto(this PlanesMembresia plan)
    {
        return new PlanMembresiaDto()
        {
            Id = plan.Id,
            Nombre = plan.Nombre,
            Descripcion = plan.Descripcion,
            Precio = plan.Precio,
            DiasValidez = plan.DiasValidez,
            Creditos = plan.Creditos,
            IsActive = plan.IsActive
        };
    }

    public static PlanesMembresia ToPlanMembresia(this BaseDeleteRequest request)
    {
        return new PlanesMembresia()
        {
            Id = request.IdToDelete
        };
    }
}
