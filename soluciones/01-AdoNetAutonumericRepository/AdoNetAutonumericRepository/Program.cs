using Microsoft.Data.Sqlite;
using AdoNetAutonumericRepository.Models;
using AdoNetAutonumericRepository.Repositories;

Console.WriteLine("=== ADO.NET Autonumeric Repository (In-Memory) ===");
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
Console.WriteLine();
Console.WriteLine("--- GetById(1) ---");
var persona1 = repository.GetById(1);
Console.WriteLine($"  {persona1?.Nombre}");

// Update
Console.WriteLine();
Console.WriteLine("--- Update(1) ---");
var updated = repository.Update(1, new Persona(1, "Ana García Modificada", "ana.mod@correo.com", DateTime.Now, DateTime.Now, false, null));
Console.WriteLine($"  Actualizado: {updated?.Nombre}");

// Delete lógico (por defecto)
Console.WriteLine();
Console.WriteLine("--- Delete(2) - Lógico ---");
var deleted = repository.Delete(2);
Console.WriteLine($"  Eliminado (IsDeleted={deleted?.IsDeleted}): {deleted?.Nombre}");

// GetAll final
Console.WriteLine();
Console.WriteLine("--- GetAll Final ---");
foreach (var p in repository.GetAll())
    Console.WriteLine($"  {p.Id}: {p.Nombre} (IsDeleted={p.IsDeleted})");

// Delete físico
Console.WriteLine();
Console.WriteLine("--- Delete(3) - Físico ---");
var deletedFisico = repository.Delete(3, false);
Console.WriteLine($"  Eliminado físico: {deletedFisico?.Nombre}");

// GetAll final
Console.WriteLine();
Console.WriteLine("--- GetAll Final (tras delete físico) ---");
foreach (var p in repository.GetAll())
    Console.WriteLine($"  {p.Id}: {p.Nombre} (IsDeleted={p.IsDeleted})");

// Casos incorrectos
Console.WriteLine();
Console.WriteLine("=== CASOS INCORRECTOS ===");

Console.WriteLine();
Console.WriteLine("--- Create - Email duplicado ---");

    var firstEmail = repository.GetAll().First().Email;
    var duplicateEmail = repository.Create(new Persona(0, "Juan Duplicate", firstEmail, DateTime.Now, DateTime.Now, false, null));
    Console.WriteLine($"  Resultado: {duplicateEmail?.ToString() ?? "null"}");


Console.WriteLine();
Console.WriteLine("--- GetById(999) - No encontrado ---");
var notFound = repository.GetById(999);
Console.WriteLine($"  Resultado: {notFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("--- Update(999) - No encontrado ---");
var updatedNotFound = repository.Update(999, new Persona(999, "Test", "test@correo.com", DateTime.Now, DateTime.Now, false, null));
Console.WriteLine($"  Resultado: {updatedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("--- Delete(999) - No encontrado ---");
var deletedNotFound = repository.Delete(999);
Console.WriteLine($"  Resultado: {deletedNotFound?.ToString() ?? "null"}");

Console.WriteLine();
Console.WriteLine("=== Fin ===");
