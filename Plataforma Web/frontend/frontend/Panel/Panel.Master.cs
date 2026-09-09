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
    /// La plantilla no protege nada: eso lo resuelve <see cref="PaginaPanel"/>,
    /// de la que heredan todas las páginas de esta carpeta. Una plantilla no
    /// puede ser el control de acceso porque se aplica después de que la página
    /// ya empezó su ciclo de vida.
    /// </summary>
    public partial class PanelMaster : MasterPage
    {
        private Candidato _candidato;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Candidatura dueña del panel. La resuelve la página, que ya la tuvo
        /// que obtener para autorizar el acceso: pedirla de nuevo sería una
        /// segunda llamada al Web Service por cada carga.
        /// </summary>
        protected Candidato Actual
        {
            get
            {
                if (_candidato == null)
                {
                    PaginaPanel pagina = Page as PaginaPanel;
                    if (pagina != null)
                        _candidato = pagina.CandidatoActual;
                }

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
