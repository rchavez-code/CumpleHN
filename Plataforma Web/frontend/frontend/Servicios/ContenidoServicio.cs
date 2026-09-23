using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web;
using frontend.Modelos;
using ws = frontend.webservices;

namespace frontend.Servicios
{
    /// <summary>
    /// Implementación de <see cref="IContenidoServicio"/> que obtiene todo del
    /// Web Service del backend, a través del Service Reference.
    ///
    /// El frontend no tiene cadena de conexión ni toca System.Data: los datos
    /// llegan únicamente por acá. El endpoint está en el
    /// <c>&lt;system.serviceModel&gt;</c> del Web.config.
    ///
    /// La traducción entre los objetos del servicio y los modelos de vista se
    /// concentra en esta clase, de modo que un cambio en el contrato del
    /// backend no obligue a tocar ninguna página.
    /// </summary>
    public class ContenidoServicio : IContenidoServicio
    {
        // =============================================================
        //  Llamada al servicio
        // =============================================================

        /// <summary>
        /// Ejecuta una llamada al Web Service cerrando siempre el canal.
        ///
        /// Si el backend no responde se devuelve <paramref name="porDefecto"/> y
        /// se guarda el motivo en <see cref="UltimoError"/>, en lugar de dejar
        /// que una excepción de WCF rompa la página. Un backend caído tiene que
        /// mostrarse como una sección vacía, no como un error del servidor.
        /// </summary>
        private static T Ejecutar<T>(Func<ws.WebServiceGlobalSoapClient, T> llamada, T porDefecto)
        {
            ws.WebServiceGlobalSoapClient cliente = null;

            try
            {
                cliente = new ws.WebServiceGlobalSoapClient();
                T resultado = llamada(cliente);
                cliente.Close();
                UltimoError = null;
                return resultado;
            }
            catch (Exception ex)
            {
                UltimoError = ex.Message;

                if (cliente != null)
                {
                    try { cliente.Abort(); }
                    catch { /* el canal ya estaba roto */ }
                }

                return porDefecto;
            }
        }

        /// <summary>
        /// Motivo del último fallo de comunicación con el backend, o null si la
        /// última llamada salió bien. Se guarda por petición.
        /// </summary>
        public static string UltimoError
        {
            get
            {
                HttpContext ctx = HttpContext.Current;
                return ctx == null ? null : ctx.Items["cumplehn.ultimoError"] as string;
            }
            private set
            {
                HttpContext ctx = HttpContext.Current;
                if (ctx != null) ctx.Items["cumplehn.ultimoError"] = value;
            }
        }

        // =============================================================
        //  Campañas
        // =============================================================

        public IList<Campana> ObtenerCampanas()
        {
            ws.Campana[] datos = Ejecutar(c => c.listarCampanas(Espacios.SlugActual), new ws.Campana[0]);

            List<Campana> lista = new List<Campana>();
            foreach (ws.Campana d in datos) lista.Add(AModelo(d));
            return lista;
        }

        public Campana ObtenerCampanaActual()
        {
            ws.Campana d = Ejecutar(c => c.obtenerCampanaActual(Espacios.SlugActual), null);
            return d == null ? null : AModelo(d);
        }

        public Campana ObtenerCampana(string slug)
        {
            ws.Campana d = Ejecutar(c => c.obtenerCampana(slug), null);
            return d == null ? null : AModelo(d);
        }

        // =============================================================
        //  Candidatos
        // =============================================================

        public IList<Candidato> ObtenerCandidatos(string campanaSlug)
        {
            ws.Candidato[] datos =
                Ejecutar(c => c.listarCandidatos(Espacios.SlugActual, campanaSlug ?? string.Empty), new ws.Candidato[0]);

            List<Candidato> lista = new List<Candidato>();
            foreach (ws.Candidato d in datos) lista.Add(AModelo(d));
            return lista;
        }

        public Candidato ObtenerCandidato(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;

            ws.Candidato d = Ejecutar(c => c.obtenerCandidato(slug), null);
            return d == null ? null : AModelo(d);
        }

        /// <summary>
        /// Candidato dueño de la sesión actual. Devuelve null si nadie inició
        /// sesión, que es lo que hace que el panel exija autenticación.
        /// </summary>
        public Candidato ObtenerCandidatoAutenticado()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null) return null;

            string slug = ctx.Session["candidatoSlug"] as string;
            if (string.IsNullOrEmpty(slug)) return null;

            return ObtenerCandidato(slug);
        }

        // =============================================================
        //  Propuestas
        // =============================================================

        public IList<Propuesta> ObtenerPropuestas(string candidatoSlug)
        {
            if (string.IsNullOrEmpty(candidatoSlug)) return new List<Propuesta>();

            ws.Propuesta[] datos =
                Ejecutar(c => c.listarPropuestasDeCandidato(candidatoSlug), new ws.Propuesta[0]);

            return AModelos(datos);
        }

        public IList<Propuesta> ObtenerPropuestasDeCampana(string campanaSlug)
        {
            ws.Propuesta[] datos =
                Ejecutar(c => c.listarPropuestasDeCampana(Espacios.SlugActual, campanaSlug ?? string.Empty), new ws.Propuesta[0]);

            return AModelos(datos);
        }

        public Propuesta ObtenerPropuesta(int id)
        {
            ws.Propuesta d = Ejecutar(c => c.obtenerPropuesta(id), null);
            return d == null ? null : AModelo(d);
        }

        private static IList<Propuesta> AModelos(ws.Propuesta[] datos)
        {
            List<Propuesta> lista = new List<Propuesta>();
            foreach (ws.Propuesta d in datos) lista.Add(AModelo(d));
            return lista;
        }

        // =============================================================
        //  Publicaciones
        // =============================================================

        public IList<Publicacion> ObtenerFeed(string campanaSlug)
        {
            ws.Publicacion[] datos =
                Ejecutar(c => c.listarFeed(Espacios.SlugActual, campanaSlug ?? string.Empty), new ws.Publicacion[0]);

            List<Publicacion> lista = new List<Publicacion>();
            foreach (ws.Publicacion d in datos) lista.Add(AModelo(d));
            return lista;
        }

        public IList<Publicacion> ObtenerPublicaciones(string candidatoSlug)
        {
            if (string.IsNullOrEmpty(candidatoSlug)) return new List<Publicacion>();

            ws.Publicacion[] datos =
                Ejecutar(c => c.listarPublicacionesDeCandidato(candidatoSlug), new ws.Publicacion[0]);

            List<Publicacion> lista = new List<Publicacion>();
            foreach (ws.Publicacion d in datos) lista.Add(AModelo(d));
            return lista;
        }

        // =============================================================
        //  Partidos
        // =============================================================

        public IList<Partido> ObtenerPartidos()
        {
            ws.Partido[] datos = Ejecutar(c => c.listarPartidos(Espacios.SlugActual), new ws.Partido[0]);

            List<Partido> lista = new List<Partido>();
            foreach (ws.Partido d in datos) lista.Add(AModelo(d));
            return lista;
        }

        public Partido ObtenerPartido(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;

            ws.Partido d = Ejecutar(c => c.obtenerPartido(slug), null);
            return d == null ? null : AModelo(d);
        }

        public IList<Candidato> ObtenerCandidatosDePartido(string partidoSlug)
        {
            if (string.IsNullOrEmpty(partidoSlug)) return new List<Candidato>();

            ws.Candidato[] datos =
                Ejecutar(c => c.listarCandidatosDePartido(partidoSlug), new ws.Candidato[0]);

            List<Candidato> lista = new List<Candidato>();
            foreach (ws.Candidato d in datos) lista.Add(AModelo(d));
            return lista;
        }

        // =============================================================
        //  Interacción
        // =============================================================

        public Interaccion ObtenerInteraccion(string tipoObjeto, int codigoObjeto, int codigoUsuario)
        {
            ws.Interaccion d = Ejecutar(
                c => c.obtenerInteraccion(tipoObjeto, codigoObjeto, codigoUsuario), null);

            if (d == null)
            {
                // Backend inaccesible: se devuelve un objeto neutro para que la
                // tarjeta se dibuje en cero en lugar de romper la página.
                return new Interaccion { TipoObjeto = tipoObjeto, CodigoObjeto = codigoObjeto };
            }

            return AModelo(d);
        }

        public Resultado Valorar(string tipoObjeto, int codigoObjeto, int codigoUsuario, int valor)
        {
            ws.RespuestaInteraccion d = Ejecutar(
                c => c.valorar(tipoObjeto, codigoObjeto, codigoUsuario, valor), null);

            if (d == null)
            {
                return new Resultado
                {
                    Ok = false,
                    Mensaje = "No se pudo contactar al servidor. Intentá de nuevo."
                };
            }

            return new Resultado { Ok = d.ok, Mensaje = d.mensaje };
        }

        public IList<Comentario> ObtenerComentarios(string tipoObjeto, int codigoObjeto)
        {
            ws.Comentario[] datos = Ejecutar(
                c => c.listarComentarios(tipoObjeto, codigoObjeto), new ws.Comentario[0]);

            List<Comentario> lista = new List<Comentario>();
            foreach (ws.Comentario d in datos) lista.Add(AModelo(d));
            return lista;
        }

        public Resultado AgregarComentario(string tipoObjeto, int codigoObjeto,
            int codigoUsuario, string texto)
        {
            ws.RespuestaComentario d = Ejecutar(
                c => c.agregarComentario(tipoObjeto, codigoObjeto, codigoUsuario, texto), null);

            if (d == null)
            {
                return new Resultado
                {
                    Ok = false,
                    Mensaje = "No se pudo contactar al servidor. Intentá de nuevo."
                };
            }

            return new Resultado { Ok = d.ok, Mensaje = d.mensaje };
        }

        // =============================================================
        //  Encuestas
        // =============================================================

        public IList<Encuesta> ObtenerEncuestasVigentes(string campanaSlug, int codigoUsuario)
        {
            ws.EncuestaPublica[] datos = Ejecutar(
                c => c.listarEncuestasVigentes(Espacios.SlugActual, campanaSlug, codigoUsuario),
                new ws.EncuestaPublica[0]);

            List<Encuesta> lista = new List<Encuesta>();
            if (datos == null) return lista;

            foreach (ws.EncuestaPublica d in datos)
            {
                lista.Add(new Encuesta
                {
                    Id = d.codigoEncuesta,
                    CampanaSlug = d.campanaSlug,
                    Pregunta = d.pregunta,
                    Descripcion = d.descripcion,
                    Categoria = d.categoria,
                    FechaInicio = d.fechaInicio,
                    FechaCierre = d.fechaCierre,
                    Estado = d.estado,
                    Votos = d.votos,
                    MiOpcion = d.miOpcion,
                    // Las opciones llegan dentro de la encuesta. Después de
                    // responder vuelven solas, porque ahí sí son lo único que
                    // cambió.
                    Opciones = AOpcionesEncuesta(d.opciones)
                });
            }

            return lista;
        }

        public ResultadoEncuesta ResponderEncuesta(
            int codigoEncuesta, int codigoOpcion, int codigoUsuario)
        {
            ws.RespuestaEncuesta d = Ejecutar(
                c => c.votarEncuesta(codigoEncuesta, codigoOpcion, codigoUsuario), null);

            if (d == null)
            {
                return new ResultadoEncuesta
                {
                    Ok = false,
                    Mensaje = "No se pudo contactar al servidor. Intentá de nuevo."
                };
            }

            return new ResultadoEncuesta
            {
                Ok = d.ok,
                Mensaje = d.mensaje,
                Votos = d.votos,
                Opciones = AOpcionesEncuesta(d.opciones)
            };
        }

        // =============================================================
        //  Iniciativas ciudadanas
        // =============================================================

        public IList<Iniciativa> ObtenerIniciativas(int codigoUsuario)
        {
            return AIniciativas(Ejecutar(
                c => c.listarIniciativas(Espacios.SlugActual, codigoUsuario), new ws.IniciativaPublica[0]));
        }

        public IList<Iniciativa> ObtenerIniciativasDeUsuario(int codigoUsuario)
        {
            return AIniciativas(Ejecutar(
                c => c.listarIniciativasDeUsuario(codigoUsuario), new ws.IniciativaPublica[0]));
        }

        public ResultadoGuardado GuardarIniciativa(int codigoUsuario, int codigoIniciativa,
            string titulo, string descripcion, int codigoCategoria, int codigoDepartamento)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.guardarIniciativa(Espacios.SlugActual, codigoUsuario, codigoIniciativa,
                    titulo, descripcion, codigoCategoria, codigoDepartamento), null);

            if (d == null)
            {
                return new ResultadoGuardado
                {
                    Ok = false,
                    Mensaje = "No se pudo contactar al servidor. Intentá de nuevo."
                };
            }

            return new ResultadoGuardado { Ok = d.ok, Mensaje = d.mensaje, Codigo = d.codigo };
        }

        public Resultado RetirarIniciativaPropia(int codigoUsuario, int codigoIniciativa)
        {
            return AResultado(Ejecutar(
                c => c.retirarIniciativaPropia(codigoUsuario, codigoIniciativa), null));
        }

        public IList<Iniciativa> ObtenerIniciativasAdmin(int codigoUsuario, string estado)
        {
            return AIniciativas(Ejecutar(
                c => c.listarIniciativasAdmin(codigoUsuario, Sesion.CodigoEspacio, estado), new ws.IniciativaPublica[0]));
        }

        public Resultado ModerarIniciativa(int codigoUsuario, int codigoIniciativa,
            bool activo, string motivo)
        {
            return AResultado(Ejecutar(
                c => c.moderarIniciativa(codigoUsuario, codigoIniciativa, activo, motivo), null));
        }

        // =============================================================
        //  Panel de la candidatura
        // =============================================================

        public EdicionPanel ObtenerEdicionPanel(int codigoUsuario, int codigoPropuesta)
        {
            ws.EdicionPanel d = Ejecutar(c => c.obtenerEdicionPanel(codigoUsuario, codigoPropuesta), null);

            // Sin respuesta el formulario queda cerrado: ofrecer guardar algo
            // que no se sabe si se acepta termina en un rechazo después de
            // que la persona escribió todo.
            if (d == null)
            {
                return new EdicionPanel
                {
                    Editable = false,
                    TextoEditable = false,
                    Motivo = "No se pudo contactar al servidor. Intentá de nuevo en un momento."
                };
            }

            return new EdicionPanel { Editable = d.editable, TextoEditable = d.textoEditable, Motivo = d.motivo };
        }

        public ResultadoGuardado GuardarPropuestaPanel(int codigoUsuario, int codigoPropuesta,
            string nombre, string descripcion, string problema, string objetivo, string beneficiarios,
            int codigoCategoria, string ubicacion, string periodoEjecucion, string estado,
            string informacionAdicional)
        {
            return AGuardado(Ejecutar(
                c => c.guardarPropuestaPanel(codigoUsuario, codigoPropuesta, nombre, descripcion,
                    problema, objetivo, beneficiarios, codigoCategoria, ubicacion, periodoEjecucion,
                    estado, informacionAdicional), null));
        }

        public ResultadoGuardado GuardarPerfilPanel(int codigoUsuario, string titular, string biografia,
            string informacionProfesional, string descripcionCandidatura, string correoPublico,
            string telefono, string sitioWeb, string facebook, string x, string instagram)
        {
            return AGuardado(Ejecutar(
                c => c.guardarPerfilPanel(codigoUsuario, titular, biografia, informacionProfesional,
                    descripcionCandidatura, correoPublico, telefono, sitioWeb, facebook, x, instagram), null));
        }

        // =============================================================
        //  Archivos
        // =============================================================

        public IList<ArchivoRespaldo> ObtenerArchivosPropuesta(int codigoPropuesta)
        {
            ws.ArchivoRespaldo[] datos = Ejecutar(
                c => c.listarArchivosPropuesta(codigoPropuesta), new ws.ArchivoRespaldo[0]);

            List<ArchivoRespaldo> lista = new List<ArchivoRespaldo>();
            if (datos == null) return lista;

            foreach (ws.ArchivoRespaldo d in datos)
            {
                lista.Add(new ArchivoRespaldo
                {
                    Id = d.codigoArchivo,
                    NombreOriginal = d.nombreOriginal,
                    TipoContenido = d.tipoContenido,
                    TamanoBytes = d.tamanoBytes,
                    FechaRegistro = d.fechaRegistro
                });
            }

            return lista;
        }

        public Resultado QuitarArchivo(int codigoUsuario, int codigoArchivo)
        {
            return AResultado(Ejecutar(c => c.quitarArchivo(codigoUsuario, codigoArchivo), null));
        }

        private static IList<Iniciativa> AIniciativas(ws.IniciativaPublica[] datos)
        {
            List<Iniciativa> lista = new List<Iniciativa>();
            if (datos == null) return lista;

            foreach (ws.IniciativaPublica d in datos)
            {
                lista.Add(new Iniciativa
                {
                    Id = d.codigoIniciativa,
                    Autora = d.autora,
                    Titulo = d.titulo,
                    Descripcion = d.descripcion,
                    CodigoCategoria = d.codigoCategoria,
                    Categoria = d.categoria,
                    CodigoDepartamento = d.codigoDepartamento,
                    Departamento = d.departamento,
                    MeGusta = d.meGusta,
                    NoMeGusta = d.noMeGusta,
                    Comentarios = d.comentarios,
                    Saldo = d.saldo,
                    MiValoracion = d.miValoracion,
                    EsMia = d.esMia,
                    Activa = d.activo,
                    MotivoBaja = d.motivoBaja,
                    PuedeEditar = d.puedeEditar,
                    FechaRegistro = d.fechaRegistro,
                    FechaEdicion = d.fechaEdicion
                });
            }

            return lista;
        }

        private static Resultado AResultado(ws.RespuestaAdmin d)
        {
            if (d == null)
            {
                return new Resultado
                {
                    Ok = false,
                    Mensaje = "No se pudo contactar al servidor. Intentá de nuevo."
                };
            }

            return new Resultado { Ok = d.ok, Mensaje = d.mensaje };
        }

        private static IList<OpcionEncuesta> AOpcionesEncuesta(ws.OpcionEncuesta[] datos)
        {
            List<OpcionEncuesta> lista = new List<OpcionEncuesta>();
            if (datos == null) return lista;

            foreach (ws.OpcionEncuesta d in datos)
            {
                lista.Add(new OpcionEncuesta
                {
                    Id = d.codigoOpcion,
                    Texto = d.texto,
                    Orden = d.orden,
                    Votos = d.votos,
                    MiVoto = d.miVoto
                });
            }

            return lista;
        }

        // =============================================================
        //  Analítica
        // =============================================================

        public Analitica ObtenerAnalitica(FiltroAnalitica filtro)
        {
            if (filtro == null) filtro = new FiltroAnalitica();

            ws.FiltroAnalitica f = new ws.FiltroAnalitica
            {
                espacioSlug = Espacios.SlugActual,
                campanaSlug = filtro.CampanaSlug ?? string.Empty,
                codigoCategoria = filtro.Categoria,
                codigoPartido = filtro.Partido,
                codigoDepartamento = filtro.Departamento,
                nivelGobierno = filtro.NivelGobierno ?? string.Empty,
                desde = filtro.Desde ?? string.Empty,
                hasta = filtro.Hasta ?? string.Empty
            };

            ws.Analitica d = Ejecutar(c => c.obtenerAnalitica(f), null);

            // Backend inaccesible. Se marca como error y no como tablero vacío:
            // son dos situaciones distintas y la página las explica distinto.
            if (d == null) return new Analitica { Error = true };

            Analitica a = new Analitica();
            a.CampanaSlug = d.campanaSlug;
            a.CampanaNombre = d.campanaNombre;
            a.Generado = d.generado;
            a.SinDatos = d.sinDatos;

            if (d.resumen != null)
            {
                a.Resumen = new ResumenAnalitica
                {
                    Propuestas = d.resumen.propuestas,
                    Candidaturas = d.resumen.candidaturas,
                    Partidos = d.resumen.partidos,
                    Publicaciones = d.resumen.publicaciones,
                    CategoriasConOferta = d.resumen.categoriasConOferta,
                    CategoriasTotal = d.resumen.categoriasTotal,
                    PropuestasEvaluadas = d.resumen.propuestasEvaluadas,
                    PropuestasVerificadas = d.resumen.propuestasVerificadas,
                    CandidaturasVerificadas = d.resumen.candidaturasVerificadas,
                    DepartamentosConCandidatura = d.resumen.departamentosConCandidatura,
                    DepartamentosTotal = d.resumen.departamentosTotal,
                    Valoraciones = d.resumen.valoraciones,
                    MeGusta = d.resumen.meGusta,
                    NoMeGusta = d.resumen.noMeGusta,
                    Comentarios = d.resumen.comentarios,
                    PersonasParticipando = d.resumen.personasParticipando,
                    UltimoRegistro = d.resumen.ultimoRegistro
                };
            }

            if (d.opciones != null)
                foreach (ws.OpcionFiltro o in d.opciones)
                    a.Opciones.Add(new OpcionFiltro { Grupo = o.grupo, Valor = o.valor, Texto = o.texto });

            if (d.categorias != null)
                foreach (ws.FilaCategoria x in d.categorias)
                    a.Categorias.Add(new FilaCategoria
                    {
                        Codigo = x.codigoCategoria,
                        Categoria = x.categoria,
                        InteresEncuesta = x.interesEncuesta,
                        Propuestas = x.propuestas,
                        Candidaturas = x.candidaturas
                    });

            if (d.estados != null)
                foreach (ws.FilaEstado x in d.estados)
                    a.Estados.Add(new FilaEstado
                    {
                        Estado = x.estado,
                        Descripcion = x.descripcion,
                        Ponderacion = x.ponderacion,
                        Propuestas = x.propuestas
                    });

            if (d.verificacion != null)
                foreach (ws.FilaVerificacion x in d.verificacion)
                    a.Verificacion.Add(new FilaVerificacion
                    {
                        Entidad = x.entidad,
                        Nivel = x.nivel,
                        Total = x.total
                    });

            if (d.participacion != null)
                foreach (ws.FilaParticipacion x in d.participacion)
                    a.Participacion.Add(new FilaParticipacion
                    {
                        TipoObjeto = x.tipoObjeto,
                        MeGusta = x.meGusta,
                        NoMeGusta = x.noMeGusta,
                        Comentarios = x.comentarios
                    });

            if (d.candidatos != null)
                foreach (ws.FilaCandidato x in d.candidatos)
                    a.Candidatos.Add(new FilaCandidato
                    {
                        Slug = x.candidatoSlug,
                        Candidato = x.candidato,
                        PartidoSiglas = x.partidoSiglas,
                        Departamento = x.departamento,
                        Cargo = x.cargo,
                        NivelGobierno = x.nivelGobierno,
                        Verificacion = x.verificacion,
                        Propuestas = x.propuestas,
                        MeGusta = x.meGusta,
                        NoMeGusta = x.noMeGusta,
                        Saldo = x.saldo,
                        Comentarios = x.comentarios
                    });

            if (d.partidos != null)
                foreach (ws.FilaPartido x in d.partidos)
                    a.Partidos.Add(new FilaPartido
                    {
                        Codigo = x.codigoPartido,
                        Siglas = x.partidoSiglas,
                        Partido = x.partido,
                        Candidaturas = x.candidaturas,
                        Propuestas = x.propuestas,
                        PropuestasPorCandidatura = x.propuestasPorCandidatura,
                        MeGusta = x.meGusta,
                        NoMeGusta = x.noMeGusta
                    });

            if (d.territorio != null)
                foreach (ws.FilaDepartamento x in d.territorio)
                    a.Territorio.Add(new FilaDepartamento
                    {
                        Codigo = x.codigoDepartamento,
                        Departamento = x.departamento,
                        Candidaturas = x.candidaturas,
                        Propuestas = x.propuestas,
                        Valoraciones = x.valoraciones
                    });

            if (d.actividad != null)
                foreach (ws.FilaActividad x in d.actividad)
                    a.Actividad.Add(new FilaActividad
                    {
                        Fecha = x.fecha,
                        MeGusta = x.meGusta,
                        NoMeGusta = x.noMeGusta,
                        Comentarios = x.comentarios
                    });

            // Los porcentajes de oferta dependen del subconjunto que quedó tras
            // el filtro, así que se calculan una vez ya armada la colección.
            a.CalcularProporciones();

            return a;
        }

        // =============================================================
        //  Catálogos
        // =============================================================

        public IList<string> ObtenerCategorias()
        {
            return Nombres(Ejecutar(c => c.listarCategorias(Espacios.SlugActual), new ws.Catalogo[0]));
        }

        public IList<string> ObtenerCargos()
        {
            return Nombres(Ejecutar(c => c.listarCargos(Espacios.SlugActual), new ws.Catalogo[0]));
        }

        public IList<string> ObtenerDepartamentos()
        {
            return Nombres(Ejecutar(c => c.listarDepartamentos(), new ws.Catalogo[0]));
        }

        private static IList<string> Nombres(ws.Catalogo[] datos)
        {
            List<string> lista = new List<string>();
            foreach (ws.Catalogo d in datos) lista.Add(d.nombre);
            return lista;
        }

        // =============================================================
        //  Traducción a modelos de vista
        // =============================================================

        private static Campana AModelo(ws.Campana d)
        {
            return new Campana
            {
                Slug = d.slug,
                EspacioSlug = d.espacioSlug ?? string.Empty,
                Nombre = d.nombre,
                Resumen = d.resumen,
                Descripcion = d.descripcion,
                Alcance = d.alcance,
                FechaInicio = d.fechaInicio,
                FechaEleccion = d.fechaEleccion,
                Estado = AEstadoCampana(d.estado),
                EsActual = d.esActual,
                TotalCandidatos = d.totalCandidatos,
                TotalPropuestas = d.totalPropuestas,
                TotalPublicaciones = d.totalPublicaciones
            };
        }

        private static Candidato AModelo(ws.Candidato d)
        {
            return new Candidato
            {
                Id = d.codigoCandidato,
                Slug = d.slug,
                EspacioSlug = d.espacioSlug ?? string.Empty,
                CampanaSlug = d.campanaSlug,
                Nombres = d.nombres,
                Apellidos = d.apellidos,
                Partido = d.partido,
                PartidoSiglas = d.partidoSiglas,
                Cargo = d.cargo,
                Nivel = ANivel(d.nivelGobierno),
                Departamento = d.departamento,
                Municipio = d.municipio,
                FotoUrl = Archivos.Url(d.codigoFoto),
                Titular = d.titular,
                Biografia = d.biografia,
                InformacionProfesional = d.informacionProfesional,
                DescripcionCandidatura = d.descripcionCandidatura,
                CorreoPublico = d.correoPublico,
                Telefono = d.telefono,
                SitioWeb = d.sitioWeb,
                Facebook = d.facebook,
                X = d.x,
                Instagram = d.instagram,
                Verificacion = AVerificacion(d.verificacion),
                PartidoSlug = d.partidoSlug,
                TotalPropuestas = d.totalPropuestas,
                TotalPublicaciones = d.totalPublicaciones,
                MeGusta = d.meGusta,
                NoMeGusta = d.noMeGusta,
                Comentarios = d.comentarios
            };
        }

        private static Propuesta AModelo(ws.Propuesta d)
        {
            return new Propuesta
            {
                Id = d.codigoPropuesta,
                EspacioSlug = d.espacioSlug ?? string.Empty,
                CandidatoSlug = d.candidatoSlug,
                CampanaSlug = d.campanaSlug,
                Nombre = d.nombre,
                Descripcion = d.descripcion,
                Problema = d.problema,
                Objetivo = d.objetivo,
                Beneficiarios = d.beneficiarios,
                Categoria = d.categoria,
                Ubicacion = d.ubicacion,
                PeriodoEjecucion = d.periodoEjecucion,
                Estado = AEstadoPropuesta(d.estado),
                ImagenUrl = d.imagenUrl,
                InformacionAdicional = d.informacionAdicional,
                Verificacion = AVerificacion(d.verificacion),
                FechaRegistro = d.fechaRegistro,
                MeGusta = d.meGusta,
                NoMeGusta = d.noMeGusta,
                Comentarios = d.comentarios
            };
        }

        private static Publicacion AModelo(ws.Publicacion d)
        {
            return new Publicacion
            {
                Id = d.codigoPublicacion,
                CampanaSlug = d.campanaSlug,
                CandidatoSlug = d.candidatoSlug,
                CandidatoNombre = d.candidatoNombre,
                CandidatoCargo = d.candidatoCargo,
                CandidatoFotoUrl = Archivos.Url(d.candidatoCodigoFoto),
                CandidatoIniciales = InicialesDe(d.candidatoNombre),
                Fecha = d.fecha,
                Texto = d.texto,
                ImagenUrl = d.imagenUrl,
                Categoria = d.categoria,
                PropuestaId = d.codigoPropuesta,
                PropuestaNombre = d.propuestaNombre,
                Verificacion = AVerificacion(d.verificacion),
                MeGusta = d.meGusta,
                NoMeGusta = d.noMeGusta,
                Comentarios = d.comentarios
            };
        }

        private static Partido AModelo(ws.Partido d)
        {
            return new Partido
            {
                Id = d.codigoPartido,
                Slug = d.slug,
                EspacioSlug = d.espacioSlug ?? string.Empty,
                Nombre = d.nombre,
                Siglas = d.siglas,
                Descripcion = d.descripcion,
                TotalCandidatos = d.totalCandidatos,
                TotalPropuestas = d.totalPropuestas,
                MeGusta = d.meGusta,
                NoMeGusta = d.noMeGusta,
                Comentarios = d.comentarios
            };
        }

        private static Interaccion AModelo(ws.Interaccion d)
        {
            return new Interaccion
            {
                TipoObjeto = d.tipoObjeto,
                CodigoObjeto = d.codigoObjeto,
                MeGusta = d.meGusta,
                NoMeGusta = d.noMeGusta,
                Comentarios = d.comentarios,
                MiValoracion = d.miValoracion
            };
        }

        private static Comentario AModelo(ws.Comentario d)
        {
            return new Comentario
            {
                Id = d.codigoComentario,
                CodigoUsuario = d.codigoUsuario,
                Autor = d.autor,
                Rol = d.rol,
                Fecha = d.fecha,
                Texto = d.texto
            };
        }

        // ---------------------------------------- Conversión de catálogos

        /* El backend envía los catálogos como texto porque es lo que se guarda
           en la base. Acá se traducen a las enumeraciones que usan las vistas,
           siempre con un valor por defecto seguro para que un nombre nuevo en
           la base no rompa ninguna página. */

        private static EstadoCampana AEstadoCampana(string valor)
        {
            if (string.Equals(valor, "Activa", StringComparison.OrdinalIgnoreCase)) return EstadoCampana.Activa;
            if (string.Equals(valor, "Cerrada", StringComparison.OrdinalIgnoreCase)) return EstadoCampana.Cerrada;
            return EstadoCampana.Proxima;
        }

        private static NivelGobierno ANivel(string valor)
        {
            if (string.Equals(valor, "Municipal", StringComparison.OrdinalIgnoreCase)) return NivelGobierno.Municipal;
            if (string.Equals(valor, "Departamental", StringComparison.OrdinalIgnoreCase)) return NivelGobierno.Departamental;
            return NivelGobierno.Nacional;
        }


        // =============================================================
        //  Administración
        //
        //  El código de usuario viaja como parámetro y el backend confirma el
        //  rol contra la base. Si la cuenta no lo tiene, las consultas vuelven
        //  vacías y las acciones en falso: el frontend no decide el permiso.
        // =============================================================

        public IList<ItemVerificacion> ObtenerBandejaVerificacion(
            int codigoUsuario, string tipoObjeto, string campanaSlug, bool soloPendientes)
        {
            ws.ItemVerificacion[] datos = Ejecutar(
                c => c.listarBandejaVerificacion(codigoUsuario, Sesion.CodigoEspacio, tipoObjeto, campanaSlug, soloPendientes),
                new ws.ItemVerificacion[0]);

            List<ItemVerificacion> lista = new List<ItemVerificacion>();
            if (datos == null) return lista;

            foreach (ws.ItemVerificacion d in datos)
            {
                lista.Add(new ItemVerificacion
                {
                    TipoObjeto = d.tipoObjeto,
                    CodigoObjeto = d.codigoObjeto,
                    Titulo = d.titulo,
                    Resumen = d.resumen,
                    Slug = d.slug,
                    Candidato = d.candidato,
                    CandidatoSlug = d.candidatoSlug,
                    CampanaSlug = d.campanaSlug,
                    CodigoVerificacion = d.codigoVerificacion,
                    Verificacion = d.verificacion,
                    VerificacionOrden = d.verificacionOrden,
                    Fecha = d.fecha
                });
            }

            return lista;
        }

        public Resultado CambiarVerificacion(
            int codigoUsuario, string tipoObjeto, int codigoObjeto,
            int codigoVerificacion, string motivo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.cambiarVerificacion(codigoUsuario, tipoObjeto, codigoObjeto,
                                           codigoVerificacion, motivo), null);

            return ARespuesta(d);
        }

        public IList<PublicacionModerada> ObtenerPublicacionesModeracion(
            int codigoUsuario, string campanaSlug, string estado)
        {
            ws.PublicacionModerada[] datos = Ejecutar(
                c => c.listarPublicacionesModeracion(codigoUsuario, Sesion.CodigoEspacio, campanaSlug, estado),
                new ws.PublicacionModerada[0]);

            List<PublicacionModerada> lista = new List<PublicacionModerada>();
            if (datos == null) return lista;

            foreach (ws.PublicacionModerada d in datos)
            {
                lista.Add(new PublicacionModerada
                {
                    CodigoPublicacion = d.codigoPublicacion,
                    Texto = d.texto,
                    Fecha = d.fecha,
                    Activa = d.activo,
                    MotivoBaja = d.motivoBaja,
                    RetiradaPor = d.retiradaPor,
                    Candidato = d.candidato,
                    CandidatoSlug = d.candidatoSlug,
                    CampanaSlug = d.campanaSlug,
                    Categoria = d.categoria,
                    Verificacion = d.verificacion,
                    MeGusta = d.meGusta,
                    NoMeGusta = d.noMeGusta,
                    Comentarios = d.comentarios
                });
            }

            return lista;
        }

        public Resultado ModerarPublicacion(
            int codigoUsuario, int codigoPublicacion, bool activa, string motivo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.moderarPublicacion(codigoUsuario, codigoPublicacion, activa, motivo), null);

            return ARespuesta(d);
        }

        public IList<RegistroAuditoria> ObtenerAuditoria(int codigoUsuario, string accion, int limite)
        {
            ws.RegistroAuditoria[] datos = Ejecutar(
                c => c.listarAuditoria(codigoUsuario, Sesion.CodigoEspacio, accion, limite),
                new ws.RegistroAuditoria[0]);

            List<RegistroAuditoria> lista = new List<RegistroAuditoria>();
            if (datos == null) return lista;

            foreach (ws.RegistroAuditoria d in datos)
            {
                lista.Add(new RegistroAuditoria
                {
                    Codigo = d.codigoAuditoria,
                    Fecha = d.fecha,
                    Usuario = d.usuario,
                    Accion = d.accion,
                    TipoObjeto = d.tipoObjeto,
                    CodigoObjeto = d.codigoObjeto,
                    Detalle = d.detalle,
                    Motivo = d.motivo,
                    Espacio = d.espacio
                });
            }

            return lista;
        }

        public IList<OpcionCatalogo> ObtenerNivelesVerificacion()
        {
            ws.Catalogo[] datos = Ejecutar(c => c.listarNivelesVerificacion(), new ws.Catalogo[0]);

            List<OpcionCatalogo> lista = new List<OpcionCatalogo>();
            if (datos == null) return lista;

            foreach (ws.Catalogo d in datos)
            {
                lista.Add(new OpcionCatalogo { Codigo = d.codigo, Nombre = d.nombre });
            }

            return lista;
        }


        // ------------------------------------------------- Catálogos admin

        public IList<PartidoAdmin> ObtenerPartidosAdmin(int codigoUsuario, bool soloActivos)
        {
            ws.PartidoAdmin[] datos = Ejecutar(
                c => c.listarPartidosAdmin(codigoUsuario, Sesion.CodigoEspacio, soloActivos), new ws.PartidoAdmin[0]);

            List<PartidoAdmin> lista = new List<PartidoAdmin>();
            if (datos == null) return lista;

            foreach (ws.PartidoAdmin d in datos)
            {
                lista.Add(new PartidoAdmin
                {
                    Codigo = d.codigoPartido,
                    Slug = d.slug,
                    Nombre = d.nombre,
                    Siglas = d.siglas,
                    Descripcion = d.descripcion,
                    Activo = d.activo,
                    Candidaturas = d.candidaturas
                });
            }

            return lista;
        }

        public ResultadoGuardado GuardarPartido(
            int codigoUsuario, int codigoPartido, string nombre, string siglas, string descripcion)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.guardarPartido(codigoUsuario, Sesion.CodigoEspacio, codigoPartido, nombre, siglas, descripcion), null);

            return AGuardado(d);
        }

        public Resultado CambiarEstadoPartido(
            int codigoUsuario, int codigoPartido, bool activo, string motivo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.cambiarEstadoPartido(codigoUsuario, codigoPartido, activo, motivo), null);

            return ARespuesta(d);
        }

        public IList<CampanaAdmin> ObtenerCampanasAdmin(int codigoUsuario)
        {
            ws.CampanaAdmin[] datos = Ejecutar(
                c => c.listarCampanasAdmin(codigoUsuario, Sesion.CodigoEspacio), new ws.CampanaAdmin[0]);

            List<CampanaAdmin> lista = new List<CampanaAdmin>();
            if (datos == null) return lista;

            foreach (ws.CampanaAdmin d in datos)
            {
                lista.Add(new CampanaAdmin
                {
                    Codigo = d.codigoCampana,
                    Slug = d.slug,
                    Nombre = d.nombre,
                    Resumen = d.resumen,
                    Descripcion = d.descripcion,
                    Alcance = d.alcance,
                    FechaInicio = d.fechaInicio,
                    FechaEleccion = d.fechaEleccion,
                    Estado = d.estado,
                    EsActual = d.esActual,
                    Candidaturas = d.candidaturas,
                    Propuestas = d.propuestas
                });
            }

            return lista;
        }

        public ResultadoGuardado GuardarCampana(
            int codigoUsuario, int codigoCampana, string nombre, string resumen,
            string descripcion, string alcance, DateTime fechaInicio, DateTime fechaEleccion,
            string estado, bool esActual)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.guardarCampana(codigoUsuario, Sesion.CodigoEspacio, codigoCampana, nombre, resumen, descripcion,
                                      alcance, fechaInicio, fechaEleccion, estado, esActual), null);

            return AGuardado(d);
        }

        public IList<CandidatoAdmin> ObtenerCandidatosAdmin(
            int codigoUsuario, string campanaSlug, bool soloActivos)
        {
            ws.CandidatoAdmin[] datos = Ejecutar(
                c => c.listarCandidatosAdmin(codigoUsuario, Sesion.CodigoEspacio, campanaSlug, soloActivos),
                new ws.CandidatoAdmin[0]);

            List<CandidatoAdmin> lista = new List<CandidatoAdmin>();
            if (datos == null) return lista;

            foreach (ws.CandidatoAdmin d in datos)
            {
                lista.Add(new CandidatoAdmin
                {
                    Codigo = d.codigoCandidato,
                    Slug = d.slug,
                    Nombres = d.nombres,
                    Apellidos = d.apellidos,
                    NombreCompleto = d.nombreCompleto,
                    CodigoCampana = d.codigoCampana,
                    CampanaSlug = d.campanaSlug,
                    Campana = d.campana,
                    CodigoPartido = d.codigoPartido,
                    Partido = d.partido,
                    CodigoCargo = d.codigoCargo,
                    Cargo = d.cargo,
                    CodigoDepartamento = d.codigoDepartamento,
                    Departamento = d.departamento,
                    Municipio = d.municipio,
                    Titular = d.titular,
                    Verificacion = d.verificacion,
                    Activo = d.activo,
                    FechaRegistro = d.fechaRegistro,
                    Login = d.login,
                    Propuestas = d.propuestas
                });
            }

            return lista;
        }

        public ResultadoGuardado GuardarCandidato(
            int codigoUsuario, int codigoCandidato, string nombres, string apellidos,
            int codigoCampana, int codigoCargo, int codigoPartido, int codigoDepartamento,
            string municipio, string titular)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.guardarCandidato(codigoUsuario, codigoCandidato, nombres, apellidos,
                                        codigoCampana, codigoCargo, codigoPartido,
                                        codigoDepartamento, municipio, titular), null);

            return AGuardado(d);
        }

        public Resultado CambiarEstadoCandidato(
            int codigoUsuario, int codigoCandidato, bool activo, string motivo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.cambiarEstadoCandidato(codigoUsuario, codigoCandidato, activo, motivo), null);

            return ARespuesta(d);
        }

        public Resultado CrearCuentaCandidato(
            int codigoUsuario, int codigoCandidato, string login, string correo, string clave)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.crearCuentaCandidato(codigoUsuario, codigoCandidato, login, correo, clave), null);

            return ARespuesta(d);
        }

        // ------------------------------------------------- Encuestas admin

        public IList<EncuestaAdmin> ObtenerEncuestasAdmin(
            int codigoUsuario, string campanaSlug, string estado)
        {
            ws.EncuestaAdmin[] datos = Ejecutar(
                c => c.listarEncuestasAdmin(codigoUsuario, Sesion.CodigoEspacio, campanaSlug, estado),
                new ws.EncuestaAdmin[0]);

            List<EncuestaAdmin> lista = new List<EncuestaAdmin>();
            if (datos == null) return lista;

            foreach (ws.EncuestaAdmin d in datos)
            {
                lista.Add(new EncuestaAdmin
                {
                    Codigo = d.codigoEncuesta,
                    CodigoCampana = d.codigoCampana,
                    CampanaSlug = d.campanaSlug,
                    Campana = d.campana,
                    Pregunta = d.pregunta,
                    Descripcion = d.descripcion,
                    CodigoCategoria = d.codigoCategoria,
                    Categoria = d.categoria,
                    FechaInicio = d.fechaInicio,
                    FechaCierre = d.fechaCierre,
                    Activo = d.activo,
                    MotivoBaja = d.motivoBaja,
                    Estado = d.estado,
                    Opciones = d.opciones,
                    Votos = d.votos
                });
            }

            return lista;
        }

        public IList<OpcionEncuesta> ObtenerOpcionesEncuestaAdmin(
            int codigoUsuario, int codigoEncuesta)
        {
            return AOpcionesEncuesta(Ejecutar(
                c => c.listarOpcionesEncuestaAdmin(codigoUsuario, codigoEncuesta),
                new ws.OpcionEncuesta[0]));
        }

        public ResultadoGuardado GuardarEncuesta(
            int codigoUsuario, int codigoEncuesta, int codigoCampana,
            string pregunta, string descripcion, int codigoCategoria,
            DateTime fechaInicio, string fechaCierre, string opciones)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.guardarEncuesta(codigoUsuario, codigoEncuesta, codigoCampana,
                                       pregunta, descripcion, codigoCategoria,
                                       fechaInicio, fechaCierre, opciones), null);

            return AGuardado(d);
        }

        public Resultado CambiarEstadoEncuesta(
            int codigoUsuario, int codigoEncuesta, string accion, string motivo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.cambiarEstadoEncuesta(codigoUsuario, codigoEncuesta, accion, motivo), null);

            return ARespuesta(d);
        }

        public IList<OpcionCatalogo> ObtenerCargosConCodigo()
        {
            return AOpciones(Ejecutar(c => c.listarCargos(Espacios.SlugActual), new ws.Catalogo[0]));
        }

        public IList<OpcionCatalogo> ObtenerDepartamentosConCodigo()
        {
            return AOpciones(Ejecutar(c => c.listarDepartamentos(), new ws.Catalogo[0]));
        }

        public IList<OpcionCatalogo> ObtenerCategoriasConCodigo()
        {
            return AOpciones(Ejecutar(c => c.listarCategorias(Espacios.SlugActual), new ws.Catalogo[0]));
        }

        private static IList<OpcionCatalogo> AOpciones(ws.Catalogo[] datos)
        {
            List<OpcionCatalogo> lista = new List<OpcionCatalogo>();
            if (datos == null) return lista;

            foreach (ws.Catalogo d in datos)
            {
                lista.Add(new OpcionCatalogo { Codigo = d.codigo, Nombre = d.nombre });
            }

            return lista;
        }

        private static ResultadoGuardado AGuardado(ws.RespuestaGuardado d)
        {
            if (d == null)
            {
                return new ResultadoGuardado
                {
                    Ok = false,
                    Mensaje = "No se pudo contactar al servidor. Intentá de nuevo.",
                    Codigo = 0
                };
            }

            return new ResultadoGuardado { Ok = d.ok, Mensaje = d.mensaje, Codigo = d.codigo };
        }


        public IList<CargoAdmin> ObtenerCargosAdmin(int codigoUsuario)
        {
            ws.CargoAdmin[] datos = Ejecutar(
                c => c.listarCargosAdmin(codigoUsuario, Sesion.CodigoEspacio), new ws.CargoAdmin[0]);

            List<CargoAdmin> lista = new List<CargoAdmin>();
            if (datos == null) return lista;

            foreach (ws.CargoAdmin d in datos)
            {
                lista.Add(new CargoAdmin
                {
                    Codigo = d.codigoCargo,
                    Nombre = d.nombre ?? string.Empty,
                    NivelGobierno = d.nivelGobierno ?? string.Empty,
                    Orden = d.orden,
                    Activo = d.activo,
                    Candidaturas = d.candidaturas
                });
            }

            return lista;
        }

        public ResultadoGuardado GuardarCargo(int codigoUsuario, int codigoCargo, string nombre, string nivelGobierno, int orden)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.guardarCargo(codigoUsuario, Sesion.CodigoEspacio, codigoCargo, nombre, nivelGobierno, orden), null);

            return AGuardado(d);
        }

        public Resultado CambiarEstadoCargo(int codigoUsuario, int codigoCargo, bool activo, string motivo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.cambiarEstadoCargo(codigoUsuario, codigoCargo, activo, motivo), null);

            return ARespuesta(d);
        }

        // ------------------------------------------------------ Espacios

        public Espacio ObtenerEspacio(string slug)
        {
            ws.Espacio d = Ejecutar(c => c.obtenerEspacio(slug ?? string.Empty), null);
            return d == null ? null : AEspacio(d);
        }

        public IList<Espacio> ObtenerEspacios(int codigoUsuario, bool soloActivos)
        {
            ws.Espacio[] datos = Ejecutar(
                c => c.listarEspacios(codigoUsuario, soloActivos), new ws.Espacio[0]);

            List<Espacio> lista = new List<Espacio>();
            if (datos == null) return lista;

            foreach (ws.Espacio d in datos) lista.Add(AEspacio(d));
            return lista;
        }

        public ResultadoGuardado GuardarEspacio(
            int codigoUsuario, int codigoEspacio, string nombre, string organizacion,
            string descripcion, bool padronCerrado, string terminoAgrupacion)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.guardarEspacio(codigoUsuario, codigoEspacio, nombre, organizacion,
                                      descripcion, padronCerrado, terminoAgrupacion), null);

            return AGuardado(d);
        }

        public Resultado CambiarEstadoEspacio(int codigoUsuario, int codigoEspacio, bool activo, string motivo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.cambiarEstadoEspacio(codigoUsuario, codigoEspacio, activo, motivo), null);

            return ARespuesta(d);
        }

        public Resultado CrearCuentaEspacio(
            int codigoUsuario, int codigoEspacio, string login, string nombre, string correo, string clave)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.crearCuentaEspacio(codigoUsuario, codigoEspacio, login, nombre, correo, clave), null);

            return ARespuesta(d);
        }

        public IList<Miembro> ObtenerPadron(int codigoUsuario, int codigoEspacio)
        {
            ws.Miembro[] datos = Ejecutar(
                c => c.listarPadron(codigoUsuario, codigoEspacio), new ws.Miembro[0]);

            List<Miembro> lista = new List<Miembro>();
            if (datos == null) return lista;

            foreach (ws.Miembro d in datos)
            {
                lista.Add(new Miembro
                {
                    Codigo = d.codigoMiembro,
                    Correo = d.correo ?? string.Empty,
                    Activo = d.activo,
                    FechaAlta = d.fechaAlta,
                    Nombre = d.nombre ?? string.Empty,
                    TieneCuenta = d.tieneCuenta,
                    PuedeParticipar = d.puedeParticipar
                });
            }

            return lista;
        }

        public ResultadoPadron CargarPadron(int codigoUsuario, int codigoEspacio, string correos)
        {
            ws.RespuestaPadron d = Ejecutar(
                c => c.cargarPadron(codigoUsuario, codigoEspacio, correos), null);

            if (d == null)
                return new ResultadoPadron { Ok = false, Mensaje = "No se pudo contactar al servidor.", Rechazados = string.Empty };

            return new ResultadoPadron { Ok = d.ok, Mensaje = d.mensaje, Rechazados = d.rechazados ?? string.Empty };
        }

        public Resultado CambiarEstadoMiembro(int codigoUsuario, int codigoMiembro, bool activo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.cambiarEstadoMiembro(codigoUsuario, codigoMiembro, activo), null);

            return ARespuesta(d);
        }

        public IList<Suscripcion> ObtenerSuscripciones(int codigoUsuario, int codigoEspacio)
        {
            ws.Suscripcion[] datos = Ejecutar(
                c => c.listarSuscripciones(codigoUsuario, codigoEspacio), new ws.Suscripcion[0]);

            List<Suscripcion> lista = new List<Suscripcion>();
            if (datos == null) return lista;

            foreach (ws.Suscripcion d in datos)
            {
                lista.Add(new Suscripcion
                {
                    Codigo = d.codigoSuscripcion,
                    Plan = d.plan ?? string.Empty,
                    VigenteDesde = d.vigenteDesde,
                    VigenteHasta = d.vigenteHasta,
                    Monto = d.monto,
                    Moneda = d.moneda ?? string.Empty,
                    Referencia = d.referenciaPago ?? string.Empty,
                    Notas = d.notas ?? string.Empty,
                    RegistradoPor = d.registradoPor ?? string.Empty,
                    FechaRegistro = d.fechaRegistro,
                    Vigente = d.vigente
                });
            }

            return lista;
        }

        public ResultadoGuardado RegistrarPago(
            int codigoUsuario, int codigoEspacio, string plan, DateTime vigenteDesde, DateTime vigenteHasta,
            decimal monto, string moneda, string referencia, string notas, int codigoPlan, int maxMiembros)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.registrarPago(codigoUsuario, codigoEspacio, plan, vigenteDesde, vigenteHasta,
                                     monto, moneda, referencia, notas, codigoPlan, maxMiembros), null);

            return AGuardado(d);
        }

        public IList<Plan> ObtenerPlanes()
        {
            ws.Plan[] datos = Ejecutar(c => c.listarPlanes(), new ws.Plan[0]);

            List<Plan> lista = new List<Plan>();
            if (datos == null) return lista;

            foreach (ws.Plan d in datos)
            {
                lista.Add(new Plan
                {
                    Codigo = d.codigoPlan,
                    Clave = d.clave ?? string.Empty,
                    Nombre = d.nombre ?? string.Empty,
                    Lema = d.lema ?? string.Empty,
                    Descripcion = d.descripcion ?? string.Empty,
                    Precio = d.precio,
                    Moneda = d.moneda ?? "HNL",
                    Dias = d.dias,
                    MaxMiembros = d.maxMiembros,
                    Destacado = d.destacado
                });
            }

            return lista;
        }

        public ResultadoGuardado EnviarSolicitud(
            string organizacion, string contacto, string correo, string telefono,
            int codigoPlan, string proceso, string fechaAproximada, string mensaje)
        {
            ws.RespuestaGuardado d = Ejecutar(
                c => c.enviarSolicitud(organizacion, contacto, correo, telefono,
                                       codigoPlan, proceso, fechaAproximada, mensaje), null);

            return AGuardado(d);
        }

        public IList<Solicitud> ObtenerSolicitudes(int codigoUsuario, string estado)
        {
            ws.Solicitud[] datos = Ejecutar(
                c => c.listarSolicitudes(codigoUsuario, estado ?? string.Empty), new ws.Solicitud[0]);

            List<Solicitud> lista = new List<Solicitud>();
            if (datos == null) return lista;

            foreach (ws.Solicitud d in datos)
            {
                lista.Add(new Solicitud
                {
                    Codigo = d.codigoSolicitud,
                    Organizacion = d.organizacion ?? string.Empty,
                    Contacto = d.nombreContacto ?? string.Empty,
                    Correo = d.correo ?? string.Empty,
                    Telefono = d.telefono ?? string.Empty,
                    CodigoPlan = d.codigoPlan,
                    Plan = d.plan ?? string.Empty,
                    Proceso = d.proceso ?? string.Empty,
                    FechaAproximada = d.fechaAproximada,
                    Mensaje = d.mensaje ?? string.Empty,
                    Estado = d.estado ?? string.Empty,
                    FechaRegistro = d.fechaRegistro,
                    CodigoEspacio = d.codigoEspacio,
                    Espacio = d.espacio ?? string.Empty,
                    AtendidaPor = d.atendidaPor ?? string.Empty,
                    FechaAtencion = d.fechaAtencion,
                    Notas = d.notas ?? string.Empty
                });
            }

            return lista;
        }

        public Resultado AtenderSolicitud(int codigoUsuario, int codigoSolicitud, string estado, int codigoEspacio, string notas)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.atenderSolicitud(codigoUsuario, codigoSolicitud, estado, codigoEspacio, notas), null);

            return ARespuesta(d);
        }

        private static Espacio AEspacio(ws.Espacio d)
        {
            return new Espacio
            {
                Codigo = d.codigoEspacio,
                Slug = d.slug ?? string.Empty,
                Nombre = d.nombre ?? string.Empty,
                Organizacion = d.organizacion ?? string.Empty,
                Descripcion = d.descripcion ?? string.Empty,
                EsPlataforma = d.esPlataforma,
                PadronCerrado = d.padronCerrado,
                TerminoAgrupacion = d.terminoAgrupacion ?? "Partido",
                Activo = d.activo,
                Estado = d.estado ?? string.Empty,
                MotivoBaja = d.motivoBaja ?? string.Empty,
                FechaCreacion = d.fechaCreacion,
                CodigoPropietario = d.codigoUsuarioPropietario,
                Propietario = d.propietario ?? string.Empty,
                PropietarioLogin = d.propietarioLogin ?? string.Empty,
                PropietarioCorreo = d.propietarioCorreo ?? string.Empty,
                Campanas = d.campanas,
                Candidaturas = d.candidaturas,
                Administradores = d.administradores,
                VigenteHasta = d.vigenteHasta,
                Pagos = d.pagos
            };
        }

        // ------------------------------------------------------- Módulos

        public IList<EstadoModulo> ObtenerModulosVisibles()
        {
            ws.EstadoModulo[] datos = Ejecutar(
                c => c.listarModulosVisibles(), new ws.EstadoModulo[0]);

            List<EstadoModulo> lista = new List<EstadoModulo>();
            if (datos == null) return lista;

            foreach (ws.EstadoModulo d in datos)
            {
                lista.Add(new EstadoModulo { Clave = d.clave, Visible = d.visible });
            }

            return lista;
        }

        public IList<ModuloAdmin> ObtenerModulosAdmin(int codigoUsuario)
        {
            ws.ModuloAdmin[] datos = Ejecutar(
                c => c.listarModulosAdmin(codigoUsuario), new ws.ModuloAdmin[0]);

            List<ModuloAdmin> lista = new List<ModuloAdmin>();
            if (datos == null) return lista;

            foreach (ws.ModuloAdmin d in datos)
            {
                lista.Add(new ModuloAdmin
                {
                    Codigo = d.codigoModulo,
                    Clave = d.clave,
                    Nombre = d.nombre,
                    Descripcion = d.descripcion,
                    Grupo = d.grupo,
                    ClavePadre = d.clavePadre,
                    Habilitado = d.habilitado,
                    Visible = d.visible,
                    ApagadoPorPadre = d.apagadoPorPadre,
                    FechaCambio = d.fechaCambio,
                    CambiadoPor = d.cambiadoPor
                });
            }

            return lista;
        }

        public Resultado CambiarEstadoModulo(
            int codigoUsuario, string clave, bool habilitado, string motivo)
        {
            ws.RespuestaAdmin d = Ejecutar(
                c => c.cambiarEstadoModulo(codigoUsuario, clave, habilitado, motivo), null);

            return ARespuesta(d);
        }

        // =============================================================
        //  Asistente
        // =============================================================

        /// <summary>
        /// Le pasa la pregunta al backend, que es quien tiene la clave del
        /// modelo y quien comprueba sesión, cuota y módulo.
        ///
        /// Es la llamada más lenta del proyecto, entre diez y veinte
        /// segundos, y por eso la página la hace desde script y no en un
        /// postback. Acá no se hace nada especial por eso: el que espera es
        /// el hilo que atiende la petición asíncrona, no la carga de la
        /// página.
        /// </summary>
        public RespuestaAsistente PreguntarAsistente(
            int codigoUsuario, string pregunta, string campanaSlug)
        {
            ws.RespuestaAsistente d = Ejecutar(
                c => c.preguntarAsistente(codigoUsuario, pregunta, campanaSlug), null);

            // Nulo es que el backend no respondió. Se distingue del rechazo,
            // que sí trae su propio motivo desde el servicio.
            if (d == null)
                return new RespuestaAsistente
                {
                    Ok = false,
                    Mensaje = "No se pudo comunicar con el servicio. "
                            + "Los gráficos del tablero siguen disponibles."
                };

            return new RespuestaAsistente
            {
                Ok = d.ok,
                Respuesta = d.respuesta ?? string.Empty,
                Mensaje = d.mensaje ?? string.Empty,
                Restantes = d.restantes,
                Fuentes = d.fuentes == null
                        ? new List<string>()
                        : new List<string>(d.fuentes)
            };
        }

        public IList<ConsultaAsistente> ObtenerConsultasAsistente(int codigoUsuario)
        {
            return AConsultas(Ejecutar(
                c => c.listarConsultasIA(codigoUsuario), new ws.ConsultaIA[0]));
        }

        public IList<ConsultaAsistente> ObtenerReporteAsistente(int codigoUsuario, IList<int> codigos)
        {
            int[] lista = new List<int>(codigos).ToArray();

            return AConsultas(Ejecutar(
                c => c.reporteConsultasIA(codigoUsuario, lista), new ws.ConsultaIA[0]));
        }

        private static IList<ConsultaAsistente> AConsultas(ws.ConsultaIA[] datos)
        {
            List<ConsultaAsistente> lista = new List<ConsultaAsistente>();
            if (datos == null) return lista;

            foreach (ws.ConsultaIA d in datos)
            {
                lista.Add(new ConsultaAsistente
                {
                    Codigo = d.codigoConsulta,
                    Pregunta = d.pregunta ?? string.Empty,
                    Respuesta = d.respuesta ?? string.Empty,
                    Fecha = d.fecha
                });
            }

            return lista;
        }

        /// <summary>
        /// Convierte la respuesta del servicio, distinguiendo el rechazo de la
        /// acción (que trae su propio mensaje) de la caída del backend.
        /// </summary>
        private static Resultado ARespuesta(ws.RespuestaAdmin d)
        {
            if (d == null)
            {
                return new Resultado
                {
                    Ok = false,
                    Mensaje = "No se pudo contactar al servidor. Intentá de nuevo."
                };
            }

            return new Resultado { Ok = d.ok, Mensaje = d.mensaje };
        }

        private static NivelVerificacion AVerificacion(string valor)
        {
            if (string.Equals(valor, "Verificado", StringComparison.OrdinalIgnoreCase)) return NivelVerificacion.Verificado;
            if (string.Equals(valor, "En revisión", StringComparison.OrdinalIgnoreCase)) return NivelVerificacion.EnRevision;
            return NivelVerificacion.Declarado;
        }

        private static EstadoPropuesta AEstadoPropuesta(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return EstadoPropuesta.Declarada;

            switch (valor.ToLowerInvariant())
            {
                case "sin avance": return EstadoPropuesta.SinAvance;
                case "en proceso": return EstadoPropuesta.EnProceso;
                case "estancada": return EstadoPropuesta.Estancada;
                case "cumplida a medias": return EstadoPropuesta.CumplidaAMedias;
                case "cumplida": return EstadoPropuesta.Cumplida;
                case "incumplida": return EstadoPropuesta.Incumplida;
                default: return EstadoPropuesta.Declarada;
            }
        }

        /// <summary>
        /// Iniciales a partir del nombre completo que envía el backend, para el
        /// avatar cuando la candidatura no tiene fotografía cargada.
        /// </summary>
        private static string InicialesDe(string nombreCompleto)
        {
            if (string.IsNullOrEmpty(nombreCompleto)) return string.Empty;

            string[] partes = nombreCompleto.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return string.Empty;
            if (partes.Length == 1) return partes[0].Substring(0, 1).ToUpperInvariant();

            return (partes[0].Substring(0, 1) + partes[partes.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }
    }
}
