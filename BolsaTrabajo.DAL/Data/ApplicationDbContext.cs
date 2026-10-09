using BolsaTrabajo.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BolsaTrabajo.DAL.Data;

// Contexto de base de datos que incluye las tablas de Identity (AspNetUsers, AspNetRoles, etc.)
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<Usuario>(options)
{
    public DbSet<Plaza> Plazas { get; set; }
    public DbSet<Postulacion> Postulaciones { get; set; }
    public DbSet<HojaDeVida> HojasDeVida { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ================================================
        // PLAZAS
        // ================================================
        modelBuilder.Entity<Plaza>(entidad =>
        {
            entidad.ToTable("Plazas");
            entidad.Property(p => p.FechaPublicacion).HasColumnType("date");
            entidad.Property(p => p.FechaCierre).HasColumnType("date");

            // Un agente publica muchas plazas; no se permite eliminar un agente con plazas
            entidad.HasOne(p => p.Agente)
                .WithMany(u => u.PlazasPublicadas)
                .HasForeignKey(p => p.IdAgente)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ================================================
        // POSTULACIONES
        // ================================================
        modelBuilder.Entity<Postulacion>(entidad =>
        {
            entidad.ToTable("Postulaciones");

            entidad.HasOne(p => p.Plaza)
                .WithMany(p => p.Postulaciones)
                .HasForeignKey(p => p.IdPlaza)
                .OnDelete(DeleteBehavior.Cascade);

            entidad.HasOne(p => p.Candidato)
                .WithMany(u => u.Postulaciones)
                .HasForeignKey(p => p.IdCandidato)
                .OnDelete(DeleteBehavior.Restrict);

            // Un candidato solo puede postularse una vez a la misma plaza
            entidad.HasIndex(p => new { p.IdPlaza, p.IdCandidato }).IsUnique();
        });

        // ================================================
        // HOJAS DE VIDA
        // ================================================
        modelBuilder.Entity<HojaDeVida>(entidad =>
        {
            entidad.ToTable("HojasDeVida");

            // Cada usuario tiene una sola hoja de vida
            entidad.HasOne(h => h.Usuario)
                .WithOne(u => u.HojaDeVida)
                .HasForeignKey<HojaDeVida>(h => h.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            entidad.HasIndex(h => h.IdUsuario).IsUnique();
        });
    }
}
