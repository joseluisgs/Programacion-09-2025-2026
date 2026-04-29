using EntityCoreGuidRepository.Models;

namespace EntityCoreGuidRepository.Factories;

/// <summary>
/// Factory para generar datos de prueba (seed data).
/// </summary>
public static class PersonaFactory
{
    /// <summary>
    /// Genera una lista de personas de prueba.
    /// </summary>
    public static IEnumerable<Persona> Seed()
    {
        return new List<Persona>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Ana García", Email = "ana@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false },
            new() { Id = Guid.NewGuid(), Nombre = "Juan Pérez", Email = "juan@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false },
            new() { Id = Guid.NewGuid(), Nombre = "María López", Email = "maria@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false }
        };
    }
}
