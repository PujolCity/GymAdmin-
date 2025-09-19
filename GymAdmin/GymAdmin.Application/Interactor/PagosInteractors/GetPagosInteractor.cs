using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.PagosInteractors;

public class GetPagosInteractor : IGetPagosInteractor
{
    private  readonly IPagosServices _pagosServices;

    public GetPagosInteractor(IPagosServices pagosServices)
    {
        _pagosServices = pagosServices;
    }

    public async Task<PagedResult<PagoDto>> ExecuteAsync(GetPagosRequest request, CancellationToken cancellationToken = default)
    {
        DateTime? hastaExclusivo = request.FechaHasta?.Date.AddDays(1);

        var filter = new PagosFilter
        {
            Texto = request.Texto,
            Status = (StatusPagosFilter)request.Status,    // si el enum de app == domain, es cast 1:1
            FechaDesde = request.FechaDesde?.Date,
            FechaHasta = hastaExclusivo
        };

        var paging = new Paging(request.PageNumber, request.PageSize);
        var sorting = new Sorting(request.SortBy ?? "FechaPago", request.SortDesc);

        var result = await _pagosServices.GetAllAsync(filter, paging, sorting, cancellationToken);
        var items = result.Items.Select(p => new PagoDto
        {
            Id = p.Id,
            SocioId = p.SocioId,
            SocioNombre = $"{p.Socio.Nombre} {p.Socio.Apellido}",
            PlanMembresiaId = p.PlanMembresiaId,
            PlanNombre = p.PlanMembresia.Nombre,
            MetodoPagoId = p.MetodoPagoId,
            MetodoPago= p.MetodoPagoRef.Nombre,
            Precio = p.Precio,
            FechaPago = p.FechaPago,
            Estado = p.Estado.ToString()
        }).ToList();

        return new PagedResult<PagoDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }
}
