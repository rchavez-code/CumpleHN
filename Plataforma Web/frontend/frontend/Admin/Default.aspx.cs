using System;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Entrada del área de administración.
    ///
    /// En esta etapa el área declara su propio alcance: qué puede hacer la
    /// cuenta hoy y qué falta por habilitar. Las facultades de administración
    /// se agregan como secciones propias, cada una con su procedimiento
    /// almacenado y su método en el Web Service.
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
    }
}
