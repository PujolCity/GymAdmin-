using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.SociosInteractors;

public class GetAllSociosInteractor : IGetAllSociosInteractor
{
    private readonly ISocioService _socioService;

    public GetAllSociosInteractor(ISocioService socioService)
    {
        _socioService = socioService;
    }

    public async Task<PagedResult<SocioDto>> ExecuteAsync(GetSociosRequest request, CancellationToken ct = default)
    {
        var filter = new PaginationFilter
        {
            Texto = request.Texto,
            Status = request.Status
        };

        var paging = new Paging(request.PageNumber, request.PageSize);

        Sorting? sorting = null;
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            sorting = new Sorting(request.SortBy, request.SortDesc);
        }

        var result = await _socioService.GetAllAsync(filter, paging, sorting, ct);

        var items = result.Items.Select(s => new SocioDto
        {
            Id = s.Id,
            Dni = s.Dni,
            Nombre = s.Nombre,
            Apellido = s.Apellido,
            Telefono = !String.IsNullOrEmpty(s.TelefonoDecrypted) ? s.TelefonoDecrypted : "-",
            ExpiracionMembresia = s.ExpiracionMembresia,
            Estado = s.IsActive? "Activo" : "Inactivo",  
            UltimaAsistencia = s.UltimaAsistencia.HasValue
            ? DateTime.SpecifyKind(s.UltimaAsistencia.Value, DateTimeKind.Utc)
            .ToLocalTime() : null,
            FechaRegistro = DateTime.SpecifyKind(s.FechaRegistro, DateTimeKind.Utc)
            .ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
            VigenciaTexto = s.VigenciaTexto,
            CreditosRestantes = s.CreditosRestantes,
            TotalCreditosComprados = s.TotalCreditosComprados,
            UltimoPagoTexto = s.UltimoPago.HasValue
            ? DateTime.SpecifyKind(s.UltimoPago.Value, DateTimeKind.Utc)
            .ToLocalTime()
            .ToString("dd/MM/yyyy HH:mm") : "—",
            PlanNombre = s.PlanNombre
        }).ToList();

        return new PagedResult<SocioDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }
}
