using EntityCoreGuidRepository.Models;
using EntityCoreGuidRepository.Repositories;

namespace EntityCoreGuidRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<Guid, Persona>
{
}
