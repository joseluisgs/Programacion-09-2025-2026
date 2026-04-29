using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using PersonaService.Models;
using PersonaService.Repositories;
using PersonaService.Services;
using PersonaService.Validators;
using PersonaService.Cache;
using PersonaService.Data;
using PersonaService.Errors;
using Serilog;
using Service = PersonaService.Services.PersonaService;

// Configuración de Serilog
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger = loggerConfiguration;

Console.WriteLine("=== Persona Service con Validador, Cache y ROP (Result Pattern) ===");
Console.WriteLine();

// Crear DbContext con SQLite en memoria
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: "personas_db")
    .Options;

using var context = new AppDbContext(options);

// Inyección manual de dependencias
IPersonaRepository repository = new PersonaRepository(context);
IValidador<Persona> validador = new ValidadorPersona();
ICache<int, Persona> cache = new LruCache<int, Persona>(3);
IPersonaService service = new Service(repository, validador, cache);

// ==================== GET ALL ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== GET ALL (Seed Data) ===");
Console.WriteLine("═══════════════════════════════════════════");
foreach (var p in service.GetAll())
    Console.WriteLine($"  {p}");

// ==================== GET BY ID - EXISTE ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== GET BY ID(1) - Existe ===");
Console.WriteLine("═══════════════════════════════════════════");
service.GetById(1).Match(
    onSuccess: p => Console.WriteLine($"  ✓ Encontrado: {p}"),
    onFailure: e => Console.WriteLine($"  ✗ Error: {e.Message}"));

// ==================== GET BY ID - NO EXISTE ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== GET BY ID(999) - NO Existe ===");
Console.WriteLine("═══════════════════════════════════════════");
service.GetById(999).Match(
    onSuccess: p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
    onFailure: e => Console.WriteLine($"  ✓ Error esperado: {e.Message}"));

// ==================== GET BY ID - DESDE CACHE ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== GET BY ID(1) - Desde Cache ===");
Console.WriteLine("═══════════════════════════════════════════");
service.GetById(1).Match(
    onSuccess: p => Console.WriteLine($"  ✓ Encontrado (desde cache): {p}"),
    onFailure: e => Console.WriteLine($"  ✗ Error: {e.Message}"));


var res = service.GetById(1);

if (res.IsSuccess)
    Console.WriteLine($"  ✓ Encontrado (desde cache): {res.Value}");
else
    Console.WriteLine($"  ✗ Error: {res.Error.Message}");

// ==================== CREATE - VALIDO ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== CREATE - Válido ===");
Console.WriteLine("═══════════════════════════════════════════");
var nueva = new Persona { Nombre = "Carlos Ruiz", Email = "carlos@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false };
service.Create(nueva).Match(
    onSuccess: p => Console.WriteLine($"  ✓ Creado: {p}"),
    onFailure: e => Console.WriteLine($"  ✗ Error: {e.Message}"));

// ==================== CREATE - EMAIL DUPLICADO ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== CREATE - Email Duplicado ===");
Console.WriteLine("═══════════════════════════════════════════");
var dup = new Persona { Nombre = "Ana Duplicate", Email = "ana@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false };
service.Create(dup).Match(
    onSuccess: p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
    onFailure: e => Console.WriteLine($"  ✓ Error esperado: {e.Message}"));

// ==================== CREATE - VALIDACION FALLIDA ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== CREATE - Validación Fallida ===");
Console.WriteLine("═══════════════════════════════════════════");
var inv = new Persona { Nombre = "", Email = "invalido", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false };
service.Create(inv).Match(
    onSuccess: p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
    onFailure: e => 
    {
        Console.WriteLine($"  ✓ Error esperado: {e.Message}");
        if (e is PersonaError.Validation validation)
            foreach (var err in validation.Errors)
                Console.WriteLine($"    - {err}");
    });

// ==================== UPDATE - VALIDO ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== UPDATE - Válido ===");
Console.WriteLine("═══════════════════════════════════════════");
service.Update(1, new Persona { Nombre = "Ana Actualizada", Email = "ana.actualizada@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false }).Match(
    onSuccess: p => Console.WriteLine($"  ✓ Actualizado: {p}"),
    onFailure: e => Console.WriteLine($"  ✗ Error: {e.Message}"));

// ==================== UPDATE - NO EXISTE ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== UPDATE - No Existe ===");
Console.WriteLine("═══════════════════════════════════════════");
service.Update(999, new Persona { Nombre = "NoExiste", Email = "noexiste@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false }).Match(
    onSuccess: p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
    onFailure: e => Console.WriteLine($"  ✓ Error esperado: {e.Message}"));

// ==================== UPDATE - EMAIL DUPLICADO ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== UPDATE - Email Duplicado ===");
Console.WriteLine("═══════════════════════════════════════════");
// Intentar cambiar el email de persona 2 al email de persona 1
service.Update(2, new Persona { Nombre = "Juan Perez", Email = "ana.actualizada@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false }).Match(
    onSuccess: p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
    onFailure: e => Console.WriteLine($"  ✓ Error esperado: {e.Message}"));

// ==================== DELETE LOGICO - EXISTE ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== DELETE LÓGICO(2) - Existe ===");
Console.WriteLine("═══════════════════════════════════════════");
service.Delete(2).Match(
    onSuccess: p => Console.WriteLine($"  ✓ Eliminado (lógico): {p}"),
    onFailure: e => Console.WriteLine($"  ✗ Error: {e.Message}"));

// ==================== DELETE LOGICO - NO EXISTE ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== DELETE LÓGICO(999) - No Existe ===");
Console.WriteLine("═══════════════════════════════════════════");
service.Delete(999).Match(
    onSuccess: p => Console.WriteLine($"  ✗ Error: debería haber fallado"),
    onFailure: e => Console.WriteLine($"  ✓ Error esperado: {e.Message}"));

// ==================== DELETE FISICO ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== DELETE FÍSICO(3) ===");
Console.WriteLine("═══════════════════════════════════════════");
service.Delete(3, false).Match(
    onSuccess: p => Console.WriteLine($"  ✓ Eliminado (físico): {p}"),
    onFailure: e => Console.WriteLine($"  ✗ Error: {e.Message}"));

// ==================== GET ALL FINAL ====================
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("=== GET ALL FINAL ===");
Console.WriteLine("═══════════════════════════════════════════");
foreach (var p in service.GetAll())
    Console.WriteLine($"  {p}");

Console.WriteLine();
Console.WriteLine("=== Fin ===");

Log.CloseAndFlush();
