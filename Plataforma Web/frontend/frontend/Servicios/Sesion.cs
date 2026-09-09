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

        /// <summary>
        /// Nombres de rol tal como los guarda el catálogo <c>Roles</c> de la
        /// base de datos. Son constantes acá para que ninguna comparación
        /// dependa de una cadena escrita a mano dentro de una página.
        /// </summary>
        public const string RolAdministrador = "Administrador";
        public const string RolCandidato = "Candidato";
        public const string RolCiudadano = "Ciudadano";

        /// <summary>
        /// Abre la sesión con los datos que devolvió el backend.
        ///
        /// Lo llaman el acceso y el registro, que son las dos maneras de entrar
        /// a la plataforma. Está acá en lugar de repetido en las dos páginas
        /// por la misma razón por la que las claves de <c>Session</c> son
        /// constantes: si una de las dos guardara una clave de menos, el
        /// síntoma aparecería mucho después y en otra página.
        /// </summary>
        public static void Iniciar(webservices.InfoUsuario usuario)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null || usuario == null) return;

            ctx.Session["usuario"] = usuario;
            ctx.Session[ClaveUsuario] = usuario.codigoUsuario;
            ctx.Session[ClaveNombre] = usuario.nombre;
            ctx.Session[ClaveRol] = usuario.rol;
            ctx.Session[ClaveCandidato] = usuario.candidatoSlug;
        }

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

        private static bool EsRol(string rol)
        {
            return string.Equals(Rol, rol, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsCandidato
        {
            get { return EsRol(RolCandidato); }
        }

        /// <summary>
        /// Administrador de la plataforma: verifica contenido, modera
        /// publicaciones y administra los catálogos.
        ///
        /// Esta propiedad decide qué se le muestra a quién. Nunca alcanza por sí
        /// sola para autorizar una acción que escriba: la sesión vive en el
        /// frontend, así que el backend tiene que confirmar el rol contra la
        /// base antes de modificar nada.
        /// </summary>
        public static bool EsAdministrador
        {
            get { return EsRol(RolAdministrador); }
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

        /// <summary>
        /// Destino al que se puede volver después de entrar o de registrarse.
        ///
        /// Acepta únicamente rutas dentro del propio sitio. Sin esta
        /// comprobación, un enlace con <c>?volver=https://sitio-ajeno</c>
        /// convertiría el acceso en un redirector hacia cualquier destino, que
        /// es la vulnerabilidad de redirección abierta que revisa OWASP.
        ///
        /// La comprueban el acceso y el registro. Vive acá y no copiada en cada
        /// página porque una copia que se quede atrás no se nota: la página
        /// sigue funcionando igual, solo deja de estar protegida.
        /// </summary>
        public static bool EsDestinoSeguro(string ruta)
        {
            if (string.IsNullOrEmpty(ruta)) return false;
            if (!ruta.StartsWith("/")) return false;

            // Descarta "//servidor" y "/\servidor", que el navegador interpreta
            // como direcciones absolutas hacia otro dominio.
            if (ruta.StartsWith("//") || ruta.StartsWith("/\\")) return false;

            return true;
        }
    }
}
