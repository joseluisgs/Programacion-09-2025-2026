using Microsoft.Data.Sqlite;
using Dapper;
using CarroCompraService.Models.Clientes;
using CarroCompraService.Config;
using CarroCompraService.Repositories.Common;
using Serilog;

namespace CarroCompraService.Repositories.Clientes;

/// <summary>
/// Repositorio de Clientes usando Dapper.
/// </summary>
public class ClienteRepository : IClienteRepository
{
    private readonly SqliteConnection _connection;
    private readonly ILogger _logger = Log.ForContext<ClienteRepository>();

    public ClienteRepository(SqliteConnection connection)
    {
        _connection = connection;
        Initialize();
        SeedData();
    }

    private void Initialize()
    {
        if (AppConfig.DropData)
        {
            _logger.Information("Borrando datos de Clientes...");
            _connection.Execute("DELETE FROM Clientes");
        }

        if (AppConfig.CreateTable)
        {
            _logger.Information("Creando tabla de Clientes...");
            _connection.Execute(@"
                DROP TABLE IF EXISTS Clientes;
                CREATE TABLE Clientes (
                    Id INTEGER PRIMARY KEY,
                    Nombre TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Direccion TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    Deleted INTEGER NOT NULL DEFAULT 0
                )");
        }
    }

    private void SeedData()
    {
        if (AppConfig.SeedData)
        {
            var count = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clientes");
            if (count == 0)
            {
                _logger.Information("Insertando datos de semilla de Clientes...");
                var seedData = Factories.ClienteFactory.Seed();
                foreach (var c in seedData)
                {
                    _connection.Execute(@"
                        INSERT INTO Clientes (Nombre, Email, Direccion, CreatedAt, UpdatedAt, Deleted)
                        VALUES (@Nombre, @Email, @Direccion, @CreatedAt, @UpdatedAt, @Deleted)",
                        new { c.Nombre, c.Email, c.Direccion, c.CreatedAt, c.UpdatedAt, c.Deleted });
                }
                _logger.Information("SeedData completado.");
            }
        }
    }

    public IEnumerable<Cliente> GetAll()
    {
        _logger.Debug("GetAll ejecutado");
        return _connection.Query<Cliente>("SELECT * FROM Clientes WHERE Deleted = 0 ORDER BY Id").ToList();
    }

    public Cliente? GetById(long id)
    {
        _logger.Debug("GetById({Id}) ejecutado", id);
        return _connection.QueryFirstOrDefault<Cliente>(
            "SELECT * FROM Clientes WHERE Id = @Id AND Deleted = 0", new { Id = id });
    }

    public Cliente? Create(Cliente cliente)
    {
        cliente.CreatedAt = DateTime.Now;
        cliente.UpdatedAt = DateTime.Now;
        cliente.Deleted = false;

        var id = _connection.ExecuteScalar<long>(@"
            INSERT INTO Clientes (Nombre, Email, Direccion, CreatedAt, UpdatedAt, Deleted)
            VALUES (@Nombre, @Email, @Direccion, @CreatedAt, @UpdatedAt, @Deleted);
            SELECT last_insert_rowid();",
            new { cliente.Nombre, cliente.Email, cliente.Direccion, cliente.CreatedAt, cliente.UpdatedAt, cliente.Deleted });

        cliente.Id = id;
        _logger.Information("Create: Cliente {Id} creado - {Nombre}", cliente.Id, cliente.Nombre);
        return cliente;
    }

    public Cliente? Update(long id, Cliente cliente)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        _connection.Execute(@"
            UPDATE Clientes 
            SET Nombre = @Nombre, Email = @Email, Direccion = @Direccion, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new { Id = id, cliente.Nombre, cliente.Email, cliente.Direccion, UpdatedAt = DateTime.Now });

        _logger.Information("Update: Cliente {Id} actualizado", id);
        return GetById(id);
    }

    public Cliente? Delete(long id, bool logical = true)
    {
        var cliente = GetById(id);
        if (cliente == null) return null;

        if (logical)
        {
            _connection.Execute(@"
                UPDATE Clientes 
                SET Deleted = 1, UpdatedAt = @UpdatedAt
                WHERE Id = @Id",
                new { Id = id, UpdatedAt = DateTime.Now });

            _logger.Information("Delete Logical: Cliente {Id} marcado como eliminado", id);
            return GetById(id);
        }
        else
        {
            _connection.Execute("DELETE FROM Clientes WHERE Id = @Id", new { Id = id });
            _logger.Information("Delete Physical: Cliente {Id} eliminado físicamente", id);
            return cliente;
        }
    }

    public Cliente? GetByEmail(string email)
    {
        _logger.Debug("GetByEmail({Email}) ejecutado", email);
        return _connection.QueryFirstOrDefault<Cliente>(
            "SELECT * FROM Clientes WHERE Email = @Email AND Deleted = 0", new { Email = email });
    }
}
