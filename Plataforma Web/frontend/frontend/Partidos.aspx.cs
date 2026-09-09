using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Listado de partidos políticos con candidaturas en la plataforma.
    /// </summary>
    public partial class PartidosPagina : PaginaDeModulo
    {
        /// <summary>Módulo al que pertenece esta página.</summary>
        protected override string ModuloRequerido
        {
            get { return Modulos.Perfiles; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Se enlaza en cada carga porque las tarjetas llevan botones de
            // participación y necesitan su modelo al procesar el clic.
            IList<Partido> lista = Contenido.Datos.ObtenerPartidos();

            rptPartidos.DataSource = lista;
            rptPartidos.DataBind();

            phVacio.Visible = lista.Count == 0;
        }

        /// <summary>
        /// Configura la barra de participación de cada tarjeta. Se hace acá y no
        /// en el marcado porque el control necesita varios valores del modelo.
        /// </summary>
        protected void rptPartidos_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            Partido p = e.Item.DataItem as Partido;

            // Se califica el tipo porque Controles.Interaccion y
            // Modelos.Interaccion comparten el nombre.
            Controles.Interaccion inter = e.Item.FindControl("inter") as Controles.Interaccion;

            if (p == null || inter == null) return;

            inter.TipoObjeto = TiposObjeto.Partido;
            inter.CodigoObjeto = p.Id;
            inter.MeGusta = p.MeGusta;
            inter.NoMeGusta = p.NoMeGusta;
            inter.Comentarios = p.Comentarios;
            inter.UrlComentarios = p.Url;
        }
    }
}
