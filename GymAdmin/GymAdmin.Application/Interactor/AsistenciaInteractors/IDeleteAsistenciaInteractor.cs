using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.AsistenciaInteractors;

public interface IDeleteAsistenciaInteractor
{
    Task<Result> ExecuteAsync(DeleteAsistenciaDto request, CancellationToken ct = default);
}
