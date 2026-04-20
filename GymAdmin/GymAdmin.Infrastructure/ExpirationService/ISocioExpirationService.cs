namespace GymAdmin.Infrastructure.ExpirationService;

public interface ISocioExpirationService
{
    Task ExpirarSociosVencidosAsync(CancellationToken ct = default);
}
