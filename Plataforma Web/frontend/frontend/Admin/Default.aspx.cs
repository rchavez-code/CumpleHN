using System;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Resumen del área de administración. Dice qué cuenta es y qué alcance
    /// tiene: toda la plataforma o un espacio. El texto sale de la sesión,
    /// que a su vez sale de lo que el Web Service respondió al entrar.
    /// </summary>
    public partial class AdminDefault : PaginaAdmin
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected string NombreUsuario
        {
            get { return Sesion.Nombre; }
        }

        protected string Titulo
        {
            get
            {
                return Sesion.AdministraPlataforma
                    ? "Administración de la plataforma"
                    : "Administración de " + Sesion.EspacioNombre;
            }
        }

        protected string Subtitulo
        {
            get
            {
                return Sesion.AdministraPlataforma
                    ? "Área privada para la revisión de contenido, los catálogos, los espacios de clientes y la configuración de CumpleHN."
                    : "Área privada del espacio: sus campañas, candidaturas, encuestas y la revisión de su contenido.";
            }
        }

        /// <summary>
        /// Qué pasa con la vigencia del espacio administrado. Vacío cuando no
        /// hay nada que decir (la plataforma, o un espacio vigente). Sale de la
        /// ficha del espacio, que la cuenta del cliente puede leer aunque su
        /// espacio esté vencido: es justamente lo que necesita saber.
        /// </summary>
        protected string AvisoVigencia
        {
            get
            {
                foreach (Modelos.Espacio x in Contenido.Datos.ObtenerEspacios(Sesion.CodigoUsuario, false))
                {
                    if (x.Codigo != Sesion.CodigoEspacio || x.EsPlataforma || x.Vigente) continue;

                    string estado = x.VigenteHasta == DateTime.MinValue
                        ? "todavía no tiene un pago registrado"
                        : "venció el " + x.VigenteHasta.ToString("d MMM yyyy");

                    return "El espacio " + estado + ". Se puede consultar, pero no participar en él ni "
                         + "administrar su contenido hasta que la plataforma registre un pago."
                         + (Sesion.AdministraPlataforma ? " Se registra desde Espacios." : " Contactá a CumpleHN.");
                }
                return string.Empty;
            }
        }

        protected string Alcance
        {
            get
            {
                if (Sesion.AdministraPlataforma)
                    return "Toda la plataforma y todos sus espacios. Ahora mismo está mirando «"
                         + Sesion.EspacioNombre + "»: cambialo desde la barra lateral.";

                return "Solo el espacio «" + Sesion.EspacioNombre + "». Lo que otras organizaciones "
                     + "administran en la plataforma no aparece acá ni se puede tocar desde esta cuenta.";
            }
        }
    }
}
