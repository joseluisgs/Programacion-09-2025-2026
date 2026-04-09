namespace AdoNetGuidRepository.Models;

/// <summary>
/// Entidad que representa una persona en el sistema.
/// </summary>
public record Persona(Guid Id, string Nombre, string Email, DateTime CreatedAt, DateTime UpdatedAt, bool IsDeleted, DateTime? DeletedAt)
{
    /// <summary>
    /// Representación en cadena de la persona.
    /// </summary>
    public override string ToString() => $"Persona({Id}, {Nombre}, {Email})";
}
