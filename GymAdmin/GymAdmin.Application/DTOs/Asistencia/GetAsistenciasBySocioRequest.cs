namespace GymAdmin.Applications.DTOs.Asistencia;

public class GetAsistenciasBySocioRequest
{
    public int SocioId { get; set; }
    public DateTime? Fecha { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; init; } = "Entrada";
    public bool SortDesc { get; init; } = true;
}
