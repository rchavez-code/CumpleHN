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
            ws.Campana[] datos = Ejecutar(c => c.listarCampanas(), new ws.Campana[0]);

            List<Campana> lista = new List<Campana>();
            foreach (ws.Campana d in datos) lista.Add(AModelo(d));
            return lista;
        }

        public Campana ObtenerCampanaActual()
        {
            ws.Campana d = Ejecutar(c => c.obtenerCampanaActual(), null);
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
                Ejecutar(c => c.listarCandidatos(campanaSlug ?? string.Empty), new ws.Candidato[0]);

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
                Ejecutar(c => c.listarPropuestasDeCampana(campanaSlug ?? string.Empty), new ws.Propuesta[0]);

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
                Ejecutar(c => c.listarFeed(campanaSlug ?? string.Empty), new ws.Publicacion[0]);

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
            ws.Partido[] datos = Ejecutar(c => c.listarPartidos(), new ws.Partido[0]);

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
        //  Analítica
        // =============================================================

        public Analitica ObtenerAnalitica(FiltroAnalitica filtro)
        {
            if (filtro == null) filtro = new FiltroAnalitica();

            ws.FiltroAnalitica f = new ws.FiltroAnalitica
            {
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
            return Nombres(Ejecutar(c => c.listarCategorias(), new ws.Catalogo[0]));
        }

        public IList<string> ObtenerCargos()
        {
            return Nombres(Ejecutar(c => c.listarCargos(), new ws.Catalogo[0]));
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
                CampanaSlug = d.campanaSlug,
                Nombres = d.nombres,
                Apellidos = d.apellidos,
                Partido = d.partido,
                PartidoSiglas = d.partidoSiglas,
                Cargo = d.cargo,
                Nivel = ANivel(d.nivelGobierno),
                Departamento = d.departamento,
                Municipio = d.municipio,
                FotoUrl = d.fotoUrl,
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
                CandidatoFotoUrl = d.candidatoFotoUrl,
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
