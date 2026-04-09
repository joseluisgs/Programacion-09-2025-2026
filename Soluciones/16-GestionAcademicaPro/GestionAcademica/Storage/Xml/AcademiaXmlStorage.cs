using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CSharpFunctionalExtensions;
using GestionAcademica.Config;
using GestionAcademica.Dto;
using GestionAcademica.Errors.Backup;
using GestionAcademica.Errors.Common;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models;
using GestionAcademica.Models.Personas;
using Serilog;

namespace GestionAcademica.Storage.Xml;

public class AcademiaXmlStorage : IAcademiaXmlStorage
{
    private readonly ILogger _logger = Log.ForContext<AcademiaXmlStorage>();

    private readonly XmlSerializerNamespaces XmlSerializerNamespaces = new();

    private readonly XmlWriterSettings XmlWriterSettings = new()
    {
        Indent = true,
        Encoding = Encoding.UTF8
    };

    public AcademiaXmlStorage()
    {
        _logger.Debug("Inicializando la clase AcademiaXmlStorage");
        InitStorage();
    }

    public Result<bool, DomainError> Salvar(IEnumerable<Persona> items, string path)
    {
        try
        {
            _logger.Debug("Guardando los items en el archivo '{path}'", path);
            var dtos = items.Select(p => p.ToDto()).ToList();
            var serializer = new XmlSerializer(typeof(List<PersonaDto>));

            using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
            using var xmlWriter = XmlWriter.Create(streamWriter, XmlWriterSettings);
            serializer.Serialize(xmlWriter, dtos, XmlSerializerNamespaces);
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
            var serializer = new XmlSerializer(typeof(List<PersonaDto>));
            using var stream = File.OpenRead(path);
            var dtos = serializer.Deserialize(stream) as List<PersonaDto>;

            if (dtos == null)
                return Result.Failure<IEnumerable<Persona>, DomainError>(BackupErrors.InvalidBackupFile("No se pudieron deserializar los DTOs."));

            return Result.Success<IEnumerable<Persona>, DomainError>(dtos.Select(dto => dto.ToModel()));
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
