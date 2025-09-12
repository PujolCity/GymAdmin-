using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.Services;

public class PlanMembresiaService : IPlanMembresiaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlanMembresiaService> _logger;

    public PlanMembresiaService(IUnitOfWork unitOfWork,
        ILogger<PlanMembresiaService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> CreateAsync(PlanesMembresia plan, CancellationToken ct = default)
    {
        var exists = await _unitOfWork.MembresiaRepo.Query()
            .AsNoTracking()
            .AnyAsync(pm => pm.Nombre.ToLower() == plan.Nombre.ToLower(), ct);

        if (exists)
            return Result.Fail("Ya existe un plan de membresía con el mismo nombre.");

        try
        {
            await _unitOfWork.MembresiaRepo.AddAsync(plan, ct);
            await _unitOfWork.CommitAsync(ct);
            _logger.LogInformation("Plan de membresía creado: {Nombre} (Id={Id})", plan.Nombre, plan.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear plan de membresía {Nombre}", plan.Nombre);
            return Result.Fail("Ocurrió un error al crear el plan de membresía.");
        }
    }

    public async Task<Result> UpdateAsync(PlanesMembresia plan, CancellationToken ct = default)
    {
        var current = await _unitOfWork.MembresiaRepo
            .GetByIdAsync(plan.Id, ct);

        if (current is null)
            return Result.Fail("El plan de membresía no existe.");

        var nameTaken = await _unitOfWork.MembresiaRepo.Query()
            .AsNoTracking()
            .AnyAsync(pm => pm.Id != plan.Id && pm.Nombre.ToLower() == plan.Nombre.ToLower(), ct);

        if (nameTaken)
            return Result.Fail("Ya existe un plan de membresía con el mismo nombre.");

        try
        {
            current.Nombre = plan.Nombre;
            current.Descripcion = plan.Descripcion;
            current.Precio = plan.Precio;
            current.DiasValidez = plan.DiasValidez;
            current.IsActive = plan.IsActive;
            current.Creditos = plan.Creditos;

            _unitOfWork.MembresiaRepo.Update(current);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Plan de membresía actualizado: {Nombre} (Id={Id})", current.Nombre, current.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar plan de membresía {Id}", plan.Id);
            return Result.Fail("Ocurrió un error al actualizar el plan de membresía.");
        }
    }

    public async Task<PagedResult<PlanesMembresia>> GetAllAsync(PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default)
    {
        var q = _unitOfWork.MembresiaRepo.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Texto))
        {
            var texto = filter.Texto.Trim().ToLower();
            q = q.Where(pm => pm.Nombre.ToLower().Contains(texto));
        }

        if (filter.Status != StatusFilter.Todos)
        {
            q = filter.Status == StatusFilter.Activo ? q.Where(pm => pm.IsActive)
                                                     : q.Where(pm => !pm.IsActive);
        }

        var total = await q.CountAsync(ct);

        if (sorting.HasValue)
        {
            var s = sorting.Value;
            q = s.SortBy.ToLower() switch
            {
                "nombre" => s.Desc ? q.OrderByDescending(pm => pm.Nombre) : q.OrderBy(pm => pm.Nombre),
                "precio" => s.Desc ? q.OrderByDescending(pm => pm.Precio) : q.OrderBy(pm => pm.Precio),
                "diasvalidez" => s.Desc ? q.OrderByDescending(pm => pm.DiasValidez) : q.OrderBy(pm => pm.DiasValidez),
                _ => q.OrderBy(pm => pm.Nombre)
            };
        }
        else
        {
            q = q.OrderBy(pm => pm.Nombre);
        }

        var page = Math.Max(1, paging.PageNumber);
        var pageSize = Math.Clamp(paging.PageSize, 1, 50);

        var items = await q.Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync(ct);

        return new PagedResult<PlanesMembresia>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<Result> DeleteAsync(PlanesMembresia plan, CancellationToken ct = default)
    {
        var exists = await _unitOfWork.MembresiaRepo.Query()
                  .AsNoTracking()
                  .AnyAsync(pm => pm.Id == plan.Id, ct);
        if (!exists)
            return Result.Fail("El plan de membresía no existe.");

        try
        {
            await _unitOfWork.MembresiaRepo.SoftDeleteAsync(plan.Id, ct);
            await _unitOfWork.CommitAsync(ct);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar plan de membresía {Id}", plan.Id);
            return Result.Fail("Ocurrió un error al eliminar el plan de membresía.");
        }
    }
}
