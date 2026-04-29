using System.IO;
using CSharpFunctionalExtensions;
using GestionAcademica.Cache;
using GestionAcademica.Config;
using GestionAcademica.Enums;
using GestionAcademica.Errors.Common;
using GestionAcademica.Errors.Personas;
using GestionAcademica.Models;
using GestionAcademica.Models.Academia;
using GestionAcademica.Models.Informes;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories;
using GestionAcademica.Repositories.Personas;
using GestionAcademica.Repositories.Personas.Base;
using GestionAcademica.Services.Report;
using GestionAcademica.Storage.Common;
using GestionAcademica.Validators.Common;
using Serilog;

namespace GestionAcademica.Services;

public class AcademiaService(
    IPersonasRepository repository,
    IStorage<Persona> storage,
    IValidador<Persona> valEstudiante,
    IValidador<Persona> valDocente,
    ICache<int, Persona> cache,
    IBackupService backupService,
    IReportService reportService
) : IAcademiaService
{
    private readonly ILogger _logger = Log.ForContext<AcademiaService>();
    private readonly IBackupService _backupService = backupService;
    private readonly IReportService _reportService = reportService;

    public int TotalPersonas => repository.GetAll().Count();

    public IEnumerable<Persona> GetAll()
    {
        _logger.Information("Obteniendo todas las personas.");
        return repository.GetAll();
    }

    public IEnumerable<Estudiante> GetEstudiantesOrderBy(TipoOrdenamiento ordenamiento = TipoOrdenamiento.Dni)
    {
        _logger.Information("Obteniendo estudiantes ordenados por {ordenamiento}.", ordenamiento);
        return GetAllOrderBy(ordenamiento, p => p is Estudiante)
            .Cast<Estudiante>();
    }

    public IEnumerable<Docente> GetDocentesOrderBy(TipoOrdenamiento ordenamiento = TipoOrdenamiento.Dni)
    {
        _logger.Information("Obteniendo docentes ordenados por {ordenamiento}.", ordenamiento);
        return GetAllOrderBy(ordenamiento, p => p is Docente)
            .Cast<Docente>();
    }

    public InformeEstudiante GenerarInformeEstudiante(Ciclo? ciclo = null, Curso? curso = null)
    {
        _logger.Information("Generando informe estadístico de estudiantes con filtro Ciclo: {ciclo}, Curso: {curso}.",
            ciclo, ciclo);

        var estudiantes = GetEstudiantesOrderBy(TipoOrdenamiento.Nota)
            .Where(e => (ciclo == null || e.Ciclo == ciclo) && (curso == null || e.Curso == curso))
            .ToList();

        var total = estudiantes.Count;
        if (total == 0) return new InformeEstudiante();

        return new InformeEstudiante
        {
            PorNota = estudiantes,
            TotalEstudiantes = total,
            Aprobados = estudiantes.Count(e => e.Calificacion >= AppConfig.NotaAprobado),
            Suspensos = estudiantes.Count(e => e.Calificacion < AppConfig.NotaAprobado),
            NotaMedia = estudiantes.Average(e => e.Calificacion)
        };
    }

    public InformeDocente GenerarInformeDocente(Ciclo? ciclo = null)
    {
        _logger.Information("Generando informe estadístico de docentes con filtro Ciclo: {ciclo}.", ciclo);

        var docentes = GetDocentesOrderBy(TipoOrdenamiento.Experiencia)
            .Where(d => ciclo == null || d.Ciclo == ciclo)
            .ToList();

        var total = docentes.Count;
        return new InformeDocente
        {
            PorExperiencia = docentes,
            TotalDocentes = total,
            ExperienciaMedia = total > 0 ? docentes.Average(d => d.Experiencia) : 0
        };
    }

    public IEnumerable<Persona> GetAllOrderBy(TipoOrdenamiento orden = TipoOrdenamiento.Dni,
        Predicate<Persona>? filtro = null)
    {
        _logger.Information("Obteniendo todas las personas ordenadas por {orden} con filtro: {filtro}.", orden,
            filtro != null ? "Sí" : "No");

        var lista = filtro == null
            ? repository.GetAll()
            : repository.GetAll().Where(p => filtro(p));

        var comparadores = new Dictionary<TipoOrdenamiento, Func<IOrderedEnumerable<Persona>>>
        {
            { TipoOrdenamiento.Id, () => lista.OrderBy(p => p.Id) },
            { TipoOrdenamiento.Dni, () => lista.OrderBy(p => p.Dni) },
            { TipoOrdenamiento.Nombre, () => lista.OrderBy(p => p.Nombre) },
            { TipoOrdenamiento.Apellidos, () => lista.OrderBy(p => p.Apellidos) },
            { TipoOrdenamiento.Ciclo, () => lista.OrderBy(p => ObtenerCicloTexto(p)) },
            { TipoOrdenamiento.Nota, () => lista.OrderByDescending(p => p is Estudiante e ? e.Calificacion : -1) },
            { TipoOrdenamiento.Experiencia, () => lista.OrderByDescending(p => p is Docente d ? d.Experiencia : -1) },
            { TipoOrdenamiento.Curso, () => lista.OrderBy(p => p is Estudiante e ? (int)e.Curso : int.MaxValue) }
        };

        return comparadores.TryGetValue(orden, out var comparador)
            ? comparador()
            : lista.OrderBy(p => p.Id);
    }

    public Result<Persona, DomainError> GetById(int id)
    {
        _logger.Information("Obteniendo persona con ID {id}", id);
        
        if (cache.Get(id) is {} cached)
            return Result.Success<Persona, DomainError>(cached);
        
        if (repository.GetById(id) is {} persona)
        {
            cache.Add(id, persona);
            return Result.Success<Persona, DomainError>(persona);
        }
        
        return Result.Failure<Persona, DomainError>(PersonaErrors.NotFound(id.ToString()));
    }

    public Result<Persona, DomainError> GetByDni(string dni)
    {
        _logger.Information("Obteniendo persona con DNI {dni}", dni);
        
        if (repository.GetByDni(dni) is {} persona)
            return Result.Success<Persona, DomainError>(persona);
        
        return Result.Failure<Persona, DomainError>(PersonaErrors.NotFound(dni));
    }

    public Result<Persona, DomainError> Save(Persona persona)
    {
        _logger.Information("Guardando nueva persona: {persona}", persona);
        
        return ValidarPersona(persona)
            .Bind(_ => CheckDniNotExists(persona.Dni))
            .Map(p => repository.Create(p)!);
    }

    public Result<Persona, DomainError> Update(int id, Persona persona)
    {
        _logger.Information("Actualizando persona con ID {id}: {persona}", id, persona);
        
        return CheckExists(id)
            .Bind(_ => ValidarPersona(persona))
            .Bind(_ => CheckDniNotExistsForUpdate(id, persona.Dni))
            .Map(p => 
            {
                cache.Remove(id);
                return repository.Update(id, p)!;
            });
    }

    public Result<Persona, DomainError> Delete(int id)
    {
        _logger.Information("Eliminando persona con ID {id}", id);
        
        return CheckExists(id)
            .Map(p =>
            {
                cache.Remove(id);
                return repository.Delete(id)!;
            });
    }

    public Result<int, DomainError> ExportarDatos()
    {
        _logger.Information("Exportando datos a almacenamiento externo.");
        
        var personas = repository.GetAll();
        var count = personas.Count();

        _logger.Information("Exportando datos a almacenamiento externo. Total personas: {count}.", count);
        
        return storage.Salvar(personas, AppConfig.AcademiaFile)
            .Map(_ => count);
    }

    public Result<int, DomainError> ImportarDatos()
    {
        _logger.Information("Importando datos desde almacenamiento externa.");
        
        return storage.Cargar(AppConfig.AcademiaFile)
            .Map(personas =>
            {
                repository.DeleteAll();
                var contador = 0;
                foreach (var p in personas)
                {
                    repository.Create(p);
                    contador++;
                }
                _logger.Information("Datos importados correctamente. Total personas: {count}", contador);
                return contador;
            });
    }

    public Result<string, DomainError> RealizarBackup()
    {
        _logger.Information("Realizando backup del sistema.");
        var personas = repository.GetAll();
        return _backupService.RealizarBackup(personas);
    }

    public Result<int, DomainError> RestaurarBackup(string archivoBackup)
    {
        _logger.Information("Restaurando backup desde: {archivo}", archivoBackup);
        
        return _backupService.RestaurarBackup(archivoBackup)
            .Map(personas =>
            {
                repository.DeleteAll();
                var contador = 0;
                foreach (var p in personas)
                {
                    repository.Create(p);
                    contador++;
                }
                _logger.Information("Restauración completada. Total registros: {count}", contador);
                return contador;
            });
    }

    public IEnumerable<string> ListarBackups()
    {
        return _backupService.ListarBackups();
    }

    private Result<Persona, DomainError> ValidarPersona(Persona persona)
    {
        var validationResult = persona switch
        {
            Estudiante => valEstudiante.Validar(persona),
            Docente => valDocente.Validar(persona),
            _ => Result.Failure<Persona, DomainError>(PersonaErrors.Validation(new[] { "Tipo de entidad no soportada para validación." }))
        };

        return validationResult;
    }

    private Result<Persona, DomainError> CheckExists(int id)
    {
        if (repository.GetById(id) is {} exists)
            return Result.Success<Persona, DomainError>(exists);
        return Result.Failure<Persona, DomainError>(PersonaErrors.NotFound(id.ToString()));
    }

    private Result<Persona, DomainError> CheckDniNotExists(string dni)
    {
        if (repository.GetByDni(dni) is {} exists)
            return Result.Failure<Persona, DomainError>(PersonaErrors.AlreadyExists(dni));
        return Result.Success<Persona, DomainError>(null!);
    }

    private Result<Persona, DomainError> CheckDniNotExistsForUpdate(int id, string dni)
    {
        if (repository.GetByDni(dni) is {} exists && exists.Id != id)
            return Result.Failure<Persona, DomainError>(PersonaErrors.AlreadyExists(dni));
        return Result.Success<Persona, DomainError>(null!);
    }

    private static string ObtenerCicloTexto(Persona p)
    {
        return p switch
        {
            Estudiante e => e.Ciclo.ToString(),
            Docente d => d.Ciclo.ToString(),
            _ => string.Empty
        };
    }

    public Result<string, DomainError> GenerarInformeEstudiantesHtml()
    {
        var estudiantes = GetEstudiantesOrderBy(TipoOrdenamiento.Nota);
        var fileName = $"informe_estudiantes_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        
        return _reportService.GenerarInformeEstudiantesHtml(estudiantes)
            .Bind(html => _reportService.GuardarInforme(html, fileName))
            .Map(_ => Path.Combine(AppConfig.ReportDirectory, fileName));
    }

    public Result<string, DomainError> GenerarInformeDocentesHtml()
    {
        var docentes = GetDocentesOrderBy(TipoOrdenamiento.Experiencia);
        var fileName = $"informe_docentes_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        
        return _reportService.GenerarInformeDocentesHtml(docentes)
            .Bind(html => _reportService.GuardarInforme(html, fileName))
            .Map(_ => Path.Combine(AppConfig.ReportDirectory, fileName));
    }

    public Result<string, DomainError> GenerarListadoPersonasHtml()
    {
        var personas = GetAll();
        var fileName = $"listado_personas_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        
        return _reportService.GenerarListadoPersonasHtml(personas)
            .Bind(html => _reportService.GuardarInforme(html, fileName))
            .Map(_ => Path.Combine(AppConfig.ReportDirectory, fileName));
    }
}
