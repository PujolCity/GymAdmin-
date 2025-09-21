using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;

namespace GymAdmin.Applications.Interactor.AsistenciaInteractors;

public class GetAsistenciasBySocioInteractor : IGetAsistenciasBySocioInteractor
{
    private readonly IAsistenciaService _asistenciaService;

    public GetAsistenciasBySocioInteractor(IAsistenciaService asistenciaService)
    {
        _asistenciaService = asistenciaService;
    }

    public async Task<PagedResult<AsistenciaDto>> ExecuteAsync(GetAsistenciasBySocioRequest request, CancellationToken ct = default)
    {
        var filter = new AsistenciaFilter
        {
            Fecha = request.Fecha,
            SocioId = request.SocioId
        };

        var paging = new Paging(request.PageNumber, request.PageSize);
        Sorting? sorting = null;
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            sorting = new Sorting(request.SortBy, request.SortDesc);
        }

        var result = await _asistenciaService.GetAsistenciasBySocioAsync(filter, paging, sorting, ct);

        var items = result.Items.Select(a => new AsistenciaDto
        {
            Id = a.Id,
            SocioId = a.SocioId,
            Entrada = a.Entrada,
            SeUsoCredito = a.SeUsoCredito,
        }).ToList();

        return new PagedResult<AsistenciaDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
