using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using GestionAcademica.Config;
using GestionAcademica.Entity;
using GestionAcademica.Factories.Personas;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories.Personas.Base;
using Serilog;

namespace GestionAcademica.Repositories.Dapper;

/// <summary>
/// Repositorio Dapper para la gestión de Personas.
/// </summary>
public class PersonasDapperRepository : IPersonasRepository
{
    private readonly ILogger _logger = Log.ForContext<PersonasDapperRepository>();
    private readonly string _connectionString;

    public PersonasDapperRepository() : this(AppConfig.DropData, AppConfig.SeedData) { }

    private PersonasDapperRepository(bool dropData, bool seedData)
    {
        _logger.Debug("Inicializando repositorio Dapper.");
        _connectionString = AppConfig.ConnectionString;
        EnsureDataFolder();
        EnsureTable();

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


    private SqliteConnection CreateConnection() => new(_connectionString);

    private void EnsureDataFolder()
    {
        if (!Directory.Exists(AppConfig.DataFolder))
        {
            Directory.CreateDirectory(AppConfig.DataFolder);
        }
    }

    private void EnsureTable()
    {
        using var connection = CreateConnection();
        connection.Open();
        connection.Execute(@"
            DROP TABLE IF EXISTS Personas;
            CREATE TABLE Personas (
                Id INTEGER PRIMARY KEY,
                Dni TEXT NOT NULL UNIQUE,
                Nombre TEXT NOT NULL,
                Apellidos TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                Tipo TEXT NOT NULL DEFAULT 'Persona',
                Calificacion REAL,
                Ciclo INTEGER,
                Curso INTEGER,
                Experiencia INTEGER,
                Especialidad TEXT
            )");
    }

    public IEnumerable<Persona> GetAll()
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM Personas WHERE IsDeleted = 0";
        var entities = connection.Query<PersonaEntity>(sql).ToList();
        return PersonaMapper.ToModel(entities);
    }

    public Persona? GetById(int id)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM Personas WHERE Id = @Id AND IsDeleted = 0";
        var entity = connection.QueryFirstOrDefault<PersonaEntity>(sql, new { Id = id });
        return PersonaMapper.ToModel(entity);
    }

    public Persona? GetByDni(string dni)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM Personas WHERE Dni = @Dni AND IsDeleted = 0";
        var entity = connection.QueryFirstOrDefault<PersonaEntity>(sql, new { Dni = dni });
        return PersonaMapper.ToModel(entity);
    }

    public bool ExisteDni(string dni)
    {
        using var connection = CreateConnection();
        var sql = "SELECT COUNT(1) FROM Personas WHERE Dni = @Dni AND IsDeleted = 0";
        return connection.ExecuteScalar<int>(sql, new { Dni = dni }) > 0;
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

        using var connection = CreateConnection();
        var sql = @"INSERT INTO Personas (Dni, Nombre, Apellidos, CreatedAt, UpdatedAt, IsDeleted, Tipo, Calificacion, Ciclo, Curso, Experiencia, Especialidad)
                    VALUES (@Dni, @Nombre, @Apellidos, @CreatedAt, @UpdatedAt, @IsDeleted, @Tipo, @Calificacion, @Ciclo, @Curso, @Experiencia, @Especialidad);
                    SELECT last_insert_rowid();";

        entity.Id = connection.ExecuteScalar<int>(sql, new
        {
            entity.Dni,
            entity.Nombre,
            entity.Apellidos,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.IsDeleted,
            entity.Tipo,
            entity.Calificacion,
            entity.Ciclo,
            entity.Curso,
            entity.Experiencia,
            entity.Especialidad
        });

        return GetById(entity.Id);
    }

    public Persona? Update(int id, Persona model)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        // Si cambió el DNI, verificar que no exista en otra persona
        if (model.Dni != existing.Dni && ExisteDni(model.Dni)) {
            _logger.Warning("No se puede actualizar persona con id {Id} porque el DNI {Dni} ya está en uso por otra persona", id, model.Dni);
            return null;
        }

        model = model with
        {
            Id = id,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var entity = PersonaMapper.ToEntity(model);

        using var connection = CreateConnection();
        var sql = @"UPDATE Personas SET 
                    Dni = @Dni, Nombre = @Nombre, Apellidos = @Apellidos, UpdatedAt = @UpdatedAt, IsDeleted = @IsDeleted,
                    Tipo = @Tipo, Calificacion = @Calificacion, Ciclo = @Ciclo, Curso = @Curso, Experiencia = @Experiencia, Especialidad = @Especialidad
                    WHERE Id = @Id";

        connection.Execute(sql, new
        {
            Id = id,
            entity.Dni,
            entity.Nombre,
            entity.Apellidos,
            entity.UpdatedAt,
            entity.IsDeleted,
            entity.Tipo,
            entity.Calificacion,
            entity.Ciclo,
            entity.Curso,
            entity.Experiencia,
            entity.Especialidad
        });

        return GetById(id);
    }

    public Persona? Delete(int id)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        using var connection = CreateConnection();
        var sql = "UPDATE Personas SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        connection.Execute(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });

        return GetById(id);
    }

    public bool DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        using var connection = CreateConnection();
        connection.Execute("DELETE FROM Personas");
        return true;
    }
}
