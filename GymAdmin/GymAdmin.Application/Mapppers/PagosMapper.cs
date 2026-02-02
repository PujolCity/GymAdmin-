using GymAdmin.Applications.DTOs;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.Mapppers;

public static class PagosMapper
{
    public static Pagos ToPagos(this PagoCreateDto pagoDto)
    {
        return new Pagos
        {
            SocioId = pagoDto.SocioId,
            PlanMembresiaId = pagoDto.PlanMembresiaId,
            MetodoPagoId = pagoDto.MetodoPagoId,
            Precio = pagoDto.Precio,
            Observaciones = pagoDto.Observaciones,
            CreditosAsignados = pagoDto.CreditosAsignados,
            Estado = EstadoPago.Pagado,
            FechaPago = pagoDto.FechaPago,
            FechaVencimiento = pagoDto.FechaVencimiento,
            AjusteImporte = pagoDto.AjusteImporte,
            MontoFinal = pagoDto.MontoFinal,
            TipoAjusteAplicado = pagoDto.TipoAjusteAplicado,
            ValorAjusteAplicado = pagoDto.ValorAjusteAplicado
        };
    }

    public static Pagos ToPagos(this BaseDeleteRequest baseDeleteRequest)
    {
        return new Pagos
        {
            Id = baseDeleteRequest.IdToDelete
        };
    }
}
