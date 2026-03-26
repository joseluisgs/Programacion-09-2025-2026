// Import para leer configuración desde JSON
using Microsoft.Extensions.Configuration;

namespace PersonaService.Config;

// Clase de configuración que lee desde appsettings.json
public class AppConfig
{
    // Sub-configuración de base de datos
    public DatabaseConfig Database { get; set; } = new();
    
    // Flag de desarrollo
    public bool Development { get; set; }
    
    // Sub-configuración de logging
    public LoggingConfig Logging { get; set; } = new();

    // Carga la configuración desde appsettings.json
    public static void Load()
    {
        // Obtiene el directorio donde está el ejecutable
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        
        // Construye el configuration builder
        var configuration = new ConfigurationBuilder()
            // Establece el directorio base
            .SetBasePath(basePath)
            // Lee appsettings.json, optional:false (es obligatorio), reloadOnChange:true (recarga si cambia)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Crea una nueva instancia de AppConfig
        var appConfig = new AppConfig();
        
        // Lee cada propiedad del JSON con clave "Database:Provider", etc.
        // Si no existe, usa el valor por defecto después de ??
        appConfig.Database.Provider = configuration.GetValue<string>("Database:Provider") ?? "EfCoreSqlite";
        appConfig.Database.ConnectionString = configuration.GetValue<string>("Database:ConnectionString") ?? "Data Source=personas.db";
        appConfig.Database.CreateTable = configuration.GetValue<bool>("Database:CreateTable", true);
        appConfig.Database.DropData = configuration.GetValue<bool>("Database:DropData", false);
        appConfig.Database.SeedData = configuration.GetValue<bool>("Database:SeedData", true);
        appConfig.Development = configuration.GetValue<bool>("Development:Enabled", false);

        // Guarda en la propiedad estática Instance para acceso posterior desde cualquier parte
        Instance = appConfig;
    }

    // Instancia singleton estática - accesible desde cualquier parte de la aplicación
    public static AppConfig Instance { get; private set; } = new();

    // Propiedades estáticas de conveniencia para acceso directo sin usar Instance
    public static string Provider => Instance.Database.Provider;
    public static string ConnectionString => Instance.Database.ConnectionString;
    public static bool CreateTable => Instance.Database.CreateTable;
    public static bool DropData => Instance.Database.DropData;
    public static bool SeedData => Instance.Database.SeedData;
    public static bool IsDevelopment => Instance.Development;
}

// Configuración de base de datos
public class DatabaseConfig
{
    // Proveedor de base de datos: EfCoreSqlite o DapperSqlite
    public string Provider { get; set; } = "EfCoreSqlite";
    // Ruta del archivo de base de datos SQLite
    public string ConnectionString { get; set; } = "Data Source=personas.db";
    // Indica si se debe crear la tabla al iniciar
    public bool CreateTable { get; set; } = true;
    // Indica si se deben borrar los datos al iniciar (modo desarrollo)
    public bool DropData { get; set; } = false;
    // Indica si se deben insertar datos de prueba (seed data)
    public bool SeedData { get; set; } = true;
}

// Configuración de logging
public class LoggingConfig
{
    // Diccionario de niveles de log
    public Dictionary<string, string> LogLevel { get; set; } = new()
    {
        { "Default", "Information" }
    };
}
