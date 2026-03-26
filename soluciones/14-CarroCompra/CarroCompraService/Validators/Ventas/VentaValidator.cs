using CSharpFunctionalExtensions;
using CarroCompraService.Models.Ventas;
using CarroCompraService.Errors;
using CarroCompraService.Validators.Common;
using Serilog;

namespace CarroCompraService.Validators.Ventas;

/// <summary>
/// Validador para la entidad Venta.
/// </summary>
public class VentaValidator : IValidador<Venta>
{
    private readonly ILogger _logger = Log.ForContext<VentaValidator>();

    /// <summary>
    /// Valida una venta comprobando cliente y líneas.
    /// </summary>
    public Result<Venta, DomainError> Validar(Venta venta)
    {
        _logger.Debug("Validando venta: {Id}", venta.Id);
        
        var errores = new List<string>();

        // Validar cliente
        if (venta.ClienteId <= 0)
            errores.Add("El cliente es obligatorio.");

        // Validar líneas
        if (venta.Lineas == null || !venta.Lineas.Any())
            errores.Add("La venta debe tener al menos una línea.");

        // Validar cada línea
        if (venta.Lineas != null)
        {
            foreach (var linea in venta.Lineas)
            {
                if (linea.Cantidad <= 0)
                    errores.Add($"La cantidad del producto '{linea.ProductoNombre}' debe ser mayor a 0.");
                if (linea.Precio < 0)
                    errores.Add($"El precio del producto '{linea.ProductoNombre}' no puede ser negativo.");
            }
        }

        if (errores.Any())
        {
            _logger.Warning("Validación fallida: {Errores}", string.Join(", ", errores));
            return Result.Failure<Venta, DomainError>(DomainErrors.Validation(errores));
        }
        
        _logger.Debug("Validación correcta");
        return Result.Success<Venta, DomainError>(venta);
    }
}
