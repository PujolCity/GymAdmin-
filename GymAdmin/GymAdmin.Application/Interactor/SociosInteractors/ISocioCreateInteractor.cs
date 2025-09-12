using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public interface ISocioCreateInteractor
{
    Task<Result> ExecuteAsync(SocioCreateDto socioCreateDto);
}
