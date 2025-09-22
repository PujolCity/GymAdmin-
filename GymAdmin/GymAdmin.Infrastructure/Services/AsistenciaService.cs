using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.Services;

public class AsistenciaService : IAsistenciaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AsistenciaService> _logger;

    public AsistenciaService(IUnitOfWork unitOfWork, ILogger<AsistenciaService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> DeleteAsync(Asistencia asistencia, CancellationToken ct = default)
    {
        var asist = await _unitOfWork.AsistenciaRepo.Query()
         .Include(a => a.Socio)
         .FirstOrDefaultAsync(a => a.Id == asistencia.Id, ct);

        if (asist is null)
            return Result.Fail("No existe la asistencia especificada.");

        if (asist.IsDeleted)
            return Result.Ok();

        try
        {
            if (asist.SeUsoCredito && asist.Socio is not null)
            {
                asist.Socio.ReintegrarCreditoPorEliminacion(asist);
                _unitOfWork.SocioRepo.Update(asist.Socio);
            }

            await _unitOfWork.AsistenciaRepo.SoftDeleteAsync(asist.Id, ct);
            await _unitOfWork.CommitAsync(ct);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la asistencia con Id {AsistenciaId}", asistencia.Id);
            return Result.Fail("Ocurrió un error al eliminar la asistencia.");
        }
    }

    public async Task<PagedResult<Asistencia>> GetAsistenciasBySocioAsync(AsistenciaFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default)
    {
        var query = _unitOfWork.AsistenciaRepo.Query()
            .Where(a => a.SocioId == filter.SocioId)
            .AsNoTracking();

        var totalItems = query.CountAsync(ct);

        if (sorting.HasValue)
        {
            var sort = sorting.Value;
            query = (sort.SortBy, sort.Desc) switch
            {
                ("Entrada", false) => query.OrderBy(e => e.Entrada),
                ("Entrada", true) => query.OrderByDescending(e => e.Entrada),

                _ => query.OrderByDescending(e => e.Entrada)
            };
        }
        else
        {
            query = query.OrderByDescending(e => e.Entrada);
        }

        var page = Math.Max(1, paging.PageNumber);
        var size = Math.Clamp(paging.PageSize, 1, 500);

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var asistencias = new List<Asistencia>(items.Count);
        foreach (var asistencia in items)
        {
            asistencias.Add(asistencia);
        }

        return new PagedResult<Asistencia>
        {
            Items = asistencias,
            TotalCount = await totalItems,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<Result> RegistrarAsync(Asistencia asistencia, CancellationToken ct = default)
    {
        var socio = await _unitOfWork.SocioRepo.GetByIdAsync(asistencia.SocioId, ct);
        if (socio is null)
            return Result.Fail("No existe un socio con el Id.");

        var seUsoCredito = socio.UsarCredito();
        if (!seUsoCredito)
            return Result.Fail("El socio no tiene créditos disponibles.");

        asistencia.SeUsoCredito = seUsoCredito;
        asistencia.Socio = socio;
        try
        {
            await _unitOfWork.AsistenciaRepo.AddAsync(asistencia, ct);
            _unitOfWork.SocioRepo.Update(socio);
            await _unitOfWork.CommitAsync(ct);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar asistencia para socio {SocioId}", asistencia.SocioId);
            return Result.Fail("Ocurrió un error al registrar la asistencia.");
        }
    }

    public async Task<Result> UpdateAsync(Asistencia asistencia, CancellationToken ct = default)
    {
        var asist = await _unitOfWork.AsistenciaRepo.GetByIdAsync(asistencia.Id, ct);

        if (asist is null)
            return Result.Fail("No existe la asistencia especificada.");

        try
        {
            asist.Entrada = asistencia.Entrada;
            asist.Observaciones = asistencia.Observaciones;
            _unitOfWork.AsistenciaRepo.Update(asist);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Asistencia actualizada: Id={Id}", asist.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la asistencia con Id {AsistenciaId}", asistencia.Id);
            return Result.Fail("Ocurrió un error al actualizar la asistencia.");
        }
    }
}
