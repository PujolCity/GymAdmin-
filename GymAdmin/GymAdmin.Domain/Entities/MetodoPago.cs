using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Entities;

public class MetodoPago : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public TipoAjusteSaldo TipoAjuste { get; set; } = TipoAjusteSaldo.Ninguno;
    public decimal ValorAjuste { get; set; } = 0.0m;
    public int Orden { get; set; }

    public ICollection<Pagos> Pagos { get; set; } = new List<Pagos>();

    //[NotMapped]
    //public ICalculadorTipoAjuste Calculador => CalculadorAjusteFactory.Crear(TipoAjusteEnum);

    //public decimal CalcularMontoFinal(decimal montoBase)
    //{
    //    var valor = ValorAjuste ?? 0;
    //    return Calculador.Aplicar(montoBase, valor);
    //}

    //public string AdjustmentDisplay =>
    //    TipoAjuste switch
    //    {
    //        TipoAjusteSaldo.Porcentaje => $"{ValorAjuste:+0.##;-0.##}%",
    //        TipoAjusteSaldo.MontoFijo => $"{ValorAjuste:+$0.##;- $0.##}",
    //        _ => "—"
    //    };
}