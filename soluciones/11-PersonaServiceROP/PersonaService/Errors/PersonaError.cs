namespace PersonaService.Errors;

/// <summary>
/// Errores de dominio para el módulo de Personas.
/// </summary>
public abstract record PersonaError(string Message)
{
    public sealed record NotFound(int Id) : PersonaError($"No se ha encontrado ninguna persona con el identificador: {Id}");
    
    public sealed record Validation(IEnumerable<string> Errors) : PersonaError("Se han detectado errores de validación en la entidad.");
    
    public sealed record AlreadyExists(string Email) : PersonaError($"Conflicto: El email {Email} ya está registrado.");
}

/// <summary>
/// Factory para crear errores de dominio.
/// </summary>
public static class PersonaErrors
{
    public static PersonaError NotFound(int id) => new PersonaError.NotFound(id);
    public static PersonaError Validation(IEnumerable<string> errors) => new PersonaError.Validation(errors);
    public static PersonaError AlreadyExists(string email) => new PersonaError.AlreadyExists(email);
}
