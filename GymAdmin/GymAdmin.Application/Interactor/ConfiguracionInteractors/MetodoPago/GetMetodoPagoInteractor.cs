using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;

public class GetMetodoPagoInteractor : IGetMetodosPagoInteractor
{
    private readonly IMetodoPagoService _metodoPagoService;

    public GetMetodoPagoInteractor(IMetodoPagoService metodoPagoService)
    {
        _metodoPagoService = metodoPagoService;
    }

    public async Task<PagedResult<MetodoPagoDto>> ExecuteAsync(GetMetodoPagoRequest request, CancellationToken ct = default)
    {
        var filter = new PaginationFilter
        {
            Texto = request.Texto,
            Status = request.Status
        };

        var paging = new Paging(request.PageNumber, request.PageSize);

        Sorting? sorting = null;
        if (!string.IsNullOrWhiteSpace(request.SortBy))
            sorting = new Sorting(request.SortBy, request.SortDesc);

        var result = await _metodoPagoService.GetAllAsync(filter, paging, sorting, ct);

        var items = result.Items.Select(mp => new MetodoPagoDto
        {
            Id = mp.Id,
            Nombre = mp.Nombre,
            IsActive = mp.IsActive,
            Estado = mp.IsActive ? "Activo" : "Inactivo",
            TipoAjuste = mp.TipoAjuste,
            ValorAjuste = mp.ValorAjuste,
            Orden = mp.Orden
        }).ToList();

        return new PagedResult<MetodoPagoDto> 
        {
            Items = items, 
            TotalCount = result.TotalCount, 
            PageNumber = result.PageNumber, 
            PageSize = result.PageSize 
        };
    }
}
