using CSharpFunctionalExtensions;
using GestionAcademica.Errors.Common;
using GestionAcademica.Models.Academia;
using GestionAcademica.Models.Personas;

namespace GestionAcademica.Services.Report;

public interface IReportService
{
    Result<string, DomainError> GenerarInformeEstudiantesHtml(IEnumerable<Estudiante> estudiantes);
    Result<string, DomainError> GenerarInformeDocentesHtml(IEnumerable<Docente> docentes);
    Result<string, DomainError> GenerarListadoPersonasHtml(IEnumerable<Persona> personas);
    Result<bool, DomainError> GuardarInforme(string html, string fileName);
}
