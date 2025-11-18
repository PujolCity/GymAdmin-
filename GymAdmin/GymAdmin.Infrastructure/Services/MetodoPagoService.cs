using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.Services;

public class MetodoPagoService(IUnitOfWork unitOfWork,
    ILogger<MetodoPagoService> logger) : IMetodoPagoService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<MetodoPagoService> _logger = logger;

    public async Task<Result> AddAsync(MetodoPago metodoPago, CancellationToken ct)
    {
        var exist = _unitOfWork.MetodoPagoRepo.Query()
            .AsNoTracking()
            .Any(mp => mp.Nombre == metodoPago.Nombre);
        if (exist)
            return Result.Fail("El nombre del metodo de pago ya está en uso");

        try
        {
            await _unitOfWork.MetodoPagoRepo.AddAsync(metodoPago, ct);
            await _unitOfWork.CommitAsync(ct);
            _logger.LogInformation("Metodo de Pago {MetodoPagoId} agregado exitosamente.", metodoPago.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar el Metodo de Pago {nombre}.", metodoPago.Nombre);
            return Result.Fail("Error al agregar el Metodo de Pago.");
        }
    }

    public Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<MetodoPago>> GetAllAsync(PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default)
    {
        var query = _unitOfWork.MetodoPagoRepo.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Texto))
        {
            query = query.Where(mp => mp.Nombre.Contains(filter.Texto));
        }

        if (filter.Status == StatusFilter.Activo)
        {
            query = filter.Status == StatusFilter.Activo ? query.Where(pm => pm.IsActive)
                                                     : query.Where(pm => !pm.IsActive);
        }

        var total = await query.CountAsync(ct);

        if (sorting.HasValue)
        {
            var s = sorting.Value;
            query = s.SortBy.ToLower() switch
            {
                "nombre" => s.Desc ? query.OrderByDescending(pm => pm.Nombre) : query.OrderBy(pm => pm.Nombre),
                _ => query.OrderBy(pm => pm.Nombre)
            };
        }
        else
        {
            query = query.OrderBy(pm => pm.Nombre);
        }

        var page = Math.Max(1, paging.PageNumber);
        var pageSize = Math.Clamp(paging.PageSize, 1, 50);

        var items = await query.Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync(ct);

        return new PagedResult<MetodoPago>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public Task<Result<MetodoPago?>> GetByIdAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> UpdateAsync(MetodoPago metodoPago, CancellationToken ct)
    {
        var current = await _unitOfWork.MetodoPagoRepo.GetByIdAsync(metodoPago.Id, ct);
        if (current is null)
            return Result.Fail("El metodo de Pago no existe");

        var nameTaken = await _unitOfWork.MetodoPagoRepo.Query()
            .AsNoTracking()
            .AnyAsync(mp => mp.Id != metodoPago.Id && mp.Nombre == metodoPago.Nombre, ct);
        if (nameTaken)
            return Result.Fail("El nombre del metodo de pago ya está en uso");

        try
        {
            current.Nombre = metodoPago.Nombre;
            current.IsActive = metodoPago.IsActive;
            current.TipoAjuste = metodoPago.TipoAjuste;
            current.ValorAjuste = metodoPago.ValorAjuste;
            current.Orden = metodoPago.Orden;
            _unitOfWork.MetodoPagoRepo.Update(current);
            await _unitOfWork.CommitAsync(ct);
            _logger.LogInformation("Metodo de Pago {MetodoPagoId} actualizado exitosamente.", metodoPago.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el Metodo de Pago {MetodoPagoId}.", metodoPago.Id);
            return Result.Fail("Error al actualizar el Metodo de Pago.");
        }
    }
}
