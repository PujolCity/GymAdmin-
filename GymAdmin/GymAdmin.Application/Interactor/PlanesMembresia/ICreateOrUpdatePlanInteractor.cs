using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.PlanesMembresia;

public interface ICreateOrUpdatePlanInteractor
{
    Task<Result> ExecuteAsync(PlanMembresiaDto planDto, CancellationToken ct = default);
}
