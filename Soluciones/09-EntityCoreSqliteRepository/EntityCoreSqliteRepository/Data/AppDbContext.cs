using EntityCoreSqliteRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityCoreSqliteRepository.Data;

/// <summary>
/// DbContext para SQLite en archivo.
/// </summary>
///
// En este ejemplo, se muestra la forma A, usando el constructor con opciones. Si quieres usar la forma B, simplemente comenta esta clase y descomenta la clase alternativa que no tiene constructor con parámetros.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<Persona> Personas { get; set; } = null!;
}

// Forma B: Constructor sin parámetros y configuración dentro de OnConfiguring
/*public class AppDbContext : DbContext
{
    public DbSet<Persona> Personas { get; set; } = null!;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=personas.db");
        }
    }
}*/