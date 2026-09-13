using System;
using System.Web;
using System.Web.Routing;

namespace frontend.Servicios
{
    /// <summary>
    /// El espacio que la petición actual está mirando.
    ///
    /// Un espacio es un cliente que usa la plataforma para su propio proceso
    /// electoral. El sitio público CumpleHN es también un espacio, el de la
    /// plataforma, y es el que se resuelve cuando nada dice otra cosa. Las
    /// páginas no lo pasan al Web Service: lo hace
    /// <see cref="ContenidoServicio"/> por ellas, leyéndolo de acá, para que
    /// una página nueva no pueda olvidarse de acotarse a su espacio.
    ///
    /// El espacio llega por dos caminos, y por eso hay dos fuentes:
    ///
    ///   · Las páginas de listado (portada, campañas, candidatos, partidos,
    ///     propuestas, analítica) lo llevan en la ruta, <c>e/{espacio}/...</c>
    ///     (RouteConfig). Sin prefijo es la plataforma.
    ///
    ///   · Las páginas de detalle (una campaña, una candidatura, un partido,
    ///     una propuesta) no lo llevan: el objeto que muestran ya sabe a qué
    ///     espacio pertenece, y la página lo declara con <see cref="Fijar"/>
    ///     apenas lo carga. Así un enlace compartido sin prefijo se dibuja
    ///     igual dentro de su espacio, con su marca y su navegación, en lugar
    ///     de aparecer como si fuera contenido de la plataforma.
    ///
    /// La ficha completa se pide al Web Service una sola vez por petición y
    /// se guarda en <c>HttpContext.Items</c>, por lo mismo que el estado de
    /// los módulos en <see cref="Modulos"/>.
    /// </summary>
    public static class Espacios
    {
        private const string ClaveRuta = "espacio";
        private const string ClaveFijado = "cumplehn.espacio.slug";
        private const string ClaveCache = "cumplehn.espacio";

        /// <summary>
        /// Slug del espacio actual, o vacío cuando es la plataforma. Es lo
        /// que viaja al Web Service en cada consulta pública.
        /// </summary>
        public static string SlugActual
        {
            get
            {
                HttpContext ctx = HttpContext.Current;
                if (ctx == null) return string.Empty;

                // Lo que una página de detalle declaró manda sobre la ruta.
                if (ctx.Items.Contains(ClaveFijado))
                    return Convert.ToString(ctx.Items[ClaveFijado]);

                // Dentro de la administración, el espacio es el que la sesión
                // administra: es de ahí que salen los catálogos (cargos) que un
                // alta de candidatura ofrece. Sin esto, Admin/ pedía siempre los
                // de la plataforma.
                string pagina = ctx.Request.AppRelativeCurrentExecutionFilePath ?? string.Empty;
                if (pagina.StartsWith("~/Admin/", StringComparison.OrdinalIgnoreCase))
                    return Sesion.EspacioSlug ?? string.Empty;

                RouteData ruta = ctx.Request.RequestContext == null
                    ? null
                    : ctx.Request.RequestContext.RouteData;

                if (ruta == null || !ruta.Values.ContainsKey(ClaveRuta)) return string.Empty;

                string slug = Convert.ToString(ruta.Values[ClaveRuta]);
                return slug == null ? string.Empty : slug.Trim().ToLowerInvariant();
            }
        }

        /// <summary>
        /// Declara el espacio de la petición a partir del objeto que la página
        /// muestra. Lo llaman las páginas de detalle apenas cargan su objeto,
        /// y antes de pedir nada más: lo que se pida después va acotado a este
        /// espacio. Vacío o nulo es la plataforma.
        /// </summary>
        public static void Fijar(string slug)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null) return;

            string s = slug == null ? string.Empty : slug.Trim().ToLowerInvariant();

            ctx.Items[ClaveFijado] = s;
            ctx.Items.Remove(ClaveCache);
        }

        /// <summary>
        /// La ficha del espacio actual, o nula si el slug no existe. Resuelta
        /// una vez por petición. Es lo que la plantilla usa para decir de
        /// quién es lo que se está viendo.
        /// </summary>
        public static Modelos.Espacio Actual
        {
            get
            {
                HttpContext ctx = HttpContext.Current;
                if (ctx == null) return Contenido.Datos.ObtenerEspacio(string.Empty);

                if (!ctx.Items.Contains(ClaveCache))
                    ctx.Items[ClaveCache] = Contenido.Datos.ObtenerEspacio(SlugActual);

                return ctx.Items[ClaveCache] as Modelos.Espacio;
            }
        }

        /// <summary>La petición está en el sitio público de la plataforma.</summary>
        public static bool EsPlataforma
        {
            get
            {
                Modelos.Espacio e = Actual;
                return e == null || e.EsPlataforma;
            }
        }

        /// <summary>
        /// Si la ruta nombra un espacio que no existe o fue retirado, manda a
        /// la portada de la plataforma. Lo llaman las páginas de listado desde
        /// su clase base (PaginaDeModulo), en OnPreInit.
        /// </summary>
        public static void ExigirExistente()
        {
            if (SlugActual.Length > 0 && Actual == null)
                HttpContext.Current.Response.Redirect("~/");
        }

        /// <summary>
        /// Una dirección de página de listado, dentro del espacio actual.
        /// <c>~/Campanas</c> queda igual en la plataforma y pasa a
        /// <c>~/e/{espacio}/Campanas</c> dentro de un espacio. Es lo que usan
        /// la navegación de la plantilla, las migas y los redireccionamientos
        /// de las páginas públicas. Las fichas (<c>~/Campana?c=</c> y el
        /// resto) no pasan por acá: no llevan prefijo y derivan el espacio del
        /// objeto.
        /// </summary>
        public static string Url(string rutaApp)
        {
            // La plataforma nunca lleva prefijo, ni siquiera cuando una ficha
            // fijó su slug: su dirección es la raíz.
            string slug = SlugActual;
            if (slug.Length == 0 || string.IsNullOrEmpty(rutaApp) || EsPlataforma) return rutaApp;

            string resto = rutaApp.StartsWith("~/") ? rutaApp.Substring(2) : rutaApp.TrimStart('/');
            return resto.Length == 0 ? "~/e/" + slug : "~/e/" + slug + "/" + resto;
        }
    }
}
