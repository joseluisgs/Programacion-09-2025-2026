- [1. Fundamentos de Bases de Datos Relacionales](#1-fundamentos-de-bases-de-datos-relacionales)
  - [1.1. Introducción: dos Mundos que Deben Converger](#11-introducción-dos-mundos-que-deben-converger)
    - [¿Por qué bases de datos?](#por-qué-bases-de-datos)
  - [1.2. Conceptos Básicos de Bases de Datos Relacionales](#12-conceptos-básicos-de-bases-de-datos-relacionales)
    - [1.2.1. Componentes Fundamentales](#121-componentes-fundamentales)
    - [1.2.2. Restricciones de Integridad](#122-restricciones-de-integridad)
  - [1.3. El Impedance Mismatch](#13-el-impedance-mismatch)
    - [1.3.1. ¿Qué es el Impedance Mismatch?](#131-qué-es-el-impedance-mismatch)
    - [1.3.2. Manifestaciones del Problema](#132-manifestaciones-del-problema)
  - [1.4. Resumen](#14-resumen)

# 1. Fundamentos de Bases de Datos Relacionales

## 1.1. Introducción: dos Mundos que Deben Converger

Hasta ahora hemos trabajado con **persistencia en ficheos** (CSV, JSON, XML). Estos formatos son excelentes para datos simples y portabilidad, pero tienen limitaciones:

### ¿Por qué bases de datos?

| Capacidad | Ficheros | Base de Datos |
|-----------|----------|---------------|
| **Consultas complejas** | ❌ Buscar en todo el archivo | ✅ WHERE, JOIN, ORDER BY |
| **Integridad referencial** | ❌ Manual | ✅ Claves foráneas |
| **Transacciones ACID** | ❌ No existe | ✅ Atomicidad, consistencia |
| **Concurrencia** | ❌ Fichero bloqueado | ✅ Múltiples usuarios |
| **Rendimiento** | ❌ Lee todo el archivo | ✅ Índices, optimización |

> 📝 **Nota del Profesor**: Una base de datos relacional es como una biblioteca bien organizada vs un trastero lleno de cajas. En la biblioteca, encuentras cualquier libro en segundos (gracias al catálogo/índice). En el trastero, tienes que abrir todas las cajas.

---

## 1.2. Conceptos Básicos de Bases de Datos Relacionales

### 1.2.1. Componentes Fundamentales

Una **Base de Datos Relacional** almacena información estructurada en **tablas**:

| Concepto | Definición | Ejemplo |
|----------|------------|---------|
| **Tabla** | Colección de registros con estructura común | `Estudiantes` |
| **Registro/Fila** | Instancia individual de datos | Un estudiante específico |
| **Columna/Campo** | Atributo que describe una propiedad | `Nombre`, `Calificacion` |
| **Tipo de Dato** | Formato de almacenamiento | `INTEGER`, `TEXT`, `REAL` |
| **Clave Primaria (PK)** | Identificador único de cada registro | `Id` |
| **Clave Foránea (FK)** | Referencia a la PK de otra tabla | `EstudianteId` |

**Ejemplo Visual:**

```
Tabla: Estudiantes
┌────┬─────────────┬──────────────┬──────────────┐
│ Id │   Nombre    │ Calificacion │ FechaNacim.  │
├────┼─────────────┼──────────────┼──────────────┤
│ 1  │ Ana García  │     8.5      │  2005-03-15  │
│ 2  │ Juan Pérez  │     7.0      │  2004-11-22  │
│ 3  │ María López │     9.2      │  2005-07-08  │
└────┴─────────────┴──────────────┴──────────────┘
       ↑              ↑              ↑
    PK (única)    Atributos       Tipo DATE
```

### 1.2.2. Restricciones de Integridad

Las **restricciones** garantizan la calidad y coherencia de los datos:

```sql
CREATE TABLE Estudiantes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Calificacion REAL CHECK(Calificacion >= 0 AND Calificacion <= 10),
    Email TEXT UNIQUE,
    FechaNacimiento DATE NOT NULL
);
```

| Restricción | Propósito | Ejemplo |
|-------------|-----------|---------|
| `PRIMARY KEY` | Identifica cada registro de forma única | `Id INTEGER PRIMARY KEY` |
| `NOT NULL` | El campo no puede ser nulo | `Nombre TEXT NOT NULL` |
| `UNIQUE` | No puede haber duplicados | `Email TEXT UNIQUE` |
| `CHECK` | Valida condición lógica | `CHECK(Calificacion >= 0)` |
| `FOREIGN KEY` | Relaciona con otra tabla | `FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id)` |

> 💡 **Tip del Examinador**: Las restricciones son como las reglas de un juego. Si no las pones, alguien hará trampa. Siempre define restricciones en tus tablas.

---

## 1.3. El Impedance Mismatch

### 1.3.1. ¿Qué es el Impedance Mismatch?

El **Impedance Mismatch** es el problema fundamental al combinar dos paradigmas incompatibles:

```
┌─────────────────────────────────┐     ┌─────────────────────────────────┐
│  Paradigma Orientado a Objetos  │     │  Paradigma Relacional           │
│           (C#, Java)            │     │          (SQL)                  │
├─────────────────────────────────┤     ├─────────────────────────────────┤
│  • Clases con herencia          │     │  • Tablas planas                │
│  • Métodos + datos              │     │  • Solo datos (sin métodos)     │
│  • Objetos complejos            │     │  • Valores simples              │
│  • Navegación por referencias   │     │  • JOINs explícitos             │
└─────────────────────────────────┘     └─────────────────────────────────┘
                         │
                         ▼
        ┌───────────────────────────────┐
        │   IMPEDANCE MISMATCH          │
        │   (Problema de adaptación)    │
        └───────────────────────────────┘
```

### 1.3.2. Manifestaciones del Problema

**1. Identidad:**

```csharp
// C#: Los objetos son iguales por referencia
Estudiante e1 = new Estudiante { Id = 1, Nombre = "Ana" };
Estudiante e2 = new Estudiante { Id = 1, Nombre = "Ana" };
Console.WriteLine(e1 == e2);  // False (diferentes objetos en memoria)

// SQL: Los registros son iguales por valor de PK
SELECT * FROM Estudiantes WHERE Id = 1;  -- Siempre devuelve el MISMO registro
```

**2. Relaciones:**

```csharp
// C#: Navegación directa
estudiante.Asignaturas.Add(new Asignatura { Nombre = "Matemáticas" });

// SQL: Requiere JOIN
SELECT e.*, a.* 
FROM Estudiantes e
INNER JOIN Asignaturas a ON e.Id = a.EstudianteId
WHERE e.Id = 1;
```

**3. Herencia:**

```csharp
// C#: Herencia natural
public class Persona { public string Nombre { get; set; } }
public class Estudiante : Persona { public double Calificacion { get; set; } }

// SQL: Múltiples estrategias (TPH, TPT, TPC)
-- Estrategia TPH (Table per Hierarchy): todo en una tabla con discriminador
CREATE TABLE Personas (
    Id INTEGER PRIMARY KEY,
    Tipo TEXT,  -- 'Estudiante' o 'Profesor'
    Nombre TEXT,
    Calificacion REAL  -- null para profesores
);
```

> 📝 **Nota del Profesor**: Este desfase es la razón por la que existen los ORMs (Object-Relational Mappers). Un ORM hace el trabajo sucio de convertir objetos a tablas y viceversa. Por eso los estudiamos en esta unidad.

---

## 1.4. Resumen

- Las **bases de datos relacionales** almacenan datos en tablas con estructura fija
- Las **claves primarias** identifican cada registro de forma única
- Las **claves foráneas** establecen relaciones entre tablas
- Las **restricciones** garantizan integridad de datos
- El **"Impedance Mismatch"** es el problema de mapear objetos a tablas
- Los **ORMs** resuelven este problema automatizando la conversión
