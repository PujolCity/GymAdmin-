using GymAdmin.Applications.DTOs;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Domain.Entities;

namespace GymAdmin.Applications.Mapppers;

public static class SocioMapper
{
    public static Socio ToSocio(this SocioCreateDto socioDto)
    {

        return new Socio()
        {
            Nombre = socioDto.Nombre,
            Apellido = socioDto.Apellido,
            Dni = socioDto.Dni,
            ExpiracionMembresia = DateTime.UtcNow.AddDays(-1) 
        };
    }

    public static SocioCreateDto ToSocioCreateDto(this SocioDto socio)
    {
        return new SocioCreateDto()
        {
            Nombre = socio.Nombre,
            Apellido = socio.Apellido,
            Dni = socio.Dni,
        };
    }
    public static Socio ToSocio(this BaseDeleteRequest request)
    {
        return new Socio()
        {
            Id = request.IdToDelete
        };
    }

    public static SocioLookupDto ToLookupDto(this Socio s)
    {
        return new SocioLookupDto
        {
            Id = s.Id,
            NombreCompleto = $"{s.Apellido}, {s.Nombre}",
            Dni = s.Dni 
        };
    }
}
