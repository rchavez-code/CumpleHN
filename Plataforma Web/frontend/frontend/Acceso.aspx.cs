using System;
using System.Web.UI;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Acceso a la plataforma.
    ///
    /// La consulta pública no requiere cuenta. El acceso existe para participar
    /// (apoyar, comentar, guardar) y para administrar una candidatura.
    ///
    /// Las credenciales no se validan acá: se envían al Web Service del backend,
    /// que es el único que conoce la base de datos. El frontend solo decide a
    /// dónde llevar al usuario según el rol que le devuelvan.
    /// </summary>
    public partial class Acceso : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // El enlace «Salir» del panel llega acá para cerrar la sesión.
            if (!IsPostBack && Request.QueryString["salir"] == "1")
            {
                Session.Clear();
                Session.Abandon();
                Response.Redirect("~/Acceso");
            }
        }

        protected void btnEntrar_Click(object sender, EventArgs e)
        {
            string usuario = txtCorreo.Text.Trim();
            string clave = txtClave.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
            {
                MostrarError("Ingresá tu usuario y tu contraseña.");
                return;
            }

            // Paso 1: conexión con el Web Service del backend.
            var client = new webservices.WebServiceGlobalSoapClient();

            webservices.RespuestaLogin respuesta;
            try
            {
                respuesta = client.ValidarLogin(usuario, clave);
                client.Close();
            }
            catch (Exception ex)
            {
                client.Abort();
                MostrarError("No se pudo contactar al servidor. " + ex.Message);
                return;
            }

            if (!respuesta.ok)
            {
                MostrarError(respuesta.mensaje);
                return;
            }

            // Acceso concedido: se guarda lo mínimo necesario en sesión.
            Session["usuario"] = respuesta.usuario;
            Session["codigoUsuario"] = respuesta.usuario.codigoUsuario;
            Session["nombreUsuario"] = respuesta.usuario.nombre;
            Session["rol"] = respuesta.usuario.rol;
            Session["candidatoSlug"] = respuesta.usuario.candidatoSlug;

            // Si llegó acá porque quiso participar en alguna página, se lo
            // devuelve a esa misma página en lugar de mandarlo al inicio.
            string volver = Request.QueryString["volver"];
            if (EsRutaLocalSegura(volver))
            {
                Response.Redirect(volver);
                return;
            }

            // Cada rol entra a su propia área. La correspondencia vive en
            // Autorizacion, para que el acceso y el control de las páginas no
            // puedan discrepar.
            Response.Redirect(Autorizacion.InicioDe(respuesta.usuario.rol));
        }

        /// <summary>
        /// Acepta únicamente rutas dentro del propio sitio.
        ///
        /// Sin esta comprobación, un enlace con
        /// <c>?volver=https://sitio-ajeno</c> convertiría la página de acceso en
        /// un redirector hacia cualquier destino, que es la vulnerabilidad de
        /// redirección abierta que revisa OWASP.
        /// </summary>
        private static bool EsRutaLocalSegura(string ruta)
        {
            if (string.IsNullOrEmpty(ruta)) return false;
            if (!ruta.StartsWith("/")) return false;

            // Descarta "//servidor" y "/\servidor", que el navegador interpreta
            // como direcciones absolutas hacia otro dominio.
            if (ruta.StartsWith("//") || ruta.StartsWith("/\\")) return false;

            return true;
        }

        private void MostrarError(string mensaje)
        {
            litError.Text = Server.HtmlEncode(mensaje);
            phError.Visible = true;
        }
    }
}
