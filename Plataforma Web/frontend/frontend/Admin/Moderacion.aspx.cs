using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Moderación de publicaciones e iniciativas ciudadanas.
    ///
    /// Retirar es una baja lógica: el contenido sale de la consulta pública y
    /// de la analítica, pero la fila y su participación se conservan. Por eso
    /// las listas muestran también lo retirado — sin poder verlo, un retiro por
    /// error sería irreversible en la práctica.
    ///
    /// El motivo es obligatorio en las dos direcciones y lo exige el
    /// procedimiento, no esta página. Las dos facultades comparten un solo
    /// panel de decisión: lo que cambia es a qué tabla se aplica, y eso lo
    /// recuerda el tipo guardado en la sesión.
    /// </summary>
    public partial class Moderacion : PaginaAdmin
    {
        private const string TipoPublicacion = "pub";
        private const string TipoIniciativa = "inic";

        private const string ClaveTipo = "admin.moder.tipo";
        private const string ClaveCodigo = "admin.moder.codigo";

        private IList<PublicacionModerada> _publicaciones;
        private IList<Iniciativa> _iniciativas;

        // Selección resuelta, común a los dos tipos.
        private string _selTipo;
        private int _selCodigo;
        private bool _selActiva;
        private bool _selHay;
        private string _selTitulo;
        private string _selAutor;
        private string _selParticipacion;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) CargarFiltros();

            // En cada carga, también en los postbacks: sin esto la fila que
            // recibe el clic se queda sin modelo. Estas tarjetas no llevan hilo
            // de comentarios, así que Page_Load alcanza — no hace falta OnInit.
            CargarPublicaciones();
            CargarIniciativas();
            MostrarSeleccion();
        }

        private void CargarFiltros()
        {
            ddlEstado.Items.Add(new ListItem("Todas", string.Empty));
            ddlEstado.Items.Add(new ListItem("Solo activas", "Activas"));
            ddlEstado.Items.Add(new ListItem("Solo retiradas", "Retiradas"));

            ddlEstadoInic.Items.Add(new ListItem("Todas", string.Empty));
            ddlEstadoInic.Items.Add(new ListItem("Solo activas", "Activas"));
            ddlEstadoInic.Items.Add(new ListItem("Solo retiradas", "Retiradas"));
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            LimpiarSeleccion();
            CargarPublicaciones();
            CargarIniciativas();
            MostrarSeleccion();
        }

        // ---------------------------------------------------- Publicaciones

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
            Seleccionar(TipoPublicacion, e);
        }

        // ------------------------------------------------------ Iniciativas

        private void CargarIniciativas()
        {
            _iniciativas = Contenido.Datos.ObtenerIniciativasAdmin(
                Sesion.CodigoUsuario, ddlEstadoInic.SelectedValue);

            bool hay = _iniciativas.Count > 0;
            phListaInic.Visible = hay;
            phVacioInic.Visible = !hay;

            if (hay)
            {
                rptIniciativas.DataSource = _iniciativas;
                rptIniciativas.DataBind();
            }
        }

        protected void rptIniciativas_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Seleccionar(TipoIniciativa, e);
        }

        private void Seleccionar(string tipo, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "moderar") return;

            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            Session[ClaveTipo] = tipo;
            Session[ClaveCodigo] = codigo;

            Mensaje = null;
            MostrarSeleccion();
        }

        // ---------------------------------------------------- Selección

        private void MostrarSeleccion()
        {
            _selHay = false;
            _selTipo = Session[ClaveTipo] as string;

            object guardado = Session[ClaveCodigo];
            if (_selTipo != null && guardado != null)
            {
                _selCodigo = Convert.ToInt32(guardado);

                if (_selTipo == TipoPublicacion) ResolverPublicacion();
                else if (_selTipo == TipoIniciativa) ResolverIniciativa();
            }

            phDecision.Visible = _selHay;

            if (!_selHay)
            {
                LimpiarSeleccion();
                return;
            }

            // El aviso sobre la analítica solo aplica al retirar.
            phAvisoRetiro.Visible = _selActiva;
            btnConfirmar.Text = _selActiva ? "Retirar de la vista pública" : "Restaurar a la vista pública";
        }

        private void ResolverPublicacion()
        {
            if (_publicaciones == null) return;

            foreach (PublicacionModerada p in _publicaciones)
            {
                if (p.CodigoPublicacion != _selCodigo) continue;

                _selHay = true;
                _selActiva = p.Activa;
                _selTitulo = p.Extracto;
                _selAutor = p.Candidato;
                _selParticipacion =
                    Vista.Plural(p.MeGusta, "apoyo", "apoyos")
                    + ", " + Vista.Plural(p.NoMeGusta, "rechazo", "rechazos")
                    + " y " + Vista.Plural(p.Comentarios, "comentario", "comentarios");
                return;
            }
        }

        private void ResolverIniciativa()
        {
            if (_iniciativas == null) return;

            foreach (Iniciativa i in _iniciativas)
            {
                if (i.Id != _selCodigo) continue;

                _selHay = true;
                _selActiva = i.Activa;
                _selTitulo = i.Titulo;
                _selAutor = i.Autora;
                _selParticipacion =
                    Vista.Plural(i.MeGusta, "apoyo", "apoyos")
                    + ", " + Vista.Plural(i.NoMeGusta, "rechazo", "rechazos")
                    + " y " + Vista.Plural(i.Comentarios, "comentario", "comentarios");
                return;
            }
        }

        private void LimpiarSeleccion()
        {
            Session.Remove(ClaveTipo);
            Session.Remove(ClaveCodigo);
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
            if (!_selHay)
            {
                MostrarMensaje("Elegí primero el contenido a moderar.", false);
                return;
            }

            // Se envía el estado contrario al actual: el botón es el mismo para
            // retirar y para restaurar.
            bool nuevoEstado = !_selActiva;
            string motivo = txtMotivo.Text.Trim();

            Resultado r = _selTipo == TipoIniciativa
                ? Contenido.Datos.ModerarIniciativa(Sesion.CodigoUsuario, _selCodigo, nuevoEstado, motivo)
                : Contenido.Datos.ModerarPublicacion(Sesion.CodigoUsuario, _selCodigo, nuevoEstado, motivo);

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            LimpiarSeleccion();
            CargarPublicaciones();
            CargarIniciativas();
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

        protected string TotalTextoInic
        {
            get
            {
                int n = _iniciativas == null ? 0 : _iniciativas.Count;
                return Vista.Plural(n, "iniciativa", "iniciativas");
            }
        }

        protected string TituloDecision
        {
            get
            {
                if (!_selHay) return string.Empty;
                return _selActiva
                    ? "Retirar de la consulta pública"
                    : "Restaurar a la consulta pública";
            }
        }

        protected string SeleccionTexto
        {
            get { return _selHay ? _selTitulo : string.Empty; }
        }

        protected string SeleccionAutor
        {
            get { return _selHay ? _selAutor : string.Empty; }
        }

        protected string SeleccionParticipacion
        {
            get { return _selHay ? _selParticipacion : string.Empty; }
        }
    }
}
