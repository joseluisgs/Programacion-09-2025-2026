using Microsoft.EntityFrameworkCore;
using GestionAcademica.Entity;

namespace GestionAcademica.Entity;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos SQLite.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<PersonaEntity> Personas { get; set; } = null!;

    private readonly string _connectionString;

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }
}
