using System.Globalization;
using GestionAcademica.Dto;
using GestionAcademica.Entity;
using GestionAcademica.Models;
using GestionAcademica.Models.Academia;
using GestionAcademica.Models.Personas;

namespace GestionAcademica.Mappers.Personas;

public static class PersonaMapper {
    // Queremos las fechas en formato ISO 8601 para asegurar
    // compatibilidad entre sistemas y evitar problemas de zona horaria
    // También usamos la cultura invariante para evitar problemas de formato numérico
    // Con los decimales, por ejemplo, que en algunos países se usa la coma en lugar del punto
    private const string IsoFormat = "s";
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    /// <summary>
    ///     Convierte un objeto de tipo Estudiante a su representación en DTO
    /// </summary>
    /// <param name="dto">El DTO a convertir</param>
    /// <returns>Una instancia de Persona (Estudiante o Docente) basada en el DTO</returns>
    /// <exception cref="ArgumentException">Si el tipo de persona en el DTO no es reconocido</exception>
    public static Persona ToModel(this PersonaDto dto) {
        var createdAt = DateTime.Parse(dto.CreatedAt, InvariantCulture);
        var updatedAt = DateTime.Parse(dto.UpdatedAt, InvariantCulture);
        return dto.Tipo switch {
            "Estudiante" => new Estudiante {
                Id = dto.Id,
                Dni = dto.Dni,
                Nombre = dto.Nombre,
                Apellidos = dto.Apellidos,
                Calificacion = double.TryParse(dto.Calificacion, NumberStyles.Any, InvariantCulture, out var calif)
                    ? calif
                    : 0.0,
                Ciclo = Enum.TryParse(dto.Ciclo, out Ciclo ciclo) ? ciclo : Ciclo.DAW,
                Curso = Enum.TryParse(dto.Curso, out Curso curso) ? curso : Curso.Primero,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                IsDeleted = dto.IsDeleted
            },
            "Docente" => new Docente {
                Id = dto.Id,
                Dni = dto.Dni,
                Nombre = dto.Nombre,
                Apellidos = dto.Apellidos,
                Experiencia = int.TryParse(dto.Experiencia, out var exp) ? exp : 0,
                Especialidad = dto.Especialidad ?? string.Empty,
                Ciclo = Enum.TryParse(dto.Ciclo, out Ciclo ciclo) ? ciclo : Ciclo.DAW,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                IsDeleted = dto.IsDeleted
            },
            _ => throw new ArgumentException($"Tipo de persona desconocido: {dto.Tipo}")
        };
    }

    /// <summary>
    ///     Convierte un objeto de tipo Persona (Estudiante o Docente) a su representación en DTO
    /// </summary>
    /// <param name="persona">La persona a convertir</param>
    /// <returns>Una instancia de PersonaDto basada en el modelo</returns>
    /// <exception cref="ArgumentException">Si el tipo de persona no es reconocido</exception>
    public static PersonaDto ToDto(this Persona persona) {
        return persona switch {
            Estudiante estudiante => new PersonaDto(
                estudiante.Id,
                estudiante.Dni,
                estudiante.Nombre,
                estudiante.Apellidos,
                "Estudiante",
                null,
                null,
                estudiante.Ciclo.ToString(),
                estudiante.Curso.ToString(),
                estudiante.Calificacion.ToString(InvariantCulture),
                estudiante.CreatedAt.ToString(IsoFormat, InvariantCulture),
                estudiante.UpdatedAt.ToString(IsoFormat, InvariantCulture),
                estudiante.IsDeleted
            ),
            Docente docente => new PersonaDto(
                docente.Id,
                docente.Dni,
                docente.Nombre,
                docente.Apellidos,
                "Docente",
                docente.Experiencia.ToString(),
                docente.Especialidad,
                docente.Ciclo.ToString(),
                null,
                null,
                docente.CreatedAt.ToString(IsoFormat, InvariantCulture),
                docente.UpdatedAt.ToString(IsoFormat, InvariantCulture),
                docente.IsDeleted
            ),
            _ => throw new ArgumentException($"Tipo de persona desconocido: {persona.GetType().Name}")
        };
    }

    /// <summary>
    /// Convierte una entidad de base de datos a modelo de dominio.
    /// </summary>
    public static Persona? ToModel(this PersonaEntity? entity)
    {
        if (entity == null) return null;

        return entity.Tipo switch
        {
            "Estudiante" => new Estudiante
            {
                Id = entity.Id,
                Dni = entity.Dni,
                Nombre = entity.Nombre,
                Apellidos = entity.Apellidos,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Calificacion = entity.Calificacion ?? 0,
                Ciclo = (Ciclo)(entity.Ciclo ?? 0),
                Curso = (Curso)(entity.Curso ?? 0)
            },
            "Docente" => new Docente
            {
                Id = entity.Id,
                Dni = entity.Dni,
                Nombre = entity.Nombre,
                Apellidos = entity.Apellidos,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                Experiencia = entity.Experiencia ?? 0,
                Especialidad = entity.Especialidad ?? string.Empty,
                Ciclo = (Ciclo)(entity.Ciclo ?? 0)
            },
            _ => throw new ArgumentException($"Tipo de persona desconocido: {entity.Tipo}")
        };
    }

    /// <summary>
    /// Convierte una lista de entidades a modelos de dominio.
    /// </summary>
    public static IEnumerable<Persona> ToModel(this IEnumerable<PersonaEntity> entities)
    {
        return entities.Select(ToModel).OfType<Persona>();
    }

    /// <summary>
    /// Convierte modelo de dominio a entidad de base de datos.
    /// </summary>
    public static PersonaEntity ToEntity(this Persona model)
    {
        return model switch
        {
            Estudiante e => new PersonaEntity
            {
                Id = e.Id,
                Dni = e.Dni,
                Nombre = e.Nombre,
                Apellidos = e.Apellidos,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                IsDeleted = e.IsDeleted,
                Tipo = "Estudiante",
                Calificacion = e.Calificacion,
                Ciclo = (int)e.Ciclo,
                Curso = (int)e.Curso
            },
            Docente d => new PersonaEntity
            {
                Id = d.Id,
                Dni = d.Dni,
                Nombre = d.Nombre,
                Apellidos = d.Apellidos,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                IsDeleted = d.IsDeleted,
                Tipo = "Docente",
                Experiencia = d.Experiencia,
                Especialidad = d.Especialidad,
                Ciclo = (int)d.Ciclo
            },
            _ => throw new ArgumentException($"Tipo de persona desconocido: {model.GetType().Name}")
        };
    }
}