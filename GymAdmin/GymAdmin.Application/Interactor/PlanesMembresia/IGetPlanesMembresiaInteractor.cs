using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.PlanesMembresia;

public interface IGetPlanesMembresiaInteractor
{
    Task<PagedResult<PlanMembresiaDto>> ExecuteAsync(GetPlanesRequest getPlanesRequest, CancellationToken cancellationToken);
}
