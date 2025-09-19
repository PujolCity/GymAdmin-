using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.AsistenciaInteractors;

public interface ICreateAsistenciaInteractor
{
    Task<Result> ExecuteAsync(CreateAsistenciaDto asistenciaDto, CancellationToken ct);
}
