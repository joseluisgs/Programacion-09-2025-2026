- [3. Diseño de Entidades: Claves y Atributos](#3-diseño-de-entidades-claves-y-atributos)
  - [3.1. El Modelo Persona](#31-el-modelo-persona)
    - [3.1.1. ¿Por qué Persona?](#311-por-qué-persona)
  - [3.2. Claves Primarias: Autonumérico vs GUID](#32-claves-primarias-autonumérico-vs-guid)
    - [3.2.1. Claves Autonuméricas](#321-claves-autonuméricas)
    - [3.2.2. Claves GUID](#322-claves-guid)
    - [3.2.3. ¿Cuál Elegir?](#323-cuál-elegir)
  - [3.3. Atributos de Control](#33-atributos-de-control)
  - [3.4. Borrado Físico vs Borrado Lógico](#34-borrado-físico-vs-borrado-lógico)
    - [3.4.1. Borrado Físico](#341-borrado-físico)
    - [3.4.2. Borrado Lógico](#342-borrado-lógico)
    - [3.4.3. ¿Cuál Elegir?](#343-cuál-elegir)
  - [3.5. Resumen](#35-resumen)

# 3. Diseño de Entidades: Claves y Atributos

## 3.1. El Modelo Persona

### 3.1.1. ¿Por qué Persona?

Usaremos una entidad **Persona** porque es el modelo más genérico y extensible. Una persona puede ser:

- Un estudiante
- Un profesor
- Un empleado
- Un cliente

> 📝 **Nota del Profesor**: En diseño de software, siempre que sea posible, usa modelos genéricos que puedas especializar. Persona es la clase base perfecta porque representa lo que tienen en común todas las "personas" de tu sistema.

**Entidad Persona:**

```csharp
public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

**Tabla en SQL:**

```sql
CREATE TABLE Personas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Email TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    DeletedAt TEXT
);
```

---

## 3.2. Claves Primarias: Autonumérico vs GUID

La **clave primaria** es el identificador único de cada registro. Hay dos estrategias principales:

### 3.2.1. Claves Autonuméricas

La base de datos genera el valor automáticamente:

```sql
CREATE TABLE Personas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,  -- La BD genera el valor
    ...
);
```

**En C#:**

```csharp
public class Persona
{
    public int Id { get; set; }  // Se asigna al hacer INSERT
}
```

**Ventajas:**
- ✅ Unicidad garantizada (la BD controla la secuencia)
- ✅ Secuencial (fácil de ordenar, depurar)
- ✅ Espacio eficiente (4 bytes)

**Desventajas:**
- ❌ Desconocido hasta después de insertar
- ❌ Dificulta el testing
- ❌ Problemas con bases de datos distribuidas (sharding)

### 3.2.2. Claves GUID

La aplicación genera el valor:

```csharp
public class Persona
{
    public Guid Id { get; set; } = Guid.NewGuid();  // Se genera aquí
}
```

**En SQL:**

```sql
CREATE TABLE Personas (
    Id TEXT PRIMARY KEY,  -- GUID como texto
    ...
);
```

**Ventajas:**
- ✅ Conocido ANTES de insertar
- ✅ Distribuible (múltiples servidores pueden generar IDs sin coordinación)
- ✅ Testing fácil (IDs predecibles)

**Desventajas:**
- ❌ Más espacio (16 bytes vs 4 bytes)
- ❌ No secuencial (difícil ordenar por "orden de creación")
- ❌ Menos legible (550e8400-e29b-41d4-a716 vs 1)

### 3.2.3. ¿Cuál Elegir?

| Escenario | Recomendación | ¿Por qué? |
|-----------|---------------|-----------|
| Aplicación monolítica | Autonumérico | Más eficiente, más simple |
| Sistema distribuido (múltiples BDs) | GUID | Sin coordinación entre servidores |
| Testing con datos predecibles | GUID | Puedes hardcodear IDs |
| Necesitas ordenación por fecha creación | Autonumérico | Los IDs son secuenciales |
| IDs expuestos en URLs | GUID | No revela información (no saber cuántos usuarios hay) |
| Optimización de espacio crítica | Autonumérico | 4 bytes vs 16 bytes |

> 📝 **Nota del Profesor**: La regla de oro es **"autonumérico por defecto"**. Solo usa GUIDs si tienes un motivo específico (sistemas distribuidos, URLs, testing).

---

## 3.3. Atributos de Control

Toda entidad profesional debe tener campos de control de ciclo de vida:

| Campo | Propósito | Ejemplo |
|-------|-----------|---------|
| `CreatedAt` | Fecha de creación del registro | `2024-01-15 10:30:00` |
| `UpdatedAt` | Fecha de última modificación | `2024-01-16 14:20:00` |
| `IsDeleted` | Marca de borrado lógico | `true`/`false` |
| `DeletedAt` | Fecha de eliminación | `2024-01-17 09:00:00` |

```csharp
public class Persona
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }  // Nullable: null = no eliminado
}
```

> 💡 **Tip del Examinador**: Estos campos son obligatorios en cualquier sistema profesional. Son esenciales para auditoría y para implementar el borrado lógico.

---

## 3.4. Borrado Físico vs Borrado Lógico

Cuando eliminas un registro, ¿qué haces realmente?

### 3.4.1. Borrado Físico

Elimina permanentemente el registro de la base de datos:

```sql
DELETE FROM Personas WHERE Id = 1;
```

**Características:**

```
ANTES:                     DESPUÉS:
┌────┬────────┐           ┌────┬────────┐
│ Id │ Nombre │           │ Id │ Nombre │
├────┼────────┤           ├────┼────────┤
│ 1  │  Ana   │  DELETE   │ 2  │  Juan  │  ← El registro 1
│ 2  │  Juan  │ ───────►  │    │        │     desaparece
└────┴────────┘           └────┴────────┘
```

- ✅ Ahorra espacio en disco
- ✅ Consultas más simples
- ❌ Irreversible (no hay vuelta atrás)
- ❌ Sin historial

### 3.4.2. Borrado Lógico

Marca el registro como eliminado sin borrarlo:

```sql
UPDATE Personas SET IsDeleted = 1, DeletedAt = datetime('now') WHERE Id = 1;
```

**Características:**

```
ANTES:                     DESPUÉS:
┌────┬────────┬──────────┐  ┌────┬────────┬──────────┐
│ Id │ Nombre │IsDeleted │  │ Id │ Nombre │IsDeleted │
├────┼────────┼──────────┤  ├────┼────────┼──────────┤
│ 1  │  Ana   │    0     │  │ 1  │  Ana   │    1     │  ← Marcado
│ 2  │  Juan  │    0     │  │ 2  │  Juan  │    0     │     como eliminado
└────┴────────┴──────────┘  └────┴────────┴──────────┘

Consultas: SELECT * FROM Personas WHERE IsDeleted = 0  ← Solo activos
```

- ✅ Recuperable (puedes "restaurar")
- ✅ Historial completo
- ✅ Cumplimiento normativo (RGPD, auditoría)
- ❌ Ocupa más espacio (el registro sigue ahí)

### 3.4.3. ¿Cuál Elegir?

| Criterio | Borrado Físico | Borrado Lógico |
|----------|----------------|----------------|
| Auditoría | ❌ | ✅ |
| RGPD/Legal | ❌ | ✅ |
| Datos temporales (logs) | ✅ | ❌ |
| Rendimiento | ✅ | ⚠️ (necesita índice) |
| Recuperación | ❌ | ✅ |

> 📝 **Nota del Profesor**: Usa **borrado lógico por defecto** en el 95% de los casos. El espacio que ahorras con borrado físico no compensa la pérdida de datos y la imposibilidad de auditoría. Solo usa borrado físico en tablas de logs/métricas donde los datos antiguos realmente no importan.

---

## 3.5. Resumen

- La **clave primaria** puede ser autonumérica o GUID
- **Autonumérico** es más eficiente, **GUID** es más flexible
- Toda entidad debe tener **campos de control** (CreatedAt, UpdatedAt, IsDeleted)
- El **borrado lógico** es preferible en la mayoría de casos
- El modelo **Persona** tiene: Id, Nombre, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
