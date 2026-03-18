using EntityCoreAutonumericRepository.Models;
using EntityCoreAutonumericRepository.Repositories;

namespace EntityCoreAutonumericRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<int, Persona>
{
}
