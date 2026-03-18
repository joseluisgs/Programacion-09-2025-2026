using AdoNetAutonumericRepository.Models;
using AdoNetAutonumericRepository.Repositories;

namespace AdoNetAutonumericRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<int, Persona>
{
}
