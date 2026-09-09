using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Administración de partidos políticos.
    ///
    /// Los partidos no se borran. Eliminar uno dejaría candidaturas sin
    /// afiliación y borraría de la historia a quién se presentó por él, así que
    /// se desactivan — y el procedimiento se niega a desactivar un partido que
    /// todavía tiene candidaturas activas.
    /// </summary>
    public partial class Partidos : PaginaAdmin
    {
        private IList<PartidoAdmin> _partidos;
        private PartidoAdmin _seleccion;

        /// <summary>
        /// Qué está haciendo la página: nada, editando o cambiando el estado.
        /// Vive en la sesión porque los dos paneles se abren desde la lista y
        /// tienen que sobrevivir al postback.
        /// </summary>
        private const string ClaveModo = "admin.partidos.modo";
        private const string ClaveCodigo = "admin.partidos.codigo";

        private const string ModoForm = "form";
        private const string ModoEstado = "estado";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Siempre, también en postback: la fila que recibe el clic necesita
            // su modelo para resolver el CommandArgument.
            CargarPartidos();
            MostrarPaneles(!IsPostBack);
        }

        // ------------------------------------------------------- Lista

        private void CargarPartidos()
        {
            _partidos = Contenido.Datos.ObtenerPartidosAdmin(Sesion.CodigoUsuario, false);

            bool hay = _partidos.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptPartidos.DataSource = _partidos;
                rptPartidos.DataBind();
            }
        }

        protected void rptPartidos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            if (e.CommandName == "editar")
            {
                Session[ClaveModo] = ModoForm;
                Session[ClaveCodigo] = codigo;
            }
            else if (e.CommandName == "estado")
            {
                Session[ClaveModo] = ModoEstado;
                Session[ClaveCodigo] = codigo;
            }
            else return;

            Mensaje = null;

            // Al abrir un panel sí se cargan los campos con lo que hay: es el
            // punto de partida de la edición.
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
        /// Decide qué panel se ve y, cuando <paramref name="cargarCampos"/> es
        /// verdadero, los llena con los datos actuales.
        ///
        /// En un postback de guardado los campos no se tocan: Page_Load corre
        /// antes que el evento del botón, así que rellenarlos acá borraría lo
        /// que la persona acaba de escribir.
        /// </summary>
        private void MostrarPaneles(bool cargarCampos)
        {
            string modo = Convert.ToString(Session[ClaveModo]);
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            _seleccion = BuscarPartido(codigo);

            // Un código que ya no está en la lista deja el panel sin sentido.
            if (codigo > 0 && _seleccion == null) modo = string.Empty;

            phForm.Visible = modo == ModoForm;
            phEstado.Visible = modo == ModoEstado;

            if (!cargarCampos) return;

            if (modo == ModoForm)
            {
                txtNombre.Text = _seleccion == null ? string.Empty : _seleccion.Nombre;
                txtSiglas.Text = _seleccion == null ? string.Empty : _seleccion.Siglas;
                txtDescripcion.Text = _seleccion == null ? string.Empty : _seleccion.Descripcion;
            }

            if (modo == ModoEstado)
            {
                txtMotivo.Text = string.Empty;
                btnConfirmarEstado.Text = _seleccion != null && _seleccion.Activo
                    ? "Desactivar partido"
                    : "Reactivar partido";
            }
        }

        private PartidoAdmin BuscarPartido(int codigo)
        {
            if (codigo <= 0 || _partidos == null) return null;

            foreach (PartidoAdmin p in _partidos)
            {
                if (p.Codigo == codigo) return p;
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

        // ----------------------------------------------------- Guardado

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            ResultadoGuardado r = Contenido.Datos.GuardarPartido(
                Sesion.CodigoUsuario, codigo,
                txtNombre.Text.Trim(), txtSiglas.Text.Trim(), txtDescripcion.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarPartidos();
        }

        protected void btnConfirmarEstado_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero el partido.", false);
                return;
            }

            Resultado r = Contenido.Datos.CambiarEstadoPartido(
                Sesion.CodigoUsuario, _seleccion.Codigo, !_seleccion.Activo, txtMotivo.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarPartidos();
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
                int n = _partidos == null ? 0 : _partidos.Count;
                return Vista.Plural(n, "partido", "partidos");
            }
        }

        protected string TituloForm
        {
            get { return _seleccion == null ? "Registrar partido" : "Editar partido"; }
        }

        protected string TituloEstado
        {
            get
            {
                if (_seleccion == null) return string.Empty;
                return _seleccion.Activo ? "Desactivar partido" : "Reactivar partido";
            }
        }

        protected string TextoEstado
        {
            get
            {
                if (_seleccion == null) return string.Empty;

                if (!_seleccion.Activo)
                    return "El partido " + _seleccion.Nombre + " volverá a estar disponible para afiliar candidaturas.";

                return "El partido " + _seleccion.Nombre + " dejará de estar disponible para nuevas candidaturas. "
                     + "Su ficha pública y las candidaturas ya registradas no se tocan.";
            }
        }
    }
}
