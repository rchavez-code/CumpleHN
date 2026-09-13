using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Administración de espacios: los clientes que usan la plataforma para
    /// su propio proceso electoral.
    ///
    /// Solo para la cuenta de la plataforma (<see cref="PaginaAdminPlataforma"/>):
    /// un cliente no puede crear otros clientes. Desde acá se registra el
    /// espacio, se le crea la cuenta con la que lo administra, y se puede
    /// «entrar» a administrarlo con la cuenta de la plataforma, que cambia el
    /// espacio de la sesión y lleva al resumen.
    ///
    /// Los espacios no se borran. Retirar uno lo saca de toda consulta
    /// pública y desactiva sus cuentas, y se puede restaurar. Lo demás sigue
    /// el patrón de Partidos: lista, panel de formulario, panel de estado.
    /// </summary>
    public partial class EspaciosPagina : PaginaAdminPlataforma
    {
        private IList<Espacio> _espacios;
        private Espacio _seleccion;

        private const string ClaveModo = "admin.espacios.modo";
        private const string ClaveCodigo = "admin.espacios.codigo";

        private const string ModoForm = "form";
        private const string ModoEstado = "estado";
        private const string ModoCuenta = "cuenta";
        private const string ModoPagos = "pagos";

        protected void Page_Load(object sender, EventArgs e)
        {
            CargarEspacios();
            MostrarPaneles(!IsPostBack);
        }

        // ------------------------------------------------------- Lista

        private void CargarEspacios()
        {
            _espacios = Contenido.Datos.ObtenerEspacios(Sesion.CodigoUsuario, false);

            bool hay = _espacios.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptEspacios.DataSource = _espacios;
                rptEspacios.DataBind();
            }
        }

        protected void rptEspacios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            if (e.CommandName == "administrar")
            {
                Espacio elegido = BuscarEspacio(codigo);
                if (elegido == null) return;

                // La cuenta de la plataforma pasa a mirar ese espacio. Todo lo
                // de Admin/ se acota por Sesion.CodigoEspacio desde ahora.
                Sesion.CambiarEspacio(elegido.Codigo, elegido.Nombre);
                Response.Redirect("~/Admin/");
                return;
            }

            if (e.CommandName == "editar") Session[ClaveModo] = ModoForm;
            else if (e.CommandName == "estado") Session[ClaveModo] = ModoEstado;
            else if (e.CommandName == "cuenta") Session[ClaveModo] = ModoCuenta;
            else if (e.CommandName == "pagos") Session[ClaveModo] = ModoPagos;
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
        /// Igual que en Partidos: los campos se cargan solo al abrir el
        /// panel, nunca en el postback que guarda, porque Page_Load corre
        /// antes que el evento del botón.
        /// </summary>
        private void MostrarPaneles(bool cargarCampos)
        {
            string modo = Convert.ToString(Session[ClaveModo]);
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            _seleccion = BuscarEspacio(codigo);

            if (codigo > 0 && _seleccion == null) modo = string.Empty;

            // La plataforma no se edita ni se retira desde acá, y su cuenta
            // ya existe. Si por la ruta llega igual, no se abre nada.
            if (_seleccion != null && _seleccion.EsPlataforma) modo = string.Empty;

            phForm.Visible = modo == ModoForm;
            phEstado.Visible = modo == ModoEstado;
            phCuenta.Visible = modo == ModoCuenta;
            phPagos.Visible = modo == ModoPagos;

            // El historial de pagos se enlaza siempre que el panel esté abierto,
            // también en el postback que registra uno: es lo que muestra el
            // recién registrado.
            if (modo == ModoPagos) CargarPagos();

            if (!cargarCampos) return;

            if (modo == ModoForm)
            {
                txtNombre.Text = _seleccion == null ? string.Empty : _seleccion.Nombre;
                txtOrganizacion.Text = _seleccion == null ? string.Empty : _seleccion.Organizacion;
                txtDescripcion.Text = _seleccion == null ? string.Empty : _seleccion.Descripcion;
                txtTermino.Text = _seleccion == null ? "Planilla" : _seleccion.TerminoAgrupacion;
                chkPadron.Checked = _seleccion == null || _seleccion.PadronCerrado;
            }

            if (modo == ModoEstado)
            {
                txtMotivo.Text = string.Empty;
                btnConfirmarEstado.Text = _seleccion != null && _seleccion.Activo
                    ? "Retirar espacio"
                    : "Restaurar espacio";
            }

            if (modo == ModoCuenta)
            {
                txtLogin.Text = string.Empty;
                txtNombreCuenta.Text = string.Empty;
                txtCorreo.Text = string.Empty;
            }

            if (modo == ModoPagos)
            {
                // Un proceso electoral que empieza hoy es el caso más común, y
                // el fin queda en blanco a propósito: lo dice el recibo.
                txtPlan.Text = "Proceso electoral";
                txtReferencia.Text = string.Empty;
                txtDesde.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtHasta.Text = string.Empty;
                txtMonto.Text = string.Empty;
                txtMoneda.Text = "HNL";
                txtNotas.Text = string.Empty;
            }
        }

        private Espacio BuscarEspacio(int codigo)
        {
            if (codigo <= 0 || _espacios == null) return null;

            foreach (Espacio x in _espacios)
            {
                if (x.Codigo == codigo) return x;
            }

            return null;
        }

        private void Cerrar()
        {
            Session.Remove(ClaveModo);
            Session.Remove(ClaveCodigo);
            phForm.Visible = false;
            phEstado.Visible = false;
            phCuenta.Visible = false;
            phPagos.Visible = false;
        }

        private void CargarPagos()
        {
            if (_seleccion == null) return;

            IList<Suscripcion> pagos = Contenido.Datos.ObtenerSuscripciones(Sesion.CodigoUsuario, _seleccion.Codigo);

            phPagosLista.Visible = pagos.Count > 0;
            phPagosVacio.Visible = pagos.Count == 0;

            rptPagos.DataSource = pagos;
            rptPagos.DataBind();
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Cerrar();
        }

        // ----------------------------------------------------- Guardado

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            ResultadoGuardado r = Contenido.Datos.GuardarEspacio(
                Sesion.CodigoUsuario, codigo,
                txtNombre.Text.Trim(), txtOrganizacion.Text.Trim(), txtDescripcion.Text.Trim(),
                chkPadron.Checked, txtTermino.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarEspacios();
            RecargarSelector();
        }

        protected void btnConfirmarEstado_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero el espacio.", false);
                return;
            }

            Resultado r = Contenido.Datos.CambiarEstadoEspacio(
                Sesion.CodigoUsuario, _seleccion.Codigo, !_seleccion.Activo, txtMotivo.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarEspacios();
            RecargarSelector();
        }

        protected void btnCrearCuenta_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero el espacio.", false);
                return;
            }

            Resultado r = Contenido.Datos.CrearCuentaEspacio(
                Sesion.CodigoUsuario, _seleccion.Codigo,
                txtLogin.Text.Trim(), txtNombreCuenta.Text.Trim(), txtCorreo.Text.Trim(), txtClave.Text);

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarEspacios();
            RecargarSelector();
        }

        /// <summary>La barra lateral lista los espacios: si cambiaron, que lo sepa.</summary>
        private void RecargarSelector()
        {
            AdminMaster m = Master as AdminMaster;
            if (m != null) m.RecargarEspacios();
        }

        protected void btnRegistrarPago_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero el espacio.", false);
                return;
            }

            DateTime desde, hasta;
            decimal monto;

            if (!DateTime.TryParse(txtDesde.Text, out desde) || !DateTime.TryParse(txtHasta.Text, out hasta))
            {
                MostrarMensaje("Indicá desde y hasta cuándo cubre el pago.", false);
                return;
            }

            if (!decimal.TryParse(txtMonto.Text, System.Globalization.NumberStyles.Number,
                                  System.Globalization.CultureInfo.InvariantCulture, out monto))
            {
                MostrarMensaje("El monto no es un número válido.", false);
                return;
            }

            ResultadoGuardado r = Contenido.Datos.RegistrarPago(
                Sesion.CodigoUsuario, _seleccion.Codigo, txtPlan.Text.Trim(), desde, hasta, monto,
                txtMoneda.Text.Trim(), txtReferencia.Text.Trim(), txtNotas.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            // El panel sigue abierto con el pago recién registrado en la lista.
            // La lista de espacios se vuelve a leer para que la vigencia cambie.
            CargarEspacios();
            _seleccion = BuscarEspacio(_seleccion.Codigo);
            CargarPagos();
            txtReferencia.Text = string.Empty;
            txtMonto.Text = string.Empty;
            txtNotas.Text = string.Empty;
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
                int n = _espacios == null ? 0 : _espacios.Count;
                return Vista.Plural(n, "espacio", "espacios");
            }
        }

        protected string TituloForm
        {
            get { return _seleccion == null ? "Registrar espacio" : "Editar espacio"; }
        }

        protected string NombreSeleccion
        {
            get { return _seleccion == null ? string.Empty : _seleccion.Nombre; }
        }

        protected string TituloEstado
        {
            get
            {
                if (_seleccion == null) return string.Empty;
                return _seleccion.Activo ? "Retirar espacio" : "Restaurar espacio";
            }
        }

        protected string TextoEstado
        {
            get
            {
                if (_seleccion == null) return string.Empty;

                if (!_seleccion.Activo)
                    return "El espacio " + _seleccion.Nombre + " volverá a estar en línea y sus cuentas de "
                         + "administración volverán a entrar.";

                return "El espacio " + _seleccion.Nombre + " saldrá de toda consulta pública y sus cuentas de "
                     + "administración quedarán desactivadas. Nada se borra: se puede restaurar.";
            }
        }

        /// <summary>Clase del chip de estado. Los nombres los deriva la base.</summary>
        protected static string EstadoClase(string estado)
        {
            switch (estado)
            {
                case "Plataforma": return "gc-chip gc-chip--declarada";
                case "Activo":     return "gc-chip gc-chip--cumplida";
                case "Vigente":    return "gc-chip gc-chip--cumplida";
                case "Vencido":    return "gc-chip gc-chip--proceso";
                default:           return "gc-chip gc-chip--estancada";
            }
        }
    }
}
