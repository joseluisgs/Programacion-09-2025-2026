using Microsoft.EntityFrameworkCore;
using EntityCoreAutonumericRepository.Models;

namespace EntityCoreAutonumericRepository.Data;

/// <summary>
/// DbContext para SQLite en memoria.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Persona> Personas { get; set; } = null!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
