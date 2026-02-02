using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Interfaces.Bussiness;

namespace GymAdmin.Domain.Factory.CalculadorAjusteFactory;

public static class CalculadorAjusteFactory
{
    public static ICalculadorTipoAjuste Crear(TipoAjusteSaldo tipo) => tipo switch
    {
        TipoAjusteSaldo.Porcentaje => new AjustePorcentaje(),
        TipoAjusteSaldo.MontoFijo => new AjusteMontoFijo(),
        _ => new SinAjuste()
    };
}