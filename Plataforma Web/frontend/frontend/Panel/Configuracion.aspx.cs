using System;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Panel
{
    /// <summary>
    /// Configuración de la cuenta del candidato. En esta etapa muestra el estado
    /// de la cuenta. Las operaciones sobre credenciales dependen del Web Service
    /// de usuarios.
    /// </summary>
    public partial class Configuracion : PaginaPanel
    {
        private Campana _campana;

        protected void Page_Load(object sender, EventArgs e)
        {
            _campana = Contenido.Datos.ObtenerCampana(CandidatoActual.CampanaSlug);
        }

        protected string CorreoAcceso
        {
            get
            {
                return string.IsNullOrEmpty(CandidatoActual.CorreoPublico)
                    ? "Sin correo registrado"
                    : CandidatoActual.CorreoPublico;
            }
        }

        protected string NombreCampana
        {
            get { return _campana != null ? _campana.Nombre : "Sin campaña asociada"; }
        }

        protected string VerificacionTexto
        {
            get { return CandidatoActual.VerificacionTexto; }
        }

        protected string VerificacionClase
        {
            get { return CandidatoActual.VerificacionClase; }
        }
    }
}
