using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public class UpdateSocioInteractor : IUpdateSocioInteractor
{
    public Task<Result> ExecuteAsync(SocioDto socioDto, CancellationToken cancellation = default)
    {

        throw new NotImplementedException();
    }
}
