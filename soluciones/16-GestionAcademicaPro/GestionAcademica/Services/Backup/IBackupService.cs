using CSharpFunctionalExtensions;
using GestionAcademica.Errors.Backup;
using GestionAcademica.Errors.Common;
using GestionAcademica.Models.Personas;

namespace GestionAcademica.Services;

public interface IBackupService
{
    Result<string, DomainError> RealizarBackup(IEnumerable<Persona> personas);
    Result<IEnumerable<Persona>, DomainError> RestaurarBackup(string archivoBackup);
    IEnumerable<string> ListarBackups();
}
