using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public interface IAnularPagoInteractor
{
    Task<Result> ExecuteAsync(PagoDto pagoDto, CancellationToken ct = default);
}
