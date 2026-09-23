using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace backend
{
    /// <summary>
    /// Entrega un archivo por su código: VerArchivo.ashx?id=N (script 25).
    ///
    /// Quién lo llama: el manejador Archivo.ashx del frontend, que le pasa
    /// la respuesta al navegador tal cual.
    ///
    /// Cómo llega del código al archivo:
    ///   1. Busca la fila con spArchivoObtener. La base solo la devuelve si
    ///      el archivo está activo y su candidatura también.
    ///   2. Con el nombre guardado en la fila arma la ruta en disco.
    ///   3. Devuelve los bytes con el tipo de contenido que dice la fila.
    ///
    /// Sin fila no hay archivo, aunque siga en disco: así funciona la baja
    /// lógica. Quitar un archivo no lo borra, pero deja de entregarse.
    /// </summary>
    public class VerArchivo : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            int codigoArchivo;
            if (!int.TryParse(context.Request.QueryString["id"], out codigoArchivo) || codigoArchivo <= 0)
            {
                NoEncontrado(context);
                return;
            }

            // ------------------------------- Paso 1: buscar el registro

            string nombreArchivo = null, nombreOriginal = null, tipoContenido = null;

            string cadena = ConfigurationManager.ConnectionStrings["CnxCumpleHN"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(cadena))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("dbo.spArchivoObtener", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoArchivo", codigoArchivo);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nombreArchivo = Convert.ToString(reader["nombreArchivo"]);
                        nombreOriginal = Convert.ToString(reader["nombreOriginal"]);
                        tipoContenido = Convert.ToString(reader["tipoContenido"]);
                    }
                }
            }

            if (nombreArchivo == null)
            {
                NoEncontrado(context);
                return;
            }

            // ---------------------------- Paso 2: la ruta en disco

            string ruta = Servicios.Archivos.Ruta(nombreArchivo);

            if (!File.Exists(ruta))
            {
                NoEncontrado(context);
                return;
            }

            // ------------------------------ Paso 3: entregar los bytes

            context.Response.ContentType = tipoContenido;

            // Que el navegador respete el tipo declarado y no intente
            // adivinarlo: un archivo que dice ser imagen se trata como imagen.
            context.Response.AddHeader("X-Content-Type-Options", "nosniff");

            // «inline» lo muestra en el navegador en lugar de descargarlo. El
            // nombre original viaja codificado, por las tildes y los espacios.
            context.Response.AddHeader("Content-Disposition",
                "inline; filename*=UTF-8''" + Uri.EscapeDataString(nombreOriginal));

            context.Response.TransmitFile(ruta);
        }

        private static void NoEncontrado(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/plain";
            context.Response.Write("No se encontró el archivo.");
        }
    }
}
