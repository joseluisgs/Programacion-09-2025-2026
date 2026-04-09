using EntityCoreSqliteRepository.Data;
using EntityCoreSqliteRepository.Models;
using EntityCoreSqliteRepository.Repositories;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== Entity Framework Core SQLite Repository ===");
Console.WriteLine();

// Crear DbContext con SQLite en archivo (la forma A, usando el constructor con opciones, pero sin usar un contenedor de DI)
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=personas.db")
    .Options;

using var context = new AppDbContext(options);

// Crear DbContext con SQLite en memoria, de la forma B
//using var context = new AppDbContext();

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

// Update
Console.WriteLine();
Console.WriteLine("--- Update(1) ---");
var updated = repository.Update(1,
    new Persona {
        Id = 1, Nombre = "Ana García Modificada", Email = "ana.mod@correo.com", CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now, IsDeleted = false
    });
Console.WriteLine($"  Actualizado: {updated?.Nombre}");

// Delete lógico
Console.WriteLine();
Console.WriteLine("--- Delete(2) - Lógico ---");
var deleted = repository.Delete(2);
Console.WriteLine($"  Eliminado (IsDeleted={deleted?.IsDeleted}): {deleted?.Nombre}");

// Delete físico
Console.WriteLine();
Console.WriteLine("--- Delete(3) - Físico ---");
var deletedFisico = repository.Delete(3, false);
Console.WriteLine($"  Eliminado físico: {deletedFisico?.Nombre}");

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

Console.WriteLine();
Console.WriteLine("--- GetById(999) - No encontrado ---");
var notFound = repository.GetById(999);
Console.WriteLine($"  Resultado: {notFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("--- Update(999) - No encontrado ---");
var updatedNotFound = repository.Update(999,
    new Persona {
        Id = 999, Nombre = "Test", Email = "test@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now,
        IsDeleted = false
    });
Console.WriteLine($"  Resultado: {updatedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("--- Delete(999) - No encontrado ---");
var deletedNotFound = repository.Delete(999);
Console.WriteLine($"  Resultado: {deletedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("=== Fin ===");