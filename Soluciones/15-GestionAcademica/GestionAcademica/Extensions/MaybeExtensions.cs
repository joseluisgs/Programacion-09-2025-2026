using C = CSharpFunctionalExtensions;

namespace GestionAcademica.Extensions;

public static class MaybeExtensions
{
    public static C.Result<T, TError> ToResult<T, TError>(this C.Maybe<T> maybe, TError error)
        where T : class
        where TError : GestionAcademica.Errors.Common.DomainError
    {
        return maybe.HasValue
            ? C.Result.Success<T, TError>(maybe.Value)
            : C.Result.Failure<T, TError>(error);
    }
    
    public static C.Result<T, TError> ToResult<T, TError>(this C.Maybe<T> maybe, Func<TError> errorFactory)
        where T : class
        where TError : GestionAcademica.Errors.Common.DomainError
    {
        return maybe.HasValue
            ? C.Result.Success<T, TError>(maybe.Value)
            : C.Result.Failure<T, TError>(errorFactory());
    }
}
