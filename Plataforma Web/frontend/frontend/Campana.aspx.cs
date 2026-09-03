using System;
using System.Collections.Generic;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Página de una campaña electoral. Concentra la experiencia principal del
    /// sitio: el feed de publicaciones, las candidaturas, las propuestas y la
    /// información del proceso.
    ///
    /// Las pestañas se resuelven en el servidor con el parámetro <c>t</c>, de
    /// modo que cada vista tiene su propia dirección compartible y funciona sin
    /// JavaScript.
    /// </summary>
    public partial class CampanaPagina : PaginaDeModulo
    {
        /// <summary>Módulo al que pertenece esta página.</summary>
        protected override string ModuloRequerido
        {
            get { return Modulos.Campanas; }
        }

        private const string TabFeed = "feed";

        private Campana _campana;
        private string _tab;

        /// <summary>Campaña en pantalla. Nunca es nula: si no se encuentra, se redirige.</summary>
        protected Campana Item
        {
            get { return _campana; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _campana = Contenido.Datos.ObtenerCampana(Request.QueryString["c"]);

            if (_campana == null)
            {
                Response.Redirect("~/Campanas");
                return;
            }

            _tab = NormalizarTab(Request.QueryString["t"]);

            Page.Title = _campana.Nombre;

            MostrarPestana();
        }

        private static string NormalizarTab(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return TabFeed;

            switch (valor.ToLowerInvariant())
            {
                case "candidatos": return "candidatos";
                case "propuestas": return "propuestas";
                case "info": return "info";
                default: return TabFeed;
            }
        }

        private void MostrarPestana()
        {
            phFeed.Visible = _tab == TabFeed;
            phCandidatos.Visible = _tab == "candidatos";
            phPropuestas.Visible = _tab == "propuestas";
            phInfo.Visible = _tab == "info";

            if (phFeed.Visible) CargarFeed();
            if (phCandidatos.Visible) CargarCandidatos();
            if (phPropuestas.Visible) CargarPropuestas();
        }

        private void CargarFeed()
        {
            IList<Publicacion> feed = Contenido.Datos.ObtenerFeed(_campana.Slug);

            rptFeed.DataSource = feed;
            rptFeed.DataBind();
            phFeedVacio.Visible = feed.Count == 0;

            rptCategorias.DataSource = Contenido.Datos.ObtenerCategorias();
            rptCategorias.DataBind();

            // El riel muestra una selección corta, no el listado completo.
            IList<Candidato> candidatos = Contenido.Datos.ObtenerCandidatos(_campana.Slug);
            List<Candidato> muestra = new List<Candidato>();
            for (int i = 0; i < candidatos.Count && i < 5; i++)
            {
                muestra.Add(candidatos[i]);
            }

            rptRail.DataSource = muestra;
            rptRail.DataBind();
        }

        private void CargarCandidatos()
        {
            IList<Candidato> candidatos = Contenido.Datos.ObtenerCandidatos(_campana.Slug);

            rptCandidatos.DataSource = candidatos;
            rptCandidatos.DataBind();
            phCandidatosVacio.Visible = candidatos.Count == 0;
        }

        private void CargarPropuestas()
        {
            IList<Propuesta> propuestas = Contenido.Datos.ObtenerPropuestasDeCampana(_campana.Slug);

            rptPropuestas.DataSource = propuestas;
            rptPropuestas.DataBind();
            phPropuestasVacio.Visible = propuestas.Count == 0;
        }

        // --------------------------------------------------------- Pestañas

        protected string UrlTab(string tab)
        {
            return ResolveUrl("~/Campana?c=" + _campana.Slug + "&t=" + tab);
        }

        protected string ClaseTab(string tab)
        {
            return _tab == tab ? "is-active" : string.Empty;
        }

        // ---------------------------------------------------------- Fechas

        protected string FechaEleccion
        {
            get { return Vista.FechaCorta(_campana.FechaEleccion); }
        }

        protected string FechaEleccionLarga
        {
            get { return Vista.Fecha(_campana.FechaEleccion); }
        }

        protected string FechaInicioLarga
        {
            get { return Vista.Fecha(_campana.FechaInicio); }
        }

        protected string CuentaRegresiva
        {
            get { return Vista.CuentaRegresiva(_campana.FechaEleccion); }
        }
    }
}
