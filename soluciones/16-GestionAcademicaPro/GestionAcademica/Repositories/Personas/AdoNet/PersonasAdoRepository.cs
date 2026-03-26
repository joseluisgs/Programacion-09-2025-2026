using System.Data;
using Microsoft.Data.Sqlite;
using GestionAcademica.Config;
using GestionAcademica.Entity;
using GestionAcademica.Factories.Personas;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories.Personas.Base;
using Serilog;

namespace GestionAcademica.Repositories.Personas.AdoNet;

/// <summary>
/// Repositorio ADO.NET para la gestión de Personas.
/// Internamente usa Entity para almacenar, y Mapper para convertir.
/// </summary>
public class PersonasAdoRepository : IPersonasRepository
{
    private readonly ILogger _logger = Log.ForContext<PersonasAdoRepository>();
    private readonly string _connectionString;

    public PersonasAdoRepository() : this(AppConfig.DropData, AppConfig.SeedData) { }

    public PersonasAdoRepository(bool dropData, bool seedData)
    {
        _logger.Debug("Inicializando repositorio ADO.NET.");
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
        using var command = connection.CreateCommand();
        command.CommandText = @"
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
            )";
        command.ExecuteNonQuery();
    }

    public IEnumerable<Persona> GetAll()
    {
        var entities = new List<PersonaEntity>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Personas WHERE IsDeleted = 0";
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            entities.Add(ReadEntity(reader));
        }
        
        return entities.ToModel();
    }

    public Persona? GetById(int id)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Personas WHERE Id = @Id AND IsDeleted = 0";
        command.Parameters.Add(new SqliteParameter("@Id", id));
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ReadEntity(reader).ToModel();
        }
        return null;
    }

    public Persona? GetByDni(string dni)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Personas WHERE Dni = @Dni AND IsDeleted = 0";
        command.Parameters.Add(new SqliteParameter("@Dni", dni));
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ReadEntity(reader).ToModel();
        }
        return null;
    }

    public bool ExisteDni(string dni)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Personas WHERE Dni = @Dni AND IsDeleted = 0";
        command.Parameters.Add(new SqliteParameter("@Dni", dni));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public Persona? Create(Persona model)
    {
        if (ExisteDni(model.Dni)) return null;

        var entity = model.ToEntity();
        entity.Id = 0;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Personas (Dni, Nombre, Apellidos, CreatedAt, UpdatedAt, IsDeleted, Tipo, Calificacion, Ciclo, Curso, Experiencia, Especialidad)
                    VALUES (@Dni, @Nombre, @Apellidos, @CreatedAt, @UpdatedAt, @IsDeleted, @Tipo, @Calificacion, @Ciclo, @Curso, @Experiencia, @Especialidad);
                    SELECT last_insert_rowid();";
        
        AddParameters(command, entity);
        entity.Id = Convert.ToInt32(command.ExecuteScalar());

        return GetById(entity.Id);
    }

    public Persona? Update(int id, Persona model)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        // Si cambió el DNI, verificar que no exista en otra persona
        if (model.Dni != existing.Dni) {
            var otherWithDni = GetByDni(model.Dni);
            if (otherWithDni != null && otherWithDni.Id != id) {
                _logger.Warning("No se puede actualizar persona con id {Id} porque el DNI {Dni} ya está en uso por otra persona", id, model.Dni);
                return null;
            }
        }

        var entity = model.ToEntity();
        entity.Id = id;
        entity.CreatedAt = existing.CreatedAt;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE Personas SET 
                    Dni = @Dni, Nombre = @Nombre, Apellidos = @Apellidos, UpdatedAt = @UpdatedAt, IsDeleted = @IsDeleted,
                    Tipo = @Tipo, Calificacion = @Calificacion, Ciclo = @Ciclo, Curso = @Curso, Experiencia = @Experiencia, Especialidad = @Especialidad
                    WHERE Id = @Id";
        
        AddParameters(command, entity, id);
        command.ExecuteNonQuery();

        return GetById(id);
    }

    public Persona? Delete(int id)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Personas SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        command.Parameters.Add(new SqliteParameter("@Id", id));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", DateTime.UtcNow.ToString("o")));
        command.ExecuteNonQuery();

        return GetById(id);
    }

    public bool DeleteAll()
    {
        _logger.Warning("Eliminando permanentemente todas las personas");
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Personas";
        command.ExecuteNonQuery();
        return true;
    }

    private PersonaEntity ReadEntity(SqliteDataReader reader)
    {
        return new PersonaEntity
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Dni = reader.GetString(reader.GetOrdinal("Dni")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Apellidos = reader.GetString(reader.GetOrdinal("Apellidos")),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt"))),
            IsDeleted = reader.GetInt32(reader.GetOrdinal("IsDeleted")) == 1,
            Tipo = reader.IsDBNull(reader.GetOrdinal("Tipo")) ? "Persona" : reader.GetString(reader.GetOrdinal("Tipo")),
            Calificacion = reader.IsDBNull(reader.GetOrdinal("Calificacion")) ? null : reader.GetDouble(reader.GetOrdinal("Calificacion")),
            Ciclo = reader.IsDBNull(reader.GetOrdinal("Ciclo")) ? null : reader.GetInt32(reader.GetOrdinal("Ciclo")),
            Curso = reader.IsDBNull(reader.GetOrdinal("Curso")) ? null : reader.GetInt32(reader.GetOrdinal("Curso")),
            Experiencia = reader.IsDBNull(reader.GetOrdinal("Experiencia")) ? null : reader.GetInt32(reader.GetOrdinal("Experiencia")),
            Especialidad = reader.IsDBNull(reader.GetOrdinal("Especialidad")) ? null : reader.GetString(reader.GetOrdinal("Especialidad"))
        };
    }

    private void AddParameters(IDbCommand command, PersonaEntity entity, int? id = null)
    {
        if (id.HasValue)
        {
            command.Parameters.Add(new SqliteParameter("@Id", id.Value));
        }
        command.Parameters.Add(new SqliteParameter("@Dni", entity.Dni));
        command.Parameters.Add(new SqliteParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqliteParameter("@Apellidos", entity.Apellidos));
        command.Parameters.Add(new SqliteParameter("@CreatedAt", entity.CreatedAt.ToString("o")));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", entity.UpdatedAt.ToString("o")));
        command.Parameters.Add(new SqliteParameter("@IsDeleted", entity.IsDeleted ? 1 : 0));
        command.Parameters.Add(new SqliteParameter("@Tipo", entity.Tipo));
        command.Parameters.Add(new SqliteParameter("@Calificacion", entity.Calificacion ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@Ciclo", entity.Ciclo ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@Curso", entity.Curso ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@Experiencia", entity.Experiencia ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@Especialidad", entity.Especialidad ?? (object)DBNull.Value));
    }
}
