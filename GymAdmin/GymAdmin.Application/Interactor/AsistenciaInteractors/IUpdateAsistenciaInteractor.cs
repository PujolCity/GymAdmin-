using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.AsistenciaInteractors;

public interface IUpdateAsistenciaInteractor
{
    Task<Result> ExecuteAsync(AsistenciaDto asistenciaDto, CancellationToken ct = default);
}
