using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.PlanesMembresia;

public class GetPlanesMembresiaInteractor : IGetPlanesMembresiaInteractor
{
    private readonly IPlanMembresiaService _planMembresiaService;

    public GetPlanesMembresiaInteractor(IPlanMembresiaService planMembresiaService)
    {
        _planMembresiaService = planMembresiaService;
    }

    public async Task<PagedResult<PlanMembresiaDto>> ExecuteAsync(GetPlanesRequest getPlanesRequest, CancellationToken cancellationToken)
    {
        var filter = new PaginationFilter
        {
            Texto = getPlanesRequest.Texto,
            Status = getPlanesRequest.Status
        };

        var paging = new Paging(getPlanesRequest.PageNumber, getPlanesRequest.PageSize);

        Sorting? sorting = null;
        if (!string.IsNullOrWhiteSpace(getPlanesRequest.SortBy))
        {
            sorting = new Sorting(getPlanesRequest.SortBy, getPlanesRequest.SortDesc);
        }

        var result = await _planMembresiaService.GetAllAsync(filter, paging, sorting, cancellationToken);

        var items = result.Items.Select(pm => new PlanMembresiaDto
        {
            Id = pm.Id,
            Nombre = pm.Nombre,
            Descripcion = pm.Descripcion,
            Precio = pm.Precio,
            DiasValidez = pm.DiasValidez,
            IsActive = pm.IsActive,
            Estado = pm.IsActive? "Activo" : "Inactivo",
            Creditos = pm.Creditos
        }).ToList();

        return new PagedResult<PlanMembresiaDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }
}
