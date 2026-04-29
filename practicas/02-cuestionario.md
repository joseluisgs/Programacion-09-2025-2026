# Cuestionario: Acceso a Datos, Repositorios y Arquitectura

- [Cuestionario: Acceso a Datos, Repositorios y Arquitectura](#cuestionario-acceso-a-datos-repositorios-y-arquitectura)
  - [Investigación sobre Tecnologías de Acceso a Datos](#investigación-sobre-tecnologías-de-acceso-a-datos)
  - [Patrón Repository y Diseño de Arquitecturas](#patrón-repository-y-diseño-de-arquitecturas)
  - [Entity Framework Core y ORM](#entity-framework-core-y-orm)
  - [Inversión de Dependencias y Railway Oriented Programming](#inversión-de-dependencias-y-railway-oriented-programming)
  - [Cuestiones de Diseño](#cuestiones-de-diseño)

---

## Investigación sobre Tecnologías de Acceso a Datos

1.  **ADO.NET vs Dapper vs EF Core:** Investiga y explica las diferencias fundamentales entre estas tres tecnologías de acceso a datos en .NET. ¿En qué escenarios sería preferible usar cada una? Considera aspectos como rendimiento, productividad, control SQL y curva de aprendizaje.

2.  **El problema N+1 en ORMs:** Investiga en qué consiste este problema común cuando se usan ORMs como Entity Framework Core. ¿Cómo puede el uso de *Eager Loading* (Include/ThenInclude) o *Split Queries* mitigar este problema?

3.  **Change Tracking en EF Core:** Explica cómo funciona el sistema de Change Tracking de Entity Framework Core. ¿Cuál es la diferencia entre Change Tracking por defecto y *Change Tracking sin seguimiento* (AsNoTracking) en términos de rendimiento y funcionalidad?

4.  **Proveedores de bases de datos en EF Core:** Investiga qué proveedores de bases de datos soporta Entity Framework Core. ¿Qué diferencia hay entre usar el proveedor de SQLite en memoria (`UseInMemoryDatabase`) para testing versus una base de datos SQLite en archivo?

---

## Patrón Repository y Diseño de Arquitecturas

5.  **Principios SOLID aplicados a Repository:** Analiza cómo el Patrón Repository ayuda a cumplir con los principios **SOLID**, especialmente el Principio de Inversión de Dependencias (DIP). ¿Por qué es importante que la lógica de negocio no dependa de detalles de implementación?

6.  **Claves surrogate vs claves naturales:** Investiga los pros y contras de usar claves surrogate (como IDs autoincrementales o GUIDs) frente a claves naturales (como DNI, ISBN, etc.). ¿En qué situaciones es recomendable cada enfoque?

7.  **Borrado lógico y cumplimiento normativo:** El Reglamento General de Protección de Datos (RGPD) exige el "derecho al olvido". Diseña una estrategia de borrado que cumpla con esta normativa pero que también permita mantener un histórico de datos para auditoría. ¿Cómo implementarías esto con un campo `IsDeleted` o `DeletedAt`?

8.  **Transaccionalidad:** Explica qué es una transacción de base de datos y por qué es importante en operaciones que afectan múltiples tablas. ¿Cómo manejarías una transacción en C# usando ADO.NET, Dapper o EF Core?

---

## Entity Framework Core y ORM

9.  **Data Annotations vs Fluent API:** En EF Core hay dos formas de configurar el modelo de entidades: mediante atributos (Data Annotations) y mediante la Fluent API. Investiga cuándo es preferible usar cada una y proporciona ejemplos de configuración que solo pueden hacerse con Fluent API.

10. **Migraciones en EF Core:** Explica el flujo de trabajo de las migraciones en Entity Framework Core. ¿Qué comandos de la CLI de .NET se utilizan para crear y aplicar migraciones? ¿En qué se diferencia `EnsureCreated()` de las migraciones?

11. **Patrones de herencia en EF Core:** Investiga los tres patrones de herencia disponibles en Entity Framework Core: TPT (Table per Type), TPH (Table per Hierarchy) y TPC (Table per Concrete Type). ¿Cuál es el más performant y por qué?

12. **Concurrency en EF Core:** ¿Cómo maneja EF Core la concurrencia optimista? Investiga el uso de propiedades de concurrencia como `[Timestamp]` o `rowversion` para detectar conflictos de concurrencia.

---

## Inversión de Dependencias y Railway Oriented Programming

13. **Contenedores de Inyección de Dependencias:** Investiga los diferentes contenedores de DI disponibles para .NET: el contenedor nativo de Microsoft, AutoFac, Ninject y DryIoc. ¿Qué ventajas tiene usar uno sobre otro?

14. **Ciclos de vida en DI:** Explica la diferencia entre Transient, Scoped y Singleton. Proporciona ejemplos de cuándo usar cada uno y los problemas que pueden surgir si se usa el ciclo de vida incorrecto (como memory leaks o estado compartido no deseado).

15. **Railway Oriented Programming vs Excepciones:** Investiga por qué Railway Oriented Programming es considerado una alternativa mejor que las excepciones para el control de flujo en operaciones de dominio. ¿En qué casos seguirías usando excepciones?

16. **CSharpFunctionalExtensions:** Investiga la librería CSharpFunctionalExtensions. ¿Qué ventajas ofrece sobre implementar Result desde cero? Explica cómo se usan operadores como `Bind`, `Map`, `Ensure` y `Match`.

17. **Patrón Unit of Work:** Investiga este patrón arquitectónico. ¿Cómo se complementa con el Patrón Repository? Proporciona un ejemplo de implementación básica.

18. **Testabilidad con DI:** ¿Cómo facilita la Inyección de Dependencias la creación de pruebas unitarias? Explica qué son los *mocks* y *stubs* y cómo se utilizan en conjunto con la DI para probar componentes aislados.

---

## Cuestiones de Diseño

19. **Diseño de entidades para repositorios:** Si diseñaras un sistema de gestión académica con entidades como Estudiante, Curso, Matrícula y Departamento:
    - ¿Qué tipo de clave primaria usarías para cada una?
    - ¿Implementarías borrado lógico o físico?
    - ¿Cómo modelarías las relaciones entre estas entidades?

20. **Elección de tecnología:** Un cliente te pide desarrollar una aplicación de gestión interna con las siguientes características:
    - 10 usuarios simultáneos máximo.
    - Necesidad de flexibilidad en consultas SQL complejas.
    - Equipo de desarrollo pequeño (2 desarrolladores).
    - Plazo de entrega corto.

    ¿Qué tecnología de acceso a datos recomiendas? Justifica tu respuesta considerando ADO.NET, Dapper y EF Core.

---



