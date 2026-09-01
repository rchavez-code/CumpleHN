using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Tablero analítico.
    ///
    /// El reparto de responsabilidades es estricto:
    ///
    ///   la base de datos   calcula      (vistas de detalle y procedimientos)
    ///   el Web Service     transporta   (sin una línea de SQL escrita a mano)
    ///   el modelo          interpreta   (KPI, hallazgos, proporciones)
    ///   esta página        dibuja       (anchos, clases y SVG)
    ///
    /// Ninguna cifra se calcula acá. Lo que esta clase produce son longitudes
    /// de barra, coordenadas de un trazo y nombres de clase.
    ///
    /// Sobre lo que el tablero NO muestra: el diseño pedía comparar cada
    /// indicador contra el período anterior, con su flecha y su porcentaje de
    /// variación. Los registros de participación de esta etapa abarcan tres
    /// días, así que esa variación sería una cifra inventada con apariencia de
    /// dato. En su lugar cada indicador se compara contra su propio total,
    /// que es la comparación que los datos sí sostienen, y las comparaciones
    /// entre entidades —categoría contra categoría, candidatura contra
    /// candidatura, partido contra partido— hacen el resto del trabajo.
    /// </summary>
    public partial class AnaliticaPagina : Page
    {
        private Analitica _datos;
        private FiltroAnalitica _filtro;

        /// <summary>Cultura invariante para todo lo que va dentro de un atributo CSS o SVG.</summary>
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        protected Analitica Datos { get { return _datos; } }
        protected FiltroAnalitica Filtro { get { return _filtro; } }

        // =============================================================
        //  Ciclo de la página
        // =============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            _filtro = LeerFiltro();
            _datos = Contenido.Datos.ObtenerAnalitica(_filtro);

            // En la primera carga los desplegables llegan vacíos: se llenan con
            // los catálogos que trajo la misma respuesta del tablero, de modo
            // que no haga falta una segunda llamada al backend solo para eso.
            if (!IsPostBack) LlenarFiltros();

            // Los repetidores se enlazan en cada carga, también en los
            // postbacks. Un repetidor sin enlazar deja las tarjetas sin modelo
            // al procesar el clic.
            Enlazar();
        }

        /// <summary>
        /// Arma la selección a partir de los controles. En la primera carga
        /// todos están vacíos y el resultado es la vista general.
        /// </summary>
        private FiltroAnalitica LeerFiltro()
        {
            return new FiltroAnalitica
            {
                CampanaSlug = ddlCampana.SelectedValue ?? string.Empty,
                Categoria = Codigo(ddlCategoria),
                Partido = Codigo(ddlPartido),
                Departamento = Codigo(ddlDepartamento),
                NivelGobierno = ddlNivel.SelectedValue ?? string.Empty,
                Desde = FechaValida(txtDesde.Text),
                Hasta = FechaValida(txtHasta.Text)
            };
        }

        private static int Codigo(DropDownList lista)
        {
            int n;
            return int.TryParse(lista.SelectedValue, out n) ? n : 0;
        }

        /// <summary>
        /// Acepta la fecha solo si tiene la forma aaaa-MM-dd. Cualquier otra
        /// cosa se descarta en silencio: el filtro es una comodidad y no debe
        /// poder dejar el tablero en un estado de error.
        /// </summary>
        private static string FechaValida(string texto)
        {
            DateTime f;
            string v = (texto ?? string.Empty).Trim();

            if (v.Length == 0) return string.Empty;

            return DateTime.TryParseExact(v, "yyyy-MM-dd", Inv, DateTimeStyles.None, out f)
                ? v : string.Empty;
        }

        private void LlenarFiltros()
        {
            foreach (OpcionFiltro o in _datos.OpcionesDe("Campana"))
                ddlCampana.Items.Add(new ListItem(o.Texto, o.Valor));

            if (!string.IsNullOrEmpty(_datos.CampanaSlug))
                ddlCampana.SelectedValue = _datos.CampanaSlug;

            Poblar(ddlCategoria, "Categoria", "Todas las categorías");
            Poblar(ddlPartido, "Partido", "Todos los partidos");
            Poblar(ddlDepartamento, "Departamento", "Todos los departamentos");
            Poblar(ddlNivel, "NivelGobierno", "Todos los niveles");
        }

        private void Poblar(DropDownList lista, string grupo, string todas)
        {
            lista.Items.Add(new ListItem(todas, string.Empty));

            foreach (OpcionFiltro o in _datos.OpcionesDe(grupo))
                lista.Items.Add(new ListItem(o.Texto, o.Valor));
        }

        private void Enlazar()
        {
            // Estados de la página. Se excluyen entre sí.
            phError.Visible = _datos.Error;
            phVacio.Visible = !_datos.Error && _datos.SinDatos;
            phTablero.Visible = !_datos.Error && !_datos.SinDatos;

            rptFichas.DataSource = FichasDeFiltro();
            rptFichas.DataBind();

            if (!phTablero.Visible)
            {
                // Sin tablero que dibujar, el asistente tampoco tiene cifras
                // sobre las cuales responder.
                rptSugerencias.DataSource = Preguntas;
                rptSugerencias.DataBind();
                return;
            }

            rptKpis.DataSource = _datos.Kpis;
            rptKpis.DataBind();

            IList<Hallazgo> hallazgos = _datos.Hallazgos;
            rptHallazgos.DataSource = hallazgos;
            rptHallazgos.DataBind();
            phSinHallazgos.Visible = hallazgos.Count == 0;

            rptOferta.DataSource = _datos.Categorias;
            rptOferta.DataBind();
            rptOfertaTabla.DataSource = _datos.Categorias;
            rptOfertaTabla.DataBind();

            rptEstados.DataSource = _datos.Estados;
            rptEstados.DataBind();
            litPilaEstados.Text = PilaEstados();

            rptPartidos.DataSource = _datos.Partidos;
            rptPartidos.DataBind();
            rptDensidad.DataSource = _datos.Partidos;
            rptDensidad.DataBind();
            rptPartidosTabla.DataSource = _datos.Partidos;
            rptPartidosTabla.DataBind();

            rptParticipacion.DataSource = _datos.Participacion;
            rptParticipacion.DataBind();
            rptParticipacionTabla.DataSource = _datos.Participacion;
            rptParticipacionTabla.DataBind();

            rptRanking.DataSource = _datos.Candidatos;
            rptRanking.DataBind();
            rptRankingTabla.DataSource = _datos.Candidatos;
            rptRankingTabla.DataBind();

            rptTerritorio.DataSource = _datos.Territorio;
            rptTerritorio.DataBind();
            rptTerritorioTabla.DataSource = _datos.Territorio;
            rptTerritorioTabla.DataBind();

            rptVerificacion.DataSource = EntidadesVerificables;
            rptVerificacion.DataBind();

            litDona.Text = DibujarDona();
            litActividad.Text = DibujarActividad();

            rptSugerencias.DataSource = Preguntas;
            rptSugerencias.DataBind();
        }

        // =============================================================
        //  Filtros
        // =============================================================

        protected void Filtro_Cambiado(object sender, EventArgs e)
        {
            // Page_Load ya releyó el tablero con la selección nueva. El evento
            // existe para que el postback ocurra.
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlCategoria.SelectedIndex = 0;
            ddlPartido.SelectedIndex = 0;
            ddlDepartamento.SelectedIndex = 0;
            ddlNivel.SelectedIndex = 0;
            txtDesde.Text = string.Empty;
            txtHasta.Text = string.Empty;

            _filtro = LeerFiltro();
            _datos = Contenido.Datos.ObtenerAnalitica(_filtro);
            Enlazar();
        }

        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            // El postback ya volvió a consultar el backend. No hay caché que
            // invalidar: cada carga es una consulta nueva.
        }

        /// <summary>Fichas que describen la selección activa, en palabras.</summary>
        private IList<string> FichasDeFiltro()
        {
            List<string> lista = new List<string>();

            if (_filtro.Categoria > 0) lista.Add("Categoría: " + ddlCategoria.SelectedItem.Text);
            if (_filtro.Partido > 0) lista.Add("Partido: " + ddlPartido.SelectedItem.Text);
            if (_filtro.Departamento > 0) lista.Add("Departamento: " + ddlDepartamento.SelectedItem.Text);
            if (!string.IsNullOrEmpty(_filtro.NivelGobierno)) lista.Add("Nivel: " + _filtro.NivelGobierno);

            if (!string.IsNullOrEmpty(_filtro.Desde) || !string.IsNullOrEmpty(_filtro.Hasta))
            {
                lista.Add("Fechas: "
                    + (string.IsNullOrEmpty(_filtro.Desde) ? "sin inicio" : _filtro.Desde)
                    + " a "
                    + (string.IsNullOrEmpty(_filtro.Hasta) ? "sin fin" : _filtro.Hasta));
            }

            return lista;
        }

        // =============================================================
        //  Encabezado
        // =============================================================

        /// <summary>
        /// Hasta cuándo llegan los datos. Es la fecha del registro más
        /// reciente, no la hora del servidor: lo que interesa saber es qué tan
        /// actual es la información, no cuándo se dibujó la página.
        /// </summary>
        protected string UltimaActualizacion
        {
            get
            {
                string f = _datos.Resumen.UltimoRegistro;

                // La frase completa se arma acá y no en el marcado: sin
                // registros, un «Información al» seguido de nada no se lee.
                if (string.IsNullOrEmpty(f)) return "Sin registros en esta selección";

                DateTime d;
                if (!DateTime.TryParseExact(f, "yyyy-MM-dd", Inv, DateTimeStyles.None, out d))
                    return "Información al " + f;

                return "Información al " + Vista.FechaCorta(d);
            }
        }

        protected string Consultado
        {
            get { return string.IsNullOrEmpty(_datos.Generado) ? "—" : _datos.Generado; }
        }

        protected string CampanaNombre
        {
            get
            {
                return string.IsNullOrEmpty(_datos.CampanaNombre)
                    ? "sin campaña seleccionada" : _datos.CampanaNombre;
            }
        }

        protected string Numero(int n) { return Vista.Numero(n); }

        /// <summary>
        /// URL de un archivo estático con la fecha de su última modificación
        /// como parámetro.
        ///
        /// El navegador cachea el CSS y el JS por nombre. Sin esta huella, al
        /// editarlos hay que acordarse de recargar sin caché, y mientras tanto
        /// la página se ve o se comporta como la versión anterior — que es una
        /// forma muy eficiente de perder media hora buscando un error que ya
        /// estaba corregido.
        ///
        /// Cuando el archivo no se puede leer se devuelve la ruta sin huella:
        /// perder el refresco automático es preferible a romper la página.
        /// </summary>
        protected string Recurso(string ruta)
        {
            string url = ResolveUrl(ruta);

            try
            {
                DateTime f = System.IO.File.GetLastWriteTimeUtc(Server.MapPath(ruta));
                return url + "?v=" + f.Ticks.ToString(Inv);
            }
            catch
            {
                return url;
            }
        }

        // =============================================================
        //  Pestañas
        // =============================================================

        /*  Los cuatro paneles se renderizan siempre, en la misma respuesta.
            Cambiar de pestaña no vuelve al servidor: los datos ya están en la
            página. El campo oculto conserva cuál estaba abierta, de modo que
            un postback de filtros devuelva a la persona donde estaba y no la
            mande de vuelta al primer panel.

            El servidor marca la pestaña activa en el HTML, así que sin
            JavaScript la página igual muestra un panel correcto — solo pierde
            la posibilidad de cambiar de uno a otro sin recargar.            */

        private static readonly string[] Pestanas =
            { "hallazgos", "oferta", "participacion", "cobertura" };

        private string _pestana;

        protected string PestanaActiva
        {
            get
            {
                if (_pestana != null) return _pestana;

                string v = Request.Form["gcPestana"];
                _pestana = Array.IndexOf(Pestanas, v) >= 0 ? v : Pestanas[0];

                return _pestana;
            }
        }

        protected string ClaseTab(string nombre)
        {
            return PestanaActiva == nombre ? "is-activa" : string.Empty;
        }

        protected string SeleccionTab(string nombre)
        {
            return PestanaActiva == nombre ? "true" : "false";
        }

        protected string OcultarTab(string nombre)
        {
            return PestanaActiva == nombre ? string.Empty : "hidden=\"hidden\"";
        }

        // =============================================================
        //  Escalas de las barras
        // =============================================================

        /// <summary>
        /// Ancho en porcentaje sobre el máximo de la serie. Se recorta a [0,100]
        /// para que un dato fuera de rango no desborde la pista.
        /// </summary>
        private static string Ancho(decimal valor, decimal maximo)
        {
            if (maximo <= 0 || valor <= 0) return "width:0";

            decimal pct = (valor * 100m) / maximo;
            if (pct > 100) pct = 100;

            return "width:" + pct.ToString("0.##", Inv) + "%";
        }

        // ------------------------------------------- Oferta contra interés

        /// <summary>
        /// Escala común de las dos series. Se toma el mayor de ambas para que
        /// compartan un solo eje: es lo que hace comparables las longitudes.
        /// </summary>
        private decimal MaxOferta
        {
            get
            {
                decimal max = 1;
                foreach (FilaCategoria c in _datos.Categorias)
                {
                    if (c.InteresEncuesta > max) max = c.InteresEncuesta;
                    if (c.PorcentajeOferta > max) max = c.PorcentajeOferta;
                }
                return max;
            }
        }

        protected string AnchoInteres(object o)
        {
            return Ancho(((FilaCategoria)o).InteresEncuesta, MaxOferta);
        }

        protected string AnchoOferta(object o)
        {
            return Ancho(((FilaCategoria)o).PorcentajeOferta, MaxOferta);
        }

        protected string TextoInteres(object o)
        {
            return ((FilaCategoria)o).InteresEncuesta.ToString("0.#") + " %";
        }

        protected string TextoOferta(object o)
        {
            FilaCategoria c = (FilaCategoria)o;
            return c.PorcentajeOferta.ToString("0.#") + " %";
        }

        protected string TextoPropuestas(object o)
        {
            FilaCategoria c = (FilaCategoria)o;
            return Vista.Plural(c.Propuestas, "propuesta", "propuestas");
        }

        protected string TextoBrecha(object o)
        {
            decimal b = ((FilaCategoria)o).Brecha;
            return (b > 0 ? "+" : string.Empty) + b.ToString("0.#") + " pp";
        }

        /// <summary>
        /// Explica en palabras la brecha más grande. Un gráfico que hay que
        /// interpretar solo se lee a medias.
        /// </summary>
        protected string LecturaBrechaTexto
        {
            get
            {
                FilaCategoria c = _datos.MayorBrecha;

                if (c == null || _datos.Resumen.Propuestas == 0)
                    return "No hay propuestas registradas en la selección, así que no hay oferta que "
                         + "contrastar con el interés medido en la encuesta.";

                if (c.Brecha <= 0)
                    return "En esta selección la oferta de propuestas cubre las categorías en una "
                         + "proporción similar al interés que expresó la ciudadanía.";

                return "La brecha más grande está en " + c.Categoria.ToLowerInvariant() + ": concentra "
                     + c.InteresEncuesta.ToString("0.#") + " % del interés ciudadano y solo "
                     + c.PorcentajeOferta.ToString("0.#") + " % de las propuestas registradas, "
                     + c.Brecha.ToString("0.#") + " puntos porcentuales de diferencia.";
            }
        }

        // ------------------------------------------------------- Partidos

        private decimal MaxPropuestasPartido
        {
            get
            {
                decimal max = 1;
                foreach (FilaPartido p in _datos.Partidos)
                    if (p.Propuestas > max) max = p.Propuestas;

                return max;
            }
        }

        private decimal MaxDensidad
        {
            get
            {
                decimal max = 0.1m;
                foreach (FilaPartido p in _datos.Partidos)
                    if (p.PropuestasPorCandidatura > max) max = p.PropuestasPorCandidatura;

                return max;
            }
        }

        protected string AnchoPartido(object o)
        {
            return Ancho(((FilaPartido)o).Propuestas, MaxPropuestasPartido);
        }

        protected string AnchoDensidad(object o)
        {
            return Ancho(((FilaPartido)o).PropuestasPorCandidatura, MaxDensidad);
        }

        protected string TextoDensidad(object o)
        {
            return ((FilaPartido)o).PropuestasPorCandidatura.ToString("0.#");
        }

        protected string SubPartido(object o)
        {
            FilaPartido p = (FilaPartido)o;
            return Vista.Plural(p.Candidaturas, "candidatura", "candidaturas");
        }

        // -------------------------------------------------- Participación

        private decimal MaxParticipacion
        {
            get
            {
                decimal max = 1;
                foreach (FilaParticipacion p in _datos.Participacion)
                    if (p.Valoraciones > max) max = p.Valoraciones;

                return max;
            }
        }

        protected string AnchoAFavor(object o)
        {
            return Ancho(((FilaParticipacion)o).MeGusta, MaxParticipacion);
        }

        protected string AnchoEnContra(object o)
        {
            return Ancho(((FilaParticipacion)o).NoMeGusta, MaxParticipacion);
        }

        /// <summary>
        /// Columna de valor: solo la proporción. El recuento vive en el
        /// subtítulo de la etiqueta — la columna de valor no envuelve, así que
        /// una frase larga acá empuja la pista de la barra hasta anularla en
        /// las columnas angostas.
        /// </summary>
        protected string TextoParticipacion(object o)
        {
            FilaParticipacion p = (FilaParticipacion)o;

            if (p.Valoraciones == 0) return "—";

            return Analitica.Pct(p.MeGusta, p.Valoraciones).ToString("0.#") + " % a favor";
        }

        protected string SubParticipacion(object o)
        {
            FilaParticipacion p = (FilaParticipacion)o;

            if (p.Valoraciones == 0 && p.Comentarios == 0) return "sin participación";

            return Vista.Numero(p.Valoraciones) + " valoraciones · "
                 + Vista.Numero(p.Comentarios) + " comentarios";
        }

        // -------------------------------------- Ranking de candidaturas

        /// <summary>
        /// Escala de la barra divergente. Es el mayor de los dos lados en toda
        /// la tabla, de modo que las dos mitades compartan una sola unidad y
        /// una barra de 20 sea el doble de larga que una de 10 en cualquier
        /// fila y hacia cualquier lado.
        /// </summary>
        private decimal MaxLado
        {
            get
            {
                decimal max = 1;
                foreach (FilaCandidato c in _datos.Candidatos)
                {
                    if (c.MeGusta > max) max = c.MeGusta;
                    if (c.NoMeGusta > max) max = c.NoMeGusta;
                }
                return max;
            }
        }

        protected string AnchoSi(object o)
        {
            return Ancho(((FilaCandidato)o).MeGusta, MaxLado);
        }

        protected string AnchoNo(object o)
        {
            return Ancho(((FilaCandidato)o).NoMeGusta, MaxLado);
        }

        protected string TextoSaldo(object o)
        {
            FilaCandidato c = (FilaCandidato)o;

            if (c.Valoraciones == 0) return "—";

            // El signo se escribe. La flecha sola sería color e ícono sin
            // texto, y el saldo cero no es ni subida ni bajada.
            string signo = c.Saldo > 0 ? "▲ +" : c.Saldo < 0 ? "▼ −" : "— ";
            return signo + Math.Abs(c.Saldo);
        }

        protected string TextoApoyo(object o)
        {
            int pct = ((FilaCandidato)o).PorcentajeApoyo;
            return pct < 0 ? "—" : pct + " %";
        }

        protected string SubCandidato(object o)
        {
            FilaCandidato c = (FilaCandidato)o;
            return c.PartidoSiglas + " · " + c.Departamento;
        }

        protected string Posicion(object contenedor)
        {
            return (((RepeaterItem)contenedor).ItemIndex + 1).ToString(Inv);
        }

        // =============================================================
        //  Estados de cumplimiento
        // =============================================================

        /// <summary>
        /// Color por el trabajo del dato: verde lo cumplido, ámbar lo que está
        /// a medio camino, escarlata lo incumplido y gris lo que todavía no se
        /// evaluó. La ponderación del estado decide, no su nombre.
        /// </summary>
        protected static string ClaseEstado(string estado)
        {
            switch (estado)
            {
                case "Cumplida": return "gc-pila__s--ok";
                case "En proceso":
                case "Cumplida a medias":
                case "Estancada": return "gc-pila__s--rev";
                case "Incumplida":
                case "Sin avance": return "gc-pila__s--no";
                default: return "gc-pila__s--dec";
            }
        }

        /// <summary>Misma codificación de color, en el cuadrito de la leyenda.</summary>
        protected string ClaseLeyendaEstado(object o)
        {
            switch (ClaseEstado(((FilaEstado)o).Estado))
            {
                case "gc-pila__s--ok": return "gc-leyenda__m--ok";
                case "gc-pila__s--rev": return "gc-leyenda__m--rev";
                case "gc-pila__s--no": return "gc-leyenda__m--no";
                default: return "gc-leyenda__m--dec";
            }
        }

        protected string TextoEstado(object o)
        {
            FilaEstado e = (FilaEstado)o;
            int total = _datos.Resumen.Propuestas;

            if (total == 0) return "0";

            return e.Propuestas + " · " + Analitica.Pct(e.Propuestas, total).ToString("0.#") + " %";
        }

        private string PilaEstados()
        {
            int total = 0;
            foreach (FilaEstado e in _datos.Estados) total += e.Propuestas;

            if (total == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (FilaEstado e in _datos.Estados)
            {
                if (e.Propuestas <= 0) continue;

                sb.Append("<span class=\"gc-pila__s ").Append(ClaseEstado(e.Estado));
                sb.Append("\" style=\"width:")
                  .Append(Analitica.Pct(e.Propuestas, total).ToString("0.##", Inv))
                  .Append("%\" title=\"")
                  .Append(Server.HtmlEncode(e.Estado + ": " + e.Propuestas))
                  .Append("\"></span>");
            }

            return sb.ToString();
        }

        protected string LecturaEstados
        {
            get
            {
                ResumenAnalitica r = _datos.Resumen;

                if (r.Propuestas == 0)
                    return "No hay propuestas registradas en la selección actual.";

                if (r.PropuestasEvaluadas == 0)
                    return "Las " + r.Propuestas + " propuestas están en estado declarada, que es el "
                         + "estado en que nacen. El seguimiento de cumplimiento empieza cuando se "
                         + "registran evidencias, y los estados los asigna la plataforma, nunca la "
                         + "propia candidatura. Por eso todavía no hay porcentaje de cumplimiento.";

                return r.PropuestasEvaluadas + " de " + r.Propuestas + " propuestas ya tienen una "
                     + "evaluación de cumplimiento respaldada por evidencia documentada.";
            }
        }

        // =============================================================
        //  Verificación
        // =============================================================

        protected static readonly string[] EntidadesVerificables =
            { "Candidaturas", "Propuestas", "Publicaciones" };

        protected string PilaVerificacion(object entidad)
        {
            string e = Convert.ToString(entidad);
            int total = _datos.TotalEntidad(e);

            if (total == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();

            // El orden es fijo, de más a menos respaldo, para que las tres
            // barras se puedan comparar de un vistazo.
            Segmento(sb, "Verificado", "gc-pila__s--ok", _datos.VerificacionDe(e, "Verificado"), total);
            Segmento(sb, "En revisión", "gc-pila__s--rev", _datos.VerificacionDe(e, "En revisión"), total);
            Segmento(sb, "Declarado", "gc-pila__s--dec", _datos.VerificacionDe(e, "Declarado"), total);

            return sb.ToString();
        }

        private void Segmento(StringBuilder sb, string nivel, string clase, int valor, int total)
        {
            if (valor <= 0) return;

            sb.Append("<span class=\"gc-pila__s ").Append(clase).Append("\" style=\"width:")
              .Append(Analitica.Pct(valor, total).ToString("0.##", Inv))
              .Append("%\" title=\"").Append(Server.HtmlEncode(nivel + ": " + valor))
              .Append("\"></span>");
        }

        protected string TextoVerificacion(object entidad)
        {
            string e = Convert.ToString(entidad);
            int total = _datos.TotalEntidad(e);

            if (total == 0) return "sin registros";

            int ok = _datos.VerificacionDe(e, "Verificado");
            return ok + " de " + total + " verificadas · "
                 + Analitica.Pct(ok, total).ToString("0.#") + " %";
        }

        protected string DetalleVerificacion(object entidad)
        {
            string e = Convert.ToString(entidad);

            return "Verificado " + _datos.VerificacionDe(e, "Verificado")
                 + " · En revisión " + _datos.VerificacionDe(e, "En revisión")
                 + " · Declarado " + _datos.VerificacionDe(e, "Declarado");
        }

        protected string LecturaVerificacion
        {
            get
            {
                int propTotal = _datos.TotalEntidad("Propuestas");
                int propOk = _datos.VerificacionDe("Propuestas", "Verificado");
                int candTotal = _datos.TotalEntidad("Candidaturas");
                int candOk = _datos.VerificacionDe("Candidaturas", "Verificado");

                if (propTotal == 0 && candTotal == 0)
                    return "La selección no tiene contenido sobre el cual medir verificación.";

                if (propTotal > 0 && propOk == 0)
                    return "El contraste está en las propuestas: " + candOk + " de " + candTotal
                         + " candidaturas tienen su perfil verificado, pero ninguna de las "
                         + propTotal + " propuestas cuenta todavía con una fuente que la respalde. "
                         + "Registrar evidencias sobre las propuestas es lo que habilita el "
                         + "seguimiento de cumplimiento.";

                return candOk + " de " + candTotal + " candidaturas y " + propOk + " de "
                     + propTotal + " propuestas cuentan con una fuente verificable registrada.";
            }
        }

        // =============================================================
        //  Territorio
        // =============================================================

        /// <summary>
        /// Paso de la rampa según la cantidad de candidaturas. Tres pasos de un
        /// solo tono, que es lo que corresponde a una magnitud. El valor va
        /// escrito dentro de la ficha, así que el color nunca es el único
        /// portador del dato.
        /// </summary>
        protected string ClaseDepartamento(object o)
        {
            FilaDepartamento d = (FilaDepartamento)o;

            if (d.Candidaturas == 0) return "gc-mapa__f--vacio";

            decimal max = MaxDepartamento;

            if (d.Candidaturas >= max) return "gc-mapa__f--n3";
            if (d.Candidaturas >= max / 2) return "gc-mapa__f--n2";

            return "gc-mapa__f--n1";
        }

        private decimal MaxDepartamento
        {
            get
            {
                decimal max = 1;
                foreach (FilaDepartamento d in _datos.Territorio)
                    if (d.Candidaturas > max) max = d.Candidaturas;

                return max;
            }
        }

        protected string TituloDepartamento(object o)
        {
            FilaDepartamento d = (FilaDepartamento)o;

            return d.Departamento + ": "
                 + Vista.Plural(d.Candidaturas, "candidatura", "candidaturas") + ", "
                 + Vista.Plural(d.Propuestas, "propuesta", "propuestas");
        }

        protected string LecturaTerritorio
        {
            get
            {
                ResumenAnalitica r = _datos.Resumen;

                if (r.DepartamentosConCandidatura == 0)
                    return "Ninguna candidatura de la selección tiene departamento asignado.";

                FilaDepartamento lider = _datos.TerritorioLider;
                string texto = "La plataforma tiene candidaturas registradas en "
                             + r.DepartamentosConCandidatura + " de los " + r.DepartamentosTotal
                             + " departamentos del país";

                if (lider != null)
                    texto += ", y " + lider.Departamento + " concentra "
                           + Vista.Plural(lider.Candidaturas, "candidatura", "candidaturas");

                return texto + ". La cobertura describe qué candidaturas se han registrado en "
                     + "CumpleHN, no cuántas compiten realmente en cada departamento.";
            }
        }

        // =============================================================
        //  Gráficos SVG
        // =============================================================

        /*  El SVG se arma en el servidor por las mismas razones que el resto
            del tablero: el navegador recibe el gráfico ya dibujado, funciona
            sin JavaScript y los <title> que lleva dentro son tooltips nativos
            que además lee el lector de pantalla.                            */

        /// <summary>
        /// Dona de dos gajos: valoraciones a favor y en contra.
        ///
        /// Es el único gráfico circular del tablero, y lo es porque cumple la
        /// única condición que lo justifica: dos partes de un mismo todo. Con
        /// tres o más categorías una barra ordenada siempre se compara mejor.
        ///
        /// Se dibuja con dos circunferencias y stroke-dasharray en lugar de
        /// arcos: no hay trigonometría que revisar y el resultado es idéntico.
        /// </summary>
        private string DibujarDona()
        {
            ResumenAnalitica r = _datos.Resumen;

            const decimal radio = 52m;
            const decimal grosor = 20m;

            decimal circunferencia = Math.Round(2m * 3.1415926535m * radio, 2);

            StringBuilder sb = new StringBuilder();
            sb.Append("<svg class=\"gc-svg\" viewBox=\"0 0 140 140\" role=\"img\" aria-label=\"");

            if (r.Valoraciones == 0)
            {
                sb.Append("Sin valoraciones registradas\">");
                sb.Append("<circle cx=\"70\" cy=\"70\" r=\"").Append(N(radio))
                  .Append("\" fill=\"none\" stroke=\"#eef0f2\" stroke-width=\"").Append(N(grosor)).Append("\" />");
                sb.Append("<text class=\"gc-txt\" x=\"70\" y=\"74\" text-anchor=\"middle\">sin datos</text>");
                sb.Append("</svg>");
                return sb.ToString();
            }

            decimal pctSi = Analitica.Pct(r.MeGusta, r.Valoraciones);
            decimal largoSi = Math.Round(circunferencia * pctSi / 100m, 2);

            sb.Append(Server.HtmlEncode(pctSi.ToString("0.#") + " % de las valoraciones a favor"))
              .Append("\">");

            // Gajo en contra: el anillo completo por debajo.
            sb.Append("<circle class=\"gc-dona--no\" cx=\"70\" cy=\"70\" r=\"").Append(N(radio))
              .Append("\" fill=\"none\" stroke=\"var(--gc-escarlata)\" stroke-width=\"").Append(N(grosor))
              .Append("\"><title>En contra: ").Append(Server.HtmlEncode(Vista.Numero(r.NoMeGusta)))
              .Append("</title></circle>");

            // Gajo a favor encima, recortado con dasharray y girado para que
            // arranque arriba. El anillo de 2 px de superficie separa los dos.
            sb.Append("<circle cx=\"70\" cy=\"70\" r=\"").Append(N(radio))
              .Append("\" fill=\"none\" stroke=\"var(--gc-verde)\" stroke-width=\"").Append(N(grosor))
              .Append("\" stroke-dasharray=\"").Append(N(largoSi)).Append(" ").Append(N(circunferencia))
              .Append("\" transform=\"rotate(-90 70 70)\"><title>A favor: ")
              .Append(Server.HtmlEncode(Vista.Numero(r.MeGusta)))
              .Append("</title></circle>");

            sb.Append("<text class=\"gc-txt gc-txt--v\" x=\"70\" y=\"68\" text-anchor=\"middle\" ")
              .Append("style=\"font-size:20px\">").Append(pctSi.ToString("0.#", Inv)).Append(" %</text>");
            sb.Append("<text class=\"gc-txt\" x=\"70\" y=\"84\" text-anchor=\"middle\">a favor</text>");

            sb.Append("</svg>");
            return sb.ToString();
        }

        /// <summary>
        /// Actividad diaria: área con la línea de valoraciones y una segunda
        /// línea con los comentarios.
        ///
        /// Una sola escala vertical para las dos series. Son magnitudes de la
        /// misma naturaleza —interacciones por día— así que comparten eje sin
        /// distorsionar nada, y un segundo eje inventaría una relación que no
        /// está en los datos.
        /// </summary>
        private string DibujarActividad()
        {
            IList<FilaActividad> dias = _datos.Actividad;

            if (dias.Count == 0)
                return "<p class=\"gc-muted gc-small\">No hay actividad registrada en el rango seleccionado.</p>";

            const decimal ancho = 720m, alto = 210m;
            const decimal izq = 34m, der = 12m, arriba = 14m, abajo = 30m;

            decimal w = ancho - izq - der;
            decimal h = alto - arriba - abajo;

            int max = 1;
            foreach (FilaActividad d in dias)
            {
                if (d.Valoraciones > max) max = d.Valoraciones;
                if (d.Comentarios > max) max = d.Comentarios;
            }

            // Techo redondeado hacia arriba, para que la última marca del eje
            // sea una cifra legible y no el máximo crudo.
            int techo = Techo(max);

            StringBuilder sb = new StringBuilder();
            sb.Append("<svg class=\"gc-svg\" viewBox=\"0 0 ").Append(N(ancho)).Append(" ").Append(N(alto))
              .Append("\" role=\"img\" aria-label=\"")
              .Append(Server.HtmlEncode("Actividad diaria entre " + dias[0].Fecha + " y " + dias[dias.Count - 1].Fecha))
              .Append("\">");

            // --- Malla horizontal y escala vertical, recesivas
            for (int i = 0; i <= 4; i++)
            {
                decimal y = arriba + (h * i / 4m);
                int valor = techo - (techo * i / 4);

                sb.Append("<line class=\"gc-malla\" x1=\"").Append(N(izq)).Append("\" y1=\"").Append(N(y))
                  .Append("\" x2=\"").Append(N(ancho - der)).Append("\" y2=\"").Append(N(y)).Append("\" />");

                sb.Append("<text class=\"gc-txt\" x=\"").Append(N(izq - 7)).Append("\" y=\"").Append(N(y + 3.5m))
                  .Append("\" text-anchor=\"end\">").Append(valor).Append("</text>");
            }

            // --- Coordenadas de cada día
            int n = dias.Count;
            decimal paso = n <= 1 ? 0 : w / (n - 1);

            StringBuilder linea = new StringBuilder();
            StringBuilder coment = new StringBuilder();
            StringBuilder area = new StringBuilder();

            for (int i = 0; i < n; i++)
            {
                decimal x = n == 1 ? izq + w / 2m : izq + paso * i;
                decimal yv = arriba + h - (h * dias[i].Valoraciones / techo);
                decimal yc = arriba + h - (h * dias[i].Comentarios / techo);

                linea.Append(i == 0 ? "M" : "L").Append(N(x)).Append(" ").Append(N(yv)).Append(" ");
                coment.Append(i == 0 ? "M" : "L").Append(N(x)).Append(" ").Append(N(yc)).Append(" ");

                if (i == 0) area.Append("M").Append(N(x)).Append(" ").Append(N(arriba + h)).Append(" ");
                area.Append("L").Append(N(x)).Append(" ").Append(N(yv)).Append(" ");
                if (i == n - 1) area.Append("L").Append(N(x)).Append(" ").Append(N(arriba + h)).Append(" Z");
            }

            sb.Append("<path class=\"gc-area--a\" d=\"").Append(area).Append("\" />");
            sb.Append("<path class=\"gc-linea gc-linea--a\" d=\"").Append(linea.ToString().TrimEnd()).Append("\" />");
            sb.Append("<path class=\"gc-linea gc-linea--b\" d=\"").Append(coment.ToString().TrimEnd()).Append("\" />");

            // --- Eje inferior
            sb.Append("<line class=\"gc-eje\" x1=\"").Append(N(izq)).Append("\" y1=\"").Append(N(arriba + h))
              .Append("\" x2=\"").Append(N(ancho - der)).Append("\" y2=\"").Append(N(arriba + h)).Append("\" />");

            // --- Puntos con su tooltip nativo. El anillo de superficie de 2 px
            //     los separa cuando dos días quedan encimados.
            for (int i = 0; i < n; i++)
            {
                decimal x = n == 1 ? izq + w / 2m : izq + paso * i;
                decimal yv = arriba + h - (h * dias[i].Valoraciones / techo);

                sb.Append("<circle class=\"gc-punto gc-punto--a\" cx=\"").Append(N(x)).Append("\" cy=\"")
                  .Append(N(yv)).Append("\" r=\"4\"><title>")
                  .Append(Server.HtmlEncode(Etiqueta(dias[i]))).Append("</title></circle>");
            }

            // --- Etiquetas de fecha: la primera, la última y a lo sumo tres
            //     intermedias, para que no se pisen entre sí.
            int salto = Math.Max(1, (int)Math.Ceiling(n / 5m));

            for (int i = 0; i < n; i++)
            {
                bool mostrar = i == 0 || i == n - 1 || i % salto == 0;
                if (!mostrar) continue;

                decimal x = n == 1 ? izq + w / 2m : izq + paso * i;
                string ancla = i == 0 ? "start" : i == n - 1 ? "end" : "middle";

                sb.Append("<text class=\"gc-txt\" x=\"").Append(N(x)).Append("\" y=\"")
                  .Append(N(alto - 10)).Append("\" text-anchor=\"").Append(ancla).Append("\">")
                  .Append(Server.HtmlEncode(DiaCorto(dias[i]))).Append("</text>");
            }

            sb.Append("</svg>");
            return sb.ToString();
        }

        /// <summary>Redondea el techo del eje al siguiente múltiplo legible.</summary>
        private static int Techo(int max)
        {
            if (max <= 4) return 4;

            int paso = max <= 20 ? 4 : max <= 100 ? 20 : max <= 500 ? 100 : 500;
            return ((max + paso - 1) / paso) * paso;
        }

        private static string Etiqueta(FilaActividad d)
        {
            return DiaCorto(d) + ": " + d.Valoraciones + " valoraciones, " + d.Comentarios + " comentarios";
        }

        private static string DiaCorto(FilaActividad d)
        {
            DateTime f = d.Dia;
            return f == DateTime.MinValue ? d.Fecha : Vista.FechaCorta(f);
        }

        /// <summary>Número para un atributo SVG. Siempre con punto decimal.</summary>
        private static string N(decimal v)
        {
            return v.ToString("0.##", Inv);
        }

        protected string LecturaActividad
        {
            get
            {
                FilaActividad pico = _datos.DiaMasActivo;
                int dias = _datos.Actividad.Count;

                if (pico == null)
                    return "No hay interacciones registradas en el rango seleccionado.";

                return "El día de mayor actividad fue el " + DiaCorto(pico) + ", con "
                     + Vista.Plural(pico.Valoraciones, "valoración", "valoraciones") + " y "
                     + Vista.Plural(pico.Comentarios, "comentario", "comentarios") + ". La serie "
                     + "cubre " + Vista.Plural(dias, "día", "días") + " de registro, un período "
                     + "demasiado corto para leer una tendencia: describe la actividad observada, "
                     + "no una proyección.";
            }
        }

        // =============================================================
        //  Asistente, prototipo
        // =============================================================

        private static readonly string[] Preguntas =
        {
            "¿Qué categoría le interesa más a la ciudadanía y cuántas propuestas tiene?",
            "¿Cuántas candidaturas tienen su información verificada?",
            "¿Cuál es el estado de cumplimiento de las propuestas registradas?",
            "¿En qué departamentos hay candidaturas registradas?"
        };

        protected string InicialesUsuario
        {
            get
            {
                string i = Sesion.Iniciales;
                return string.IsNullOrEmpty(i) ? "TÚ" : i;
            }
        }

        protected void rptSugerencias_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "preguntar") return;

            int indice;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out indice)) return;
            if (indice < 0 || indice >= Preguntas.Length) return;

            Responder(Preguntas[indice], indice);
        }

        protected void btnPreguntar_Click(object sender, EventArgs e)
        {
            string pregunta = txtPregunta.Text.Trim();
            if (string.IsNullOrEmpty(pregunta)) return;

            // El prototipo no interpreta lenguaje natural: reconoce la pregunta
            // de ejemplo más parecida por palabras clave. Cuando no reconoce
            // nada lo dice, en lugar de inventar una respuesta.
            Responder(pregunta, Reconocer(pregunta));
            txtPregunta.Text = string.Empty;
        }

        private static int Reconocer(string pregunta)
        {
            string p = pregunta.ToLowerInvariant();

            if (p.Contains("categor") || p.Contains("interes") || p.Contains("prioriza")) return 0;
            if (p.Contains("verific")) return 1;
            if (p.Contains("cumpl") || p.Contains("estado")) return 2;
            if (p.Contains("departamento") || p.Contains("territor") || p.Contains("dónde")
                || p.Contains("donde")) return 3;

            return -1;
        }

        /// <summary>
        /// Arma la respuesta con las cifras del tablero que está en pantalla.
        /// La redacción es fija, los números no, y las fuentes citan el objeto
        /// de base de datos del que sale cada cifra.
        /// </summary>
        private void Responder(string pregunta, int indice)
        {
            litPregunta.Text = Server.HtmlEncode(pregunta);
            phConversacion.Visible = true;

            List<string> fuentes = new List<string>();
            string cuerpo;

            switch (indice)
            {
                case 0:
                    {
                        FilaCategoria mayor = null;
                        foreach (FilaCategoria c in _datos.Categorias)
                            if (mayor == null || c.InteresEncuesta > mayor.InteresEncuesta) mayor = c;

                        if (mayor == null)
                        {
                            cuerpo = "<p>No hay categorías con datos en la selección actual.</p>";
                            break;
                        }

                        cuerpo = "<p>La categoría con mayor interés ciudadano es <strong>"
                               + Server.HtmlEncode(mayor.Categoria) + "</strong>, con "
                               + mayor.InteresEncuesta.ToString("0.#") + " % de las respuestas de la "
                               + "encuesta.</p><p>En esta selección tiene <strong>" + mayor.Propuestas
                               + "</strong> propuestas registradas, el " + mayor.PorcentajeOferta.ToString("0.#")
                               + " % del total documentado. La diferencia entre lo que se prioriza y lo "
                               + "que se propone es de " + mayor.Brecha.ToString("0.#")
                               + " puntos porcentuales.</p>";

                        fuentes.Add("Encuesta del proyecto, tabla VI-17 «Interés sobre los tipos de información de promesa política» (n = 150)");
                        fuentes.Add("Procedimiento spAnaliticaCategorias, campaña " + _datos.CampanaNombre);
                        break;
                    }

                case 1:
                    {
                        int ok = _datos.VerificacionDe("Candidaturas", "Verificado");
                        int rev = _datos.VerificacionDe("Candidaturas", "En revisión");
                        int dec = _datos.VerificacionDe("Candidaturas", "Declarado");

                        cuerpo = "<p>De las " + _datos.Resumen.Candidaturas + " candidaturas de "
                               + Server.HtmlEncode(CampanaNombre) + ", <strong>" + ok
                               + "</strong> tienen su información verificada, " + rev
                               + " está en revisión y " + dec + " se muestran únicamente como "
                               + "declaradas por la propia candidatura.</p>"
                               + "<p>En las propuestas la proporción es distinta: "
                               + _datos.VerificacionDe("Propuestas", "Verificado") + " de "
                               + _datos.TotalEntidad("Propuestas") + " tienen fuente verificable. "
                               + "Verificado significa que existe al menos una fuente registrada que "
                               + "respalda el contenido. Mientras no la haya, la plataforma lo presenta "
                               + "como afirmación de la candidatura y no como hecho comprobado.</p>";

                        fuentes.Add("Procedimiento spAnaliticaVerificacion, campaña " + _datos.CampanaNombre);
                        fuentes.Add("Catálogo NivelesVerificacion de la base de datos");
                        break;
                    }

                case 2:
                    {
                        cuerpo = "<p>" + Server.HtmlEncode(LecturaEstados) + "</p>"
                               + "<p>El esquema de estados que usa la plataforma tiene siete niveles, "
                               + "desde declarada hasta cumplida o incumplida, con los intermedios en "
                               + "proceso, estancada, sin avance y cumplida a medias. Cada cambio de "
                               + "estado exige evidencia documentada.</p>";

                        fuentes.Add("Procedimiento spAnaliticaEstados, campaña " + _datos.CampanaNombre);
                        fuentes.Add("Catálogo EstadosPropuesta, esquema basado en el rastreador de promesas de PolitiFact (2018)");
                        break;
                    }

                case 3:
                    {
                        StringBuilder sb = new StringBuilder();
                        ResumenAnalitica r = _datos.Resumen;

                        sb.Append("<p>Hay candidaturas registradas en <strong>")
                          .Append(r.DepartamentosConCandidatura).Append("</strong> de los ")
                          .Append(r.DepartamentosTotal).Append(" departamentos del país:</p><p>");

                        foreach (FilaDepartamento d in _datos.Territorio)
                        {
                            if (d.Candidaturas == 0) continue;

                            sb.Append("— <strong>").Append(Server.HtmlEncode(d.Departamento))
                              .Append("</strong>: ")
                              .Append(Vista.Plural(d.Candidaturas, "candidatura", "candidaturas"))
                              .Append(" y ").Append(Vista.Plural(d.Propuestas, "propuesta", "propuestas"))
                              .Append(".<br />");
                        }

                        sb.Append("</p><p>Los ").Append(r.DepartamentosTotal - r.DepartamentosConCandidatura)
                          .Append(" departamentos restantes no tienen ninguna candidatura registrada en la "
                                + "plataforma. Eso describe la cobertura de CumpleHN, no la oferta "
                                + "electoral real de cada departamento.</p>");

                        cuerpo = sb.ToString();

                        fuentes.Add("Procedimiento spAnaliticaTerritorio, campaña " + _datos.CampanaNombre);
                        fuentes.Add("Catálogo Departamentos de la base de datos, los 18 departamentos de Honduras");
                        break;
                    }

                default:
                    cuerpo = "<p>Todavía no puedo responder esa pregunta. En esta etapa el asistente es "
                           + "una maqueta y solo atiende las preguntas de ejemplo que aparecen arriba.</p>"
                           + "<p>Cuando se conecte el modelo de lenguaje, la respuesta se va a construir "
                           + "sobre las mismas propuestas y evidencias de la base de datos, y va a citar "
                           + "sus fuentes igual que en los ejemplos.</p>";

                    fuentes.Add("Sin fuentes: el asistente no reconoció la pregunta");
                    break;
            }

            litRespuesta.Text = cuerpo;

            rptFuentes.DataSource = fuentes;
            rptFuentes.DataBind();
        }
    }
}
