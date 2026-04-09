using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using GestionAcademica.Config;
using GestionAcademica.Enums;
using GestionAcademica.Infrastructure;
using GestionAcademica.Models.Academia;
using GestionAcademica.Models.Personas;
using GestionAcademica.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Spectre.Console;

// ====================================================================
// GESTIÓN ACADÉMICA - CONFIGURACIÓN INICIAL
// ====================================================================

// --------------------------------------------------------------------
// CONFIGURACIÓN DE SERILOG CON NIVELES Y ARCHIVO
// --------------------------------------------------------------------
var logLevel = AppConfig.LogLevel switch {
    "Debug" => LogEventLevel.Debug,
    "Information" => LogEventLevel.Information,
    "Warning" => LogEventLevel.Warning,
    "Error" => LogEventLevel.Error,
    _ => LogEventLevel.Information
};

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        Path.Combine(AppConfig.LogDirectory, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: AppConfig.LogRetainDays,
        outputTemplate: AppConfig.LogOutputTemplate,
        restrictedToMinimumLevel: logLevel)
    .Enrich.WithProperty("Application", "GestionAcademica")
    .CreateLogger();

Log.Logger = loggerConfiguration;

Log.Information("🚀 Sistema iniciado - Logging configurado");
Log.Information("📁 Directorio de logs: {LogDir}", AppConfig.LogDirectory);
Log.Information("📅 Retención de logs: {Days} días", AppConfig.LogRetainDays);

Console.Title = "🎓 Sistema de Gestión Académica - DAW";
Console.OutputEncoding = Encoding.UTF8;


Main();

Log.CloseAndFlush();
AnsiConsole.MarkupLine("\n[yellow]⌨️  Presiona una tecla para salir...[/]");
Console.ReadKey();
return;

// --------------------------------------------------------------------
// FLUJO PRINCIPAL
// --------------------------------------------------------------------
void Main() {
    // ----------------------------------------------------------------
    // INYECCIÓN DE DEPENDENCIAS
    // ----------------------------------------------------------------

    // Construimos el ServiceProvider a través del contenedor de dependencias
    var serviceProvider = DependenciesProvider.BuildServiceProvider();
    // Creamos un scope para gestionar la vida de los servicios
    using var scope = serviceProvider.CreateScope();

    // Obtenemos el servicio principal para la gestión académica
    var service = scope.ServiceProvider.GetRequiredService<IAcademiaService>();


// --------------------------------------------------------------------
// BIENVENIDA CON SECTORES VISUALES (SPECTRE.CONSOLE)
// --------------------------------------------------------------------

// FigletText: Título grande estilizado
    AnsiConsole.Write(new FigletText("Gestión Académica").Color(Color.LightSkyBlue1).Centered());

// Rule: Línea decorativa con texto
    AnsiConsole.Write(new Rule("[lightsteelblue]━━━━━ SISTEMA DE GESTIÓN ACADÉMICA - DAW ━━━━━[/]")
        .RuleStyle(Color.LightSkyBlue1).Centered());

// --------------------------------------------------------------------
// PANEL DE ESTADÍSTICAS DEL SISTEMA (MEJORA 5)
// --------------------------------------------------------------------
// Obtenemos estadísticas del sistema para mostrar en el inicio
    var todos = service.GetAll().ToList();
    var estudiantes = todos.OfType<Estudiante>().ToList();
    var docentes = todos.OfType<Docente>().ToList();

    var panelStats = new Panel($"""
                                [cyan]📊 ESTADÍSTICAS DEL SISTEMA[/]

                                [blue]👨‍🎓 Estudiantes:[/]    [yellow]{estudiantes.Count}[/]
                                [blue]👨‍🏫 Docentes:[/]      [yellow]{docentes.Count}[/]
                                [blue]👥 Total Personal:[/]  [yellow]{todos.Count}[/]

                                [blue]📱 DAM:[/]  [yellow]{estudiantes.Count(e => e.Ciclo == Ciclo.DAM) + docentes.Count(d => d.Ciclo == Ciclo.DAM)}[/]
                                [blue]🌐 DAW:[/]  [yellow]{estudiantes.Count(e => e.Ciclo == Ciclo.DAW) + docentes.Count(d => d.Ciclo == Ciclo.DAW)}[/]
                                [blue]🖥️  ASIR:[/] [yellow]{estudiantes.Count(e => e.Ciclo == Ciclo.ASIR) + docentes.Count(d => d.Ciclo == Ciclo.ASIR)}[/]
                                """)
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.LightSkyBlue1)
        .Header("📈 RESUMEN DE LA ACADEMIA")
        .Padding(new Padding(1, 0, 1, 0));
    AnsiConsole.Write(panelStats);

    // ----------------------------------------------------------------
    // BUCLE PRINCIPAL DEL MENÚ
    // ----------------------------------------------------------------

    OpcionMenu opcion;

    do {
        opcion = MostrarMenuPrincipal();

        // ----------------------------------------------------------------
        // GESTIÓN DE OPCIONES
        // ----------------------------------------------------------------

        switch (opcion) {
            // ---- OPERACIONES GENERALES ----
            case OpcionMenu.ListarTodas: ListarTodas(service); break;
            case OpcionMenu.BuscarDni: BuscarPorDniGeneral(service); break;
            case OpcionMenu.BuscarId: BuscarPorIdGeneral(service); break;
            case OpcionMenu.ListarTodasHtml: ListarTodasHtml(service); break;

            // ---- GESTIÓN DE ESTUDIANTES ----
            case OpcionMenu.ListarEstudiantes: ListarEstudiantes(service); break;
            case OpcionMenu.AnadirEstudiante: AnadirNuevoEstudiante(service); break;
            case OpcionMenu.ActualizarEstudiante: ActualizarEstudiante(service); break;
            case OpcionMenu.EliminarEstudiante: EliminarEstudiante(service); break;
            case OpcionMenu.InformeEstudiantes: MostrarInformeEstudiantes(service); break;
            case OpcionMenu.InformeEstudiantesHtml: InformeEstudiantesHtml(service); break;

            // ---- GESTIÓN DE DOCENTES ----
            case OpcionMenu.ListarDocentes: ListarDocentes(service); break;
            case OpcionMenu.AnadirDocente: AnadirNuevoDocente(service); break;
            case OpcionMenu.ActualizarDocente: ActualizarDocente(service); break;
            case OpcionMenu.EliminarDocente: EliminarDocente(service); break;
            case OpcionMenu.InformeDocentes: MostrarInformeDocentes(service); break;
            case OpcionMenu.InformeDocentesHtml: InformeDocentesHtml(service); break;

            // ---- IMPORTAR/EXPORTAR ----
            case OpcionMenu.ImportarDatos: ImportarDatos(service); break;
            case OpcionMenu.ExportarDatos: ExportarDatos(service); break;

            // ---- COPIAS DE SEGURIDAD ----
            case OpcionMenu.RealizarBackup: RealizarBackup(service); break;
            case OpcionMenu.RestaurarBackup: RestaurarBackup(service); break;

            // ---- SALIR ----
            case OpcionMenu.Salir:
                AnsiConsole.MarkupLine("\n[green]👋 Cerrando el sistema. ¡Hasta pronto![/]");
                break;
        }

        if (opcion != OpcionMenu.Salir) {
            AnsiConsole.MarkupLine("\n[yellow]⌨️  Presione una tecla para continuar...[/]");
            Console.ReadKey();
        }
    } while (opcion != OpcionMenu.Salir);
}

// --------------------------------------------------------------------
// MENÚ PRINCIPAL
// --------------------------------------------------------------------
OpcionMenu MostrarMenuPrincipal() {
    // Rule: Línea decorativa para el menú
    AnsiConsole.Write(new Rule("[lightseagreen]📋 MENÚ PRINCIPAL[/]").RuleStyle(Color.LightSeaGreen));

    // -------------------------------------------------------------------------
    // PRESENTACIÓN DEL MENÚ CON MARKUP ENRIQUECIDO
    // -------------------------------------------------------------------------

    AnsiConsole.MarkupLine("[lightseagreen]━━━━━━━ 1. OPERACIONES GENERALES ━━━━━━━[/]");
    AnsiConsole.MarkupLine("  [yellow]1.[/] 👥 Listar todo el personal");
    AnsiConsole.MarkupLine("  [yellow]2.[/] 🔍 Buscar persona por DNI");
    AnsiConsole.MarkupLine("  [yellow]3.[/] 🆔 Buscar persona por ID");
    AnsiConsole.MarkupLine("  [yellow]4.[/] 🖨️ Listar todo (HTML)");

    AnsiConsole.MarkupLine("\n[lightseagreen]━━━━━━━ 2. GESTIÓN DE ESTUDIANTES ━━━━━━━[/]");
    AnsiConsole.MarkupLine("  [yellow]5.[/] 📜 Listar Estudiantes");
    AnsiConsole.MarkupLine("  [yellow]6.[/] ➕ Añadir Estudiante");
    AnsiConsole.MarkupLine("  [yellow]7.[/] 📝 Actualizar Estudiante");
    AnsiConsole.MarkupLine("  [yellow]8.[/] 🗑️  Eliminar Estudiante");
    AnsiConsole.MarkupLine("  [yellow]9.[/] 📊 Informe de Rendimiento");
    AnsiConsole.MarkupLine(" [yellow]10.[/] 🖨️ Informe Rendimiento (HTML)");

    AnsiConsole.MarkupLine("\n[lightseagreen]━━━━━━━ 3. GESTIÓN DE DOCENTES ━━━━━━━[/]");
    AnsiConsole.MarkupLine(" [yellow]11.[/] 📜 Listar Docentes");
    AnsiConsole.MarkupLine(" [yellow]12.[/] ➕ Añadir Docente");
    AnsiConsole.MarkupLine(" [yellow]13.[/] 📝 Actualizar Docente");
    AnsiConsole.MarkupLine(" [yellow]14.[/] 🗑️  Eliminar Docente");
    AnsiConsole.MarkupLine(" [yellow]15.[/] 📈 Informe de Experiencia");
    AnsiConsole.MarkupLine(" [yellow]16.[/] 🖨️ Informe Experiencia (HTML)");

    AnsiConsole.MarkupLine("\n[lightseagreen]━━━━━━━ 4. IMPORTAR/EXPORTAR DATOS ━━━━━━━[/]");
    AnsiConsole.MarkupLine(" [yellow]17.[/] 📥 Importar desde Fichero");
    AnsiConsole.MarkupLine(" [yellow]18.[/] 📤 Exportar a Fichero");

    AnsiConsole.MarkupLine("\n[lightseagreen]━━━━━━━ 5. COPIAS DE SEGURIDAD ━━━━━━━[/]");
    AnsiConsole.MarkupLine(" [yellow]19.[/] 💾 Crear Backup");
    AnsiConsole.MarkupLine(" [yellow]20.[/] ♻️  Restaurar Backup");

    AnsiConsole.MarkupLine("\n[red]━━━━━━━ 0. SALIR ━━━━━━━[/]");

    // AnsiConsole.Ask: Input con Spectre para introducir opción
    var opcionStr = AnsiConsole.Ask<string>("\n👉 Seleccione una opción: ");

    // Validación de la opción introducida
    if (!int.TryParse(opcionStr, out var opcionValue) || !Enum.IsDefined(typeof(OpcionMenu), opcionValue)) {
        AnsiConsole.MarkupLine("[red]❌ Opción no válida.[/]");
        return OpcionMenu.Salir;
    }

    return (OpcionMenu)opcionValue;
}

// ====================================================================
// MÉTODOS DE OPERACIÓN - GESTIÓN DE PERSONAS
// ====================================================================

// -------------------------------------------------------------------------
// LISTAR TODAS LAS PERSONAS CON CRITERIO DE ORDENACIÓN
// -------------------------------------------------------------------------
void ListarTodas(IAcademiaService service) {
    // Rule: Sección decorativa
    AnsiConsole.Write(new Rule("[lightseagreen]👥 LISTADO INTEGRAL DEL PERSONAL[/]").RuleStyle(Color.LightSeaGreen));

    // AnsiConsole.Ask: Input para seleccionar criterio
    var criterio =
        AnsiConsole.Ask<string>(
            "⚙️  Criterios: [yellow]1.ID[/], [yellow]2.DNI[/], [yellow]3.Apellidos[/], [yellow]4.Nombre[/], [yellow]5.Ciclo[/]\n🎯 Seleccione criterio: ");

    // -------------------------------------------------------------------------
    // MAPEO DE CRITERIO A ENUM
    // -------------------------------------------------------------------------
    var orden = criterio switch {
        "1" => TipoOrdenamiento.Id,
        "2" => TipoOrdenamiento.Dni,
        "3" => TipoOrdenamiento.Apellidos,
        "4" => TipoOrdenamiento.Nombre,
        _ => TipoOrdenamiento.Ciclo
    };

    // AnsiConsole.Status: Spinner de carga
    AnsiConsole.Status().Start("📊 Cargando personal...", ctx => {
        var lista = service.GetAllOrderBy(orden);
        ImprimirTablaPersonas(lista);
    });
}

// -------------------------------------------------------------------------
// BUSCAR PERSONA POR DNI
// -------------------------------------------------------------------------
void BuscarPorDniGeneral(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]🔍 BÚSQUEDA POR DNI[/]").RuleStyle(Color.LightSeaGreen));

    // Función de lectura de DNI validado
    var dni = LeerDniValidado();

    // AnsiConsole.Status: Spinner de búsqueda
    AnsiConsole.Status().Start("🔎 Buscando persona...", ctx => {
        service.GetByDni(dni).Match(
            p => ImprimirPanelPersona(p),
            error => MostrarError(error.Message)
        );
    });
}

// -------------------------------------------------------------------------
// BUSCAR PERSONA POR ID
// -------------------------------------------------------------------------
void BuscarPorIdGeneral(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]🆔 BÚSQUEDA POR ID[/]").RuleStyle(Color.LightSeaGreen));

    var idStr = AnsiConsole.Ask<string>("🆔 Introduzca ID: ");

    // Validación con Regex
    if (!Regex.IsMatch(idStr, @"^\d+$")) {
        MostrarError("Debe ser un número entero.");
        return;
    }

    AnsiConsole.Status().Start("🔎 Buscando persona...", ctx => {
        service.GetById(int.Parse(idStr)).Match(
            p => ImprimirPanelPersona(p),
            error => MostrarError(error.Message)
        );
    });
}

// -------------------------------------------------------------------------
// LISTAR ESTUDIANTES CON CRITERIO DE ORDENACIÓN
// -------------------------------------------------------------------------
void ListarEstudiantes(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]🎓 LISTADO DE ESTUDIANTES[/]").RuleStyle(Color.LightSeaGreen));

    var criterio =
        AnsiConsole.Ask<string>(
            "⚙️  Criterios: [yellow]1.ID[/], [yellow]2.DNI[/], [yellow]3.Apellidos[/], [yellow]4.Nombre[/], [yellow]5.Nota[/], [yellow]6.Curso[/], [yellow]7.Ciclo[/]\n🎯 Seleccione criterio: ");

    var orden = criterio switch {
        "1" => TipoOrdenamiento.Id,
        "2" => TipoOrdenamiento.Dni,
        "3" => TipoOrdenamiento.Apellidos,
        "4" => TipoOrdenamiento.Nombre,
        "5" => TipoOrdenamiento.Nota,
        "6" => TipoOrdenamiento.Curso,
        _ => TipoOrdenamiento.Ciclo
    };

    AnsiConsole.Status().Start("📊 Cargando estudiantes...", ctx => {
        var lista = service.GetEstudiantesOrderBy(orden);
        ImprimirTablaEstudiantes(lista);
    });
}

// -------------------------------------------------------------------------
// AÑADIR NUEVO ESTUDIANTE
// -------------------------------------------------------------------------
void AnadirNuevoEstudiante(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]➕ ALTA DE NUEVO ESTUDIANTE[/]").RuleStyle(Color.LightSeaGreen));

    // AnsiConsole.Confirm: Confirmación antes de continuar
    if (!AnsiConsole.Confirm("⚠️  ¿Desea dar de alta un nuevo estudiante?")) {
        AnsiConsole.MarkupLine("[yellow]👋 Operación cancelada.[/]");
        return;
    }

    // -------------------------------------------------------------------------
    // LECTURA DE DATOS DEL ESTUDIANTE
    // -------------------------------------------------------------------------
    var dni = LeerDniValidado();
    var nombre = AnsiConsole.Ask<string>("👤 Nombre ([green]2-30 letras[/]): ");
    var apellidos = AnsiConsole.Ask<string>("👤 Apellidos ([green]2-50 letras[/]): ");
    var nota = LeerNotaValida();
    var ciclo = LeerCiclo();
    var curso = LeerCurso();

    // -------------------------------------------------------------------------
    // CREACIÓN DEL ESTUDIANTE
    // -------------------------------------------------------------------------
    var temp = new Estudiante
        { Dni = dni, Nombre = nombre, Apellidos = apellidos, Calificacion = nota, Ciclo = ciclo, Curso = curso };

    // Rule: Sección decorativa para revisión
    AnsiConsole.Write(new Rule("[yellow]👀 REVISE LOS DATOS[/]").RuleStyle(Color.Yellow));
    ImprimirPanelPersona(temp);

    if (AnsiConsole.Confirm("¿Confirmar alta?"))
        AnsiConsole.Status().Start("💾 Guardando estudiante...", ctx => {
            service.Save(temp).Match(
                creado => {
                    MostrarExito("✅ Guardado con éxito.");
                    ImprimirPanelPersona(creado);
                },
                error => MostrarError($"❌ ERROR: {error.Message}")
            );
        });
}

// -------------------------------------------------------------------------
// ACTUALIZAR ESTUDIANTE
// -------------------------------------------------------------------------
void ActualizarEstudiante(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]📝 ACTUALIZACIÓN DE ESTUDIANTE[/]").RuleStyle(Color.LightSeaGreen));

    if (!AnsiConsole.Confirm("⚠️  ¿Desea actualizar un estudiante?")) {
        AnsiConsole.MarkupLine("[yellow]👋 Operación cancelada.[/]");
        return;
    }

    // -------------------------------------------------------------------------
    // BÚSQUEDA DEL ESTUDIANTE A ACTUALIZAR
    // -------------------------------------------------------------------------
    var dni = LeerDniValidado();
    var pResult = service.GetByDni(dni);

    if (pResult.IsFailure) {
        MostrarError($"❌ ERROR: {pResult.Error.Message}");
        return;
    }

    var p = pResult.Value;
    if (p is not Estudiante est) {
        MostrarError("❌ ERROR: No es un Estudiante.");
        return;
    }

    // -------------------------------------------------------------------------
    // VISUALIZAR DATOS ACTUALES
    // -------------------------------------------------------------------------
    ImprimirPanelPersona(est);

    // -------------------------------------------------------------------------
    // LECTURA DE NUEVOS DATOS (CON VALORES POR DEFECTO)
    // -------------------------------------------------------------------------
    var nNom = AnsiConsole.Ask<string>($"👤 Nombre [{est.Nombre}] (Enter mant.): ");
    var nApe = AnsiConsole.Ask<string>($"👤 Apellidos [{est.Apellidos}] (Enter mant.): ");
    var nota = AnsiConsole.Confirm("¿Cambiar nota?") ? LeerNotaValida() : est.Calificacion;
    var ciclo = AnsiConsole.Confirm("¿Cambiar ciclo?") ? LeerCiclo() : est.Ciclo;
    var curso = AnsiConsole.Confirm("¿Cambiar curso?") ? LeerCurso() : est.Curso;

    // -------------------------------------------------------------------------
    // CREACIÓN DEL REGISTRO ACTUALIZADO (RECORD WITH)
    // -------------------------------------------------------------------------
    var act = est with {
        Nombre = string.IsNullOrWhiteSpace(nNom) ? est.Nombre : nNom,
        Apellidos = string.IsNullOrWhiteSpace(nApe) ? est.Apellidos : nApe,
        Calificacion = nota,
        Ciclo = ciclo,
        Curso = curso
    };

    // -------------------------------------------------------------------------
    // REVISIÓN Y CONFIRMACIÓN
    // -------------------------------------------------------------------------
    AnsiConsole.Write(new Rule("[yellow]👀 REVISE CAMBIOS[/]").RuleStyle(Color.Yellow));
    ImprimirPanelPersona(act);

    if (AnsiConsole.Confirm("¿Actualizar?"))
        AnsiConsole.Status().Start("📝 Actualizando estudiante...", ctx => {
            service.Update(est.Id, act).Match(
                actualizado => {
                    MostrarExito("✅ Actualizado.");
                    ImprimirPanelPersona(actualizado);
                },
                error => MostrarError($"❌ ERROR: {error.Message}")
            );
        });
}

// -------------------------------------------------------------------------
// ELIMINAR ESTUDIANTE (BORRADO FÍSICO)
// -------------------------------------------------------------------------
void EliminarEstudiante(IAcademiaService service) {
    // Rule: Sección en ROJO para indicar acción destructiva
    AnsiConsole.Write(new Rule("[red]🗑️  ELIMINACIÓN DE ESTUDIANTE[/]").RuleStyle(Color.Red));

    var dni = LeerDniValidado();
    var pResult = service.GetByDni(dni);

    if (pResult.IsFailure) {
        MostrarError($"❌ ERROR: {pResult.Error.Message}");
        return;
    }

    var p = pResult.Value;
    if (p is not Estudiante) {
        MostrarError("❌ ERROR: No es un Estudiante.");
        return;
    }

    ImprimirPanelPersona(p);

    // AnsiConsole.Confirm: Confirmación CRÍTICA en ROJO
    if (AnsiConsole.Confirm($"[red]⚠️  ¿Eliminar a {p.NombreCompleto}? Esta acción es irreversible.[/]"))
        AnsiConsole.Status().Start("🗑️  Eliminando estudiante...", ctx => {
            service.Delete(p.Id).Match(
                eliminado => {
                    MostrarExito("✅ Borrado físicamente.");
                    ImprimirPanelPersona(eliminado);
                },
                error => MostrarError($"❌ ERROR: {error.Message}")
            );
        });
}

// -------------------------------------------------------------------------
// INFORME DE RENDIMIENTO DE ESTUDIANTES
// -------------------------------------------------------------------------
void MostrarInformeEstudiantes(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]📊 INFORME DE RENDIMIENTO ACADÉMICO[/]").RuleStyle(Color.LightSeaGreen));

    var alcance =
        AnsiConsole.Ask<string>(
            "⚙️  Alcance: [yellow]1.Global[/], [yellow]2.Por Ciclo[/], [yellow]3.Por Curso[/], [yellow]4.Clase Específica[/]\n🎯 Seleccione alcance: ");

    Ciclo? fCiclo = null;
    Curso? fCurso = null;

    switch (alcance) {
        case "2": fCiclo = LeerCiclo(); break;
        case "3": fCurso = LeerCurso(); break;
        case "4":
            fCiclo = LeerCiclo();
            fCurso = LeerCurso();
            break;
    }

    AnsiConsole.Status().Start("📊 Generando informe...", ctx => {
        var inf = service.GenerarInformeEstudiante(fCiclo, fCurso);
        var desc = alcance switch {
            "2" => $"Ciclo {fCiclo}",
            "3" => $"Curso {fCurso}",
            "4" => $"{fCurso}º {fCiclo}",
            _ => "Global"
        };

        // Panel: Resumen del informe con bordes decorativos
        var panel = new Panel($"""
                               [lightseagreen]📍 ALCANCE:[/] {desc}
                               [lightseagreen]👨‍🎓 Estudiantes:[/] {inf.TotalEstudiantes} | [lightseagreen]📈 Media:[/] {inf.NotaMedia.ToString("F2", AppConfig.Locale)}
                               [lightseagreen]✅ Aprobados:[/] {inf.Aprobados} ({inf.PorcentajeAprobados.ToString("F2", AppConfig.Locale)}%)
                               """)
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.LightSkyBlue1)
            .Header("📊 RESUMEN DE RENDIMIENTO");
        AnsiConsole.Write(panel);

        AnsiConsole.MarkupLine("\n[lightseagreen]🏆 RANKING POR NOTA (DESCENDENTE):[/]");
        ImprimirTablaEstudiantes(inf.PorNota);
    });
}

// ====================================================================
// MÉTODOS DE OPERACIÓN - GESTIÓN DE DOCENTES
// ====================================================================

// -------------------------------------------------------------------------
// LISTAR DOCENTES CON CRITERIO DE ORDENACIÓN
// -------------------------------------------------------------------------
void ListarDocentes(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]👨‍🏫 LISTADO DE DOCENTES[/]").RuleStyle(Color.LightSeaGreen));

    var criterio =
        AnsiConsole.Ask<string>(
            "⚙️  Criterios: [yellow]1.ID[/], [yellow]2.DNI[/], [yellow]3.Apellidos[/], [yellow]4.Nombre[/], [yellow]5.Experiencia[/], [yellow]6.Módulo[/], [yellow]7.Ciclo[/]\n🎯 Seleccione criterio: ");

    var orden = criterio switch {
        "1" => TipoOrdenamiento.Id,
        "2" => TipoOrdenamiento.Dni,
        "3" => TipoOrdenamiento.Apellidos,
        "4" => TipoOrdenamiento.Nombre,
        "5" => TipoOrdenamiento.Experiencia,
        "6" => TipoOrdenamiento.Modulo,
        _ => TipoOrdenamiento.Ciclo
    };

    AnsiConsole.Status().Start("📊 Cargando docentes...", ctx => {
        var lista = service.GetDocentesOrderBy(orden);
        ImprimirTablaDocentes(lista);
    });
}

// -------------------------------------------------------------------------
// AÑADIR NUEVO DOCENTE
// -------------------------------------------------------------------------
void AnadirNuevoDocente(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]➕ ALTA DE NUEVO DOCENTE[/]").RuleStyle(Color.LightSeaGreen));

    if (!AnsiConsole.Confirm("⚠️  ¿Desea dar de alta un nuevo docente?")) {
        AnsiConsole.MarkupLine("[yellow]👋 Operación cancelada.[/]");
        return;
    }

    // -------------------------------------------------------------------------
    // LECTURA DE DATOS DEL DOCENTE
    // -------------------------------------------------------------------------
    var dni = LeerDniValidado();
    var nombre = AnsiConsole.Ask<string>("👤 Nombre ([green]2-30 letras[/]): ");
    var apellidos = AnsiConsole.Ask<string>("👤 Apellidos ([green]2-50 letras[/]): ");
    var experiencia = AnsiConsole.Ask<int>("⏳ Años de Experiencia: ");
    var modulo = LeerModulo();
    var ciclo = LeerCiclo();

    var temp = new Docente {
        Dni = dni, Nombre = nombre, Apellidos = apellidos, Experiencia = experiencia, Especialidad = modulo,
        Ciclo = ciclo
    };

    ImprimirPanelPersona(temp);

    if (AnsiConsole.Confirm("¿Confirmar alta?"))
        AnsiConsole.Status().Start("💾 Guardando docente...", ctx => {
            service.Save(temp).Match(
                creado => {
                    MostrarExito("✅ Guardado con éxito.");
                    ImprimirPanelPersona(creado);
                },
                error => MostrarError($"❌ ERROR: {error.Message}")
            );
        });
}

// -------------------------------------------------------------------------
// ACTUALIZAR DOCENTE
// -------------------------------------------------------------------------
void ActualizarDocente(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]📝 ACTUALIZACIÓN DE DOCENTE[/]").RuleStyle(Color.LightSeaGreen));

    if (!AnsiConsole.Confirm("⚠️  ¿Desea actualizar un docente?")) {
        AnsiConsole.MarkupLine("[yellow]👋 Operación cancelada.[/]");
        return;
    }

    var dni = LeerDniValidado();
    var pResult = service.GetByDni(dni);

    if (pResult.IsFailure) {
        MostrarError($"❌ ERROR: {pResult.Error.Message}");
        return;
    }

    var p = pResult.Value;
    if (p is not Docente doc) {
        MostrarError("❌ ERROR: No es un Docente.");
        return;
    }

    ImprimirPanelPersona(doc);

    var nNom = AnsiConsole.Ask<string>($"👤 Nombre [{doc.Nombre}] (Enter mant.): ");
    var nApe = AnsiConsole.Ask<string>($"👤 Apellidos [{doc.Apellidos}] (Enter mant.): ");
    var exp = AnsiConsole.Confirm("¿Cambiar experiencia?")
        ? AnsiConsole.Ask<int>("⏳ Nuevos años de experiencia: ")
        : doc.Experiencia;
    var mod = AnsiConsole.Confirm("¿Cambiar módulo?") ? LeerModulo() : doc.Especialidad;
    var ciclo = AnsiConsole.Confirm("¿Cambiar ciclo?") ? LeerCiclo() : doc.Ciclo;

    var act = doc with {
        Nombre = string.IsNullOrWhiteSpace(nNom) ? doc.Nombre : nNom,
        Apellidos = string.IsNullOrWhiteSpace(nApe) ? doc.Apellidos : nApe,
        Experiencia = exp,
        Especialidad = mod,
        Ciclo = ciclo
    };

    ImprimirPanelPersona(act);

    if (AnsiConsole.Confirm("¿Actualizar?"))
        AnsiConsole.Status().Start("📝 Actualizando docente...", ctx => {
            service.Update(doc.Id, act).Match(
                actualizado => {
                    MostrarExito("✅ Actualizado.");
                    ImprimirPanelPersona(actualizado);
                },
                error => MostrarError($"❌ ERROR: {error.Message}")
            );
        });
}

// -------------------------------------------------------------------------
// ELIMINAR DOCENTE (BORRADO FÍSICO)
// -------------------------------------------------------------------------
void EliminarDocente(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[red]🗑️  ELIMINACIÓN DE DOCENTE[/]").RuleStyle(Color.Red));
    var dni = LeerDniValidado();
    var pResult = service.GetByDni(dni);

    if (pResult.IsFailure) {
        MostrarError($"❌ ERROR: {pResult.Error.Message}");
        return;
    }

    var p = pResult.Value;
    if (p is not Docente) {
        MostrarError("❌ ERROR: No es un Docente.");
        return;
    }

    ImprimirPanelPersona(p);

    if (AnsiConsole.Confirm($"[red]⚠️  ¿Eliminar a {p.NombreCompleto}? Esta acción es irreversible.[/]"))
        AnsiConsole.Status().Start("🗑️  Eliminando docente...", ctx => {
            service.Delete(p.Id).Match(
                eliminado => {
                    MostrarExito("✅ Borrado.");
                    ImprimirPanelPersona(eliminado);
                },
                error => MostrarError($"❌ ERROR: {error.Message}")
            );
        });
}

// -------------------------------------------------------------------------
// INFORME DE EXPERIENCIA DE DOCENTES
// -------------------------------------------------------------------------
void MostrarInformeDocentes(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]📈 INFORME DE CUADRO DOCENTE[/]").RuleStyle(Color.LightSeaGreen));

    var alcance =
        AnsiConsole.Ask<string>("⚙️  Alcance: [yellow]1.Global[/], [yellow]2.Por Ciclo[/]\n🎯 Seleccione alcance: ");

    Ciclo? fCiclo = null;
    if (alcance == "2") fCiclo = LeerCiclo();

    AnsiConsole.Status().Start("📊 Generando informe...", ctx => {
        var inf = service.GenerarInformeDocente(fCiclo);
        var desc = alcance == "2" ? $"Ciclo {fCiclo}" : "Global";

        var panel = new Panel($"""
                               [lightseagreen]📍 ALCANCE:[/] {desc}
                               [lightseagreen]👨‍🏫 Docentes:[/] {inf.TotalDocentes} | [lightseagreen]⏳ Media:[/] {inf.ExperienciaMedia.ToString("F2", AppConfig.Locale)} años
                               """)
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.LightSkyBlue1)
            .Header("📈 RESUMEN DE EXPERIENCIA");
        AnsiConsole.Write(panel);

        AnsiConsole.MarkupLine("\n[lightseagreen]🏆 RANKING POR EXPERIENCIA (DESCENDENTE):[/]");
        ImprimirTablaDocentes(inf.PorExperiencia);
    });
}

// ====================================================================
// MÉTODOS DE OPERACIÓN - EXPORTACIÓN HTML
// ====================================================================

// -------------------------------------------------------------------------
// GENERAR LISTADO HTML DE TODAS LAS PERSONAS
// -------------------------------------------------------------------------
void ListarTodasHtml(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]🖨️ GENERANDO LISTADO HTML[/]").RuleStyle(Color.LightSeaGreen));

    AnsiConsole.Status().Start("🖨️ Generando HTML...", ctx => {
        service.GenerarListadoPersonasHtml()
            .Match(
                filePath => {
                    MostrarExito($"✅ Informe guardado: {Path.GetFileName(filePath)}");
                    Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                    AnsiConsole.MarkupLine("[green]✅ Informe abierto en el navegador[/]");
                },
                error => MostrarError(error.Message)
            );
    });
}

// -------------------------------------------------------------------------
// GENERAR INFORME HTML DE ESTUDIANTES
// -------------------------------------------------------------------------
void InformeEstudiantesHtml(IAcademiaService service) {
    AnsiConsole.Write(
        new Rule("[lightseagreen]🖨️ GENERANDO INFORME HTML DE ESTUDIANTES[/]").RuleStyle(Color.LightSeaGreen));

    AnsiConsole.Status().Start("🖨️ Generando HTML...", ctx => {
        service.GenerarInformeEstudiantesHtml()
            .Match(
                filePath => {
                    MostrarExito($"✅ Informe guardado: {Path.GetFileName(filePath)}");
                    Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                    AnsiConsole.MarkupLine("[green]✅ Informe abierto en el navegador[/]");
                },
                error => MostrarError(error.Message)
            );
    });
}

// -------------------------------------------------------------------------
// GENERAR INFORME HTML DE DOCENTES
// -------------------------------------------------------------------------
void InformeDocentesHtml(IAcademiaService service) {
    AnsiConsole.Write(
        new Rule("[lightseagreen]🖨️ GENERANDO INFORME HTML DE DOCENTES[/]").RuleStyle(Color.LightSeaGreen));

    AnsiConsole.Status().Start("🖨️ Generando HTML...", ctx => {
        service.GenerarInformeDocentesHtml()
            .Match(
                filePath => {
                    MostrarExito($"✅ Informe guardado: {Path.GetFileName(filePath)}");
                    Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                    AnsiConsole.MarkupLine("[green]✅ Informe abierto en el navegador[/]");
                },
                error => MostrarError(error.Message)
            );
    });
}

// ====================================================================
// MÉTODOS DE OPERACIÓN - IMPORTAR/EXPORTAR DATOS
// ====================================================================

// -------------------------------------------------------------------------
// IMPORTAR DATOS DESDE FICHERO (MEJORA 1 - BARRA DE PROGRESO)
// -------------------------------------------------------------------------
void ImportarDatos(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]📥 IMPORTAR DATOS DESDE FICHERO[/]").RuleStyle(Color.LightSeaGreen));

    if (!AnsiConsole.Confirm(
            $"⚠️  Desea importar los datos desde el fiche o: {AppConfig.AcademiaFile}\nEsta acción puede sobrescribir datos existentes. ¿Desea continuar?")) {
        AnsiConsole.MarkupLine("[yellow]👋 Operación cancelada.[/]");
        return;
    }

    // Barra de progreso para importar datos
    AnsiConsole.Progress()
        .Columns(new TaskDescriptionColumn(), new SpinnerColumn(), new PercentageColumn(), new RemainingTimeColumn())
        .Start(ctx => {
            var task = ctx.AddTask("[cyan]📥 Importando datos...[/]");

            while (!ctx.IsFinished) {
                Thread.Sleep(100);
                task.Increment(10);
                if (task.Percentage >= 100) break;
            }

            service.ImportarDatos().Match(
                importados => MostrarExito($"✅ Importados {importados} registros."),
                error => MostrarError($"☠️ ERROR AL IMPORTAR: {error.Message}")
            );
        });
}

// -------------------------------------------------------------------------
// EXPORTAR DATOS A FICHERO (MEJORA 1 - BARRA DE PROGRESO)
// -------------------------------------------------------------------------
void ExportarDatos(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]📤 EXPORTAR DATOS A FICHERO[/]").RuleStyle(Color.LightSeaGreen));

    // Barra de progreso para exportar datos
    AnsiConsole.Progress()
        .Columns(new TaskDescriptionColumn(), new SpinnerColumn(), new PercentageColumn(), new RemainingTimeColumn())
        .Start(ctx => {
            var task = ctx.AddTask("[cyan]📤 Exportando datos...[/]");

            while (!ctx.IsFinished) {
                Thread.Sleep(100);
                task.Increment(10);
                if (task.Percentage >= 100) break;
            }

            service.ExportarDatos().Match(
                exportados => MostrarExito($"✅ Exportados {exportados} registros."),
                error => MostrarError($"☠️ ERROR AL EXPORTAR: {error.Message}")
            );
        });
}

// ====================================================================
// MÉTODOS DE OPERACIÓN - COPIAS DE SEGURIDAD
// ====================================================================

// -------------------------------------------------------------------------
// CREAR COPIA DE SEGURIDAD
// -------------------------------------------------------------------------
void RealizarBackup(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]💾 CREAR COPIA DE SEGURIDAD[/]").RuleStyle(Color.LightSeaGreen));

    if (!AnsiConsole.Confirm("⚠️  ¿Desea crear una copia de seguridad de todos los datos?")) {
        AnsiConsole.MarkupLine("[yellow]👋 Operación cancelada.[/]");
        return;
    }

    AnsiConsole.Status().Start("💾 Creando backup...", ctx => {
        service.RealizarBackup().Match(
            ruta => {
                MostrarExito("✅ Backup creado correctamente.");
                AnsiConsole.MarkupLine($"[lightseagreen]📁 Archivo:[/] {ruta}");
            },
            error => MostrarError($"☠️ ERROR AL CREAR BACKUP: {error.Message}")
        );
    });
}

// -------------------------------------------------------------------------
// RESTAURAR COPIA DE SEGURIDAD
// -------------------------------------------------------------------------
void RestaurarBackup(IAcademiaService service) {
    AnsiConsole.Write(new Rule("[lightseagreen]♻️ RESTAURAR COPIA DE SEGURIDAD[/]").RuleStyle(Color.LightSeaGreen));

    var backups = service.ListarBackups().ToList();
    if (backups.Count == 0) {
        MostrarError("❌ No hay copias de seguridad disponibles.");
        return;
    }

    // -------------------------------------------------------------------------
    // TABLE: PRESENTACIÓN DE BACKUPS DISPONIBLES
    // -------------------------------------------------------------------------
    var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.LightSkyBlue1);
    table.AddColumn(new TableColumn("[lightseagreen]#[/]").Centered());
    table.AddColumn(new TableColumn("[lightseagreen]Archivo[/]").LeftAligned());
    table.AddColumn(new TableColumn("[lightseagreen]Tamaño[/]").Centered());
    table.AddColumn(new TableColumn("[lightseagreen]Fecha[/]").RightAligned());

    for (var i = 0; i < backups.Count; i++) {
        var file = new FileInfo(backups[i]);
        var size = file.Length < 1024 ? $"{file.Length} B" : $"{Math.Round(file.Length / 1024.0, 1)} KB";
        table.AddRow($"{i + 1}", file.Name, size, file.CreationTime.ToString("g"));
    }

    AnsiConsole.MarkupLine("[lightseagreen]📋 COPIAS DE SEGURIDAD DISPONIBLES:[/]");
    AnsiConsole.Write(table);

    var indice = AnsiConsole.Ask<int>("🎯 Seleccione archivo (0 para volver): ") - 1;

    if (indice < 0) {
        AnsiConsole.MarkupLine("[yellow]👋 Operación cancelada.[/]");
        return;
    }

    if (indice >= backups.Count) {
        MostrarError("❌ Selección inválida.");
        return;
    }

    var archivoSeleccionado = backups[indice];
    AnsiConsole.MarkupLine($"\n[lightseagreen]📄 Seleccionado:[/] {archivoSeleccionado}");

    // -------------------------------------------------------------------------
    // CONFIRMACIÓN CRÍTICA EN ROJO
    // -------------------------------------------------------------------------
    if (!AnsiConsole.Confirm("[red]⚠️  Esta acción eliminará todos los datos actuales. ¿Continuar?[/]")) {
        AnsiConsole.MarkupLine("[yellow]👋 Operación cancelada.[/]");
        return;
    }

    AnsiConsole.Status().Start("♻️ Restaurando backup...", ctx => {
        service.RestaurarBackup(archivoSeleccionado).Match(
            restaurados => MostrarExito($"✅ Restauración completada. Total registros: {restaurados}"),
            error => MostrarError($"☠️ ERROR AL RESTAURAR: {error.Message}")
        );
    });
}

// ====================================================================
// RENDERIZADO UNIFICADO - TABLAS CON SPECTRE.CONSOLE
// ====================================================================

// -------------------------------------------------------------------------
// TABLE: LISTADO DE PERSONAS (MEZCLA DE ESTUDIANTES Y DOCENTES) (MEJORA 4)
// -------------------------------------------------------------------------
void ImprimirTablaPersonas(IEnumerable<Persona> lista) {
    // Table: Tabla con bordes rounded y color
    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.LightSkyBlue1)
        .Title("👥 [LightSkyBlue1]LISTADO COMPLETO DEL PERSONAL[/]");

    // -------------------------------------------------------------------------
    // DEFINICIÓN DE COLUMNAS
    // -------------------------------------------------------------------------
    table.AddColumn(new TableColumn("[lightseagreen]ID[/]").Centered());
    table.AddColumn(new TableColumn("[lightseagreen]DNI[/]").Centered());
    table.AddColumn(new TableColumn("[lightseagreen]Nombre Completo[/]").LeftAligned());
    table.AddColumn(new TableColumn("[lightseagreen]Ciclo[/]").Centered());
    table.AddColumn(new TableColumn("[lightseagreen]Tipo[/]").Centered());

    // -------------------------------------------------------------------------
    // FILAS CON DATOS Y COLORES POR CATEGORÍA
    // -------------------------------------------------------------------------
    foreach (var p in lista) {
        var (tipoEmoji, tipoTexto, cicloColor) = p switch {
            Estudiante e => ("🎓", "Estudiante", e.Ciclo switch {
                Ciclo.DAM => "cyan",
                Ciclo.DAW => "yellow",
                Ciclo.ASIR => "magenta",
                _ => "white"
            }),
            Docente d => ("👨‍🏫", "Docente", d.Ciclo switch {
                Ciclo.DAM => "cyan",
                Ciclo.DAW => "yellow",
                Ciclo.ASIR => "magenta",
                _ => "white"
            }),
            _ => ("❓", "Desconocido", "white")
        };

        var tipoColor = p is Estudiante ? "b" : "lightgreen";
        table.AddRow($"{p.Id}", p.Dni, p.NombreCompleto,
            $"[{cicloColor}]{p switch { Estudiante e => e.Ciclo.ToString(), Docente d => d.Ciclo.ToString(), _ => "N/A" }}[/]",
            $"[{tipoColor}]{tipoEmoji} {tipoTexto}[/]");
    }

    AnsiConsole.Write(table);
}

// -------------------------------------------------------------------------
// TABLE: LISTADO DE ESTUDIANTES CON COLOR CONDICIONAL EN NOTAS (MEJORA 4)
// -------------------------------------------------------------------------
void ImprimirTablaEstudiantes(IEnumerable<Estudiante> lista) {
    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.LightCyan1)
        .Title("🎓 [LightCyan1]LISTADO DE ESTUDIANTES[/]");

    table.AddColumn(new TableColumn("[cyan]ID[/]").Centered());
    table.AddColumn(new TableColumn("[cyan]DNI[/]").Centered());
    table.AddColumn(new TableColumn("[cyan]Nombre Completo[/]").LeftAligned());
    table.AddColumn(new TableColumn("[cyan]Ciclo[/]").Centered());
    table.AddColumn(new TableColumn("[cyan]Curso[/]").Centered());
    table.AddColumn(new TableColumn("[cyan]Nota[/]").Centered());
    table.AddColumn(new TableColumn("[cyan]Evaluación[/]").Centered());

    foreach (var e in lista) {
        // -------------------------------------------------------------------------
        // COLOR CONDICIONAL: Verde si aprueba, rojo si suspende
        // -------------------------------------------------------------------------
        var notaColor = e.Calificacion >= 5 ? "green" : "red";
        var cicloColor = e.Ciclo switch {
            Ciclo.DAM => "cyan",
            Ciclo.DAW => "yellow",
            Ciclo.ASIR => "magenta",
            _ => "white"
        };
        table.AddRow(
            $"{e.Id}",
            e.Dni,
            e.NombreCompleto,
            $"[{cicloColor}]{e.Ciclo}[/]",
            $"{(int)e.Curso}º",
            $"[{notaColor}]{e.Calificacion.ToString("F2", AppConfig.Locale)}[/]",
            e.CalificacionCualitativa
        );
    }

    AnsiConsole.Write(table);
}

// -------------------------------------------------------------------------
// TABLE: LISTADO DE DOCENTES CON COLOR CONDICIONAL EN EXPERIENCIA (MEJORA 4)
// -------------------------------------------------------------------------
void ImprimirTablaDocentes(IEnumerable<Docente> lista) {
    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.LightGreen)
        .Title("👨‍🏫 [LightGreen]LISTADO DE DOCENTES[/]");

    table.AddColumn(new TableColumn("[lightgreen]ID[/]").Centered());
    table.AddColumn(new TableColumn("[lightgreen]DNI[/]").Centered());
    table.AddColumn(new TableColumn("[lightgreen]Nombre Completo[/]").LeftAligned());
    table.AddColumn(new TableColumn("[lightgreen]Ciclo[/]").Centered());
    table.AddColumn(new TableColumn("[lightgreen]Exp[/]").Centered());
    table.AddColumn(new TableColumn("[lightgreen]Especialidad[/]").Centered());

    foreach (var d in lista) {
        var expColor = d.Experiencia switch {
            >= 10 => "green",
            >= 5 => "yellow",
            _ => "red"
        };
        var cicloColor = d.Ciclo switch {
            Ciclo.DAM => "cyan",
            Ciclo.DAW => "yellow",
            Ciclo.ASIR => "magenta",
            _ => "white"
        };
        table.AddRow(
            $"{d.Id}",
            d.Dni,
            d.NombreCompleto,
            $"[{cicloColor}]{d.Ciclo}[/]",
            $"[{expColor}]{d.Experiencia} años[/]",
            d.Especialidad
        );
    }

    AnsiConsole.Write(table);
}

// -------------------------------------------------------------------------
// PANEL: FICHA INDIVIDUAL DE PERSONA
// -------------------------------------------------------------------------
void ImprimirPanelPersona(Persona p) {
    var contenido = new StringBuilder();
    contenido.AppendLine($"[lightseagreen]🆔 ID:[/]          {(p.Id == 0 ? "[yellow]PENDIENTE[/]" : $"{p.Id}")}");
    contenido.AppendLine($"[lightseagreen]🆔 DNI:[/]         {p.Dni}");
    contenido.AppendLine($"[lightseagreen]👤 APELLIDOS:[/]   {p.Apellidos}");
    contenido.AppendLine($"[lightseagreen]👤 NOMBRE:[/]      {p.Nombre}");

    if (p is Estudiante e) {
        contenido.AppendLine("[lightseagreen]🎭 TIPO:[/]        [cyan]🎓 ESTUDIANTE[/]");
        var notaColor = e.Calificacion >= 5 ? "green" : "red";
        contenido.AppendLine(
            $"[lightseagreen]📝 NOTA:[/]        [{notaColor}]{e.Calificacion.ToString("F2", AppConfig.Locale)}[/]");
        contenido.AppendLine($"[lightseagreen]🎖️  EVAL:[/]        {e.CalificacionCualitativa}");
        contenido.AppendLine($"[lightseagreen]📂 CICLO:[/]       {e.Ciclo}");
        contenido.AppendLine($"[lightseagreen]📅 CURSO:[/]       {e.Curso}");
    }
    else if (p is Docente d) {
        contenido.AppendLine("[lightseagreen]🎭 TIPO:[/]        [cyan]👨‍🏫 DOCENTE[/]");
        var expColor = d.Experiencia >= 5 ? "green" : "yellow";
        contenido.AppendLine($"[lightseagreen]⏳ EXP:[/]         [{expColor}]{d.Experiencia} años[/]");
        contenido.AppendLine($"[lightseagreen]📚 MOD:[/]         {d.Especialidad}");
        contenido.AppendLine($"[lightseagreen]📂 CICLO:[/]       {d.Ciclo}");
    }

    if (p.CreatedAt != default) {
        contenido.AppendLine();
        contenido.AppendLine(
            $"[lightseagreen]📅 ALTA (LOC):[/]  {p.CreatedAt.ToLocalTime().ToString("g", AppConfig.Locale)}");
        contenido.AppendLine(
            $"[lightseagreen]🔄 MOD  (LOC):[/]  {p.UpdatedAt.ToLocalTime().ToString("g", AppConfig.Locale)}");
        var estado = p.IsDeleted ? "[red]❌ ELIMINADO[/]" : "[green]✅ ACTIVO[/]";
        contenido.AppendLine($"[lightseagreen]🚦 ESTADO:[/]      {estado}");
    }

    // Panel: Visualización de datos con bordes decorativos
    var panel = new Panel(contenido.ToString())
        .Border(BoxBorder.Heavy)
        .BorderColor(Color.LightSkyBlue1)
        .Header("🆔 IDENTIDAD ACADÉMICA");
    AnsiConsole.Write(panel);
}

// ====================================================================
// PANELES DE FEEDBACK - ERRORES Y ÉXITOS
// ====================================================================

// -------------------------------------------------------------------------
// PANEL: MENSAJE DE ERROR (BORDES ROJOS) + LOGGING
// -------------------------------------------------------------------------
void MostrarError(string mensaje) {
    // Loguear el error a archivo (nivel Error)
    Log.Error("❌ {ErrorMessage}", mensaje);

    var panel = new Panel($"[red]{mensaje}[/]")
        .Border(BoxBorder.Heavy)
        .BorderColor(Color.Red)
        .Header("❌ ERROR");
    AnsiConsole.Write(panel);
}

// -------------------------------------------------------------------------
// PANEL: MENSAJE DE ÉXITO (BORDES VERDES)
// -------------------------------------------------------------------------
void MostrarExito(string mensaje) {
    var panel = new Panel($"[green]{mensaje}[/]")
        .Border(BoxBorder.Heavy)
        .BorderColor(Color.Green)
        .Header("✅ ÉXITO");
    AnsiConsole.Write(panel);
}

// ====================================================================
// APOYO (INPUT) - FUNCIONES DE LECTURA VALIDADA
// ====================================================================

// -------------------------------------------------------------------------
// VALIDACIÓN DE DNI (ALGORITMO OFICIAL ESPAÑOL)
// -------------------------------------------------------------------------
bool ValidarDniCompleto(string d) {
    if (!Regex.IsMatch(d, @"^(\d{8})([A-Z])$")) return false;
    var n = int.Parse(d.Substring(0, 8));
    return "TRWAGMYFPDXBNJZSQVHLCKE"[n % 23] == d[8];
}

// -------------------------------------------------------------------------
// LECTURA DE DNI CON VALIDACIÓN Y HINTS VISUALES (MEJORA 6)
// -------------------------------------------------------------------------
string LeerDniValidado() {
    while (true) {
        // Hint visual con ejemplo
        AnsiConsole.MarkupLine("[dim]💡 Ejemplo: 12345678A (8 dígitos + letra mayúscula)[/]");

        // AnsiConsole.Ask: Input con Spectre
        var d = AnsiConsole.Ask<string>("🆔 Introduzca DNI ([green]8 dígitos + letra[/]): ").Trim().ToUpper();
        if (ValidarDniCompleto(d)) return d;

        // Mensaje de error con sugerencia
        AnsiConsole.MarkupLine("[red]❌ ERROR: DNI inválido.[/]");
        AnsiConsole.MarkupLine(
            "[yellow]⚠️  Asegúrese de que:[yellow]\n  - Tenga 8 dígitos seguidos de una letra mayúscula\n  - La letra sea correcta según el algoritmo español[/]\n");
    }
}

// -------------------------------------------------------------------------
// LECTURA DE NOTA CON VALIDACIÓN DE RANGO Y HINTS (MEJORA 6)
// -------------------------------------------------------------------------
double LeerNotaValida() {
    var sep = AppConfig.Locale.NumberFormat.NumberDecimalSeparator;
    while (true) {
        // Hint visual con rango
        AnsiConsole.MarkupLine("[dim]💡 Rango válido: 0-10 | Ejemplo: 7,5 o 7.5[/]");

        var s = AnsiConsole.Ask<string>($"📝 Nota ([yellow]0-10[/], use '{sep}'): ").Trim().Replace(",", ".");
        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) && n >= 0 && n <= 10) {
            // Feedback visual según la nota
            var feedback = n switch {
                >= 9 => "[green]⭐ Excelente![/]",
                >= 7 => "[green]👍 Notable[/]",
                >= 5 => "[yellow]✅ Suficiente[/]",
                _ => "[red]❌ Insuficiente[/]"
            };
            AnsiConsole.MarkupLine(feedback);
            return n;
        }

        AnsiConsole.MarkupLine("[red]❌ Error: Formato o rango incorrecto.[/]");
        AnsiConsole.MarkupLine("[yellow]⚠️  Introduzca un número entre 0 y 10[/]\n");
    }
}

// -------------------------------------------------------------------------
// LECTURA DE CICLO CON SELECCIÓN VISUAL
// -------------------------------------------------------------------------
Ciclo LeerCiclo() {
    // -------------------------------------------------------------------------
    // PRESENTACIÓN VISUAL DE OPCIONES
    // -------------------------------------------------------------------------
    AnsiConsole.MarkupLine("📂 Ciclos Disponibles:");
    AnsiConsole.MarkupLine("  [yellow]1.[/] 📱 DAM (Desarrollo Aplicaciones Multiplataforma)");
    AnsiConsole.MarkupLine("  [yellow]2.[/] 🌐 DAW (Desarrollo Aplicaciones Web)");
    AnsiConsole.MarkupLine("  [yellow]3.[/] 🖥️  ASIR (Administración Sistemas Informáticos en Red)");

    var op = AnsiConsole.Ask<string>("🎯 Seleccione Ciclo (1-3): ");
    return op switch {
        "1" => Ciclo.DAM,
        "2" => Ciclo.DAW,
        _ => Ciclo.ASIR
    };
}

// -------------------------------------------------------------------------
// LECTURA DE CURSO CON SELECCIÓN VISUAL
// -------------------------------------------------------------------------
Curso LeerCurso() {
    AnsiConsole.MarkupLine("📅 Cursos Disponibles:");
    AnsiConsole.MarkupLine("  [yellow]1.[/] 📚 Primero");
    AnsiConsole.MarkupLine("  [yellow]2.[/] 📚 Segundo");

    var op = AnsiConsole.Ask<string>("🎯 Seleccione Curso (1-2): ");
    return op == "1" ? Curso.Primero : Curso.Segundo;
}

// -------------------------------------------------------------------------
// LECTURA DE MÓDULO CON SELECCIÓN VISUAL
// -------------------------------------------------------------------------
string LeerModulo() {
    AnsiConsole.MarkupLine("📚 Módulos Disponibles:");
    AnsiConsole.MarkupLine($"  [yellow]1.[/] 💻 {Modulos.Programacion}");
    AnsiConsole.MarkupLine($"  [yellow]2.[/] 🗄️  {Modulos.BasesDatos}");
    AnsiConsole.MarkupLine($"  [yellow]3.[/] ⚙️  {Modulos.Entornos}");
    AnsiConsole.MarkupLine($"  [yellow]4.[/] 📝 {Modulos.LenguajesMarcas}");

    var op = AnsiConsole.Ask<string>("🎯 Seleccione Módulo (1-4): ");
    return op switch {
        "1" => Modulos.Programacion,
        "2" => Modulos.BasesDatos,
        "3" => Modulos.Entornos,
        _ => Modulos.LenguajesMarcas
    };
}