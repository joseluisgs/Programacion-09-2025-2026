using CSharpFunctionalExtensions;
using CarroCompraService.Models.Ventas;
using CarroCompraService.Repositories.Ventas;
using CarroCompraService.Validators.Common;
using CarroCompraService.Services.Factura;
using CarroCompraService.Errors;
using CarroCompraService.Extensions;
using Serilog;

namespace CarroCompraService.Services.Ventas;

/// <summary>
/// Servicio de gestión de ventas.
/// </summary>
public class VentasService(
    IVentaRepository repository,
    IValidador<Venta> validator,
    IFacturaService facturaService
) : IVentasService
{
    private readonly ILogger _logger = Log.ForContext<VentasService>();

    public Result<IEnumerable<Venta>, DomainError> GetAll()
    {
        _logger.Information("Obteniendo todas las ventas");
        return Result.Success<IEnumerable<Venta>, DomainError>(repository.GetAll());
    }

    public Result<Venta, DomainError> GetById(Guid id)
    {
        _logger.Information("Buscando venta Id={Id}", id);
        
        return Maybe.From(repository.GetById(id))
            .ToResult(DomainErrors.NotFound($"Venta con id {id} no encontrada"));
    }

    public Result<Venta, DomainError> Create(Venta venta)
    {
        _logger.Information("Creando venta para cliente {ClienteId}", venta.ClienteId);
        
        return Result.Success<Venta, DomainError>(venta)
            .Bind(validator.Validar)
            .Map(v => repository.Create(v)!)
            .Tap(v => _logger.Information("Creada: {Id}", v.Id))
            .TapError(LogError);
    }

    public Result<Venta, DomainError> Update(Guid id, Venta venta)
    {
        _logger.Information("Actualizando Id={Id}", id);
        
        return CheckExists(id)
            .Bind(_ => validator.Validar(venta))
            .Map(v => repository.Update(id, v)!)
            .TapError(LogError);
    }

    public Result<Venta, DomainError> Delete(Guid id, bool logical = true)
    {
        _logger.Information("Eliminando Id={Id}", id);
        
        return CheckExists(id)
            .Map(v => repository.Delete(id, logical)!)
            .TapError(LogError);
    }

    public Result<IEnumerable<Venta>, DomainError> GetByClienteId(long clienteId)
    {
        _logger.Information("Buscando ventas para cliente {ClienteId}", clienteId);
        return Result.Success<IEnumerable<Venta>, DomainError>(repository.GetByClienteId(clienteId));
    }

    public Result<string, DomainError> GenerarFacturaHtml(Guid id)
    {
        _logger.Information("Generando factura HTML para venta {Id}", id);
        
        return GetById(id)
            .Bind(facturaService.GenerarFacturaHtml)
            .Tap(html => _logger.Information("Factura HTML generada"));
    }

    private Result<Venta, DomainError> CheckExists(Guid id) =>
        Maybe.From(repository.GetById(id))
            .ToResult(DomainErrors.NotFound($"Venta con id {id} no encontrada"));

    private void LogError(DomainError error)
    {
        switch (error) {
            case DomainError.Validation v: _logger.Error("Fallo Validación: {E}", string.Join(", ", v.Errors)); break;
            case DomainError.NotFound n: _logger.Warning("Fallo Existencia: {Mensaje}", n.Mensaje); break;
        }
    }
}
