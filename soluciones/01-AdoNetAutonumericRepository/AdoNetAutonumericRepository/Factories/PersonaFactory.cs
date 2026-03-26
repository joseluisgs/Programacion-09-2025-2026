using AdoNetAutonumericRepository.Models;

namespace AdoNetAutonumericRepository.Factories;

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
            new(0, "Ana García", "ana@correo.com", DateTime.Now, DateTime.Now, false, null),
            new(0, "Juan Pérez", "juan@correo.com", DateTime.Now, DateTime.Now, false, null),
            new(0, "María López", "maria@correo.com", DateTime.Now, DateTime.Now, false, null)
        };
    }
}
