using System.Text;
using CSharpFunctionalExtensions;
using GestionAcademica.Config;
using GestionAcademica.Dto;
using GestionAcademica.Errors.Backup;
using GestionAcademica.Errors.Common;
using GestionAcademica.Mappers.Personas;
using GestionAcademica.Models;
using GestionAcademica.Models.Personas;
using GestionAcademica.Storage.Text;
using Serilog;

namespace GestionAcademica.Storage.Csv;

public class AcademiaCsvStorage : IAcademiaCsvStorage
{
    private readonly ILogger _logger = Log.ForContext<AcademiaCsvStorage>();

    public AcademiaCsvStorage()
    {
        _logger.Debug("Inicializando la clase AcademiaCsvStorage");
        InitStorage();
    }

    public Result<bool, DomainError> Salvar(IEnumerable<Persona> items, string path)
    {
        try
        {
            _logger.Debug("Guardando los items en el archivo '{path}'", path);
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine("Id;Dni;Nombre;Apellidos;Tipo;Experiencia;Especialidad;Ciclo;Curso;Calificacion;CreatedAt;UpdatedAt;IsDeleted");

            foreach (var p in items)
            {
                var dto = p.ToDto();
                writer.WriteLine($"{dto.Id};{dto.Dni};{dto.Nombre};{dto.Apellidos};{dto.Tipo};{dto.Experiencia};{dto.Especialidad};{dto.Ciclo};{dto.Curso};{dto.Calificacion};{dto.CreatedAt};{dto.UpdatedAt};{dto.IsDeleted}");
            }
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
                .Skip(1)
                .Select(linea => linea.Split(';'))
                .Select(campos => new PersonaDto(
                    int.Parse(campos[0]),
                    campos[1],
                    campos[2],
                    campos[3],
                    campos[4],
                    campos[5],
                    campos[6],
                    campos[7],
                    campos[8],
                    campos[9],
                    campos[10],
                    campos[11],
                    bool.TryParse(campos[12], out var isDel) && isDel
                ).ToModel());

            return Result.Success<IEnumerable<Persona>, DomainError>(personas);
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
