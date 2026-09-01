using System;
using System.IO;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Panel
{
    /// <summary>
    /// Plantilla del panel privado del candidato. Es una plantilla aparte de la
    /// pública porque el panel tiene su propia estructura: barra lateral de
    /// navegación, barra superior con acciones y área de contenido.
    ///
    /// Pendiente: la protección de esta carpeta se resuelve cuando exista
    /// autenticación. Mientras tanto el candidato lo provee el servicio de
    /// contenido de demostración.
    /// </summary>
    public partial class PanelMaster : MasterPage
    {
        private Candidato _candidato;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Candidato dueño del panel. Sustituye a la sesión mientras la
        /// autenticación no está implementada.
        /// </summary>
        protected Candidato Actual
        {
            get
            {
                if (_candidato == null)
                    _candidato = Contenido.Datos.ObtenerCandidatoAutenticado();

                return _candidato;
            }
        }

        private string PaginaActual
        {
            get
            {
                string ruta = Page.AppRelativeVirtualPath;
                if (string.IsNullOrEmpty(ruta)) return string.Empty;
                return Path.GetFileNameWithoutExtension(ruta);
            }
        }

        protected string ClaseNav(params string[] paginas)
        {
            string actual = PaginaActual;

            for (int i = 0; i < paginas.Length; i++)
            {
                if (string.Equals(actual, paginas[i], StringComparison.OrdinalIgnoreCase))
                    return "is-active";
            }

            return string.Empty;
        }

        // ---------------------------------------------------- Presentación

        protected string NombreCandidato
        {
            get { return Actual != null ? Actual.NombreCompleto : "Candidatura sin nombre"; }
        }

        protected string CargoCandidato
        {
            get { return Actual != null ? Actual.Cargo : string.Empty; }
        }

        protected string Iniciales
        {
            get
            {
                if (Actual == null) return string.Empty;
                return Actual.TieneFoto ? string.Empty : Actual.Iniciales;
            }
        }

        protected string EstiloAvatar
        {
            get
            {
                if (Actual == null || !Actual.TieneFoto) return string.Empty;
                return Vista.EstiloAvatar(ResolveUrl(Actual.FotoUrl));
            }
        }

        protected string UrlPerfilPublico
        {
            get
            {
                return Actual != null
                    ? ResolveUrl(Actual.Url)
                    : ResolveUrl("~/Candidatos");
            }
        }
    }
}
