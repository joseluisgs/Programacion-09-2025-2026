using CSharpFunctionalExtensions;
using CarroCompraService.Errors;

namespace CarroCompraService.Validators.Common;

/// <summary>
/// Interfaz genérica para validadores de entidades.
/// </summary>
public interface IValidador<T> where T : class
{
    /// <summary>
    /// Valida una entidad y devuelve un Result con el error si falla.
    /// </summary>
    /// <param name="entity">Entidad a validar.</param>
    /// <returns>Result con la entidad validada o error.</returns>
    Result<T, DomainError> Validar(T entity);
}
