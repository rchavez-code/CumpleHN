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
            string clase = "col-lg-6 gc-mb";

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
