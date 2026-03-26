using GestionAcademica.Errors.Common;

namespace GestionAcademica.Errors.Personas;

/// <summary>
/// Contenedor de errores específicos para el dominio de Personas.
/// </summary>
public abstract record PersonaError(string Message) : DomainError(Message)
{
    public sealed record NotFound(string Id)
        : PersonaError($"No se ha encontrado ninguna persona con el identificador: {Id}");

    public sealed record Validation(IEnumerable<string> Errors)
        : PersonaError("Se han detectado errores de validación en la entidad.");

    public sealed record AlreadyExists(string Dni)
        : PersonaError($"Conflicto de integridad: El DNI {Dni} ya está registrado en el sistema.");

    public sealed record StorageError(string Details)
        : PersonaError($"Error de almacenamiento: {Details}");
}

/// <summary>
/// Factory para crear errores de dominio de Personas.
/// </summary>
public static class PersonaErrors
{
    public static DomainError NotFound(string id) => new PersonaError.NotFound(id);
    public static DomainError Validation(IEnumerable<string> errors) => new PersonaError.Validation(errors);
    public static DomainError AlreadyExists(string dni) => new PersonaError.AlreadyExists(dni);
    public static DomainError StorageError(string details) => new PersonaError.StorageError(details);
}
