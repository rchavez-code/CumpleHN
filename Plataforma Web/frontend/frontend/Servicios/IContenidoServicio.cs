using System;
using System.Collections.Generic;
using frontend.Modelos;

namespace frontend.Servicios
{
    /// <summary>
    /// Contrato único por el que las páginas obtienen contenido.
    ///
    /// Ninguna página consulta datos por su cuenta: todas pasan por acá. Esa es
    /// la razón de existir de esta interfaz — cuando el Web Service ASMX del
    /// backend esté listo, se agrega una implementación que lo consuma y se
    /// cambia una sola línea en <see cref="Contenido"/>, sin tocar el frontend.
    /// </summary>
    public interface IContenidoServicio
    {
        // ------------------------------------------------------- Campañas

        IList<Campana> ObtenerCampanas();

        /// <summary>Campaña destacada en la portada. Puede devolver null.</summary>
        Campana ObtenerCampanaActual();

        /// <summary>Campaña por slug. Devuelve null si no existe.</summary>
        Campana ObtenerCampana(string slug);

        // ----------------------------------------------------- Candidatos

        /// <summary>Candidatos de una campaña. Slug vacío devuelve todos.</summary>
        IList<Candidato> ObtenerCandidatos(string campanaSlug);

        Candidato ObtenerCandidato(string slug);

        /// <summary>
        /// Candidato autenticado en el panel privado. Mientras no exista
        /// autenticación real, devuelve un candidato de demostración.
        /// </summary>
        Candidato ObtenerCandidatoAutenticado();

        // ----------------------------------------------------- Propuestas

        /// <summary>Propuestas de un candidato.</summary>
        IList<Propuesta> ObtenerPropuestas(string candidatoSlug);

        /// <summary>Propuestas de una campaña completa.</summary>
        IList<Propuesta> ObtenerPropuestasDeCampana(string campanaSlug);

        Propuesta ObtenerPropuesta(int id);

        // --------------------------------------------------- Publicaciones

        /// <summary>Feed de una campaña, ordenado de la más reciente a la más antigua.</summary>
        IList<Publicacion> ObtenerFeed(string campanaSlug);

        IList<Publicacion> ObtenerPublicaciones(string candidatoSlug);

        // ------------------------------------------------------- Partidos

        IList<Partido> ObtenerPartidos();

        Partido ObtenerPartido(string slug);

        /// <summary>Candidaturas afiliadas a un partido.</summary>
        IList<Candidato> ObtenerCandidatosDePartido(string partidoSlug);

        // --------------------------------------------------- Interacción

        /// <summary>
        /// Participación acumulada sobre un objeto. Con
        /// <paramref name="codigoUsuario"/> en cero devuelve solo los totales,
        /// que es el caso de un visitante sin cuenta.
        /// </summary>
        Interaccion ObtenerInteraccion(string tipoObjeto, int codigoObjeto, int codigoUsuario);

        /// <summary>
        /// Registra un me gusta (valor 1) o un no me gusta (valor -1). Repetir
        /// el mismo voto lo retira y votar lo contrario lo cambia.
        /// </summary>
        Resultado Valorar(string tipoObjeto, int codigoObjeto, int codigoUsuario, int valor);

        IList<Comentario> ObtenerComentarios(string tipoObjeto, int codigoObjeto);

        Resultado AgregarComentario(string tipoObjeto, int codigoObjeto, int codigoUsuario, string texto);

        // ----------------------------------------------------- Encuestas

        /* La tercera pata de la participación, junto a valoraciones y
           comentarios. Consultar es público y responder exige cuenta, el mismo
           criterio de las otras dos. */

        /// <summary>
        /// Encuestas abiertas de una campaña, con sus opciones ya cargadas y
        /// en una sola llamada. Con la campaña vacía usa la destacada, que es
        /// como las pide la portada.
        ///
        /// Solo las abiertas: una cerrada terminó su votación y se consulta
        /// desde administración. Devuelve todas, y cuántas se muestran a la vez
        /// lo decide la página.
        ///
        /// El código de usuario decide si el reparto de votos viene revelado, y
        /// se resuelve por encuesta: alguien puede haber respondido una y no la
        /// otra. Quien lo decide es el procedimiento almacenado, no esta capa.
        /// </summary>
        IList<Encuesta> ObtenerEncuestasVigentes(string campanaSlug, int codigoUsuario);

        /// <summary>
        /// Registra la respuesta de una persona y devuelve la encuesta con el
        /// resultado ya actualizado. Elegir otra opción mientras sigue abierta
        /// cambia la respuesta anterior en lugar de sumar otra.
        /// </summary>
        ResultadoEncuesta ResponderEncuesta(int codigoEncuesta, int codigoOpcion, int codigoUsuario);

        // --------------------------------------------------- Iniciativas

        /* La otra mitad de la participación: además de reaccionar, la
           ciudadanía propone. Consultar es público y proponer exige cuenta
           ciudadana con correo confirmado, la misma puerta que votar y
           comentar. Las reacciones sobre una iniciativa van por el módulo de
           interacción, con TiposObjeto.Iniciativa. */

        /// <summary>
        /// Iniciativas activas, de la más popular (meGusta − noMeGusta) a la
        /// menos, con desempate por la más reciente. Devuelve todas: cuántas
        /// se ven y de a cuántas se despliegan lo decide la portada.
        /// </summary>
        IList<Iniciativa> ObtenerIniciativas(int codigoUsuario);

        /// <summary>
        /// Las iniciativas de una persona, activas y retiradas, para «Mi
        /// cuenta». Cada una dice si su texto todavía se puede editar.
        /// </summary>
        IList<Iniciativa> ObtenerIniciativasDeUsuario(int codigoUsuario);

        /// <summary>
        /// Alta (código cero) o edición. El texto solo se edita mientras nadie
        /// reaccionó, y lo decide el backend, no esta capa. Departamento en
        /// cero es «sin departamento».
        /// </summary>
        ResultadoGuardado GuardarIniciativa(int codigoUsuario, int codigoIniciativa,
            string titulo, string descripcion, int codigoCategoria, int codigoDepartamento);

        /// <summary>Retiro por quien la propuso. Baja lógica: las reacciones se conservan.</summary>
        Resultado RetirarIniciativaPropia(int codigoUsuario, int codigoIniciativa);

        /// <summary>
        /// Listado para moderación. <paramref name="estado"/> acepta
        /// «Activas», «Retiradas» o vacío para todas.
        /// </summary>
        IList<Iniciativa> ObtenerIniciativasAdmin(int codigoUsuario, string estado);

        /// <summary>Retira o restaura desde administración, con motivo obligatorio.</summary>
        Resultado ModerarIniciativa(int codigoUsuario, int codigoIniciativa, bool activo, string motivo);

        // ------------------------------------------ Panel de la candidatura

        /* El proyecto de campaña y el perfil público. La candidatura no
           viaja: el backend la deriva de la cuenta. */

        /// <summary>
        /// Qué puede cambiar la candidatura. Con propuesta en cero responde
        /// por el alta y por el perfil.
        /// </summary>
        EdicionPanel ObtenerEdicionPanel(int codigoUsuario, int codigoPropuesta);

        /// <summary>
        /// Alta (código cero) o edición de un proyecto. El estado va por
        /// nombre, y solo se aceptan «Declarada» y «En proceso».
        /// </summary>
        ResultadoGuardado GuardarPropuestaPanel(int codigoUsuario, int codigoPropuesta,
            string nombre, string descripcion, string problema, string objetivo, string beneficiarios,
            int codigoCategoria, string ubicacion, string periodoEjecucion, string estado,
            string informacionAdicional);

        /// <summary>
        /// Presentación, contacto y redes. La identificación de la candidatura
        /// la registra la administración y no se guarda desde acá.
        /// </summary>
        ResultadoGuardado GuardarPerfilPanel(int codigoUsuario, string titular, string biografia,
            string informacionProfesional, string descripcionCandidatura, string correoPublico,
            string telefono, string sitioWeb, string facebook, string x, string instagram);

        // ----------------------------------------------------- Archivos

        /* Subir y mostrar un archivo va por Servicios/Archivos.cs y los
           manejadores, porque mueven bytes. Acá quedan las dos operaciones
           que no los mueven. */

        /// <summary>Documentos de respaldo activos de una propuesta.</summary>
        IList<ArchivoRespaldo> ObtenerArchivosPropuesta(int codigoPropuesta);

        /// <summary>Quita un archivo por baja lógica. Solo la candidatura dueña.</summary>
        Resultado QuitarArchivo(int codigoUsuario, int codigoArchivo);

        // ----------------------------------------------------- Analítica

        /// <summary>
        /// Tablero completo para una selección de filtros. Con la campaña
        /// vacía usa la destacada.
        ///
        /// Devuelve siempre un objeto: cuando el backend no responde llega con
        /// <c>Error</c> en verdadero, y cuando la selección no tiene registros
        /// llega con <c>SinDatos</c>. La página distingue los dos casos, porque
        /// «no hay datos» y «no se pudo consultar» piden mensajes distintos.
        /// </summary>
        Analitica ObtenerAnalitica(FiltroAnalitica filtro);

        // ------------------------------------------------ Administración

        /* Todo lo de esta sección exige rol Administrador. El código de usuario
           viaja como parámetro y el backend confirma el rol contra la base: lo
           que el frontend sabe de su sesión decide qué se muestra, nunca qué se
           permite. Cuando la cuenta no tiene el rol, las consultas devuelven
           listas vacías y las acciones un Resultado en falso. */

        /// <summary>
        /// Cola de trabajo de la verificación, con candidaturas, propuestas y
        /// publicaciones juntas. Con <paramref name="soloPendientes"/> deja
        /// fuera lo ya verificado.
        /// </summary>
        IList<ItemVerificacion> ObtenerBandejaVerificacion(
            int codigoUsuario, string tipoObjeto, string campanaSlug, bool soloPendientes);

        /// <summary>
        /// Asigna el nivel de verificación. El motivo es donde queda anotada la
        /// fuente, y el backend lo exige al marcar como verificado.
        /// </summary>
        Resultado CambiarVerificacion(
            int codigoUsuario, string tipoObjeto, int codigoObjeto,
            int codigoVerificacion, string motivo);

        /// <summary>
        /// Publicaciones para moderación, incluidas las retiradas.
        /// <paramref name="estado"/> acepta Activas, Retiradas o vacío.
        /// </summary>
        IList<PublicacionModerada> ObtenerPublicacionesModeracion(
            int codigoUsuario, string campanaSlug, string estado);

        /// <summary>
        /// Retira una publicación de la consulta pública, o la restaura. Es una
        /// baja lógica: la fila se conserva con su motivo y su responsable.
        /// </summary>
        Resultado ModerarPublicacion(
            int codigoUsuario, int codigoPublicacion, bool activa, string motivo);

        /// <summary>Bitácora de administración, de lo más reciente a lo más antiguo.</summary>
        IList<RegistroAuditoria> ObtenerAuditoria(int codigoUsuario, string accion, int limite);

        // --------------------------------------------------- Catálogos admin

        /* Partidos, campañas y candidaturas. Nada se borra: los partidos y las
           candidaturas se desactivan, y las campañas se cierran con su estado.
           Borrar un partido dejaría candidaturas huérfanas. */

        IList<PartidoAdmin> ObtenerPartidosAdmin(int codigoUsuario, bool soloActivos);

        /// <summary>Alta si el código es cero, edición si no.</summary>
        ResultadoGuardado GuardarPartido(
            int codigoUsuario, int codigoPartido, string nombre, string siglas, string descripcion);

        Resultado CambiarEstadoPartido(int codigoUsuario, int codigoPartido, bool activo, string motivo);

        IList<CampanaAdmin> ObtenerCampanasAdmin(int codigoUsuario);

        ResultadoGuardado GuardarCampana(
            int codigoUsuario, int codigoCampana, string nombre, string resumen,
            string descripcion, string alcance, DateTime fechaInicio, DateTime fechaEleccion,
            string estado, bool esActual);

        IList<CandidatoAdmin> ObtenerCandidatosAdmin(
            int codigoUsuario, string campanaSlug, bool soloActivos);

        ResultadoGuardado GuardarCandidato(
            int codigoUsuario, int codigoCandidato, string nombres, string apellidos,
            int codigoCampana, int codigoCargo, int codigoPartido, int codigoDepartamento,
            string municipio, string titular);

        Resultado CambiarEstadoCandidato(
            int codigoUsuario, int codigoCandidato, bool activo, string motivo);

        /// <summary>
        /// Crea la cuenta de acceso de una candidatura. Sin cuenta, la
        /// candidatura solo existe como ficha que alguien más llenó.
        /// </summary>
        Resultado CrearCuentaCandidato(
            int codigoUsuario, int codigoCandidato, string login, string correo, string clave);

        /* Cargos del espacio. Son de cada espacio (script 23): la plataforma
           tiene los de elección popular y una organización crea los suyos. */
        IList<CargoAdmin> ObtenerCargosAdmin(int codigoUsuario);
        ResultadoGuardado GuardarCargo(int codigoUsuario, int codigoCargo, string nombre, string nivelGobierno, int orden);
        Resultado CambiarEstadoCargo(int codigoUsuario, int codigoCargo, bool activo, string motivo);

        // ------------------------------------------------- Encuestas admin

        /// <summary>
        /// Todas las encuestas, incluidas las retiradas y las que todavía no
        /// abren. <paramref name="estado"/> acepta uno de los cuatro estados o
        /// vacío para todos.
        /// </summary>
        IList<EncuestaAdmin> ObtenerEncuestasAdmin(int codigoUsuario, string campanaSlug, string estado);

        /// <summary>
        /// Opciones de una encuesta con su conteo sin reservar. Es lo que
        /// necesita quien administra para decidir si la cierra.
        /// </summary>
        IList<OpcionEncuesta> ObtenerOpcionesEncuestaAdmin(int codigoUsuario, int codigoEncuesta);

        /// <summary>
        /// Alta si el código es cero, edición si no. Las opciones van en un
        /// solo texto, una por línea, y las separa el procedimiento almacenado.
        ///
        /// Con votos ya registrados, la pregunta y las fechas se pueden
        /// corregir pero las opciones no: cambiarlas dejaría respuestas
        /// apuntando a una pregunta que ya no es la que se respondió.
        /// </summary>
        ResultadoGuardado GuardarEncuesta(
            int codigoUsuario, int codigoEncuesta, int codigoCampana,
            string pregunta, string descripcion, int codigoCategoria,
            DateTime fechaInicio, string fechaCierre, string opciones);

        /// <summary>
        /// Cierra, reabre, retira o restaura una encuesta. Cerrar termina la
        /// votación y deja el resultado a la vista, retirar la saca del sitio
        /// público. Las dos exigen motivo.
        /// </summary>
        Resultado CambiarEstadoEncuesta(
            int codigoUsuario, int codigoEncuesta, string accion, string motivo);

        /// <summary>Cargos con su código, para el formulario de candidaturas.</summary>
        IList<OpcionCatalogo> ObtenerCargosConCodigo();

        IList<OpcionCatalogo> ObtenerDepartamentosConCodigo();

        /// <summary>
        /// Categorías con su código, que es lo que necesita el formulario de
        /// encuestas. Las páginas públicas usan la versión de solo nombre
        /// porque filtran por texto.
        /// </summary>
        IList<OpcionCatalogo> ObtenerCategoriasConCodigo();

        // ------------------------------------------------------- Espacios

        /* Un espacio es un cliente que usa la plataforma para su propio
           proceso electoral. La ficha es pública: la plantilla la necesita
           para decir de quién es lo que se ve. Crear, editar y retirar
           espacios, y crear sus cuentas, es solo de la plataforma.

           El resto del contrato no recibe el espacio. La implementación lo
           toma de la ruta (Espacios.SlugActual) para las consultas públicas y
           de la sesión (Sesion.CodigoEspacio) para las de administración, de
           modo que una página no pueda olvidarse de acotarse al suyo. */
        Espacio ObtenerEspacio(string slug);
        IList<Espacio> ObtenerEspacios(int codigoUsuario, bool soloActivos);
        ResultadoGuardado GuardarEspacio(
            int codigoUsuario, int codigoEspacio, string nombre, string organizacion,
            string descripcion, bool padronCerrado, string terminoAgrupacion);
        Resultado CambiarEstadoEspacio(int codigoUsuario, int codigoEspacio, bool activo, string motivo);
        Resultado CrearCuentaEspacio(
            int codigoUsuario, int codigoEspacio, string login, string nombre, string correo, string clave);

        /* El padrón: quién puede participar en un espacio con padrón cerrado.
           Correos, no cuentas, y baja lógica. */
        IList<Miembro> ObtenerPadron(int codigoUsuario, int codigoEspacio);
        ResultadoPadron CargarPadron(int codigoUsuario, int codigoEspacio, string correos);
        Resultado CambiarEstadoMiembro(int codigoUsuario, int codigoMiembro, bool activo);

        /* Suscripciones: la vigencia del espacio y los pagos que la sostienen.
           Registrar un pago es solo de la plataforma. No hay pasarela. */
        IList<Suscripcion> ObtenerSuscripciones(int codigoUsuario, int codigoEspacio);
        ResultadoGuardado RegistrarPago(
            int codigoUsuario, int codigoEspacio, string plan, DateTime vigenteDesde, DateTime vigenteHasta,
            decimal monto, string moneda, string referencia, string notas, int codigoPlan, int maxMiembros);

        /* La oferta y las solicitudes. Los planes son públicos (la página
           para organizaciones los muestra desde la base). Enviar una solicitud
           no exige cuenta. La bandeja y su atención son solo de la plataforma. */
        IList<Plan> ObtenerPlanes();
        ResultadoGuardado EnviarSolicitud(
            string organizacion, string contacto, string correo, string telefono,
            int codigoPlan, string proceso, string fechaAproximada, string mensaje);
        IList<Solicitud> ObtenerSolicitudes(int codigoUsuario, string estado);
        Resultado AtenderSolicitud(int codigoUsuario, int codigoSolicitud, string estado, int codigoEspacio, string notas);


        // ------------------------------------------------------- Módulos

        /// <summary>
        /// Estado de cada elemento apagable del sitio. No exige rol: lo
        /// consulta cada página en cada carga, también las de un visitante
        /// anónimo.
        /// </summary>
        IList<EstadoModulo> ObtenerModulosVisibles();

        IList<ModuloAdmin> ObtenerModulosAdmin(int codigoUsuario);

        /// <summary>
        /// Oculta un elemento del sitio público o lo devuelve a la vista.
        /// Ocultar exige motivo, encender no.
        /// </summary>
        Resultado CambiarEstadoModulo(int codigoUsuario, string clave, bool habilitado, string motivo);

        // ------------------------------------------------------ Asistente

        /// <summary>
        /// Responde una pregunta en lenguaje natural sobre lo registrado en
        /// la plataforma.
        ///
        /// El código de usuario viaja porque el backend comprueba con él la
        /// sesión y la cuota diaria, y porque deja la consulta registrada. La
        /// página no puede decidir ninguna de esas tres cosas: lo que el
        /// frontend sabe de su sesión decide qué muestra, nunca qué se
        /// permite.
        /// </summary>
        RespuestaAsistente PreguntarAsistente(int codigoUsuario, string pregunta,
                                              string campanaSlug);

        /// <summary>
        /// Las consultas respondidas de una persona, de la más reciente a la
        /// más antigua y sin la respuesta, para elegir cuáles van al reporte.
        /// </summary>
        IList<ConsultaAsistente> ObtenerConsultasAsistente(int codigoUsuario);

        /// <summary>
        /// Las consultas elegidas, completas y en orden cronológico. La base
        /// descarta las que no son de esta persona.
        /// </summary>
        IList<ConsultaAsistente> ObtenerReporteAsistente(int codigoUsuario, IList<int> codigos);

        // ------------------------------------------------------ Catálogos

        /// <summary>Categorías temáticas disponibles para clasificar propuestas.</summary>
        IList<string> ObtenerCategorias();

        /// <summary>Cargos de elección popular a los que puede aspirar un candidato.</summary>
        IList<string> ObtenerCargos();

        IList<string> ObtenerDepartamentos();

        /// <summary>
        /// Niveles de verificación con su código, que es lo que necesita el
        /// desplegable de la bandeja. Los otros catálogos devuelven solo el
        /// nombre porque las páginas públicas filtran por texto.
        /// </summary>
        IList<OpcionCatalogo> ObtenerNivelesVerificacion();
    }
}
