using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using CSharpFunctionalExtensions;
using GestionAcademica.Cache;
using GestionAcademica.Config;
using GestionAcademica.Enums;
using GestionAcademica.Infrastructure;
using GestionAcademica.Models;
using GestionAcademica.Models.Academia;
using GestionAcademica.Models.Personas;
using GestionAcademica.Repositories.Personas.Base;
using GestionAcademica.Services;
using GestionAcademica.Services.Report;
using Serilog;
using static System.Console;

// ====================================================================
// GESTIÓN ACADÉMICA - CONFIGURACIÓN INICIAL
// ====================================================================

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger = loggerConfiguration;

Title = "🎓 Sistema de Gestión Académica - DAW";
OutputEncoding = Encoding.UTF8;

Main();

Log.CloseAndFlush();
WriteLine("\n⌨️  Presiona una tecla para salir...");
ReadKey();
return;

// --------------------------------------------------------------------
// FLUJO PRINCIPAL
// --------------------------------------------------------------------
void Main()
{

    Console.WriteLine("=== Gestión Académica con Inyección de Dependencias ===");
    Console.WriteLine();

    // Construimos el ServiceProvider a través del contenedor de dependencias
    var serviceProvider = DependenciesProvider.BuildServiceProvider();
    // Creamos un scope para gestionar la vida de los servicios
    using var scope = serviceProvider.CreateScope();

    // Obtenemos el servicio principal para la gestión académica
    var service = scope.ServiceProvider.GetRequiredService<IAcademiaService>();


    OpcionMenu opcion;
    const string RegexOpcionMenu = @"^([0-9]|1[0-9]|20)$";

    WriteLine("🚀 SISTEMA DE GESTIÓN ACADÉMICA (ESTILO DAW)");
    WriteLine(new string('━', 45));

    do
    {
        MostrarMenu();

        var opcionStr = LeerCadenaValidada("👉 Seleccione una opción: ", RegexOpcionMenu, "Opción no válida (0-20).");
        var opcionValue = int.Parse(opcionStr);
        opcion = (OpcionMenu)opcionValue;

        switch (opcion)
        {
            case OpcionMenu.ListarTodas: ListarTodas(service); break;
            case OpcionMenu.BuscarDni: BuscarPorDniGeneral(service); break;
            case OpcionMenu.BuscarId: BuscarPorIdGeneral(service); break;
            case OpcionMenu.ListarTodasHtml: ListarTodasHtml(service); break;
            case OpcionMenu.ListarEstudiantes: ListarEstudiantes(service); break;
            case OpcionMenu.AnadirEstudiante: AnadirNuevoEstudiante(service); break;
            case OpcionMenu.ActualizarEstudiante: ActualizarEstudiante(service); break;
            case OpcionMenu.EliminarEstudiante: EliminarEstudiante(service); break;
            case OpcionMenu.InformeEstudiantes: MostrarInformeEstudiantes(service); break;
            case OpcionMenu.InformeEstudiantesHtml: InformeEstudiantesHtml(service); break;
            case OpcionMenu.ListarDocentes: ListarDocentes(service); break;
            case OpcionMenu.AnadirDocente: AnadirNuevoDocente(service); break;
            case OpcionMenu.ActualizarDocente: ActualizarDocente(service); break;
            case OpcionMenu.EliminarDocente: EliminarDocente(service); break;
            case OpcionMenu.InformeDocentes: MostrarInformeDocentes(service); break;
            case OpcionMenu.InformeDocentesHtml: InformeDocentesHtml(service); break;
            case OpcionMenu.ImportarDatos: ImportarDatos(service); break;
            case OpcionMenu.ExportarDatos: ExportarDatos(service); break;
            case OpcionMenu.RealizarBackup: RealizarBackup(service); break;
            case OpcionMenu.RestaurarBackup: RestaurarBackup(service); break;
            case OpcionMenu.Salir: WriteLine("\n👋 Cerrando el sistema. ¡Hasta pronto!"); break;
        }

        if (opcion != OpcionMenu.Salir)
        {
            WriteLine("\n⌨️  Presione una tecla para continuar...");
            ReadKey();
        }
    } while (opcion != OpcionMenu.Salir);
}

void MostrarMenu()
{
    WriteLine("\n📋 --- 1. OPERACIONES GENERALES ---");
    WriteLine(new string('─', 45));
    WriteLine($"  {(int)OpcionMenu.ListarTodas}. 👥 Listar todo el personal");
    WriteLine($"  {(int)OpcionMenu.BuscarDni}. 🔍 Buscar persona por DNI");
    WriteLine($"  {(int)OpcionMenu.BuscarId}. 🆔 Buscar persona por ID");
    WriteLine($"  {(int)OpcionMenu.ListarTodasHtml}. 🖨️ Listar todo (HTML)");

    WriteLine("\n🎓 --- 2. GESTIÓN DE ESTUDIANTES ---");
    WriteLine(new string('─', 45));
    WriteLine($"  {(int)OpcionMenu.ListarEstudiantes}. 📜 Listar Estudiantes");
    WriteLine($"  {(int)OpcionMenu.AnadirEstudiante}. ➕ Añadir Estudiante");
    WriteLine($"  {(int)OpcionMenu.ActualizarEstudiante}. 📝 Actualizar Estudiante");
    WriteLine($"  {(int)OpcionMenu.EliminarEstudiante}. 🗑️  Eliminar Estudiante");
    WriteLine($"  {(int)OpcionMenu.InformeEstudiantes}. 📊 Informe de Rendimiento");
    WriteLine($"  {(int)OpcionMenu.InformeEstudiantesHtml}. 🖨️ Informe Rendimiento (HTML)");

    WriteLine("\n👨‍🏫 --- 3. GESTIÓN DE DOCENTES ---");
    WriteLine(new string('─', 45));
    WriteLine($"  {(int)OpcionMenu.ListarDocentes}. 📜 Listar Docentes");
    WriteLine($"  {(int)OpcionMenu.AnadirDocente}. ➕ Añadir Docente");
    WriteLine($"  {(int)OpcionMenu.ActualizarDocente}. 📝 Actualizar Docente");
    WriteLine($"  {(int)OpcionMenu.EliminarDocente}. 🗑️  Eliminar Docente");
    WriteLine($"  {(int)OpcionMenu.InformeDocentes}. 📈 Informe de Experiencia");
    WriteLine($"  {(int)OpcionMenu.InformeDocentesHtml}. 🖨️ Informe Experiencia (HTML)");

    WriteLine("\n💾 --- 4. IMPORTAR/EXPORTAR DATOS ---");
    WriteLine(new string('─', 45));
    WriteLine($"  {(int)OpcionMenu.ImportarDatos}. 📥 Importar desde Fichero");
    WriteLine($"  {(int)OpcionMenu.ExportarDatos}. 📤 Exportar a Fichero");

    WriteLine("\n💿 --- 5. COPIAS DE SEGURIDAD ---");
    WriteLine(new string('─', 45));
    WriteLine($"  {(int)OpcionMenu.RealizarBackup}. 💾 Crear Backup");
    WriteLine($"  {(int)OpcionMenu.RestaurarBackup}. ♻️  Restaurar Backup");

    WriteLine("\n🚪 --- 0. SALIR ---");
    WriteLine(new string('━', 45));
}

// ====================================================================
// MÉTODOS DE OPERACIÓN
// ====================================================================

void ListarTodas(IAcademiaService service)
{
    WriteLine("\n👥 --- LISTADO INTEGRAL DEL PERSONAL ---");
    WriteLine("⚙️  Criterios: 1.ID, 2.DNI, 3.Apellidos, 4.Nombre, 5.Ciclo");
    var op = LeerCadenaValidada("🎯 Seleccione criterio: ", "^[1-5]$", "Elija entre 1 y 5.");

    var criterio = op switch
    {
        "1" => TipoOrdenamiento.Id,
        "2" => TipoOrdenamiento.Dni,
        "3" => TipoOrdenamiento.Apellidos,
        "4" => TipoOrdenamiento.Nombre,
        _ => TipoOrdenamiento.Ciclo
    };

    var lista = service.GetAllOrderBy(criterio);
    ImprimirTablaPersonas(lista);
}

void BuscarPorDniGeneral(IAcademiaService service)
{
    WriteLine("\n🔍 --- BÚSQUEDA POR DNI ---");
    var dni = LeerDniValidado();
    service.GetByDni(dni).Match(
        onSuccess: p => ImprimirFichaPersona(p),
        onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
    );
}

void BuscarPorIdGeneral(IAcademiaService service)
{
    WriteLine("\n🆔 --- BÚSQUEDA POR ID ---");
    var idStr = LeerCadenaValidada("Introduzca ID: ", @"^\d+$", "Debe ser un número entero.");
    service.GetById(int.Parse(idStr)).Match(
        onSuccess: p => ImprimirFichaPersona(p),
        onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
    );
}

void ListarEstudiantes(IAcademiaService service)
{
    WriteLine("\n🎓 --- LISTADO DE ESTUDIANTES ---");
    WriteLine("⚙️  Criterios: 1.ID, 2.DNI, 3.Apellidos, 4.Nombre, 5.Nota, 6.Curso, 7.Ciclo");
    var op = LeerCadenaValidada("🎯 Seleccione criterio: ", "^[1-7]$", "Elija entre 1 y 7.");

    var criterio = op switch
    {
        "1" => TipoOrdenamiento.Id,
        "2" => TipoOrdenamiento.Dni,
        "3" => TipoOrdenamiento.Apellidos,
        "4" => TipoOrdenamiento.Nombre,
        "5" => TipoOrdenamiento.Nota,
        "6" => TipoOrdenamiento.Curso,
        _ => TipoOrdenamiento.Ciclo
    };

    // El servicio se encarga de aplicar este filtro ANTES de ordenar.
    var lista = service.GetEstudiantesOrderBy(criterio);
    ImprimirTablaEstudiantes(lista);
}

void AnadirNuevoEstudiante(IAcademiaService service)
{
    WriteLine("\n➕ --- ALTA DE NUEVO ESTUDIANTE ---");
    WriteLine("  0. ⬅️  VOLVER");

    if (!PedirConfirmacion("¿Desea dar de alta un nuevo estudiante?"))
    {
        WriteLine("👋 Operación cancelada.");
        return;
    }

    var dni = LeerDniValidado();
    var nom = LeerCadenaValidada("👤 Nombre: ", @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]{2,30}$", "Mínimo 2 car.");
    var ape = LeerCadenaValidada("👤 Apellidos: ", @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]{2,50}$", "Mínimo 2 car.");
    var nota = LeerNotaValida();
    var ciclo = LeerCiclo();
    var curso = LeerCurso();

    var temp = new Estudiante
    { Dni = dni, Nombre = nom, Apellidos = ape, Calificacion = nota, Ciclo = ciclo, Curso = curso };
    WriteLine("\n👀 REVISE LOS DATOS:");
    ImprimirFichaPersona(temp);

    if (PedirConfirmacion("¿Confirmar alta?"))
        service.Save(temp).Match(
            onSuccess: creado =>
            {
                WriteLine("✅ Guardado con éxito.");
                ImprimirFichaPersona(creado);
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
}

void ActualizarEstudiante(IAcademiaService service)
{
    WriteLine("\n📝 --- ACTUALIZACIÓN DE ESTUDIANTE ---");
    WriteLine("  0. ⬅️  VOLVER");

    if (!PedirConfirmacion("¿Desea actualizar un estudiante?"))
    {
        WriteLine("👋 Operación cancelada.");
        return;
    }

    var dni = LeerDniValidado();
    var pResult = service.GetByDni(dni);
    if (pResult.IsFailure)
    {
        WriteLine($"❌ ERROR: {pResult.Error.Message}");
        return;
    }
    var p = pResult.Value;
    if (p is not Estudiante est)
    {
        WriteLine("❌ ERROR: No es un Estudiante.");
        return;
    }

    ImprimirFichaPersona(est);
    var nNom = LeerCadenaValidada($"👤 Nombre [{est.Nombre}] (Enter mant.): ", @"^([a-zA-ZñÑáéíóúÁÉÍÓÚ\s]{2,30})?$",
        "Error.");
    var nApe = LeerCadenaValidada($"👤 Apellidos [{est.Apellidos}] (Enter mant.): ",
        @"^([a-zA-ZñÑáéíóúÁÉÍÓÚ\s]{2,50})?$", "Error.");
    var nota = PedirConfirmacion("¿Cambiar nota?") ? LeerNotaValida() : est.Calificacion;
    var ciclo = PedirConfirmacion("¿Cambiar ciclo?") ? LeerCiclo() : est.Ciclo;
    var curso = PedirConfirmacion("¿Cambiar curso?") ? LeerCurso() : est.Curso;

    var act = est with
    {
        Nombre = string.IsNullOrWhiteSpace(nNom) ? est.Nombre : nNom,
        Apellidos = string.IsNullOrWhiteSpace(nApe) ? est.Apellidos : nApe,
        Calificacion = nota,
        Ciclo = ciclo,
        Curso = curso
    };

    WriteLine("\n👀 REVISE CAMBIOS:");
    ImprimirFichaPersona(act);
    if (PedirConfirmacion("¿Actualizar?"))
    {
        service.Update(est.Id, act).Match(
            onSuccess: actualizado =>
            {
                WriteLine("✅ Actualizado.");
                ImprimirFichaPersona(actualizado);
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
    }
}

void EliminarEstudiante(IAcademiaService service)
{
    WriteLine("\n🗑️  --- ELIMINACIÓN DE ESTUDIANTE ---");
    var dni = LeerDniValidado();
    var pResult = service.GetByDni(dni);
    if (pResult.IsFailure)
    {
        WriteLine($"❌ ERROR: {pResult.Error.Message}");
        return;
    }
    var p = pResult.Value;
    if (p is not Estudiante)
    {
        WriteLine("❌ ERROR: No es un Estudiante.");
        return;
    }

    ImprimirFichaPersona(p);
    if (PedirConfirmacion($"¿Eliminar a {p.NombreCompleto}?"))
    {
        service.Delete(p.Id).Match(
            onSuccess: eliminado =>
            {
                WriteLine("✅ Borrado físicamente.");
                ImprimirFichaPersona(eliminado);
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
    }
}

void MostrarInformeEstudiantes(IAcademiaService service)
{
    WriteLine("\n📊 --- INFORME DE RENDIMIENTO ACADÉMICO ---");
    WriteLine("⚙️  Alcance: 1.Global, 2.Por Ciclo, 3.Por Curso, 4.Clase Específica");
    var alc = LeerCadenaValidada("🎯 Seleccione alcance: ", "^[1-4]$", "Elija entre 1 y 4.");

    Ciclo? fCiclo = null;
    Curso? fCurso = null;

    switch (alc)
    {
        case "2": fCiclo = LeerCiclo(); break;
        case "3": fCurso = LeerCurso(); break;
        case "4":
            fCiclo = LeerCiclo();
            fCurso = LeerCurso();
            break;
    }

    var inf = service.GenerarInformeEstudiante(fCiclo, fCurso);
    var desc = alc switch
    {
        "2" => $"Ciclo {fCiclo}",
        "3" => $"Curso {fCurso}",
        "4" => $"{fCurso}º {fCiclo}",
        _ => "Global"
    };

    WriteLine(new string('─', 65));
    WriteLine($"📍 ALCANCE: {desc}");
    WriteLine(
        $"👨‍🎓 Estudiantes: {inf.TotalEstudiantes} | 📈 Media: {inf.NotaMedia.ToString("F2", AppConfig.Locale)}");
    WriteLine(
        $"✅ Aprobados: {inf.Aprobados} ({inf.PorcentajeAprobados.ToString("F2", AppConfig.Locale)}%)");
    WriteLine(new string('─', 65));
    WriteLine("\n🏆 RANKING POR NOTA (DESCENDENTE):");
    ImprimirTablaEstudiantes(inf.PorNota);
}

void ListarDocentes(IAcademiaService service)
{
    WriteLine("\n👨‍🏫 --- LISTADO DE DOCENTES ---");
    WriteLine("⚙️  Criterios: 1.ID, 2.DNI, 3.Apellidos, 4.Nombre, 5.Experiencia, 6.Módulo, 7.Ciclo");
    var op = LeerCadenaValidada("🎯 Seleccione criterio: ", "^[1-7]$", "Elija entre 1 y 7.");

    var criterio = op switch
    {
        "1" => TipoOrdenamiento.Id,
        "2" => TipoOrdenamiento.Dni,
        "3" => TipoOrdenamiento.Apellidos,
        "4" => TipoOrdenamiento.Nombre,
        "5" => TipoOrdenamiento.Experiencia,
        "6" => TipoOrdenamiento.Modulo,
        _ => TipoOrdenamiento.Ciclo
    };

    var lista = service.GetDocentesOrderBy(criterio);
    ImprimirTablaDocentes(lista);
}

void AnadirNuevoDocente(IAcademiaService service)
{
    WriteLine("\n➕ --- ALTA DE NUEVO DOCENTE ---");
    WriteLine("  0. ⬅️  VOLVER");

    if (!PedirConfirmacion("¿Desea dar de alta un nuevo docente?"))
    {
        WriteLine("👋 Operación cancelada.");
        return;
    }

    var dni = LeerDniValidado();
    var nom = LeerCadenaValidada("👤 Nombre: ", @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]{2,30}$", "Mínimo 2 car.");
    var ape = LeerCadenaValidada("👤 Apellidos: ", @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]{2,50}$", "Mínimo 2 car.");
    var expStr = LeerCadenaValidada("⏳ Años de Experiencia: ", @"^\d+$", "Número entero.");
    var exp = int.Parse(expStr);
    var mod = LeerModulo();
    var ciclo = LeerCiclo();

    var temp = new Docente
    { Dni = dni, Nombre = nom, Apellidos = ape, Experiencia = exp, Especialidad = mod, Ciclo = ciclo };
    ImprimirFichaPersona(temp);

    if (PedirConfirmacion("¿Confirmar alta?"))
        service.Save(temp).Match(
            onSuccess: creado =>
            {
                WriteLine("✅ Guardado con éxito.");
                ImprimirFichaPersona(creado);
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
}

void ActualizarDocente(IAcademiaService service)
{
    WriteLine("\n📝 --- ACTUALIZACIÓN DE DOCENTE ---");
    WriteLine("  0. ⬅️  VOLVER");

    if (!PedirConfirmacion("¿Desea actualizar un docente?"))
    {
        WriteLine("👋 Operación cancelada.");
        return;
    }

    var dni = LeerDniValidado();
    var pResult = service.GetByDni(dni);
    if (pResult.IsFailure)
    {
        WriteLine($"❌ ERROR: {pResult.Error.Message}");
        return;
    }
    var p = pResult.Value;
    if (p is not Docente doc)
    {
        WriteLine("❌ ERROR: No es un Docente.");
        return;
    }

    ImprimirFichaPersona(doc);
    var nNom = LeerCadenaValidada($"👤 Nombre [{doc.Nombre}] (Enter mant.): ", @"^([a-zA-ZñÑáéíóúÁÉÍÓÚ\s]{2,30})?$",
        "Error.");
    var nApe = LeerCadenaValidada($"👤 Apellidos [{doc.Apellidos}] (Enter mant.): ",
        @"^([a-zA-ZñÑáéíóúÁÉÍÓÚ\s]{2,50})?$", "Error.");
    var exp = PedirConfirmacion("¿Cambiar exp?")
        ? int.Parse(LeerCadenaValidada("⏳ Exp: ", @"^\d+$", "Num."))
        : doc.Experiencia;
    var mod = PedirConfirmacion("¿Cambiar mod?") ? LeerModulo() : doc.Especialidad;
    var ciclo = PedirConfirmacion("¿Cambiar ciclo?") ? LeerCiclo() : doc.Ciclo;

    var act = doc with
    {
        Nombre = string.IsNullOrWhiteSpace(nNom) ? doc.Nombre : nNom,
        Apellidos = string.IsNullOrWhiteSpace(nApe) ? doc.Apellidos : nApe,
        Experiencia = exp,
        Especialidad = mod,
        Ciclo = ciclo
    };

    ImprimirFichaPersona(act);
    if (PedirConfirmacion("¿Actualizar?"))
    {
        service.Update(doc.Id, act).Match(
            onSuccess: actualizado =>
            {
                WriteLine("✅ Actualizado.");
                ImprimirFichaPersona(actualizado);
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
    }
}

void EliminarDocente(IAcademiaService service)
{
    WriteLine("\n🗑️  --- ELIMINACIÓN DE DOCENTE ---");
    var dni = LeerDniValidado();
    var pResult = service.GetByDni(dni);
    if (pResult.IsFailure)
    {
        WriteLine($"❌ ERROR: {pResult.Error.Message}");
        return;
    }
    var p = pResult.Value;
    if (p is not Docente)
    {
        WriteLine("❌ ERROR: No es un Docente.");
        return;
    }

    ImprimirFichaPersona(p);
    if (PedirConfirmacion($"¿Eliminar a {p.NombreCompleto}?"))
    {
        service.Delete(p.Id).Match(
            onSuccess: eliminado =>
            {
                WriteLine("✅ Borrado.");
                ImprimirFichaPersona(eliminado);
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
    }
}

void MostrarInformeDocentes(IAcademiaService service)
{
    WriteLine("\n📈 --- INFORME DE CUADRO DOCENTE ---");
    WriteLine("⚙️  Alcance: 1.Global, 2.Por Ciclo");
    var alc = LeerCadenaValidada("🎯 Seleccione alcance: ", "^[1-2]$", "Elija entre 1 y 2.");

    Ciclo? fCiclo = null;
    if (alc == "2") fCiclo = LeerCiclo();

    var inf = service.GenerarInformeDocente(fCiclo);
    var desc = alc == "2" ? $"Ciclo {fCiclo}" : "Global";

    WriteLine(new string('─', 65));
    WriteLine($"📍 ALCANCE: {desc}");
    WriteLine(
        $"👨‍🏫 Docentes: {inf.TotalDocentes} | ⏳ Media: {inf.ExperienciaMedia.ToString("F2", AppConfig.Locale)} años");
    WriteLine(new string('─', 65));
    WriteLine("\n🏆 RANKING POR EXPERIENCIA (DESCENDENTE):");
    ImprimirTablaDocentes(inf.PorExperiencia);
}

void ListarTodasHtml(IAcademiaService service)
{
    WriteLine("\n🌐 --- GENERANDO LISTADO HTML ---");

    service.GenerarListadoPersonasHtml()
        .Match(
            onSuccess: filePath =>
            {
                WriteLine($"✅ Informe guardado: {Path.GetFileName(filePath)}");
                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                WriteLine("✅ Informe abierto en el navegador");
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
}

void InformeEstudiantesHtml(IAcademiaService service)
{
    WriteLine("\n🌐 --- GENERANDO INFORME HTML DE ESTUDIANTES ---");

    service.GenerarInformeEstudiantesHtml()
        .Match(
            onSuccess: filePath =>
            {
                WriteLine($"✅ Informe guardado: {Path.GetFileName(filePath)}");
                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                WriteLine("✅ Informe abierto en el navegador");
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
}

void InformeDocentesHtml(IAcademiaService service)
{
    WriteLine("\n🌐 --- GENERANDO INFORME HTML DE DOCENTES ---");

    service.GenerarInformeDocentesHtml()
        .Match(
            onSuccess: filePath =>
            {
                WriteLine($"✅ Informe guardado: {Path.GetFileName(filePath)}");
                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                WriteLine("✅ Informe abierto en el navegador");
            },
            onFailure: error => WriteLine($"❌ ERROR: {error.Message}")
        );
}

void ImportarDatos(IAcademiaService service)
{
    WriteLine("\n📥 --- IMPORTAR DATOS DESDE FICHERO ---");
    if (!PedirConfirmacion(
            $"Desea importar los datos desde el fiche o: {AppConfig.AcademiaFile}\nEsta acción puede sobrescribir datos existentes. ¿Desea continuar?"))
    {
        WriteLine("👋 Operación cancelada.");
        return;
    }

    service.ImportarDatos().Match(
        onSuccess: importados => WriteLine($"✅ Importados {importados} registros."),
        onFailure: error => WriteLine($"☠️ ERROR AL IMPORTAR: {error.Message}")
    );
}

void ExportarDatos(IAcademiaService service)
{
    WriteLine("\n📤 --- EXPORTAR DATOS A FICHERO ---");
    service.ExportarDatos().Match(
        onSuccess: exportados => WriteLine($"✅ Exportados {exportados} registros."),
        onFailure: error => WriteLine($"☠️ ERROR AL EXPORTAR: {error.Message}")
    );
}

void RealizarBackup(IAcademiaService service)
{
    WriteLine("\n💾 --- CREAR COPIA DE SEGURIDAD ---");
    if (!PedirConfirmacion("¿Desea crear una copia de seguridad de todos los datos?"))
    {
        WriteLine("👋 Operación cancelada.");
        return;
    }

    service.RealizarBackup().Match(
        onSuccess: ruta =>
        {
            WriteLine("✅ Backup creado correctamente.");
            WriteLine($"📁 Archivo: {ruta}");
        },
        onFailure: error => WriteLine($"☠️ ERROR AL CREAR BACKUP: {error.Message}")
    );
}

void RestaurarBackup(IAcademiaService service)
{
    WriteLine("\n♻️ --- RESTAURAR COPIA DE SEGURIDAD ---");

    var backups = service.ListarBackups().ToList();
    if (backups.Count == 0)
    {
        WriteLine("❌ No hay copias de seguridad disponibles.");
        return;
    }

    WriteLine("\n📋 COPIAS DE SEGURIDAD DISPONIBLES:");
    WriteLine("  0. ⬅️  VOLVER");
    for (var i = 0; i < backups.Count; i++)
    {
        var file = new FileInfo(backups[i]);
        var size = file.Length < 1024
            ? $"{file.Length} B"
            : $"{Math.Round(file.Length / 1024.0, 1)} KB";
        WriteLine($"  {i + 1}. 📄 {file.Name} ({size}) - {file.CreationTime:g}");
    }

    var opcion = LeerCadenaValidada("🎯 Seleccione archivo (0 para volver): ", @"^\d+$", "Número inválido.");
    var indice = int.Parse(opcion) - 1;

    if (indice < 0)
    {
        WriteLine("👋 Operación cancelada.");
        return;
    }

    if (indice >= backups.Count)
    {
        WriteLine("❌ Selección inválida.");
        return;
    }

    var archivoSeleccionado = backups[indice];
    WriteLine($"\n📄 Seleccionado: {archivoSeleccionado}");

    if (!PedirConfirmacion("⚠️  Esta acción eliminará todos los datos actuales. ¿Continuar?"))
    {
        WriteLine("👋 Operación cancelada.");
        return;
    }

    service.RestaurarBackup(archivoSeleccionado).Match(
        onSuccess: restaurados => WriteLine($"✅ Restauración completada. Total registros: {restaurados}"),
        onFailure: error => WriteLine($"☠️ ERROR AL RESTAURAR: {error.Message}")
    );
}

// ====================================================================
// RENDERIZADO UNIFICADO
// ====================================================================

void ImprimirTablaPersonas(IEnumerable<Persona> lista)
{
    var line = new string('━', 105);
    WriteLine(line);
    WriteLine(
        $"{"🆔 ID",-5} | {"🆔 DNI",-10} | {"👤 Nombre Completo",-35} | {"📂 Ciclo",-8} | {"🎭 Tipo",-12}");
    WriteLine(line.Replace('━', '─'));
    foreach (var p in lista)
    {
        var (tipo, ciclo) = p switch
        {
            Estudiante e => ("🎓 Estudiante", e.Ciclo.ToString()),
            Docente d => ("👨‍🏫 Docente", d.Ciclo.ToString()),
            _ => ("❓", "N/A")
        };
        WriteLine($" {p.Id,-5} | {p.Dni,-10} | {p.NombreCompleto,-35} | {ciclo,-8} | {tipo}");
    }

    WriteLine(line);
}

void ImprimirTablaEstudiantes(IEnumerable<Estudiante> lista)
{
    var line = new string('━', 125);
    WriteLine(line);
    WriteLine(
        $"{"🆔 ID",-5} | {"🆔 DNI",-10} | {"👤 Nombre Completo",-35} | {"📂 Ciclo",-10} | {"📅 Cur",-6} | {"📝 Nota",-7} | {"🎖️  Evaluación"}");
    WriteLine(line.Replace('━', '─'));
    foreach (var e in lista)
        WriteLine(
            $" {e.Id,-5} | {e.Dni,-10} | {e.NombreCompleto,-35} | {e.Ciclo,-10} | {(int)e.Curso,-6} | {e.Calificacion.ToString("F2", AppConfig.Locale),-7} | {e.CalificacionCualitativa}");
    WriteLine(line);
}

void ImprimirTablaDocentes(IEnumerable<Docente> lista)
{
    var line = new string('━', 115);
    WriteLine(line);
    WriteLine(
        $"{"🆔 ID",-5} | {"🆔 DNI",-10} | {"👤 Nombre Completo",-35} | {"📂 Ciclo",-8} | {"⏳ Exp",-6} | {"📚 Especialidad"}");
    WriteLine(line.Replace('━', '─'));
    foreach (var d in lista)
        WriteLine(
            $" {d.Id,-5} | {d.Dni,-10} | {d.NombreCompleto,-35} | {d.Ciclo,-8} | {d.Experiencia,-6} | {d.Especialidad}");
    WriteLine(line);
}

void ImprimirFichaPersona(Persona p)
{
    var line = new string('━', 65);
    WriteLine();
    WriteLine(line);
    WriteLine("  🆔 IDENTIDAD ACADÉMICA");
    WriteLine(line.Replace('━', '─'));
    WriteLine($"  🆔 ID:          {(p.Id == 0 ? "PENDIENTE" : p.Id)}");
    WriteLine($"  🆔 DNI:         {p.Dni}");
    WriteLine($"  👤 APELLIDOS:   {p.Apellidos}");
    WriteLine($"  👤 NOMBRE:      {p.Nombre}");

    if (p is Estudiante e)
    {
        WriteLine("  🎭 TIPO:        🎓 ESTUDIANTE");
        WriteLine($"  📝 NOTA:        {e.Calificacion.ToString("F2", AppConfig.Locale)}");
        WriteLine($"  🎖️  EVAL:        {e.CalificacionCualitativa}");
        WriteLine($"  📂 CICLO:       {e.Ciclo}");
        WriteLine($"  📅 CURSO:       {e.Curso}");
    }
    else if (p is Docente d)
    {
        WriteLine("  🎭 TIPO:        👨‍🏫 DOCENTE");
        WriteLine($"  ⏳ EXP:         {d.Experiencia} años");
        WriteLine($"  📚 MOD:         {d.Especialidad}");
        WriteLine($"  📂 CICLO:       {d.Ciclo}");
    }

    if (p.CreatedAt != default)
    {
        WriteLine(new string('─', 65));
        WriteLine($"  📅 ALTA (LOC):  {p.CreatedAt.ToLocalTime().ToString("g", AppConfig.Locale)}");
        WriteLine($"  🔄 MOD  (LOC):  {p.UpdatedAt.ToLocalTime().ToString("g", AppConfig.Locale)}");
        var estado = p.IsDeleted ? "❌ ELIMINADO" : "✅ ACTIVO";
        WriteLine($"  🚦 ESTADO:      {estado}");
    }

    WriteLine(line);
    WriteLine();
}

// ====================================================================
// APOYO (INPUT)
// ====================================================================

bool ValidarDniCompleto(string d)
{
    if (!Regex.IsMatch(d, @"^(\d{8})([A-Z])$")) return false;
    var n = int.Parse(d.Substring(0, 8));
    return "TRWAGMYFPDXBNJZSQVHLCKE"[n % 23] == d[8];
}

string LeerDniValidado()
{
    while (true)
    {
        Write("🆔 Introduzca DNI: ");
        var d = ReadLine()?.Trim().ToUpper() ?? "";
        if (ValidarDniCompleto(d)) return d;
        WriteLine("❌ ERROR: DNI inválido.");
    }
}

double LeerNotaValidada()
{
    var sep = AppConfig.Locale.NumberFormat.NumberDecimalSeparator;
    while (true)
    {
        Write($"📝 Nota (0-10, use '{sep}'): ");
        var s = ReadLine()?.Trim().Replace(",", ".") ?? "";
        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) && n >= 0 &&
            n <= 10) return n;
        WriteLine("❌ Error: Formato o rango incorrecto.");
    }
}

double LeerNotaValida()
{
    return LeerNotaValidada();
} // Alias por compatibilidad

Ciclo LeerCiclo()
{
    WriteLine("📂 Ciclos Disponibles: 1.DAM, 2.DAW, 3.ASIR");
    return (Ciclo)(int.Parse(LeerCadenaValidada("🎯 Elija Ciclo: ", @"^[1-3]$", "Seleccione entre 1 y 3.")) - 1);
}

Curso LeerCurso()
{
    WriteLine("📅 Cursos Disponibles: 1.Primero, 2.Segundo");
    return (Curso)int.Parse(LeerCadenaValidada("🎯 Elija Curso: ", @"^[1-2]$", "Seleccione 1 o 2."));
}

string LeerModulo()
{
    WriteLine(
        $"📚 Módulos: 1.{Modulos.Programacion}, 2.{Modulos.BasesDatos}, 3.{Modulos.Entornos}, 4.{Modulos.LenguajesMarcas}");
    return LeerCadenaValidada("🎯 Elija Módulo: ", @"^[1-4]$", "Seleccione entre 1 y 4.") switch
    {
        "1" => Modulos.Programacion,
        "2" => Modulos.BasesDatos,
        "3" => Modulos.Entornos,
        _ => Modulos.LenguajesMarcas
    };
}

string LeerCadenaValidada(string prompt, string regex, string error)
{
    while (true)
    {
        Write(prompt);
        var input = ReadLine()?.Trim() ?? "";
        if (Regex.IsMatch(input, regex)) return input;
        WriteLine($"❌ ERROR: {error}");
    }
}

bool PedirConfirmacion(string mensaje)
{
    Write($"\n⚠️  {mensaje} (S para confirmar): ");
    var res = char.ToUpper(ReadKey(false).KeyChar) == 'S';
    WriteLine();
    return res;
}