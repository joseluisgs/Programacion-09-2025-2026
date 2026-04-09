- [4. Tecnologías de Acceso a Datos](#4-tecnologías-de-acceso-a-datos)
  - [4.1. La Pirámide de Acceso a Datos](#41-la-pirámide-de-acceso-a-datos)
  - [4.2. ADO.NET](#42-adonet)
    - [4.2.1. ¿Qué es ADO.NET?](#421-qué-es-adonet)
    - [4.2.2. Componentes Principales](#422-componentes-principales)
    - [4.2.3. Ejemplo de Uso](#423-ejemplo-de-uso)
  - [4.3. Dapper (Micro-ORM)](#43-dapper-micro-orm)
    - [4.3.1. Características](#431-características)
  - [4.4. Entity Framework Core (ORM)](#44-entity-framework-core-orm)
  - [4.5. Comparativa](#45-comparativa)
  - [4.6. Resumen](#46-resumen)

# 4. Tecnologías de Acceso a Datos

## 4.1. La Pirámide de Acceso a Datos

En .NET tenemos tres niveles de abstracción para acceder a bases de datos:

```
                         ┌─────────────────────┐
                         │   EF Core (ORM)     │  ← Máximo dinamismo, más overhead
                         │   • Abstracción total│
                         │   • SQL automático   │
                         │   • Migraciones     │
                         ├─────────────────────┤
                         │      Dapper         │  ← Balance ideal velocidad/flexibilidad
                         │  (Micro-ORM)       │
                         │   • SQL manual      │
                         │   • Mapping auto    │
                         │   • Muy rápido     │
                         ├─────────────────────┤
                         │     ADO.NET         │  ← Control total, más código
                         │   • SQL 100% manual │
                         │   • Mapping manual  │
                         │   • Rendimiento    │
                         └─────────────────────┘
                         
         ▲ SUBE             NIVEL DE ABSTRACCIÓN           ▲ BAJA
         │                                                     │
         │  + Código         - Flexibilidad                   │
         │  - Flexibilidad  + Control                        │
         └───────────────────────────────────────────────────┘
```

> 📝 **Nota del Profesor**: No hay una tecnología "mejor" que otra. La elección depende de tus necesidades:
> - ¿Priorizas velocidad de desarrollo? → EF Core
> - ¿Priorizas rendimiento con control SQL? → Dapper
> - ¿Necesitas control absoluto? → ADO.NET

---

## 4.2. ADO.NET

### 4.2.1. ¿Qué es ADO.NET?

**ADO.NET** es la API base de .NET para conectarse a bases de datos relacionales. Es la tecnología más "cercana al metal".

> 📝 **Nota del Profesor**: ADO.NET existe desde .NET Framework 1.0 (2002). Aunque tiene más de 20 años, sigue siendo la base sobre la que funcionan Dapper y EF Core. Entender ADO.NET te ayuda a entender cómo funcionan las capas superiores.

### 4.2.2. Componentes Principales

| Componente | Propósito |
|------------|-----------|
| `SqliteConnection` | Establece conexión a la BD |
| `SqliteCommand` | Representa una sentencia SQL |
| `SqliteDataReader` | Lee resultados de una consulta (forward-only) |
| `SqliteParameter` | Parámetro seguro para SQL |

### 4.2.3. Ejemplo de Uso

```csharp
using Microsoft.Data.Sqlite;

string connectionString = "Data Source=escuela.db";

// 1. CONEXIÓN
using var connection = new SqliteConnection(connectionString);
connection.Open();
Console.WriteLine($"✓ Conectado a: {connection.DataSource}");

// 2. INSERT
using var insertCmd = connection.CreateCommand();
insertCmd.CommandText = "INSERT INTO Personas (Nombre, Email) VALUES (@nombre, @email)";
insertCmd.Parameters.AddWithValue("@nombre", "Ana");
insertCmd.Parameters.AddWithValue("@email", "ana@correo.com");
insertCmd.ExecuteNonQuery();
Console.WriteLine("✓ Persona insertada");

// 3. SELECT
using var queryCmd = connection.CreateCommand();
queryCmd.CommandText = "SELECT Id, Nombre, Email FROM Personas";

using var reader = queryCmd.ExecuteReader();
while (reader.Read())
{
    int id = reader.GetInt32(0);
    string nombre = reader.GetString(1);
    string? email = reader.IsDBNull(2) ? null : reader.GetString(2);
    
    Console.WriteLine($"  [{id}] {nombre} - {email}");
}
```

---

## 4.3. Dapper (Micro-ORM)

### 4.3.1. Características

**Dapper** es un micro-ORM creado por **Stack Overflow**. Es "más que un mapper" pero más ligero que EF Core.

**Características:**
- Mapping automático de resultados a objetos
- SQL 100% manual
- Rendimiento extremadamente alto
- Código muy conciso

```csharp
using Dapper;

// Mapping automático (sin boilerplate)
var personas = connection.Query<Persona>("SELECT * FROM Personas");

// Parámetros seguros (automático)
var persona = connection.QueryFirstOrDefault<Persona>(
    "SELECT * FROM Personas WHERE Id = @Id", 
    new { Id = 1 });
```

> 📝 **Nota del Profesor**: Stack Overflow procesa miles de millones de consultas al mes con Dapper. Si lo usan para la página más visitada del mundo, es suficientemente rápido para tu proyecto.

---

## 4.4. Entity Framework Core (ORM)

**EF Core** es el ORM completo de Microsoft.

```csharp
using Microsoft.EntityFrameworkCore;

// Entidad
public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}

// DbContext
public class AppDbContext : DbContext
{
    public DbSet<Persona> Personas { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=escuela.db");
}

// Uso
using var context = new AppDbContext();

// INSERT (genera SQL automáticamente)
context.Personas.Add(new Persona { Nombre = "Ana" });
context.SaveChanges();

// SELECT (LINQ → SQL)
var personas = context.Personas
    .Where(p => p.Nombre.StartsWith("A"))
    .ToList();
```

---

## 4.5. Comparativa

| Aspecto | ADO.NET | Dapper | EF Core |
|---------|---------|--------|---------|
| **Líneas de código** | Más (~50) | Medio (~20) | Menos (~15) |
| **Mapeo manual** | ✅ Sí | ❌ No | ❌ No |
| **SQL** | 100% manual | 100% manual | Automático |
| **Rendimiento** | Rápido | Muy rápido | Rápido |
| **Curva aprendizaje** | Baja | Baja | Media |
| **Flexibilidad** | Máxima | Alta | Media |

**Cuándo usar cada uno:**

| Situación | Tecnología recomendada |
|-----------|----------------------|
| SQL muy complejo/específico | Dapper |
| Máximo rendimiento crítico | Dapper o ADO.NET |
| Desarrollo rápido | EF Core |
| Necesitas migraciones | EF Core |
| Legacy / mantenimiento | ADO.NET o Dapper |
| Sistema distribuido | Dapper |

> 💡 **Tip del Examinador**: En el examen, si no te especifican qué usar, **Dapper** es el término medio más seguro. Tienes control SQL sin el boilerplate de ADO.NET.

---

## 4.6. Resumen

- Existen **tres niveles** de acceso a datos: ADO.NET, Dapper, EF Core
- **ADO.NET** ofrece control total pero requiere mucho código
- **Dapper** es el punto medio ideal: SQL manual con mapping automático
- **EF Core** abstrae completamente el SQL
- La elección depende de las necesidades de control y rendimiento
