using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;

namespace backend.Servicios
{
    /// <summary>
    /// Lo que comparten los dos manejadores de archivos: dónde se guardan y
    /// qué tipos se aceptan (script 25).
    ///
    /// SubirArchivo.ashx escribe en esta carpeta y VerArchivo.ashx lee de
    /// ella. Si cada uno armara la ruta por su cuenta, bastaría con que uno
    /// cambiara para que los archivos subidos no se encontraran nunca.
    /// </summary>
    public static class Archivos
    {
        /// <summary>Tamaño máximo de la fotografía: 2 MB.</summary>
        public const int MaximoFoto = 2 * 1024 * 1024;

        /// <summary>Tamaño máximo de un documento de respaldo: 5 MB.</summary>
        public const int MaximoRespaldo = 5 * 1024 * 1024;

        /// <summary>
        /// Carpeta física donde quedan los archivos.
        ///
        /// Por defecto es App_Data/Archivos. IIS nunca entrega el contenido de
        /// App_Data aunque alguien escriba la dirección a mano, así que la
        /// única manera de leer un archivo es VerArchivo.ashx, que antes busca
        /// su registro en la base. Se puede cambiar con la clave
        /// CarpetaArchivos del Web.config.
        /// </summary>
        public static string Carpeta
        {
            get
            {
                string virtualRuta = ConfigurationManager.AppSettings["CarpetaArchivos"];
                if (string.IsNullOrWhiteSpace(virtualRuta)) virtualRuta = "~/App_Data/Archivos";

                string ruta = HostingEnvironment.MapPath(virtualRuta);

                // La primera vez la carpeta no existe. CreateDirectory no hace
                // nada si ya está creada.
                Directory.CreateDirectory(ruta);
                return ruta;
            }
        }

        /// <summary>
        /// Ruta completa de un archivo a partir del nombre guardado en la base.
        ///
        /// Path.GetFileName descarta cualquier carpeta que venga en el nombre.
        /// Los nombres los genera el propio backend, pero si alguna vez una
        /// fila trajera «..\..\Web.config», esto la deja en «Web.config»
        /// dentro de la carpeta de archivos, y nunca fuera de ella.
        /// </summary>
        public static string Ruta(string nombreArchivo)
        {
            return Path.Combine(Carpeta, Path.GetFileName(nombreArchivo));
        }

        /// <summary>
        /// Reconoce el tipo real del archivo por sus primeros bytes y devuelve
        /// su tipo de contenido y su extensión, o null si no es ninguno de los
        /// aceptados.
        ///
        /// No se confía en la extensión del nombre: cambiarla es renombrar el
        /// archivo. Estos bytes, llamados «firma», los escribe el programa que
        /// creó el archivo y no cambian al renombrarlo.
        ///
        ///   JPG empieza con FF D8 FF
        ///   PNG empieza con 89 50 4E 47  («‰PNG»)
        ///   PDF empieza con 25 50 44 46  («%PDF»)
        /// </summary>
        public static TipoArchivo Reconocer(byte[] datos)
        {
            if (datos == null || datos.Length < 4) return null;

            if (datos[0] == 0xFF && datos[1] == 0xD8 && datos[2] == 0xFF)
                return new TipoArchivo("image/jpeg", ".jpg", true);

            if (datos[0] == 0x89 && datos[1] == 0x50 && datos[2] == 0x4E && datos[3] == 0x47)
                return new TipoArchivo("image/png", ".png", true);

            if (datos[0] == 0x25 && datos[1] == 0x50 && datos[2] == 0x44 && datos[3] == 0x46)
                return new TipoArchivo("application/pdf", ".pdf", false);

            return null;
        }
    }

    /// <summary>Un tipo de archivo aceptado.</summary>
    public class TipoArchivo
    {
        public TipoArchivo(string tipoContenido, string extension, bool esImagen)
        {
            TipoContenido = tipoContenido;
            Extension = extension;
            EsImagen = esImagen;
        }

        /// <summary>El que viaja en la cabecera Content-Type al entregarlo.</summary>
        public string TipoContenido { get; private set; }

        /// <summary>La que lleva el nombre en disco.</summary>
        public string Extension { get; private set; }

        /// <summary>La fotografía solo admite imágenes.</summary>
        public bool EsImagen { get; private set; }
    }
}
