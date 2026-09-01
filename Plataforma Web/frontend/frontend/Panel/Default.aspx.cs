using System;
using System.Collections.Generic;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Panel
{
    /// <summary>
    /// Resumen del panel del candidato. Responde de un vistazo qué falta por
    /// completar y qué se ha registrado hasta el momento.
    /// </summary>
    public partial class PanelDefault : Page
    {
        private Candidato _candidato;
        private Campana _campana;
        private IList<Propuesta> _propuestas;
        private IList<Publicacion> _publicaciones;

        protected void Page_Load(object sender, EventArgs e)
        {
            _candidato = Contenido.Datos.ObtenerCandidatoAutenticado();

            if (_candidato == null)
            {
                Response.Redirect("~/Acceso");
                return;
            }

            _campana = Contenido.Datos.ObtenerCampana(_candidato.CampanaSlug);
            _propuestas = Contenido.Datos.ObtenerPropuestas(_candidato.Slug);
            _publicaciones = Contenido.Datos.ObtenerPublicaciones(_candidato.Slug);

            CargarProyectos();
        }

        private void CargarProyectos()
        {
            // En el resumen se listan solo los más recientes.
            List<Propuesta> recientes = new List<Propuesta>();
            for (int i = 0; i < _propuestas.Count && i < 5; i++)
            {
                recientes.Add(_propuestas[i]);
            }

            bool hay = recientes.Count > 0;

            phProyectos.Visible = hay;
            phProyectosVacio.Visible = !hay;

            if (hay)
            {
                rptProyectos.DataSource = recientes;
                rptProyectos.DataBind();
            }
        }

        // ------------------------------------------------ Estado del perfil

        protected int PerfilCompleto
        {
            get { return _candidato.PerfilCompleto; }
        }

        protected string MensajePerfil
        {
            get
            {
                int p = PerfilCompleto;

                if (p >= 100) return "Tu perfil está completo. Mantenelo al día conforme avance la campaña.";
                if (p >= 70) return "Falta poco. Los campos pendientes son los que la ciudadanía consulta más.";
                if (p >= 40) return "Vas a mitad de camino. Un perfil completo se consulta bastante más.";
                return "Completá tu biografía, tu información profesional y tu fotografía para que tu perfil se entienda.";
            }
        }

        // -------------------------------------------------------- Cifras

        protected string TotalProyectos
        {
            get { return Vista.Numero(_propuestas.Count); }
        }

        protected string TotalPublicaciones
        {
            get { return Vista.Numero(_publicaciones.Count); }
        }

        protected string TotalApoyos
        {
            get
            {
                int suma = 0;
                foreach (Publicacion p in _publicaciones) suma += p.MeGusta;
                return Vista.Numero(suma);
            }
        }

        protected string TotalComentarios
        {
            get
            {
                int suma = 0;
                foreach (Publicacion p in _publicaciones) suma += p.Comentarios;
                return Vista.Numero(suma);
            }
        }

        // -------------------------------------------------- Presentación

        protected string VerificacionTexto
        {
            get { return _candidato.VerificacionTexto; }
        }

        protected string VerificacionClase
        {
            get { return _candidato.VerificacionClase; }
        }

        protected string Cargo
        {
            get { return _candidato.Cargo; }
        }

        protected string NombreCampana
        {
            get { return _campana != null ? _campana.Nombre : "Sin campaña asociada"; }
        }

        protected string FechaEleccion
        {
            get { return _campana != null ? Vista.FechaCorta(_campana.FechaEleccion) : "—"; }
        }

        protected string CuentaRegresiva
        {
            get { return _campana != null ? Vista.CuentaRegresiva(_campana.FechaEleccion) : "—"; }
        }

        protected string UrlPerfilPublico
        {
            get { return ResolveUrl(_candidato.Url); }
        }
    }
}
