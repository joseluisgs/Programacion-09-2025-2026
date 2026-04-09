# Test: Acceso a Datos y Repositorios en C#

- [Test: Acceso a Datos y Repositorios en C#](#test-acceso-a-datos-y-repositorios-en-c)
  - [Bloque 1: Fundamentos de Bases de Datos Relacionales](#bloque-1-fundamentos-de-bases-de-datos-relacionales)
  - [Bloque 2: ORM y Mapeo Objecto-Relacional](#bloque-2-orm-y-mapeo-objeto-relacional)
  - [Bloque 3: Claves y Estrategias de Identificación](#bloque-3-claves-y-estrategias-de-identificación)
  - [Bloque 4: Borrado Físico vs Lógico](#bloque-4-borrado-físico-vs-lógico)
  - [Bloque 5: Tecnologías de Acceso a Datos](#bloque-5-tecnologías-de-acceso-a-datos)
  - [Bloque 6: Entity Framework Core](#bloque-6-entity-framework-core)
  - [Bloque 7: Inyección de Dependencias](#bloque-7-inyección-de-dependencias)
  - [Bloque 8: Railway Oriented Programming](#bloque-8-railway-oriented-programming)

---

#### Bloque 1: Fundamentos de Bases de Datos Relacionales

1. **¿Cuál es la función principal de una clave primaria (Primary Key) en una tabla SQL?**
   a) Aumentar la velocidad de las consultas.
   b) Identificar de forma única cada registro en una tabla.
   c) Permitir valores nulos en la tabla.
   d) Crear relaciones entre tablas opcionales.

2. **¿Qué tipo de relación representa una tabla intermedia en una relación N:M?**
   a) Relación uno a uno.
   b) Relación uno a muchos.
   c) Relación muchos a muchos.
   d) Relación reflexiva.

3. **¿Qué significa ACID en el contexto de bases de datos?**
   a) Advanced Computer Integrated Database.
   b) Atomicity, Consistency, Isolation, Durability.
   c) Association of Computer Industry Data.
   d) Automated Connection for Integrated Databases.

4. **¿Cuál es la diferencia entre una base de datos embebida y una base de datos servidor?**
   a) Las embebidas no pueden manejar múltiples conexiones.
   b) Las embebidas se ejecutan en el mismo proceso que la aplicación.
   c) No hay diferencia, son términos sinonimos.
   d) Las embebidas siempre son más rápidas.

5. **¿Qué base de datos embebida es multiplataforma y se almacena en un archivo?**
   a) SQL Server.
   b) MySQL.
   c) SQLite.
   d) Oracle.

6. **¿Qué significa SQL en el contexto de bases de datos?**
   a) Simple Query Language.
   b) Structured Query Language.
   c) Standard Question Language.
   d) Sequential Query Logic.

7. **¿Cuál de las siguientes es una base de datos relacional?**
   a) MongoDB.
   b) Redis.
   c) PostgreSQL.
   d) Cassandra.

8. **¿Qué tipo de índice mejora principalmente las búsquedas por columna?**
   a) Índice de texto completo.
   b) Índice hash.
   c) Índice B-tree.
   d) Índice espacial.

9. **¿Cuál es la función de una clave foránea (Foreign Key)?**
   a) Identificar de forma única cada registro.
   b) Establecer una relación entre dos tablas.
   c) Incrementar el rendimiento de las consultas.
   d) Encriptar los datos de la columna.

10. **¿Qué tipo de base de datos es SQLite?**
    a) Cliente-Servidor.
    b) Embebida.
    c) Distribuida.
    d) En memoria exclusivamente.

---

#### Bloque 2: ORM y Mapeo Objecto-Relacional

11. **¿Qué es un ORM (Object-Relational Mapping)?**
    a) Un lenguaje de programación orientado a objetos.
    b) Una técnica para convertir objetos en tablas relacionales y viceversa.
    c) Un tipo de base de datos NoSQL.
    d) Un protocolo de comunicación de red.

12. **¿Cuál es una ventaja principal de usar un ORM como Entity Framework Core?**
    a) Elimina por completo la necesidad de conocer SQL.
    b) Permite trabajar con objetos tipados directamente en el lenguaje de programación.
    c) Garantiza siempre el máximo rendimiento.
    d) Elimina la necesidad de bases de datos físicas.

13. **¿Qué es el "desajuste de impedancia" (Impedance Mismatch)?**
    a) Un error de conexión a la base de datos.
    b) La diferencia entre el modelo de objetos y el modelo relacional.
    c) Un tipo de índice de base de datos.
    d) La pérdida de datos durante la migración.

14. **En un ORM, ¿cómo se representa típicamente una relación 1:N?**
    a) Con dos claves primarias en la misma tabla.
    b) Con una clave foránea en la tabla "muchos" apuntando a la tabla "uno".
    c) Con una tabla intermedia.
    d) Con un array de objetos en la tabla padre.

15. **¿Qué estrategia usa EF Core para cargar relaciones automáticamente?**
    a) Solo Lazy Loading.
    b) Solo Eager Loading.
    c) Lazy Loading, Eager Loading y Explicit Loading.
    d) No tiene estrategias de carga.

16. **¿Qué es el mapping en un ORM?**
    a) El proceso de crear tablas en la base de datos.
    b) La correspondencia entre propiedades de objetos y columnas de tablas.
    c) Un tipo de índice de base de datos.
    d) El proceso de migrar datos entre bases de datos.

17. **¿Cuál de las siguientes es una desventaja de usar un ORM?**
    a) Mayor productividad del desarrollador.
    b) Posible pérdida de rendimiento en consultas complejas.
    c) Seguridad mejorada automáticamente.
    d) Menor necesidad de conocer la base de datos.

18. **¿Qué significa que un ORM es "type-safe"?**
    a) Que protege contra ataques de inyección SQL.
    b) Que el compilador verifica la compatibilidad de tipos en tiempo de compilación.
    c) Que encripta los datos automáticamente.
    d) Que soporta todos los tipos de datos de SQL.

19. **En Entity Framework Core, ¿qué es DbSet?**
    a) Una conexión a la base de datos.
    b) Una colección que representa una tabla y permite operaciones CRUD.
    c) Un tipo de índice.
    d) Un método para ejecutar SQL nativo.

20. **¿Qué patrón arquitectónico sigue típicamente un ORM?**
    a) MVC.
    b) Repository.
    c) Singleton.
    d) Factory.

---

#### Bloque 3: Claves y Estrategias de Identificación

21. **¿Qué es una clave surrogate (surrogada)?**
    a) Una clave natural basada en datos de negocio.
    b) Una clave artificial generada automáticamente sin significado de negocio.
    c) Una clave compuesta de múltiples columnas.
    d) Una clave que puede tener valores nulos.

22. **¿Cuál es una desventaja de usar GUID/UUID como clave primaria?**
    a) No son únicos a nivel global.
    b) Son demasiado pequeños y pueden causar colisiones.
    c) Ocupan mucho espacio (16 bytes) y son lentos para indexar.
    d) No son compatibles con bases de datos.

23. **¿Qué tipo de clave se genera automáticamente con AUTOINCREMENT en SQLite?**
    a) GUID.
    b) Clave natural.
    c) Entero secuencial.
    d) Hash.

24. **¿En qué caso es preferible usar una clave natural en lugar de una surrogate?**
    a) Cuando no hay ningún dato naturalmente único.
    b) Cuando el dato natural es estable y no cambiará nunca.
    c) Cuando se necesita máximo rendimiento.
    d) Cuando la tabla tendrá millones de registros.

25. **¿Qué anotación de EF Core se usa para especificar una clave autoincremental?**
    a) `[AutoIncrement]`
    b) `[Identity]`
    c) `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]`
    d) `[PrimaryKey]`

26. **¿Cuál es la ventaja de usar claves enteras autoincrementales?**
    a) Son más legibles para los usuarios.
    b) Ocupan poco espacio y son rápidas para indexar.
    c) Son únicas entre diferentes bases de datos.
    d) Permitenordenar por orden de inserción automáticamente.

27. **¿Qué problema puede ocurrir si usas el DNI como clave primaria?**
    a) Los DNI pueden cambiar.
    b) Los DNI pueden tener formato diferente entre países.
    c) Solo admite valores numéricos.
    d) No es un número.

28. **¿Qué es una clave compuesta?**
    a) Una clave que usa GUID.
    b) Una clave formada por múltiples columnas.
    c) Una clave que se genera automáticamente.
    d) Una clave que acepta valores duplicados.

29. **En SQLite, ¿qué tipo de datos se usa para almacenar un GUID?**
    a) INTEGER.
    b) TEXT.
    c) REAL.
    d) BLOB.

30. **¿Cuál es el tamaño en bytes de un GUID?**
    a) 4 bytes.
    b) 8 bytes.
    c) 16 bytes.
    d) 32 bytes.

---

#### Bloque 4: Borrado Físico vs Lógico

31. **¿En qué consiste el borrado físico (hard delete)?**
    a) Marcar el registro como eliminado sin borrarlo de la base de datos.
    b) Eliminar permanentemente el registro de la tabla con DELETE.
    c) Mover el registro a una tabla de histórico.
    d) Reemplazar el registro con ceros.

32. **¿Cuál es una ventaja del borrado lógico?**
    a) Libera espacio inmediatamente en la base de datos.
    b) Permite mantener un historial de registros eliminados.
    c) Es más rápido que el borrado lógico.
    d) No requiere modificar la estructura de la tabla.

33. **¿Qué campo es común añadir para implementar borrado lógico?**
    a) DeletedDate.
    b) IsDeleted o DeletedAt.
    c) RemoveFlag.
    d) Status con valores 0/1.

34. **¿Cuál es el problema principal del borrado lógico en consultas?**
    a) Necesita permisos especiales de administrador.
    b) Hay que recordar siempre añadir la condición WHERE IsDeleted = false.
    c) No funciona con claves foráneas.
    d) Es más lento que el borrado físico.

35. **¿Qué tipo de borrado usarías para cumplir con normativas de protección de datos (RGPD)?**
    a) Borrado físico.
    b) Borrado lógico.
    c) Borrado en cascada.
    d) Ninguno, el RGPD no regula esto.

36. **¿Qué ventaja tiene el borrado físico sobre el lógico?**
    a) Mantiene el historial de datos.
    b) Libera espacio en la base de datos.
    c) Es más seguro para datos sensibles.
    d) No requiere modificar la tabla.

37. **¿Qué problema puede causar el borrado físico en integridad referencial?**
    a) Ninguno, siempre funciona correctamente.
    b) Puede violar las restricciones de clave foránea.
    c) Solo afecta a tablas sin relaciones.
    d) Requiere más permisos.

38. **¿Cómo se implementa típicamente el borrado lógico en EF Core?**
    a) Eliminando el registro y recreándolo.
    b) Actualizando el campo IsDeleted a true.
    c) Moviendo el registro a otra tabla.
    d) Encriptando los datos del registro.

39. **¿Qué estrategia de borrado es mejor para un sistema de facturación?**
    a) Borrado físico, para liberar espacio.
    b) Borrado lógico, para mantener histórico de facturas.
    c) No usar borrado, mantener todos los registros.
    d) Depende del tipo de factura.

40. **En una tabla de usuarios, ¿qué enfoque de borrado es más común?**
    a) Borrado físico.
    b) Borrado lógico con campo activo.
    c) No usar borrado, marcar como inactivo.
    d) Usar tablas separadas para usuarios eliminados.

---

#### Bloque 5: Tecnologías de Acceso a Datos

41. **¿Cuál es la principal diferencia entre ADO.NET y Dapper?**
    a) ADO.NET es un ORM y Dapper no lo es.
    b) Dapper es más ligero y requiere escribir menos código para el mapeo.
    c) ADO.NET solo funciona con SQL Server.
    d) Dapper no puede ejecutar consultas SQL.

42. **¿Qué es Dapper?**
    a) Un proveedor de bases de datos.
    b) Un micro-ORM que ejecuta SQL y mapea resultados automáticamente.
    c) Un lenguaje de programación.
    d) Un servidor de bases de datos.

43. **En ADO.NET, ¿qué objeto se utiliza para ejecutar comandos SQL?**
    a) SqlDataReader.
    b) SqlCommand.
    c) SqlConnection.
    d) SqlDataAdapter.

44. **¿Por qué Dapper es más rápido que Entity Framework Core en operaciones simples?**
    a) Usa un compilador JIT más agresivo.
    b) Ejecuta SQL directamente sin overhead de cambio tracking.
    c) Siempre usa caché de resultados.
    d) No necesita conexión a la base de datos.

45. **¿Qué tecnología de acceso a datos es mejor para aplicaciones donde el rendimiento es crítico y se necesita control total del SQL?**
    a) Entity Framework Core.
    b) Dapper.
    c) LINQ to SQL.
    d) NHibernate.

46. **¿Qué significa SQL injection?**
    a) Un tipo de base de datos.
    b) Un ataque que inserta código SQL malicioso.
    c) Un método para optimizar consultas.
    d) Un tipo de índice.

47. **En Dapper, ¿qué método se usa para ejecutar una consulta que devuelve múltiples resultados?**
    a) Execute().
    b) Query().
    c) QuerySingle().
    d) QueryMultiple().

48. **¿Cuál es la ventaja principal de ADO.NET sobre los ORMs?**
    a) Mayor productividad.
    b) Control total sobre el SQL ejecutado.
    c) Mapping automático.
    d) Cambio tracking automático.

49. **¿Qué paquete NuGet necesitas instalar para usar Dapper con SQLite?**
    a) Dapper.
    b) Dapper.Sqlite.
    c) Microsoft.Data.Sqlite.
    d) Dapper y Microsoft.Data.Sqlite.

50. **¿En qué escenario sería preferible usar EF Core sobre Dapper?**
    a) Cuando se necesita máximo rendimiento.
    b) Cuando se requiere escribir SQL complejo manualmente.
    c) Cuando se prioriza la productividad y las consultas son simples.
    d) Cuando no se quiere usar un ORM.

---

#### Bloque 6: Entity Framework Core

51. **¿Qué es el DbContext en Entity Framework Core?**
    a) Una herramienta de migración de bases de datos.
    b) Una clase que representa una sesión con la base de datos y gestiona entidades.
    c) Un tipo de dato para almacenar fechas.
    d) Un método para ejecutar consultas LINQ.

52. **¿Qué método de EF Core crea la base de datos si no existe?**
    a) CreateDatabase().
    b) EnsureCreated().
    c) Migrate().
    d) Initialize().

53. **¿Qué son las migraciones en EF Core?**
    a) Un tipo de relación entre entidades.
    b) Un mecanismo para gestionar cambios en el esquema de la base de datos.
    c) Un método para copiar datos entre bases de datos.
    d) Un tipo de consulta LINQ.

54. **¿Qué es el Change Tracking en EF Core?**
    a) Un sistema de caché para consultas.
    b) La capacidad de detectar automáticamente los cambios en las entidades.
    c) Un método para trackear el tiempo de ejecución de consultas.
    d) Un tipo de índice de base de datos.

55. **¿Cuál es la diferencia entre First() y FirstOrDefault() en LINQ con EF Core?**
    a) First() devuelve el primer elemento sin verificar si existe; FirstOrDefault() devuelve null si no hay resultados.
    b) No hay diferencia, son iguales.
    c) FirstOrDefault() es más lento.
    d) First() solo funciona con listas en memoria.

56. **¿Qué significa AsNoTracking() en EF Core?**
    a) Desactiva el cambio tracking para mejorar rendimiento.
    b) Desactiva la conexión a la base de datos.
    c) Desactiva las migraciones.
    d) Desactiva el caché de consultas.

57. **¿Qué método de EF Core se usa para incluir entidades relacionadas en una consulta?**
    a) Include().
    b) Join().
    c) ThenInclude().
    d) Both a) y c).

58. **¿Qué es una migración en EF Core?**
    a) Un backup de la base de datos.
    b) Un archivo que contiene los cambios del esquema.
    c) Un tipo de consulta.
    d) Un método de conexión.

59. **¿Cuál es el proveedor de EF Core para SQLite?**
    a) Microsoft.EntityFrameworkCore.SqlServer.
    b) Microsoft.EntityFrameworkCore.Sqlite.
    c) Microsoft.EntityFrameworkCore.InMemory.
    d) Microsoft.EntityFrameworkCore.MySQL.

60. **¿Qué comando de la CLI de .NET crea una nueva migración en EF Core?**
    a) dotnet ef database update.
    b) dotnet ef migrations add.
    c) dotnet ef dbcontext scaffold.
    d) dotnet ef database create.

---

#### Bloque 7: Inyección de Dependencias

61. **¿Qué es la Inyección de Dependencias (DI)?**
    a) Un patrón para crear bases de datos automáticamente.
    b) Una técnica donde las dependencias se pasan desde fuera en lugar de crearse internamente.
    c) Un tipo de consulta SQL.
    d) Un método para conectar aplicaciones a internet.

62. **¿Cuál es el principio fundamental de la Inyección de Dependencias?**
    a) Las clases deben crear sus propias dependencias.
    b) Las dependencias deben ser declaradas como variables globales.
    c) Las clases de alto nivel no deben depender de clases de bajo nivel; ambas deben depender de abstracciones.
    d) Solo se puede inyectar una dependencia por clase.

63. **En .NET, ¿cuál es la diferencia entre AddTransient y AddScoped?**
    a) No hay diferencia, son iguales.
    b) Transient crea una nueva instancia cada vez; Scoped crea una instancia por request HTTP.
    c) Scoped es más rápido que Transient.
    d) Transient solo funciona con interfaces.

64. **¿Qué beneficio principal proporciona la Inyección de Dependencias para los tests?**
    a) Los tests se ejecutan más rápido.
    b) Permite sustituir implementaciones reales por mocks o stubs.
    c) No necesita base de datos para tests.
    d) Elimina la necesidad de escribir tests.

65. **¿Qué archivo se utiliza en .NET para configurar la aplicación desde un archivo externo?**
    a) config.json.
    b) appsettings.json.
    c) settings.xml.
    d) database.config.

66. **¿Qué significa IOC en el contexto de inyección de dependencias?**
    a) Internet of Things.
    b) Inversion of Control.
    c) Input Output Controller.
    d) Integrated Object Compiler.

67. **¿Cuál es el ciclo de vida adecuado para un DbContext en una aplicación web ASP.NET Core?**
    a) Transient.
    b) Scoped.
    c) Singleton.
    d) No se debe usar DI para DbContext.

68. **¿Qué es un mock en el contexto de testing?**
    a) Un tipo de base de datos de prueba.
    b) Una implementación falsa de una dependencia.
    c) Un método para simular errores.
    d) Un tipo de test.

69. **¿Qué beneficios tiene usar interfaces para las dependencias?**
    a) Permite cambiar la implementación sin modificar el código cliente.
    b) Reduce el número de líneas de código.
    c) Aumenta el rendimiento.
    d) Es obligatorio en C#.

70. **En .NET, ¿qué contenedor de DI viene incluido por defecto?**
    a) Autofac.
    b) Ninject.
    c) Microsoft.Extensions.DependencyInjection.
    d) Castle Windsor.

---

#### Bloque 8: Railway Oriented Programming

71. **¿Qué problema busca resolver Railway Oriented Programming (ROP)?**
    a) El rendimiento de las consultas a bases de datos.
    b) El manejo de errores de forma más elegante que las excepciones.
    c) La conexión a múltiples bases de datos.
    d) El mapeo de objetos a tablas.

72. **En ROP, ¿qué representa el "tren" en la metáfora?**
    a) La ejecución del programa.
    b) Un tipo de dato especial.
    c) Un servidor de base de datos.
    d) Un patrón de diseño de interfaz.

73. **¿Qué tipo devuelve típicamente una función que sigue ROP?**
    a) void.
    b) Result<T, TError> o similar que puede contener éxito o error.
    c) Un boolean.
    d) Un nullable type simple.

74. **¿Cuál es la ventaja de usar Result sobre excepciones para control de flujo?**
    a) Las excepciones son más rápidas.
    b) El código es más legible y no rompe el flujo natural con try-catch.
    c) Las excepciones no existen en C#.
    d) Result solo funciona con bases de datos.

75. **En el contexto de ROP, ¿qué hace el método Bind??**
    a) Convierte un Result en un valor simple.
    b) Encadena operaciones que pueden fallar.
    c) Imprime el resultado por consola.
    d) Guarda el resultado en base de datos.

76. **¿Qué librería de C# proporciona implementaciones de Result para ROP?**
    a) Newtonsoft.Json.
    b) CSharpFunctionalExtensions.
    c) FluentValidation.
    d) AutoMapper.

77. **En ROP, ¿qué hace el método Map?**
    a) Transforma el valor en caso de éxito.
    b) Convierte errores.
    c) Valida el resultado.
    d) Imprime el resultado.

78. **¿Qué hace el método Match en un Result?**
    a) Busca un valor específico.
    b) Combina éxito y error en una sola lambda.
    c) Convierte el Result a lista.
    d) Valida el tipo del resultado.

79. **¿En qué casos es apropiado seguir usando excepciones en lugar de ROP?**
    a) Para validar datos de entrada del usuario.
    b) Para errores verdaderamente excepcionales (fallo de base de datos, archivo no encontrado).
    c) Para control de flujo normal.
    d) Para替代 valores nulos.

80. **¿Qué representa la "vía del éxito" en la metáfora del tren de ROP?**
    a) El camino que sigue el código cuando todo va bien.
    b) Un tipo de base de datos.
    c) Un método de validación.
    d) Un patrón de diseño.

***

