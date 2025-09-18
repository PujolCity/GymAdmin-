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
    private const int CREDITOS_DEFAULT = 0;

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

    public async Task<Result> AnularPagoAsync(Pagos pagoAnulacion, CancellationToken ct = default)
    {
        // ⚠️ Sólo usamos el Id (evitamos “update por objeto venido de UI”)
        var pagoId = pagoAnulacion.Id;

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            _logger.LogDebug("AnularPagoAsync INICIO. PagoId={PagoId}", pagoId);

            // 1) Carga con tracking + socio (puntos cómodos de breakpoint)
            var pago = await _unitOfWork.PagosRepo.Query()
                .Include(p => p.Socio)
                .SingleOrDefaultAsync(p => p.Id == pagoId, ct);

            if (pago is null)
            {
                _logger.LogWarning("Pago no encontrado. PagoId={PagoId}", pagoId);
                await tx.RollbackAsync(ct);
                return Result.Fail("No existe un pago con ese Id.");
            }

            if (pago.Estado == EstadoPago.Anulado)
            {
                _logger.LogDebug("Pago ya estaba anulado. PagoId={PagoId}", pagoId);
                await tx.CommitAsync(ct); // idempotente
                return Result.Ok();
            }

            var socio = pago.Socio ?? throw new InvalidOperationException("Pago sin socio asociado.");
            var saldoAntes = socio.CreditosRestantes;
            var aRestar = pago.CreditosAsignados;

            _logger.LogDebug("Validación saldo. SocioId={SocioId}, SaldoAntes={Saldo}, CreditosDelPago={Cred}",
                socio.Id, saldoAntes, aRestar);

            // 2) Validación de consumo (regla simple sin ledger)
            if (saldoAntes < aRestar)
            {
                _logger.LogWarning("Saldo insuficiente para anular. SocioId={SocioId}, Saldo={Saldo}, ARestar={ARestar}",
                    socio.Id, saldoAntes, aRestar);
                await tx.RollbackAsync(ct);
                return Result.Fail("No se puede anular: ya se consumieron créditos de ese pago.");
            }

            // 3) Revertir saldo
            socio.CreditosRestantes = saldoAntes - aRestar;

            // 4) Recalcular vencimiento si este pago sostenía el actual
            var expAntes = socio.ExpiracionMembresia;
            var sosteniaVenc = expAntes != null && expAntes == pago.FechaVencimiento;

            DateTime? nuevoVenc = expAntes;
            if (sosteniaVenc)
            {
                nuevoVenc = await _unitOfWork.PagosRepo.Query()
                    .Where(x => x.SocioId == socio.Id && x.Estado == EstadoPago.Pagado && x.Id != pago.Id)
                    .MaxAsync(x => (DateTime?)x.FechaVencimiento, ct);

                socio.ExpiracionMembresia = nuevoVenc;
            }

            // 5) Marcar anulado
            pago.Estado = EstadoPago.Anulado;

            _logger.LogDebug("Aplicando cambios. SocioId={SocioId}, Saldo {Antes}->{Despues}, Vence {ExpAntes}->{ExpDespues}",
                socio.Id, saldoAntes, socio.CreditosRestantes, expAntes, socio.ExpiracionMembresia);

            // 6) Guardar y commit TX
            var affected = await _unitOfWork.CommitAsync(ct);
            _logger.LogDebug("SaveChanges OK. Filas afectadas={Affected}", affected);

            await tx.CommitAsync(ct);
            _logger.LogDebug("Transacción COMMIT. PagoId={PagoId}", pagoId);

            return Result.Ok();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Concurrencia al anular pago. PagoId={PagoId}", pagoAnulacion.Id);
            return Result.Fail("Otro proceso modificó el registro. Reintentá.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Error al anular pago. PagoId={PagoId}", pagoAnulacion.Id);
            return Result.Fail("Ocurrió un error al anular el pago.");
        }
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

        socio.AplicaCompraReseteando(plan.Creditos, pago.FechaVencimiento);

        try
        {
            await _unitOfWork.PagosRepo.AddAsync(pago, ct);
            _unitOfWork.SocioRepo.Update(socio);
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
