using Microsoft.EntityFrameworkCore;
using EntityCoreGuidRepository.Models;
using EntityCoreGuidRepository.Data;
using EntityCoreGuidRepository.Factories;

namespace EntityCoreGuidRepository.Repositories;

/// <summary>
/// Repositorio implementado con Entity Framework Core InMemory.
/// Usa clave GUID.
/// </summary>
public class PersonaRepository : IPersonaRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Constructor que recibe el contexto y crea la tabla y datos iniciales.
    /// </summary>
    /// <param name="context">DbContext de EF Core.</param>
    public PersonaRepository(AppDbContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
        SeedData();
    }

    /// <summary>
    /// Carga los datos iniciales de prueba.
    /// </summary>
    private void SeedData()
    {
        if (_context.Personas.Any()) return;
        
        _context.Personas.AddRange(PersonaFactory.Seed());
        _context.SaveChanges();
    }

    /// <inheritdoc/>
    public IEnumerable<Persona> GetAll()
    {
        return _context.Personas
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public Persona? GetById(Guid id)
    {
        return _context.Personas
            .FirstOrDefault(p => p.Id == id);
    }

    /// <inheritdoc/>
    public Persona? Create(Persona persona)
    {
        persona.CreatedAt = DateTime.Now;
        persona.UpdatedAt = DateTime.Now;
        persona.IsDeleted = false;

        _context.Personas.Add(persona);
        _context.SaveChanges();
        
        return persona;
    }

    /// <inheritdoc/>
    public Persona? Update(Guid id, Persona persona)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        existing.Nombre = persona.Nombre;
        existing.Email = persona.Email;
        existing.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
        return existing;
    }

    /// <inheritdoc/>
    public Persona? Delete(Guid id, bool logical = true)
    {
        var persona = GetById(id);
        if (persona == null) return null;

        if (logical)
        {
            persona.IsDeleted = true;
            persona.DeletedAt = DateTime.Now;
            persona.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
            return persona;
        }
        else
        {
            _context.Personas.Remove(persona);
            _context.SaveChanges();
            return persona;
        }
    }
}
