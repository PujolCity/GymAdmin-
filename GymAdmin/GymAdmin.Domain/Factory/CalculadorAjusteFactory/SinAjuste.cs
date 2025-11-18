using GymAdmin.Domain.Interfaces.Bussiness;

namespace GymAdmin.Domain.Factory.CalculadorAjusteFactory;

public class SinAjuste : ICalculadorTipoAjuste
{
    public decimal Aplicar(decimal montoBase, decimal valor) => montoBase;

}
