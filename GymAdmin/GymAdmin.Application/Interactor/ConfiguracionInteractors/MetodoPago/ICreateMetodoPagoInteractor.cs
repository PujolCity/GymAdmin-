using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;

public interface ICreateMetodoPagoInteractor
{
    Task<Result<MetodoPagoDto>> ExecuteAsync(MetodoPagoCreateDTO metodoPagoCreateDTO, CancellationToken ct = default);
}
