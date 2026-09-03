using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Administración de campañas electorales.
    ///
    /// Una campaña no se borra: cuando termina, se cierra. Sus candidaturas y
    /// sus propuestas siguen consultables, que es justamente de lo que se trata
    /// dar seguimiento a lo prometido.
    /// </summary>
    public partial class Campanas : PaginaAdmin
    {
        private IList<CampanaAdmin> _campanas;
        private CampanaAdmin _seleccion;

        private const string ClaveCodigo = "admin.campanas.codigo";
        private const string ClaveAbierto = "admin.campanas.abierto";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) CargarEstados();

            CargarCampanas();
            MostrarForm(!IsPostBack);
        }

        private void CargarEstados()
        {
            // Los tres valores que acepta la restricción CK_Campanas_estado.
            ddlEstado.Items.Add(new ListItem("En curso", "Activa"));
            ddlEstado.Items.Add(new ListItem("Próxima", "Proxima"));
            ddlEstado.Items.Add(new ListItem("Cerrada", "Cerrada"));
        }

        // ------------------------------------------------------- Lista

        private void CargarCampanas()
        {
            _campanas = Contenido.Datos.ObtenerCampanasAdmin(Sesion.CodigoUsuario);

            bool hay = _campanas.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptCampanas.DataSource = _campanas;
                rptCampanas.DataBind();
            }
        }

        protected void rptCampanas_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "editar") return;

            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            Session[ClaveCodigo] = codigo;
            Session[ClaveAbierto] = true;

            Mensaje = null;
            MostrarForm(true);
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            Session[ClaveCodigo] = 0;
            Session[ClaveAbierto] = true;

            Mensaje = null;
            MostrarForm(true);
        }

        // --------------------------------------------------- Formulario

        /// <summary>
        /// Muestra el formulario y, con <paramref name="cargarCampos"/>, lo
        /// llena. En el postback que guarda no se llena: Page_Load corre antes
        /// que el evento del botón y borraría lo que se acaba de escribir.
        /// </summary>
        private void MostrarForm(bool cargarCampos)
        {
            bool abierto = Session[ClaveAbierto] != null && Convert.ToBoolean(Session[ClaveAbierto]);
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            _seleccion = BuscarCampana(codigo);

            if (codigo > 0 && _seleccion == null) abierto = false;

            phForm.Visible = abierto;

            if (!abierto || !cargarCampos) return;

            if (_seleccion == null)
            {
                txtNombre.Text = string.Empty;
                txtResumen.Text = string.Empty;
                txtDescripcion.Text = string.Empty;
                txtAlcance.Text = string.Empty;
                txtInicio.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                txtEleccion.Text = string.Empty;
                chkActual.Checked = false;
                ddlEstado.ClearSelection();
                ddlEstado.Items.FindByValue("Proxima").Selected = true;
                return;
            }

            txtNombre.Text = _seleccion.Nombre;
            txtResumen.Text = _seleccion.Resumen;
            txtDescripcion.Text = _seleccion.Descripcion;
            txtAlcance.Text = _seleccion.Alcance;

            // El control de fecha del navegador espera siempre el formato ISO,
            // sin importar la cultura de la aplicación.
            txtInicio.Text = _seleccion.FechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            txtEleccion.Text = _seleccion.FechaEleccion.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            chkActual.Checked = _seleccion.EsActual;

            ListItem estado = ddlEstado.Items.FindByValue(_seleccion.Estado);
            if (estado != null)
            {
                ddlEstado.ClearSelection();
                estado.Selected = true;
            }
        }

        private CampanaAdmin BuscarCampana(int codigo)
        {
            if (codigo <= 0 || _campanas == null) return null;

            foreach (CampanaAdmin c in _campanas)
            {
                if (c.Codigo == codigo) return c;
            }

            return null;
        }

        private void Cerrar()
        {
            Session.Remove(ClaveCodigo);
            Session.Remove(ClaveAbierto);
            phForm.Visible = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Cerrar();
        }

        // ----------------------------------------------------- Guardado

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            DateTime inicio;
            DateTime eleccion;

            // El input de tipo date entrega ISO. Se valida acá porque el
            // procedimiento espera fechas, no texto.
            if (!DateTime.TryParseExact(txtInicio.Text.Trim(), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out inicio))
            {
                MostrarMensaje("Indicá la fecha de inicio de la campaña.", false);
                return;
            }

            if (!DateTime.TryParseExact(txtEleccion.Text.Trim(), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out eleccion))
            {
                MostrarMensaje("Indicá la fecha de la elección.", false);
                return;
            }

            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            ResultadoGuardado r = Contenido.Datos.GuardarCampana(
                Sesion.CodigoUsuario, codigo,
                txtNombre.Text.Trim(), txtResumen.Text.Trim(), txtDescripcion.Text.Trim(),
                txtAlcance.Text.Trim(), inicio, eleccion,
                ddlEstado.SelectedValue, chkActual.Checked);

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarCampanas();
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

        protected string TotalTexto
        {
            get
            {
                int n = _campanas == null ? 0 : _campanas.Count;
                return Vista.Plural(n, "campaña", "campañas");
            }
        }

        protected string TituloForm
        {
            get { return _seleccion == null ? "Registrar campaña" : "Editar campaña"; }
        }
    }
}
