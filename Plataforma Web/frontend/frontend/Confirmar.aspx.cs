using System;
using System.Web.UI;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Destino del enlace que llega por correo al registrarse.
    ///
    /// El token viaja en la dirección porque tiene que sobrevivir un viaje por
    /// correo y una pegada en la barra del navegador. Es de un solo uso y de
    /// vida corta, y el backend solo guarda su hash, así que la dirección deja
    /// de servir en cuanto se usa.
    ///
    /// La página no valida nada: manda el token al Web Service y muestra lo que
    /// conteste. Quien decide es la base, que es la única que sabe si el token
    /// existe, si venció y si ya se gastó.
    /// </summary>
    public partial class Confirmar : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            string token = Request.QueryString["t"];

            if (string.IsNullOrEmpty(token))
            {
                Mostrar(false, "El enlace está incompleto",
                    "La dirección no trae el código de confirmación. Copiala completa desde el "
                    + "correo que te enviamos.");
                return;
            }

            var cliente = new webservices.WebServiceGlobalSoapClient();

            webservices.RespuestaAdmin r;
            try
            {
                r = cliente.ConfirmarCorreo(token);
                cliente.Close();
            }
            catch (Exception ex)
            {
                cliente.Abort();
                Mostrar(false, "No se pudo confirmar",
                    "No se pudo contactar al servidor. " + ex.Message);
                return;
            }

            // Si quien abre el enlace es la misma persona que ya tiene la sesión
            // abierta, se le actualiza acá. Sin esto tendría que salir y volver
            // a entrar para que desapareciera el aviso de la plantilla.
            if (r.ok) Sesion.MarcarCorreoConfirmado();

            Mostrar(r.ok, r.ok ? "Tu correo quedó confirmado" : "El enlace no sirve", r.mensaje);
        }

        private void Mostrar(bool ok, string titulo, string mensaje)
        {
            litTitulo.Text = Server.HtmlEncode(titulo);
            litMensaje.Text = Server.HtmlEncode(mensaje);

            phSeguir.Visible = ok;
            phEntrar.Visible = !ok;
        }
    }
}
