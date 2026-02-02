using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Domain.Entities;

namespace GymAdmin.Applications.Mapppers;

public static class MetodoPagoMapper
{
    public static MetodoPagoCreateDTO ToMetodoPagoCreateDTO(this MetodoPagoDto metodoPagoDto)
    {
        return new MetodoPagoCreateDTO
        {
            Nombre = metodoPagoDto.Nombre,
            IsActive = metodoPagoDto.IsActive,
            TipoAjuste = metodoPagoDto.TipoAjuste,
            ValorAjuste = metodoPagoDto.ValorAjuste,
            Orden = metodoPagoDto.Orden
        };
    }

    public static MetodoPago ToMetodoPago(this MetodoPagoCreateDTO metodoPagoCreateDTO)
    {
        return new MetodoPago
        {
            Nombre = metodoPagoCreateDTO.Nombre,
            IsActive = metodoPagoCreateDTO.IsActive,
            TipoAjuste = metodoPagoCreateDTO.TipoAjuste,
            ValorAjuste = metodoPagoCreateDTO.ValorAjuste,
            Orden = metodoPagoCreateDTO.Orden
        };
    }

    public static MetodoPagoDto ToMetodoPagoDto(this MetodoPago metodoPago)
    {
        return new MetodoPagoDto
        {
            Id = metodoPago.Id,
            Nombre = metodoPago.Nombre,
            IsActive = metodoPago.IsActive,
            Estado = metodoPago.IsActive ? "Activo" : "Inactivo",
            TipoAjuste = metodoPago.TipoAjuste,
            ValorAjuste = metodoPago.ValorAjuste,
            Orden = metodoPago.Orden
        };
    }

    public static MetodoPago ToMetodoPago(this MetodoPagoDto metodoPagoDto)
    {
        return new MetodoPago
        {
            Id = metodoPagoDto.Id,
            Nombre = metodoPagoDto.Nombre,
            IsActive = metodoPagoDto.IsActive,
            TipoAjuste = metodoPagoDto.TipoAjuste,
            ValorAjuste = metodoPagoDto.ValorAjuste,
            Orden = metodoPagoDto.Orden
        };
    }
}
