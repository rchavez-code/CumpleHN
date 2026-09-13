using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// La bandeja de solicitudes de espacio: lo que las organizaciones dejan
    /// en la página pública «Para organizaciones». Solo la plataforma.
    ///
    /// «Crear el espacio» no crea nada acá: manda a la página de espacios con
    /// la solicitud precargada, y es esa página la que, al guardar, marca la
    /// solicitud como atendida con el espacio nuevo. Así el alta sigue
    /// teniendo un solo camino.
    /// </summary>
    public partial class SolicitudesPagina : PaginaAdminPlataforma
    {
        private IList<Solicitud> _solicitudes;
        private Solicitud _seleccion;

        private const string ClaveCodigo = "admin.solicitudes.codigo";

        protected void Page_Load(object sender, EventArgs e)
        {
            CargarSolicitudes();
            MostrarPanel(!IsPostBack);
        }

        private void CargarSolicitudes()
        {
            _solicitudes = Contenido.Datos.ObtenerSolicitudes(Sesion.CodigoUsuario, ddlEstado.SelectedValue);

            bool hay = _solicitudes.Count > 0;
            phLista.Visible = hay;
            phVacio.Visible = !hay;

            rptSolicitudes.DataSource = _solicitudes;
            rptSolicitudes.DataBind();
        }

        protected void ddlEstado_Changed(object sender, EventArgs e)
        {
            Cerrar();
            CargarSolicitudes();
        }

        protected void rptSolicitudes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            if (e.CommandName == "crear")
            {
                Response.Redirect("~/Admin/Espacios?solicitud=" + codigo);
                return;
            }

            if (e.CommandName != "descartar") return;

            Session[ClaveCodigo] = codigo;
            Mensaje = null;
            MostrarPanel(true);
        }

        private void MostrarPanel(bool cargarCampos)
        {
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);
            _seleccion = Buscar(codigo);

            phDescartar.Visible = _seleccion != null;

            if (cargarCampos && _seleccion != null) txtNotas.Text = string.Empty;
        }

        private Solicitud Buscar(int codigo)
        {
            if (codigo <= 0 || _solicitudes == null) return null;
            foreach (Solicitud s in _solicitudes)
            {
                if (s.Codigo == codigo) return s;
            }
            return null;
        }

        private void Cerrar()
        {
            Session.Remove(ClaveCodigo);
            phDescartar.Visible = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Cerrar();
        }

        protected void btnDescartar_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero la solicitud.", false);
                return;
            }

            Resultado r = Contenido.Datos.AtenderSolicitud(
                Sesion.CodigoUsuario, _seleccion.Codigo, "Descartada", 0, txtNotas.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarSolicitudes();
        }

        // ------------------------------------------------- Presentación

        protected string Mensaje { get; private set; }
        private bool _mensajeOk;

        private void MostrarMensaje(string texto, bool ok)
        {
            Mensaje = texto;
            _mensajeOk = ok;
            phMensaje.Visible = !string.IsNullOrEmpty(texto);
        }

        protected string ClaseMensaje
        {
            get { return _mensajeOk ? "gc-ok gc-mb" : "gc-alert gc-mb"; }
        }

        protected string NombreSeleccion
        {
            get { return _seleccion == null ? string.Empty : _seleccion.Organizacion; }
        }

        protected string TotalTexto
        {
            get
            {
                int n = _solicitudes == null ? 0 : _solicitudes.Count;
                return Vista.Plural(n, "solicitud", "solicitudes");
            }
        }
    }
}
