using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Moderación de publicaciones.
    ///
    /// Retirar es una baja lógica: la publicación sale de la consulta pública
    /// y de la analítica, pero la fila y su participación se conservan. Por eso
    /// la lista muestra también las retiradas — sin poder verlas, un retiro por
    /// error sería irreversible en la práctica.
    ///
    /// El motivo es obligatorio en las dos direcciones y lo exige el
    /// procedimiento, no esta página.
    /// </summary>
    public partial class Moderacion : PaginaAdmin
    {
        private IList<PublicacionModerada> _publicaciones;
        private PublicacionModerada _seleccion;

        private const string ClaveSeleccion = "admin.moder.codigo";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) CargarFiltros();

            // En cada carga, también en los postbacks: sin esto la fila que
            // recibe el clic se queda sin modelo.
            CargarPublicaciones();
            MostrarSeleccion();
        }

        private void CargarFiltros()
        {
            ddlEstado.Items.Add(new ListItem("Todas", string.Empty));
            ddlEstado.Items.Add(new ListItem("Solo activas", "Activas"));
            ddlEstado.Items.Add(new ListItem("Solo retiradas", "Retiradas"));
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            LimpiarSeleccion();
            CargarPublicaciones();
            MostrarSeleccion();
        }

        // ------------------------------------------------------- Lista

        private void CargarPublicaciones()
        {
            _publicaciones = Contenido.Datos.ObtenerPublicacionesModeracion(
                Sesion.CodigoUsuario, string.Empty, ddlEstado.SelectedValue);

            bool hay = _publicaciones.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptPublicaciones.DataSource = _publicaciones;
                rptPublicaciones.DataBind();
            }
        }

        protected void rptPublicaciones_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "moderar") return;

            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            Session[ClaveSeleccion] = codigo;

            Mensaje = null;
            MostrarSeleccion();
        }

        // ---------------------------------------------------- Selección

        private void MostrarSeleccion()
        {
            _seleccion = null;

            object guardado = Session[ClaveSeleccion];

            if (guardado != null && _publicaciones != null)
            {
                int codigo = Convert.ToInt32(guardado);

                foreach (PublicacionModerada p in _publicaciones)
                {
                    if (p.CodigoPublicacion == codigo)
                    {
                        _seleccion = p;
                        break;
                    }
                }
            }

            phDecision.Visible = _seleccion != null;

            if (_seleccion == null)
            {
                LimpiarSeleccion();
                return;
            }

            // El aviso sobre la analítica solo aplica al retirar.
            phAvisoRetiro.Visible = _seleccion.Activa;
            btnConfirmar.Text = _seleccion.Activa ? "Retirar publicación" : "Restaurar publicación";
        }

        private void LimpiarSeleccion()
        {
            Session.Remove(ClaveSeleccion);
            phDecision.Visible = false;
            txtMotivo.Text = string.Empty;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarSeleccion();
        }

        // ----------------------------------------------------- Decisión

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero la publicación.", false);
                return;
            }

            // Se envía el estado contrario al actual: el botón es el mismo para
            // retirar y para restaurar.
            bool nuevoEstado = !_seleccion.Activa;

            Resultado r = Contenido.Datos.ModerarPublicacion(
                Sesion.CodigoUsuario, _seleccion.CodigoPublicacion, nuevoEstado, txtMotivo.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            LimpiarSeleccion();
            CargarPublicaciones();
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

        protected string EstadoTexto(object activa)
        {
            return Convert.ToBoolean(activa) ? "Visible" : "Retirada";
        }

        protected string EstadoClase(object activa)
        {
            return Convert.ToBoolean(activa)
                ? "gc-chip gc-chip--cumplida"
                : "gc-chip gc-chip--incumplida";
        }

        protected string TotalTexto
        {
            get
            {
                int n = _publicaciones == null ? 0 : _publicaciones.Count;
                return Vista.Plural(n, "publicación", "publicaciones");
            }
        }

        protected string TituloDecision
        {
            get
            {
                if (_seleccion == null) return string.Empty;
                return _seleccion.Activa
                    ? "Retirar de la consulta pública"
                    : "Restaurar a la consulta pública";
            }
        }

        protected string SeleccionTexto
        {
            get { return _seleccion == null ? string.Empty : _seleccion.Extracto; }
        }

        protected string SeleccionCandidato
        {
            get { return _seleccion == null ? string.Empty : _seleccion.Candidato; }
        }

        protected string SeleccionParticipacion
        {
            get
            {
                if (_seleccion == null) return string.Empty;

                return Vista.Plural(_seleccion.MeGusta, "apoyo", "apoyos")
                     + ", " + Vista.Plural(_seleccion.NoMeGusta, "rechazo", "rechazos")
                     + " y " + Vista.Plural(_seleccion.Comentarios, "comentario", "comentarios");
            }
        }
    }
}
