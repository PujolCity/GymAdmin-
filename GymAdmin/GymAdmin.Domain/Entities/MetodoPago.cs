using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Factory.CalculadorAjusteFactory;
using GymAdmin.Domain.Interfaces.Bussiness;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymAdmin.Domain.Entities;

public class MetodoPago : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public TipoAjuste TipoAjuste { get; set; } = TipoAjuste.Ninguno;
    public decimal? ValorAjuste { get; set; }
    public int Orden { get; set; }

    public ICollection<Pagos> Pagos { get; set; } = new List<Pagos>();

    [NotMapped]
    public ICalculadorTipoAjuste Calculador => CalculadorAjusteFactory.Crear(TipoAjuste);

    public decimal CalcularMontoFinal(decimal montoBase)
    {
        var valor = ValorAjuste ?? 0;
        return Calculador.Aplicar(montoBase, valor);
    }

    public string AdjustmentDisplay =>
        TipoAjuste switch
        {
            TipoAjuste.Porcentaje => $"{ValorAjuste:+0.##;-0.##}%",
            TipoAjuste.MontoFijo => $"{ValorAjuste:+$0.##;- $0.##}",
            _ => "—"
        };
}