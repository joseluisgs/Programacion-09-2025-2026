using Microsoft.EntityFrameworkCore;
using CarroCompraService.Models.Productos;
using CarroCompraService.Models.Clientes;
using CarroCompraService.Models.Ventas;

namespace CarroCompraService.Data;

/// <summary>
/// DbContext de Entity Framework Core.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Venta> Ventas { get; set; } = null!;
    public DbSet<LineaVenta> LineasVentas { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Categoria).HasConversion<string>();
            entity.HasQueryFilter(e => !e.Deleted);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Direccion).IsRequired().HasMaxLength(200);
            entity.HasQueryFilter(e => !e.Deleted);
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClienteNombre).IsRequired().HasMaxLength(100);
            entity.HasQueryFilter(e => !e.Deleted);
        });

        modelBuilder.Entity<LineaVenta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductoNombre).IsRequired().HasMaxLength(100);
        });
    }
}
