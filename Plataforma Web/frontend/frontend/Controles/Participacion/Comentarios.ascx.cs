using System;
using System.Collections.Generic;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Controles
{
    /// <summary>
    /// Hilo de comentarios sobre cualquier objeto de la plataforma.
    ///
    /// Leer los comentarios es público. Publicar requiere cuenta, y cuando no
    /// hay sesión el formulario se sustituye por una invitación a acceder que
    /// regresa a esta misma página.
    /// </summary>
    public partial class Comentarios : UserControl
    {
        /// <summary>
        /// Tipo de objeto comentado. Ver <see cref="TiposObjeto"/>. Se guarda en
        /// ViewState para que el control siga sabiendo sobre qué se comenta
        /// aunque el contenedor no se vuelva a enlazar en el postback.
        /// </summary>
        public string TipoObjeto
        {
            get { return Convert.ToString(ViewState["tipo"]); }
            set { ViewState["tipo"] = value; }
        }

        /// <summary>Código del objeto comentado.</summary>
        public int CodigoObjeto
        {
            get { return ViewState["codigo"] == null ? 0 : (int)ViewState["codigo"]; }
            set { ViewState["codigo"] = value; }
        }

        /// <summary>
        /// Se dispara al publicar un comentario, para que la página que contiene
        /// el control pueda refrescar su propio contador.
        /// </summary>
        public event EventHandler ComentarioPublicado;

        protected void Page_Load(object sender, EventArgs e)
        {
            // El hilo se lee siempre. Lo que la administración puede cerrar es
            // la posibilidad de agregar comentarios nuevos, no la de leer los
            // que ya existen.
            bool abierta = Modulos.Visible(Modulos.Interaccion);
            bool autenticado = Sesion.Autenticado;

            phFormulario.Visible = abierta && autenticado;
            phInvitacion.Visible = abierta && !autenticado;
            phCerrado.Visible = !abierta;

            if (!IsPostBack) Cargar();
        }

        /// <summary>
        /// Vuelve a leer el hilo. Se llama también después de publicar para que
        /// el comentario nuevo aparezca sin recargar la página completa.
        /// </summary>
        public void Cargar()
        {
            if (string.IsNullOrEmpty(TipoObjeto) || CodigoObjeto <= 0)
            {
                rptComentarios.Visible = false;
                phVacio.Visible = false;
                return;
            }

            IList<Comentario> lista = Contenido.Datos.ObtenerComentarios(TipoObjeto, CodigoObjeto);

            bool hay = lista.Count > 0;
            rptComentarios.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptComentarios.DataSource = lista;
                rptComentarios.DataBind();
            }
        }

        protected void btnPublicar_Click(object sender, EventArgs e)
        {
            if (!Sesion.Autenticado)
            {
                Response.Redirect(Sesion.UrlAccesoDeVuelta());
                return;
            }

            Resultado r = Contenido.Datos.AgregarComentario(
                TipoObjeto, CodigoObjeto, Sesion.CodigoUsuario, txtComentario.Text);

            if (!r.Ok)
            {
                litMensaje.Text = Server.HtmlEncode(r.Mensaje);
                phMensaje.Visible = true;
                return;
            }

            phMensaje.Visible = false;
            txtComentario.Text = string.Empty;

            Cargar();

            if (ComentarioPublicado != null) ComentarioPublicado(this, EventArgs.Empty);
        }

        // ---------------------------------------------------- Presentación

        protected string Iniciales
        {
            get { return Sesion.Iniciales; }
        }

        protected string UrlAcceso
        {
            get { return ResolveUrl(Sesion.UrlAccesoDeVuelta()); }
        }

        /// <summary>
        /// Marca visualmente cuando quien comenta es una candidatura, porque no
        /// es lo mismo que opine un tercero que quien es dueño del contenido.
        /// </summary>
        protected string EtiquetaRol(object dato)
        {
            Comentario c = dato as Comentario;
            if (c == null || !c.EsCandidato) return string.Empty;

            return "<span class=\"gc-chip\" style=\"font-size:.68rem;padding:1px 7px;\">Candidatura</span>";
        }
    }
}
