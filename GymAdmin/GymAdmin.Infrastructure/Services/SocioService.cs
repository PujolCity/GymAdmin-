using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.Services;

public class SocioService : ISocioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SocioService> _logger;
    private readonly ICryptoService _cryptoService;

    public SocioService(IUnitOfWork unitOfWork,
        ILogger<SocioService> logger,
        ICryptoService cryptoService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cryptoService = cryptoService;
    }

    public async Task<Result> CreateAsync(Socio socio, CancellationToken ct = default)
    {

        var socioExistente = await _unitOfWork.SocioRepo.GetSocioByDni(socio.Dni);
        if (socioExistente != null)
            return Result.Fail("Ya existe un socio con el mismo DNI.");

        try
        {
            await _unitOfWork.SocioRepo.AddAsync(socio);
            await _unitOfWork.CommitAsync(ct);

            return Result.Ok();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true)
        {
            _logger.LogWarning(ex, "Intento de duplicado al crear socio {Dni}", socio.DniEncrypted);
            return Result.Fail("Ya existe un socio con el mismo email o DNI.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear socio {Dni}", socio.DniEncrypted);
            return Result.Fail("Ocurrió un error al crear el socio.");
        }
    }

    public async Task<Result> UpdateAsync(Socio socio, CancellationToken ct = default)
    {
        var socioUpdate = await _unitOfWork.SocioRepo.GetByIdAsync(socio.Id, ct);
        if (socioUpdate == null)
            return Result.Fail("El socio no existe.");

        var socioWithSameDni = await _unitOfWork.SocioRepo.GetSocioByDni(socio.Dni);
        if (socioWithSameDni != null && socioWithSameDni.Id != socio.Id)
            return Result.Fail("Ya existe un socio con el mismo DNI.");
        try
        {
            socioUpdate.Nombre = socio.Nombre;
            socioUpdate.Apellido = socio.Apellido;
            socioUpdate.Dni = socio.Dni;

            _unitOfWork.SocioRepo.Update(socioUpdate);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Socio actualizado: {Nombre} {Apellido} (Id={Id})", socio.Nombre, socio.Apellido, socio.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar socio {SocioId}", socio.Id);
            return Result.Fail("Ocurrió un error al actualizar el socio.");
        }
    }

    public async Task<Result> DeleteAsync(Socio socio, CancellationToken ct = default)
    {
        var existSocio = await _unitOfWork.SocioRepo.Query()
            .AsNoTracking()
            .AnyAsync(s => s.Id == socio.Id, ct);

        if (!existSocio)
            return Result.Fail("El socio no existe.");

        try
        {
            await _unitOfWork.SocioRepo.SoftDeleteAsync(socio.Id, ct);
            await _unitOfWork.CommitAsync(ct);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar socio {SocioId}", socio.Id);
            return Result.Fail("Ocurrió un error al eliminar el socio.");
        }
    }

    public async Task<PagedResult<Socio>> GetAllAsync(
     PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default)
    {
        var sociosQuery = _unitOfWork.SocioRepo.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Texto))
        {
            var texto = filter.Texto.Trim();
            var digitsOnly = new string(texto.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length == 8)
            {
                var hash = _cryptoService.ComputeHash(digitsOnly);
                sociosQuery = sociosQuery.Where(s => s.DniHash == hash);
            }
            else
            {
                var like = $"%{texto}%";
                sociosQuery = sociosQuery.Where(s =>
                    EF.Functions.Like(s.Nombre, like) ||
                    EF.Functions.Like(s.Apellido, like));
            }
        }

        if (filter.Status != StatusFilter.Todos)
        {
            var nowUtc = DateTime.UtcNow;
            sociosQuery = filter.Status == StatusFilter.Activo
                ? sociosQuery.Where(s => s.ExpiracionMembresia != null && s.ExpiracionMembresia >= nowUtc)
                : sociosQuery.Where(s => s.ExpiracionMembresia == null || s.ExpiracionMembresia < nowUtc);
        }

        var total = await sociosQuery.CountAsync(ct);

        var projected = sociosQuery.Select(s => new
        {
            Socio = s,
            UltimaAsistencia = s.Asistencias.Max(a => (DateTime?)a.Entrada),

            UltimoPagoInfo = s.Pagos
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.FechaPago)                 
                .Select(p => new
                {
                    p.FechaPago,
                    Precio = p.Precio,
                    PlanNombre = p.PlanMembresia != null
                ? p.PlanMembresia.Nombre
                : null
                })
                .FirstOrDefault(),

            TotalCreditos = s.TotalCreditosComprados
        });

        if (sorting.HasValue)
        {
            var sort = sorting.Value;
            projected = (sort.SortBy, sort.Desc) switch
            {
                ("UltimaAsistencia", true) => projected.OrderByDescending(x => x.UltimaAsistencia),
                ("UltimaAsistencia", false) => projected.OrderBy(x => x.UltimaAsistencia),

                ("NombreCompleto", true) => projected.OrderByDescending(x => x.Socio.Apellido)
                                                        .ThenByDescending(x => x.Socio.Nombre),
                ("NombreCompleto", false) => projected.OrderBy(x => x.Socio.Apellido)
                                                        .ThenBy(x => x.Socio.Nombre),

                ("ExpiracionMembresia", true) => projected.OrderByDescending(x => x.Socio.ExpiracionMembresia),
                ("ExpiracionMembresia", false) => projected.OrderBy(x => x.Socio.ExpiracionMembresia),

                _ => projected.OrderByDescending(x => x.Socio.FechaRegistro)
            };
        }
        else
        {
            projected = projected.OrderByDescending(x => x.Socio.FechaRegistro);
        }

        var page = Math.Max(1, paging.PageNumber);
        var size = Math.Clamp(paging.PageSize, 1, 500);

        var pageItems = await projected
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        var socios = new List<Socio>(pageItems.Count);
        foreach (var x in pageItems)
        {
            var socio = x.Socio;

            if (socio is IEncryptableEntity enc)
                enc.HandleDecryption(_cryptoService); 

            socio.UltimaAsistencia = x.UltimaAsistencia;

            if (x.UltimoPagoInfo is not null)
            {
                socio.UltimoPago = DateTime.SpecifyKind(x.UltimoPagoInfo.FechaPago, DateTimeKind.Utc);

                socio.PlanNombre = string.IsNullOrWhiteSpace(x.UltimoPagoInfo.PlanNombre) ? "—" : x.UltimoPagoInfo.PlanNombre;

                socio.PlanPrecio = x.UltimoPagoInfo.Precio; 
            }
            else
            {
                socio.UltimoPago = null;
                socio.PlanNombre = "—";
                socio.PlanPrecio = 0m;
            }

            if (socio is Socio s)
                s.ExpireIfNeeded();

            socios.Add(socio);
        }

        return new PagedResult<Socio>
        {
            Items = socios,
            TotalCount = total,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<List<Socio>> GetAllForLookupAsync(CancellationToken ct = default)
    {
        var socios = await _unitOfWork.SocioRepo.GetAllAsync(ct);

        return socios
            .OrderBy(s => s.Apellido)
            .ThenBy(s => s.Nombre)
            .ToList();
    }

    public async Task<Result<Socio>> GetSocioByIdAsync(int socioId, CancellationToken ct = default)
    {
        var data = await _unitOfWork.SocioRepo.Query()
            .AsNoTracking()
            .Where(s => s.Id == socioId)
            .Select(s => new
            {
                Socio = s,
                UltimoPago = s.Pagos
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.FechaPago) // <- o p.Fecha
                    .Select(p => new
                    {
                        p.FechaPago,              // <- o p.Fecha
                        p.Precio,
                        PlanNombre = p.PlanMembresia.Nombre
                    })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (data is null)
            return Result<Socio>.Fail("No existe un socio con el Id.");

        var socio = data.Socio;
        socio.PlanNombre = data.UltimoPago?.PlanNombre ?? "—";
        socio.PlanPrecio = data.UltimoPago?.Precio ?? 0m;   

        if (socio is IEncryptableEntity enc)
            enc.HandleDecryption(_cryptoService);

        socio.UltimoPago = data.UltimoPago is null
            ? (DateTime?)null
            : DateTime.SpecifyKind(data.UltimoPago.FechaPago, DateTimeKind.Utc);

        return Result<Socio>.Ok(socio);
    }
}
