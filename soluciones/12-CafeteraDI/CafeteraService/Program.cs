// Import del contenedor DI
using Microsoft.Extensions.DependencyInjection;
// Import de la infraestructura
using CafeteraService.Infrastructure;
// Import de los modelos
using CafeteraService.Models;

// Alias para evitar conflicto con namespace
using ICalentador = CafeteraService.Models.ICalentador;
using IBomba = CafeteraService.Models.IBomba;
using ICafetera = CafeteraService.Models.ICafetera;

// ============================================
// EJEMPLO DE INYECCIÓN DE DEPENDENCIAS
// ============================================

Console.WriteLine("=== Ejemplo de DI con Cafetera ===");
Console.WriteLine();

// 1. Construye el ServiceProvider con las dependencias registradas
//    Esto crea el contenedor DI y registra todos los servicios
var serviceProvider = DependenciesProvider.BuildServiceProvider();

// 2. Obtiene la cafetera desde DI
//    El contenedor inyecta automáticamente las dependencias (ICalentador, IBomba)
var cafetera = serviceProvider.GetRequiredService<ICafetera>();

// 3. Sirve el café
Console.WriteLine($"Cafetera: {cafetera}");
cafetera.Servir();

Console.WriteLine();

// ============================================
// Ejemplo 2: Singleton - misma instancia
// ============================================
Console.WriteLine("=== Segunda llamada (misma instancia - Singleton) ===");

// Obtiene otra cafetereta - será la MISMA instancia porque es Singleton
var cafetera1 = serviceProvider.GetRequiredService<ICafetera>();
var cafetera2 = serviceProvider.GetRequiredService<ICafetera>();

Console.WriteLine($"Cafetera1: {cafetera1}");
Console.WriteLine($"Cafetera2: {cafetera2}");
Console.WriteLine($"¿Son la misma instancia? {ReferenceEquals(cafetera1, cafetera2)}");

Console.WriteLine();

// ============================================
// Ejemplo 3: Verificar dependencias compartidas
// ============================================
Console.WriteLine("=== Verificando dependencias compartidas ===");

// Obtiene el calentador - será la MISMA instancia
var calentador1 = serviceProvider.GetRequiredService<ICalentador>();
var calentador2 = serviceProvider.GetRequiredService<ICalentador>();

Console.WriteLine($"Calentador1: {calentador1}");
Console.WriteLine($"Calentador2: {calentador2}");
Console.WriteLine($"¿Son la misma instancia? {ReferenceEquals(calentador1, calentador2)}");

Console.WriteLine();
Console.WriteLine("=== Fin ===");
