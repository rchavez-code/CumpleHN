using System;
using System.IO;
using System.Web.UI;

namespace frontend
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // El encabezado cambia según haya sesión o no. Es lo que le indica a
            // la persona que ya puede participar.
            bool autenticado = Servicios.Sesion.Autenticado;

            phAnonimo.Visible = !autenticado;
            phSesion.Visible = autenticado;
            phPanel.Visible = autenticado && Servicios.Sesion.EsCandidato;
            phAdmin.Visible = autenticado && Servicios.Sesion.EsAdministrador;
        }

        protected string NombreUsuario
        {
            get { return Servicios.Sesion.Nombre; }
        }

        protected string InicialesUsuario
        {
            get { return Servicios.Sesion.Iniciales; }
        }

        /// <summary>
        /// Nombre del archivo de la página actual, sin extensión.
        /// </summary>
        private string PaginaActual
        {
            get
            {
                string ruta = Page.AppRelativeVirtualPath;
                if (string.IsNullOrEmpty(ruta)) return string.Empty;
                return Path.GetFileNameWithoutExtension(ruta);
            }
        }

        /// <summary>
        /// Marca el enlace de navegación activo. Se pasan todas las páginas que
        /// pertenecen a la misma sección, de modo que la ficha de un candidato
        /// mantenga iluminado el enlace de Candidatos.
        /// </summary>
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
