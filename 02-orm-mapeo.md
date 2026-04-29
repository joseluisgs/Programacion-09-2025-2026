- [2. Mapeo Objeto-Relacional (ORM)](#2-mapeo-objeto-relacional-orm)
  - [2.1. ¿Qué es un ORM?](#21-qué-es-un-orm)
    - [2.1.1. Definición](#211-definición)
    - [2.1.2. Cómo funciona](#212-cómo-funciona)
  - [2.2. Ventajas de Usar un ORM](#22-ventajas-de-usar-un-orm)
  - [2.3. Entity Framework Core](#23-entity-framework-core)
    - [2.3.1. ¿Qué es EF Core?](#231-qué-es-ef-core)
    - [2.3.2. Instalación (NuGet)](#232-instalación-nuget)
  - [2.4. Comparativa: Sin ORM vs Con ORM](#24-comparativa-sin-orm-vs-con-orm)
    - [2.4.1. Código comparativo](#241-código-comparativo)
  - [2.5. Resumen](#25-resumen)

# 2. Mapeo Objeto-Relacional (ORM)

## 2.1. ¿Qué es un ORM?

### 2.1.1. Definición

Un **ORM** (Object-Relational Mapper) es una librería que automatiza la traducción entre el mundo de objetos (C#) y el mundo relacional (SQL).

> 📝 **Nota del Profesor**: Piensa en el ORM como un traductor simultáneo en una cumbre internacional. Tú hablas en tu idioma (C#) y él traduce a SQL para la base de datos, y viceversa.

### 2.1.2. Cómo funciona

```
┌─────────────────────────────────────────┐
│  Aplicación C# (Objetos)                │
│                                         │
│  var estudiante = new Estudiante {      │
│      Nombre = "Ana",                    │
│      Calificacion = 8.5                 │
│  };                                     │
│                                         │
│  context.Estudiantes.Add(estudiante);   │
│  context.SaveChanges();                 │
└─────────────┬───────────────────────────┘
              │
              ▼ (Traducción automática)
┌─────────────────────────────────────────┐
│  ORM (Entity Framework Core)            │
│  • Mapeo automático Objeto ↔ Tabla      │
│  • Generación automática de SQL         │
│  • Change Tracking (seguimiento)        │
└─────────────┬───────────────────────────┘
              ▼
┌─────────────────────────────────────────┐
│  Base de Datos (SQLite)                 │
│                                         │
│  INSERT INTO Estudiantes (Nombre,       │
│      Calificacion) VALUES ('Ana', 8.5); │
└─────────────────────────────────────────┘
```

---

## 2.2. Ventajas de Usar un ORM

| Ventaja | Sin ORM (ADO.NET) | Con ORM (EF Core) |
|---------|-------------------|-------------------|
| **Productividad** | Escribir SQL manualmente | Código C# limpio |
| **Seguridad** | Propenso a SQL Injection | Parametrización automática |
| **Mantenimiento** | Cambiar esquema → reescribir código | Cambios centralizados |
| **Testabilidad** | Difícil mockar SQL | Fácil usar BD en memoria |
| **Consultas** | Strings SQL concatenados | LINQ type-safe |
| **Portabilidad** | Dependiente del motor SQL | Cambio de motor sin código |

> 📝 **Nota del Profesor**: La mayor ventaja del ORM es que **piensas en objetos, no en tablas**. Te permite centrarte en la lógica de negocio y olvidar los detalles de SQL.

---

## 2.3. Entity Framework Core

### 2.3.1. ¿Qué es EF Core?

**Entity Framework Core** es el ORM oficial de Microsoft para .NET moderno (Core, 5, 6, 7, 8, 9, 10).

**Características principales:**

- ✅ **Código limpio**: Trabaja con objetos, no con SQL
- ✅ **LINQ**: Consultas type-safe en C#
- ✅ **Múltiples proveedores**: SQLite, SQL Server, PostgreSQL, MySQL
- ✅ **Migraciones**: Versionado del esquema de BD
- ✅ **Change Tracking**: Detecta automáticamente qué cambió
- ✅ **Lazy/Eager Loading**: Control sobre cuándo cargar relaciones

### 2.3.2. Instalación (NuGet)

```bash
# Paquete base
dotnet add package Microsoft.EntityFrameworkCore

# Proveedor SQLite
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Proveedor InMemory (para testing)
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# Herramientas de diseño
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

## 2.4. Comparativa: Sin ORM vs Con ORM

### 2.4.1. Código comparativo

**❌ SIN ORM (ADO.NET manual):**

```csharp
public Estudiante GetById(int id)
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();
    
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT Id, Nombre, Calificacion FROM Estudiantes WHERE Id = @id";
    command.Parameters.AddWithValue("@id", id);
    
    using var reader = command.ExecuteReader();
    if (reader.Read())
    {
        return new Estudiante
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Calificacion = reader.GetDouble(2)
        };
    }
    return null;
}
```

**✅ CON ORM (Entity Framework Core):**

```csharp
public Estudiante? GetById(int id)
{
    return context.Estudiantes.FirstOrDefault(e => e.Id == id);
}
```

**Comparativa visual:**

```
Líneas de código:
───────────────────────────────────────────────────────────
ADO.NET:   ████████████████████████████████████████████ 25 líneas
Dapper:    ████████████████ 10 líneas
EF Core:   ██████ 3 líneas
```

> 💡 **Tip del Examinador**: Menos código no siempre significa mejor. EF Core es más lento que ADO.NET puro para consultas muy complejas, pero para el 90% de los casos, la diferencia es negligible y la productividad justifica el uso del ORM.

---

## 2.5. Resumen

- Un **ORM** automatiza la conversión entre objetos y tablas
- **EF Core** es el ORM oficial de Microsoft
- Los ORMs mejoran productividad y seguridad
- El código es más legible y mantenible
- Existen alternativas como **Dapper** (micro-ORM) para más control
- Elige ORM cuando priorices productividad, ADO.NET/Dapper cuando priorices control
