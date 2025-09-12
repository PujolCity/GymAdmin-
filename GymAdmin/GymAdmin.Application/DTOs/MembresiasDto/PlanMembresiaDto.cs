namespace GymAdmin.Applications.DTOs.MembresiasDto;

public class PlanMembresiaDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public int Creditos { get; init; }
    public int DiasValidez { get; init; }
    public decimal Precio { get; init; }
    public bool IsActive { get; set; }
    public string Estado { get; set; } = "Inactivo";
}
