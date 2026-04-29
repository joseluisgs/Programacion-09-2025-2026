using CSharpFunctionalExtensions;
using CarroCompraService.Models.Ventas;
using CarroCompraService.Errors;

namespace CarroCompraService.Services.Ventas;

/// <summary>
/// Interfaz del servicio de Ventas.
/// </summary>
public interface IVentasService
{
    Result<IEnumerable<Venta>, DomainError> GetAll();
    Result<Venta, DomainError> GetById(Guid id);
    Result<Venta, DomainError> Create(Venta venta);
    Result<Venta, DomainError> Update(Guid id, Venta venta);
    Result<Venta, DomainError> Delete(Guid id, bool logical = true);
    Result<IEnumerable<Venta>, DomainError> GetByClienteId(long clienteId);
    Result<string, DomainError> GenerarFacturaHtml(Guid id);
}
