using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Portada. Destaca la campaña actual, muestra sus candidaturas y da
    /// entrada a las demás campañas y al registro de candidatos.
    /// </summary>
    public partial class _Default : Page
    {
        private Campana _actual;

        /// <summary>
        /// Las iniciativas se enlazan en <c>OnInit</c>, no en <c>Page_Load</c>.
        ///
        /// Sus tarjetas llevan hilo de comentarios dentro de un repetidor, y
        /// enlazar en Page_Load recrearía la tarjeta después de que ASP.NET ya
        /// procesó el postback, rompiendo la publicación de un comentario. Es
        /// el mismo motivo por el que «Mi cuenta» y el feed de campaña enlazan
        /// acá. Las demás tarjetas de la portada (candidaturas, campañas,
        /// encuestas) no tienen hilo inline, así que siguen en Page_Load.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CargarIniciativas();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _actual = Contenido.Datos.ObtenerCampanaActual();

            // Sin campaña destacada el bloque completo desaparece en lugar de
            // dibujarse vacío.
            phActual.Visible = _actual != null;

            // Se enlaza en cada carga, también en los postbacks, porque las
            // tarjetas de candidato llevan botones de participación y necesitan
            // su modelo presente cuando se procesa el clic.
            CargarCandidatos();
            CargarCampanas();
            CargarEncuestas();
        }

        /// <summary>
        /// Cuántas encuestas se ven sin desplegar. Es una decisión de
        /// presentación y vive acá, no en la base: el procedimiento devuelve
        /// todas las abiertas.
        /// </summary>
        private const int EncuestasALaVista = 2;

        private int _totalEncuestas;

        /// <summary>
        /// Encuestas abiertas de la campaña destacada.
        ///
        /// El bloque entero desaparece cuando no hay ninguna, igual que el de
        /// la campaña destacada: una sección que anuncia preguntas y no las
        /// tiene es peor que no estar.
        ///
        /// La comprobación del módulo usa <c>Visible</c> y no
        /// <c>Habilitado</c>, para que quien administra siga viendo las
        /// encuestas apagadas —marcadas como ocultas— y pueda revisarlas antes
        /// de publicarlas.
        /// </summary>
        private void CargarEncuestas()
        {
            if (!Modulos.Visible(Modulos.Encuestas))
            {
                phEncuestas.Visible = false;
                return;
            }

            string slug = _actual != null ? _actual.Slug : null;

            IList<Encuesta> encuestas =
                Contenido.Datos.ObtenerEncuestasVigentes(slug, Sesion.CodigoUsuario);

            _totalEncuestas = encuestas.Count;

            phEncuestas.Visible = _totalEncuestas > 0;
            if (_totalEncuestas == 0) return;

            // Se enlaza en cada carga, también en los postbacks: sin modelo, el
            // clic sobre una opción llega sin saber a qué encuesta pertenece.
            rptEncuestas.DataSource = encuestas;
            rptEncuestas.DataBind();

            phVerMas.Visible = _totalEncuestas > EncuestasALaVista;

            // El despliegue lo recuerda el input oculto, no el servidor: así
            // sobrevive al postback de responder una encuesta sin gastar una
            // consulta en recordarlo.
            zonaEncuestas.Attributes["class"] =
                EncuestasDesplegadas ? "gc-encs is-abierta" : "gc-encs";
        }

        /// <summary>
        /// Columna de cada tarjeta. A partir de la tercera lleva la clase que
        /// la esconde hasta que alguien pulse «Ver más».
        /// </summary>
        protected string ClaseColumnaEncuesta(int indice)
        {
            string clase = "col-12 gc-mb";

            if (indice >= EncuestasALaVista) clase += " gc-encs__extra";

            return clase;
        }

        protected bool EncuestasDesplegadas
        {
            get { return hdnEncuestas.Value == "1"; }
        }

        protected string TextoVerMas
        {
            get
            {
                int ocultas = _totalEncuestas - EncuestasALaVista;
                if (ocultas < 1) return "Ver más";

                return "Ver " + Vista.Plural(ocultas, "encuesta más", "encuestas más");
            }
        }

        protected string TextoBotonEncuestas
        {
            get { return EncuestasDesplegadas ? "Ver menos" : TextoVerMas; }
        }

        // =============================================== Iniciativas

        /// <summary>Cuántas iniciativas revela cada «Ver más». Decisión de la página.</summary>
        private const int IniciativasPorBloque = 4;

        private IList<Iniciativa> _iniciativas;

        /// <summary>
        /// Cuántos bloques de cuatro están desplegados. Se lee del input oculto
        /// directamente de <c>Request.Form</c> y no de <c>hdnIniciativas.Value</c>
        /// porque el enlace ocurre en OnInit, antes de que ASP.NET cargue los
        /// valores del postback en los controles. Por defecto uno: los primeros
        /// cuatro a la vista.
        /// </summary>
        private int _bloquesAbiertos = 1;

        private void CargarIniciativas()
        {
            // El administrador ve el módulo aunque esté oculto, marcado. Para el
            // resto desaparece. Es la misma regla de las encuestas.
            if (!Modulos.Visible(Modulos.Iniciativas))
            {
                phIniciativas.Visible = false;
                return;
            }

            _iniciativas = Contenido.Datos.ObtenerIniciativas(Sesion.CodigoUsuario);

            // Una sección que anuncia iniciativas y no tiene ninguna es peor que
            // no estar, igual que el bloque de encuestas.
            phIniciativas.Visible = _iniciativas.Count > 0;
            if (_iniciativas.Count == 0) return;

            int valor;
            string enviado = Request.Form[hdnIniciativas.UniqueID];
            if (!string.IsNullOrEmpty(enviado) && int.TryParse(enviado, out valor) && valor >= 1)
                _bloquesAbiertos = valor;

            hdnIniciativas.Value = _bloquesAbiertos.ToString();

            rptIniciativas.DataSource = _iniciativas;
            rptIniciativas.DataBind();

            phVerMasInic.Visible = _iniciativas.Count > IniciativasPorBloque;

            // Franja de administrador cuando el módulo está apagado.
            phIniciativasOcultas.Visible = !Modulos.Habilitado(Modulos.Iniciativas);

            // El ciudadano propone desde su cuenta. Quien no tiene sesión va al
            // acceso que lo devuelve a su cuenta. El candidato y el administrador
            // no proponen, así que no ven el enlace.
            phProponer.Visible = Sesion.EsCiudadano || !Sesion.Autenticado;
        }

        protected int BloqueDe(int indice)
        {
            return indice / IniciativasPorBloque;
        }

        /// <summary>
        /// Columna de cada tarjeta. Dos por fila. El servidor siempre deja a la
        /// vista el primer bloque y oculta el resto, sin importar cuántos estén
        /// desplegados: el despliegue lo reaplica el script al cargar, leyendo
        /// el input oculto.
        ///
        /// La clase no puede depender de <c>_bloquesAbiertos</c> porque es un
        /// enlace de datos <c>&lt;%# %&gt;</c> dentro del repetidor, y al enlazar
        /// en OnInit el <c>LoadViewState</c> posterior lo pisaría con el valor
        /// del render anterior. Dejarla constante evita ese conflicto, y el
        /// contador y el botón —que son expresiones de render— sí reflejan el
        /// estado real. Es el mismo reparto del «Ver más» de las encuestas: el
        /// estado vive en un input oculto y el cliente lo aplica.
        /// </summary>
        protected string ClaseColumnaIniciativa(int indice)
        {
            string clase = "col-lg-6 gc-mb gc-inic-col";

            if (BloqueDe(indice) >= 1) clase += " is-oculta";

            return clase;
        }

        protected int TotalIniciativas
        {
            get { return _iniciativas == null ? 0 : _iniciativas.Count; }
        }

        protected int PorBloqueInic
        {
            get { return IniciativasPorBloque; }
        }

        private int IniciativasVisibles
        {
            get { return Math.Min(_bloquesAbiertos * IniciativasPorBloque, TotalIniciativas); }
        }

        protected string TextoContadorInic
        {
            get { return IniciativasVisibles + " de " + TotalIniciativas + " iniciativas"; }
        }

        protected string TextoBotonInic
        {
            get
            {
                if (IniciativasVisibles >= TotalIniciativas) return "Ver menos";

                int faltan = Math.Min(IniciativasPorBloque, TotalIniciativas - IniciativasVisibles);
                return "Ver " + faltan + " más";
            }
        }

        protected string UrlProponer
        {
            get
            {
                if (Sesion.EsCiudadano) return ResolveUrl("~/MiCuenta");

                return ResolveUrl("~/Acceso?volver=" + Server.UrlEncode("/MiCuenta"));
            }
        }

        private void CargarCandidatos()
        {
            string slug = _actual != null ? _actual.Slug : null;

            IList<Candidato> candidatos = Contenido.Datos.ObtenerCandidatos(slug);

            // En portada se muestra una selección, no el listado completo.
            List<Candidato> muestra = new List<Candidato>();
            for (int i = 0; i < candidatos.Count && i < 6; i++)
            {
                muestra.Add(candidatos[i]);
            }

            rptCandidatos.DataSource = muestra;
            rptCandidatos.DataBind();
        }

        private void CargarCampanas()
        {
            IList<Campana> todas = Contenido.Datos.ObtenerCampanas();

            List<Campana> otras = new List<Campana>();
            foreach (Campana c in todas)
            {
                if (!c.EsActual) otras.Add(c);
            }

            rptCampanas.DataSource = otras;
            rptCampanas.DataBind();
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            string termino = txtBuscar.Text.Trim();

            if (string.IsNullOrEmpty(termino))
            {
                Response.Redirect("~/Candidatos");
                return;
            }

            Response.Redirect("~/Candidatos?q=" + HttpUtility.UrlEncode(termino));
        }

        // ------------------------------------------------- Cifras generales

        protected string TotalCampanas
        {
            get { return Vista.Numero(Contenido.Datos.ObtenerCampanas().Count); }
        }

        protected string TotalCandidatos
        {
            get { return Vista.Numero(Contenido.Datos.ObtenerCandidatos(null).Count); }
        }

        protected string TotalPropuestas
        {
            get { return Vista.Numero(Contenido.Datos.ObtenerPropuestasDeCampana(null).Count); }
        }

        // -------------------------------------------------- Campaña actual

        protected string NombreActual
        {
            get { return _actual != null ? _actual.Nombre : string.Empty; }
        }

        protected string ResumenActual
        {
            get { return _actual != null ? _actual.Resumen : string.Empty; }
        }

        protected string CandidatosActual
        {
            get { return _actual != null ? Vista.Numero(_actual.TotalCandidatos) : "0"; }
        }

        protected string PropuestasActual
        {
            get { return _actual != null ? Vista.Numero(_actual.TotalPropuestas) : "0"; }
        }

        protected string PublicacionesActual
        {
            get { return _actual != null ? Vista.Numero(_actual.TotalPublicaciones) : "0"; }
        }

        protected string FechaEleccion
        {
            get { return _actual != null ? Vista.FechaCorta(_actual.FechaEleccion) : string.Empty; }
        }

        protected string CuentaRegresiva
        {
            get { return _actual != null ? Vista.CuentaRegresiva(_actual.FechaEleccion) : string.Empty; }
        }

        protected string UrlActual
        {
            get { return _actual != null ? ResolveUrl(_actual.Url) : ResolveUrl("~/Campanas"); }
        }
    }
}
