using AdoNetSqliteFileRepository.Models;
using AdoNetSqliteFileRepository.Repositories;

namespace AdoNetSqliteFileRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<int, Persona>
{
}
