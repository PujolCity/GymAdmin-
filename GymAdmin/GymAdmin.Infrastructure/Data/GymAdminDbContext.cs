using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Infrastructure.Data;

// COMANDO PARA MIGRACIONES EN CONSOLA DE GESTIÓN DE PAQUETES:
// Add-Migration ExpiracionNulleable -Project GymAdmin.Infrastructure -StartupProject GymAdmin.Desktop

public class GymAdminDbContext : DbContext
{
    public ICryptoService _cryptoService;

    public GymAdminDbContext(DbContextOptions<GymAdminDbContext> options, 
        ICryptoService cryptoService)
           : base(options)
    {
        _cryptoService = cryptoService;
    }

    // DbSets
    public DbSet<Socio> Socios { get; set; }
    public DbSet<PlanesMembresia> PlanesMembresia { get; set; }
    public DbSet<Pagos> Pagos { get; set; }
    public DbSet<Asistencia> Asistencias { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Socio
        modelBuilder.Entity<Socio>(entity =>
        {
            entity.HasQueryFilter(m => !m.IsDeleted);
            entity.HasKey(m => m.Id);


            entity.Property(m => m.Nombre)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.Apellido)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.DniEncrypted).IsRequired();
            
            entity.Property(m => m.FechaRegistro)
                .IsRequired();

            entity.Property(m => m.CreditosRestantes)
                .HasDefaultValue(0);

            entity.Property(m => m.TotalCreditosComprados)
                .HasDefaultValue(0);

            entity.Property(m => m.ExpiracionMembresia)
                .IsRequired();

            // Índices únicos
            entity.HasIndex(m => m.DniEncrypted)
                .IsUnique();

            // Relaciones
            entity.HasMany(m => m.Pagos)
                .WithOne(p => p.Socio)
                .HasForeignKey(p => p.SocioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Asistencias)
                .WithOne(a => a.Socio)
                .HasForeignKey(a => a.SocioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de PlanMembresia
        modelBuilder.Entity<PlanesMembresia>(entity =>
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

            entity.Property(pm => pm.DiasPorSemana)
                .IsRequired();

            entity.Property(pm => pm.IsActive)
                .HasDefaultValue(true);
        });

        // Configuración de Pago
        modelBuilder.Entity<Pagos>(entity =>
        {
            entity.HasQueryFilter(p => !p.IsDeleted);
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Precio)
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");

            entity.Property(p => p.FechaPago)
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
            entity.HasOne(p => p.Socio)
                .WithMany(m => m.Pagos)
                .HasForeignKey(p => p.SocioId)
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
            entity.HasOne(a => a.Socio)
                .WithMany(m => m.Asistencias)
                .HasForeignKey(a => a.SocioId)
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

            entity.Property(u => u.EmailEncrypted)
                .HasMaxLength(100);

            entity.Property(u => u.FullName)
                .HasMaxLength(100);

            entity.Property(u => u.IsActive)
                .HasDefaultValue(true);

            // Índices únicos
            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.HasIndex(u => u.EmailEncrypted)
                .IsUnique();
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        HandleEncryption();

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        HandleEncryption();

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

    private void HandleEncryption()
    {
        foreach (var entry in ChangeTracker.Entries<IEncryptableEntity>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.HandleEncryption(_cryptoService);
            }
        }
    }
}