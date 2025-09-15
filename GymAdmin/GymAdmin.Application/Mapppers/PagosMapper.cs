using GymAdmin.Applications.DTOs.MetodosDePagoDto;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.Mapppers;

public static class PagosMapper
{
    public static MetodoPagoDto ToMetodoPagoDto(this MetodoPago metodoPago)
    { 
        return new MetodoPagoDto
        {
            Id = metodoPago.Id,
            Nombre = metodoPago.Nombre
        };
    }

    public static Pagos ToPagos (this PagoCreateDto pagoDto)
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
            FechaVencimiento = pagoDto.FechaVencimiento
        };
    }
}
