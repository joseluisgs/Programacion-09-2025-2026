using Microsoft.EntityFrameworkCore;
using CarroCompraService.Data;
using CarroCompraService.Models.Ventas;
using CarroCompraService.Config;
using CarroCompraService.Repositories.Common;
using Serilog;

namespace CarroCompraService.Repositories.Ventas;

/// <summary>
/// Repositorio de Ventas usando Entity Framework Core.
/// </summary>
public class VentaRepository : IVentaRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger _logger = Log.ForContext<VentaRepository>();

    public VentaRepository(AppDbContext context)
    {
        _context = context;
        Initialize();
    }

    private void Initialize()
    {
        if (AppConfig.DropData)
        {
            _logger.Information("Borrando datos de Ventas...");
            _context.LineasVentas.RemoveRange(_context.LineasVentas);
            _context.Ventas.RemoveRange(_context.Ventas);
            _context.SaveChanges();
        }

        if (AppConfig.CreateTable)
        {
            _logger.Information("Creando tabla de Ventas...");
            _context.Database.EnsureCreated();
        }
    }

    public IEnumerable<Venta> GetAll()
    {
        _logger.Debug("GetAll ejecutado");
        var ventas = _context.Ventas.IgnoreQueryFilters()
            .OrderByDescending(v => v.CreatedAt)
            .ToList();
        
        foreach (var venta in ventas)
        {
            venta.Lineas = _context.LineasVentas
                .Where(l => l.VentaId == venta.Id)
                .ToList();
        }
        
        return ventas;
    }

    public Venta? GetById(Guid id)
    {
        _logger.Debug("GetById({Id}) ejecutado", id);
        var venta = _context.Ventas.IgnoreQueryFilters()
            .FirstOrDefault(v => v.Id == id);
        
        if (venta != null)
        {
            venta.Lineas = _context.LineasVentas
                .Where(l => l.VentaId == id)
                .ToList();
        }
        
        return venta;
    }

    public Venta? Create(Venta venta)
    {
        venta.CreatedAt = DateTime.Now;
        venta.UpdatedAt = DateTime.Now;
        venta.Deleted = false;

        _context.Ventas.Add(venta);
        
        foreach (var linea in venta.Lineas)
        {
            linea.VentaId = venta.Id;
            _context.LineasVentas.Add(linea);
        }
        
        _context.SaveChanges();
        
        _logger.Information("Create: Venta {Id} creada - {Total}", venta.Id, venta.Total);
        return venta;
    }

    public Venta? Update(Guid id, Venta venta)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        existing.ClienteId = venta.ClienteId;
        existing.ClienteNombre = venta.ClienteNombre;
        existing.UpdatedAt = DateTime.Now;

        var lineasAnteriores = _context.LineasVentas.Where(l => l.VentaId == id);
        _context.LineasVentas.RemoveRange(lineasAnteriores);
        
        foreach (var linea in venta.Lineas)
        {
            linea.VentaId = id;
            _context.LineasVentas.Add(linea);
        }

        _context.SaveChanges();
        _logger.Information("Update: Venta {Id} actualizada", id);
        
        return GetById(id);
    }

    public Venta? Delete(Guid id, bool logical = true)
    {
        var venta = GetById(id);
        if (venta == null) return null;

        if (logical)
        {
            venta.Deleted = true;
            venta.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
            _logger.Information("Delete Logical: Venta {Id} marcada como eliminada", id);
            return venta;
        }
        else
        {
            var lineas = _context.LineasVentas.Where(l => l.VentaId == id);
            _context.LineasVentas.RemoveRange(lineas);
            
            _context.Ventas.Remove(venta);
            _context.SaveChanges();
            
            _logger.Information("Delete Physical: Venta {Id} eliminada físicamente", id);
            return venta;
        }
    }

    public IEnumerable<Venta> GetByClienteId(long clienteId)
    {
        _logger.Debug("GetByClienteId({ClienteId}) ejecutado", clienteId);
        var ventas = _context.Ventas.IgnoreQueryFilters()
            .Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.CreatedAt)
            .ToList();
        
        foreach (var venta in ventas)
        {
            venta.Lineas = _context.LineasVentas
                .Where(l => l.VentaId == venta.Id)
                .ToList();
        }
        
        return ventas;
    }
}
