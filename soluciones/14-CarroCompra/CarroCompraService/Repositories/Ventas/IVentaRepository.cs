using CarroCompraService.Models.Ventas;
using CarroCompraService.Repositories.Common;

namespace CarroCompraService.Repositories.Ventas;

/// <summary>
/// Interfaz de repositorio para Ventas.
/// </summary>
public interface IVentaRepository : ICrudRepository<Guid, Venta>
{
    IEnumerable<Venta> GetByClienteId(long clienteId);
}
