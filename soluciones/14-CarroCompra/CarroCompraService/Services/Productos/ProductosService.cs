using CSharpFunctionalExtensions;
using CarroCompraService.Models.Productos;
using CarroCompraService.Repositories.Productos;
using CarroCompraService.Validators.Common;
using CarroCompraService.Cache;
using CarroCompraService.Errors;
using CarroCompraService.Extensions;
using Serilog;

namespace CarroCompraService.Services.Productos;

/// <summary>
/// Servicio de gestión de productos.
/// </summary>
public class ProductosService(
    IProductoRepository repository,
    IValidador<Producto> validator,
    ICache<long, Producto> cache
) : IProductosService
{
    private readonly ILogger _logger = Log.ForContext<ProductosService>();

    public Result<IEnumerable<Producto>, DomainError> GetAll()
    {
        _logger.Information("Obteniendo todos los productos");
        return Result.Success<IEnumerable<Producto>, DomainError>(repository.GetAll());
    }

    public Result<Producto, DomainError> GetById(long id)
    {
        _logger.Information("Buscando producto Id={Id}", id);
        
        return Maybe.From(cache.Get(id))
            .ToResult(DomainErrors.NotFound($"Producto con id {id} no encontrado"))
            .Tap(_ => _logger.Information("[CACHE] Encontrado"))
            .OnFailureCompensate(_ => GetFromRepository(id));
    }

    public Result<Producto, DomainError> Create(Producto producto)
    {
        _logger.Information("Creando producto: {Nombre}", producto.Nombre);
        
        return Result.Success<Producto, DomainError>(producto)
            .Bind(validator.Validar)
            .Bind(CheckNombreIsUnique)
            .Map(p => repository.Create(p)!)
            .Tap(p => _logger.Information("Creado: {Id}", p.Id))
            .TapError(LogError);
    }

    public Result<Producto, DomainError> Update(long id, Producto producto)
    {
        _logger.Information("Actualizando Id={Id}", id);
        
        return CheckExists(id)
            .Bind(_ => validator.Validar(producto))
            .Bind(p => CheckNombreIsUniqueForUpdate(id, p))
            .Map(p => repository.Update(id, p)!)
            .Tap(_ => cache.Remove(id))
            .TapError(LogError);
    }

    public Result<Producto, DomainError> Delete(long id, bool logical = true)
    {
        _logger.Information("Eliminando Id={Id}", id);
        
        return CheckExists(id)
            .Map(p => repository.Delete(id, logical)!)
            .Tap(_ => cache.Remove(id))
            .TapError(LogError);
    }

    public Result<IEnumerable<Producto>, DomainError> GetByCategoria(Categoria categoria)
    {
        _logger.Information("Buscando por categoria: {Categoria}", categoria);
        return Result.Success<IEnumerable<Producto>, DomainError>(repository.GetByCategoria(categoria));
    }

    public Result<IEnumerable<Producto>, DomainError> GetByNombre(string nombre)
    {
        _logger.Information("Buscando por nombre: {Nombre}", nombre);
        return Result.Success<IEnumerable<Producto>, DomainError>(repository.GetByNombre(nombre));
    }

    public Result<IEnumerable<Producto>, DomainError> GetAllPaginated(int page, int size)
    {
        _logger.Information("Obteniendo paginados: page={Page}, size={Size}", page, size);
        return Result.Success<IEnumerable<Producto>, DomainError>(repository.GetAllPaginated(page, size));
    }

    private Result<Producto, DomainError> GetFromRepository(long id) =>
        Maybe.From(repository.GetById(id))
            .ToResult(DomainErrors.NotFound($"Producto con id {id} no encontrado"))
            .Tap(p => cache.Put(id, p))
            .Tap(_ => _logger.Information("[DB] Encontrado y cacheado"));

    private Result<Producto, DomainError> CheckExists(long id) =>
        Maybe.From(repository.GetById(id))
            .ToResult(DomainErrors.NotFound($"Producto con id {id} no encontrado"));

    private Result<Producto, DomainError> CheckNombreIsUnique(Producto producto) =>
        repository.FindByNombre(producto.Nombre) is null
            ? Result.Success<Producto, DomainError>(producto)
            : Result.Failure<Producto, DomainError>(DomainErrors.AlreadyExists(producto.Nombre));

    private Result<Producto, DomainError> CheckNombreIsUniqueForUpdate(long id, Producto producto)
    {
        var existente = repository.FindByNombre(producto.Nombre);
        return (existente is null || existente.Id == id)
            ? Result.Success<Producto, DomainError>(producto)
            : Result.Failure<Producto, DomainError>(DomainErrors.AlreadyExists(producto.Nombre));
    }

    private void LogError(DomainError error)
    {
        switch (error) {
            case DomainError.Validation v: _logger.Error("Fallo Validación: {E}", string.Join(", ", v.Errors)); break;
            case DomainError.AlreadyExists e: _logger.Error("Fallo Unicidad: {Nombre} ya existe", e.Mensaje); break;
            case DomainError.NotFound n: _logger.Warning("Fallo Existencia: {Mensaje}", n.Mensaje); break;
        }
    }
}
