using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Domain.Entities;

namespace GymAdmin.Applications.Mapppers;

public static class AsistenciaMapper
{
    public static Asistencia ToAsistencia(this CreateAsistenciaDto createAsistenciaDto)
    {

        return new Asistencia
        {
            SocioId = createAsistenciaDto.IdSocio,
            Entrada = DateTime.UtcNow,
        };
    }
}
