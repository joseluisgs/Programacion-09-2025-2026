using System.Text;
using CSharpFunctionalExtensions;
using GestionAcademica.Config;
using GestionAcademica.Errors.Backup;
using GestionAcademica.Errors.Common;
using GestionAcademica.Models;
using GestionAcademica.Models.Academia;
using GestionAcademica.Models.Personas;
using Serilog;

namespace GestionAcademica.Storage.Text;

public class AcademiaTextStorage : IAcademiaTextStorage
{
    private readonly ILogger _logger = Log.ForContext<AcademiaTextStorage>();

    public AcademiaTextStorage()
    {
        _logger.Debug("Inicializando la clase AcademiaTextStorage");
        InitStorage();
    }

    public Result<bool, DomainError> Salvar(IEnumerable<Persona> items, string path)
    {
        try
        {
            _logger.Debug("Guardando los items en el archivo '{path}'", path);
            using var writer = new StreamWriter(path, false, Encoding.UTF8);

            foreach (var p in items)
                writer.WriteLine(ObteneLineaDePersona(p));
            
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
            var personas = File.ReadLines(path, Encoding.UTF8)
                .Select(ObtenerPersonaDeLinea);
            
            return Result.Success<IEnumerable<Persona>, DomainError>(personas);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al cargar los items del archivo '{path}'", path);
            return Result.Failure<IEnumerable<Persona>, DomainError>(BackupErrors.InvalidBackupFile(ex.Message));
        }
    }

    private Persona ObtenerPersonaDeLinea(string linea)
    {
        try
        {
            var partes = linea.Split(';');
            return partes[7] switch
            {
                "Estudiante" => new Estudiante
                {
                    Id = int.Parse(partes[0]),
                    Dni = partes[1],
                    Nombre = partes[2],
                    Apellidos = partes[3],
                    CreatedAt = DateTime.Parse(partes[4]),
                    UpdatedAt = DateTime.Parse(partes[5]),
                    IsDeleted = bool.Parse(partes[6]),
                    Calificacion = double.Parse(partes[8]),
                    Ciclo = Enum.Parse<Ciclo>(partes[9]),
                    Curso = Enum.Parse<Curso>(partes[10])
                },
                "Docente" => new Docente
                {
                    Id = int.Parse(partes[0]),
                    Dni = partes[1],
                    Nombre = partes[2],
                    Apellidos = partes[3],
                    CreatedAt = DateTime.Parse(partes[4]),
                    UpdatedAt = DateTime.Parse(partes[5]),
                    IsDeleted = bool.Parse(partes[6]),
                    Experiencia = int.Parse(partes[8]),
                    Especialidad = partes[9],
                    Ciclo = Enum.Parse<Ciclo>(partes[10])
                },
                _ => throw new InvalidOperationException($"Tipo de persona desconocido en la línea: {linea}")
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al convertir la línea a persona: {message}", ex.Message);
            throw;
        }
    }

    private string ObteneLineaDePersona(Persona persona)
    {
        try
        {
            var datosComunes = $"{persona.Id};{persona.Dni};{persona.Nombre};{persona.Apellidos};{persona.CreatedAt};{persona.UpdatedAt};{persona.IsDeleted}";

            var datosPropios = persona switch
            {
                Estudiante e => $"Estudiante;{e.Calificacion};{e.Ciclo};{e.Curso}",
                Docente d => $"Docente;{d.Experiencia};{d.Especialidad};{d.Ciclo}",
                _ => throw new InvalidOperationException("Tipo de persona desconocido")
            };
            return $"{datosComunes};{datosPropios}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al convertir la persona a línea de texto: {message}", ex.Message);
            throw;
        }
    }

    private void InitStorage()
    {
        if (Directory.Exists(AppConfig.DataFolder))
            return;
        _logger.Debug("El directorio 'data' no existe. Creándolo...");
        Directory.CreateDirectory(AppConfig.DataFolder);
    }
}
