using CSharpFunctionalExtensions;
using PersonaService.Models;
using PersonaService.Repositories;
using PersonaService.Validators;
using PersonaService.Cache;
using PersonaService.Errors;
using PersonaService.Extensions;
using Serilog;

namespace PersonaService.Services;

/// <summary>
/// Servicio de gestión de personas implementado bajo el patrón Railway Oriented Programming (ROP).
/// Proporciona operaciones CRUD con validación de reglas de negocio, gestión de caché y control de errores de dominio.
/// </summary>
/// <param name="repository">Repositorio de persistencia de personas.</param>
/// <param name="validador">Validador de reglas de negocio para la entidad Persona.</param>
/// <param name="cache">Sistema de caché para optimizar las lecturas.</param>
public class PersonaService(IPersonaRepository repository, IValidador<Persona> validador, ICache<int, Persona> cache) : IPersonaService
{
    private readonly ILogger _logger = Log.ForContext<PersonaService>();

    /// <summary>
    /// Recupera todas las personas registradas en el sistema.
    /// </summary>
    /// <returns>Una colección de todas las entidades Persona.</returns>
    public IEnumerable<Persona> GetAll()
    {
        _logger.Information("Obteniendo todas las personas");
        return repository.GetAll();
    }

    /// <summary>
    /// Busca una persona por su identificador único, consultando primero en caché y luego en repositorio.
    /// </summary>
    /// <param name="id">Identificador único de la persona.</param>
    /// <returns>Un Result con la Persona si existe, o un DomainError.NotFound si no se halla.</returns>
    public Result<Persona, DomainError> GetById(int id)
    {
        _logger.Information("Buscando persona Id={Id}", id);
        
        return Maybe.From(cache.Get(id))
            .ToResult(DomainErrors.NotFound(id)) // 1. HAPPY PATH: El dato está en CACHÉ (vía rápida)
            .Tap(_ => _logger.Information("[CACHE] Encontrada")) 
            .OnFailureCompensate(_ => GetFromRepository(id));   // 2. HAPPY PATH (ALT): Si no está en caché, se busca en BD
    }

    /// <summary>
    /// Crea una nueva persona en el sistema tras validar sus datos y la unicidad de su email.
    /// </summary>
    /// <param name="persona">Entidad persona a crear.</param>
    /// <returns>Un Result con la Persona creada (incluyendo su nuevo ID) o un error de validación/duplicidad.</returns>
    public Result<Persona, DomainError> Create(Persona persona)
    {
        _logger.Information("Creando persona: {Nombre}", persona.Nombre);
        
        return Result.Success<Persona, DomainError>(persona)
            .Bind(validador.Validar)        // 1. HAPPY PATH: La persona pasa todas las reglas de validación
            .Bind(CheckEmailIsUnique)       // 2. HAPPY PATH: El email no está registrado por otro usuario
            .Map(SaveInRepository)          // 3. HAPPY PATH: La persona se guarda físicamente en la BD
            .Tap(p => _logger.Information("Creada: {Id}", p.Id)) 
            .TapError(LogError);            // Solo si algún paso de arriba falló (vía de error)
    }

    /// <summary>
    /// Actualiza los datos de una persona existente identificada por su ID.
    /// </summary>
    /// <param name="id">ID de la persona a actualizar.</param>
    /// <param name="persona">Nuevos datos de la persona.</param>
    /// <returns>Un Result con la Persona actualizada o un error si no existe o los datos son inválidos.</returns>
    public Result<Persona, DomainError> Update(int id, Persona persona)
    {
        _logger.Information("Actualizando Id={Id}", id);
        
        return CheckExists(id)              // 1. HAPPY PATH: La persona existe en el sistema
            .Bind(_ => validador.Validar(persona))      // 2. HAPPY PATH: Los nuevos datos son correctos
            .Bind(p => CheckEmailIsUniqueForUpdate(id, p)) // 3. HAPPY PATH: El nuevo email no colisiona con otros
            .Map(p => UpdateInRepository(id, p))           // 4. HAPPY PATH: Los cambios se guardan en la BD
            .Tap(_ => cache.Remove(id))     // Limpieza de caché (post-éxito)
            .TapError(LogError);            
    }

    /// <summary>
    /// Elimina una persona del sistema, permitiendo elegir entre borrado lógico o físico.
    /// </summary>
    /// <param name="id">ID de la persona a eliminar.</param>
    /// <param name="logical">Indica si el borrado es lógico (true) o físico (false).</param>
    /// <returns>Un Result con la Persona eliminada o un error si no se encontró.</returns>
    public Result<Persona, DomainError> Delete(int id, bool logical = true)
    {
        _logger.Information("Eliminando Id={Id}", id);
        
        return CheckExists(id)              // 1. HAPPY PATH: La persona existe para ser borrada
            .Map(p => repository.Delete(id, logical)!)     // 2. HAPPY PATH: Se marca como borrada/se elimina de BD
            .Tap(_ => cache.Remove(id))     // Limpieza de caché (post-éxito)
            .TapError(LogError);            
    }

    /// <summary>
    /// Intenta recuperar una persona desde el repositorio de persistencia y actualizar la caché.
    /// </summary>
    /// <param name="id">ID de la persona.</param>
    /// <returns>Result con la persona encontrada o NotFound.</returns>
    private Result<Persona, DomainError> GetFromRepository(int id) =>
        Maybe.From(repository.GetById(id))
            .ToResult(DomainErrors.NotFound(id)) // 1. HAPPY PATH: La persona se encuentra en la Base de Datos
            .Tap(p => cache.Add(id, p))                          // 2. HAPPY PATH: Se añade a la caché para futuras consultas
            .Tap(_ => _logger.Information("[DB] Encontrada y cacheada"));

    /// <summary>
    /// Comprueba si una persona existe en el repositorio.
    /// </summary>
    /// <param name="id">ID a buscar.</param>
    /// <returns>Result exitoso con la persona si existe.</returns>
    private Result<Persona, DomainError> CheckExists(int id) =>
        Maybe.From(repository.GetById(id))                       // 1. HAPPY PATH: Se consulta existencia en Repositorio
            .ToResult(DomainErrors.NotFound(id)); // 2. HAPPY PATH: Si existe, seguimos en vía ÉXITO

    /// <summary>
    /// Verifica que el email de la persona no esté duplicado en el sistema.
    /// </summary>
    /// <param name="persona">Persona con el email a validar.</param>
    /// <returns>Éxito si es único, Fallo de tipo AlreadyExists en caso contrario.</returns>
    private Result<Persona, DomainError> CheckEmailIsUnique(Persona persona) =>
        repository.FindByEmail(persona.Email) is null            // 1. HAPPY PATH: No existe nadie con ese email en la BD
            ? Result.Success<Persona, DomainError>(persona)      // 2. HAPPY PATH: Retornamos ÉXITO con la persona
            : Result.Failure<Persona, DomainError>(DomainErrors.AlreadyExists(persona.Email));

    /// <summary>
    /// Verifica que el email para una actualización sea único (ignorando a la propia persona que se edita).
    /// </summary>
    /// <param name="id">ID de la persona actual.</param>
    /// <param name="persona">Nuevos datos con el email.</param>
    /// <returns>Éxito si el email es válido para esta persona.</returns>
    private Result<Persona, DomainError> CheckEmailIsUniqueForUpdate(int id, Persona persona)
    {
        var existente = repository.FindByEmail(persona.Email);
        return (existente is null || existente.Id == id)         // 1. HAPPY PATH: El email es nuevo o pertenece a la misma persona
            ? Result.Success<Persona, DomainError>(persona)      // 2. HAPPY PATH: Validamos la actualización como ÉXITO
            : Result.Failure<Persona, DomainError>(DomainErrors.AlreadyExists(persona.Email));
    }

    /// <summary>
    /// Envía la persona al repositorio para su creación persistente.
    /// </summary>
    private Persona SaveInRepository(Persona persona) => repository.Create(persona)!;   // HAPPY PATH: Persistencia en BD completada
    
    /// <summary>
    /// Envía la persona al repositorio para su actualización persistente.
    /// </summary>
    private Persona UpdateInRepository(int id, Persona persona) => repository.Update(id, persona)!; // HAPPY PATH: Cambio persistido

    /// <summary>
    /// Centraliza el registro de errores en el log del sistema basándose en el tipo de DomainError.
    /// </summary>
    /// <param name="error">Error de dominio detectado.</param>
    private void LogError(DomainError error)
    {
        // VÍA DE ERROR: Centralización del logging según el tipo de fallo de dominio
        switch (error) {
            case DomainError.Validation v: _logger.Error("Fallo Validación: {E}", string.Join(", ", v.Errors)); break;
            case DomainError.AlreadyExists e: _logger.Error("Fallo Unicidad: {Email} ya existe", e.Email); break;
            case DomainError.NotFound n: _logger.Warning("Fallo Existencia: Id {Id} no hallado", n.Id); break;
        }
    }
}
