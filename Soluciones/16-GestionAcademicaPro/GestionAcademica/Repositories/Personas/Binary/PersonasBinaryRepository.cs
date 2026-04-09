using System.Text;
using GestionAcademica.Config;
using GestionAcademica.Entity;
using GestionAcademica.Factories.Personas;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories.Personas.Base;
using Serilog;

namespace GestionAcademica.Repositories.Personas.Binary;

/// <summary>
///     Repositorio binario para la gestión de Personas.
///     ALGORITMO con LINQ:
///     - FindFreeSpace: usa LINQ para encontrar el mejor hueco (best-fit)
///     - CalcularFragmentacion: usa LINQ para sumar tamaños
///     - Compactar: usa LINQ para reconstruir el archivo
///     NOTA PARA EL ALUMNO: Best-fit minimiza el desperdicio de espacio.
/// </summary>
public class PersonasBinaryRepository : IPersonasRepository
{
    private readonly ILogger _logger = Log.ForContext<PersonasBinaryRepository>();

    private const string DataFileName = "academia-db.dat";
    private const string IndexFileName = "academia-db.idx";
    private const string FragmentsFileName = "academia-db.frag";

    private const string MagicNumber = "ACAD";
    private const string MagicNumberIndex = "ACDI";
    private const string MagicNumberFragments = "ACFR";
    private const int CurrentVersion = 1;

    private const double FragmentationThreshold = 0.3;

    private int _idCounter = 0;
    private readonly Dictionary<int, long> _idIndex = new();
    private readonly Dictionary<string, int> _dniIndex = new();
    private readonly Dictionary<int, long> _sizeIndex = new();
    private readonly Dictionary<long, long> _fragments = new();

    public PersonasBinaryRepository() : this(AppConfig.DropData, AppConfig.SeedData) { }

    private PersonasBinaryRepository(bool dropData, bool seedData)
    {
        _logger.Debug("Inicializando el motor de persistencia binaria.");
        EnsureDataFolder();
        InitializeFile();

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
        else
        {
            LoadIndex();
            LoadFragments();
        }
    }


    private string DataFilePath => Path.Combine(AppConfig.DataFolder, DataFileName);
    private string IndexFilePath => Path.Combine(AppConfig.DataFolder, IndexFileName);
    private string FragmentsFilePath => Path.Combine(AppConfig.DataFolder, FragmentsFileName);

    private void EnsureDataFolder()
    {
        if (!Directory.Exists(AppConfig.DataFolder))
        {
            Directory.CreateDirectory(AppConfig.DataFolder);
        }
    }

    private void InitializeFile()
    {
        try
        {
            if (!File.Exists(DataFilePath))
            {
                _logger.Information("Creando base de datos binaria v{CurrentVersion}.", CurrentVersion);
                using var stream = File.Create(DataFilePath);
                using var writer = new BinaryWriter(stream, Encoding.UTF8);
                writer.Write(Encoding.ASCII.GetBytes(MagicNumber));
                writer.Write(CurrentVersion);
            }
            else
            {
                using var stream = File.OpenRead(DataFilePath);
                using var reader = new BinaryReader(stream, Encoding.UTF8);
                if (stream.Length < 8)
                {
                    throw new InvalidOperationException("Archivo corrupto o demasiado pequeño.");
                }
                var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                if (magic != MagicNumber)
                {
                    throw new InvalidOperationException("La firma del archivo no coincide con nuestra base de datos.");
                }
                var version = reader.ReadInt32();
                if (version != CurrentVersion)
                {
                    throw new InvalidOperationException($"Versión de archivo no soportada ({version}).");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al inicializar el almacenamiento binario.");
            throw;
        }
    }

    /// <summary>
    ///     Busca el mejor hueco libre para el tamaño requerido.
    ///     ALGORITMO LINQ:
    ///     1. Filtramos los huecos cuyo tamaño sea >= al requerido
    ///     2. Los ordenamos por tamaño (menor primero) para encontrar el mejor ajuste (best-fit)
    ///     3. Tomamos el primero que cumpla
    ///     NOTA PARA EL ALUMNO: Best-fit minimiza el desperdicio de espacio.
    /// </summary>
    private long FindFreeSpace(long requiredSize)
    {
        // LINQ: Buscar el hueco más pequeño que sea suficiente
        var bestHole = _fragments
            .Where(kvp => kvp.Value >= requiredSize)
            .OrderBy(kvp => kvp.Value)
            .FirstOrDefault();

        var holePos = bestHole.Key;

        if (holePos == default || !_fragments.ContainsKey(holePos))
            return -1;

        var holeSize = _fragments[holePos];
        _fragments.Remove(holePos);

        if (holeSize > requiredSize + 10)
        {
            long remainingPos = holePos + requiredSize;
            long remainingSize = holeSize - requiredSize;
            _fragments[remainingPos] = remainingSize;
        }

        return holePos;
    }

    /// <summary>
    ///     Calcula el porcentaje de fragmentación del archivo.
    ///     Fórmula: (suma de tamaños de huecos) / (tamaño total del archivo)
    ///     ALGORITMO LINQ: Usa Sum() para calcular el tamaño total de huecos.
    /// </summary>
    private double CalcularFragmentacion()
    {
        if (!File.Exists(DataFilePath))
            return 0;

        var tamanoArchivo = new FileInfo(DataFilePath).Length;
        if (tamanoArchivo == 0)
            return 0;

        // LINQ: Sumar todos los tamaños de huecos
        var tamanoHuecos = _fragments.Values.Sum();

        return (double)tamanoHuecos / tamanoArchivo;
    }

    /// <summary>
    ///     Compacta el archivo de datos eliminando todos los huecos.
    ///     ALGORITMO:
    ///     1. Leer todos los registros existentes
    ///     2. Crear un nuevo archivo de datos vacío
    ///     3. Escribir todos los registros secuencialmente (sin huecos)
    ///     4. Actualizar el índice con las nuevas posiciones
    ///     5. Vaciar la lista de huecos
    /// </summary>
    private void Compactar()
    {
        if (_idIndex.Count == 0)
            return;

        _logger.Warning("Compactando archivo binario por alta fragmentación...");

        // LINQ: Obtener todos los registros
        var registros = _idIndex.Keys
            .Select(id => (Id: id, Entity: ReadPersonaAt(_idIndex[id])))
            .OfType<(int Id, PersonaEntity Entity)>()
            .ToDictionary(x => x.Id, x => x.Entity);

        using var stream = File.Create(DataFilePath);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        writer.Write(Encoding.ASCII.GetBytes(MagicNumber));
        writer.Write(CurrentVersion);

        _idIndex.Clear();
        _fragments.Clear();

        foreach (var (id, entity) in registros)
        {
            var position = stream.Position;
            var entityConId = new PersonaEntity
            {
                Id = id,
                Dni = entity.Dni,
                Nombre = entity.Nombre,
                Apellidos = entity.Apellidos,
                Tipo = entity.Tipo,
                Experiencia = entity.Experiencia,
                Especialidad = entity.Especialidad,
                Ciclo = entity.Ciclo,
                Curso = entity.Curso,
                Calificacion = entity.Calificacion,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted
            };
            EscribirPersonaBinario(writer, entityConId);
            _idIndex[id] = position;
            _sizeIndex[id] = GetPersonaSize(entityConId);
        }

        SaveIndex();
        SaveFragments();
        _logger.Information("Compactación completada.");
    }

    /// <summary>
    ///     Comprueba si la fragmentación supera el umbral y compacta si es necesario.
    ///     Se llama después de cada Delete.
    /// </summary>
    private void ComprobarFragmentacion()
    {
        if (CalcularFragmentacion() > FragmentationThreshold)
            Compactar();
    }

    private void EscribirPersonaBinario(BinaryWriter writer, PersonaEntity entity)
    {
        writer.Write(entity.Id);
        writer.Write(entity.Dni);
        writer.Write(entity.Nombre);
        writer.Write(entity.Apellidos);
        writer.Write(entity.Tipo);
        writer.Write(entity.Experiencia ?? 0);
        writer.Write(entity.Especialidad ?? string.Empty);
        writer.Write(entity.Ciclo ?? 0);
        writer.Write(entity.Curso ?? 0);
        writer.Write(entity.Calificacion ?? 0.0);
        writer.Write(entity.CreatedAt.ToString("o"));
        writer.Write(entity.UpdatedAt.ToString("o"));
        writer.Write(entity.IsDeleted);
    }

    private long WritePersona(PersonaEntity entity, long position = -1)
    {
        using var stream = File.Open(DataFilePath, FileMode.Open, FileAccess.Write);

        if (position == -1)
        {
            stream.Seek(0, SeekOrigin.End);
        }
        else
        {
            stream.Seek(position, SeekOrigin.Begin);
        }

        var actualPosition = stream.Position;
        using var writer = new BinaryWriter(stream, Encoding.UTF8);
        EscribirPersonaBinario(writer, entity);

        return actualPosition;
    }

    private PersonaEntity? ReadPersonaAt(long position)
    {
        if (!File.Exists(DataFilePath)) return null;

        using var stream = File.OpenRead(DataFilePath);
        stream.Seek(position, SeekOrigin.Begin);
        using var reader = new BinaryReader(stream, Encoding.UTF8);
        return LeerPersonaBinario(reader);
    }

    private PersonaEntity? LeerPersonaBinario(BinaryReader reader)
    {
        try
        {
            return new PersonaEntity
            {
                Id = reader.ReadInt32(),
                Dni = reader.ReadString(),
                Nombre = reader.ReadString(),
                Apellidos = reader.ReadString(),
                Tipo = reader.ReadString(),
                Experiencia = reader.ReadInt32(),
                Especialidad = reader.ReadString(),
                Ciclo = reader.ReadInt32(),
                Curso = reader.ReadInt32(),
                Calificacion = reader.ReadDouble(),
                CreatedAt = DateTime.Parse(reader.ReadString()),
                UpdatedAt = DateTime.Parse(reader.ReadString()),
                IsDeleted = reader.ReadBoolean()
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al deserializar un registro.");
            return null;
        }
    }

    private long GetPersonaSize(PersonaEntity entity)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8);
        EscribirPersonaBinario(writer, entity);
        return ms.Length;
    }

    public IEnumerable<Persona> GetAll()
    {
        _logger.Debug("Consultando repositorio binario.");

        // LINQ: Usamos LINQ para obtener y filtrar personas
        return _idIndex.Keys
            .OrderBy(id => id)
            .Select(id => GetById(id))
            .OfType<Persona>()
            .ToList();
    }

    public Persona? GetById(int id)
    {
        if (_idIndex.TryGetValue(id, out var position))
        {
            var entity = ReadPersonaAt(position);
            return entity?.ToModel();
        }
        return null;
    }

    public Persona? GetByDni(string dni)
    {
        // LINQ: Buscar en el índice de DNI
        if (_dniIndex.TryGetValue(dni, out var id))
        {
            return GetById(id);
        }
        return null;
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

        try
        {
            var size = GetPersonaSize(entity);
            var foundPosition = FindFreeSpace(size);

            var position = WritePersona(entity, foundPosition);

            _idIndex[entity.Id] = position;
            _dniIndex[entity.Dni] = entity.Id;
            _sizeIndex[entity.Id] = size;

            SaveIndex();
            if (foundPosition != -1) SaveFragments();

            return entity.ToModel();
        }
        catch
        {
            throw new InvalidOperationException("No se pudo completar la creación del registro binario.");
        }
    }

    public Persona? Update(int id, Persona model)
    {
        if (!_idIndex.TryGetValue(id, out var oldPosition)) return null;

        var oldSize = _sizeIndex[id];
        // LINQ: Buscar el DNI asociado a esta ID
        var existingDni = _dniIndex.FirstOrDefault(x => x.Value == id).Key;

        // Si cambió el DNI, verificar que no exista en otra persona
        if (model.Dni != existingDni && _dniIndex.TryGetValue(model.Dni, out var otroId) && otroId != id)
        {
            _logger.Warning("No se puede actualizar persona con id {Id} porque el DNI {Dni} ya está en uso por otra persona", id, model.Dni);
            return null;
        }

        var entity = model.ToEntity();
        entity.Id = id;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        try
        {
            var newSize = GetPersonaSize(entity);
            long finalPosition;

            if (newSize <= oldSize)
            {
                finalPosition = WritePersona(entity, oldPosition);
                if (oldSize > newSize + 10)
                {
                    _fragments[oldPosition + newSize] = oldSize - newSize;
                    SaveFragments();
                }
            }
            else
            {
                _fragments[oldPosition] = oldSize;
                var foundPosition = FindFreeSpace(newSize);
                finalPosition = WritePersona(entity, foundPosition);
                SaveFragments();
            }

            _idIndex[id] = finalPosition;
            _sizeIndex[id] = newSize;

            if (existingDni != entity.Dni)
            {
                _dniIndex.Remove(existingDni);
                _dniIndex[entity.Dni] = id;
            }

            SaveIndex();
            return entity.ToModel();
        }
        catch
        {
            throw new InvalidOperationException("No se pudo completar la actualización del registro binario.");
        }
    }

    public Persona? Delete(int id)
    {
        if (!_idIndex.TryGetValue(id, out var position)) return null;

        var entity = ReadPersonaAt(position);
        if (entity == null) return null;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        WritePersona(entity, position);

        _dniIndex.Remove(entity.Dni);

        ComprobarFragmentacion();

        return entity.ToModel();
    }

    public bool DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");

        _idIndex.Clear();
        _dniIndex.Clear();
        _sizeIndex.Clear();
        _fragments.Clear();
        _idCounter = 0;

        if (File.Exists(DataFilePath)) File.Delete(DataFilePath);
        if (File.Exists(IndexFilePath)) File.Delete(IndexFilePath);
        if (File.Exists(FragmentsFilePath)) File.Delete(FragmentsFilePath);

        InitializeFile();

        return true;
    }

    private void LoadIndex()
    {
        if (!File.Exists(IndexFilePath)) return;

        try
        {
            using var stream = File.OpenRead(IndexFilePath);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != MagicNumberIndex) return;

            reader.ReadInt32();
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var id = reader.ReadInt32();
                var position = reader.ReadInt64();
                _idIndex[id] = position;
                _idCounter++;
            }

            var dniCount = reader.ReadInt32();
            for (var i = 0; i < dniCount; i++)
            {
                var dni = reader.ReadString();
                var id = reader.ReadInt32();
                _dniIndex[dni] = id;
            }

            var sizeCount = reader.ReadInt32();
            for (var i = 0; i < sizeCount; i++)
            {
                var id = reader.ReadInt32();
                var size = reader.ReadInt64();
                if (_idIndex.ContainsKey(id))
                    _sizeIndex[id] = size;
            }

            _logger.Information("Índices binarios cargados: {Count} registros.", _idIndex.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al cargar los índices.");
        }
    }

    private void SaveIndex()
    {
        using var stream = File.Create(IndexFilePath);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        writer.Write(Encoding.ASCII.GetBytes(MagicNumberIndex));
        writer.Write(CurrentVersion);

        writer.Write(_idIndex.Count);
        foreach (var kvp in _idIndex)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }

        writer.Write(_dniIndex.Count);
        foreach (var kvp in _dniIndex)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }

        writer.Write(_sizeIndex.Count);
        foreach (var kvp in _sizeIndex)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }
    }

    private void LoadFragments()
    {
        if (!File.Exists(FragmentsFilePath)) return;

        try
        {
            using var stream = File.OpenRead(FragmentsFilePath);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != MagicNumberFragments) return;

            reader.ReadInt32();
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                _fragments[reader.ReadInt64()] = reader.ReadInt64();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al cargar fragmentos.");
        }
    }

    private void SaveFragments()
    {
        using var stream = File.Create(FragmentsFilePath);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        writer.Write(Encoding.ASCII.GetBytes(MagicNumberFragments));
        writer.Write(CurrentVersion);

        writer.Write(_fragments.Count);
        foreach (var kvp in _fragments)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }
    }
}
