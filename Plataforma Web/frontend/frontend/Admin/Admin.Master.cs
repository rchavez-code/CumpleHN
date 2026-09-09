using System;
using System.IO;
using System.Web.UI;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Plantilla del área de administración de la plataforma.
    ///
    /// Comparte el layout del panel del candidato (barra lateral, barra
    /// superior y área de contenido) porque resuelven el mismo problema, pero
    /// es una plantilla aparte: la navegación, la identidad que muestra y el
    /// alcance de las dos áreas no tienen nada en común.
    ///
    /// La plantilla no protege nada. El control de acceso vive en
    /// <see cref="PaginaAdmin"/>, de la que heredan todas las páginas de esta
    /// carpeta.
    /// </summary>
    public partial class AdminMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected string NombreUsuario
        {
            get { return Sesion.Nombre; }
        }

        protected string Iniciales
        {
            get { return Sesion.Iniciales; }
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
    }
}
