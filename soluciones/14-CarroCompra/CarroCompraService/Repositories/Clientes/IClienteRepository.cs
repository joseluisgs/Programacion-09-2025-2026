using CarroCompraService.Models.Clientes;
using CarroCompraService.Repositories.Common;

namespace CarroCompraService.Repositories.Clientes;

/// <summary>
/// Interfaz de repositorio para Clientes.
/// </summary>
public interface IClienteRepository : ICrudRepository<long, Cliente>
{
    Cliente? GetByEmail(string email);
}
