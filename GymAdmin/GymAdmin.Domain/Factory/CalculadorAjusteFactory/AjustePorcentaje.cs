using GymAdmin.Domain.Interfaces.Bussiness;

namespace GymAdmin.Domain.Factory.CalculadorAjusteFactory;

public class AjustePorcentaje : ICalculadorTipoAjuste
{
    public decimal Aplicar(decimal montoBase, decimal valor)
       => montoBase + (montoBase * valor / 100m);
}
