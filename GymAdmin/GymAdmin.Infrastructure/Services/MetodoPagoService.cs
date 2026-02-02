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

    public async Task<Result<MetodoPago>> CreateAsync(MetodoPago metodoPago, CancellationToken ct)
    {
        var exist = await _unitOfWork.MetodoPagoRepo.Query()
            .AsNoTracking()
            .AnyAsync(mp => mp.Nombre.ToLower() == metodoPago.Nombre.ToLower() && !mp.IsDeleted, ct);
        if (exist)
            return Result<MetodoPago>.Fail("El nombre del metodo de pago ya está en uso");
        int maxOrden = await _unitOfWork.MetodoPagoRepo.GetMaxOrden(ct);

        metodoPago.Orden = maxOrden + 1;

        try
        {
            await _unitOfWork.MetodoPagoRepo.AddAsync(metodoPago, ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Metodo de Pago {MetodoPagoId} agregado exitosamente.", metodoPago.Id);
            return Result<MetodoPago>.Ok(metodoPago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar el Metodo de Pago {nombre}.", metodoPago.Nombre);
            return Result<MetodoPago>.Fail("Error al agregar el Metodo de Pago.");
        }
    }

    public async Task<Result> DeleteAsync(MetodoPago metodoPago, CancellationToken ct)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                // IMPORTANTE: ignorar filtros para poder encontrar aunque esté softdeleted
                var entity = await _unitOfWork.MetodoPagoRepo.Query()
                    .IgnoreQueryFilters()
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == metodoPago.Id, token);

                if (entity is null)
                    return Result.Fail("El método de pago no existe.");

                if (entity.IsDeleted)
                    return Result.Ok(); // idempotente

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                var ordenEliminado = entity.Orden;

                // Compactar orden: todos los que estaban después bajan 1 (solo los NO eliminados)
                await _unitOfWork.MetodoPagoRepo.CompactOrdenAfterDeleteAsync(ordenEliminado, token);

                await _unitOfWork.CommitAsync(token);
                return Result.Ok();
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar MetodoPago {Id}", metodoPago.Id);
            return Result.Fail("Error al eliminar el Método de Pago.");
        }
    }

    public async Task<PagedResult<MetodoPago>> GetAllAsync(PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default)
    {
        var query = _unitOfWork.MetodoPagoRepo.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Texto))
        {
            var texto = filter.Texto.Trim();
            var like = $"%{texto}%";

            query = query.Where(mp =>
                    EF.Functions.Like(mp.Nombre, like));
        }

        if (filter.Status != StatusFilter.Todos)
        {
            query = filter.Status == StatusFilter.Activo ? query.Where(pm => pm.IsActive)
                                                     : query.Where(pm => !pm.IsActive);
        }

        var total = await query.CountAsync(ct);

        query = query.OrderBy(pm => pm.Orden);

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

    public async Task<Result> UpdateAsync(MetodoPago metodoPago, CancellationToken ct)
    {
        var current = await _unitOfWork.MetodoPagoRepo.GetByIdAsync(metodoPago.Id, ct);
        if (current is null)
            return Result.Fail("El metodo de Pago no existe");

        var nameTaken = await _unitOfWork.MetodoPagoRepo.Query()
            .AsNoTracking()
            .AnyAsync(mp => mp.Id != metodoPago.Id
            && mp.Nombre.ToLower() == metodoPago.Nombre.ToLower()
            && !mp.IsDeleted, ct);
        if (nameTaken)
            return Result.Fail("El nombre del metodo de pago ya está en uso");

        try
        {
            current.Nombre = metodoPago.Nombre;
            current.IsActive = metodoPago.IsActive;
            current.TipoAjuste = metodoPago.TipoAjuste;
            current.ValorAjuste = metodoPago.ValorAjuste;
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

    public async Task<Result> MoveDownAsync(int metodoPagoId, CancellationToken ct)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async c =>
            {
                var actual = await _unitOfWork.MetodoPagoRepo.Query()
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == metodoPagoId && !x.IsDeleted, c);

                if (actual is null) return Result.Fail("No existe el método de pago.");

                var next = await _unitOfWork.MetodoPagoRepo.GetNeighborDownAsync(actual.Orden, c);
                if (next is null) return Result.Ok();

                await _unitOfWork.MetodoPagoRepo.SwapOrdenAsync(actual.Id, next.Id, c);

                return Result.Ok();
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al mover hacia abajo el Metodo de Pago {MetodoPagoId}.", metodoPagoId);
            return Result.Fail("Error al mover hacia abajo el Metodo de Pago.");
        }
    }

    public async Task<Result> MoveUpAsync(int metodoPagoId, CancellationToken ct)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async c =>
            {
                var actual = await _unitOfWork.MetodoPagoRepo.Query()
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == metodoPagoId && !x.IsDeleted, c);

                if (actual is null) return Result.Fail("No existe el método de pago.");
                if (actual.Orden <= 1) return Result.Ok(); // ya está arriba

                var prev = await _unitOfWork.MetodoPagoRepo.GetNeighborUpAsync(actual.Orden, c);
                if (prev is null) return Result.Ok();

                await _unitOfWork.MetodoPagoRepo.SwapOrdenAsync(actual.Id, prev.Id, c);

                return Result.Ok();
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al mover hacia arriba el Metodo de Pago {MetodoPagoId}.", metodoPagoId);
            return Result.Fail("Error al mover hacia arriba el Metodo de Pago.");
        }
    }
}
