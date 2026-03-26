using DapperAutonumericRepository.Models;
using DapperAutonumericRepository.Repositories;

namespace DapperAutonumericRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<int, Persona>
{
}
