namespace GymAdmin.Domain.Entities;

public class PlanesMembresia : EntityBase
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Creditos { get; set; }         
    public int DiasValidez { get; set; }      
    public bool IsActive { get; set; } = true;
    public int DiasPorSemana{ get; set; } 

    // Ejemplos:
    // - Plan 1 día: Credits=1, ValidityDays=1
    // - Plan 7 días: Credits=7, ValidityDays=7  
    // - Plan 2 días/semana: Credits=8, ValidityDays=30 (4 semanas)
}
