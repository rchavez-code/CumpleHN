using System;
using System.Web;
using System.Web.UI;

namespace frontend
{
    /// <summary>
    /// Página de error del sitio.
    ///
    /// Evita que un fallo del servidor le muestre a la persona una traza de
    /// excepción, que además revela rutas y versiones del servidor. El caso más
    /// frecuente es el rechazo de una entrada por la validación de peticiones de
    /// ASP.NET, y para ese se da una explicación concreta.
    /// </summary>
    public partial class ErrorPagina : Page
    {
        private bool _esEntradaRechazada;

        protected void Page_Load(object sender, EventArgs e)
        {
            Exception ultimo = Server.GetLastError();

            // Con redirectMode ResponseRewrite la excepción original viene
            // envuelta en una HttpUnhandledException.
            if (ultimo != null && ultimo.InnerException != null)
                ultimo = ultimo.InnerException;

            _esEntradaRechazada = ultimo is HttpRequestValidationException;

            phTexto.Visible = _esEntradaRechazada;

            Server.ClearError();
        }

        protected string Titulo
        {
            get
            {
                return _esEntradaRechazada
                    ? "No pudimos guardar ese texto"
                    : "Algo salió mal";
            }
        }

        protected string Mensaje
        {
            get
            {
                return _esEntradaRechazada
                    ? "El contenido que enviaste incluye caracteres que el sistema no acepta por seguridad."
                    : "Ocurrió un error al procesar la página. Si el problema sigue, volvé a intentarlo en unos minutos.";
            }
        }
    }
}
