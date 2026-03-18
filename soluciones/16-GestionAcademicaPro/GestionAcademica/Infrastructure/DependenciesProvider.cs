using Microsoft.Extensions.DependencyInjection;
using GestionAcademica.Cache;
using GestionAcademica.Config;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories;
using GestionAcademica.Repositories.Personas.Base;
using GestionAcademica.Repositories.Personas.Memory;
using GestionAcademica.Repositories.Personas.Binary;
using GestionAcademica.Repositories.Personas.Json;
using GestionAcademica.Repositories.Dapper;
using GestionAcademica.Repositories.Personas.AdoNet;
using GestionAcademica.Repositories.EfCore;
using GestionAcademica.Services;
using GestionAcademica.Services.Report;
using GestionAcademica.Storage;
using GestionAcademica.Storage.Common;
using GestionAcademica.Storage.Json;
using GestionAcademica.Storage.Xml;
using GestionAcademica.Storage.Csv;
using GestionAcademica.Storage.CsvAlt;
using GestionAcademica.Storage.Text;
using GestionAcademica.Storage.Binary;
using GestionAcademica.Validators;
using GestionAcademica.Validators.Common;

namespace GestionAcademica.Infrastructure;

public static class DependenciesProvider
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        
        RegisterStorages(services);
        RegisterRepositories(services);
        RegisterServices(services);
        
        return services.BuildServiceProvider();
    }

    private static void RegisterStorages(IServiceCollection services)
    {
        services.AddTransient<IStorage<Persona>>(sp =>
        {
            var storageType = AppConfig.StorageType.ToLower();
            return storageType switch
            {
                "json" => new AcademiaJsonStorage(),
                "xml" => new AcademiaXmlStorage(),
                "csv" => new AcademiaCsvStorage(),
                "csv-alt" => new AcademiaCsvAltStorage(),
                "txt" or "text" => new AcademiaTextStorage(),
                "bin" or "binary" => new AcademiaBinStorage(),
                _ => new AcademiaJsonStorage()
            };
        });

    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddSingleton<IPersonasRepository>(sp =>
        {
            var repoType = AppConfig.RepositoryType.ToLower();
            return repoType switch
            {
                "memory" => new PersonasMemoryRepository(),
                "binary" => new PersonasBinaryRepository(),
                "json" => new PersonasJsonRepository(),
                "dapper" => new PersonasDapperRepository(),
                "adonet" => new PersonasAdoRepository(),
                "efcore" => new PersonasEfRepository(),
                _ => new PersonasMemoryRepository()
            };
        });
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICache<int, Persona>>(sp =>
            new LruCache<int, Persona>(AppConfig.CacheSize));

        services.AddTransient<IValidador<Persona>, ValidadorEstudiante>();
        services.AddTransient<IValidador<Persona>, ValidadorDocente>();

        services.AddTransient<IBackupService, BackupService>();
        
        services.AddTransient<IReportService, ReportService>();
        
        services.AddScoped<IAcademiaService, AcademiaService>();
    }
}
