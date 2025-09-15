using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public interface ICreatePagoInteractor
{
    Task<Result> ExecuteAsync(PagoCreateDto pagoCreateDto, CancellationToken ct = default);
}
