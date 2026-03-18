using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using PersonaService.Config;
using PersonaService.Infrastructure;
using PersonaService.Repositories;
using PersonaService.Services;
using PersonaService.Models;
using PersonaService.Errors;
using Serilog;
using Svc = PersonaService.Services.PersonaService;

Console.WriteLine("=== Persona Service con DI Automatizada + ROP ===");
Console.WriteLine();

// Configurar Serilog
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger = loggerConfiguration;

// Mostrar configuración
Console.WriteLine($"📋 Proveedor: {AppConfig.Provider}");
Console.WriteLine($"📋 ConnectionString: {AppConfig.ConnectionString}");
Console.WriteLine($"📋 CreateTable: {AppConfig.CreateTable}");
Console.WriteLine($"📋 DropData: {AppConfig.DropData}");
Console.WriteLine($"📋 SeedData: {AppConfig.SeedData}");
Console.WriteLine();

// Build ServiceProvider con DI
// Al resolver IPersonaRepository, se ejecutará Initialize() y SeedData() en el constructor
var serviceProvider = DependenciesProvider.BuildServiceProvider();

// Obtener servicios desde DI
using var scope = serviceProvider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IPersonaService>();

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== GET ALL ===");
Console.WriteLine("═══════════════════════════════════════════");
foreach (var p in service.GetAll())
    Console.WriteLine($"  {p}");

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== GET BY ID(1) ===");
Console.WriteLine("═══════════════════════════════════════════");
service.GetById(1).Match(
    p => Console.WriteLine($"  ✓ Encontrado: {p}"),
    e => Console.WriteLine($"  ✗ Error: {e.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== CREATE - Válido ===");
Console.WriteLine("═══════════════════════════════════════════");
var nueva = new Persona { Nombre = "Carlos Ruiz", Email = "carlos@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false };
service.Create(nueva).Match(
    p => Console.WriteLine($"  ✓ Creado: {p}"),
    e => Console.WriteLine($"  ✗ Error: {e.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== CREATE - Email Duplicado ===");
Console.WriteLine("═══════════════════════════════════════════");
var dup = new Persona { Nombre = "Ana Duplicate", Email = "ana@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false };
service.Create(dup).Match(
    p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
    e => Console.WriteLine($"  ✓ Error esperado: {e.Message}"));

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== GET ALL FINAL ===");
Console.WriteLine("═══════════════════════════════════════════");
foreach (var p in service.GetAll())
    Console.WriteLine($"  {p}");

Console.WriteLine();
Console.WriteLine("=== Fin ===");

Log.CloseAndFlush();
