using GymAdmin.Applications.DTOs.PagosDto;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public interface IGetSociosLookupInteractor
{
    Task<List<SocioLookupDto>> ExecuteAsync(string texto, CancellationToken ct = default);
}
