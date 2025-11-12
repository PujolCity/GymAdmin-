using GymAdmin.Applications.DTOs.ConfiguracionDto;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors;

public interface IGetConfigurationInteractor
{
    Task<ConfiguracionDto> ExecuteAsync(CancellationToken ct = default);
}
