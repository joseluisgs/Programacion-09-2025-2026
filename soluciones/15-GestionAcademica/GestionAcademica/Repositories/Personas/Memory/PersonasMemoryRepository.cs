using GestionAcademica.Config;
using GestionAcademica.Entity;
using GestionAcademica.Factories.Personas;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories.Common;
using GestionAcademica.Repositories.Personas.Base;
using Serilog;

namespace GestionAcademica.Repositories.Personas.Memory;

/// <summary>
/// Repositorio en memoria para la gestión de Personas.
/// Internamente usa Entity para almacenar, y Mapper para convertir.
/// </summary>
public class PersonasMemoryRepository : IPersonasRepository
{
    private readonly ILogger _logger = Log.ForContext<PersonasMemoryRepository>();
    private int _idCounter = 0;
    private readonly Dictionary<int, PersonaEntity> _porId = new();
    private readonly Dictionary<string, int> _dniIndex = new();

    public PersonasMemoryRepository() : this(AppConfig.DropData, AppConfig.SeedData) { }

    private PersonasMemoryRepository(bool dropData, bool seedData)
    {
        if (dropData)
        {
            _logger.Warning("Borrando datos...");
            DeleteAll();
        }

        if (seedData)
        {
            _logger.Information("Cargando datos de semilla...");
            foreach (var persona in PersonasFactory.Seed())
            {
                Create(persona);
            }
            _logger.Information("SeedData completado.");
        }
    }


    /// <inheritdoc cref="IPersonasRepository.GetAll" />
    public IEnumerable<Persona> GetAll()
    {
        _logger.Debug("Obteniendo todas las personas");
        return _porId.Values.ToModel();
    }

    /// <inheritdoc cref="ICrudRepository{TKey,TEntity}.GetById" />
    public Persona? GetById(int id)
    {
        _logger.Debug($"Obteniendo persona con id {id}");
        return _porId.GetValueOrDefault(id).ToModel();
    }

    /// <inheritdoc cref="IPersonasRepository.GetByDni" />
    public Persona? GetByDni(string dni)
    {
        _logger.Debug($"Obteniendo persona con DNI {dni}");
        return _dniIndex.TryGetValue(dni, out var id) && _porId.TryGetValue(id, out var entity)
            ? entity.ToModel()
            : null;
    }

    /// <inheritdoc cref="IPersonasRepository.ExisteDni" />
    public bool ExisteDni(string dni)
    {
        return _dniIndex.ContainsKey(dni);
    }

    /// <inheritdoc cref="ICrudRepository{int, Persona}.Create" />
    public Persona? Create(Persona model)
    {
        _logger.Debug("Creando nueva persona {entity}", model);
        if (ExisteDni(model.Dni)) return null;

        var entity = model.ToEntity();
        entity.Id = ++_idCounter;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        _porId[entity.Id] = entity;
        _dniIndex[entity.Dni] = entity.Id;
        return entity.ToModel();
    }

    /// <inheritdoc cref="ICrudRepository{int, Persona}.Update" />
    public Persona? Update(int id, Persona model)
    {
        _logger.Debug("Actualizando persona con Id {Id} con datos {Persona}", id, model);

        if (!_porId.TryGetValue(id, out var actual)) return null;

        // Si cambió el DNI, verificar que no exista en otra persona
        if (model.Dni != actual.Dni && _dniIndex.TryGetValue(model.Dni, out var otroId) && otroId != id) {
            _logger.Warning("No se puede actualizar persona con id {Id} porque el DNI {Dni} ya está en uso por otra persona", id, model.Dni);
            return null;
        }

        var entity = model.ToEntity();
        entity.Id = id;
        entity.CreatedAt = actual.CreatedAt;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        _porId[id] = entity;

        if (actual.Dni != entity.Dni) {
            _dniIndex.Remove(actual.Dni);
            _dniIndex[entity.Dni] = id;
        }

        return entity.ToModel();
    }

    /// <inheritdoc cref="ICrudRepository{int, Persona}.Delete" />
    public Persona? Delete(int id)
    {
        _logger.Debug($"Eliminando persona con id {id}");

        if (!_porId.Remove(id, out var entity)) return null;

        _dniIndex.Remove(entity.Dni);

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        return entity.ToModel();
    }

    /// <inheritdoc cref="IPersonasRepository.DeleteAll" />
    public bool DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        _porId.Clear();
        _dniIndex.Clear();
        _idCounter = 0;
        return true;
    }
}
