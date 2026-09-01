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
    public partial class Configuracion : Page
    {
        private Candidato _candidato;
        private Campana _campana;

        protected void Page_Load(object sender, EventArgs e)
        {
            _candidato = Contenido.Datos.ObtenerCandidatoAutenticado();

            if (_candidato == null)
            {
                Response.Redirect("~/Acceso");
                return;
            }

            _campana = Contenido.Datos.ObtenerCampana(_candidato.CampanaSlug);
        }

        protected string CorreoAcceso
        {
            get
            {
                return string.IsNullOrEmpty(_candidato.CorreoPublico)
                    ? "Sin correo registrado"
                    : _candidato.CorreoPublico;
            }
        }

        protected string NombreCampana
        {
            get { return _campana != null ? _campana.Nombre : "Sin campaña asociada"; }
        }

        protected string VerificacionTexto
        {
            get { return _candidato.VerificacionTexto; }
        }

        protected string VerificacionClase
        {
            get { return _candidato.VerificacionClase; }
        }
    }
}
