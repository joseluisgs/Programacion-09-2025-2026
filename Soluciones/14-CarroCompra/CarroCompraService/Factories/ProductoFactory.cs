using CarroCompraService.Models.Productos;

namespace CarroCompraService.Factories;

// Factory para crear datos de prueba de Productos
public static class ProductoFactory
{
    // Crea una lista de productos de prueba
    public static List<Producto> Seed()
    {
        return new List<Producto>
        {
            new() { Nombre = "Portátil HP", Precio = 599.99, Stock = 10, Categoria = Categoria.ELECTRONICA },
            new() { Nombre = "Ratón Inalámbrico", Precio = 19.99, Stock = 50, Categoria = Categoria.ELECTRONICA },
            new() { Nombre = "Teclado Mecánico", Precio = 89.99, Stock = 25, Categoria = Categoria.ELECTRONICA },
            new() { Nombre = "Balón de Fútbol", Precio = 24.99, Stock = 100, Categoria = Categoria.DEPORTE },
            new() { Nombre = "Raqueta de Tenis", Precio = 49.99, Stock = 30, Categoria = Categoria.DEPORTE },
            new() { Nombre = "Camiseta Deportivo", Precio = 15.99, Stock = 200, Categoria = Categoria.DEPORTE },
            new() { Nombre = "Vaqueros Levi's", Precio = 79.99, Stock = 60, Categoria = Categoria.MODA },
            new() { Nombre = "Zapatillas Nike", Precio = 119.99, Stock = 40, Categoria = Categoria.MODA },
            new() { Nombre = "Gorra Baseball", Precio = 12.99, Stock = 150, Categoria = Categoria.MODA },
            new() { Nombre = "Auriculares Sony", Precio = 149.99, Stock = 35, Categoria = Categoria.ELECTRONICA }
        };
    }
}
