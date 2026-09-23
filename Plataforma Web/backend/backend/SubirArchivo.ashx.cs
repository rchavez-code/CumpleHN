using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using backend.Servicios;

namespace backend
{
    /// <summary>
    /// Recibe un archivo del panel de la candidatura y lo guarda (script 25).
    ///
    /// Quién lo llama: el servidor del frontend, nunca el navegador. La página
    /// del panel recibe el archivo en su postback y lo reenvía acá con
    /// Servicios/Archivos.cs del frontend.
    ///
    /// Qué recibe:
    ///   - En la dirección: codigoUsuario, proposito (Foto o Respaldo),
    ///     codigoPropuesta (cero en la foto) y nombre (el nombre original).
    ///   - En el cuerpo del POST: los bytes del archivo, tal cual.
    ///
    /// Qué hace, en este orden:
    ///   1. Comprueba el tamaño y el tipo real del archivo.
    ///   2. Le pone un nombre nuevo y lo escribe en la carpeta de archivos.
    ///   3. Registra la fila con spArchivoRegistrar, que comprueba los permisos.
    ///   4. Si la base rechaza, borra el archivo recién escrito.
    ///
    /// Responde siempre un JSON { ok, mensaje, codigo }.
    ///
    /// Deuda conocida, la misma de todo el Web Service (anexo OWASP): el
    /// código de usuario viene en el parámetro y no en un token de sesión.
    /// Lo que sí garantiza la base es que ese usuario solo puede subir
    /// archivos a su propia candidatura.
    /// </summary>
    public class SubirArchivo : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (context.Request.HttpMethod != "POST")
            {
                Responder(context, false, "El archivo se envía por POST.", 0);
                return;
            }

            // ---------------------------------------- Datos de la dirección

            int codigoUsuario, codigoPropuesta;
            int.TryParse(context.Request.QueryString["codigoUsuario"], out codigoUsuario);
            int.TryParse(context.Request.QueryString["codigoPropuesta"], out codigoPropuesta);

            string proposito = context.Request.QueryString["proposito"] ?? string.Empty;
            string nombreOriginal = Path.GetFileName(context.Request.QueryString["nombre"] ?? "archivo");

            if (proposito != "Foto" && proposito != "Respaldo")
            {
                Responder(context, false, "El tipo de archivo no es válido.", 0);
                return;
            }

            // ------------------------------------------- Paso 1: validar

            // El tamaño se mira antes de leer nada, para no cargar en memoria
            // un archivo que de todos modos se va a rechazar.
            int maximo = proposito == "Foto" ? Archivos.MaximoFoto : Archivos.MaximoRespaldo;
            int tamano = context.Request.ContentLength;

            if (tamano <= 0)
            {
                Responder(context, false, "No se recibió ningún archivo.", 0);
                return;
            }

            if (tamano > maximo)
            {
                Responder(context, false, proposito == "Foto"
                    ? "La fotografía supera los 2 MB permitidos."
                    : "El documento supera los 5 MB permitidos.", 0);
                return;
            }

            byte[] datos = new byte[tamano];
            int leidos = 0;
            while (leidos < tamano)
            {
                int n = context.Request.InputStream.Read(datos, leidos, tamano - leidos);
                if (n == 0) break;
                leidos += n;
            }

            // El tipo real sale de los primeros bytes, no de la extensión.
            TipoArchivo tipo = Archivos.Reconocer(datos);

            if (tipo == null || (proposito == "Foto" && !tipo.EsImagen))
            {
                Responder(context, false, proposito == "Foto"
                    ? "La fotografía tiene que ser JPG o PNG."
                    : "El documento tiene que ser PDF, JPG o PNG.", 0);
                return;
            }

            // --------------------------------------- Paso 2: escribir en disco

            // Un GUID como nombre: nunca se repite y no contiene nada de lo
            // que escribió la persona. El nombre original solo se guarda en
            // la base, para mostrarlo.
            string nombreArchivo = Guid.NewGuid().ToString("N") + tipo.Extension;
            string ruta = Archivos.Ruta(nombreArchivo);

            File.WriteAllBytes(ruta, datos);

            // -------------------------------- Paso 3: registrar en la base

            bool ok = false;
            string mensaje = "No se pudo registrar el archivo.";
            int codigo = 0;

            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["CnxCumpleHN"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(cadena))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spArchivoRegistrar", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                    cmd.Parameters.AddWithValue("@proposito", proposito);
                    cmd.Parameters.AddWithValue("@codigoPropuesta", codigoPropuesta);
                    cmd.Parameters.AddWithValue("@nombreArchivo", nombreArchivo);
                    cmd.Parameters.AddWithValue("@nombreOriginal", nombreOriginal);
                    cmd.Parameters.AddWithValue("@tipoContenido", tipo.TipoContenido);
                    cmd.Parameters.AddWithValue("@tamanoBytes", tamano);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ok = Convert.ToBoolean(reader["ok"]);
                            mensaje = Convert.ToString(reader["mensaje"]);
                            codigo = Convert.ToInt32(reader["codigo"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                mensaje = "No se pudo registrar el archivo: " + ex.Message;
            }

            // --------------------- Paso 4: si la base rechazó, no dejar rastro

            // Un archivo en disco sin su fila no lo puede entregar nadie, pero
            // ocuparía espacio para siempre. Se borra acá, que es el único
            // momento en que se sabe que sobra.
            if (!ok && File.Exists(ruta)) File.Delete(ruta);

            Responder(context, ok, mensaje, codigo);
        }

        /// <summary>Escribe la respuesta JSON { ok, mensaje, codigo }.</summary>
        private static void Responder(HttpContext context, bool ok, string mensaje, int codigo)
        {
            JavaScriptSerializer json = new JavaScriptSerializer();
            context.Response.Write(json.Serialize(new { ok = ok, mensaje = mensaje, codigo = codigo }));
        }
    }
}
