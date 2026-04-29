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
  - [3.5. Restricciones en Base de Datos vs Lógica de Aplicación](#35-restricciones-en-base-de-datos-vs-lógica-de-aplicación)
    - [3.5.1. Tipos de Restricciones en la BD](#351-tipos-de-restricciones-en-la-bd)
    - [3.5.2. Excepciones por Violación de Restricciones](#352-excepciones-por-violación-de-restricciones)
    - [3.5.3. ¿Restricciones en BD o en Software?](#353-restricciones-en-bd-o-en-software)
    - [3.5.4. El Compromiso: Defensa en Profundidad](#354-el-compromiso-defensa-en-profundidad)
  - [3.6. Resumen](#36-resumen)

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

## 3.5. Restricciones en Base de Datos vs Lógica de Aplicación

Una decisión crucial en diseño de bases de datos es **dónde** implementar las restricciones de integridad: ¿en la base de datos o en el código de la aplicación?

### 3.5.1. Tipos de Restricciones en la BD

Las restricciones (*constraints*) son reglas que la base de datos enforce para garantizar la integridad de los datos:

| Restricción | Descripción | Ejemplo SQL |
|-------------|-------------|-------------|
| **PRIMARY KEY** | Identificador único de cada registro | `PRIMARY KEY (Id)` |
| **NOT NULL** | Campo obligatorio | `Nombre TEXT NOT NULL` |
| **UNIQUE** | Valor no puede repetirse | `Email TEXT UNIQUE` |
| **CHECK** | Validación de valor | `Edad INTEGER CHECK (Edad >= 0)` |
| **FOREIGN KEY** | Referencia a otra tabla | `REFERENCES Tabla(Id)` |
| **DEFAULT** | Valor por defecto | `DEFAULT 0` |

**Ejemplo completo:**

```sql
CREATE TABLE Personas (
    -- PRIMARY KEY: identificador único
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- NOT NULL: campo obligatorio
    Nombre TEXT NOT NULL,
    
    -- UNIQUE: no puede haber dos personas con el mismo email
    Email TEXT UNIQUE,
    
    -- CHECK: edad no puede ser negativa
    Edad INTEGER CHECK (Edad >= 0),
    
    -- DEFAULT: si no se especifica, es 0
    Activo INTEGER DEFAULT 1,
    
    -- DEFAULT: fecha de creación automática
    CreatedAt TEXT DEFAULT (datetime('now'))
);

CREATE TABLE Empleados (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PersonaId INTEGER NOT NULL,
    Cargo TEXT NOT NULL,
    Salario REAL CHECK (Salario >= 0),
    
    -- FOREIGN KEY: asegura que PersonaId existe en Personas
    FOREIGN KEY (PersonaId) REFERENCES Personas(Id)
);
```

### 3.5.2. Excepciones por Violación de Restricciones

Cuando se viola una restricción, la base de datos lanza una **excepción** en el código C#. Debes capturar estas excepciones para manejar errores elegantemente:

| Restricción Violada | Tipo de Excepción | Ejemplo de Situación |
|---------------------|-------------------|----------------------|
| **PRIMARY KEY** | `SqliteException` (código 19) | Insertar dos registros con mismo Id |
| **UNIQUE** | `SqliteException` (código 19) | Insertar email que ya existe |
| **CHECK** | `SqliteException` (código 19) | Insertar edad negativa |
| **FOREIGN KEY** | `SqliteException` (código 19) | Insertar Empleado con PersonaId inexistente |
| **NOT NULL** | `SqliteException` (código 19) | Insertar sin Nombre |

**Ejemplo en C#:**

```csharp
try
{
    using var connection = new SqliteConnection(cadenaConexion);
    connection.Open();
    
    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Personas (Nombre, Email, Edad) 
        VALUES (@nombre, @email, @edad)";
    cmd.Parameters.AddWithValue("@nombre", "Ana");
    cmd.Parameters.AddWithValue("@email", "ana@email.com");
    cmd.Parameters.AddWithValue("@edad", -5);  // ❌ Violación CHECK
    
    cmd.ExecuteNonQuery();  // 💥 Lanza excepción
}
catch (SqliteException ex)
{
    // Código 19 = violation of constraint
    if (ex.SqliteErrorCode == 19)
    {
        Console.WriteLine("❌ Error: La edad no puede ser negativa");
    }
    else
    {
        Console.WriteLine($"❌ Error de base de datos: {ex.Message}");
    }
}
```

**Otro ejemplo - Email único:**

```csharp
try
{
    // Intentar insertar persona con email duplicado
    _repo.Insertar(new Persona { Nombre = "Juan", Email = "ana@email.com" });
}
catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
{
    //捕获 ошибку UNIQUE constraint
    Console.WriteLine("❌ Error: El email 'ana@email.com' ya está registrado");
}
```

> ⚠️ **Importante**: Si nocontrolas estas excepciones, tu aplicación **fallará** (crash). Debes siempre envolver operaciones de escritura en `try-catch` para proporcionar una experiencia de usuario adecuada.

### 3.5.3. ¿Restricciones en BD o en Software?

Esta es una decisión de diseño importante. Veamos las ventajas y desventajas:

| Enfoque | Ventajas | Desventajas |
|---------|----------|-------------|
| **Solo en BD** | ✅ Seguridad (nadie puede saltarse las reglas) | ❌ Mensajes de error menos claros |
| | ✅ Integridad garantizada | ❌ Dependencia de la tecnología BD |
| | ✅ Funciona con cualquier aplicación | ❌ Validación tardía (solo al insertar) |
| **Solo en Software** | ✅ Mejor experiencia de usuario | ❌ Validación puede saltarse |
| | ✅ Mensajes personalizados | ❌ múltiples apps = duplicar lógica |
| | ✅ Más rápido (no hay round-trip a BD) | ❌ No protege contra errores humanos |
| **Ambos (recomendado)** | ✅ defense in depth | ⚠️ Más código |

### 3.5.4. El Compromiso: Defensa en Profundidad

El enfoque recomendado es usar **ambos**: restricciones en la base de datos **Y** validación en software. Esto se llama "defensa en profundidad".

**Estructura recomendada:**

```csharp
public class PersonaValidator
{
    public (bool Valido, List<string> Errores) Validar(Persona persona)
    {
        var errores = new List<string>();
        
        // Validación de software (más rápida y con mejor mensaje)
        if (string.IsNullOrWhiteSpace(persona.Nombre))
            errores.Add("El nombre es obligatorio");
        
        if (persona.Email != null && !persona.Email.Contains('@'))
            errores.Add("El email no tiene formato válido");
        
        if (persona.Edad < 0 || persona.Edad > 150)
            errores.Add("La edad debe estar entre 0 y 150");
        
        return (errores.Count == 0, errores);
    }
}

// Uso
var (valido, errores) = _validator.Validar(persona);
if (!valido)
{
    foreach (var error in errores)
        Console.WriteLine($"❌ {error}");
    return;  // No llegar a la BD
}

// Si llegamos aquí, intentamos el INSERT
// La BD tiene restricciones como backup
try
{
    _repo.Insertar(persona);
}
catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
{
    Console.WriteLine("❌ Error de integridad en la base de datos");
}
```

**Tabla de decisiones:**

| Restricción | ¿Dónde implementarla? | ¿Por qué? |
|-------------|----------------------|-----------|
| PRIMARY KEY | **BD (automático)** | La BD garantiza unicidad |
| NOT NULL | **Ambos** | Software para UX, BD como backup |
| UNIQUE | **Ambos** | Software para verificar antes, BD como backup |
| CHECK | **BD obligatoriamente** | Solo la BD puede garantizarlo |
| FOREIGN KEY | **BD obligatoriamente** | Solo la BD garantiza integridad referencial |

> 📝 **Nota del Profesor**: Las restricciones CHECK y FOREIGN KEY **deben** estar en la base de datos porque no hay forma de guarantee su cumplimiento desde software (otras aplicaciones podrían insertar datos directamente en la BD). Sin embargo, las restricciones NOT NULL y UNIQUE pueden validarse en software para proporcionar mejores mensajes de error al usuario.

> 💡 **Tip del Examinador**: En el examen pueden preguntarte qué hacer cuando una restricción de la BD se viola. La respuesta debe ser: "Capturar la excepción SqliteException y mostrar un mensaje claro al usuario." También pueden preguntarte si es mejor validar en BD o en software - la respuesta correcta es "ambos" (defensa en profundidad).

---

## 3.6. Resumen

- La **clave primaria** puede ser autonumérica o GUID
- **Autonumérico** es más eficiente, **GUID** es más flexible
- Toda entidad debe tener **campos de control** (CreatedAt, UpdatedAt, IsDeleted)
- El **borrado lógico** es preferible en la mayoría de casos
- El modelo **Persona** tiene: Id, Nombre, Email, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
- Las **restricciones en BD** (PRIMARY KEY, UNIQUE, CHECK, FOREIGN KEY) protejen la integridad de los datos
- Las **excepciones** por violación de restricciones son del tipo `SqliteException` (código 19)
- El enfoque **defensa en profundidad** usa restricciones en BD **Y** validación en software
- Las restricciones **CHECK** y **FOREIGN KEY** deben estar obligatoriamente en la BD
