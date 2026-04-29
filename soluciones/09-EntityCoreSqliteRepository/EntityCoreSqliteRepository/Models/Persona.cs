using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EntityCoreSqliteRepository.Models;

/// <summary>
///     Entidad que representa una persona en el sistema.
/// </summary>
[Table("Personas")]
[Index(nameof(Email), IsUnique = true)]
public class Persona {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string Nombre { get; set; } = string.Empty;

    [MaxLength(100)] [EmailAddress] public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public override string ToString() {
        return $"Persona({Id}, {Nombre}, {Email})";
    }
}