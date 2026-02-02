using GymAdmin.Domain.Interfaces.Bussiness;

namespace GymAdmin.Domain.Factory.CalculadorAjusteFactory;

public class SinAjuste : ICalculadorTipoAjuste
{
    public decimal Calcular(decimal montoBase, decimal valor) => montoBase;

    public decimal CalcularDelta(decimal baseAmount, decimal valorAjuste) 
        => 0m;
}
