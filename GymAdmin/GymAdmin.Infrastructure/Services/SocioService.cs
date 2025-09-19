using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Pagination;
using GymAdmin.Domain.Results;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public async Task<PagedResult<Socio>> GetAllAsync(PaginationFilter filter, Paging paging, Sorting? sorting = null, CancellationToken ct = default)
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
            var hoy = DateTime.Today;
            sociosQuery = filter.Status == StatusFilter.Activo
                ? sociosQuery.Where(s => s.ExpiracionMembresia != null && s.ExpiracionMembresia >= hoy)
                : sociosQuery.Where(s => s.ExpiracionMembresia == null || s.ExpiracionMembresia < hoy);
        }

        var total = await sociosQuery.CountAsync(ct);

        var projected = sociosQuery.Select(s => new
        {
            Socio = s,
            UltimaAsistencia = s.Asistencias.Max(a => (DateTime?)a.Entrada)
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

                _ => projected.OrderBy(x => x.Socio.Apellido).ThenBy(x => x.Socio.Nombre)
            };
        }
        else
        {
            projected = projected.OrderBy(x => x.Socio.Apellido).ThenBy(x => x.Socio.Nombre);
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
            socio.UltimaAsistencia = x.UltimaAsistencia;

            if (socio is IEncryptableEntity enc)
                enc.HandleDecryption(_cryptoService);

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

    public async Task<Result> RegistrarAsistenciaAsync(Asistencia asistencia, CancellationToken ct = default)
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
}
