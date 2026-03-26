using CSharpFunctionalExtensions;

namespace CarroCompraService.Extensions;

// Extensiones para Maybe de CSharpFunctionalExtensions
public static class MaybeExtensions
{
    // Convierte Maybe a Result con un error si está vacío
    public static Result<T, TError> ToResult<T, TError>(this Maybe<T> maybe, TError error) 
        where T : class
        where TError : Errors.DomainError
    {
        return maybe.HasValue
            ? Result.Success<T, TError>(maybe.Value)
            : Result.Failure<T, TError>(error);
    }
}
