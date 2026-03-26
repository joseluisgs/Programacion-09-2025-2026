using CSharpFunctionalExtensions;
using CarroCompraService.Models.Productos;
using CarroCompraService.Errors;
using CarroCompraService.Validators.Common;
using Serilog;

namespace CarroCompraService.Validators.Productos;

/// <summary>
/// Validador para la entidad Producto.
/// </summary>
public class ProductoValidator : IValidador<Producto>
{
    private readonly ILogger _logger = Log.ForContext<ProductoValidator>();

    /// <summary>
    /// Valida un producto comprobando nombre, precio y stock.
    /// </summary>
    public Result<Producto, DomainError> Validar(Producto producto)
    {
        _logger.Debug("Validando producto: {Nombre}", producto.Nombre);
        
        var errores = new List<string>();

        // Validar nombre
        if (string.IsNullOrWhiteSpace(producto.Nombre))
            errores.Add("El nombre es obligatorio.");
        else if (producto.Nombre.Length < 2)
            errores.Add("El nombre debe tener al menos 2 caracteres.");
        else if (producto.Nombre.Length > 100)
            errores.Add("El nombre no puede exceder 100 caracteres.");

        // Validar precio
        if (producto.Precio < 0)
            errores.Add("El precio no puede ser negativo.");

        // Validar stock
        if (producto.Stock < 0)
            errores.Add("El stock no puede ser negativo.");

        // Si hay errores, devolver failure
        if (errores.Any())
        {
            _logger.Warning("Validación fallida: {Errores}", string.Join(", ", errores));
            return Result.Failure<Producto, DomainError>(DomainErrors.Validation(errores));
        }
        
        _logger.Debug("Validación correcta");
        return Result.Success<Producto, DomainError>(producto);
    }
}
