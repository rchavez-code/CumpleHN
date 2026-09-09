using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace frontend.Servicios
{
    /// <summary>
    /// Direcciones de los archivos estáticos, con una huella de versión.
    ///
    /// El navegador cachea el CSS y el JS por nombre. Sin la huella, al
    /// editarlos hay que acordarse de recargar sin caché, y mientras tanto la
    /// página se ve o se comporta como la versión anterior — que es una forma
    /// muy eficiente de perder media hora buscando un error que ya estaba
    /// corregido.
    ///
    /// Ya pasó tres veces en este proyecto. Las dos primeras con el CSS y el JS
    /// del tablero, que por eso se enlazan así desde <c>Analitica.aspx</c>. La
    /// tercera con <c>cumplehn.css</c>, que iba por el paquete
    /// <c>~/Content/css</c>: con las optimizaciones apagadas —que es como
    /// corre en desarrollo— ese paquete no combina ni versiona nada, solo
    /// escribe un enlace por archivo con su ruta pelada. El resultado es que
    /// una hoja de estilos recién editada se veía con el diseño anterior.
    ///
    /// Vive en una clase compartida y no repetida en cada página por la misma
    /// razón que la comprobación de rol vive en <c>PaginaSegura</c>: si cada
    /// quien la copia, basta con que una copia se olvide para perder la
    /// garantía, y el síntoma es que todo parece funcionar.
    /// </summary>
    public static class Recursos
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>
        /// Dirección del archivo con <c>?v=</c> y la fecha de su última
        /// modificación.
        ///
        /// Cuando el archivo no se puede leer devuelve la ruta sin huella:
        /// perder el refresco automático es preferible a romper la página.
        /// </summary>
        public static string Url(string ruta)
        {
            string url = VirtualPathUtility.ToAbsolute(ruta);

            try
            {
                DateTime f = File.GetLastWriteTimeUtc(HostingEnvironment.MapPath(ruta));
                return url + "?v=" + f.Ticks.ToString(Inv);
            }
            catch
            {
                return url;
            }
        }
    }
}
