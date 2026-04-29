using CSharpFunctionalExtensions;
using CarroCompraService.Models.Clientes;
using CarroCompraService.Repositories.Clientes;
using CarroCompraService.Validators.Common;
using CarroCompraService.Errors;
using Serilog;

namespace CarroCompraService.Services.Clientes;

/// <summary>
/// Servicio de gestión de clientes.
/// </summary>
public class ClientesService(
    IClienteRepository repository,
    IValidador<Cliente> validator
) : IClientesService
{
    private readonly ILogger _logger = Log.ForContext<ClientesService>();

    public Result<IEnumerable<Cliente>, DomainError> GetAll()
    {
        _logger.Information("Obteniendo todos los clientes");
        return Result.Success<IEnumerable<Cliente>, DomainError>(repository.GetAll());
    }

    public Result<Cliente, DomainError> GetById(long id)
    {
        _logger.Information("Buscando cliente Id={Id}", id);
        
        return Maybe.From(repository.GetById(id))
            .ToResult(DomainErrors.NotFound($"Cliente con id {id} no encontrado"));
    }

    public Result<Cliente, DomainError> Create(Cliente cliente)
    {
        _logger.Information("Creando cliente: {Nombre}", cliente.Nombre);
        
        return Result.Success<Cliente, DomainError>(cliente)
            .Bind(validator.Validar)
            .Bind(CheckEmailIsUnique)
            .Map(c => repository.Create(c)!)
            .Tap(c => _logger.Information("Creado: {Id}", c.Id))
            .TapError(LogError);
    }

    public Result<Cliente, DomainError> Update(long id, Cliente cliente)
    {
        _logger.Information("Actualizando Id={Id}", id);
        
        return CheckExists(id)
            .Bind(_ => validator.Validar(cliente))
            .Bind(c => CheckEmailIsUniqueForUpdate(id, c))
            .Map(c => repository.Update(id, c)!)
            .TapError(LogError);
    }

    public Result<Cliente, DomainError> Delete(long id, bool logical = true)
    {
        _logger.Information("Eliminando Id={Id}", id);
        
        return CheckExists(id)
            .Map(c => repository.Delete(id, logical)!)
            .TapError(LogError);
    }

    private Result<Cliente, DomainError> CheckExists(long id) =>
        Maybe.From(repository.GetById(id))
            .ToResult(DomainErrors.NotFound($"Cliente con id {id} no encontrado"));

    private Result<Cliente, DomainError> CheckEmailIsUnique(Cliente cliente) =>
        repository.GetByEmail(cliente.Email) is null
            ? Result.Success<Cliente, DomainError>(cliente)
            : Result.Failure<Cliente, DomainError>(DomainErrors.AlreadyExists(cliente.Email));

    private Result<Cliente, DomainError> CheckEmailIsUniqueForUpdate(long id, Cliente cliente)
    {
        var existente = repository.GetByEmail(cliente.Email);
        return (existente is null || existente.Id == id)
            ? Result.Success<Cliente, DomainError>(cliente)
            : Result.Failure<Cliente, DomainError>(DomainErrors.AlreadyExists(cliente.Email));
    }

    private void LogError(DomainError error)
    {
        switch (error) {
            case DomainError.Validation v: _logger.Error("Fallo Validación: {E}", string.Join(", ", v.Errors)); break;
            case DomainError.AlreadyExists e: _logger.Error("Fallo Unicidad: {Email} ya existe", e.Mensaje); break;
            case DomainError.NotFound n: _logger.Warning("Fallo Existencia: {Mensaje}", n.Mensaje); break;
        }
    }
}
