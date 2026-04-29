using DapperAutonumericRepository.Models;

namespace DapperAutonumericRepository.Factories;

/// <summary>
/// Factory para generar datos de prueba (seed data).
/// </summary>
public static class PersonaFactory
{
    /// <summary>
    /// Genera una lista de personas de prueba.
    /// Nota: Dapper requiere constructor sin parámetros, usar con new().
    /// </summary>
    public static IEnumerable<Persona> Seed()
    {
        return new List<Persona>
        {
            new() { Id = 0, Nombre = "Ana García", Email = "ana@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false },
            new() { Id = 0, Nombre = "Juan Pérez", Email = "juan@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false },
            new() { Id = 0, Nombre = "María López", Email = "maria@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false }
        };
    }
}
