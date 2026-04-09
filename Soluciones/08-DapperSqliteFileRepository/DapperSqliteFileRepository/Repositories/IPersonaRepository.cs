using DapperSqliteFileRepository.Models;
using DapperSqliteFileRepository.Repositories;

namespace DapperSqliteFileRepository.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de personas.
/// </summary>
public interface IPersonaRepository : ICrudRepository<int, Persona>
{
}
