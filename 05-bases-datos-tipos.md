- [5. Tipos de Bases de Datos](#5-tipos-de-bases-de-datos)
  - [5.1. SQLite en Fichero](#51-sqlite-en-fichero)
    - [5.1.1. Características](#511-características)
    - [5.1.2. Uso](#512-uso)
  - [5.2. SQLite en Memoria](#52-sqlite-en-memoria)
    - [5.2.1. Características](#521-características)
    - [5.2.2. Uso](#522-uso)
  - [5.3. EF Core InMemory](#53-ef-core-inmemory)
    - [5.3.1. Características](#531-características)
    - [5.3.2. Uso](#532-uso)
  - [5.4. Comparativa](#54-comparativa)
  - [5.5. Transparencia: Cambiar de Motor](#55-transparencia-cambiar-de-motor)
    - [5.5.1. ¿Por qué es importante?](#551-por-qué-es-importante)
    - [5.5.2. Implementación](#552-implementación)
  - [5.6. Resumen](#56-resumen)

# 5. Tipos de Bases de Datos

## 5.1. SQLite en Fichero

### 5.1.1. Características

**SQLite** es un motor de base de datos **embebido**. No necesita un servidor separado; todo está en un archivo.

```
┌─────────────────────────────────────┐
│         Tu Programa                 │
│  ┌─────────────────────────────┐    │
│  │     Aplicación C#           │    │
│  └──────────────┬──────────────┘    │
│                 │                   │
│                 ▼                   │
│  ┌─────────────────────────────┐    │
│  │     Motor SQLite            │    │
│  │  (Incluido en la app)       │    │
│  └──────────────┬──────────────┘    │
│                 │                   │
└─────────────────▼───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│   ┌───────────────────────────┐     │
│   │   escuela.db              │     │  ← Archivo en disco
│   └───────────────────────────┘     │
└─────────────────────────────────────┘
```

**Ventajas:**
- ✅ Persistente: Los datos sobreviven al cierre de la app
- ✅ Portable: El archivo se puede copiar/enviar
- ✅ Sin instalación: No necesita servidor
- ✅ Apto para producción

**Desventajas:**
- ❌ Más lento que un servidor dedicado
- ❌ Concurrencia limitada

### 5.1.2. Uso

```csharp
options.UseSqlite("Data Source=escuela.db");
//                           ↑ Archivo físico en disco
```

> ⚠️ **Importante - Seed Data**: Al usar archivos, los datos **persisten** entre ejecuciones. En el repositorio:
> 1. **Verificar si existen datos** antes de insertar seed
> 2. **No duplicar** los registros en cada ejecución
> 
> ```csharp
> private void SeedData()
> {
>     // Verificar si ya existen datos
>     var count = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Personas");
>     if (count > 0) return;  // Ya hay datos, no insertar
>     
>     foreach (var persona in PersonaFactory.Seed())
>         Create(persona);
> }
> ```

---

## 5.2. SQLite en Memoria

### 5.2.1. Características

La base de datos vive **completamente en RAM**. Se destruye al cerrar la conexión.

```
┌─────────────────────────────────────┐
│         Tu Programa                 │
│  ┌─────────────────────────────┐    │
│  │     Aplicación C#           │    │
│  └──────────────┬──────────────┘    │
│                 │                   │
│                 ▼                   │
│  ┌─────────────────────────────┐    │
│  │     Motor SQLite            │    │
│  │     (en memoria)            │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
         │                  │
         │ Los datos se    │
         │ pierden al      │
         ▼ cerrar          ▼
    ┌─────────────────────────────┐
    │         RAM                 │
    │  ┌───────────────────────┐  │
    │  │  Datos temporales     │  │
    │  └───────────────────────┘  │
    └─────────────────────────────┘
```

**Ventajas:**
- ✅ Ultra-rápido (sin acceso a disco)
- ✅ Ideal para testing

**Desventajas:**
- ❌ **Volátil**: Los datos se pierden al cerrar
- ❌ No apta para producción

### 5.2.2. Uso

```csharp
// ⚠️ BASIC: Se destruye al cerrar la conexión (datos perdidos)
options.UseSqlite("Data Source=:memory:");

// ✅ CON SHARED CACHE: Comparte entre conexiones
options.UseSqlite("Data Source=:memory:;Mode=Memory;Cache=Shared");
```

> 📝 **Nota del Profesor**: 
> - **`Data Source=:memory:`** → Cada conexión tiene su propia BD. Si cierras la conexión, pierdes los datos.
> - **Con Dapper**: Usa **Singleton** para la conexión, sino los datos desaparecen.
> - **Con EF Core**: El proveedor gestiona internamente la conexión y funciona bien con Scoped.
> - **Shared Cache**: Útil para testing con múltiples conexiones.

---

## 5.3. EF Core InMemory

### 5.3.1. Características

**NO es SQLite**. Es una base de datos **simulada** que no usa SQL real.

```
┌─────────────────────────────────────┐
│         Tu Programa                 │
│  ┌─────────────────────────────┐    │
│  │     Aplicación C#           │    │
│  └──────────────┬──────────────┘    │
│                 │                   │
│                 ▼                   │
│  ┌─────────────────────────────┐    │
│  │  EF Core InMemory Provider  │    │
│  │  (Simulación, no hay SQL)   │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

**Ventajas:**
- ✅ Muy rápido (ni siquiera hay motor SQL)
- ✅ Perfecto para pruebas unitarias
- ✅ API estilo LINQ

**Desventajas:**
- ❌ **No es SQL real**: No valida restricciones FK, CHECK
- ❌ Comportamiento puede diferir de SQLite/SQL Server
- ❌ No para integration tests

### 5.3.2. Uso

```csharp
// Instalación
dotnet add package Microsoft.EntityFrameworkCore.InMemory

// Configuración
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("TestDb")  // ← Nombre de la BD
    .Options;

using var context = new AppDbContext(options);
```

> 📝 **Nota del Profesor**: 
> - **EF Core InMemory** NO es SQLite real, es una simulación.
> - **Gestión interna**: EF Core gestiona la conexión internamente, no dependes de una conexión externa.
> - **Scoped funciona**: A diferencia de SQLite :memory:, puedes usar Scoped sin problemas.
> - **Ideal para testing**: No necesita configuración de BD real.

---

## 5.4. Comparativa

| Aspecto | SQLite Fichero | SQLite Memoria | EF Core InMemory |
|---------|---------------|----------------|------------------|
| **Persistencia** | ✅ Sí | ❌ No | ❌ No |
| **Velocidad** | Media | Alta | Muy Alta |
| **Testing** | ⚠️ Posible | ✅ Ideal | ✅ Ideal |
| **Restricciones SQL** | ✅ Sí | ✅ Sí | ❌ No |
| **Producción** | ✅ Sí | ❌ No | ❌ No |
| **DI recomendado** | Scoped | **Singleton** ⚠️ | Scoped ✅ |

> ⚠️ **Importante**: SQLite `:memory:` necesita **Singleton** para mantener datos. EF Core InMemory gestiona internamente y funciona con Scoped.

---

## 5.5. Transparencia: Cambiar de Motor

### 5.5.1. ¿Por qué es importante?

La lógica de negocio **no debe depender** del motor de BD específico:

```
┌─────────────────────────────────────────────┐
│              Tu Servicio                    │
│  No sabe si usa SQLite, SQL Server, etc.    │
│  Solo conoce IRepository                    │
└─────────────────────┬───────────────────────┘
                      │
         ┌────────────┴────────────┐
         │                        │
         ▼                        ▼
   ┌──────────┐           ┌──────────┐
   │  SQLite  │           │ InMemory │
   │   .db    │           │  (Test)  │
   └──────────┘           └──────────┘
   
   Mismo código, diferentes motores
```

### 5.5.2. Implementación

**appsettings.json:**

```json
{
  "Database": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=personas.db"
  }
}
```

**Program.cs:**

```csharp
var builder = Host.CreateApplicationBuilder(args);

// Leer configuración
var config = builder.Configuration;
var provider = config["Database:Provider"];
var connectionString = config["Database:ConnectionString"];

// Registrar DbContext según configuración
builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (provider)
    {
        case "Sqlite": 
            options.UseSqlite(connectionString); 
            break;
        case "InMemory": 
            options.UseInMemoryDatabase("TestDb"); 
            break;
        case "SqlServer":
            options.UseSqlServer(connectionString);
            break;
    }
});
```

> 💡 **Tip del Examinador**: Este patrón te permite desarrollar con SQLite (rápido, local) y luego cambiar a SQL Server (producción) sin modificar código de negocio.

---

## 5.6. Resumen

- **SQLite en ficheo**: Persistente, para producción
- **SQLite en memoria**: Volátil, ideal para testing
- **EF Core InMemory**: Simulación sin SQL, para pruebas unitarias
- La configuración debe ser **transparente** al cambio
- Easy cambiar entre motores según el entorno (desarrollo vs producción)
