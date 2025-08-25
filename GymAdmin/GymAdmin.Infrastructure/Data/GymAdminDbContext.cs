using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Infrastructure.Data;

public class GymAdminDbContext : DbContext
{
    public GymAdminDbContext(DbContextOptions<GymAdminDbContext> options)
           : base(options)
    {
    }

    // DbSets
    public DbSet<Miembro> Miembros { get; set; }
    public DbSet<PlanMembresia> PlanesMembresia { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<Asistencia> Asistencias { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Miembro
        modelBuilder.Entity<Miembro>(entity =>
        {
            entity.HasQueryFilter(m => !m.IsDeleted);
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Dni)
                .IsRequired()
                .HasMaxLength(8);

            entity.Property(m => m.Nombre)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.Apellido)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.Email)
                .HasMaxLength(100);

            entity.Property(m => m.RegistrationDate)
                .IsRequired();

            entity.Property(m => m.CreditosRestantes)
                .HasDefaultValue(0);

            entity.Property(m => m.TotalCreditosComprados)
                .HasDefaultValue(0);

            entity.Property(m => m.ExpiracionMembresia)
                .IsRequired();

            // Índices únicos
            entity.HasIndex(m => m.Dni)
                .IsUnique();

            entity.HasIndex(m => m.Email)
                .IsUnique();

            // Relaciones
            entity.HasMany(m => m.Payments)
                .WithOne(p => p.Miembro)
                .HasForeignKey(p => p.MiembroId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Attendances)
                .WithOne(a => a.Miembro)
                .HasForeignKey(a => a.MiembroId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de PlanMembresia
        modelBuilder.Entity<PlanMembresia>(entity =>
        {
            entity.HasQueryFilter(pm => !pm.IsDeleted);
            entity.HasKey(pm => pm.Id);

            entity.Property(pm => pm.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(pm => pm.Descripcion)
                .HasMaxLength(500);

            entity.Property(pm => pm.Precio)
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");

            entity.Property(pm => pm.Creditos)
                .IsRequired();

            entity.Property(pm => pm.DiasValidez)
                .IsRequired();

            entity.Property(pm => pm.DaysPorSemana)
                .IsRequired();

            entity.Property(pm => pm.IsActive)
                .HasDefaultValue(true);
        });

        // Configuración de Pago
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasQueryFilter(p => !p.IsDeleted);
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");

            entity.Property(p => p.PaymentDate)
                .IsRequired();

            entity.Property(p => p.MetodoPago)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Observaciones)
                .HasMaxLength(500);

            entity.Property(p => p.CreditosAsignados)
                .IsRequired();

            entity.Property(p => p.FechaVencimiento)
                .IsRequired();

            // Relaciones
            entity.HasOne(p => p.Miembro)
                .WithMany(m => m.Payments)
                .HasForeignKey(p => p.MiembroId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.PlanMembresia)
                .WithMany()
                .HasForeignKey(p => p.PlanMembresiaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Asistencia
        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasQueryFilter(a => !a.IsDeleted);
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Entrada)
                .IsRequired();

            entity.Property(a => a.SeUsoCredito)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(a => a.Observaciones)
                .HasMaxLength(500);

            // Relaciones
            entity.HasOne(a => a.Miembro)
                .WithMany(m => m.Attendances)
                .HasForeignKey(a => a.MiembroId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de SystemConfig
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasQueryFilter(sc => !sc.IsDeleted);
            entity.HasKey(sc => sc.Id);

            entity.Property(sc => sc.NombreGimnasio)
                .HasMaxLength(100);

            entity.Property(sc => sc.Direccion)
                .HasMaxLength(200);

            entity.Property(sc => sc.Telefono)
                .HasMaxLength(20);

            entity.Property(sc => sc.DiasValidezCredito)
                .HasDefaultValue(30);

            entity.Property(sc => sc.ExpiracionAutomaticaCredito)
                .HasDefaultValue(true);
        });

        // Configuración de User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasQueryFilter(u => !u.IsDeleted);
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.Email)
                .HasMaxLength(100);

            entity.Property(u => u.FullName)
                .HasMaxLength(100);

            entity.Property(u => u.IsActive)
                .HasDefaultValue(true);

            // Índices únicos
            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is EntityBase &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (EntityBase)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.MarkAsUpdated();
            }
        }
    }
}