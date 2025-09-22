using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public interface IGetSocioByIdInteractor
{
    Task<Result<SocioDto>> ExecuteAsync(int socioId, CancellationToken ct = default);
}
