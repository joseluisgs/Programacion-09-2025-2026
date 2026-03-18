using Microsoft.Data.Sqlite;
using AdoNetGuidRepository.Models;
using AdoNetGuidRepository.Factories;

namespace AdoNetGuidRepository.Repositories;

/// <summary>
/// Repositorio implementado con ADO.NET y SQLite en memoria.
/// Usa clave GUID.
/// </summary>
public class PersonaRepository : IPersonaRepository
{
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Constructor que recibe la conexión y crea la tabla y datos iniciales.
    /// </summary>
    /// <param name="connection">Conexión SQLite.</param>
    public PersonaRepository(SqliteConnection connection)
    {
        _connection = connection;
        CreateTable();
        SeedData();
    }

    /// <summary>
    /// Crea la tabla Personas si no existe.
    /// </summary>
    private void CreateTable()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Personas (
                Id TEXT PRIMARY KEY,
                Nombre TEXT NOT NULL,
                Email TEXT UNIQUE,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                DeletedAt TEXT
            )";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Carga los datos iniciales de prueba.
    /// </summary>
    private void SeedData()
    {
        foreach (var persona in PersonaFactory.Seed())
            Create(persona);
    }

    /// <inheritdoc/>
    public IEnumerable<Persona> GetAll()
    {
        var personas = new List<Persona>();
        
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Personas ORDER BY CreatedAt DESC";
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
            personas.Add(MapPersona(reader));
        
        return personas;
    }

    /// <inheritdoc/>
    public Persona? GetById(Guid id)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Personas WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id.ToString());
        
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapPersona(reader) : null;
    }

    /// <inheritdoc/>
    public Persona? Create(Persona persona)
    {
        persona = persona with 
        { 
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };

        using var command = _connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Personas (Id, Nombre, Email, CreatedAt, UpdatedAt, IsDeleted)
            VALUES (@id, @nombre, @email, @createdAt, @updatedAt, @isDeleted)";
        
        command.Parameters.AddWithValue("@id", persona.Id.ToString());
        command.Parameters.AddWithValue("@nombre", persona.Nombre);
        command.Parameters.AddWithValue("@email", persona.Email ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", persona.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@updatedAt", persona.UpdatedAt.ToString("o"));
        command.Parameters.AddWithValue("@isDeleted", persona.IsDeleted ? 1 : 0);

        command.ExecuteNonQuery();
        
        return persona;
    }

    /// <inheritdoc/>
    public Persona? Update(Guid id, Persona persona)
    {
        persona = persona with { Id = id, UpdatedAt = DateTime.Now };
        
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            UPDATE Personas 
            SET Nombre = @nombre, Email = @email, UpdatedAt = @updatedAt
            WHERE Id = @id";
        
        command.Parameters.AddWithValue("@id", id.ToString());
        command.Parameters.AddWithValue("@nombre", persona.Nombre);
        command.Parameters.AddWithValue("@email", persona.Email ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@updatedAt", persona.UpdatedAt.ToString("o"));

        return command.ExecuteNonQuery() > 0 ? GetById(id) : null;
    }

    /// <inheritdoc/>
    public Persona? Delete(Guid id, bool logical = true)
    {
        var persona = GetById(id);
        if (persona == null) return null;

        if (logical)
        {
            // Borrado lógico
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                UPDATE Personas 
                SET IsDeleted = 1, DeletedAt = @deletedAt, UpdatedAt = @updatedAt
                WHERE Id = @id";
            
            command.Parameters.AddWithValue("@id", id.ToString());
            command.Parameters.AddWithValue("@deletedAt", DateTime.Now.ToString("o"));
            command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));

            return command.ExecuteNonQuery() > 0 
                ? persona with { IsDeleted = true, DeletedAt = DateTime.Now, UpdatedAt = DateTime.Now } 
                : null;
        }
        else
        {
            // Borrado físico
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM Personas WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());

            return command.ExecuteNonQuery() > 0 ? persona : null;
        }
    }

    /// <summary>
    /// Mapea un SqliteDataReader a Persona.
    /// </summary>
    private static Persona MapPersona(SqliteDataReader reader) => new(
        Id: Guid.Parse(reader.GetString(0)),
        Nombre: reader.GetString(1),
        Email: reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
        CreatedAt: DateTime.Parse(reader.GetString(3)),
        UpdatedAt: DateTime.Parse(reader.GetString(4)),
        IsDeleted: reader.GetInt32(5) == 1,
        DeletedAt: reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6))
    );
}
