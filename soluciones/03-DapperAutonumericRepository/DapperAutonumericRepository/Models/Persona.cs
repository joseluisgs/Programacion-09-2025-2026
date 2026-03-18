namespace DapperAutonumericRepository.Models;

/// <summary>
/// Entidad que representa una persona en el sistema.
/// </summary>
public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public override string ToString() => $"Persona({Id}, {Nombre}, {Email})";
}
