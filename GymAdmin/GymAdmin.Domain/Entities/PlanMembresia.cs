namespace GymAdmin.Domain.Entities;

public class PlanMembresia : EntityBase
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Creditos { get; set; }          // Cantidad de clases incluídas
    public int DiasValidez { get; set; }       // Días de validez del plan
    public bool IsActive { get; set; } = true;
    public int DaysPorSemana{ get; set; }      // Para planes de X días por semana

    // Ejemplos:
    // - Plan 1 día: Credits=1, ValidityDays=1
    // - Plan 7 días: Credits=7, ValidityDays=7  
    // - Plan 2 días/semana: Credits=8, ValidityDays=30 (4 semanas)
}
