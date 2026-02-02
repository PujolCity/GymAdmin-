using GymAdmin.Domain.Interfaces.Bussiness;

namespace GymAdmin.Domain.Factory.CalculadorAjusteFactory;

public class AjusteMontoFijo : ICalculadorTipoAjuste
{
    public decimal Calcular(decimal montoBase, decimal valor)
       => montoBase + valor;

    public decimal CalcularDelta(decimal baseAmount, decimal valorAjuste) 
        => valorAjuste;
}
