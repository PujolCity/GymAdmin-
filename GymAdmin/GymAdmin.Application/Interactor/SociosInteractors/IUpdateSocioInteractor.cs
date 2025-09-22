using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public interface IUpdateSocioInteractor
{
    Task<Result> ExecuteAsync(SocioUpdateDto socioDto, CancellationToken cancellation = default);
}
