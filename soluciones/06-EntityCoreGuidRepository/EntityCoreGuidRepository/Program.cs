using Microsoft.EntityFrameworkCore;
using EntityCoreGuidRepository.Models;
using EntityCoreGuidRepository.Repositories;
using EntityCoreGuidRepository.Data;

Console.WriteLine("=== Entity Framework Core GUID Repository (In-Memory) ===");
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
var firstId = repository.GetAll().First().Id;
Console.WriteLine();
Console.WriteLine($"--- GetById({firstId}) ---");
var persona1 = repository.GetById(firstId);
Console.WriteLine($"  {persona1?.Nombre}");

// Update
Console.WriteLine();
Console.WriteLine("--- Update ---");
var updated = repository.Update(firstId, new Persona { Id = firstId, Nombre = "Ana García Modificada", Email = "ana.mod@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false });
Console.WriteLine($"  Actualizado: {updated?.Nombre}");

// Delete lógico
var secondId = repository.GetAll().ElementAt(1).Id;
Console.WriteLine();
Console.WriteLine($"--- Delete({secondId}) - Lógico ---");
var deleted = repository.Delete(secondId);
Console.WriteLine($"  Eliminado (IsDeleted={deleted?.IsDeleted}): {deleted?.Nombre}");

// Delete físico
var thirdId = repository.GetAll().ElementAt(1).Id;
Console.WriteLine();
Console.WriteLine($"--- Delete({thirdId}) - Físico ---");
var deletedFisico = repository.Delete(thirdId, false);
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
    var duplicateEmail = repository.Create(new Persona { Id = Guid.NewGuid(), Nombre = "Juan Duplicate", Email = firstEmail, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false });
    Console.WriteLine($"  Resultado: {duplicateEmail?.ToString() ?? "null"}");
} catch (Exception ex) {
    Console.WriteLine($"  Error: {ex.Message}");
}

var nonExistentGuid = Guid.NewGuid();
Console.WriteLine();
Console.WriteLine($"--- GetById({nonExistentGuid}) - No encontrado ---");
var notFound = repository.GetById(nonExistentGuid);
Console.WriteLine($"  Resultado: {notFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine($"--- Update({nonExistentGuid}) - No encontrado ---");
var updatedNotFound = repository.Update(nonExistentGuid, new Persona { Id = nonExistentGuid, Nombre = "Test", Email = "test@correo.com", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsDeleted = false });
Console.WriteLine($"  Resultado: {updatedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine($"--- Delete({nonExistentGuid}) - No encontrado ---");
var deletedNotFound = repository.Delete(nonExistentGuid);
Console.WriteLine($"  Resultado: {deletedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("=== Fin ===");
