using System.IO.Compression;
using CSharpFunctionalExtensions;
using GestionAcademica.Config;
using GestionAcademica.Errors.Backup;
using GestionAcademica.Errors.Common;
using GestionAcademica.Models.Personas;
using GestionAcademica.Storage.Common;
using Serilog;

namespace GestionAcademica.Services;

public class BackupService(
    IStorage<Persona> storage
) : IBackupService
{
    private readonly string _backDirectory = AppConfig.BackupDirectory;
    private readonly ILogger _logger = Log.ForContext<BackupService>();

    public Result<string, DomainError> RealizarBackup(IEnumerable<Persona> personas)
    {
        _logger.Information("Iniciando proceso de backup.");

        var personasList = personas.ToList();
        if (personasList.Count == 0)
        {
            _logger.Warning("No hay datos para respaldar.");
            return Result.Failure<string, DomainError>(BackupErrors.CreationError("No hay datos para respaldar."));
        }

        try
        {
            Directory.CreateDirectory(_backDirectory);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al crear el directorio de backup: {dir}", _backDirectory);
            return Result.Failure<string, DomainError>(BackupErrors.DirectoryError($"No se pudo crear el directorio: {_backDirectory}"));
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"backup-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var jsonPath = Path.Combine(tempDir, "data.json");
            var salvarResult = storage.Salvar(personasList, jsonPath);
            if (salvarResult.IsFailure)
            {
                _logger.Error("Error al serializar los datos.");
                return Result.Failure<string, DomainError>(BackupErrors.CreationError("Error al serializar los datos."));
            }

            var fecha = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var zipPath = Path.Combine(_backDirectory, $"{fecha}-back.zip");

            try
            {
                ZipFile.CreateFromDirectory(tempDir, zipPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al crear el archivo ZIP.");
                return Result.Failure<string, DomainError>(BackupErrors.CreationError("Error al comprimir el backup."));
            }

            _logger.Information("Backup creado correctamente: {zipPath}", zipPath);
            return Result.Success<string, DomainError>(zipPath);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
                _logger.Debug("Directorio temporal limpiado.");
            }
        }
    }

    public Result<IEnumerable<Persona>, DomainError> RestaurarBackup(string archivoBackup)
    {
        _logger.Information("Iniciando restauración desde: {archivo}", archivoBackup);

        if (!File.Exists(archivoBackup))
        {
            _logger.Warning("Archivo de backup no encontrado: {path}", archivoBackup);
            return Result.Failure<IEnumerable<Persona>, DomainError>(BackupErrors.FileNotFound(archivoBackup));
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"restore-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            try
            {
                ZipFile.ExtractToDirectory(archivoBackup, tempDir);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al extraer el archivo ZIP.");
                return Result.Failure<IEnumerable<Persona>, DomainError>(BackupErrors.InvalidBackupFile("No se pudo extraer el archivo ZIP."));
            }

            var jsonPath = Path.Combine(tempDir, "data.json");
            if (!File.Exists(jsonPath))
            {
                _logger.Warning("El archivo de backup no contiene datos válidos (data.json no encontrado).");
                return Result.Failure<IEnumerable<Persona>, DomainError>(BackupErrors.InvalidBackupFile("El archivo de backup no contiene datos válidos."));
            }

            var cargarResult = storage.Cargar(jsonPath);
            if (cargarResult.IsFailure)
            {
                _logger.Error("Error al deserializar los datos del backup.");
                return Result.Failure<IEnumerable<Persona>, DomainError>(BackupErrors.InvalidBackupFile("El archivo de backup contiene datos corruptos."));
            }

            _logger.Information("Datos extraídos del backup correctamente.");
            return Result.Success<IEnumerable<Persona>, DomainError>(cargarResult.Value);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
                _logger.Debug("Directorio temporal limpiado.");
            }
        }
    }

    public IEnumerable<string> ListarBackups()
    {
        if (!Directory.Exists(_backDirectory)) return Enumerable.Empty<string>();

        return Directory.GetFiles(_backDirectory, "*.zip")
            .OrderByDescending(f => File.GetCreationTime(f));
    }
}
