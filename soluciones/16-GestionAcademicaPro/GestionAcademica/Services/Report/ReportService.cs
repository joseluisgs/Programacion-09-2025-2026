using CSharpFunctionalExtensions;
using GestionAcademica.Config;
using GestionAcademica.Errors.Common;
using GestionAcademica.Errors.Personas;
using GestionAcademica.Models.Academia;
using GestionAcademica.Models.Personas;
using Serilog;

namespace GestionAcademica.Services.Report;

public class ReportService : IReportService
{
    private readonly ILogger _logger = Log.ForContext<ReportService>();

    public Result<string, DomainError> GenerarInformeEstudiantesHtml(IEnumerable<Estudiante> estudiantes)
    {
        _logger.Information("Generando informe HTML de estudiantes");
        
        try
        {
            var lista = estudiantes.ToList();
            var fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            var notaAprobado = AppConfig.NotaAprobado;
            var aprobados = lista.Count(e => e.Calificacion >= notaAprobado);
            var suspensos = lista.Count(e => e.Calificacion < notaAprobado);
            var media = lista.Count > 0 ? lista.Average(e => e.Calificacion) : 0;
            
            var html = $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <title>Informe de Estudiantes</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ font-size: 2.5em; margin-bottom: 10px; }}
        .stats {{ display: flex; justify-content: space-around; padding: 20px; background: #f8f9fa; border-bottom: 3px solid #667eea; }}
        .stat-box {{ text-align: center; padding: 15px; }}
        .stat-number {{ font-size: 2em; font-weight: bold; color: #667eea; }}
        .stat-number.aprobados {{ color: #28a745; }}
        .stat-number.suspensos {{ color: #dc3545; }}
        .stat-label {{ color: #666; margin-top: 5px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background-color: #667eea; color: white; padding: 12px; text-align: left; font-weight: 600; }}
        td {{ padding: 12px; border-bottom: 1px solid #eee; color: #555; }}
        tr:hover {{ background-color: #f5f5f5; }}
        .aprobado {{ color: #28a745; font-weight: bold; }}
        .suspenso {{ color: #dc3545; font-weight: bold; }}
        .nota {{ text-align: center; }}
        .ciclo {{ text-align: center; }}
        .footer {{ background-color: #333; color: white; padding: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>📊 Informe de Estudiantes</h1>
            <p>Fecha: {fecha}</p>
        </div>
        <div class=""stats"">
            <div class=""stat-box"">
                <div class=""stat-number"">{lista.Count}</div>
                <div class=""stat-label"">Total Estudiantes</div>
            </div>
            <div class=""stat-box"">
                <div class=""stat-number aprobados"">{aprobados}</div>
                <div class=""stat-label"">Aprobados</div>
            </div>
            <div class=""stat-box"">
                <div class=""stat-number suspensos"">{suspensos}</div>
                <div class=""stat-label"">Suspensos</div>
            </div>
            <div class=""stat-box"">
                <div class=""stat-number"">{media:F2}</div>
                <div class=""stat-label"">Nota Media</div>
            </div>
        </div>
        <table>
            <thead>
                <tr>
                    <th>DNI</th>
                    <th>Nombre</th>
                    <th>Apellidos</th>
                    <th>Ciclo</th>
                    <th>Curso</th>
                    <th class=""nota"">Nota</th>
                    <th class=""nota"">Estado</th>
                </tr>
            </thead>
            <tbody>";

            foreach (var e in lista.OrderByDescending(x => x.Calificacion))
            {
                var estado = e.Calificacion >= notaAprobado ? "aprobado" : "suspenso";
                var estadoTexto = e.Calificacion >= notaAprobado ? "Aprobado" : "Suspenso";
                html += $@"
                <tr>
                    <td>{e.Dni}</td>
                    <td>{e.Nombre}</td>
                    <td>{e.Apellidos}</td>
                    <td class=""ciclo"">{e.Ciclo}</td>
                    <td class=""ciclo"">{e.Curso}</td>
                    <td class=""nota"">{e.Calificacion:F1}</td>
                    <td class=""{estado}"">{estadoTexto}</td>
                </tr>";
            }

            html += @"
            </tbody>
        </table>
        <div class=""footer"">
            <p>Sistema de Gestión Académica - DAW Academy</p>
        </div>
    </div>
</body>
</html>";

            _logger.Information("Informe HTML de estudiantes generado correctamente");
            return Result.Success<string, DomainError>(html);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al generar informe HTML de estudiantes");
            return Result.Failure<string, DomainError>(
                PersonaErrors.Validation(new[] { $"Error al generar informe: {ex.Message}" }));
        }
    }

    public Result<string, DomainError> GenerarInformeDocentesHtml(IEnumerable<Docente> docentes)
    {
        _logger.Information("Generando informe HTML de docentes");
        
        try
        {
            var lista = docentes.ToList();
            var fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            var media = lista.Count > 0 ? lista.Average(d => d.Experiencia) : 0;
            
            var html = $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <title>Informe de Docentes</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ font-size: 2.5em; margin-bottom: 10px; }}
        .stats {{ display: flex; justify-content: space-around; padding: 20px; background: #f8f9fa; border-bottom: 3px solid #11998e; }}
        .stat-box {{ text-align: center; padding: 15px; }}
        .stat-number {{ font-size: 2em; font-weight: bold; color: #11998e; }}
        .stat-label {{ color: #666; margin-top: 5px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background-color: #11998e; color: white; padding: 12px; text-align: left; font-weight: 600; }}
        td {{ padding: 12px; border-bottom: 1px solid #eee; color: #555; }}
        tr:hover {{ background-color: #f5f5f5; }}
        .experiencia {{ text-align: center; }}
        .ciclo {{ text-align: center; }}
        .footer {{ background-color: #333; color: white; padding: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>👨‍🏫 Informe de Docentes</h1>
            <p>Fecha: {fecha}</p>
        </div>
        <div class=""stats"">
            <div class=""stat-box"">
                <div class=""stat-number"">{lista.Count}</div>
                <div class=""stat-label"">Total Docentes</div>
            </div>
            <div class=""stat-box"">
                <div class=""stat-number"">{media:F1}</div>
                <div class=""stat-label"">Años Experiencia Media</div>
            </div>
        </div>
        <table>
            <thead>
                <tr>
                    <th>DNI</th>
                    <th>Nombre</th>
                    <th>Apellidos</th>
                    <th>Especialidad</th>
                    <th>Ciclo</th>
                    <th class=""experiencia"">Experiencia</th>
                </tr>
            </thead>
            <tbody>";

            foreach (var d in lista.OrderByDescending(x => x.Experiencia))
            {
                html += $@"
                <tr>
                    <td>{d.Dni}</td>
                    <td>{d.Nombre}</td>
                    <td>{d.Apellidos}</td>
                    <td>{d.Especialidad}</td>
                    <td class=""ciclo"">{d.Ciclo}</td>
                    <td class=""experiencia"">{d.Experiencia} años</td>
                </tr>";
            }

            html += @"
            </tbody>
        </table>
        <div class=""footer"">
            <p>Sistema de Gestión Académica - DAW Academy</p>
        </div>
    </div>
</body>
</html>";

            _logger.Information("Informe HTML de docentes generado correctamente");
            return Result.Success<string, DomainError>(html);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al generar informe HTML de docentes");
            return Result.Failure<string, DomainError>(
                PersonaErrors.Validation(new[] { $"Error al generar informe: {ex.Message}" }));
        }
    }

    public Result<string, DomainError> GenerarListadoPersonasHtml(IEnumerable<Persona> personas)
    {
        _logger.Information("Generando listado HTML de personas");
        
        try
        {
            var lista = personas.ToList();
            var fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            var html = $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <title>Listado de Personal</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #ee0979 0%, #ff6a00 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ font-size: 2.5em; margin-bottom: 10px; }}
        .stats {{ padding: 20px; background: #f8f9fa; border-bottom: 3px solid #ee0979; text-align: center; }}
        .stat-number {{ font-size: 2em; font-weight: bold; color: #ee0979; }}
        .stat-label {{ color: #666; margin-top: 5px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background-color: #ee0979; color: white; padding: 12px; text-align: left; font-weight: 600; }}
        td {{ padding: 12px; border-bottom: 1px solid #eee; color: #555; }}
        tr:hover {{ background-color: #f5f5f5; }}
        .estudiante {{ background-color: #e3f2fd; }}
        .docente {{ background-color: #e8f5e9; }}
        .tipo {{ text-align: center; font-weight: bold; }}
        .footer {{ background-color: #333; color: white; padding: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>👥 Listado de Personal</h1>
            <p>Fecha: {fecha}</p>
        </div>
        <div class=""stats"">
            <div class=""stat-number"">{lista.Count}</div>
            <div class=""stat-label"">Total Personas</div>
        </div>
        <table>
            <thead>
                <tr>
                    <th>ID</th>
                    <th>DNI</th>
                    <th>Nombre</th>
                    <th>Apellidos</th>
                    <th class=""tipo"">Tipo</th>
                </tr>
            </thead>
            <tbody>";

            foreach (var p in lista.OrderBy(x => x.Apellidos).ThenBy(x => x.Nombre))
            {
                var claseFila = p is Estudiante ? "estudiante" : "docente";
                var tipoTexto = p is Estudiante ? "🎓 Estudiante" : "👨‍🏫 Docente";
                html += $@"
                <tr class=""{claseFila}"">
                    <td>{p.Id}</td>
                    <td>{p.Dni}</td>
                    <td>{p.Nombre}</td>
                    <td>{p.Apellidos}</td>
                    <td class=""tipo"">{tipoTexto}</td>
                </tr>";
            }

            html += @"
            </tbody>
        </table>
        <div class=""footer"">
            <p>Sistema de Gestión Académica - DAW Academy</p>
        </div>
    </div>
</body>
</html>";

            _logger.Information("Listado HTML de personas generado correctamente");
            return Result.Success<string, DomainError>(html);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al generar listado HTML de personas");
            return Result.Failure<string, DomainError>(
                PersonaErrors.Validation(new[] { $"Error al generar informe: {ex.Message}" }));
        }
    }

    public Result<bool, DomainError> GuardarInforme(string html, string fileName)
    {
        var directory = AppConfig.ReportDirectory;
        _logger.Information("Guardando informe en directorio {Directory}", directory);
        
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, html);
            
            _logger.Information("Informe guardado correctamente en {FilePath}", filePath);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al guardar informe");
            return Result.Failure<bool, DomainError>(
                PersonaErrors.Validation(new[] { $"Error al guardar informe: {ex.Message}" }));
        }
    }
}
