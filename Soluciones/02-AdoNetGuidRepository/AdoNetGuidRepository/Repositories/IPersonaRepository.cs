using AdoNetGuidRepository.Models;
using AdoNetGuidRepository.Repositories;

namespace AdoNetGuidRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<Guid, Persona>
{
}
