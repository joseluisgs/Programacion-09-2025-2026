# Ejercicios de Construcción: Repositorios y Servicios

- [Ejercicios de Construcción: Repositorios y Servicios](#ejercicios-de-construcción-repositorios-y-servicios)
  - [Ejercicio 1: Repositorio de Productos con ADO.NET](#ejercicio-1-repositorio-de-productos-con-adonet)
    - [Modelo de Datos](#modelo-de-datos)
    - [Interfaz](#interfaz)
    - [Requisitos](#requisitos)
    - [Datos de Prueba](#datos-de-prueba)
  - [Ejercicio 2: Repositorio de Libros con Dapper](#ejercicio-2-repositorio-de-libros-con-dapper)
    - [Modelo de Datos](#modelo-de-datos-1)
    - [Requisitos](#requisitos-1)
    - [Datos de Prueba](#datos-de-prueba-1)
  - [Ejercicio 3: Gestión de Empleados con Entity Framework Core](#ejercicio-3-gestión-de-empleados-con-entity-framework-core)
    - [Modelo de Datos](#modelo-de-datos-2)
    - [Requisitos](#requisitos-2)
    - [Datos de Prueba](#datos-de-prueba-2)
  - [Ejercicio 4: Sistema de Biblioteca con DI y Múltiples Implementaciones](#ejercicio-4-sistema-de-biblioteca-con-di-y-múltiples-implementaciones)
    - [Modelos de Datos](#modelos-de-datos)
    - [Interfaz Común](#interfaz-común)
    - [Requisitos](#requisitos-3)
    - [Datos de Prueba](#datos-de-prueba-3)
  - [Ejercicio 5: Servicio de Gestión de Alumnos con ROP](#ejercicio-5-servicio-de-gestión-de-alumnos-con-rop)
    - [Errores de Dominio](#errores-de-dominio)
    - [Modelo de Datos](#modelo-de-datos-3)
    - [Interfaz de Servicio](#interfaz-de-servicio)
    - [Requisitos](#requisitos-4)
    - [Datos de Prueba](#datos-de-prueba-4)

---

## Ejercicio 1: Repositorio de Productos con ADO.NET

Implementa un repositorio de productos usando **ADO.NET** con SQLite.

### Modelo de Datos

```csharp
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

### Interfaz

```csharp
public interface ICrudRepository<TKey, TEntity> where TEntity : class
{
    IEnumerable<TEntity> GetAll();
    TEntity? GetById(TKey id);
    TEntity? Create(TEntity entity);
    TEntity? Update(TKey id, TEntity entity);
    TEntity? Delete(TKey id);
}
```

### Requisitos

1. **Crea la tabla `Productos`** con el script SQL apropiado.
2. **Implementa `ProductoRepository`** usando `SqliteConnection` de `Microsoft.Data.Sqlite`.
3. **Implementa borrado lógico** usando el campo `IsDeleted`.
4. **Usa constructores primarios** de C# 12+.
5. **Incluye validación básica**: No crear productos con precio negativo o nombre vacío.

### Datos de Prueba

```csharp
var productos = new List<Producto>
{
    new() { Nombre = "Portátil HP", Descripcion = "15.6 pulg, 16GB RAM", Precio = 899.99m, Stock = 10, Categoria = "Electrónica" },
    new() { Nombre = "Teclado Mecánico", Descripcion = "RGB, switches azules", Precio = 79.99m, Stock = 50, Categoria = "Periféricos" },
    new() { Nombre = "Monitor 27\"", Descripcion = "4K UHD", Precio = 349.99m, Stock = 25, Categoria = "Electrónica" },
    new() { Nombre = "Silla Gaming", Descripcion = "Ergonómica, negra", Precio = 199.99m, Stock = 15, Categoria = "Muebles" },
    new() { Nombre = "Webcam HD", Descripcion = "1080p, micrófono", Precio = 59.99m, Stock = 30, Categoria = "Periféricos" }
};
```

---

## Ejercicio 2: Repositorio de Libros con Dapper

Implementa un repositorio de libros usando **Dapper** con SQLite.

### Modelo de Datos

```csharp
public class Libro
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int AñoPublicacion { get; set; }
    public string Genero { get; set; } = string.Empty;
    public int Paginas { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Requisitos

1. **Crea la tabla `Libros`** con GUID como clave primaria (texto).
2. **Implementa `LibroRepository`** usando Dapper.
3. **Usa clave GUID** generada en C# (no SQLite AUTOINCREMENT).
4. **Usa constructores primarios** de C# 12+.
5. **Implementa consultas con parámetros** para buscar por título, autor y género.
6. **Crea un método adicional** `GetByISBN(string isbn)` que busque por ISBN exacto.

### Datos de Prueba

```csharp
var libros = new List<Libro>
{
    new() { Titulo = "El Quijote", Autor = "Miguel de Cervantes", ISBN = "978-84-376-0494-0", AñoPublicacion = 1605, Genero = "Novela", Paginas = 863 },
    new() { Titulo = "1984", Autor = "George Orwell", ISBN = "978-84-9836-427-1", AñoPublicacion = 1949, Genero = "Ciencia Ficción", Paginas = 326 },
    new() { Titulo = "Cien Años de Soledad", Autor = "Gabriel García Márquez", ISBN = "978-84-204-2068-5", AñoPublicacion = 1967, Genero = "Realismo Mágico", Paginas = 351 },
    new() { Titulo = "Fundación", Autor = "Isaac Asimov", ISBN = "978-84-450-0780-5", AñoPublicacion = 1951, Genero = "Ciencia Ficción", Paginas = 244 },
    new() { Titulo = "Dune", Autor = "Frank Herbert", ISBN = "978-84-450-0781-2", AñoPublicacion = 1965, Genero = "Ciencia Ficción", Paginas = 412 }
};
```

---

## Ejercicio 3: Gestión de Empleados con Entity Framework Core

Implementa un sistema de gestión de empleados usando **Entity Framework Core** con SQLite.

### Modelo de Datos

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Empleados")]
public class Empleado
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [MaxLength(9)]
    public string DNI { get; set; } = string.Empty;

    [Column("Email")]
    [MaxLength(200)]
    public string Correo { get; set; } = string.Empty;

    public DateTime FechaContratacion { get; set; }

    [Column("Salario")]
    public decimal Salario { get; set; }

    [MaxLength(50)]
    public string Departamento { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Requisitos

1. **Configura EF Core** con Data Annotations.
2. **Crea el DbContext** y usa `EnsureCreated()` para inicializar la BD.
3. **Implementa `EmpleadoRepository`** con clave entera autoincremental.
4. **Usa constructores primarios** de C# 12+.
5. **Implementa consultas LINQ**:
   - Buscar empleados por departamento.
   - Buscar empleados contratados después de una fecha.
   - Calcular la masa salarial total por departamento.

### Datos de Prueba

```csharp
var empleados = new List<Empleado>
{
    new() { Nombre = "Ana", Apellidos = "García López", DNI = "12345678A", Correo = "ana.garcia@empresa.com", FechaContratacion = new DateTime(2020, 3, 15), Salario = 35000m, Departamento = "IT" },
    new() { Nombre = "Carlos", Apellidos = "Martínez Sánchez", DNI = "23456789B", Correo = "carlos.martinez@empresa.com", FechaContratacion = new DateTime(2019, 8, 1), Salario = 42000m,Departamento = "Ventas" },
    new() { Nombre = "María", Apellidos = "Rodríguez Torres", DNI = "34567890C", Correo = "maria.rodriguez@empresa.com", FechaContratacion = new DateTime(2021, 1, 10), Salario = 28000m, Departamento = "RRHH" },
    new() { Nombre = "Juan", Apellidos = "Fernández Ruiz", DNI = "45678901D", Correo = "juan.fernandez@empresa.com", FechaContratacion = new DateTime(2018, 5, 20), Salario = 55000m, Departamento = "IT" },
    new() { Nombre = "Laura", Apellidos = "Gómez Díaz", DNI = "56789012E", Correo = "laura.gomez@empresa.com", FechaContratacion = new DateTime(2022, 2, 28), Salario = 32000m, Departamento = "Ventas" }
};
```

---

## Ejercicio 4: Sistema de Biblioteca con DI y Múltiples Implementaciones

Implementa un sistema de biblioteca que soporte **múltiples tecnologías de almacenamiento** mediante inyección de dependencias.

### Modelos de Datos

```csharp
public class Editorial
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public DateTime FoundedYear { get; set; }
}

public class Autor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
}

public class Libro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int AutorId { get; set; }
    public int EditorialId { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public int AñoPublicacion { get; set; }
    public int Paginas { get; set; }
}
```

### Interfaz Común

```csharp
public interface IBookRepository
{
    IEnumerable<Libro> GetAll();
    Libro? GetById(int id);
    IEnumerable<Libro> GetByAutor(int autorId);
    IEnumerable<Libro> GetByEditorial(int editorialId);
    IEnumerable<Libro> GetByYear(int año);
}
```

### Requisitos

1. **Crea tres implementaciones** del repositorio:
   - `BookRepositoryInMemory`: Almacena en una `List<Libro>` en memoria.
   - `BookRepositoryDapper`: Usa Dapper con SQLite en archivo.
   - `BookRepositoryEfCore`: Usa Entity Framework Core con SQLite en archivo.

2. **Configura la DI** en `Program.cs` para poder intercambiar implementaciones mediante `appsettings.json`:
   ```json
   {
     "Database": {
       "Provider": "InMemory",  // O "Dapper" o "EfCore"
       "ConnectionString": "Data Source=biblioteca.db"
     }
   }
   ```

3. **Crea un servicio** `BibliotecaService` que dependa de `IBookRepository`.

4. **Usa constructores primarios** en todas las clases.

### Datos de Prueba

```csharp
var autores = new List<Autor>
{
    new() { Id = 1, Nombre = "Miguel de Cervantes", Apellidos = "Saavedra", FechaNacimiento = new DateTime(1547, 9, 29) },
    new() { Id = 2, Nombre = "George", Apellidos = "Orwell", FechaNacimiento = new DateTime(1903, 6, 25) },
    new() { Id = 3, Nombre = "Isaac", Apellidos = "Asimov", FechaNacimiento = new DateTime(1920, 1, 2) }
};

var editoriales = new List<Editorial>
{
    new() { Id = 1, Nombre = "Santillana", Pais = "España", FoundedYear = 1945 },
    new() { Id = 2, Nombre = "Penguin Random House", Pais = "Reino Unido", FoundedYear = 2013 },
    new() { Id = 3, Nombre = "Planeta", Pais = "España", FoundedYear = 1949 }
};

var libros = new List<Libro>
{
    new() { Id = 1, Titulo = "El Quijote", AutorId = 1, EditorialId = 1, ISBN = "978-84-376-0494-0", AñoPublicacion = 1605, Paginas = 863 },
    new() { Id = 2, Titulo = "1984", AutorId = 2, EditorialId = 2, ISBN = "978-84-9836-427-1", AñoPublicacion = 1949, Paginas = 326 },
    new() { Id = 3, Titulo = "Fundación", AutorId = 3, EditorialId = 3, ISBN = "978-84-450-0780-5", AñoPublicacion = 1951, Paginas = 244 }
};
```

---

## Ejercicio 5: Servicio de Gestión de Alumnos con ROP

Implementa un servicio de gestión de alumnos usando **Railway Oriented Programming** con CSharpFunctionalExtensions.

### Errores de Dominio

```csharp
public abstract class DomainError
{
    public string Message { get; }
    protected DomainError(string message) => Message = message;
}

public class AlumnoNotFoundError : DomainError
{
    public int Id { get; }
    public AlumnoNotFoundError(int id) : base($"Alumno con ID {id} no encontrado") => Id = id;
}

public class ValidationError : DomainError
{
    public ValidationError(string message) : base(message) { }
}

public class StorageError : DomainError
{
    public Exception Exception { get; }
    public StorageError(Exception ex) : base($"Error de almacenamiento: {ex.Message}") => Exception = ex;
}
```

### Modelo de Datos

```csharp
public class Alumno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NIF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double Nota { get; set; }
    public bool Repetidor { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

### Interfaz de Servicio

```csharp
public interface IAlumnoService
{
    Result<Alumno, DomainError> GetById(int id);
    Result<Alumno, DomainError> Create(Alumno alumno);
    Result<Alumno, DomainError> Update(int id, Alumno alumno);
    Result<Alumno, DomainError> Delete(int id);
    Result<IEnumerable<Alumno>, DomainError> GetAll();
    Result<IEnumerable<Alumno>, DomainError> GetByNotaMinima(double notaMinima);
}
```

### Requisitos

1. **Instala CSharpFunctionalExtensions**: `dotnet add package CSharpFunctionalExtensions`

2. **Implementa validaciones** en el servicio:
   - Nombre y apellidos no pueden estar vacíos.
   - NIF debe tener formato válido (8 dígitos + 1 letra).
   - Email debe contener '@' y '.'.
   - Nota debe estar entre 0 y 10.

3. **Usa operadores de ROP**:
   - `Result.Success()` y `Result.Failure()` para crear resultados.
   - `Bind()` para encadenar operaciones.
   - `Map()` para transformar el valor.
   - `Ensure()` para validaciones condicionales.
   - `Match()` para consumir el resultado.

4. **Usa constructores primarios** de C# 12+.

5. **Implementa lógica de negocio**:
   - Un alumno con nota >= 5 se considera "aprobado".
   - No se puede matricular a un repetidor si ya tiene 3 asignaturas suspensas.

### Datos de Prueba

```csharp
var alumnos = new List<Alumno>
{
    new() { Id = 1, Nombre = "Juan", Apellidos = "García López", NIF = "12345678A", Email = "juan.garcia@alumnos.edu", Nota = 7.5, Repetidor = false, FechaNacimiento = new DateTime(2000, 5, 15) },
    new() { Id = 2, Nombre = "María", Apellidos = "Martínez Sánchez", NIF = "23456789B", Email = "maria.martinez@alumnos.edu", Nota = 5.2, Repetidor = false, FechaNacimiento = new DateTime(2001, 8, 22) },
    new() { Id = 3, Nombre = "Carlos", Apellidos = "Rodríguez Torres", NIF = "34567890C", Email = "carlos.rodriguez@alumnos.edu", Nota = 3.8, Repetidor = true, FechaNacimiento = new DateTime(1999, 2, 10) },
    new() { Id = 4, Nombre = "Ana", Apellidos = "Fernández Ruiz", NIF = "45678901D", Email = "ana.fernandez@alumnos.edu", Nota = 9.1, Repetidor = false, FechaNacimiento = new DateTime(2002, 11, 3) },
    new() { Id = 5, Nombre = "Pedro", Apellidos = "Gómez Díaz", NIF = "56789012E", Email = "pedro.gomez@alumnos.edu", Nota = 4.5, Repetidor = false, FechaNacimiento = new DateTime(2000, 7, 28) }
};
```

---



