using CSharpFunctionalExtensions;
using PersonaService.Models;
using PersonaService.Errors;

namespace PersonaService.Services;

/// <summary>
/// Interfaz para el servicio de personas.
/// </summary>
public interface IPersonaService
{
    IEnumerable<Persona> GetAll();
    Result<Persona, DomainError> GetById(int id);
    Result<Persona, DomainError> Create(Persona persona);
    Result<Persona, DomainError> Update(int id, Persona persona);
    Result<Persona, DomainError> Delete(int id, bool logical = true);
}
