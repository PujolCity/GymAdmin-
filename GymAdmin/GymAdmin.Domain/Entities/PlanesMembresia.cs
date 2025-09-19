namespace GymAdmin.Domain.Entities;

public class PlanesMembresia : EntityBase
{
    public string Nombre { get; set; }
    public string? Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Creditos { get; set; }         
    public int DiasValidez { get; set; }      
    public bool IsActive { get; set; } = true;

    // Ejemplos:
    // - Plan 1 día: Credits=1, ValidityDays=1
    // - Plan 7 días: Credits=7, ValidityDays=7  
    // - Plan 2 días/semana: Credits=8, ValidityDays=30 (4 semanas)
}
