using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.Services;

public class PagosServices : IPagosServices
{
    private readonly ILogger<PagosServices> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICryptoService _cryptoService;

    public PagosServices(ILogger<PagosServices> logger,
        IUnitOfWork unitOfWorks,
        ICryptoService cryptoService)
    {
        _logger = logger;
        _unitOfWork = unitOfWorks;
        _cryptoService = cryptoService;
    }

    public async Task<Result<List<MetodoPago>>> GetMetodosPagoAsync(bool isActive = true, CancellationToken ct = default)
    {
        var query = _unitOfWork.MetodoPagoRepo.Query();

        if (isActive)
            query = query.Where(m => m.IsActive);

        var result = await query.ToListAsync();
        return Result<List<MetodoPago>>.Ok(result);
    }

    public async Task<PagedResult<Pagos>> GetAllAsync(PagosFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default)
    {
        IQueryable<Pagos> q = _unitOfWork.PagosRepo.Query()
            .AsNoTracking()
            .Include(p => p.Socio)
            .Include(p => p.MetodoPagoRef)
            .Include(p => p.PlanMembresia);

        if (!string.IsNullOrWhiteSpace(filter.Texto))
        {
            var texto = filter.Texto.Trim();
            var digitsOnly = new string(texto.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length == 8)
            {
                var hash = _cryptoService.ComputeHash(digitsOnly);
                q = q.Where(p => p.Socio.DniHash == hash);
            }
            else
            {
                var like = $"%{texto}%";
                q = q.Where(p =>
                    EF.Functions.Like(p.Socio.Nombre, like) ||
                    EF.Functions.Like(p.Socio.Apellido, like) ||
                    EF.Functions.Like(p.PlanMembresia.Nombre, like));
            }
        }

        // Status
        if (filter.Status != StatusPagosFilter.Todos)
        {
            q = filter.Status == StatusPagosFilter.Pagado
                ? q.Where(p => p.Estado == EstadoPago.Pagado)
                : q.Where(p => p.Estado == EstadoPago.Anulado);
        }

        if (filter.FechaDesde.HasValue)
            q = q.Where(p => p.FechaPago >= filter.FechaDesde.Value);

        if (filter.FechaHasta.HasValue)
            q = q.Where(p => p.FechaPago < filter.FechaHasta.Value);

        // Total antes de paginar
        var total = await q.CountAsync(ct);

        // Sorting
        IOrderedQueryable<Pagos> oq;
        if (sorting.HasValue)
        {
            var s = sorting.Value;
            oq = (s.SortBy, s.Desc) switch
            {
                ("FechaPago", true) => q.OrderByDescending(p => p.FechaPago),
                ("FechaPago", false) => q.OrderBy(p => p.FechaPago),

                ("NombreCompleto", true) => q.OrderByDescending(p => p.Socio.Apellido).ThenByDescending(p => p.Socio.Nombre),
                ("NombreCompleto", false) => q.OrderBy(p => p.Socio.Apellido).ThenBy(p => p.Socio.Nombre),

                ("Metodo", true) => q.OrderByDescending(p => p.MetodoPagoRef.Nombre),
                ("Metodo", false) => q.OrderBy(p => p.MetodoPagoRef.Nombre),

                ("Plan", true) => q.OrderByDescending(p => p.PlanMembresia.Nombre),
                ("Plan", false) => q.OrderBy(p => p.PlanMembresia.Nombre),

                ("Monto", true) => q.OrderByDescending(p => p.Precio),
                ("Monto", false) => q.OrderBy(p => p.Precio),

                _ => q.OrderByDescending(p => p.FechaPago)
            };
        }
        else
        {
            oq = q.OrderByDescending(p => p.FechaPago);
        }

        // Paging
        var page = Math.Max(1, paging.PageNumber);
        var size = Math.Clamp(paging.PageSize, 1, 500);

        var items = await oq
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        foreach (var p in items)
        {
            if (p.Socio is IEncryptableEntity enc)
                enc.HandleDecryption(_cryptoService);
        }

        return new PagedResult<Pagos>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = size
        };
    }

    public Task<Result> AnularPagoAsync(int pagoId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> CrearPagoAsync(Pagos pago, CancellationToken ct = default)
    {
        var socio = await _unitOfWork.SocioRepo.GetByIdAsync(pago.SocioId, ct);
        if (socio is null)
            return Result.Fail("No existe un socio con el Id.");
        var plan = await _unitOfWork.MembresiaRepo.GetByIdAsync(pago.PlanMembresiaId, ct);
        if (plan is null)
            return Result.Fail("No existe un plan de membresía con el Id.");
        var metodo = await _unitOfWork.MetodoPagoRepo.GetByIdAsync(pago.MetodoPagoId, ct);
        if (metodo is null)
            return Result.Fail("No existe un método de pago con el Id.");

        try
        {
            await _unitOfWork.PagosRepo.AddAsync(pago, ct);
            await _unitOfWork.CommitAsync(ct);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear pago para socio {SocioId}", pago.SocioId);
            return Result.Fail("Ocurrió un error al crear el pago.");
        }
    }
}
