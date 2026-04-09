namespace PersonaService.Errors;

/// <summary>
/// Errores de dominio para el módulo de Personas.
/// </summary>
public abstract record DomainError(string Message)
{
    public sealed record NotFound(int Id) : DomainError($"No se ha encontrado ninguna persona con el identificador: {Id}");
    
    public sealed record Validation(IEnumerable<string> Errors) : DomainError("Se han detectado errores de validación en la entidad.");
    
    public sealed record AlreadyExists(string Email) : DomainError($"Conflicto: El email {Email} ya está registrado.");
}

/// <summary>
/// Factory para crear errores de dominio.
/// </summary>
public static class DomainErrors
{
    public static DomainError NotFound(int id) => new DomainError.NotFound(id);
    public static DomainError Validation(IEnumerable<string> errors) => new DomainError.Validation(errors);
    public static DomainError AlreadyExists(string email) => new DomainError.AlreadyExists(email);
}
