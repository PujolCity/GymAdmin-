using System.ComponentModel.DataAnnotations;

namespace GymAdmin.Domain.Entities;

public abstract class EntityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Soft delete (opcional)
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Métodos helpers
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        MarkAsUpdated();
    }
}