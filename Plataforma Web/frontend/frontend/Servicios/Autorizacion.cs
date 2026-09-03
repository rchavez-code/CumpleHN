using System;
using System.Web.UI;
using frontend.Modelos;

namespace frontend.Servicios
{
    /// <summary>
    /// Reglas de acceso a las áreas privadas del sitio.
    ///
    /// La consulta pública no exige cuenta. Lo que se protege acá son las dos
    /// áreas privadas: el panel de la candidatura y la administración de la
    /// plataforma.
    ///
    /// La comprobación no puede delegarse en <c>&lt;authorization&gt;</c> del
    /// Web.config porque ese mecanismo se apoya en la autenticación de
    /// formularios de ASP.NET, y CumpleHN guarda la sesión en <c>Session</c>
    /// después de validar contra el Web Service. Por eso se resuelve en código,
    /// y por eso se resuelve en una clase base de página en lugar de repetirse
    /// en cada archivo: una página nueva que olvide copiar el bloque quedaría
    /// abierta, que es el fallo de control de acceso del A01 de OWASP.
    /// </summary>
    public static class Autorizacion
    {
        /// <summary>
        /// Área a la que pertenece cada rol. Es el destino después de entrar y
        /// también a donde se devuelve a quien toca un área que no le
        /// corresponde.
        /// </summary>
        public static string InicioDe(string rol)
        {
            if (string.Equals(rol, Sesion.RolAdministrador, StringComparison.OrdinalIgnoreCase))
                return "~/Admin/";

            if (string.Equals(rol, Sesion.RolCandidato, StringComparison.OrdinalIgnoreCase))
                return "~/Panel/";

            // Un ciudadano registrado no tiene área privada: participa en las
            // páginas públicas.
            return "~/";
        }

        /// <summary>Área que le corresponde a quien está en sesión ahora.</summary>
        public static string InicioDelUsuario()
        {
            return InicioDe(Sesion.Rol);
        }
    }

    /// <summary>
    /// Página que solo se sirve a un rol determinado.
    ///
    /// La comprobación ocurre en <c>OnPreInit</c>, que es el primer paso del
    /// ciclo de vida: se decide antes de que exista un solo control, de modo
    /// que ningún dato del área privada llega a construirse para quien no
    /// debería verlo.
    /// </summary>
    public abstract class PaginaSegura : Page
    {
        /// <summary>Rol del catálogo <c>Roles</c> que da acceso a la página.</summary>
        protected abstract string RolExigido { get; }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            // Sin sesión se pide acceso, conservando la dirección para volver
            // acá una vez que entre.
            if (!Sesion.Autenticado)
            {
                Response.Redirect(Sesion.UrlAccesoDeVuelta());
                return;
            }

            // Con sesión pero con otro rol no se pide acceso de nuevo, que sería
            // un rodeo sin salida: se le devuelve a su propia área.
            if (!string.Equals(Sesion.Rol, RolExigido, StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(Autorizacion.InicioDelUsuario());
                return;
            }

            AlAutorizar();
        }

        /// <summary>
        /// Gancho para lo que cada área necesite resolver una vez confirmado el
        /// rol. Se ejecuta solo si el acceso fue concedido.
        /// </summary>
        protected virtual void AlAutorizar()
        {
        }
    }

    /// <summary>
    /// Página del panel privado de una candidatura.
    ///
    /// Además del rol, garantiza que la sesión resuelva una candidatura real.
    /// Las páginas heredan <see cref="CandidatoActual"/> ya resuelto y no nulo,
    /// así que no vuelven a comprobarlo.
    /// </summary>
    public abstract class PaginaPanel : PaginaSegura
    {
        private Candidato _candidato;

        protected override string RolExigido
        {
            get { return Sesion.RolCandidato; }
        }

        protected override void AlAutorizar()
        {
            _candidato = Contenido.Datos.ObtenerCandidatoAutenticado();

            // La cuenta tiene rol Candidato pero no resuelve una ficha: es una
            // inconsistencia de datos, no una sesión válida.
            if (_candidato == null)
                Response.Redirect("~/Acceso");
        }

        /// <summary>Candidatura dueña de la sesión. Nunca es nula dentro de la página.</summary>
        public Candidato CandidatoActual
        {
            get { return _candidato; }
        }
    }

    /// <summary>
    /// Página del área de administración de la plataforma.
    /// </summary>
    public abstract class PaginaAdmin : PaginaSegura
    {
        protected override string RolExigido
        {
            get { return Sesion.RolAdministrador; }
        }
    }
}
