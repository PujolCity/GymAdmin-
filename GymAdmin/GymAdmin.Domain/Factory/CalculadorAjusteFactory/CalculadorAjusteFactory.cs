using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Bussiness;

namespace GymAdmin.Domain.Factory.CalculadorAjusteFactory;

public static class CalculadorAjusteFactory
{
    public static ICalculadorTipoAjuste Crear(TipoAjuste tipo) => tipo switch
    {
        TipoAjuste.Porcentaje => new AjustePorcentaje(),
        TipoAjuste.MontoFijo => new AjusteMontoFijo(),
        _ => new SinAjuste()
    };
}