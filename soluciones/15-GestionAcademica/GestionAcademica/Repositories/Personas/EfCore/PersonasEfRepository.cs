using Microsoft.EntityFrameworkCore;
using GestionAcademica.Config;
using GestionAcademica.Entity;
using GestionAcademica.Factories.Personas;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories.Personas.Base;
using Serilog;

namespace GestionAcademica.Repositories.EfCore;

/// <summary>
/// Repositorio Entity Framework Core para la gestión de Personas.
/// </summary>
public class PersonasEfRepository : IPersonasRepository
{
    private readonly ILogger _logger = Log.ForContext<PersonasEfRepository>();
    private readonly AppDbContext _context;

    public PersonasEfRepository() : this(AppConfig.DropData, AppConfig.SeedData) { }

    private PersonasEfRepository(bool dropData, bool seedData)
    {
        _logger.Debug("Inicializando repositorio EF Core.");
        _context = new AppDbContext(AppConfig.ConnectionString);

        if (dropData)
        {
            _logger.Warning("Borrando datos...");
            DeleteAll();
        }
        
        if (dropData || seedData)
        {
            _logger.Information("Cargando datos de semilla...");
            foreach (var persona in PersonasFactory.Seed())
            {
                Create(persona);
            }
            _logger.Information("SeedData completado.");
        }
    }


    public IEnumerable<Persona> GetAll()
    {
        var entities = _context.Personas.Where(p => !p.IsDeleted).ToList();
        return PersonaMapper.ToModel(entities);
    }

    public Persona? GetById(int id)
    {
        var entity = _context.Personas.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
        return PersonaMapper.ToModel(entity);
    }

    public Persona? GetByDni(string dni)
    {
        var entity = _context.Personas.FirstOrDefault(p => p.Dni == dni && !p.IsDeleted);
        return PersonaMapper.ToModel(entity);
    }

    public bool ExisteDni(string dni)
    {
        return _context.Personas.Any(p => p.Dni == dni && !p.IsDeleted);
    }

    public Persona? Create(Persona model)
    {
        if (ExisteDni(model.Dni)) return null;

        model = model with
        {
            Id = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var entity = PersonaMapper.ToEntity(model);
        _context.Personas.Add(entity);
        _context.SaveChanges();

        return GetById(entity.Id);
    }

    public Persona? Update(int id, Persona model)
    {
        var entity = _context.Personas.FirstOrDefault(p => p.Id == id);
        if (entity == null) return null;

        // Si cambió el DNI, verificar que no exista en otra persona
        if (model.Dni != entity.Dni && _context.Personas.Any(p => p.Dni == model.Dni && p.Id != id)) {
            _logger.Warning("No se puede actualizar persona con id {Id} porque el DNI {Dni} ya está en uso por otra persona", id, model.Dni);
            return null;
        }

        entity.Dni = model.Dni;
        entity.Nombre = model.Nombre;
        entity.Apellidos = model.Apellidos;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        if (model is Estudiante est)
        {
            entity.Tipo = "Estudiante";
            entity.Calificacion = est.Calificacion;
            entity.Ciclo = (int)est.Ciclo;
            entity.Curso = (int)est.Curso;
        }
        else if (model is Docente doc)
        {
            entity.Tipo = "Docente";
            entity.Experiencia = doc.Experiencia;
            entity.Especialidad = doc.Especialidad;
            entity.Ciclo = (int)doc.Ciclo;
        }

        _context.SaveChanges();
        return GetById(id);
    }

    public Persona? Delete(int id)
    {
        var entity = _context.Personas.FirstOrDefault(p => p.Id == id);
        if (entity == null) return null;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();

        return GetById(id);
    }

    public bool DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        _context.Personas.RemoveRange(_context.Personas);
        _context.SaveChanges();
        return true;
    }
}
