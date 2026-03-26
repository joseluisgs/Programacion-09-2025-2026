using CSharpFunctionalExtensions;
using PersonaService.Errors;

namespace PersonaService.Validators;

/// <summary>
/// Interfaz para el validador.
/// </summary>
public interface IValidador<T> where T : class
{
    Result<T, DomainError> Validar(T entity);
}
