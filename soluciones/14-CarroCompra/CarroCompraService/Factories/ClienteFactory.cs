using CarroCompraService.Models.Clientes;

namespace CarroCompraService.Factories;

public static class ClienteFactory
{
    public static List<Cliente> Seed()
    {
        return new List<Cliente>
        {
            new() { Nombre = "Ana García", Email = "ana@correo.com", Direccion = "Calle Mayor 1" },
            new() { Nombre = "Juan Pérez", Email = "juan@correo.com", Direccion = "Avenida Barcelona 25" },
            new() { Nombre = "María López", Email = "maria@correo.com", Direccion = "Plaza España 3" },
            new() { Nombre = "Carlos Ruiz", Email = "carlos@correo.com", Direccion = "Calle Gran Vía 10" }
        };
    }
}
