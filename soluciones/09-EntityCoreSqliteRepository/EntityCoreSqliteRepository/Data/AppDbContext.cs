using Microsoft.EntityFrameworkCore;
using EntityCoreSqliteRepository.Models;

namespace EntityCoreSqliteRepository.Data;

/// <summary>
/// DbContext para SQLite en archivo.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Persona> Personas { get; set; } = null!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Persona>()
            .HasIndex(p => p.Email)
            .IsUnique();
    }
}
