namespace CarroCompraService.Models.Ventas;

/// <summary>
/// Entidad que representa una línea de venta (un producto vendido).
/// </summary>
public class LineaVenta
{
    /// <summary>
    /// Identificador único de la línea de venta.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Identificador de la venta a la que pertenece.
    /// </summary>
    public Guid VentaId { get; set; }
    
    /// <summary>
    /// Identificador del producto.
    /// </summary>
    public long ProductoId { get; set; }
    
    /// <summary>
    /// Nombre del producto en el momento de la venta.
    /// </summary>
    public string ProductoNombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Precio original del producto.
    /// </summary>
    public double ProductoPrecio { get; set; }
    
    /// <summary>
    /// Cantidad de productos vendidos.
    /// </summary>
    public int Cantidad { get; set; }
    
    /// <summary>
    /// Precio unitario en el momento de la venta.
    /// </summary>
    public double Precio { get; set; }

    /// <summary>
    /// Representación en string de la línea de venta.
    /// </summary>
    public override string ToString() => 
        $"LineaVenta(id={Id}, producto={ProductoNombre}, cantidad={Cantidad}, precio={Precio})";
}
