using Microsoft.Extensions.Configuration;

namespace CarroCompraService.Config;

public class AppConfig
{
    public DatabaseConfig Database { get; set; } = new();
    public CacheConfig Cache { get; set; } = new();
    public FacturaConfig Factura { get; set; } = new();
    public bool Development { get; set; }

    public static void Load()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var appConfig = new AppConfig();
        
        appConfig.Database.Provider = configuration.GetValue<string>("Database:Provider") ?? "EfCoreSqlite";
        appConfig.Database.ConnectionString = configuration.GetValue<string>("Database:ConnectionString") ?? "Data Source=carrocompra.db";
        appConfig.Database.CreateTable = configuration.GetValue<bool>("Database:CreateTable", true);
        appConfig.Database.DropData = configuration.GetValue<bool>("Database:DropData", false);
        appConfig.Database.SeedData = configuration.GetValue<bool>("Database:SeedData", true);
        
        appConfig.Cache.Size = configuration.GetValue<int>("Cache:Size", 10);
        
        appConfig.Factura.Directory = configuration.GetValue<string>("Factura:Directory") ?? "Facturas";
        appConfig.Factura.TiendaNombre = configuration.GetValue<string>("Factura:TiendaNombre") ?? "Mi Tienda";
        appConfig.Factura.TiendaDireccion = configuration.GetValue<string>("Factura:TiendaDireccion") ?? "Dirección";
        
        appConfig.Development = configuration.GetValue<bool>("Development:Enabled", false);

        Instance = appConfig;
    }

    public static AppConfig Instance { get; private set; } = new();

    public static string Provider => Instance.Database.Provider;
    public static string ConnectionString => Instance.Database.ConnectionString;
    public static bool CreateTable => Instance.Database.CreateTable;
    public static bool DropData => Instance.Database.DropData;
    public static bool SeedData => Instance.Database.SeedData;
    public static int CacheSize => Instance.Cache.Size;
    public static string FacturaDirectory => Instance.Factura.Directory;
    public static string FacturaTiendaNombre => Instance.Factura.TiendaNombre;
    public static string FacturaTiendaDireccion => Instance.Factura.TiendaDireccion;
    public static bool IsDevelopment => Instance.Development;
}

public class DatabaseConfig
{
    public string Provider { get; set; } = "EfCoreSqlite";
    public string ConnectionString { get; set; } = "Data Source=carrocompra.db";
    public bool CreateTable { get; set; } = true;
    public bool DropData { get; set; } = false;
    public bool SeedData { get; set; } = true;
}

public class CacheConfig
{
    public int Size { get; set; } = 10;
}

public class FacturaConfig
{
    public string Directory { get; set; } = "Facturas";
    public string TiendaNombre { get; set; } = "Mi Tienda";
    public string TiendaDireccion { get; set; } = "Dirección";
}
