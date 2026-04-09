using Microsoft.EntityFrameworkCore;
using GestionAcademica.Entity;

namespace GestionAcademica.Entity;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos SQLite.
/// </summary>
public class AppDbContext(string connectionString) : DbContext {
    public DbSet<PersonaEntity> Personas { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString);
    }
}
