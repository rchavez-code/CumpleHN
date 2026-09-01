using System;
using System.Collections.Generic;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Ficha pública de un partido político, con sus candidaturas y el espacio
    /// de participación ciudadana.
    /// </summary>
    public partial class PartidoPagina : Page
    {
        private Partido _partido;

        /// <summary>Partido en pantalla. Nunca es nulo: si no existe, se redirige.</summary>
        protected Partido Item
        {
            get { return _partido; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _partido = Contenido.Datos.ObtenerPartido(Request.QueryString["id"]);

            if (_partido == null)
            {
                Response.Redirect("~/Partidos");
                return;
            }

            Page.Title = _partido.Nombre;

            interPartido.TipoObjeto = TiposObjeto.Partido;
            interPartido.CodigoObjeto = _partido.Id;
            interPartido.MeGusta = _partido.MeGusta;
            interPartido.NoMeGusta = _partido.NoMeGusta;
            interPartido.Comentarios = _partido.Comentarios;
            interPartido.ComentarClic += (s, ev) => comentsPartido.Cargar();

            comentsPartido.TipoObjeto = TiposObjeto.Partido;
            comentsPartido.CodigoObjeto = _partido.Id;

            // Al publicar un comentario, el contador de la barra se pone al día.
            comentsPartido.ComentarioPublicado += (s, ev) => interPartido.Refrescar();

            // Se enlaza en cada carga porque las tarjetas de candidato llevan
            // botones de participación.
            IList<Candidato> candidatos = Contenido.Datos.ObtenerCandidatosDePartido(_partido.Slug);

            rptCandidatos.DataSource = candidatos;
            rptCandidatos.DataBind();

            phSinCandidatos.Visible = candidatos.Count == 0;
        }

        protected string SiglasTexto
        {
            get { return string.IsNullOrEmpty(_partido.Siglas) ? "Sin siglas registradas" : _partido.Siglas; }
        }

        protected string TextoCandidatos
        {
            get { return _partido.TotalCandidatos == 1 ? "candidatura" : "candidaturas"; }
        }

        protected string TextoPropuestas
        {
            get { return _partido.TotalPropuestas == 1 ? "propuesta" : "propuestas"; }
        }
    }
}
