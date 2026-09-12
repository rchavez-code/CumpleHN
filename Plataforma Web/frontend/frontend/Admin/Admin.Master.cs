using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
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
    ///
    /// Sí muestra, y deja cambiar, el espacio que se está administrando. La
    /// cuenta de la plataforma administra todos y elige uno del desplegable.
    /// La de un cliente queda fija en el suyo. Lo que se elige acá solo decide
    /// qué se muestra: cada acción la vuelve a comprobar el Web Service contra
    /// la base.
    /// </summary>
    public partial class AdminMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AdministraPlataforma) return;

            ddlEspacio.Visible = true;
            lblEspacio.Visible = false;

            // La lista sobrevive al postback por ViewState. Se pide una sola
            // vez por página, no en cada clic. La página de espacios la vuelve
            // a pedir después de registrar uno, con RecargarEspacios.
            if (!Page.IsPostBack) RecargarEspacios();
        }

        /// <summary>
        /// Vuelve a llenar el desplegable de espacios y deja seleccionado el
        /// de la sesión. Lo llama la página de espacios cuando la lista cambió
        /// en el mismo postback, para que el registro recién hecho aparezca
        /// sin recargar.
        /// </summary>
        public void RecargarEspacios()
        {
            if (!AdministraPlataforma) return;

            IList<Espacio> espacios = Contenido.Datos.ObtenerEspacios(Sesion.CodigoUsuario, true);

            ddlEspacio.ClearSelection();
            ddlEspacio.Items.Clear();
            foreach (Espacio x in espacios)
            {
                ddlEspacio.Items.Add(new ListItem(x.Nombre, x.Codigo.ToString()));
            }

            ListItem actual = ddlEspacio.Items.FindByValue(Sesion.CodigoEspacio.ToString());
            if (actual != null) actual.Selected = true;
        }

        /// <summary>
        /// Cambia el espacio de la sesión y vuelve a cargar la misma página
        /// limpia. La página ya corrió su Page_Load con el espacio anterior,
        /// así que seguir sin recargar mostraría la lista vieja bajo el rótulo
        /// nuevo, que es justo la confusión que el rótulo existe para evitar.
        /// </summary>
        protected void ddlEspacio_SelectedIndexChanged(object sender, EventArgs e)
        {
            int codigo;
            if (!int.TryParse(ddlEspacio.SelectedValue, out codigo)) return;

            Sesion.CambiarEspacio(codigo, ddlEspacio.SelectedItem.Text);
            Response.Redirect(Request.RawUrl);
        }

        protected string NombreUsuario
        {
            get { return Sesion.Nombre; }
        }

        protected string Iniciales
        {
            get { return Sesion.Iniciales; }
        }

        protected bool AdministraPlataforma
        {
            get { return Sesion.AdministraPlataforma; }
        }

        protected string EspacioNombre
        {
            get { return Sesion.EspacioNombre; }
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
