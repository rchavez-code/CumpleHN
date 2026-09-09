using System;
using System.Collections.Generic;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Listado de campañas electorales agrupadas por estado.
    /// </summary>
    public partial class Campanas : PaginaDeModulo
    {
        /// <summary>Módulo al que pertenece esta página.</summary>
        protected override string ModuloRequerido
        {
            get { return Modulos.Campanas; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            IList<Campana> todas = Contenido.Datos.ObtenerCampanas();

            List<Campana> activas = new List<Campana>();
            List<Campana> proximas = new List<Campana>();
            List<Campana> cerradas = new List<Campana>();

            foreach (Campana c in todas)
            {
                switch (c.Estado)
                {
                    case EstadoCampana.Activa: activas.Add(c); break;
                    case EstadoCampana.Proxima: proximas.Add(c); break;
                    default: cerradas.Add(c); break;
                }
            }

            Enlazar(rptActivas, activas);
            Enlazar(rptProximas, proximas);
            Enlazar(rptCerradas, cerradas);

            phVacio.Visible = todas.Count == 0;
        }

        /// <summary>
        /// Un Repeater sin elementos no dibuja su encabezado, así que basta con
        /// no asignarle datos para que la sección completa desaparezca.
        /// </summary>
        private static void Enlazar(System.Web.UI.WebControls.Repeater rpt, List<Campana> datos)
        {
            if (datos.Count == 0)
            {
                rpt.Visible = false;
                return;
            }

            rpt.DataSource = datos;
            rpt.DataBind();
        }
    }
}
