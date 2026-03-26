using Microsoft.EntityFrameworkCore;
using PersonaService.Models;
using PersonaService.Data;
using PersonaService.Config;
using PersonaService.Factories;
using Serilog;

namespace PersonaService.Repositories;

public class PersonaRepositoryEfCore : IPersonaRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger _logger = Log.ForContext<PersonaRepositoryEfCore>();

    public PersonaRepositoryEfCore(AppDbContext context)
    {
        _context = context;
        Initialize();
        SeedData();
    }

    private void Initialize()
    {
        if (AppConfig.CreateTable)
        {
            _logger.Information("Creando tabla si no existe...");
            _context.Database.EnsureCreated();
        }

        if (AppConfig.DropData)
        {
            _logger.Information("Borrando datos existentes...");
            _context.Personas.RemoveRange(_context.Personas);
            _context.SaveChanges();
        }
    }

    private void SeedData()
    {
        if (AppConfig.SeedData && !_context.Personas.Any())
        {
            _logger.Information("Insertando datos de semilla...");
            var seedData = PersonaFactory.Seed();
            _context.Personas.AddRange(seedData);
            _context.SaveChanges();
            _logger.Information("SeedData completado. Total registros: {Count}", _context.Personas.Count());
        }
    }

    public IEnumerable<Persona> GetAll()
    {
        _logger.Debug("GetAll ejecutado");
        return _context.Personas.OrderBy(p => p.Id).ToList();
    }

    public Persona? GetById(int id)
    {
        _logger.Debug("GetById({Id}) ejecutado", id);
        return _context.Personas.FirstOrDefault(p => p.Id == id);
    }

    public Persona? Create(Persona persona)
    {
        persona.CreatedAt = DateTime.Now;
        persona.UpdatedAt = DateTime.Now;
        persona.IsDeleted = false;

        _context.Personas.Add(persona);
        _context.SaveChanges();
        
        _logger.Information("Create: Persona {Id} creada - {Nombre}", persona.Id, persona.Nombre);
        return persona;
    }

    public Persona? Update(int id, Persona persona)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        existing.Nombre = persona.Nombre;
        existing.Email = persona.Email;
        existing.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
        _logger.Information("Update: Persona {Id} actualizada - {Nombre}", id, existing.Nombre);
        return existing;
    }

    public Persona? Delete(int id, bool logical = true)
    {
        var persona = GetById(id);
        if (persona == null) return null;

        if (logical)
        {
            persona.IsDeleted = true;
            persona.DeletedAt = DateTime.Now;
            persona.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
            _logger.Information("Delete Logical: Persona {Id} marcada como eliminada", id);
            return persona;
        }
        else
        {
            _context.Personas.Remove(persona);
            _context.SaveChanges();
            _logger.Information("Delete Physical: Persona {Id} eliminada físicamente", id);
            return persona;
        }
    }

    public Persona? FindByEmail(string email)
    {
        _logger.Debug("FindByEmail({Email}) ejecutado", email);
        return _context.Personas.FirstOrDefault(p => p.Email == email);
    }
}
