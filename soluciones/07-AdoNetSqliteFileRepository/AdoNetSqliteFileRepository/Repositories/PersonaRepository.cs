using AdoNetSqliteFileRepository.Factories;
using AdoNetSqliteFileRepository.Models;
using Microsoft.Data.Sqlite;

namespace AdoNetSqliteFileRepository.Repositories;

/// <summary>
///     Repositorio implementado con ADO.NET y SQLite en archivo.
///     Usa clave autonumérica.
/// </summary>
public class PersonaRepository : IPersonaRepository {
    private readonly SqliteConnection _connection;

    /// <summary>
    ///     Constructor que recibe la conexión y crea la tabla y datos iniciales.
    /// </summary>
    /// <param name="connection">Conexión SQLite.</param>
    public PersonaRepository(SqliteConnection connection) {
        _connection = connection;
        CreateTable();
        SeedData();
    }

    /// <inheritdoc />
    public IEnumerable<Persona> GetAll() {
        var personas = new List<Persona>();

        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Personas ORDER BY Id";

        using var reader = command.ExecuteReader();
        while (reader.Read())
            personas.Add(MapPersona(reader));

        return personas;
    }

    /// <inheritdoc />
    public Persona? GetById(int id) {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Personas WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapPersona(reader) : null;
    }

    /// <inheritdoc />
    public Persona? Create(Persona persona) {
        try {
            persona = persona with {
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            };

            using var command = _connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO Personas (Nombre, Email, CreatedAt, UpdatedAt, IsDeleted)
            VALUES (@nombre, @email, @createdAt, @updatedAt, @isDeleted);
            SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@nombre", persona.Nombre);
            command.Parameters.AddWithValue("@email", persona.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@createdAt", persona.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("@updatedAt", persona.UpdatedAt.ToString("o"));
            command.Parameters.AddWithValue("@isDeleted", persona.IsDeleted ? 1 : 0);

            persona = persona with { Id = Convert.ToInt32(command.ExecuteScalar()) };

            return persona;
        }
        catch (SqliteException ex) {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public Persona? Update(int id, Persona persona) {
        try {
            persona = persona with { UpdatedAt = DateTime.Now };

            using var command = _connection.CreateCommand();
            command.CommandText = @"
            UPDATE Personas 
            SET Nombre = @nombre, Email = @email, UpdatedAt = @updatedAt
            WHERE Id = @id";

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@nombre", persona.Nombre);
            command.Parameters.AddWithValue("@email", persona.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@updatedAt", persona.UpdatedAt.ToString("o"));

            return command.ExecuteNonQuery() > 0 ? persona : null;
        }
        catch (SqliteException ex) {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public Persona? Delete(int id, bool logical = true) {
        var existing = GetById(id);
        if (existing == null) return null;
        if (logical) {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                UPDATE Personas 
                SET IsDeleted = 1, DeletedAt = @deletedAt, UpdatedAt = @updatedAt
                WHERE Id = @id";

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@deletedAt", DateTime.Now.ToString("o"));
            command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));

            return command.ExecuteNonQuery() > 0
                ? existing with { IsDeleted = true, DeletedAt = DateTime.Now, UpdatedAt = DateTime.Now }
                : null;
        }
        else {
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM Personas WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            return command.ExecuteNonQuery() > 0
                ? existing with { IsDeleted = true, DeletedAt = DateTime.Now, UpdatedAt = DateTime.Now }
                : null;
        }
    }

    /// <summary>
    ///     Crea la tabla Personas si no existe.
    /// </summary>
    private void CreateTable() {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            DROP TABLE IF EXISTS Personas;
            CREATE TABLE Personas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
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
    ///     Carga los datos iniciales de prueba solo si no existen.
    /// </summary>
    private void SeedData() {
        // Verificar si ya existen datos
        using var checkCmd = _connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Personas";
        var count = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (count > 0) return;

        foreach (var persona in PersonaFactory.Seed())
            Create(persona);
    }

    /// <summary>
    ///     Mapea un SqliteDataReader a Persona.
    /// </summary>
    private Persona MapPersona(SqliteDataReader reader) {
        return new Persona(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            DateTime.Parse(reader.GetString(3)),
            DateTime.Parse(reader.GetString(4)),
            reader.GetInt32(5) == 1,
            reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6))
        );
    }
}