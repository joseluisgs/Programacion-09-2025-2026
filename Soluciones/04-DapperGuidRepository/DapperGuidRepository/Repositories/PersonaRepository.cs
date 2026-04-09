using Microsoft.Data.Sqlite;
using Dapper;
using DapperGuidRepository.Models;
using DapperGuidRepository.Factories;

namespace DapperGuidRepository.Repositories;

/// <summary>
/// Repositorio implementado con Dapper y SQLite en memoria.
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
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Personas (
                Id TEXT PRIMARY KEY,
                Nombre TEXT NOT NULL,
                Email TEXT UNIQUE,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                DeletedAt TEXT
            )");
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
        const string sql = "SELECT Id, Nombre, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt FROM Personas ORDER BY CreatedAt DESC";
        
        return _connection.Query<(string Id, string Nombre, string Email, string CreatedAt, string UpdatedAt, int IsDeleted, string? DeletedAt)>(sql)
            .Select(x => new Persona
            {
                Id = Guid.Parse(x.Id),
                Nombre = x.Nombre,
                Email = x.Email,
                CreatedAt = DateTime.Parse(x.CreatedAt),
                UpdatedAt = DateTime.Parse(x.UpdatedAt),
                IsDeleted = x.IsDeleted == 1,
                DeletedAt = x.DeletedAt != null ? DateTime.Parse(x.DeletedAt) : null
            });
    }

    /// <inheritdoc/>
    public Persona? GetById(Guid id)
    {
        const string sql = "SELECT Id, Nombre, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt FROM Personas WHERE Id = @Id";
        
        var result = _connection.QueryFirstOrDefault<(string Id, string Nombre, string Email, string CreatedAt, string UpdatedAt, int IsDeleted, string? DeletedAt)>(sql, new { Id = id.ToString() });
        
        if (result.Id == null) return null;
        
        return new Persona
        {
            Id = Guid.Parse(result.Id),
            Nombre = result.Nombre,
            Email = result.Email,
            CreatedAt = DateTime.Parse(result.CreatedAt),
            UpdatedAt = DateTime.Parse(result.UpdatedAt),
            IsDeleted = result.IsDeleted == 1,
            DeletedAt = result.DeletedAt != null ? DateTime.Parse(result.DeletedAt) : null
        };
    }

    /// <inheritdoc/>
    public Persona? Create(Persona persona)
    {
        persona.Id = Guid.NewGuid();
        persona.CreatedAt = DateTime.Now;
        persona.UpdatedAt = DateTime.Now;
        persona.IsDeleted = false;

        const string sql = @"
            INSERT INTO Personas (Id, Nombre, Email, CreatedAt, UpdatedAt, IsDeleted)
            VALUES (@Id, @Nombre, @Email, @CreatedAt, @UpdatedAt, @IsDeleted)";

        _connection.Execute(sql, new 
        { 
            Id = persona.Id.ToString(),
            persona.Nombre, 
            persona.Email, 
            CreatedAt = persona.CreatedAt.ToString("o"),
            UpdatedAt = persona.UpdatedAt.ToString("o"),
            IsDeleted = persona.IsDeleted ? 1 : 0
        });
        
        return persona;
    }

    /// <inheritdoc/>
    public Persona? Update(Guid id, Persona persona)
    {
        persona.Id = id;
        persona.UpdatedAt = DateTime.Now;
        
        const string sql = @"
            UPDATE Personas 
            SET Nombre = @Nombre, Email = @Email, UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var filas = _connection.Execute(sql, new 
        { 
            Id = id.ToString(), 
            persona.Nombre, 
            persona.Email, 
            UpdatedAt = persona.UpdatedAt.ToString("o") 
        });
        
        return filas > 0 ? GetById(id) : null;
    }

    /// <inheritdoc/>
    public Persona? Delete(Guid id, bool logical = true)
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
                Id = id.ToString(), 
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
            var filas = _connection.Execute(sql, new { Id = id.ToString() });
            return filas > 0 ? persona : null;
        }
    }
}
