namespace GymAdmin.Applications.DTOs.ConfiguracionDto;

public class ConfiguracionDto
{
    public string NombreGym { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string CarpetaBase { get; set; } = string.Empty;
    public string CarpetaBackups { get; set; } = string.Empty;
    public DateTime? UltimoBackupAt { get; set; }
    public int BackupRetentionCount { get; set; } = 7;

}
