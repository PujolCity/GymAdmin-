using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.DTOs.MetodosPagoDto;

public class MetodoPagoCreateDTO
{
    public string Nombre { get; set; }
    public bool IsActive { get; set; } = true;
    public TipoAjusteSaldo TipoAjuste { get; set; } = TipoAjusteSaldo.Ninguno;
    public decimal ValorAjuste { get; set; } = 0.0m;
    public int Orden { get; set; }
}
