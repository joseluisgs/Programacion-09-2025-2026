using CSharpFunctionalExtensions;
using CarroCompraService.Models.Ventas;
using CarroCompraService.Errors;
using CarroCompraService.Config;
using Serilog;

namespace CarroCompraService.Services.Factura;

/// <summary>
/// Interfaz para el servicio de facturación y exportación de ventas.
/// </summary>
public interface IFacturaService
{
    /// <summary>
    /// Genera una factura en formato HTML.
    /// </summary>
    /// <param name="venta">Venta a facturar.</param>
    /// <returns>Resultado con el HTML generado o error.</returns>
    Result<string, DomainError> GenerarFacturaHtml(Venta venta);
    
    /// <summary>
    /// Guarda la factura HTML en un archivo.
    /// </summary>
    /// <param name="html">Contenido HTML.</param>
    /// <param name="fileName">Nombre del archivo.</param>
    /// <returns>Resultado con éxito o error.</returns>
    Result<bool, DomainError> GuardarFactura(string html, string fileName);
}
