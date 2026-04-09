using System.Text.Json;
using GestionAcademica.Config;
using GestionAcademica.Entity;
using GestionAcademica.Factories.Personas;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories.Common;
using GestionAcademica.Repositories.Personas.Base;
using Serilog;

namespace GestionAcademica.Repositories.Personas.Json;

/// <summary>
/// Repositorio JSON para la gestión de Personas.
/// Internamente usa Entity para almacenar, y Mapper para convertir.
/// </summary>
public class PersonasJsonRepository : IPersonasRepository
{
    private readonly ILogger _logger = Log.ForContext<PersonasJsonRepository>();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    private readonly string _filePath;
    private int _idCounter = 0;
    private readonly Dictionary<int, PersonaEntity> _porId = new();
    private readonly Dictionary<string, int> _dniIndex = new();

    public PersonasJsonRepository() : this(AppConfig.DropData, AppConfig.SeedData) { }

    private PersonasJsonRepository(bool dropData, bool seedData)
    {
        _logger.Debug("Inicializando repositorio JSON.");
        _filePath = Path.Combine(AppConfig.DataFolder, "academia.json");
        EnsureDirectory();

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
        else if (File.Exists(_filePath))
        {
            Load();
        }
    }


    private void EnsureDirectory()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_filePath)) return;

            var json = File.ReadAllText(_filePath);
            var entities = JsonSerializer.Deserialize<List<PersonaEntity>>(json, _jsonOptions);
            
            if (entities == null) return;

            foreach (var e in entities)
            {
                _porId[e.Id] = e;
                _dniIndex[e.Dni] = e.Id;
                if (e.Id > _idCounter) _idCounter = e.Id;
            }

            _logger.Information("Cargados {count} registros desde JSON.", _porId.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al cargar el archivo JSON.");
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_porId.Values.ToList(), _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al guardar el archivo JSON.");
        }
    }

    public IEnumerable<Persona> GetAll()
    {
        return _porId.Values.Where(e => !e.IsDeleted).ToModel();
    }

    public Persona? GetById(int id)
    {
        return _porId.GetValueOrDefault(id).ToModel();
    }

    public Persona? GetByDni(string dni)
    {
        return _dniIndex.TryGetValue(dni, out var id) && _porId.TryGetValue(id, out var entity)
            ? entity.ToModel()
            : null;
    }

    public bool ExisteDni(string dni)
    {
        return _dniIndex.ContainsKey(dni);
    }

    public Persona? Create(Persona model)
    {
        if (ExisteDni(model.Dni)) return null;

        var entity = model.ToEntity();
        entity.Id = ++_idCounter;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        _porId[entity.Id] = entity;
        _dniIndex[entity.Dni] = entity.Id;

        Save();
        return entity.ToModel();
    }

    public Persona? Update(int id, Persona model)
    {
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

        if (actual.Dni != entity.Dni)
        {
            _dniIndex.Remove(actual.Dni);
            _dniIndex[entity.Dni] = id;
        }

        Save();
        return entity.ToModel();
    }

    public Persona? Delete(int id)
    {
        if (!_porId.TryGetValue(id, out var entity)) return null;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        
        Save();
        return entity.ToModel();
    }

    public bool DeleteAll()
    {
        _porId.Clear();
        _dniIndex.Clear();
        _idCounter = 0;

        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }

        _logger.Information("Repositorio JSON limpiado.");
        return true;
    }
}
