using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.PlanesMembresia;

public interface IDeletePlanMembresiaInteractor
{
    Task<Result> ExecuteAsync(PlanMembresiaDto request, CancellationToken ct = default);
}
