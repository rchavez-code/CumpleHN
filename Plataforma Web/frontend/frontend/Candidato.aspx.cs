using System;
using System.Collections.Generic;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Perfil público de un candidato. Es la vista que consulta cualquier
    /// visitante, sin necesidad de cuenta, y la contraparte de lo que el propio
    /// candidato administra desde su panel privado.
    /// </summary>
    public partial class CandidatoPagina : PaginaDeModulo
    {
        /// <summary>Módulo al que pertenece esta página.</summary>
        protected override string ModuloRequerido
        {
            get { return Modulos.Perfiles; }
        }

        private const string TabPropuestas = "propuestas";

        private Candidato _candidato;
        private Campana _campana;
        private string _tab;

        /// <summary>Candidato en pantalla. Nunca es nulo: si no existe, se redirige.</summary>
        protected Candidato Item
        {
            get { return _candidato; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _candidato = Contenido.Datos.ObtenerCandidato(Request.QueryString["id"]);

            if (_candidato == null)
            {
                Response.Redirect("~/Candidatos");
                return;
            }

            _campana = Contenido.Datos.ObtenerCampana(_candidato.CampanaSlug);
            _tab = NormalizarTab(Request.QueryString["t"]);

            Page.Title = _candidato.NombreCompleto;

            MostrarPestana();
            MostrarContacto();
        }

        private static string NormalizarTab(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return TabPropuestas;

            switch (valor.ToLowerInvariant())
            {
                case "publicaciones": return "publicaciones";
                case "opiniones": return "opiniones";
                case "info": return "info";
                default: return TabPropuestas;
            }
        }

        private void MostrarPestana()
        {
            phPropuestas.Visible = _tab == TabPropuestas;
            phPublicaciones.Visible = _tab == "publicaciones";
            phOpiniones.Visible = _tab == "opiniones";
            phInfo.Visible = _tab == "info";

            // La barra de valoración del perfil está siempre visible, en
            // cualquier pestaña.
            interPerfil.TipoObjeto = TiposObjeto.Candidato;
            interPerfil.CodigoObjeto = _candidato.Id;
            interPerfil.MeGusta = _candidato.MeGusta;
            interPerfil.NoMeGusta = _candidato.NoMeGusta;
            interPerfil.Comentarios = _candidato.Comentarios;
            interPerfil.UrlComentarios = _tab == "opiniones" ? null : _candidato.Url + "&t=opiniones";

            if (phOpiniones.Visible)
            {
                comentsPerfil.TipoObjeto = TiposObjeto.Candidato;
                comentsPerfil.CodigoObjeto = _candidato.Id;

                interPerfil.ComentarClic += (s, ev) => comentsPerfil.Cargar();

                // Al publicar un comentario, el contador de la barra se pone al día.
                comentsPerfil.ComentarioPublicado += (s, ev) => interPerfil.Refrescar();
            }

            if (phPropuestas.Visible)
            {
                IList<Propuesta> propuestas = Contenido.Datos.ObtenerPropuestas(_candidato.Slug);
                rptPropuestas.DataSource = propuestas;
                rptPropuestas.DataBind();
                phPropuestasVacio.Visible = propuestas.Count == 0;
            }

            if (phPublicaciones.Visible)
            {
                IList<Publicacion> publicaciones = Contenido.Datos.ObtenerPublicaciones(_candidato.Slug);
                rptPublicaciones.DataSource = publicaciones;
                rptPublicaciones.DataBind();
                phPublicacionesVacio.Visible = publicaciones.Count == 0;
            }
        }

        /// <summary>
        /// Solo se dibujan los datos de contacto que la candidatura llenó, para
        /// no mostrar filas vacías en el perfil público.
        /// </summary>
        private void MostrarContacto()
        {
            phCorreo.Visible = !string.IsNullOrEmpty(_candidato.CorreoPublico);
            phTelefono.Visible = !string.IsNullOrEmpty(_candidato.Telefono);
            phSitio.Visible = !string.IsNullOrEmpty(_candidato.SitioWeb);

            List<string> redes = new List<string>();
            if (!string.IsNullOrEmpty(_candidato.Facebook)) redes.Add("Facebook: " + _candidato.Facebook);
            if (!string.IsNullOrEmpty(_candidato.X)) redes.Add("X: @" + _candidato.X);
            if (!string.IsNullOrEmpty(_candidato.Instagram)) redes.Add("Instagram: @" + _candidato.Instagram);

            phRedes.Visible = redes.Count > 0;

            if (phRedes.Visible)
            {
                rptRedes.DataSource = redes;
                rptRedes.DataBind();
            }

            phContacto.Visible = phCorreo.Visible || phTelefono.Visible || phSitio.Visible || phRedes.Visible;
        }

        // --------------------------------------------------------- Pestañas

        protected string UrlTab(string tab)
        {
            return ResolveUrl("~/Candidato?id=" + _candidato.Slug + "&t=" + tab);
        }

        protected string ClaseTab(string tab)
        {
            return _tab == tab ? "is-active" : string.Empty;
        }

        // ----------------------------------------------------- Presentación

        protected string Iniciales
        {
            get { return _candidato.TieneFoto ? string.Empty : _candidato.Iniciales; }
        }

        protected string EstiloAvatar
        {
            get
            {
                return _candidato.TieneFoto
                    ? Vista.EstiloAvatar(ResolveUrl(_candidato.FotoUrl))
                    : string.Empty;
            }
        }

        protected string NivelTexto
        {
            get { return Vista.TextoNivel(_candidato.Nivel); }
        }

        protected string NombreCampana
        {
            get { return _campana != null ? _campana.Nombre : "Sin campaña asociada"; }
        }

        protected string UrlCampana
        {
            get
            {
                return _campana != null
                    ? ResolveUrl(_campana.Url)
                    : ResolveUrl("~/Campanas");
            }
        }
    }
}
