using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Administración de candidaturas y de sus cuentas de acceso.
    ///
    /// La plataforma registra los datos de identificación — quién es, a qué
    /// aspira, por qué partido. El perfil, las propuestas y las publicaciones
    /// los llena la propia candidatura desde su panel: la plataforma la
    /// registra, no la redacta.
    /// </summary>
    public partial class Candidatos : PaginaAdmin
    {
        private IList<CandidatoAdmin> _candidatos;
        private CandidatoAdmin _seleccion;

        private const string ClaveModo = "admin.candidatos.modo";
        private const string ClaveCodigo = "admin.candidatos.codigo";

        private const string ModoForm = "form";
        private const string ModoCuenta = "cuenta";
        private const string ModoEstado = "estado";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) CargarCatalogos();

            CargarCandidatos();
            MostrarPaneles(!IsPostBack);
        }

        // ---------------------------------------------------- Catálogos

        private void CargarCatalogos()
        {
            IList<CampanaAdmin> campanas = Contenido.Datos.ObtenerCampanasAdmin(Sesion.CodigoUsuario);

            ddlFiltroCampana.Items.Add(new ListItem("Todas las campañas", string.Empty));

            foreach (CampanaAdmin c in campanas)
            {
                ddlCampana.Items.Add(new ListItem(c.Nombre, c.Codigo.ToString()));
                ddlFiltroCampana.Items.Add(new ListItem(c.Nombre, c.Slug));
            }

            foreach (OpcionCatalogo cargo in Contenido.Datos.ObtenerCargosConCodigo())
            {
                ddlCargo.Items.Add(new ListItem(cargo.Nombre, cargo.Codigo.ToString()));
            }

            // Solo se ofrecen partidos activos: uno desactivado dejó de admitir
            // candidaturas nuevas, que es lo único que significa desactivarlo.
            ddlPartido.Items.Add(new ListItem("Independiente", "0"));

            foreach (PartidoAdmin p in Contenido.Datos.ObtenerPartidosAdmin(Sesion.CodigoUsuario, true))
            {
                ddlPartido.Items.Add(new ListItem(p.Nombre, p.Codigo.ToString()));
            }

            ddlDepartamento.Items.Add(new ListItem("Sin departamento", "0"));

            foreach (OpcionCatalogo d in Contenido.Datos.ObtenerDepartamentosConCodigo())
            {
                ddlDepartamento.Items.Add(new ListItem(d.Nombre, d.Codigo.ToString()));
            }
        }

        // ------------------------------------------------------- Lista

        private void CargarCandidatos()
        {
            _candidatos = Contenido.Datos.ObtenerCandidatosAdmin(
                Sesion.CodigoUsuario, ddlFiltroCampana.SelectedValue, false);

            bool hay = _candidatos.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptCandidatos.DataSource = _candidatos;
                rptCandidatos.DataBind();
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            Cerrar();
            CargarCandidatos();
        }

        protected void rptCandidatos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            if (e.CommandName == "editar") Session[ClaveModo] = ModoForm;
            else if (e.CommandName == "cuenta") Session[ClaveModo] = ModoCuenta;
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

        // ------------------------------------------------------ Paneles

        /// <summary>
        /// Abre el panel que corresponda y, con <paramref name="cargarCampos"/>,
        /// lo llena con lo que hay.
        ///
        /// En el postback que guarda no se llena: Page_Load corre antes que el
        /// evento del botón y borraría lo que se acaba de escribir.
        /// </summary>
        private void MostrarPaneles(bool cargarCampos)
        {
            string modo = Convert.ToString(Session[ClaveModo]);
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            _seleccion = BuscarCandidato(codigo);

            if (codigo > 0 && _seleccion == null) modo = string.Empty;

            phForm.Visible = modo == ModoForm;
            phCuenta.Visible = modo == ModoCuenta;
            phEstado.Visible = modo == ModoEstado;

            if (!cargarCampos) return;

            if (modo == ModoForm) CargarForm();
            if (modo == ModoCuenta) CargarCuenta();

            if (modo == ModoEstado)
            {
                txtMotivo.Text = string.Empty;
                btnConfirmarEstado.Text = _seleccion != null && _seleccion.Activo
                    ? "Retirar candidatura"
                    : "Reincorporar candidatura";
            }
        }

        private void CargarForm()
        {
            if (_seleccion == null)
            {
                txtNombres.Text = string.Empty;
                txtApellidos.Text = string.Empty;
                txtMunicipio.Text = string.Empty;
                txtTitular.Text = string.Empty;
                Seleccionar(ddlPartido, "0");
                Seleccionar(ddlDepartamento, "0");
                return;
            }

            txtNombres.Text = _seleccion.Nombres;
            txtApellidos.Text = _seleccion.Apellidos;
            txtMunicipio.Text = _seleccion.Municipio;
            txtTitular.Text = _seleccion.Titular;

            Seleccionar(ddlCampana, _seleccion.CodigoCampana.ToString());
            Seleccionar(ddlCargo, _seleccion.CodigoCargo.ToString());
            Seleccionar(ddlPartido, _seleccion.CodigoPartido.ToString());
            Seleccionar(ddlDepartamento, _seleccion.CodigoDepartamento.ToString());
        }

        private void CargarCuenta()
        {
            txtClave.Text = string.Empty;

            if (_seleccion == null)
            {
                txtLogin.Text = string.Empty;
                txtCorreo.Text = string.Empty;
                return;
            }

            // Se propone un usuario a partir del nombre, que es la convención
            // de las cuentas ya existentes: inicial del nombre más el apellido.
            txtLogin.Text = LoginSugerido(_seleccion);
            txtCorreo.Text = string.Empty;
        }

        private static string LoginSugerido(CandidatoAdmin c)
        {
            if (string.IsNullOrEmpty(c.Nombres) || string.IsNullOrEmpty(c.Apellidos))
                return string.Empty;

            string[] apellidos = c.Apellidos.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (apellidos.Length == 0) return string.Empty;

            return (c.Nombres.Substring(0, 1) + apellidos[0]).ToLowerInvariant();
        }

        private static void Seleccionar(DropDownList lista, string valor)
        {
            ListItem item = lista.Items.FindByValue(valor);
            if (item == null) return;

            lista.ClearSelection();
            item.Selected = true;
        }

        private CandidatoAdmin BuscarCandidato(int codigo)
        {
            if (codigo <= 0 || _candidatos == null) return null;

            foreach (CandidatoAdmin c in _candidatos)
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
            phCuenta.Visible = false;
            phEstado.Visible = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Cerrar();
        }

        // ----------------------------------------------------- Acciones

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            ResultadoGuardado r = Contenido.Datos.GuardarCandidato(
                Sesion.CodigoUsuario, codigo,
                txtNombres.Text.Trim(), txtApellidos.Text.Trim(),
                Entero(ddlCampana.SelectedValue), Entero(ddlCargo.SelectedValue),
                Entero(ddlPartido.SelectedValue), Entero(ddlDepartamento.SelectedValue),
                txtMunicipio.Text.Trim(), txtTitular.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarCandidatos();
        }

        protected void btnCrearCuenta_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero la candidatura.", false);
                return;
            }

            Resultado r = Contenido.Datos.CrearCuentaCandidato(
                Sesion.CodigoUsuario, _seleccion.Codigo,
                txtLogin.Text.Trim(), txtCorreo.Text.Trim(), txtClave.Text);

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarCandidatos();
        }

        protected void btnConfirmarEstado_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero la candidatura.", false);
                return;
            }

            Resultado r = Contenido.Datos.CambiarEstadoCandidato(
                Sesion.CodigoUsuario, _seleccion.Codigo, !_seleccion.Activo, txtMotivo.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarCandidatos();
        }

        private static int Entero(string valor)
        {
            int n;
            return int.TryParse(valor, out n) ? n : 0;
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
                int n = _candidatos == null ? 0 : _candidatos.Count;
                return Vista.Plural(n, "candidatura", "candidaturas");
            }
        }

        protected string TituloForm
        {
            get { return _seleccion == null ? "Registrar candidatura" : "Editar candidatura"; }
        }

        protected string SeleccionNombre
        {
            get { return _seleccion == null ? string.Empty : _seleccion.NombreCompleto; }
        }

        protected string TituloEstado
        {
            get
            {
                if (_seleccion == null) return string.Empty;
                return _seleccion.Activo ? "Retirar candidatura" : "Reincorporar candidatura";
            }
        }

        protected string TextoEstado
        {
            get
            {
                if (_seleccion == null) return string.Empty;

                if (!_seleccion.Activo)
                    return _seleccion.NombreCompleto + " volverá a la consulta pública, junto con su cuenta de acceso.";

                return _seleccion.NombreCompleto + " dejará de aparecer en la consulta pública y su cuenta de "
                     + "acceso quedará desactivada. Sus propuestas y publicaciones no se borran.";
            }
        }
    }
}
