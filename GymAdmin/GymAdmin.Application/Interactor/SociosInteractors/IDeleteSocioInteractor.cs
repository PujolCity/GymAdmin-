using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public interface IDeleteSocioInteractor
{
    public Task<Result> ExecuteAsync(SocioDto socioDto, CancellationToken ct = default);
}
