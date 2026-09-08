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
            CargarEncuesta();
        }

        /// <summary>
        /// Encuesta abierta de la campaña destacada.
        ///
        /// El bloque entero desaparece cuando no hay ninguna, igual que el de
        /// la campaña destacada: una sección que anuncia una pregunta y no la
        /// tiene es peor que no estar.
        ///
        /// La comprobación del módulo usa <c>Visible</c> y no
        /// <c>Habilitado</c>, para que quien administra siga viendo la encuesta
        /// apagada —marcada como oculta— y pueda revisarla antes de publicarla.
        /// </summary>
        private void CargarEncuesta()
        {
            if (!Modulos.Visible(Modulos.Encuestas))
            {
                phEncuesta.Visible = false;
                return;
            }

            string slug = _actual != null ? _actual.Slug : null;

            Encuesta encuesta = Contenido.Datos.ObtenerEncuestaVigente(slug, Sesion.CodigoUsuario);

            phEncuesta.Visible = encuesta != null;

            // Se asigna en cada carga por la misma razón que los repetidores:
            // sin modelo, el clic sobre una opción llega sin saber a qué
            // encuesta pertenece.
            if (encuesta != null) tarjetaEncuesta.Item = encuesta;
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
