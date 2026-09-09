using System;
using System.Collections.Generic;
using System.Linq;
using frontend.Modelos;

namespace frontend.Servicios
{
    /// <summary>
    /// Implementación de <see cref="IContenidoServicio"/> con datos en memoria.
    ///
    /// DATOS DE DEMOSTRACIÓN. Sirven para validar el diseño y la navegación
    /// mientras el backend ASMX y la base de datos se construyen. Se reemplaza
    /// por completo, sin tocar las páginas.
    ///
    /// Los candidatos y los partidos son ficticios a propósito: la plataforma es
    /// neutral y no debe insinuar afiliaciones de personas reales con contenido
    /// que nadie declaró.
    /// </summary>
    public class ContenidoDemo : IContenidoServicio
    {
        private readonly List<Campana> _campanas;
        private readonly List<Candidato> _candidatos;
        private readonly List<Propuesta> _propuestas;
        private readonly List<Publicacion> _publicaciones;

        public ContenidoDemo()
        {
            _campanas = ConstruirCampanas();
            _candidatos = ConstruirCandidatos();
            _propuestas = ConstruirPropuestas();
            _publicaciones = ConstruirPublicaciones();

            RecalcularTotales();
        }

        // =============================================================
        //  Campañas
        // =============================================================

        public IList<Campana> ObtenerCampanas()
        {
            return _campanas
                .OrderBy(c => c.Estado == EstadoCampana.Activa ? 0 : c.Estado == EstadoCampana.Proxima ? 1 : 2)
                .ThenByDescending(c => c.FechaEleccion)
                .ToList();
        }

        public Campana ObtenerCampanaActual()
        {
            return _campanas.FirstOrDefault(c => c.EsActual);
        }

        public Campana ObtenerCampana(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return ObtenerCampanaActual();
            return _campanas.FirstOrDefault(c => c.Slug == slug);
        }

        // =============================================================
        //  Candidatos
        // =============================================================

        public IList<Candidato> ObtenerCandidatos(string campanaSlug)
        {
            IEnumerable<Candidato> q = _candidatos;

            if (!string.IsNullOrEmpty(campanaSlug))
                q = q.Where(c => c.CampanaSlug == campanaSlug);

            return q.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToList();
        }

        public Candidato ObtenerCandidato(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;
            return _candidatos.FirstOrDefault(c => c.Slug == slug);
        }

        public Candidato ObtenerCandidatoAutenticado()
        {
            // Sustituto de la sesión mientras no exista autenticación real.
            return _candidatos.FirstOrDefault(c => c.Slug == "ana-molina-caballero");
        }

        // =============================================================
        //  Propuestas
        // =============================================================

        public IList<Propuesta> ObtenerPropuestas(string candidatoSlug)
        {
            if (string.IsNullOrEmpty(candidatoSlug)) return new List<Propuesta>();

            return _propuestas
                .Where(p => p.CandidatoSlug == candidatoSlug)
                .OrderByDescending(p => p.FechaRegistro)
                .ToList();
        }

        public IList<Propuesta> ObtenerPropuestasDeCampana(string campanaSlug)
        {
            IEnumerable<Propuesta> q = _propuestas;

            if (!string.IsNullOrEmpty(campanaSlug))
                q = q.Where(p => p.CampanaSlug == campanaSlug);

            return q.OrderByDescending(p => p.FechaRegistro).ToList();
        }

        public Propuesta ObtenerPropuesta(int id)
        {
            return _propuestas.FirstOrDefault(p => p.Id == id);
        }

        // =============================================================
        //  Publicaciones
        // =============================================================

        public IList<Publicacion> ObtenerFeed(string campanaSlug)
        {
            IEnumerable<Publicacion> q = _publicaciones;

            if (!string.IsNullOrEmpty(campanaSlug))
                q = q.Where(p => p.CampanaSlug == campanaSlug);

            return q.OrderByDescending(p => p.Fecha).ToList();
        }

        public IList<Publicacion> ObtenerPublicaciones(string candidatoSlug)
        {
            if (string.IsNullOrEmpty(candidatoSlug)) return new List<Publicacion>();

            return _publicaciones
                .Where(p => p.CandidatoSlug == candidatoSlug)
                .OrderByDescending(p => p.Fecha)
                .ToList();
        }

        // =============================================================
        //  Partidos e interacción
        // =============================================================

        /* Esta implementación existe para trabajar el diseño visual sin
           levantar el backend ni la base de datos. La participación ciudadana
           necesita cuentas y persistencia, así que acá se devuelve vacía en
           lugar de simularla: un like que no se guarda en ningún lado sería
           más confuso que no tenerlo. Con el backend en marcha estos métodos
           los atiende ContenidoServicio. */

        public IList<Partido> ObtenerPartidos()
        {
            List<Partido> lista = new List<Partido>();

            foreach (string nombre in _candidatos
                .Where(c => !string.IsNullOrEmpty(c.Partido))
                .Select(c => c.Partido)
                .Distinct()
                .OrderBy(n => n))
            {
                lista.Add(new Partido
                {
                    Id = lista.Count + 1,
                    Slug = nombre.ToLowerInvariant().Replace(' ', '-'),
                    Nombre = nombre,
                    TotalCandidatos = _candidatos.Count(c => c.Partido == nombre)
                });
            }

            return lista;
        }

        public Partido ObtenerPartido(string slug)
        {
            return ObtenerPartidos().FirstOrDefault(p => p.Slug == slug);
        }

        public IList<Candidato> ObtenerCandidatosDePartido(string partidoSlug)
        {
            Partido p = ObtenerPartido(partidoSlug);
            if (p == null) return new List<Candidato>();

            return _candidatos.Where(c => c.Partido == p.Nombre).ToList();
        }

        public Interaccion ObtenerInteraccion(string tipoObjeto, int codigoObjeto, int codigoUsuario)
        {
            return new Interaccion { TipoObjeto = tipoObjeto, CodigoObjeto = codigoObjeto };
        }

        public Resultado Valorar(string tipoObjeto, int codigoObjeto, int codigoUsuario, int valor)
        {
            return new Resultado
            {
                Ok = false,
                Mensaje = "La participación requiere el backend en marcha."
            };
        }

        public IList<Comentario> ObtenerComentarios(string tipoObjeto, int codigoObjeto)
        {
            return new List<Comentario>();
        }

        public Resultado AgregarComentario(string tipoObjeto, int codigoObjeto,
            int codigoUsuario, string texto)
        {
            return new Resultado
            {
                Ok = false,
                Mensaje = "La participación requiere el backend en marcha."
            };
        }

        /// <summary>
        /// El tablero se calcula con vistas de la base de datos, así que en modo
        /// de demostración se devuelve vacío en lugar de inventar cifras. Un
        /// indicador con números falsos es peor que un tablero en blanco.
        /// </summary>
        /// <summary>
        /// El tablero no tiene versión de demostración. Sus indicadores son
        /// agregaciones sobre la base completa, y reproducirlas en memoria
        /// daría cifras que no corresponden a ningún registro real — justo lo
        /// que el módulo existe para evitar.
        ///
        /// Se devuelve el estado vacío, que la página ya sabe mostrar.
        /// </summary>
        public Analitica ObtenerAnalitica(FiltroAnalitica filtro)
        {
            return new Analitica { SinDatos = true };
        }

        // =============================================================
        //  Asistente
        // =============================================================

        /// <summary>
        /// El asistente no tiene versión de demostración, y es a propósito.
        ///
        /// Esta clase existe para trabajar el diseño sin levantar el backend.
        /// Devolver acá una respuesta inventada sería exactamente lo que el
        /// asistente tiene prohibido hacer, y además haría creer que funciona
        /// mientras se revisa la maqueta. Dice lo que pasa y ya.
        /// </summary>
        public RespuestaAsistente PreguntarAsistente(
            int codigoUsuario, string pregunta, string campanaSlug)
        {
            return new RespuestaAsistente
            {
                Ok = false,
                Mensaje = "El asistente necesita el backend en marcha. "
                        + "Los datos de demostración no lo incluyen."
            };
        }

        // =============================================================
        //  Catálogos
        // =============================================================

        /// <summary>
        /// Taxonomía temática derivada de la Clasificación de las Funciones del
        /// Gobierno (ONU, 2000). El orden refleja el interés medido en la
        /// encuesta del proyecto: seguridad, salud y educación al frente.
        /// </summary>

        // =============================================================
        //  Administración
        //
        //  El origen de demostración no la implementa. Administrar significa
        //  escribir, y estos datos viven en memoria: se pierden al reciclar el
        //  proceso, así que una verificación hecha acá sería una verificación
        //  que se deshace sola. Peor que no poder hacerla.
        //
        //  Las consultas devuelven vacío y las acciones lo dicen en su mensaje,
        //  para que la pantalla explique la situación en vez de fallar.
        // =============================================================

        public IList<ItemVerificacion> ObtenerBandejaVerificacion(
            int codigoUsuario, string tipoObjeto, string campanaSlug, bool soloPendientes)
        {
            return new List<ItemVerificacion>();
        }

        public Resultado CambiarVerificacion(
            int codigoUsuario, string tipoObjeto, int codigoObjeto,
            int codigoVerificacion, string motivo)
        {
            return SinAdministracion();
        }

        public IList<PublicacionModerada> ObtenerPublicacionesModeracion(
            int codigoUsuario, string campanaSlug, string estado)
        {
            return new List<PublicacionModerada>();
        }

        public Resultado ModerarPublicacion(
            int codigoUsuario, int codigoPublicacion, bool activa, string motivo)
        {
            return SinAdministracion();
        }

        public IList<RegistroAuditoria> ObtenerAuditoria(int codigoUsuario, string accion, int limite)
        {
            return new List<RegistroAuditoria>();
        }

        public IList<OpcionCatalogo> ObtenerNivelesVerificacion()
        {
            return new List<OpcionCatalogo>
            {
                new OpcionCatalogo { Codigo = 1, Nombre = NivelesVerificacion.Declarado },
                new OpcionCatalogo { Codigo = 2, Nombre = NivelesVerificacion.EnRevision },
                new OpcionCatalogo { Codigo = 3, Nombre = NivelesVerificacion.Verificado }
            };
        }


        // ------------------------------------------------- Catálogos admin

        public IList<PartidoAdmin> ObtenerPartidosAdmin(int codigoUsuario, bool soloActivos)
        {
            return new List<PartidoAdmin>();
        }

        public ResultadoGuardado GuardarPartido(
            int codigoUsuario, int codigoPartido, string nombre, string siglas, string descripcion)
        {
            return SinAdministracionGuardado();
        }

        public Resultado CambiarEstadoPartido(
            int codigoUsuario, int codigoPartido, bool activo, string motivo)
        {
            return SinAdministracion();
        }

        public IList<CampanaAdmin> ObtenerCampanasAdmin(int codigoUsuario)
        {
            return new List<CampanaAdmin>();
        }

        public ResultadoGuardado GuardarCampana(
            int codigoUsuario, int codigoCampana, string nombre, string resumen,
            string descripcion, string alcance, DateTime fechaInicio, DateTime fechaEleccion,
            string estado, bool esActual)
        {
            return SinAdministracionGuardado();
        }

        public IList<CandidatoAdmin> ObtenerCandidatosAdmin(
            int codigoUsuario, string campanaSlug, bool soloActivos)
        {
            return new List<CandidatoAdmin>();
        }

        public ResultadoGuardado GuardarCandidato(
            int codigoUsuario, int codigoCandidato, string nombres, string apellidos,
            int codigoCampana, int codigoCargo, int codigoPartido, int codigoDepartamento,
            string municipio, string titular)
        {
            return SinAdministracionGuardado();
        }

        public Resultado CambiarEstadoCandidato(
            int codigoUsuario, int codigoCandidato, bool activo, string motivo)
        {
            return SinAdministracion();
        }

        public Resultado CrearCuentaCandidato(
            int codigoUsuario, int codigoCandidato, string login, string correo, string clave)
        {
            return SinAdministracion();
        }

        public IList<OpcionCatalogo> ObtenerCargosConCodigo()
        {
            return new List<OpcionCatalogo>();
        }

        public IList<OpcionCatalogo> ObtenerDepartamentosConCodigo()
        {
            return new List<OpcionCatalogo>();
        }

        public IList<OpcionCatalogo> ObtenerCategoriasConCodigo()
        {
            return new List<OpcionCatalogo>();
        }

        private static ResultadoGuardado SinAdministracionGuardado()
        {
            Resultado r = SinAdministracion();
            return new ResultadoGuardado { Ok = r.Ok, Mensaje = r.Mensaje, Codigo = 0 };
        }


        // ------------------------------------------------------- Módulos

        /// <summary>
        /// Sin base de datos no hay catálogo de módulos, y una lista vacía deja
        /// todo visible: es lo que corresponde para trabajar en el diseño.
        /// </summary>
        // =============================================================
        //  Encuestas
        //
        //  Los datos en memoria no incluyen encuestas. Devolver la lista vacía
        //  es lo correcto: la portada ya sabe esconder el bloque cuando no hay
        //  ninguna abierta, así que este origen se ve como un sitio sin
        //  encuesta en curso y no como uno roto.
        // =============================================================

        public IList<Encuesta> ObtenerEncuestasVigentes(string campanaSlug, int codigoUsuario)
        {
            return new List<Encuesta>();
        }

        public ResultadoEncuesta ResponderEncuesta(
            int codigoEncuesta, int codigoOpcion, int codigoUsuario)
        {
            return new ResultadoEncuesta
            {
                Ok = false,
                Mensaje = "Las encuestas necesitan la base de datos. "
                        + "Este origen de datos es solo de demostración."
            };
        }

        public IList<EncuestaAdmin> ObtenerEncuestasAdmin(
            int codigoUsuario, string campanaSlug, string estado)
        {
            return new List<EncuestaAdmin>();
        }

        public IList<OpcionEncuesta> ObtenerOpcionesEncuestaAdmin(
            int codigoUsuario, int codigoEncuesta)
        {
            return new List<OpcionEncuesta>();
        }

        public ResultadoGuardado GuardarEncuesta(
            int codigoUsuario, int codigoEncuesta, int codigoCampana,
            string pregunta, string descripcion, int codigoCategoria,
            DateTime fechaInicio, string fechaCierre, string opciones)
        {
            return SinAdministracionGuardado();
        }

        public Resultado CambiarEstadoEncuesta(
            int codigoUsuario, int codigoEncuesta, string accion, string motivo)
        {
            return SinAdministracion();
        }

        public IList<EstadoModulo> ObtenerModulosVisibles()
        {
            return new List<EstadoModulo>();
        }

        public IList<ModuloAdmin> ObtenerModulosAdmin(int codigoUsuario)
        {
            return new List<ModuloAdmin>();
        }

        public Resultado CambiarEstadoModulo(
            int codigoUsuario, string clave, bool habilitado, string motivo)
        {
            return SinAdministracion();
        }

        private static Resultado SinAdministracion()
        {
            return new Resultado
            {
                Ok = false,
                Mensaje = "La administración necesita la base de datos. " +
                          "Este origen de datos es solo de demostración."
            };
        }

        public IList<string> ObtenerCategorias()
        {
            return new List<string>
            {
                "Seguridad y orden público",
                "Salud",
                "Educación",
                "Economía y empleo",
                "Infraestructura",
                "Transparencia y gobernanza"
            };
        }

        public IList<string> ObtenerCargos()
        {
            return new List<string>
            {
                "Presidencia de la República",
                "Designación presidencial",
                "Diputación al Congreso Nacional",
                "Diputación al Parlamento Centroamericano",
                "Alcaldía municipal",
                "Regiduría municipal"
            };
        }

        public IList<string> ObtenerDepartamentos()
        {
            return new List<string>
            {
                "Atlántida", "Choluteca", "Colón", "Comayagua", "Copán", "Cortés",
                "El Paraíso", "Francisco Morazán", "Gracias a Dios", "Intibucá",
                "Islas de la Bahía", "La Paz", "Lempira", "Ocotepeque", "Olancho",
                "Santa Bárbara", "Valle", "Yoro"
            };
        }

        // =============================================================
        //  Semillas
        // =============================================================

        private static List<Campana> ConstruirCampanas()
        {
            return new List<Campana>
            {
                new Campana
                {
                    Slug = "generales-2029",
                    Nombre = "Elecciones Generales 2029",
                    Resumen = "Presidencia de la República, Congreso Nacional y corporaciones municipales.",
                    Descripcion =
                        "Proceso electoral en el que se eligen la Presidencia de la República, las 128 diputaciones " +
                        "al Congreso Nacional, las diputaciones al Parlamento Centroamericano y las 298 corporaciones " +
                        "municipales del país. En CumpleHN el registro de candidaturas y el seguimiento de propuestas " +
                        "permanecen abiertos durante todo el ciclo, de modo que los compromisos queden documentados " +
                        "desde el momento en que se formulan y no solo al final de la campaña.",
                    Alcance = "Nacional, departamental y municipal",
                    FechaInicio = new DateTime(2026, 3, 1),
                    FechaEleccion = new DateTime(2029, 11, 25),
                    Estado = EstadoCampana.Activa,
                    EsActual = true
                },
                new Campana
                {
                    Slug = "primarias-2029",
                    Nombre = "Elecciones Primarias 2029",
                    Resumen = "Elección interna de las candidaturas de cada partido político.",
                    Descripcion =
                        "Proceso interno mediante el cual cada partido político define las candidaturas que " +
                        "presentará en las elecciones generales. El registro de precandidaturas en la plataforma " +
                        "abre con la convocatoria oficial.",
                    Alcance = "Interno de cada partido político",
                    FechaInicio = new DateTime(2028, 9, 3),
                    FechaEleccion = new DateTime(2029, 3, 11),
                    Estado = EstadoCampana.Proxima
                },
                new Campana
                {
                    Slug = "generales-2025",
                    Nombre = "Elecciones Generales 2025",
                    Resumen = "Ciclo cerrado. Disponible para consultar el cumplimiento de lo prometido.",
                    Descripcion =
                        "Ciclo electoral concluido. Su valor en la plataforma es el seguimiento: las propuestas " +
                        "registradas durante la campaña quedan disponibles para contrastarlas con lo que " +
                        "efectivamente se ejecutó, con las evidencias que respaldan cada estado de cumplimiento.",
                    Alcance = "Nacional, departamental y municipal",
                    FechaInicio = new DateTime(2025, 1, 20),
                    FechaEleccion = new DateTime(2025, 11, 30),
                    Estado = EstadoCampana.Cerrada
                }
            };
        }

        private static List<Candidato> ConstruirCandidatos()
        {
            const string campana = "generales-2029";

            return new List<Candidato>
            {
                new Candidato
                {
                    Slug = "ana-molina-caballero",
                    CampanaSlug = campana,
                    Nombres = "Ana Lucía",
                    Apellidos = "Molina Caballero",
                    Partido = "Movimiento Cívico Nacional",
                    PartidoSiglas = "MCN",
                    Cargo = "Presidencia de la República",
                    Nivel = NivelGobierno.Nacional,
                    Departamento = "Francisco Morazán",
                    Titular = "Gestión pública abierta y datos verificables como política de Estado.",
                    Biografia =
                        "Economista con veinte años de trayectoria en administración pública y evaluación de " +
                        "programas sociales. Ha coordinado unidades de planificación en el nivel central y " +
                        "acompañado procesos de presupuesto abierto en gobiernos locales.",
                    InformacionProfesional =
                        "Licenciatura en Economía. Maestría en Políticas Públicas. Docente universitaria en " +
                        "evaluación de programas.",
                    DescripcionCandidatura =
                        "Una candidatura centrada en que cada compromiso público tenga una meta medible, un " +
                        "responsable con nombre y un plazo verificable, publicados desde el primer día.",
                    CorreoPublico = "contacto@analuciamolina.hn",
                    SitioWeb = "https://analuciamolina.hn",
                    Facebook = "analuciamolinahn",
                    X = "analuciamolina",
                    Verificacion = NivelVerificacion.Verificado
                },
                new Candidato
                {
                    Slug = "rodrigo-nunez-berrios",
                    CampanaSlug = campana,
                    Nombres = "Rodrigo",
                    Apellidos = "Núñez Berríos",
                    Partido = "Partido Progreso Ciudadano",
                    PartidoSiglas = "PPC",
                    Cargo = "Presidencia de la República",
                    Nivel = NivelGobierno.Nacional,
                    Departamento = "Cortés",
                    Titular = "Empleo formal, seguridad en los barrios y salud que no se caiga a pedazos.",
                    Biografia =
                        "Ingeniero civil y empresario del sector construcción. Fue presidente de una cámara " +
                        "de comercio local y participó en mesas de seguridad ciudadana en el norte del país.",
                    InformacionProfesional = "Ingeniería Civil. Especialización en gestión de proyectos.",
                    DescripcionCandidatura =
                        "Propuestas medibles en generación de empleo formal, reducción de homicidios y " +
                        "abastecimiento hospitalario, con metas anuales publicadas.",
                    CorreoPublico = "prensa@rodrigonunez.hn",
                    Facebook = "rodrigonunezhn",
                    Verificacion = NivelVerificacion.Verificado
                },
                new Candidato
                {
                    Slug = "carmen-villeda-lopez",
                    CampanaSlug = campana,
                    Nombres = "Carmen",
                    Apellidos = "Villeda López",
                    Partido = "Alianza Democrática Hondureña",
                    PartidoSiglas = "ADH",
                    Cargo = "Alcaldía municipal",
                    Nivel = NivelGobierno.Municipal,
                    Departamento = "Francisco Morazán",
                    Municipio = "Distrito Central",
                    Titular = "La ciudad se arregla cuadra por cuadra, con presupuesto a la vista.",
                    Biografia =
                        "Arquitecta y gestora urbana. Trabajó doce años en planificación municipal y en " +
                        "programas de mejoramiento de barrios en la capital.",
                    InformacionProfesional = "Arquitectura. Maestría en Planificación Urbana.",
                    DescripcionCandidatura =
                        "Un plan de ciudad con obras georreferenciadas, costos publicados y avance " +
                        "actualizado cada trimestre.",
                    CorreoPublico = "carmen@villedaparalaciudad.hn",
                    Instagram = "villedaparalaciudad",
                    Verificacion = NivelVerificacion.EnRevision
                },
                new Candidato
                {
                    Slug = "hector-paz-mejia",
                    CampanaSlug = campana,
                    Nombres = "Héctor",
                    Apellidos = "Paz Mejía",
                    Cargo = "Diputación al Congreso Nacional",
                    Nivel = NivelGobierno.Departamental,
                    Departamento = "Francisco Morazán",
                    Titular = "Candidatura independiente por la transparencia legislativa.",
                    Biografia =
                        "Abogado especializado en derecho administrativo. Ha litigado solicitudes de acceso " +
                        "a la información pública y acompañado a organizaciones civiles en auditorías sociales.",
                    InformacionProfesional = "Licenciatura en Derecho. Especialidad en Derecho Administrativo.",
                    DescripcionCandidatura =
                        "Voto público razonado, agenda legislativa abierta y rendición de cuentas del uso " +
                        "del fondo departamental.",
                    CorreoPublico = "hector.paz@independiente.hn",
                    Verificacion = NivelVerificacion.Declarado
                },
                new Candidato
                {
                    Slug = "gabriela-fonseca-ordonez",
                    CampanaSlug = campana,
                    Nombres = "Gabriela",
                    Apellidos = "Fonseca Ordóñez",
                    Partido = "Frente Renovador",
                    PartidoSiglas = "FR",
                    Cargo = "Alcaldía municipal",
                    Nivel = NivelGobierno.Municipal,
                    Departamento = "Cortés",
                    Municipio = "San Pedro Sula",
                    Titular = "Agua potable, drenaje y escuelas dignas antes que obra vistosa.",
                    Biografia =
                        "Médica salubrista con experiencia en atención primaria y coordinación de brigadas " +
                        "en comunidades del valle de Sula.",
                    InformacionProfesional = "Doctorado en Medicina. Maestría en Salud Pública.",
                    DescripcionCandidatura =
                        "Prioridad al saneamiento básico, la red de agua y la infraestructura escolar, con " +
                        "indicadores de cobertura publicados por colonia.",
                    CorreoPublico = "contacto@gabrielafonseca.hn",
                    Facebook = "gabrielafonsecahn",
                    X = "gfonsecahn",
                    Verificacion = NivelVerificacion.Verificado
                },
                new Candidato
                {
                    Slug = "marlon-castellanos-rivas",
                    CampanaSlug = campana,
                    Nombres = "Marlon",
                    Apellidos = "Castellanos Rivas",
                    Partido = "Partido Progreso Ciudadano",
                    PartidoSiglas = "PPC",
                    Cargo = "Diputación al Congreso Nacional",
                    Nivel = NivelGobierno.Departamental,
                    Departamento = "Choluteca",
                    Titular = "El sur también cuenta: riego, carretera y empleo joven.",
                    Biografia =
                        "Ingeniero agrónomo. Dirigió cooperativas de productores en la zona sur y programas " +
                        "de tecnificación de riego.",
                    InformacionProfesional = "Ingeniería Agronómica. Diplomado en Desarrollo Rural.",
                    DescripcionCandidatura =
                        "Agenda legislativa enfocada en infraestructura productiva del sur y en empleo " +
                        "para población joven rural.",
                    CorreoPublico = "marlon@castellanosporelsur.hn",
                    Verificacion = NivelVerificacion.Declarado
                }
            };
        }

        private static List<Propuesta> ConstruirPropuestas()
        {
            const string campana = "generales-2029";
            DateTime hoy = DateTime.Now;

            return new List<Propuesta>
            {
                new Propuesta
                {
                    Id = 1,
                    CandidatoSlug = "ana-molina-caballero",
                    CampanaSlug = campana,
                    Nombre = "Portal de compromisos con metas medibles",
                    Descripcion =
                        "Publicar en un portal único todos los compromisos del plan de gobierno con su meta " +
                        "cuantitativa, la institución responsable, el presupuesto asignado y el avance " +
                        "actualizado cada trimestre.",
                    Problema =
                        "Los compromisos públicos se anuncian sin meta ni responsable, lo que hace imposible " +
                        "verificar después si se cumplieron.",
                    Objetivo =
                        "Que el 100 % de los compromisos del plan de gobierno tenga meta, responsable, " +
                        "presupuesto y plazo publicados durante el primer año de gestión.",
                    Beneficiarios = "Toda la ciudadanía, con énfasis en organizaciones de auditoría social",
                    Categoria = "Transparencia y gobernanza",
                    PeriodoEjecucion = "Primer año de gestión",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-42)
                },
                new Propuesta
                {
                    Id = 2,
                    CandidatoSlug = "ana-molina-caballero",
                    CampanaSlug = campana,
                    Nombre = "Abastecimiento continuo de medicamentos esenciales",
                    Descripcion =
                        "Sistema nacional de trazabilidad de inventarios que reporte en línea la existencia de " +
                        "medicamentos del listado básico en cada hospital y centro de salud.",
                    Problema =
                        "El desabastecimiento de medicamentos se detecta cuando el paciente ya está en la " +
                        "ventanilla, sin datos previos que permitan anticiparlo.",
                    Objetivo =
                        "Reducir a menos del 10 % los eventos de desabastecimiento del listado básico en " +
                        "hospitales públicos al cierre del segundo año.",
                    Beneficiarios = "Pacientes del sistema público de salud",
                    Categoria = "Salud",
                    PeriodoEjecucion = "Primeros dos años de gestión",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-38)
                },
                new Propuesta
                {
                    Id = 3,
                    CandidatoSlug = "ana-molina-caballero",
                    CampanaSlug = campana,
                    Nombre = "Presupuesto educativo con seguimiento por centro",
                    Descripcion =
                        "Asignación transparente del presupuesto de infraestructura escolar, con ficha pública " +
                        "por centro educativo que detalle obra, monto y estado de ejecución.",
                    Problema =
                        "No existe un registro público consolidado que permita saber cuánto se invirtió en " +
                        "cada centro educativo y en qué.",
                    Objetivo = "Ficha pública de inversión para el 100 % de centros educativos oficiales.",
                    Beneficiarios = "Comunidad educativa: estudiantes, docentes y familias",
                    Categoria = "Educación",
                    PeriodoEjecucion = "2030 – 2032",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-21)
                },
                new Propuesta
                {
                    Id = 4,
                    CandidatoSlug = "rodrigo-nunez-berrios",
                    CampanaSlug = campana,
                    Nombre = "Programa de primer empleo formal para jóvenes",
                    Descripcion =
                        "Incentivo temporal a la contratación formal de personas de 18 a 29 años sin " +
                        "experiencia laboral registrada, con verificación mensual de permanencia en planilla.",
                    Problema =
                        "La población joven entra al mercado laboral por la vía informal y sin cobertura de " +
                        "seguridad social.",
                    Objetivo = "Cincuenta mil nuevos empleos formales para jóvenes en cuatro años.",
                    Beneficiarios = "Personas de 18 a 29 años que buscan su primer empleo",
                    Categoria = "Economía y empleo",
                    PeriodoEjecucion = "2030 – 2033",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-33)
                },
                new Propuesta
                {
                    Id = 5,
                    CandidatoSlug = "rodrigo-nunez-berrios",
                    CampanaSlug = campana,
                    Nombre = "Plan de reducción de homicidios por municipio",
                    Descripcion =
                        "Metas de reducción diferenciadas por municipio según su tasa base, con publicación " +
                        "mensual de cifras y evaluación semestral independiente.",
                    Problema =
                        "Las metas de seguridad se anuncian a nivel nacional, lo que oculta municipios donde " +
                        "la situación empeora.",
                    Objetivo = "Reducir la tasa nacional de homicidios en un 25 % en cuatro años.",
                    Beneficiarios = "Población de los veinte municipios con mayor incidencia",
                    Categoria = "Seguridad y orden público",
                    Ubicacion = "Veinte municipios priorizados",
                    PeriodoEjecucion = "2030 – 2033",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-27)
                },
                new Propuesta
                {
                    Id = 6,
                    CandidatoSlug = "carmen-villeda-lopez",
                    CampanaSlug = campana,
                    Nombre = "Mil cuadras: repavimentación con avance público",
                    Descripcion =
                        "Intervención de mil cuadras priorizadas por estado de la vía y densidad de tránsito, " +
                        "con mapa público de avance y costo por cuadra.",
                    Problema =
                        "La obra vial se anuncia por kilómetros totales, sin que el vecino pueda saber si su " +
                        "cuadra está incluida ni cuándo.",
                    Objetivo = "Mil cuadras repavimentadas en cuatro años, con mapa de avance actualizado.",
                    Beneficiarios = "Residentes de las colonias priorizadas del Distrito Central",
                    Categoria = "Infraestructura",
                    Ubicacion = "Distrito Central, Francisco Morazán",
                    PeriodoEjecucion = "2030 – 2033",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-18)
                },
                new Propuesta
                {
                    Id = 7,
                    CandidatoSlug = "carmen-villeda-lopez",
                    CampanaSlug = campana,
                    Nombre = "Presupuesto municipal abierto y auditable",
                    Descripcion =
                        "Publicación mensual de ingresos, egresos y contrataciones de la municipalidad en " +
                        "formato de datos abiertos reutilizables.",
                    Problema =
                        "La información financiera municipal se publica en formatos que no permiten análisis " +
                        "ni comparación entre períodos.",
                    Objetivo = "Publicación mensual en datos abiertos desde el primer trimestre de gestión.",
                    Beneficiarios = "Contribuyentes del municipio y organizaciones de auditoría social",
                    Categoria = "Transparencia y gobernanza",
                    Ubicacion = "Distrito Central, Francisco Morazán",
                    PeriodoEjecucion = "Primer trimestre de gestión",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-12)
                },
                new Propuesta
                {
                    Id = 8,
                    CandidatoSlug = "gabriela-fonseca-ordonez",
                    CampanaSlug = campana,
                    Nombre = "Agua potable continua en colonias sin servicio",
                    Descripcion =
                        "Ampliación de la red de agua potable en colonias con abastecimiento intermitente, " +
                        "con medición pública de horas de servicio por sector.",
                    Problema =
                        "Buena parte de las colonias recibe agua por horas o por cisterna, sin registro " +
                        "público que permita exigir continuidad.",
                    Objetivo = "Servicio continuo en cuarenta colonias actualmente sin cobertura estable.",
                    Beneficiarios = "Aproximadamente ciento veinte mil habitantes",
                    Categoria = "Infraestructura",
                    Ubicacion = "San Pedro Sula, Cortés",
                    PeriodoEjecucion = "2030 – 2032",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-24)
                },
                new Propuesta
                {
                    Id = 9,
                    CandidatoSlug = "gabriela-fonseca-ordonez",
                    CampanaSlug = campana,
                    Nombre = "Red de atención primaria en salud por sector",
                    Descripcion =
                        "Un equipo de atención primaria por cada sector definido, con horario extendido y " +
                        "reporte mensual de consultas atendidas y referencias.",
                    Problema =
                        "La atención primaria se concentra en pocos centros, lo que traslada a los hospitales " +
                        "casos que podrían resolverse antes.",
                    Objetivo = "Cobertura de atención primaria para el 90 % de los sectores del municipio.",
                    Beneficiarios = "Población sin acceso cercano a atención primaria",
                    Categoria = "Salud",
                    Ubicacion = "San Pedro Sula, Cortés",
                    PeriodoEjecucion = "2030 – 2033",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-9)
                },
                new Propuesta
                {
                    Id = 10,
                    CandidatoSlug = "hector-paz-mejia",
                    CampanaSlug = campana,
                    Nombre = "Voto legislativo público y razonado",
                    Descripcion =
                        "Publicar el sentido de cada voto en el pleno y en comisiones, acompañado de una " +
                        "justificación escrita antes de la siguiente sesión.",
                    Problema =
                        "El electorado no puede contrastar el discurso de campaña con el comportamiento " +
                        "legislativo posterior.",
                    Objetivo = "Registro público del 100 % de los votos emitidos, con justificación.",
                    Beneficiarios = "Electorado del departamento de Francisco Morazán",
                    Categoria = "Transparencia y gobernanza",
                    PeriodoEjecucion = "Todo el período legislativo",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-6)
                },
                new Propuesta
                {
                    Id = 11,
                    CandidatoSlug = "marlon-castellanos-rivas",
                    CampanaSlug = campana,
                    Nombre = "Tecnificación de riego para pequeños productores del sur",
                    Descripcion =
                        "Programa de riego tecnificado con asistencia técnica y seguimiento de rendimiento " +
                        "por parcela beneficiada.",
                    Problema =
                        "La producción del sur depende de lluvia irregular, con pérdidas recurrentes y sin " +
                        "medición del efecto de los apoyos entregados.",
                    Objetivo = "Cinco mil hectáreas con riego tecnificado y rendimiento medido.",
                    Beneficiarios = "Pequeños productores agrícolas de la zona sur",
                    Categoria = "Economía y empleo",
                    Ubicacion = "Choluteca y Valle",
                    PeriodoEjecucion = "2030 – 2033",
                    Estado = EstadoPropuesta.Declarada,
                    FechaRegistro = hoy.AddDays(-4)
                }
            };
        }

        private static List<Publicacion> ConstruirPublicaciones()
        {
            const string campana = "generales-2029";
            DateTime hoy = DateTime.Now;

            List<Publicacion> lista = new List<Publicacion>
            {
                new Publicacion
                {
                    Id = 1,
                    CampanaSlug = campana,
                    CandidatoSlug = "ana-molina-caballero",
                    Fecha = hoy.AddHours(-3),
                    Texto =
                        "Publiqué la ficha completa del portal de compromisos: cada meta con su indicador, su " +
                        "responsable institucional y el presupuesto que requiere. Si algo de esto no se puede " +
                        "medir, no debería estar en un plan de gobierno. Queda abierto a revisión.",
                    Categoria = "Transparencia y gobernanza",
                    PropuestaId = 1,
                    PropuestaNombre = "Portal de compromisos con metas medibles",
                    MeGusta = 342,
                    Comentarios = 57,
                    Verificacion = NivelVerificacion.Verificado
                },
                new Publicacion
                {
                    Id = 2,
                    CampanaSlug = campana,
                    CandidatoSlug = "gabriela-fonseca-ordonez",
                    Fecha = hoy.AddHours(-9),
                    Texto =
                        "Estuvimos en cuatro colonias del sector sur levantando el registro de horas de " +
                        "servicio de agua. El dato que encontramos no coincide con el reporte oficial. " +
                        "Vamos a publicar la medición completa por sector la próxima semana.",
                    Categoria = "Infraestructura",
                    PropuestaId = 8,
                    PropuestaNombre = "Agua potable continua en colonias sin servicio",
                    ImagenUrl = "demo",
                    MeGusta = 218,
                    Comentarios = 34
                },
                new Publicacion
                {
                    Id = 3,
                    CampanaSlug = campana,
                    CandidatoSlug = "rodrigo-nunez-berrios",
                    Fecha = hoy.AddDays(-1).AddHours(-2),
                    Texto =
                        "Sobre el programa de primer empleo: la meta es cincuenta mil plazas formales en " +
                        "cuatro años, verificables en planilla del seguro social. No cuento como empleo lo " +
                        "que no aparezca cotizando.",
                    Categoria = "Economía y empleo",
                    PropuestaId = 4,
                    PropuestaNombre = "Programa de primer empleo formal para jóvenes",
                    MeGusta = 411,
                    Comentarios = 88,
                    Verificacion = NivelVerificacion.Verificado
                },
                new Publicacion
                {
                    Id = 4,
                    CampanaSlug = campana,
                    CandidatoSlug = "carmen-villeda-lopez",
                    Fecha = hoy.AddDays(-2),
                    Texto =
                        "Ya está el mapa preliminar de las mil cuadras priorizadas, con el criterio técnico " +
                        "de selección a la vista: estado de la vía, tránsito diario y población servida. " +
                        "Si su cuadra no aparece, el formulario de observación está abierto.",
                    Categoria = "Infraestructura",
                    PropuestaId = 6,
                    PropuestaNombre = "Mil cuadras: repavimentación con avance público",
                    ImagenUrl = "demo",
                    MeGusta = 176,
                    Comentarios = 62
                },
                new Publicacion
                {
                    Id = 5,
                    CampanaSlug = campana,
                    CandidatoSlug = "hector-paz-mejia",
                    Fecha = hoy.AddDays(-3).AddHours(-5),
                    Texto =
                        "Como candidatura independiente no tengo estructura de partido, así que mi compromiso " +
                        "es el que puedo sostener solo: publicar cada voto y su razón. Empiezo desde ahora " +
                        "con mi posición sobre los proyectos de ley en discusión.",
                    Categoria = "Transparencia y gobernanza",
                    PropuestaId = 10,
                    PropuestaNombre = "Voto legislativo público y razonado",
                    MeGusta = 129,
                    Comentarios = 41
                },
                new Publicacion
                {
                    Id = 6,
                    CampanaSlug = campana,
                    CandidatoSlug = "ana-molina-caballero",
                    Fecha = hoy.AddDays(-4).AddHours(-7),
                    Texto =
                        "El desabastecimiento de medicamentos no se resuelve con anuncios, se resuelve con " +
                        "inventarios en línea. Presenté la propuesta con la meta concreta: menos del 10 % de " +
                        "eventos de desabastecimiento del listado básico al cierre del segundo año.",
                    Categoria = "Salud",
                    PropuestaId = 2,
                    PropuestaNombre = "Abastecimiento continuo de medicamentos esenciales",
                    MeGusta = 505,
                    Comentarios = 103,
                    Verificacion = NivelVerificacion.Verificado
                },
                new Publicacion
                {
                    Id = 7,
                    CampanaSlug = campana,
                    CandidatoSlug = "marlon-castellanos-rivas",
                    Fecha = hoy.AddDays(-5).AddHours(-3),
                    Texto =
                        "Reunión con cooperativas de Choluteca y Valle. La demanda es la misma de hace diez " +
                        "años: riego y camino de acceso. La diferencia esta vez es que la meta queda escrita " +
                        "acá, con hectáreas y rendimiento medido.",
                    Categoria = "Economía y empleo",
                    PropuestaId = 11,
                    PropuestaNombre = "Tecnificación de riego para pequeños productores del sur",
                    ImagenUrl = "demo",
                    MeGusta = 97,
                    Comentarios = 19
                },
                new Publicacion
                {
                    Id = 8,
                    CampanaSlug = campana,
                    CandidatoSlug = "gabriela-fonseca-ordonez",
                    Fecha = hoy.AddDays(-7),
                    Texto =
                        "Atención primaria significa resolver cerca de la casa lo que hoy termina en " +
                        "emergencia del hospital. Un equipo por sector, horario extendido y reporte mensual " +
                        "de consultas. Está publicado con su indicador de cobertura.",
                    Categoria = "Salud",
                    PropuestaId = 9,
                    PropuestaNombre = "Red de atención primaria en salud por sector",
                    MeGusta = 264,
                    Comentarios = 45,
                    Verificacion = NivelVerificacion.Verificado
                }
            };

            return lista;
        }

        /// <summary>
        /// Deriva los contadores en lugar de escribirlos a mano, para que las
        /// cifras que se muestran nunca contradigan a los datos.
        /// </summary>
        private void RecalcularTotales()
        {
            foreach (Candidato c in _candidatos)
            {
                c.TotalPropuestas = _propuestas.Count(p => p.CandidatoSlug == c.Slug);
                c.TotalPublicaciones = _publicaciones.Count(p => p.CandidatoSlug == c.Slug);
            }

            // Datos del autor desnormalizados en cada publicación del feed.
            foreach (Publicacion p in _publicaciones)
            {
                Candidato autor = _candidatos.FirstOrDefault(c => c.Slug == p.CandidatoSlug);
                if (autor == null) continue;

                p.CandidatoNombre = autor.NombreCompleto;
                p.CandidatoCargo = autor.Cargo;
                p.CandidatoFotoUrl = autor.FotoUrl;
                p.CandidatoIniciales = autor.Iniciales;
            }

            foreach (Campana ca in _campanas)
            {
                ca.TotalCandidatos = _candidatos.Count(c => c.CampanaSlug == ca.Slug);
                ca.TotalPropuestas = _propuestas.Count(p => p.CampanaSlug == ca.Slug);
                ca.TotalPublicaciones = _publicaciones.Count(p => p.CampanaSlug == ca.Slug);
            }
        }
    }
}
