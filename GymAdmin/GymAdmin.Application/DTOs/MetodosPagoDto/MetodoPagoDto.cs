using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.DTOs.MetodosPagoDto;

public class MetodoPagoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } 
    public bool IsActive { get; set; } = true;
    public string Estado {  get; set; }
    public TipoAjusteSaldo TipoAjuste { get; set; } = TipoAjusteSaldo.Ninguno;
    public decimal ValorAjuste { get; set; } = 0.0m;
    public int Orden { get; set; }
}
