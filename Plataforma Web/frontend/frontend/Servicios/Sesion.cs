using System;
using System.Web;

namespace frontend.Servicios
{
    /// <summary>
    /// Acceso a los datos de la sesión actual.
    ///
    /// Concentra la lectura de <c>Session</c> en un solo lugar para que las
    /// páginas y los controles no repartan cadenas mágicas como
    /// <c>Session["codigoUsuario"]</c> por todo el proyecto.
    /// </summary>
    public static class Sesion
    {
        private const string ClaveUsuario = "codigoUsuario";
        private const string ClaveNombre = "nombreUsuario";
        private const string ClaveRol = "rol";
        private const string ClaveCandidato = "candidatoSlug";

        private static object Leer(string clave)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null) return null;
            return ctx.Session[clave];
        }

        /// <summary>Código del usuario en sesión, o cero si nadie inició sesión.</summary>
        public static int CodigoUsuario
        {
            get
            {
                object v = Leer(ClaveUsuario);
                if (v == null) return 0;

                int codigo;
                return int.TryParse(Convert.ToString(v), out codigo) ? codigo : 0;
            }
        }

        public static bool Autenticado
        {
            get { return CodigoUsuario > 0; }
        }

        public static string Nombre
        {
            get { return Convert.ToString(Leer(ClaveNombre)); }
        }

        public static string Rol
        {
            get { return Convert.ToString(Leer(ClaveRol)); }
        }

        public static string CandidatoSlug
        {
            get { return Convert.ToString(Leer(ClaveCandidato)); }
        }

        public static bool EsCandidato
        {
            get { return string.Equals(Rol, "Candidato", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Iniciales del usuario en sesión, para el avatar del formulario de
        /// comentarios.
        /// </summary>
        public static string Iniciales
        {
            get { return Modelos.Vista.InicialesDe(Nombre); }
        }

        /// <summary>
        /// Dirección de acceso que regresa a la página actual después de entrar.
        ///
        /// Es lo que hace que pedir cuenta no le cueste al usuario perder dónde
        /// estaba: da su opinión, se le pide acceso, y vuelve al mismo lugar.
        /// </summary>
        public static string UrlAccesoDeVuelta()
        {
            HttpContext ctx = HttpContext.Current;

            string volver = ctx == null || ctx.Request == null
                ? "~/"
                : ctx.Request.Url.PathAndQuery;

            return "~/Acceso?volver=" + HttpUtility.UrlEncode(volver);
        }
    }
}
