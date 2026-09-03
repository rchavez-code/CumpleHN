using System;
using System.Collections.Generic;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Panel
{
    /// <summary>
    /// Publicaciones del candidato. En esta etapa es solo lectura: la redacción
    /// y edición llegan junto con el módulo de interacción ciudadana.
    /// </summary>
    public partial class Publicaciones : PaginaPanel
    {
        private int _total;

        protected void Page_Load(object sender, EventArgs e)
        {
            IList<Publicacion> publicaciones = Contenido.Datos.ObtenerPublicaciones(CandidatoActual.Slug);
            _total = publicaciones.Count;

            phLista.Visible = _total > 0;
            phVacio.Visible = _total == 0;

            if (_total > 0)
            {
                rptPublicaciones.DataSource = publicaciones;
                rptPublicaciones.DataBind();
            }
        }

        protected string TotalTexto
        {
            get { return Vista.Plural(_total, "publicación", "publicaciones"); }
        }
    }
}
