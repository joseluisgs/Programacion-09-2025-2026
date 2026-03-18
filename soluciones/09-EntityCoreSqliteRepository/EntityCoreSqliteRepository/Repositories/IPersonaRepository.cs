using EntityCoreSqliteRepository.Models;
using EntityCoreSqliteRepository.Repositories;

namespace EntityCoreSqliteRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<int, Persona>
{
}
