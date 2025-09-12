using GymAdmin.Applications.DTOs.SociosDto;
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
            ExpiracionMembresia = s.ExpiracionMembresia,
            Estado = s.IsMembresiaExpirada ? "Inactivo" : "Activo",
            UltimaAsistencia = s.UltimaAsistencia,
            VigenciaTexto = s.VigenciaTexto 
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
