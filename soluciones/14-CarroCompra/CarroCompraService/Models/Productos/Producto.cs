using System.ComponentModel.DataAnnotations;

namespace CarroCompraService.Models.Productos;

/// <summary>
/// Entidad que representa un producto en el sistema.
/// </summary>
public class Producto
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    public double Precio { get; set; }
    
    public int Stock { get; set; }
    
    public Categoria Categoria { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public bool Deleted { get; set; } = false;

    /// <summary>
    /// Representación en string del producto.
    /// </summary>
    public override string ToString() => 
        $"Producto(id={Id}, nombre='{Nombre}', precio={Precio}, stock={Stock}, categoria={Categoria}, deleted={Deleted})";
}
