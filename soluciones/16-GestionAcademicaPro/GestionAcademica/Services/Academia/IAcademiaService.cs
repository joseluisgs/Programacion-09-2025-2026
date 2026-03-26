using CSharpFunctionalExtensions;
using GestionAcademica.Enums;
using GestionAcademica.Errors.Common;
using GestionAcademica.Models;
using GestionAcademica.Models.Academia;
using GestionAcademica.Models.Informes;
using GestionAcademica.Models.Personas;

namespace GestionAcademica.Services;

public interface IAcademiaService
{
    int TotalPersonas { get; }
    IEnumerable<Persona> GetAll();
    IEnumerable<Persona> GetAllOrderBy(TipoOrdenamiento orden = TipoOrdenamiento.Dni, Predicate<Persona>? filtro = null);
    IEnumerable<Estudiante> GetEstudiantesOrderBy(TipoOrdenamiento ordenamiento = TipoOrdenamiento.Dni);
    IEnumerable<Docente> GetDocentesOrderBy(TipoOrdenamiento ordenamiento = TipoOrdenamiento.Dni);

    Result<Persona, DomainError> GetById(int id);
    Result<Persona, DomainError> GetByDni(string dni);
    Result<Persona, DomainError> Save(Persona persona);
    Result<Persona, DomainError> Update(int id, Persona persona);
    Result<Persona, DomainError> Delete(int id);

    InformeEstudiante GenerarInformeEstudiante(Ciclo? ciclo = null, Curso? curso = null);
    InformeDocente GenerarInformeDocente(Ciclo? ciclo = null);

    Result<int, DomainError> ImportarDatos();
    Result<int, DomainError> ExportarDatos();
    Result<string, DomainError> RealizarBackup();
    Result<int, DomainError> RestaurarBackup(string archivoBackup);
    IEnumerable<string> ListarBackups();

    Result<string, DomainError> GenerarInformeEstudiantesHtml();
    Result<string, DomainError> GenerarInformeDocentesHtml();
    Result<string, DomainError> GenerarListadoPersonasHtml();
}
