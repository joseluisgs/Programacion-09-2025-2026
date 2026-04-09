using CarroCompraService.Models.Productos;
using CarroCompraService.Repositories.Common;

namespace CarroCompraService.Repositories.Productos;

/// <summary>
/// Interfaz de repositorio para Productos.
/// </summary>
public interface IProductoRepository : ICrudRepository<long, Producto>
{
    IEnumerable<Producto> GetByCategoria(Categoria categoria);
    IEnumerable<Producto> GetByNombre(string nombre);
    IEnumerable<Producto> GetAllPaginated(int page, int size);
    Producto? FindByNombre(string nombre);
}
