using CSharpFunctionalExtensions;
using CF = CSharpFunctionalExtensions;

namespace PersonaService.Extensions;

/// <summary>
/// Extensiones para Maybe que facilitan la conversión a Result (patrón ROP).
/// </summary>
public static class MaybeExtensions
{
    /// <summary>
    /// Extiende Maybe<T> con el método ToResult para convertirlo en Result.
    /// </summary>
    /// <typeparam name="T">Tipo del valor contenido en el Maybe</typeparam>
    /// <param name="maybe">Instancia de Maybe a extender</param>
    /// <remarks>
    /// Esta extensión resuelve dos problemas de CSharpFunctionalExtensions:
    /// 1. Solo acepta string como error, no tipos personalizados
    /// 2. No infiere tipos base con jerarquías de errores
    /// </remarks>
    extension<T>(Maybe<T> maybe) where T : class
    {
        /// <summary>
        /// Convierte Maybe en Result con error directo.
        /// </summary>
        /// <typeparam name="TError">Tipo del error</typeparam>
        /// <param name="error">Error a usar si el Maybe está vacío</param>
        /// <returns>
        /// - Si maybe.HasValue → Result.Success(valor)
        /// - Si !HasValue → Result.Failure(error)
        /// </returns>
        /// <example>
        /// Maybe.From(repository.GetById(1)).ToResult(DomainErrors.NotFound(1))
        /// </example>
        public CF.Result<T, TError> ToResult<TError>(TError error)
        {
            return maybe.HasValue
                ? CF.Result.Success<T, TError>(maybe.Value)
                : CF.Result.Failure<T, TError>(error);
        }
        
        /// <summary>
        /// Convierte Maybe en Result con error lazy (factory).
        /// Útil para evitar crear errores costosos si no son necesarios.
        /// </summary>
        /// <typeparam name="TError">Tipo del error</typeparam>
        /// <param name="errorFactory">Función que crea el error solo si es necesario</param>
        /// <returns>
        /// - Si maybe.HasValue → Result.Success(valor) - error NUNCA se crea
        /// - Si !HasValue → Result.Failure(errorFactory()) - error SOLO se crea si es null
        /// </returns>
        /// <example>
        /// Maybe.From(repository.GetById(1)).ToResult(() => DomainErrors.NotFound(1))
        /// </example>
        public CF.Result<T, TError> ToResult<TError>(Func<TError> errorFactory)
        {
            return maybe.HasValue
                ? CF.Result.Success<T, TError>(maybe.Value)
                : CF.Result.Failure<T, TError>(errorFactory());
        }
    }
}
