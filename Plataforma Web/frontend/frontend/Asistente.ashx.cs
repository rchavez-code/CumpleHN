using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Puerta del asistente para las llamadas desde script.
    ///
    /// Por qué existe, y no un postback. La consulta al modelo tarda entre
    /// diez y veinte segundos. En un postback eso deja la página entera
    /// congelada bajo el velo de carga todo ese rato, y encima pierde el
    /// desplazamiento y la pestaña activa del tablero. Acá la página se queda
    /// donde está y solo el bloque del asistente muestra que está trabajando.
    ///
    /// Por qué no se llama al backend directamente desde el navegador. El
    /// frontend y el backend son orígenes distintos, así que haría falta
    /// abrir CORS en el Web Service. Eso dejaría a cualquier página de
    /// cualquier sitio invocando el método que gasta dinero. La llamada sale
    /// del servidor del frontend, como todas las demás del proyecto.
    ///
    /// Por qué IRequiresSessionState. Un manejador no recibe la sesión salvo
    /// que lo pida, y sin sesión no hay código de usuario que mandar — el
    /// asistente quedaría cerrado para todo el mundo, incluso con sesión
    /// iniciada.
    /// </summary>
    public class Asistente : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (!string.Equals(context.Request.HttpMethod, "POST",
                               StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405;
                Escribir(context, Rechazo("Método no permitido."));
                return;
            }

            // El módulo se comprueba también acá, aunque el Web Service lo
            // vuelva a comprobar. Esconder el bloque de la página no alcanza:
            // esta dirección se puede escribir a mano.
            if (!Modulos.Visible(Modulos.AnaliticaAsistente))
            {
                Escribir(context, Rechazo("El asistente no está disponible."));
                return;
            }

            if (!Sesion.Autenticado)
            {
                Escribir(context, Rechazo("Para preguntarle al asistente hay que "
                                        + "iniciar sesión."));
                return;
            }

            Peticion p = Leer(context);

            if (p == null || string.IsNullOrEmpty((p.pregunta ?? string.Empty).Trim()))
            {
                Escribir(context, Rechazo("Escribí una pregunta."));
                return;
            }

            RespuestaAsistente r = Contenido.Datos.PreguntarAsistente(
                Sesion.CodigoUsuario, p.pregunta.Trim(), p.campanaSlug);

            Escribir(context, new Dictionary<string, object>
            {
                { "ok",        r.Ok },
                { "respuesta", r.Respuesta },
                { "fuentes",   r.Fuentes },
                { "mensaje",   r.Mensaje },
                { "restantes", r.Restantes }
            });
        }

        // =============================================================
        //  Auxiliares
        // =============================================================

        /// <summary>Cuerpo de la petición. Nulo si no viene o no es JSON válido.</summary>
        private static Peticion Leer(HttpContext context)
        {
            try
            {
                string cuerpo;

                using (StreamReader lector = new StreamReader(context.Request.InputStream))
                    cuerpo = lector.ReadToEnd();

                if (string.IsNullOrEmpty(cuerpo)) return null;

                return new JavaScriptSerializer().Deserialize<Peticion>(cuerpo);
            }
            catch
            {
                return null;
            }
        }

        private static Dictionary<string, object> Rechazo(string mensaje)
        {
            return new Dictionary<string, object>
            {
                { "ok",        false },
                { "respuesta", string.Empty },
                { "fuentes",   new string[0] },
                { "mensaje",   mensaje },
                { "restantes", 0 }
            };
        }

        private static void Escribir(HttpContext context, object dato)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(dato));
        }

        /// <summary>
        /// Los nombres van en minúscula porque son los del JSON que manda el
        /// script, y el deserializador empareja por nombre exacto.
        /// </summary>
        private class Peticion
        {
            public string pregunta { get; set; }
            public string campanaSlug { get; set; }
        }
    }
}
