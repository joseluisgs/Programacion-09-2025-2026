namespace CarroCompraService.Models.Ventas;

/// <summary>
/// Entidad que representa una venta (carrito de compra) en el sistema.
/// </summary>
public class Venta
{
    /// <summary>
    /// Identificador único de la venta (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Identificador del cliente que realiza la compra.
    /// </summary>
    public long ClienteId { get; set; }
    
    /// <summary>
    /// Nombre del cliente en el momento de la venta.
    /// </summary>
    public string ClienteNombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Líneas de productos vendidos.
    /// </summary>
    public List<LineaVenta> Lineas { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public bool Deleted { get; set; } = false;

    /// <summary>
    /// Calcula el total de la venta.
    /// </summary>
    public double Total => Lineas.Sum(l => l.Precio * l.Cantidad);

    /// <summary>
    /// Representación en string de la venta.
    /// </summary>
    public override string ToString() => 
        $"Venta(id={Id}, cliente={ClienteNombre}, total={Total}, lineas={Lineas.Count})";
}
