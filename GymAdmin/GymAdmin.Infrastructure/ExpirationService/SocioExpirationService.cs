using GymAdmin.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymAdmin.Infrastructure.ExpirationService;

public class SocioExpirationService : ISocioExpirationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SocioExpirationService> _logger;

    public SocioExpirationService(ILogger<SocioExpirationService> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task ExpirarSociosVencidosAsync(CancellationToken ct = default)
    {
        var hoy = DateTime.Today;

        var sociosQuery = _unitOfWork.SocioRepo.Query().AsNoTracking();

        var sociosAExpirar = await sociosQuery
            .Where(s =>
                s.IsActive &&
                (
                    s.CreditosRestantes <= 0 ||
                    (s.ExpiracionMembresia.HasValue && s.ExpiracionMembresia.Value < hoy)
                ))
            .ToListAsync(ct);

        if (sociosAExpirar.Count == 0)
        {
            _logger.LogInformation("No hay socios para expirar.");
            return;
        }

        foreach (var socio in sociosAExpirar)
        {
            socio.IsActive = false;
        }

        _unitOfWork.SocioRepo.UpdateRange(sociosAExpirar);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Se expiraron {Cantidad} socios vencidos.", sociosAExpirar.Count);
    }
}
