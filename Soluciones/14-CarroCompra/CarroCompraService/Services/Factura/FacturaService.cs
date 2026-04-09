using CSharpFunctionalExtensions;
using CarroCompraService.Models.Ventas;
using CarroCompraService.Errors;
using CarroCompraService.Config;
using Serilog;

namespace CarroCompraService.Services.Factura;

/// <summary>
/// Servicio para generar facturas en formato HTML.
/// </summary>
public class FacturaService : IFacturaService
{
    private readonly ILogger _logger = Log.ForContext<FacturaService>();

    public Result<string, DomainError> GenerarFacturaHtml(Venta venta)
    {
        _logger.Information("Generando factura HTML para venta {Id}", venta.Id);
        
        try
        {
            var tiendaNombre = AppConfig.FacturaTiendaNombre;
            var tiendaDireccion = AppConfig.FacturaTiendaDireccion;
            var fechaFormateada = venta.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            
            var html = $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <title>Factura {venta.Id}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
        .factura-container {{ max-width: 800px; margin: 0 auto; background: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ font-size: 2.5em; margin-bottom: 10px; }}
        .tienda-info {{ background-color: #f8f9fa; padding: 20px 30px; border-bottom: 3px solid #667eea; }}
        .tienda-info h2 {{ color: #333; font-size: 1.5em; margin-bottom: 5px; }}
        .tienda-info p {{ color: #666; font-size: 1em; }}
        .info-venta {{ display: flex; justify-content: space-between; padding: 20px 30px; background-color: #fff; }}
        .info-box {{ flex: 1; }}
        .info-box p {{ margin: 5px 0; color: #555; }}
        .info-box strong {{ color: #333; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
        th {{ background-color: #667eea; color: white; padding: 12px; text-align: left; font-weight: 600; }}
        td {{ padding: 12px; border-bottom: 1px solid #eee; color: #555; }}
        tr:last-child td {{ border-bottom: none; }}
        .cantidad {{ text-align: center; }}
        .precio, .subtotal {{ text-align: right; }}
        .totales {{ background-color: #f8f9fa; padding: 20px 30px; text-align: right; }}
        .total-row {{ font-size: 1.5em; font-weight: bold; color: #667eea; }}
        .footer {{ background-color: #333; color: white; padding: 20px; text-align: center; }}
        .footer p {{ margin: 5px 0; }}
        .gracias {{ font-size: 1.2em; font-weight: bold; color: #667eea; }}
    </style>
</head>
<body>
    <div class=""factura-container"">
        <div class=""header"">
            <h1>🧾 FACTURA</h1>
        </div>
        <div class=""tienda-info"">
            <h2>{tiendaNombre}</h2>
            <p>{tiendaDireccion}</p>
        </div>
        <div class=""info-venta"">
            <div class=""info-box"">
                <p><strong>Factura ID:</strong> {venta.Id}</p>
                <p><strong>Fecha:</strong> {fechaFormateada}</p>
            </div>
            <div class=""info-box"" style=""text-align: right;"">
                <p><strong>Cliente:</strong> {venta.ClienteNombre}</p>
                <p><strong>ID Cliente:</strong> {venta.ClienteId}</p>
            </div>
        </div>
        <table>
            <thead>
                <tr>
                    <th>Producto</th>
                    <th class=""cantidad"">Cantidad</th>
                    <th class=""precio"">Precio Unit.</th>
                    <th class=""subtotal"">Subtotal</th>
                </tr>
            </thead>
            <tbody>";

            foreach (var linea in venta.Lineas)
            {
                var subtotal = linea.Precio * linea.Cantidad;
                html += $@"
                <tr>
                    <td>{linea.ProductoNombre}</td>
                    <td class=""cantidad"">{linea.Cantidad}</td>
                    <td class=""precio"">{linea.Precio:C2}</td>
                    <td class=""subtotal"">{subtotal:C2}</td>
                </tr>";
            }

            html += $@"
            </tbody>
        </table>
        <div class=""totales"">
            <p class=""total-row"">TOTAL: {venta.Total:C2}</p>
        </div>
        <div class=""footer"">
            <p class=""gracias"">¡Gracias por su compra!</p>
            <p>{tiendaNombre} - {tiendaDireccion}</p>
        </div>
    </div>
</body>
</html>";

            _logger.Information("Factura HTML generada correctamente");
            return Result.Success<string, DomainError>(html);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al generar factura HTML");
            return Result.Failure<string, DomainError>(
                DomainErrors.Validation(new List<string> { $"Error al generar factura: {ex.Message}" }));
        }
    }

    public Result<bool, DomainError> GuardarFactura(string html, string fileName)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var directory = Path.Combine(basePath, AppConfig.FacturaDirectory);
        _logger.Information("Guardando factura en directorio {Directory}", directory);
        
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, html);
            
            _logger.Information("Factura guardada correctamente en {FilePath}", filePath);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al guardar factura");
            return Result.Failure<bool, DomainError>(
                DomainErrors.Validation(new List<string> { $"Error al guardar factura: {ex.Message}" }));
        }
    }
}
