using CSharpFunctionalExtensions;
using CarroCompraService.Models.Clientes;
using CarroCompraService.Errors;

namespace CarroCompraService.Services.Clientes;

/// <summary>
/// Interfaz del servicio de Clientes.
/// </summary>
public interface IClientesService
{
    Result<IEnumerable<Cliente>, DomainError> GetAll();
    Result<Cliente, DomainError> GetById(long id);
    Result<Cliente, DomainError> Create(Cliente cliente);
    Result<Cliente, DomainError> Update(long id, Cliente cliente);
    Result<Cliente, DomainError> Delete(long id, bool logical = true);
}
