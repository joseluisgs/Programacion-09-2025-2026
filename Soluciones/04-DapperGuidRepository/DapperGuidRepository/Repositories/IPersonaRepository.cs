using DapperGuidRepository.Models;
using DapperGuidRepository.Repositories;

namespace DapperGuidRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<Guid, Persona>
{
}
