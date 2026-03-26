// Import del contenedor DI nativo de .NET
using Microsoft.Extensions.DependencyInjection;
// Import de los modelos
using CafeteraService.Models;

namespace CafeteraService.Infrastructure;

/// <summary>
/// Proveedor centralizado de dependencias.
/// </summary>
public static class DependenciesProvider
{
    /// <summary>
    /// Método principal que registra todos los servicios en el contenedor DI
    /// </summary>
    public static IServiceProvider BuildServiceProvider()
    {
        // 1. Crea la colección de servicios
        var services = new ServiceCollection();
        
        // 2. Registra las dependencias
        RegisterServices(services);
        
        // 3. Construye y retorna el ServiceProvider
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Registra todos los servicios
    /// </summary>
    private static void RegisterServices(IServiceCollection services)
    {
        // ========================================
        // CICLOS DE VIDA EN DI:
        // ========================================
        // 
        // Singleton: UNA SOLA INSTANCIA para toda la aplicación.
        //            Se crea la primera vez que se pide y se reutiliza.
        //            Ejemplo: Cache, Configuration, Logger.
        //
        // Scoped:    UNA INSTANCIA por ámbito (normalmente por request HTTP).
        //            Se crea una vez por request y se comparte dentro de ese request.
        //            Ejemplo: DbContext en aplicaciones web.
        //
        // Transient: UNA NUEVA INSTANCIA cada vez que se pide.
        //            Cada llamada crea una nueva instancia.
        //            Ejemplo: Servicios ligeros, factories.
        //
        // ========================================
        
        // --- OPCIÓN 1: Singleton (una sola instancia compartida) ---
        // Se crea una vez y se comparte en toda la aplicación.
        // Si la cafetera es singleton, también lo serán sus dependencias.
        services.AddSingleton<ICalentador, CalentadorElectrico>();
        services.AddSingleton<IBomba, Termosifon>();
        services.AddSingleton<ICafetera, Cafetera>();

        // --- OPCIÓN 2: Scoped (una instancia por ámbito) ---
        // Ejemplo:
        // services.AddScoped<ICalentador, CalentadorElectrico>();
        // services.AddScoped<IBomba, Termosifon>();
        // services.AddScoped<ICafetera, Cafetera>();

        // --- OPCIÓN 3: Transient (nueva instancia cada vez) ---
        // Ejemplo:
        // services.AddTransient<ICalentador, CalentadorElectrico>();
        // services.AddTransient<IBomba, Termosifon>();
        // services.AddTransient<ICafetera, Cafetera>();
    }
}
