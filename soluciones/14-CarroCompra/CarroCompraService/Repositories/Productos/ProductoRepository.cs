using Microsoft.EntityFrameworkCore;
using CarroCompraService.Data;
using CarroCompraService.Models.Productos;
using CarroCompraService.Config;
using CarroCompraService.Repositories.Common;
using Serilog;

namespace CarroCompraService.Repositories.Productos;

/// <summary>
/// Repositorio de Productos usando Entity Framework Core.
/// </summary>
public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger _logger = Log.ForContext<ProductoRepository>();

    public ProductoRepository(AppDbContext context)
    {
        _context = context;
        Initialize();
        SeedData();
    }

    private void Initialize()
    {
        if (AppConfig.DropData)
        {
            _logger.Information("Borrando datos de Productos...");
            _context.Productos.RemoveRange(_context.Productos);
            _context.SaveChanges();
        }

        if (AppConfig.CreateTable)
        {
            _logger.Information("Creando tabla de Productos...");
            _context.Database.EnsureCreated();
        }
    }

    private void SeedData()
    {
        if (AppConfig.SeedData && !_context.Productos.Any())
        {
            _logger.Information("Insertando datos de semilla de Productos...");
            var seedData = Factories.ProductoFactory.Seed();
            _context.Productos.AddRange(seedData);
            _context.SaveChanges();
            _logger.Information("SeedData completado. Total: {Count}", _context.Productos.Count());
        }
    }

    public IEnumerable<Producto> GetAll()
    {
        _logger.Debug("GetAll ejecutado");
        return _context.Productos.IgnoreQueryFilters().OrderBy(p => p.Id).ToList();
    }

    public Producto? GetById(long id)
    {
        _logger.Debug("GetById({Id}) ejecutado", id);
        return _context.Productos.IgnoreQueryFilters().FirstOrDefault(p => p.Id == id);
    }

    public Producto? Create(Producto producto)
    {
        producto.CreatedAt = DateTime.Now;
        producto.UpdatedAt = DateTime.Now;
        producto.Deleted = false;

        _context.Productos.Add(producto);
        _context.SaveChanges();
        
        _logger.Information("Create: Producto {Id} creado - {Nombre}", producto.Id, producto.Nombre);
        return producto;
    }

    public Producto? Update(long id, Producto producto)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        existing.Nombre = producto.Nombre;
        existing.Precio = producto.Precio;
        existing.Stock = producto.Stock;
        existing.Categoria = producto.Categoria;
        existing.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
        _logger.Information("Update: Producto {Id} actualizado", id);
        return existing;
    }

    public Producto? Delete(long id, bool logical = true)
    {
        var producto = GetById(id);
        if (producto == null) return null;

        if (logical)
        {
            producto.Deleted = true;
            producto.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
            _logger.Information("Delete Logical: Producto {Id} marcado como eliminado", id);
            return producto;
        }
        else
        {
            _context.Productos.Remove(producto);
            _context.SaveChanges();
            _logger.Information("Delete Physical: Producto {Id} eliminado físicamente", id);
            return producto;
        }
    }

    public IEnumerable<Producto> GetByCategoria(Categoria categoria)
    {
        _logger.Debug("GetByCategoria({Categoria}) ejecutado", categoria);
        return _context.Productos.IgnoreQueryFilters()
            .Where(p => p.Categoria == categoria)
            .OrderBy(p => p.Id)
            .ToList();
    }

    public IEnumerable<Producto> GetByNombre(string nombre)
    {
        _logger.Debug("GetByNombre({Nombre}) ejecutado", nombre);
        return _context.Productos.IgnoreQueryFilters()
            .Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()))
            .OrderBy(p => p.Id)
            .ToList();
    }

    public IEnumerable<Producto> GetAllPaginated(int page, int size)
    {
        _logger.Debug("GetAllPaginated(page={Page}, size={Size}) ejecutado", page, size);
        return _context.Productos
            .OrderBy(p => p.Id)
            .Skip(page * size)
            .Take(size)
            .ToList();
    }

    public Producto? FindByNombre(string nombre)
    {
        _logger.Debug("FindByNombre({Nombre}) ejecutado", nombre);
        return _context.Productos.IgnoreQueryFilters()
            .FirstOrDefault(p => p.Nombre.ToLower() == nombre.ToLower());
    }
}
