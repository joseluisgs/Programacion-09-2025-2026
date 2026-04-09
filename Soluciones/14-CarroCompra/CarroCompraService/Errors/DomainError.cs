namespace CarroCompraService.Errors;

/// <summary>
/// Errores de dominio para el módulo de CarroCompra.
/// </summary>
public abstract record DomainError(string Message)
{
    public sealed record NotFound(string Mensaje) : DomainError(Mensaje);
    
    public sealed record Validation(IEnumerable<string> Errors) : DomainError("Se han detectado errores de validación en la entidad.");
    
    public sealed record AlreadyExists(string Mensaje) : DomainError($"Conflicto: {Mensaje} ya existe.");
}

/// <summary>
/// Factory para crear errores de dominio.
/// </summary>
public static class DomainErrors
{
    public static DomainError NotFound(string mensaje) => new DomainError.NotFound(mensaje);
    public static DomainError Validation(IEnumerable<string> errors) => new DomainError.Validation(errors);
    public static DomainError AlreadyExists(string mensaje) => new DomainError.AlreadyExists(mensaje);
}
