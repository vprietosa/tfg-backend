using Microsoft.EntityFrameworkCore;
using TfgApi.Models;

namespace TfgApi.Data;

public class TfgDbContext : DbContext
{
    public TfgDbContext(DbContextOptions<TfgDbContext> options) : base(options)
    {
    }

    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<Autonomo> Autonomos => Set<Autonomo>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Practica> Practicas => Set<Practica>();
    public DbSet<PracticaRealizada> PracticasRealizadas => Set<PracticaRealizada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Si en SQL tus tablas se llaman exactamente así, no hace falta.
        // Pero lo dejo por si quieres asegurarlo (y evitar que EF cambie nombres).
        modelBuilder.Entity<Alumno>().ToTable("Alumnos");
        modelBuilder.Entity<Autonomo>().ToTable("Autonomos");
        modelBuilder.Entity<Empresa>().ToTable("Empresas");
        modelBuilder.Entity<Practica>().ToTable("Practicas");
        modelBuilder.Entity<PracticaRealizada>().ToTable("PracticasRealizadas");

        // CHECK: una práctica pertenece a Empresa o Autonomo (en SQL ya lo tienes),
        // aquí solo lo reforzamos a nivel de EF si te interesa (opcional).
        modelBuilder.Entity<Practica>()
            .HasCheckConstraint("CK_Practicas_Origen",
                "([EmpresaId] IS NOT NULL AND [AutonomoId] IS NULL) OR ([EmpresaId] IS NULL AND [AutonomoId] IS NOT NULL)");

        // UNIQUE (AlumnoId, PracticaId) en PracticasRealizadas (en SQL ya lo tienes).
        modelBuilder.Entity<PracticaRealizada>()
            .HasIndex(x => new { x.AlumnoId, x.PracticaId })
            .IsUnique();
    }
}
