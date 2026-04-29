using Microsoft.Data.Sqlite;
using AdoNetGuidRepository.Models;
using AdoNetGuidRepository.Repositories;

Console.WriteLine("=== ADO.NET GUID Repository (In-Memory) ===");
Console.WriteLine();

// Crear conexión SQLite en memoria (SINGLETON para persistencia)
var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();

// Inyectar dependencia manualmente
IPersonaRepository repository = new PersonaRepository(connection);

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
var updated = repository.Update(firstId, new Persona(firstId, "Ana García Modificada", "ana.mod@correo.com", DateTime.Now, DateTime.Now, false, null));
Console.WriteLine($"  Actualizado: {updated?.Nombre}");

// Delete
var secondId = repository.GetAll().ElementAt(1).Id;
Console.WriteLine();
Console.WriteLine($"--- Delete({secondId}) ---");
var deleted = repository.Delete(secondId);
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
    var duplicateEmail = repository.Create(new Persona(Guid.NewGuid(), "Juan Duplicate", firstEmail, DateTime.Now, DateTime.Now, false, null));
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
var updatedNotFound = repository.Update(nonExistentGuid, new Persona(nonExistentGuid, "Test", "test@correo.com", DateTime.Now, DateTime.Now, false, null));
Console.WriteLine($"  Resultado: {updatedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine($"--- Delete({nonExistentGuid}) - No encontrado ---");
var deletedNotFound = repository.Delete(nonExistentGuid);
Console.WriteLine($"  Resultado: {deletedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("=== Fin ===");
