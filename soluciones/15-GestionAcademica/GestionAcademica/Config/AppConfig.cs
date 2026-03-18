using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace GestionAcademica.Config;

/// <summary>
/// Clase de configuración que lee desde appsettings.json.
/// </summary>
public class AppConfig
{
    private static readonly IConfiguration Config;
    
    static AppConfig()
    {
        Config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    public static double NotaAprobado => Config.GetValue<double>("Academica:NotaAprobado");

    public static CultureInfo Locale => CultureInfo.GetCultureInfo("es-ES");

    public static string DataFolder => Path.Combine(
        Environment.CurrentDirectory, 
        Config.GetValue<string>("Repository:Directory") ?? "data");

    public static string ConnectionString => Config.GetValue<string>("Repository:ConnectionString") ?? "Data Source=data/academia.db";

    public static string StorageType => Config.GetValue<string>("Storage:Type") ?? "json";

    public static string RepositoryType
    {
        get
        {
            var type = Config.GetValue<string>("Repository:Type") ?? "memory";
            return type.ToLower() switch
            {
                "memory" => "memory",
                "binary" => "binary",
                "json" => "json",
                "dapper" => "dapper",
                "adonet" => "adonet",
                "efcore" => "efcore",
                _ => "memory"
            };
        }
    }

    public static string AcademiaFile
    {
        get
        {
            var extension = StorageType.ToLower() switch
            {
                "json" => "json",
                "xml" => "xml",
                "csv" or "csv-alt" => "csv",
                "txt" or "text" => "txt",
                "bin" => "bin",
                _ => "json"
            };
            return Path.Combine(DataFolder, $"academia.{extension}");
        }
    }

    public static int CacheSize => Config.GetValue<int>("Cache:Size", 10);

    public static bool DropData => Config.GetValue<bool>("Repository:DropData", false);

    public static bool SeedData => Config.GetValue<bool>("Repository:SeedData", true);

    public static string BackupDirectory => Path.Combine(
        AppContext.BaseDirectory, 
        Config.GetValue<string>("Backup:Directory") ?? "back");

    public static string BackupFormat
    {
        get
        {
            var format = Config.GetValue<string>("Backup:Format") ?? "json";
            return format.ToLower() switch
            {
                "json" => "json",
                "xml" => "xml",
                "csv" or "csv-alt" => "csv",
                "txt" or "text" => "txt",
                "bin" => "bin",
                _ => "json"
            };
        }
    }

    public static bool IsDevelopment => Config.GetValue<bool>("Development:Enabled", false);

    public static string ReportDirectory => Path.Combine(
        AppContext.BaseDirectory, 
        Config.GetValue<string>("Reports:Directory") ?? "reports");
}
