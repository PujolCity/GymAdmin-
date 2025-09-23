using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public class GetSocioByIdInteractor : IGetSocioByIdInteractor
{
    private readonly ISocioService _socioService;

    public GetSocioByIdInteractor(ISocioService socioService)
    {
        _socioService = socioService;
    }

    public async Task<Result<SocioDto>> ExecuteAsync(int socioId, CancellationToken ct = default)
    {
        var result = await _socioService.GetSocioByIdAsync(socioId, ct);
        if (result.IsFailed)
            return Result<SocioDto>.Fail(result.Errors);
        var socioDto = new SocioDto
        {
            Id = result.Value.Id,
            Apellido = result.Value.Apellido,
            Nombre = result.Value.Nombre,
            Dni = result.Value.Dni,
            CreditosRestantes = result.Value.CreditosRestantes,
            ExpiracionMembresia = result.Value.ExpiracionMembresia,
            UltimaAsistencia = result.Value.UltimaAsistencia,
            UltimoPagoTexto = result.Value.UltimoPago.HasValue
            ? DateTime.SpecifyKind(result.Value.UltimoPago.Value, DateTimeKind.Utc)
            .ToLocalTime()
            .ToString("dd/MM/yyyy HH:mm") : "—",
            TotalCreditosComprados = result.Value.TotalCreditosComprados,
            VigenciaTexto = result.Value.VigenciaTexto,
            FechaRegistro = result.Value.FechaRegistro.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
            Estado = result.Value.IsMembresiaExpirada ? "Inactivo" : "Activo",
            PlanNombre = result.Value.PlanNombre
        };
        if (!socioDto.UltimoPagoTexto.Equals("-"))
            socioDto.UltimoPagoTexto = string.Concat(
                socioDto.UltimoPagoTexto,
                " - Plan: ",
                socioDto.PlanNombre
            );

        return Result<SocioDto>.Ok(socioDto);
    }
}
