namespace GymAdmin.Domain.Entities;

public class User : EntityBase
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
}