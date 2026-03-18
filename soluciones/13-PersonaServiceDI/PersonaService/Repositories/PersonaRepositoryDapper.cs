using Dapper;
using Microsoft.Data.Sqlite;
using PersonaService.Models;
using PersonaService.Config;
using PersonaService.Factories;
using Serilog;

namespace PersonaService.Repositories;

/// <summary>
/// Repositorio implementado con Dapper y SQLite.
/// </summary>
public class PersonaRepositoryDapper : IPersonaRepository
{
    private readonly SqliteConnection _connection;
    private readonly ILogger _logger = Log.ForContext<PersonaRepositoryDapper>();

    public PersonaRepositoryDapper(SqliteConnection connection)
    {
        _connection = connection;
        Initialize();
        SeedData();
    }

    private void Initialize()
    {
        if (AppConfig.CreateTable)
        {
            _logger.Information("Creando tabla si no existe...");
            _connection.Execute(@"
                DROP TABLE IF EXISTS Personas;
                CREATE TABLE Personas (
                    Id INTEGER PRIMARY KEY,
                    Nombre TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0,
                    DeletedAt TEXT
                )");
        }

        if (AppConfig.DropData)
        {
            _logger.Information("Borrando datos existentes...");
            _connection.Execute("DELETE FROM Personas");
        }
    }

    private void SeedData()
    {
        if (AppConfig.SeedData)
        {
            var count = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Personas");
            if (count == 0)
            {
                _logger.Information("Insertando datos de semilla...");
                var seedData = PersonaFactory.Seed();
                foreach (var p in seedData)
                {
                    _connection.Execute(@"
                        INSERT INTO Personas (Nombre, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
                        VALUES (@Nombre, @Email, @CreatedAt, @UpdatedAt, @IsDeleted, @DeletedAt)",
                        new { p.Nombre, p.Email, p.CreatedAt, p.UpdatedAt, p.IsDeleted, p.DeletedAt });
                }
                _logger.Information("SeedData completado.");
            }
        }
    }

    public IEnumerable<Persona> GetAll()
    {
        _logger.Debug("GetAll ejecutado");
        var personas = _connection.Query<Persona>("SELECT * FROM Personas ORDER BY Id");
        return personas.ToList();
    }

    public Persona? GetById(int id)
    {
        _logger.Debug("GetById({Id}) ejecutado", id);
        return _connection.QueryFirstOrDefault<Persona>(
            "SELECT * FROM Personas WHERE Id = @Id", new { Id = id });
    }

    public Persona? Create(Persona persona)
    {
        persona.CreatedAt = DateTime.Now;
        persona.UpdatedAt = DateTime.Now;
        persona.IsDeleted = false;

        var id = _connection.ExecuteScalar<int>(@"
            INSERT INTO Personas (Nombre, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
            VALUES (@Nombre, @Email, @CreatedAt, @UpdatedAt, @IsDeleted, @DeletedAt);
            SELECT last_insert_rowid();",
            new { persona.Nombre, persona.Email, persona.CreatedAt, persona.UpdatedAt, persona.IsDeleted, persona.DeletedAt });

        persona.Id = id;
        _logger.Information("Create: Persona {Id} creada - {Nombre}", persona.Id, persona.Nombre);
        return persona;
    }

    public Persona? Update(int id, Persona persona)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        _connection.Execute(@"
            UPDATE Personas 
            SET Nombre = @Nombre, Email = @Email, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new { Id = id, persona.Nombre, persona.Email, UpdatedAt = DateTime.Now });

        _logger.Information("Update: Persona {Id} actualizada - {Nombre}", id, persona.Nombre);
        return GetById(id);
    }

    public Persona? Delete(int id, bool logical = true)
    {
        var persona = GetById(id);
        if (persona == null) return null;

        if (logical)
        {
            _connection.Execute(@"
                UPDATE Personas 
                SET IsDeleted = 1, DeletedAt = @DeletedAt, UpdatedAt = @UpdatedAt
                WHERE Id = @Id",
                new { Id = id, DeletedAt = DateTime.Now, UpdatedAt = DateTime.Now });

            _logger.Information("Delete Logical: Persona {Id} marcada como eliminada", id);
            return GetById(id);
        }
        else
        {
            _connection.Execute("DELETE FROM Personas WHERE Id = @Id", new { Id = id });
            _logger.Information("Delete Physical: Persona {Id} eliminada físicamente", id);
            return persona;
        }
    }

    public Persona? FindByEmail(string email)
    {
        _logger.Debug("FindByEmail({Email}) ejecutado", email);
        return _connection.QueryFirstOrDefault<Persona>(
            "SELECT * FROM Personas WHERE Email = @Email", new { Email = email });
    }
}
