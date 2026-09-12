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
    /// plataforma, y es el que se resuelve cuando la dirección no lleva
    /// prefijo de espacio. Las páginas no lo pasan al Web Service: lo hace
    /// <see cref="ContenidoServicio"/> por ellas, leyéndolo de acá, para que
    /// una página nueva no pueda olvidarse de acotarse a su espacio.
    ///
    /// El slug sale de la ruta (<c>e/{espacio}/...</c>) cuando la hay, y es
    /// vacío en el resto del sitio. La ficha completa se pide al Web Service
    /// una sola vez por petición y se guarda en <c>HttpContext.Items</c>, por
    /// lo mismo que el estado de los módulos en <see cref="Modulos"/>.
    /// </summary>
    public static class Espacios
    {
        private const string ClaveRuta = "espacio";
        private const string ClaveCache = "cumplehn.espacio";

        /// <summary>
        /// Slug del espacio de la ruta, o vacío cuando es la plataforma.
        /// Es lo que viaja al Web Service en cada consulta pública.
        /// </summary>
        public static string SlugActual
        {
            get
            {
                HttpContext ctx = HttpContext.Current;
                if (ctx == null) return string.Empty;

                RouteData ruta = RouteTable.Routes.GetRouteData(new HttpContextWrapper(ctx));
                if (ruta == null || !ruta.Values.ContainsKey(ClaveRuta)) return string.Empty;

                string slug = Convert.ToString(ruta.Values[ClaveRuta]);
                return slug == null ? string.Empty : slug.Trim().ToLowerInvariant();
            }
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
    }
}
