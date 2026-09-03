using System;
using System.Collections.Generic;
using System.Web;
using frontend.Modelos;

namespace frontend.Servicios
{
    /// <summary>
    /// Qué partes del sitio están visibles para quien consulta.
    ///
    /// La administración puede ocultar un módulo entero o un gráfico concreto
    /// del tablero. Lo oculto desaparece del sitio público y sigue visible para
    /// el rol Administrador, con un aviso: así se puede revisar antes de
    /// publicar, o retirar algo que no está listo sin perder la capacidad de
    /// verlo.
    ///
    /// El estado se consulta <b>una sola vez por petición</b>. Sin esa caché,
    /// una página con doce gráficos abriría doce llamadas al Web Service para
    /// responder doce veces la misma pregunta.
    /// </summary>
    public static class Modulos
    {
        /* Las claves las declara el catálogo dbo.Modulos. Se repiten acá como
           constantes para que ninguna página compare contra una cadena escrita
           a mano: una clave mal escrita no da error, simplemente deja el
           elemento siempre visible, que es el fallo más difícil de notar. */

        public const string Campanas = "campanas";
        public const string Perfiles = "perfiles";
        public const string Propuestas = "propuestas";
        public const string Interaccion = "interaccion";
        public const string Analitica = "analitica";

        public const string AnaliticaKpi = "analitica.kpi";
        public const string AnaliticaHallazgos = "analitica.hallazgos";
        public const string AnaliticaBrecha = "analitica.brecha";
        public const string AnaliticaEstados = "analitica.estados";
        public const string AnaliticaPartidos = "analitica.partidos";
        public const string AnaliticaDensidad = "analitica.densidad";
        public const string AnaliticaSigno = "analitica.signo";
        public const string AnaliticaTipos = "analitica.tipos";
        public const string AnaliticaRanking = "analitica.ranking";
        public const string AnaliticaActividad = "analitica.actividad";
        public const string AnaliticaTerritorio = "analitica.territorio";
        public const string AnaliticaVerificacion = "analitica.verificacion";
        public const string AnaliticaAsistente = "analitica.asistente";

        private const string ClaveCache = "cumplehn.modulos";

        /// <summary>
        /// Estado de todos los elementos, resuelto una vez por petición.
        /// </summary>
        private static IDictionary<string, bool> Estado
        {
            get
            {
                HttpContext ctx = HttpContext.Current;

                // Sin contexto no hay caché posible. Se consulta directo, que
                // es el caso de las pruebas y de cualquier proceso fuera de una
                // petición web.
                if (ctx == null) return Consultar();

                IDictionary<string, bool> cache = ctx.Items[ClaveCache] as IDictionary<string, bool>;

                if (cache == null)
                {
                    cache = Consultar();
                    ctx.Items[ClaveCache] = cache;
                }

                return cache;
            }
        }

        private static IDictionary<string, bool> Consultar()
        {
            Dictionary<string, bool> mapa = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            foreach (EstadoModulo m in Contenido.Datos.ObtenerModulosVisibles())
            {
                mapa[m.Clave] = m.Visible;
            }

            return mapa;
        }

        /// <summary>
        /// Si el elemento está visible para el público.
        ///
        /// Una clave desconocida devuelve verdadero: si el catálogo todavía no
        /// registra un elemento, o el backend no responde, el sitio se muestra
        /// completo. Fallar hacia mostrar es lo correcto acá — un error de
        /// comunicación no debería vaciar la plataforma.
        /// </summary>
        public static bool Habilitado(string clave)
        {
            bool visible;
            return !Estado.TryGetValue(clave, out visible) || visible;
        }

        /// <summary>
        /// Si el elemento debe dibujarse en esta petición.
        ///
        /// Es lo que consultan las páginas. Devuelve verdadero cuando el
        /// elemento está visible, y también cuando está oculto pero quien mira
        /// administra la plataforma.
        /// </summary>
        public static bool Visible(string clave)
        {
            return Habilitado(clave) || Sesion.EsAdministrador;
        }

        /// <summary>
        /// Si el elemento se está mostrando solo porque quien mira es
        /// administrador. La página lo usa para avisar que el público no lo ve:
        /// sin ese aviso, el administrador creería que el sitio se ve como lo
        /// está viendo él.
        /// </summary>
        public static bool OcultoAlPublico(string clave)
        {
            return !Habilitado(clave) && Sesion.EsAdministrador;
        }

        /// <summary>
        /// Corta la petición cuando el módulo está oculto y quien entra no
        /// administra.
        ///
        /// Ocultar el enlace del menú no alcanza: la dirección se puede
        /// escribir a mano, y un módulo que se sigue sirviendo a quien conoce
        /// su URL no está oculto.
        /// </summary>
        public static void ExigirVisible(string clave)
        {
            if (Visible(clave)) return;

            HttpContext ctx = HttpContext.Current;
            if (ctx != null) ctx.Response.Redirect("~/");
        }
    }
}
