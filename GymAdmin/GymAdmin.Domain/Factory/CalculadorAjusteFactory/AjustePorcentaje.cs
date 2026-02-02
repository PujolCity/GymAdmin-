using GymAdmin.Domain.Interfaces.Bussiness;

namespace GymAdmin.Domain.Factory.CalculadorAjusteFactory;

public class AjustePorcentaje : ICalculadorTipoAjuste
{
    public decimal Calcular(decimal montoBase, decimal valor)
       => montoBase + (montoBase * valor / 100m);

    public decimal CalcularDelta(decimal baseAmount, decimal valorAjuste)
        => Math.Round(baseAmount * (valorAjuste / 100m), 2);
}
