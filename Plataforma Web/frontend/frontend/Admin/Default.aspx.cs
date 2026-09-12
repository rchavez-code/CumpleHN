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
