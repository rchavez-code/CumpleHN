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
            phCuenta.Visible = autenticado && Servicios.Sesion.EsCiudadano;

            // El menú refleja lo que la administración dejó visible. Para el
            // administrador se muestra todo, porque él sí puede entrar.
            phNavCampanas.Visible = Servicios.Modulos.Visible(Servicios.Modulos.Campanas);
            phNavPerfiles.Visible = Servicios.Modulos.Visible(Servicios.Modulos.Perfiles);
            phNavPropuestas.Visible = Servicios.Modulos.Visible(Servicios.Modulos.Propuestas);
            phNavAnalitica.Visible = Servicios.Modulos.Visible(Servicios.Modulos.Analitica);

            MostrarAvisoConfirmacion();
        }

        /// <summary>
        /// Franja de correo sin confirmar.
        ///
        /// Lo que la sesión sabe decide qué se muestra, nunca qué se permite:
        /// la participación la autoriza el Web Service comprobando contra la
        /// base. Esta franja existe para que la persona sepa qué le falta antes
        /// de chocarse con el rechazo, no para protegerlo.
        /// </summary>
        private void MostrarAvisoConfirmacion()
        {
            if (!Servicios.Sesion.Autenticado || Servicios.Sesion.CorreoConfirmado) return;

            // Si el registro dejó algo que decir —sobre todo cuando el correo
            // no pudo salir— eso es lo que hay que mostrar, y no el texto
            // genérico. Se consume: reaparecer en cada página sin que nada
            // nuevo haya pasado sería ruido.
            string aviso = Servicios.Sesion.TomarAviso();

            litConfirmar.Text = Server.HtmlEncode(string.IsNullOrEmpty(aviso)
                ? "Te falta confirmar tu correo para poder apoyar publicaciones, "
                  + "comentar y responder encuestas."
                : aviso);

            phConfirmar.Visible = true;
        }

        /// <summary>
        /// Reenvío del enlace.
        ///
        /// Corre después de Page_Load, así que lo que escriba acá pisa el
        /// aviso genérico que aquel dejó puesto — que es justo lo que se
        /// quiere. Es el mismo orden del ciclo de vida que en Verificacion.aspx
        /// hacía falta cuidar, solo que acá juega a favor.
        /// </summary>
        protected void lnkReenviar_Click(object sender, EventArgs e)
        {
            var cliente = new webservices.WebServiceGlobalSoapClient();

            webservices.RespuestaAdmin r;
            try
            {
                r = cliente.ReenviarConfirmacion(Servicios.Sesion.CodigoUsuario);
                cliente.Close();
            }
            catch (Exception ex)
            {
                cliente.Abort();
                r = new webservices.RespuestaAdmin
                {
                    ok = false,
                    mensaje = "No se pudo contactar al servidor. " + ex.Message
                };
            }

            litConfirmar.Text = Server.HtmlEncode(r.mensaje);
            phConfirmar.Visible = true;

            // El botón desaparece cuando el envío salió bien: volver a pulsarlo
            // solo se toparía con el intervalo mínimo del procedimiento.
            lnkReenviar.Visible = !r.ok;
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
