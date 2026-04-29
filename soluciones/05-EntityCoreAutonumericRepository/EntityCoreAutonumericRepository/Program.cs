using Microsoft.EntityFrameworkCore;
using EntityCoreAutonumericRepository.Models;
using EntityCoreAutonumericRepository.Repositories;
using EntityCoreAutonumericRepository.Data;

Console.WriteLine("=== Entity Framework Core Autonumeric Repository (In-Memory) ===");
Console.WriteLine();

// Crear DbContext con SQLite en memoria
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: "personas_db")
    .Options;

using var context = new AppDbContext(options);

// Inyectar dependencia manualmente
IPersonaRepository repository = new PersonaRepository(context);

// GetAll
Console.WriteLine("--- GetAll (Seed Data) ---");
foreach (var p in repository.GetAll())
    Console.WriteLine($"  {p.Id}: {p.Nombre} ({p.Email})");

// GetById
Console.WriteLine();
Console.WriteLine("--- GetById(1) ---");
var persona1 = repository.GetById(1);
Console.WriteLine($"  {persona1?.Nombre}");

// Create
Console.WriteLine();
Console.WriteLine("--- Create ---");
var newPersona = repository.Create(new Persona { Nombre = "Carlos López", Email = "carlos.lopez@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false });
Console.WriteLine($"  Creado: {newPersona?.Id}: {newPersona?.Nombre} ({newPersona?.Email})");

// Update
Console.WriteLine();
Console.WriteLine("--- Update(1) ---");
var updated = repository.Update(1, new Persona { Id = 1, Nombre = "Ana García Modificada", Email = "ana.mod@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false });
Console.WriteLine($"  Actualizado: {updated?.Nombre}");

// Delete
Console.WriteLine();
Console.WriteLine("--- Delete(2) ---");
var deleted = repository.Delete(2);
Console.WriteLine($"  Eliminado (IsDeleted={deleted?.IsDeleted}): {deleted?.Nombre}");

// GetAll final
Console.WriteLine();
Console.WriteLine("--- GetAll Final ---");
foreach (var p in repository.GetAll())
    Console.WriteLine($"  {p.Id}: {p.Nombre} (IsDeleted={p.IsDeleted})");

// Casos incorrectos
Console.WriteLine();
Console.WriteLine("=== CASOS INCORRECTOS ===");

Console.WriteLine();

Console.WriteLine("--- Create - Email duplicado ---");
try {
    var firstEmail = repository.GetAll().First().Email;
    var duplicateEmail = repository.Create(new Persona {
        Nombre = "Juan Duplicate", Email = firstEmail, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now,
        IsDeleted = false
    });
    Console.WriteLine($"  Resultado: {duplicateEmail?.ToString() ?? "null"}");
}
catch (Exception ex) {
    Console.WriteLine($"  Error: {ex.Message}, exception type: {ex.GetType()}");
    // Obtener detalles de la excepción de SQLite
    var sqliteEx = ex.InnerException;
    Console.WriteLine($"  SQLite Error Code: {sqliteEx?.HResult}, Message: {sqliteEx?.Message}");
}

// Queha pasado? En realidad, al usar InMemoryDatabase, no se aplican las restricciones de unicidad ni se lanzan excepciones como en SQLite real. Por eso, el resultado del intento de crear un email duplicado será exitoso y no se lanzará una excepción. Esto es una limitación del proveedor InMemory de EF Core, que no implementa todas las características de un motor de base de datos real. En un entorno de producción con SQLite o SQL Server,
// sí se lanzaría una excepción por violación de la restricción de unicidad.

Console.WriteLine();
Console.WriteLine("--- GetById(999) - No encontrado ---");
var notFound = repository.GetById(999);
Console.WriteLine($"  Resultado: {notFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("--- Update(999) - No encontrado ---");
var updatedNotFound = repository.Update(999, new Persona { Id = 999, Nombre = "Test", Email = "test@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false });
Console.WriteLine($"  Resultado: {updatedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("--- Delete(999) - No encontrado ---");
var deletedNotFound = repository.Delete(999);
Console.WriteLine($"  Resultado: {deletedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("=== Fin ===");
