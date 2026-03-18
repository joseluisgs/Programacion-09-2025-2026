using System.ComponentModel.DataAnnotations;

namespace PersonaService.Models;

/// <summary>
/// Entidad que representa una persona en el sistema.
/// </summary>
public class Persona
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public override string ToString() => 
        $"Persona(Id={Id}, Nombre={Nombre}, Email={Email}, CreatedAt={CreatedAt:yyyy-MM-dd HH:mm:ss}, UpdatedAt={UpdatedAt:yyyy-MM-dd HH:mm:ss}, IsDeleted={IsDeleted}, DeletedAt={DeletedAt:yyyy-MM-dd HH:mm:ss})";
}
