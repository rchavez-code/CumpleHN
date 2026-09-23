using System;
using System.Net;
using System.Web;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Muestra un archivo guardado en el backend: Archivo.ashx?id=N.
    ///
    /// Es la dirección que usan las páginas, por ejemplo en la foto de una
    /// candidatura o en el enlace a un documento de respaldo.
    ///
    /// Cómo funciona:
    ///   1. Recibe el código del archivo.
    ///   2. Se lo pide a VerArchivo.ashx del backend, que busca el registro
    ///      en la base y con él encuentra el archivo en disco.
    ///   3. Copia la respuesta (bytes y cabeceras) al navegador.
    ///
    /// Por qué no apuntar el navegador directo al backend: es la misma regla
    /// que en Asistente.ashx. El backend no se expone al navegador, y todas
    /// las llamadas salen del servidor del frontend.
    /// </summary>
    public class Archivo : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // Paso 1: el código. Solo números, así no se le puede colar nada
            // a la dirección que se arma para el backend.
            int codigoArchivo;
            if (!int.TryParse(context.Request.QueryString["id"], out codigoArchivo) || codigoArchivo <= 0)
            {
                NoEncontrado(context);
                return;
            }

            // Paso 2: pedírselo al backend.
            byte[] datos;
            string tipoContenido, disposicion;

            try
            {
                using (WebClient cliente = new WebClient())
                {
                    datos = cliente.DownloadData(Archivos.UrlBackend(codigoArchivo));
                    tipoContenido = cliente.ResponseHeaders[HttpResponseHeader.ContentType];
                    disposicion = cliente.ResponseHeaders["Content-Disposition"];
                }
            }
            catch (WebException)
            {
                // El backend respondió 404 (no existe o se quitó) o no está en
                // marcha. Para quien mira la página es lo mismo.
                NoEncontrado(context);
                return;
            }

            // Paso 3: entregarlo con las mismas cabeceras que puso el backend.
            context.Response.ContentType = tipoContenido ?? "application/octet-stream";
            context.Response.AddHeader("X-Content-Type-Options", "nosniff");

            if (!string.IsNullOrEmpty(disposicion))
                context.Response.AddHeader("Content-Disposition", disposicion);

            // Una hora en la caché del navegador: la misma foto aparece en
            // varias tarjetas de una página. Al cambiarla cambia el código, y
            // con él la dirección, así que nunca se ve una foto vieja.
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddHours(1));

            context.Response.BinaryWrite(datos);
        }

        private static void NoEncontrado(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/plain";
            context.Response.Write("No se encontró el archivo.");
        }
    }
}
