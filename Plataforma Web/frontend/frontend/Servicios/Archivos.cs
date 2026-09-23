using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using frontend.Modelos;

namespace frontend.Servicios
{
    /// <summary>
    /// Puente del frontend con los manejadores de archivos del backend
    /// (script 25).
    ///
    /// El frontend no guarda archivos ni sabe dónde quedan. Para subir, le
    /// manda los bytes a SubirArchivo.ashx del backend. Para mostrar, las
    /// páginas apuntan a Archivo.ashx del frontend, que se los pide a
    /// VerArchivo.ashx. El navegador nunca habla con el backend.
    ///
    /// La foto se sube con proposito «Foto» y codigoPropuesta en cero. Un
    /// documento de respaldo, con «Respaldo» y el código de su propuesta.
    /// </summary>
    public static class Archivos
    {
        /// <summary>Tamaño máximo de la fotografía. El backend vuelve a comprobarlo.</summary>
        public const int MaximoFoto = 2 * 1024 * 1024;

        /// <summary>Tamaño máximo de un documento de respaldo. El backend vuelve a comprobarlo.</summary>
        public const int MaximoRespaldo = 5 * 1024 * 1024;

        /// <summary>
        /// Dirección del backend, de la clave BackendUrl del Web.config. Por
        /// HTTP: el certificado de desarrollo de HTTPS hace fallar la llamada.
        /// </summary>
        private static string BackendUrl
        {
            get
            {
                string url = ConfigurationManager.AppSettings["BackendUrl"] ?? "http://localhost:51720/";
                return url.EndsWith("/") ? url : url + "/";
            }
        }

        /// <summary>
        /// Dirección con la que una página muestra un archivo. Apunta al
        /// manejador del frontend, nunca al backend. Vacía cuando no hay
        /// archivo, que es lo que las tarjetas entienden como «sin foto».
        /// </summary>
        public static string Url(int codigoArchivo)
        {
            return codigoArchivo > 0 ? "~/Archivo.ashx?id=" + codigoArchivo : string.Empty;
        }

        /// <summary>
        /// Dirección interna del backend que entrega un archivo. La usa solo
        /// Archivo.ashx, del lado del servidor.
        /// </summary>
        public static string UrlBackend(int codigoArchivo)
        {
            return BackendUrl + "VerArchivo.ashx?id=" + codigoArchivo;
        }

        /// <summary>
        /// Sube un archivo al backend y devuelve lo que respondió.
        ///
        /// Los bytes van en el cuerpo del POST y el resto en la dirección.
        /// El nombre original viaja codificado porque puede traer espacios y
        /// tildes. El backend decide todo lo demás: tipo, tamaño y permisos.
        /// </summary>
        public static ResultadoGuardado Subir(int codigoUsuario, string proposito, int codigoPropuesta,
                                              string nombreOriginal, byte[] datos)
        {
            string url = BackendUrl + "SubirArchivo.ashx"
                + "?codigoUsuario=" + codigoUsuario
                + "&proposito=" + HttpUtility.UrlEncode(proposito)
                + "&codigoPropuesta=" + codigoPropuesta
                + "&nombre=" + HttpUtility.UrlEncode(nombreOriginal ?? "archivo");

            try
            {
                using (WebClient cliente = new WebClient())
                {
                    cliente.Headers[HttpRequestHeader.ContentType] = "application/octet-stream";

                    byte[] respuesta = cliente.UploadData(url, "POST", datos);

                    // La respuesta es un JSON { ok, mensaje, codigo }.
                    string texto = System.Text.Encoding.UTF8.GetString(respuesta);
                    Dictionary<string, object> json =
                        new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(texto);

                    return new ResultadoGuardado
                    {
                        Ok = Convert.ToBoolean(json["ok"]),
                        Mensaje = Convert.ToString(json["mensaje"]),
                        Codigo = Convert.ToInt32(json["codigo"])
                    };
                }
            }
            catch (Exception)
            {
                // Backend apagado, tiempo agotado o respuesta ilegible: para
                // quien sube el archivo es lo mismo, no se guardó.
                return new ResultadoGuardado
                {
                    Ok = false,
                    Mensaje = "No se pudo contactar al servidor de archivos. Intentá de nuevo."
                };
            }
        }
    }
}
