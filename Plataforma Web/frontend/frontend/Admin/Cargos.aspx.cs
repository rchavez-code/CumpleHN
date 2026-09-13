using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Los cargos del espacio administrado: a qué se presenta cada
    /// candidatura. Son de cada espacio (script 23), así que la plataforma
    /// administra los de elección popular y una organización crea los suyos.
    ///
    /// Mismo patrón que Partidos: lista, panel de formulario para alta y
    /// edición, panel de estado con motivo. Un cargo no se borra: se
    /// desactiva y deja de ofrecerse en el alta de candidaturas.
    /// </summary>
    public partial class CargosPagina : PaginaAdmin
    {
        private IList<CargoAdmin> _cargos;
        private CargoAdmin _seleccion;

        private const string ClaveModo = "admin.cargos.modo";
        private const string ClaveCodigo = "admin.cargos.codigo";

        private const string ModoForm = "form";
        private const string ModoEstado = "estado";

        protected void Page_Load(object sender, EventArgs e)
        {
            CargarCargos();
            MostrarPaneles(!IsPostBack);
        }

        private void CargarCargos()
        {
            _cargos = Contenido.Datos.ObtenerCargosAdmin(Sesion.CodigoUsuario);

            bool hay = _cargos.Count > 0;
            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptCargos.DataSource = _cargos;
                rptCargos.DataBind();
            }
        }

        protected void rptCargos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            if (e.CommandName == "editar") Session[ClaveModo] = ModoForm;
            else if (e.CommandName == "estado") Session[ClaveModo] = ModoEstado;
            else return;

            Session[ClaveCodigo] = codigo;
            Mensaje = null;
            MostrarPaneles(true);
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            Session[ClaveModo] = ModoForm;
            Session[ClaveCodigo] = 0;
            Mensaje = null;
            MostrarPaneles(true);
        }

        private void MostrarPaneles(bool cargarCampos)
        {
            string modo = Convert.ToString(Session[ClaveModo]);
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            _seleccion = Buscar(codigo);
            if (codigo > 0 && _seleccion == null) modo = string.Empty;

            phForm.Visible = modo == ModoForm;
            phEstado.Visible = modo == ModoEstado;

            if (!cargarCampos) return;

            if (modo == ModoForm)
            {
                txtNombre.Text = _seleccion == null ? string.Empty : _seleccion.Nombre;
                txtOrden.Text = _seleccion == null ? (_cargos.Count + 1).ToString() : _seleccion.Orden.ToString();

                // Una organización elige cargos institucionales. La plataforma
                // arranca en Nacional, que es lo que sus cargos son.
                string nivel = _seleccion != null ? _seleccion.NivelGobierno
                             : Sesion.AdministraPlataforma && Sesion.CodigoEspacio == PlataformaCodigo ? "Nacional"
                             : "Institucional";
                ListItem item = ddlNivel.Items.FindByValue(nivel);
                ddlNivel.ClearSelection();
                if (item != null) item.Selected = true;
            }

            if (modo == ModoEstado)
            {
                txtMotivo.Text = string.Empty;
                btnConfirmarEstado.Text = _seleccion != null && _seleccion.Activo ? "Desactivar cargo" : "Reactivar cargo";
            }
        }

        /// <summary>El código del espacio de la plataforma, o cero si no se resuelve.</summary>
        private int PlataformaCodigo
        {
            get
            {
                foreach (Espacio x in Contenido.Datos.ObtenerEspacios(Sesion.CodigoUsuario, true))
                {
                    if (x.EsPlataforma) return x.Codigo;
                }
                return 0;
            }
        }

        private CargoAdmin Buscar(int codigo)
        {
            if (codigo <= 0 || _cargos == null) return null;
            foreach (CargoAdmin c in _cargos)
            {
                if (c.Codigo == codigo) return c;
            }
            return null;
        }

        private void Cerrar()
        {
            Session.Remove(ClaveModo);
            Session.Remove(ClaveCodigo);
            phForm.Visible = false;
            phEstado.Visible = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Cerrar();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            int orden;
            int.TryParse(txtOrden.Text, out orden);

            ResultadoGuardado r = Contenido.Datos.GuardarCargo(
                Sesion.CodigoUsuario, codigo, txtNombre.Text.Trim(), ddlNivel.SelectedValue, orden);

            MostrarMensaje(r.Mensaje, r.Ok);
            if (!r.Ok) return;

            Cerrar();
            CargarCargos();
        }

        protected void btnConfirmarEstado_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero el cargo.", false);
                return;
            }

            Resultado r = Contenido.Datos.CambiarEstadoCargo(
                Sesion.CodigoUsuario, _seleccion.Codigo, !_seleccion.Activo, txtMotivo.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);
            if (!r.Ok) return;

            Cerrar();
            CargarCargos();
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
                int n = _cargos == null ? 0 : _cargos.Count;
                return Vista.Plural(n, "cargo", "cargos");
            }
        }

        protected string TituloForm
        {
            get { return _seleccion == null ? "Registrar cargo" : "Editar cargo"; }
        }

        protected string TituloEstado
        {
            get
            {
                if (_seleccion == null) return string.Empty;
                return _seleccion.Activo ? "Desactivar cargo" : "Reactivar cargo";
            }
        }

        protected string TextoEstado
        {
            get
            {
                if (_seleccion == null) return string.Empty;

                if (!_seleccion.Activo)
                    return "El cargo " + _seleccion.Nombre + " volverá a ofrecerse al registrar candidaturas.";

                return "El cargo " + _seleccion.Nombre + " dejará de ofrecerse para candidaturas nuevas. "
                     + (_seleccion.Candidaturas > 0
                        ? "Las " + _seleccion.Candidaturas + " candidaturas que ya lo tienen no cambian."
                        : "No tiene candidaturas registradas.");
            }
        }
    }
}
