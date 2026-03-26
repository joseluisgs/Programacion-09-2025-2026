// Import de EF Core
using Microsoft.EntityFrameworkCore;
// Import de SQLite para .NET
using Microsoft.Data.Sqlite;
// Import de contenedor DI nativo de .NET
using Microsoft.Extensions.DependencyInjection;
// Import de la configuración
using PersonaService.Config;
// Import de las interfaces de repositorio
using PersonaService.Repositories;
// Import del validador
using PersonaService.Validators;
// Import de la cache
using PersonaService.Cache;
// Import del DbContext
using PersonaService.Data;
// Import de los modelos
using PersonaService.Models;

// Alias para evitar conflicto de nombres entre namespace y clase
// PersonaService.Services.PersonaService (clase) vs PersonaService (namespace)
using Svc = PersonaService.Services.PersonaService;

namespace PersonaService.Infrastructure;

/// <summary>
/// Proveedor centralizado de dependencias.
/// Configura todos los servicios según appsettings.json.
/// </summary>
public static class DependenciesProvider
{
    /// <summary>
    /// Método principal que registra todos los servicios en el contenedor DI
    /// </summary>
    public static IServiceProvider BuildServiceProvider()
    {
        // 1. Carga la configuración desde appsettings.json
        AppConfig.Load();
        
        // 2. Crea la colección de servicios
        var services = new ServiceCollection();
        
        // 3. Registra servicios según el proveedor configurado en appsettings.json
        switch (AppConfig.Provider)
        {
            // Si el provider es EF Core con SQLite
            case "EfCoreSqlite":
                RegisterEfCoreSqlite(services);
                break;
                
            // Si el provider es Dapper con SQLite
            case "DapperSqlite":
                RegisterDapperSqlite(services);
                break;
                
            // Si el provider no es válido
            default:
                Console.WriteLine($"⚠️ Proveedor desconocido: {AppConfig.Provider}");
                Console.WriteLine($"⚠️ Usando EfCoreSqlite por defecto");
                RegisterEfCoreSqlite(services);
                break;
        }
        
        // 4. Registra servicios comunes (validador, cache, servicio)
        RegisterCommonServices(services);
        
        // 5. Construye y retorna el ServiceProvider
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Registra EF Core con SQLite archivo
    /// </summary>
    private static void RegisterEfCoreSqlite(IServiceCollection services)
    {
        Console.WriteLine("📦 Registrando: EF Core + SQLite Archivo");
        
        // DbContext con SQLite - se crea una instancia por scoped request
        // EF Core gestiona la conexión automáticamente
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(AppConfig.ConnectionString);
        });
        
        // Repositorio EF Core - inyectado cuando se pida IPersonaRepository
        services.AddScoped<IPersonaRepository, PersonaRepositoryEfCore>();
    }

    /// <summary>
    /// Registra Dapper con SQLite archivo
    /// </summary>
    private static void RegisterDapperSqlite(IServiceCollection services)
    {
        Console.WriteLine("📦 Registrando: Dapper + SQLite Archivo");
        
        // Connection factory - crea una nueva conexión por cada scoped request
        // IMPORTANTE: abrir la conexión antes de retornarla
        services.AddScoped<SqliteConnection>(_ =>
        {
            var connection = new SqliteConnection(AppConfig.ConnectionString);
            connection.Open();
            return connection;
        });
        
        // Repositorio Dapper - inyectado cuando se pida IPersonaRepository
        services.AddScoped<IPersonaRepository, PersonaRepositoryDapper>();
    }

    /// <summary>
    /// Registra servicios comunes a todos los proveedores de BD
    /// </summary>
    private static void RegisterCommonServices(IServiceCollection services)
    {
        // Validador - una instancia por request (Scoped)
        services.AddScoped<IValidador<Persona>, ValidadorPersona>();
        
        // Cache LRU - UNA SOLA INSTANCIA para toda la app (Singleton)
        // Capacidad de 3 elementos - se comparte entre todas las peticiones
        services.AddSingleton<ICache<int, Persona>>(new LruCache<int, Persona>(3));
        
        // Servicio principal - una instancia por request (Scoped)
        // Usa alias Svc para evitar conflicto de nombres
        services.AddScoped<PersonaService.Services.IPersonaService, Svc>();
    }
}
