using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CSharpFunctionalExtensions;
using GestionAcademica.Config;
using GestionAcademica.Dto;
using GestionAcademica.Errors.Backup;
using GestionAcademica.Errors.Common;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models;
using GestionAcademica.Models.Personas;
using Serilog;

namespace GestionAcademica.Storage.CsvAlt;

public class AcademiaCsvAltStorage : IAcademiaCsvAltStorage
{
    private readonly ILogger _logger = Log.ForContext<AcademiaCsvAltStorage>();

    private readonly CsvConfiguration CsvConfiguration = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ";",
        HasHeaderRecord = true
    };

    public AcademiaCsvAltStorage()
    {
        _logger.Debug("Inicializando la clase AcademiaCsvAltStorage");
        InitStorage();
    }

    public Result<bool, DomainError> Salvar(IEnumerable<Persona> items, string path)
    {
        try
        {
            _logger.Debug("Guardando los items en el archivo '{path}'", path);
            var dtos = items.Select(p => p.ToDto()).ToList();
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CsvConfiguration);
            csv.WriteRecords(dtos);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al guardar los items en el archivo '{path}'", path);
            return Result.Failure<bool, DomainError>(BackupErrors.CreationError(ex.Message));
        }
    }

    public Result<IEnumerable<Persona>, DomainError> Cargar(string path)
    {
        _logger.Debug("Cargando los items del archivo '{path}'", path);
        
        if (!Path.Exists(path))
        {
            _logger.Warning("El archivo '{path}' no existe.", path);
            return Result.Failure<IEnumerable<Persona>, DomainError>(BackupErrors.FileNotFound(path));
        }

        try
        {
            using var reader = new StreamReader(path, Encoding.UTF8);
            using var csv = new CsvReader(reader, CsvConfiguration);
            var dtos = csv.GetRecords<PersonaDto>().ToList();
            return Result.Success<IEnumerable<Persona>, DomainError>(dtos.Select(dto => dto.ToModel()).ToList());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al cargar los items del archivo '{path}'", path);
            return Result.Failure<IEnumerable<Persona>, DomainError>(BackupErrors.InvalidBackupFile(ex.Message));
        }
    }

    private void InitStorage()
    {
        if (Directory.Exists(AppConfig.DataFolder))
            return;
        _logger.Debug("El directorio 'data' no existe. Creándolo...");
        Directory.CreateDirectory("data");
    }
}
