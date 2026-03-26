// Import del contenedor DI nativo de .NET
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
// Import de los modelos
using CarroCompraService.Models.Productos;
using CarroCompraService.Models.Clientes;
using CarroCompraService.Models.Ventas;
// Import de los repositorios
using CarroCompraService.Repositories.Productos;
using CarroCompraService.Repositories.Clientes;
using CarroCompraService.Repositories.Ventas;
// Import de los servicios
using CarroCompraService.Services.Productos;
using CarroCompraService.Services.Clientes;
using CarroCompraService.Services.Ventas;
// Import de los validadores
using CarroCompraService.Validators.Productos;
using CarroCompraService.Validators.Clientes;
using CarroCompraService.Validators.Ventas;
using CarroCompraService.Validators.Common;
// Import de la caché
using CarroCompraService.Cache;
// Import de la configuración
using CarroCompraService.Config;
// Import del DbContext
using CarroCompraService.Data;
// Import de servicios de factura
using CarroCompraService.Services.Factura;
// Alias para evitar conflicto con namespace
using IProductoRepository = CarroCompraService.Repositories.Productos.IProductoRepository;
using IClienteRepository = CarroCompraService.Repositories.Clientes.IClienteRepository;
using IVentaRepository = CarroCompraService.Repositories.Ventas.IVentaRepository;
using ProductoRepository = CarroCompraService.Repositories.Productos.ProductoRepository;
using ClienteRepository = CarroCompraService.Repositories.Clientes.ClienteRepository;
using VentaRepository = CarroCompraService.Repositories.Ventas.VentaRepository;
using IValidadorProducto = CarroCompraService.Validators.Common.IValidador<CarroCompraService.Models.Productos.Producto>;
using IValidadorCliente = CarroCompraService.Validators.Common.IValidador<CarroCompraService.Models.Clientes.Cliente>;
using IValidadorVenta = CarroCompraService.Validators.Common.IValidador<CarroCompraService.Models.Ventas.Venta>;

namespace CarroCompraService.Infrastructure;

/// <summary>
/// Proveedor centralizado de dependencias.
/// Configura todos los servicios según appsettings.json.
/// </summary>
public static class DependenciesProvider
{
    /// <summary>
    /// Método principal que registra todos los servicios en el contenedor DI
    /// </summary>
    public static IServiceProvider BuildServiceProvider()
    {
        // 1. Carga la configuración desde appsettings.json
        AppConfig.Load();
        
        // 2. Crea la colección de servicios
        var services = new ServiceCollection();
        
        // 3. Registra EF Core (para Productos y Ventas)
        RegisterEfCoreSqlite(services);
        
        // 4. Registra Dapper (para Clientes)
        RegisterDapperSqlite(services);
        
        // 5. Registra servicios comunes (validadores, cache, servicios)
        RegisterCommonServices(services);
        
        // 6. Construye y retorna el ServiceProvider
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Registra EF Core con SQLite archivo (para Productos y Ventas)
    /// </summary>
    private static void RegisterEfCoreSqlite(IServiceCollection services)
    {
        Console.WriteLine("📦 Registrando: EF Core + SQLite");
        
        // DbContext con SQLite - se crea una instancia por scoped request
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(AppConfig.ConnectionString);
        });
        
        // Repositorio Productos con EF Core
        services.AddScoped<IProductoRepository, ProductoRepository>();
        
        // Repositorio Ventas con EF Core
        services.AddScoped<IVentaRepository, VentaRepository>();
    }

    /// <summary>
    /// Registra Dapper con SQLite archivo (para Clientes)
    /// </summary>
    private static void RegisterDapperSqlite(IServiceCollection services)
    {
        Console.WriteLine("📦 Registrando: Dapper + SQLite");
        
        // Connection factory - crea una nueva conexión por cada scoped request
        services.AddScoped<SqliteConnection>(_ =>
        {
            var connection = new SqliteConnection(AppConfig.ConnectionString);
            connection.Open();
            return connection;
        });
        
        // Repositorio Clientes con Dapper
        services.AddScoped<IClienteRepository, ClienteRepository>();
    }

    /// <summary>
    /// Registra servicios comunes
    /// </summary>
    private static void RegisterCommonServices(IServiceCollection services)
    {
        // Validadores - una instancia por request (Scoped)
        services.AddScoped<IValidadorProducto, ProductoValidator>();
        services.AddScoped<IValidadorCliente, ClienteValidator>();
        services.AddScoped<IValidadorVenta, VentaValidator>();
        
        // Cache LRU - UNA SOLA INSTANCIA para toda la app (Singleton)
        services.AddSingleton<ICache<long, Producto>>(new LruCache<long, Producto>(AppConfig.CacheSize));
        
        // Servicios - una instancia por request (Scoped)
        services.AddScoped<IProductosService, ProductosService>();
        services.AddScoped<IClientesService, ClientesService>();
        services.AddScoped<IVentasService, VentasService>();
        
        // Storage
        services.AddTransient<IFacturaService, FacturaService>();
    }
}
