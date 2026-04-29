using Microsoft.Extensions.DependencyInjection;
using CSharpFunctionalExtensions;
using CarroCompraService.Infrastructure;
using CarroCompraService.Services.Productos;
using CarroCompraService.Services.Clientes;
using CarroCompraService.Services.Ventas;
using CarroCompraService.Services.Factura;
using CarroCompraService.Models.Productos;
using CarroCompraService.Models.Clientes;
using CarroCompraService.Models.Ventas;
using CarroCompraService.Errors;
using Serilog;
using System.Diagnostics;
using CarroCompraService.Config;

Console.WriteLine("=== CarroCompra Service con DI ===");
Console.WriteLine();

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger = loggerConfiguration;

var serviceProvider = DependenciesProvider.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var productosService = scope.ServiceProvider.GetRequiredService<IProductosService>();
var clientesService = scope.ServiceProvider.GetRequiredService<IClientesService>();
var ventasService = scope.ServiceProvider.GetRequiredService<IVentasService>();
var facturaService = scope.ServiceProvider.GetRequiredService<IFacturaService>();

Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== PRODUCTOS: GET ALL ===");
Console.WriteLine("═══════════════════════════════════════════");
productosService.GetAll()
    .Match(
        onSuccess: productos => 
        {
            foreach (var p in productos)
                Console.WriteLine($"  {p}");
        },
        onFailure: error => Console.WriteLine($"  ✗ Error: {error.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== PRODUCTOS: GET BY ID (1) ===");
Console.WriteLine("═══════════════════════════════════════════");
productosService.GetById(1)
    .Match(
        onSuccess: p => Console.WriteLine($"  ✓ Encontrado: {p}"),
        onFailure: error => Console.WriteLine($"  ✗ Error: {error.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== PRODUCTOS: GET BY ID (99 - NO EXISTE) ===");
Console.WriteLine("═══════════════════════════════════════════");
productosService.GetById(99)
    .Match(
        onSuccess: p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
        onFailure: error => Console.WriteLine($"  ✓ Error esperado: {error.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== PRODUCTOS: CREATE (VÁLIDO) ===");
Console.WriteLine("═══════════════════════════════════════════");
var nuevoProducto = new Producto { Nombre = "Nuevo Producto", Precio = 99.99, Stock = 10, Categoria = Categoria.OTROS };
productosService.Create(nuevoProducto)
    .Match(
        onSuccess: p => Console.WriteLine($"  ✓ Creado: {p}"),
        onFailure: error => Console.WriteLine($"  ✗ Error: {error.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== PRODUCTOS: CREATE (INVÁLIDO) ===");
Console.WriteLine("═══════════════════════════════════════════");
var productoInvalido = new Producto { Nombre = "X", Precio = -10, Stock = 10, Categoria = Categoria.OTROS };
productosService.Create(productoInvalido)
    .Match(
        onSuccess: p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
        onFailure: error => Console.WriteLine($"  ✓ Error esperado: {error.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== CLIENTES: GET ALL ===");
Console.WriteLine("═══════════════════════════════════════════");
clientesService.GetAll()
    .Match(
        onSuccess: clientes => 
        {
            foreach (var c in clientes)
                Console.WriteLine($"  {c}");
        },
        onFailure: error => Console.WriteLine($"  ✗ Error: {error.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== CLIENTES: CREATE (VÁLIDO) ===");
Console.WriteLine("═══════════════════════════════════════════");
var nuevoCliente = new Cliente { Nombre = "Nuevo Cliente", Email = "nuevo@cliente.com", Direccion = "Calle Nueva 123" };
clientesService.Create(nuevoCliente)
    .Match(
        onSuccess: c => Console.WriteLine($"  ✓ Creado: {c}"),
        onFailure: error => Console.WriteLine($"  ✗ Error: {error.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== VENTAS: CREAR ===");
Console.WriteLine("═══════════════════════════════════════════");

var clientes = clientesService.GetAll().GetValueOrDefault();
var productos = productosService.GetAll().GetValueOrDefault();

var clienteResult = clientes?.FirstOrDefault();
var producto1 = productos?.ElementAtOrDefault(0);
var producto2 = productos?.ElementAtOrDefault(1);

if (clienteResult != null && producto1 != null && producto2 != null)
{
    var venta = new Venta
    {
        ClienteId = clienteResult.Id,
        ClienteNombre = clienteResult.Nombre,
        Lineas = new List<LineaVenta>
        {
            new() { ProductoId = producto1.Id, ProductoNombre = producto1.Nombre, ProductoPrecio = producto1.Precio, Cantidad = 2, Precio = producto1.Precio },
            new() { ProductoId = producto2.Id, ProductoNombre = producto2.Nombre, ProductoPrecio = producto2.Precio, Cantidad = 1, Precio = producto2.Precio }
        }
    };
    
    Console.WriteLine($"  Venta a crear: {venta}");
    Console.WriteLine($"  Total: {venta.Total}");
    
    ventasService.Create(venta)
        .Match(
            onSuccess: v => Console.WriteLine($"  ✓ Venta creada: {v}"),
            onFailure: error => Console.WriteLine($"  ✗ Error: {error.Message}"));
}

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== VENTAS: GET ALL ===");
Console.WriteLine("═══════════════════════════════════════════");
ventasService.GetAll()
    .Match(
        onSuccess: ventas => 
        {
            foreach (var v in ventas)
                Console.WriteLine($"  {v}");
        },
        onFailure: error => Console.WriteLine($"  ✗ Error: {error.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== VENTAS: GENERAR Y GUARDAR FACTURA HTML ===");
Console.WriteLine("═══════════════════════════════════════════");
var primeraVenta = ventasService.GetAll().GetValueOrDefault()?.FirstOrDefault();
if (primeraVenta != null)
{
    ventasService.GenerarFacturaHtml(primeraVenta.Id)
        .Bind(html => facturaService.GuardarFactura(html, $"factura_{primeraVenta.Id}.html"))
        .Match(
            onSuccess: _ => 
            {
                Console.WriteLine($"  ✓ Factura guardada en Facturas/factura_{primeraVenta.Id}.html");
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var directory = Path.Combine(basePath, AppConfig.FacturaDirectory);
                var rutaAbsoluta = Path.Combine(directory, $"factura_{primeraVenta.Id}.html");
                // Abrir la factura en el navegador
                Process.Start(new ProcessStartInfo
                {
                    FileName = rutaAbsoluta, // Ruta absoluta al archivo HTML
                    UseShellExecute = true // Para abrir con la aplicación predeterminada (navegador)
                });
                Console.WriteLine($"  ✓ Factura abierta en el navegador");
            },
            onFailure: error => Console.WriteLine($"  ✗ Error: {error.Message}"));
}
else
{
    Console.WriteLine("  ✗ No hay ventas para generar factura");
}

Console.WriteLine();
Console.WriteLine("=== Fin ===");

Log.CloseAndFlush();
