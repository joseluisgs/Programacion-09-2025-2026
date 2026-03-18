using Microsoft.EntityFrameworkCore;
using EntityCoreGuidRepository.Models;

namespace EntityCoreGuidRepository.Data;

/// <summary>
/// DbContext para EF Core InMemory.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Persona> Personas { get; set; } = null!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
