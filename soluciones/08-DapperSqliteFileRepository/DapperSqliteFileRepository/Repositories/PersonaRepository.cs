using Microsoft.Data.Sqlite;
using Dapper;
using DapperSqliteFileRepository.Models;
using DapperSqliteFileRepository.Factories;

namespace DapperSqliteFileRepository.Repositories;

/// <summary>
/// Repositorio implementado con Dapper y SQLite en archivo.
/// Usa clave autonumérica.
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
        _connection.Execute(@"
            DROP TABLE IF EXISTS Personas;
            CREATE TABLE Personas (
                Id INTEGER PRIMARY KEY,
                Nombre TEXT NOT NULL,
                Email TEXT UNIQUE,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                DeletedAt TEXT
            )");
    }

    /// <summary>
    /// Carga los datos iniciales de prueba solo si no existen.
    /// </summary>
    private void SeedData()
    {
        var count = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Personas");
        if (count > 0) return;
        
        foreach (var persona in PersonaFactory.Seed())
            Create(persona);
    }

    /// <inheritdoc/>
    public IEnumerable<Persona> GetAll()
    {
        const string sql = "SELECT * FROM Personas ORDER BY Id";
        return _connection.Query<Persona>(sql);
    }

    /// <inheritdoc/>
    public Persona? GetById(int id)
    {
        const string sql = "SELECT * FROM Personas WHERE Id = @Id";
        return _connection.QueryFirstOrDefault<Persona>(sql, new { Id = id });
    }

    /// <inheritdoc/>
    public Persona? Create(Persona persona)
    {
        persona.CreatedAt = DateTime.Now;
        persona.UpdatedAt = DateTime.Now;
        persona.IsDeleted = false;

        const string sql = @"
            INSERT INTO Personas (Nombre, Email, CreatedAt, UpdatedAt, IsDeleted)
            VALUES (@Nombre, @Email, @CreatedAt, @UpdatedAt, @IsDeleted);
            SELECT last_insert_rowid();";

        persona.Id = _connection.ExecuteScalar<int>(sql, new 
        { 
            persona.Nombre, 
            persona.Email, 
            CreatedAt = persona.CreatedAt.ToString("o"),
            UpdatedAt = persona.UpdatedAt.ToString("o"),
            IsDeleted = persona.IsDeleted ? 1 : 0
        });
        
        return persona;
    }

    /// <inheritdoc/>
    public Persona? Update(int id, Persona persona)
    {
        persona.Id = id;
        persona.UpdatedAt = DateTime.Now;
        
        const string sql = @"
            UPDATE Personas 
            SET Nombre = @Nombre, Email = @Email, UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var filas = _connection.Execute(sql, new 
        { 
            Id = id, 
            persona.Nombre, 
            persona.Email, 
            UpdatedAt = persona.UpdatedAt.ToString("o") 
        });
        
        return filas > 0 ? GetById(id) : null;
    }

    /// <inheritdoc/>
    public Persona? Delete(int id, bool logical = true)
    {
        var persona = GetById(id);
        if (persona == null) return null;

        if (logical)
        {
            const string sql = @"
                UPDATE Personas 
                SET IsDeleted = 1, DeletedAt = @DeletedAt, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var now = DateTime.Now;
            var filas = _connection.Execute(sql, new 
            { 
                Id = id, 
                DeletedAt = now.ToString("o"),
                UpdatedAt = now.ToString("o")
            });
            
            if (filas > 0)
            {
                persona.IsDeleted = true;
                persona.DeletedAt = now;
                persona.UpdatedAt = now;
                return persona;
            }
            return null;
        }
        else
        {
            const string sql = "DELETE FROM Personas WHERE Id = @Id";
            var filas = _connection.Execute(sql, new { Id = id });
            return filas > 0 ? persona : null;
        }
    }
}
