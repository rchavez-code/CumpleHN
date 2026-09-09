using System;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Ficha completa de una propuesta o proyecto de campaña, incluido el
    /// esquema de estados por el que puede transitar su cumplimiento.
    /// </summary>
    public partial class PropuestaPagina : PaginaDeModulo
    {
        /// <summary>Módulo al que pertenece esta página.</summary>
        protected override string ModuloRequerido
        {
            get { return Modulos.Propuestas; }
        }

        private Propuesta _propuesta;
        private Candidato _autor;

        /// <summary>Propuesta en pantalla. Nunca es nula: si no existe, se redirige.</summary>
        protected Propuesta Item
        {
            get { return _propuesta; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(Request.QueryString["id"], out id))
            {
                Response.Redirect("~/Propuestas");
                return;
            }

            _propuesta = Contenido.Datos.ObtenerPropuesta(id);

            if (_propuesta == null)
            {
                Response.Redirect("~/Propuestas");
                return;
            }

            _autor = Contenido.Datos.ObtenerCandidato(_propuesta.CandidatoSlug);

            Page.Title = _propuesta.Nombre;

            phAdicional.Visible = !string.IsNullOrEmpty(_propuesta.InformacionAdicional);
            phAutor.Visible = _autor != null;

            interPropuesta.TipoObjeto = TiposObjeto.Propuesta;
            interPropuesta.CodigoObjeto = _propuesta.Id;
            interPropuesta.MeGusta = _propuesta.MeGusta;
            interPropuesta.NoMeGusta = _propuesta.NoMeGusta;
            interPropuesta.Comentarios = _propuesta.Comentarios;

            comentsPropuesta.TipoObjeto = TiposObjeto.Propuesta;
            comentsPropuesta.CodigoObjeto = _propuesta.Id;

            interPropuesta.ComentarClic += (s, ev) => comentsPropuesta.Cargar();

            // Al publicar un comentario, el contador de la barra se pone al día.
            comentsPropuesta.ComentarioPublicado += (s, ev) => interPropuesta.Refrescar();
        }

        // ---------------------------------------------------------- Autoría

        protected string NombreAutor
        {
            get { return _autor != null ? _autor.NombreCompleto : string.Empty; }
        }

        protected string CargoAutor
        {
            get { return _autor != null ? _autor.Cargo : string.Empty; }
        }

        protected string InicialesAutor
        {
            get { return _autor != null ? _autor.Iniciales : string.Empty; }
        }

        protected string UrlCandidato
        {
            get
            {
                return _autor != null
                    ? ResolveUrl(_autor.Url)
                    : ResolveUrl("~/Candidatos");
            }
        }

        protected string FechaRegistro
        {
            get { return Vista.FechaCorta(_propuesta.FechaRegistro); }
        }
    }
}
