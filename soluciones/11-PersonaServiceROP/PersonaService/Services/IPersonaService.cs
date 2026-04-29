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
    Result<Persona, PersonaError> GetById(int id);
    Result<Persona, PersonaError> Create(Persona persona);
    Result<Persona, PersonaError> Update(int id, Persona persona);
    Result<Persona, PersonaError> Delete(int id, bool logical = true);
}
