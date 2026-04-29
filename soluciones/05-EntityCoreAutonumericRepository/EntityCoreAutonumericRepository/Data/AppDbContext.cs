using Microsoft.EntityFrameworkCore;
using EntityCoreAutonumericRepository.Models;

namespace EntityCoreAutonumericRepository.Data;

/// <summary>
/// DbContext para SQLite en memoria.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<Persona> Personas { get; set; } = null!;
}
