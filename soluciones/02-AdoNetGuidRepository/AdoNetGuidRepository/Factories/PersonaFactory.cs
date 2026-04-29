using AdoNetGuidRepository.Models;

namespace AdoNetGuidRepository.Factories;

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
            new(Guid.NewGuid(), "Ana García", "ana@correo.com", DateTime.Now, DateTime.Now, false, null),
            new(Guid.NewGuid(), "Juan Pérez", "juan@correo.com", DateTime.Now, DateTime.Now, false, null),
            new(Guid.NewGuid(), "María López", "maria@correo.com", DateTime.Now, DateTime.Now, false, null)
        };
    }
}
