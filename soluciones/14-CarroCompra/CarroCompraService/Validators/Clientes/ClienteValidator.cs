using CSharpFunctionalExtensions;
using CarroCompraService.Models.Clientes;
using CarroCompraService.Errors;
using CarroCompraService.Validators.Common;
using Serilog;

namespace CarroCompraService.Validators.Clientes;

/// <summary>
/// Validador para la entidad Cliente.
/// </summary>
public class ClienteValidator : IValidador<Cliente>
{
    private readonly ILogger _logger = Log.ForContext<ClienteValidator>();

    /// <summary>
    /// Valida un cliente comprobando nombre, email y dirección.
    /// </summary>
    public Result<Cliente, DomainError> Validar(Cliente cliente)
    {
        _logger.Debug("Validando cliente: {Nombre}", cliente.Nombre);
        
        var errores = new List<string>();

        // Validar nombre
        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            errores.Add("El nombre es obligatorio.");
        else if (cliente.Nombre.Length < 2)
            errores.Add("El nombre debe tener al menos 2 caracteres.");

        // Validar email
        if (string.IsNullOrWhiteSpace(cliente.Email))
            errores.Add("El email es obligatorio.");
        else if (!cliente.Email.Contains('@'))
            errores.Add("El email debe ser válido.");

        // Validar dirección
        if (string.IsNullOrWhiteSpace(cliente.Direccion))
            errores.Add("La dirección es obligatoria.");

        if (errores.Any())
        {
            _logger.Warning("Validación fallida: {Errores}", string.Join(", ", errores));
            return Result.Failure<Cliente, DomainError>(DomainErrors.Validation(errores));
        }
        
        _logger.Debug("Validación correcta");
        return Result.Success<Cliente, DomainError>(cliente);
    }
}
