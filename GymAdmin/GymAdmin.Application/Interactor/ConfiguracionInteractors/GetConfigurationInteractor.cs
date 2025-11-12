using GymAdmin.Applications.DTOs.ConfiguracionDto;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors;

public class GetConfigurationInteractor : IGetConfigurationInteractor
{
    public Task<ConfiguracionDto> ExecuteAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
