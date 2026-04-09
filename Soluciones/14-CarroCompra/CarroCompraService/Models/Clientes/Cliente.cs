using System.ComponentModel.DataAnnotations;

namespace CarroCompraService.Models.Clientes;

/// <summary>
/// Entidad que representa un cliente en el sistema.
/// </summary>
public class Cliente
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Direccion { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public bool Deleted { get; set; } = false;

    /// <summary>
    /// Representación en string del cliente.
    /// </summary>
    public override string ToString() => 
        $"Cliente(id={Id}, nombre='{Nombre}', email='{Email}', direccion='{Direccion}')";
}
