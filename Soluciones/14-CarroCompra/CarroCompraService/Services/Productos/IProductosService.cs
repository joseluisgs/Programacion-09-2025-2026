using CSharpFunctionalExtensions;
using CarroCompraService.Models.Productos;
using CarroCompraService.Errors;

namespace CarroCompraService.Services.Productos;

/// <summary>
/// Interfaz del servicio de Productos.
/// </summary>
public interface IProductosService
{
    Result<IEnumerable<Producto>, DomainError> GetAll();
    Result<Producto, DomainError> GetById(long id);
    Result<Producto, DomainError> Create(Producto producto);
    Result<Producto, DomainError> Update(long id, Producto producto);
    Result<Producto, DomainError> Delete(long id, bool logical = true);
    Result<IEnumerable<Producto>, DomainError> GetByCategoria(Categoria categoria);
    Result<IEnumerable<Producto>, DomainError> GetByNombre(string nombre);
    Result<IEnumerable<Producto>, DomainError> GetAllPaginated(int page, int size);
}
