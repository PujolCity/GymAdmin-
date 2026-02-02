using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;

public interface IMoveDownInteractor
{
    Task<Result> ExecuteAsync(MetodoPagoDto metodoPagoDto, CancellationToken ct);
}
