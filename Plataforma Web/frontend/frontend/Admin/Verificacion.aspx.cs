using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Bandeja de verificación de contenido.
    ///
    /// Une los tres tipos que llevan nivel de verificación en una sola cola,
    /// porque quien revisa trabaja por antigüedad y no por tipo.
    ///
    /// El nivel se guarda con el procedimiento spAdminCambiarVerificacion, que
    /// además registra la decisión en la bitácora. Esta página no decide nada:
    /// recoge el nivel y el motivo, y muestra lo que el backend responde.
    /// </summary>
    public partial class Verificacion : PaginaAdmin
    {
        private IList<ItemVerificacion> _bandeja;
        private ItemVerificacion _seleccion;

        private const string ClaveTipoSel = "admin.verif.tipo";
        private const string ClaveCodigoSel = "admin.verif.codigo";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarFiltros();
                CargarNiveles();
            }

            // El repetidor se enlaza en cada carga, también en los postbacks:
            // sin eso la fila que recibe el clic se queda sin datos.
            CargarBandeja();

            // El nivel del desplegable solo se preselecciona en la primera
            // carga. En un postback, Page_Load corre antes que el evento del
            // botón: preseleccionar acá pisaría el nivel que la persona acaba
            // de elegir, y se guardaría siempre el que el contenido ya tenía.
            MostrarSeleccion(!IsPostBack);
        }

        // ------------------------------------------------------ Filtros

        private void CargarFiltros()
        {
            ddlTipo.Items.Add(new ListItem("Todos los tipos", string.Empty));
            ddlTipo.Items.Add(new ListItem("Candidaturas", TiposObjeto.Candidato));
            ddlTipo.Items.Add(new ListItem("Propuestas", TiposObjeto.Propuesta));
            ddlTipo.Items.Add(new ListItem("Publicaciones", TiposObjeto.Publicacion));

            ddlPendientes.Items.Add(new ListItem("Solo lo pendiente", "1"));
            ddlPendientes.Items.Add(new ListItem("Todo el contenido", "0"));
        }

        private void CargarNiveles()
        {
            foreach (OpcionCatalogo nivel in Contenido.Datos.ObtenerNivelesVerificacion())
            {
                ddlNivel.Items.Add(new ListItem(nivel.Nombre, nivel.Codigo.ToString()));
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            // Cambiar el filtro descarta la selección: el contenido elegido
            // puede quedar fuera de la lista que se está mirando.
            LimpiarSeleccion();
            CargarBandeja();
            MostrarSeleccion(true);
        }

        private bool SoloPendientes
        {
            get { return ddlPendientes.SelectedValue != "0"; }
        }

        // ------------------------------------------------------ Bandeja

        private void CargarBandeja()
        {
            _bandeja = Contenido.Datos.ObtenerBandejaVerificacion(
                Sesion.CodigoUsuario, ddlTipo.SelectedValue, string.Empty, SoloPendientes);

            bool hay = _bandeja.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptBandeja.DataSource = _bandeja;
                rptBandeja.DataBind();
            }
        }

        protected void rptBandeja_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "revisar") return;

            string[] partes = Convert.ToString(e.CommandArgument).Split('|');
            if (partes.Length != 2) return;

            int codigo;
            if (!int.TryParse(partes[1], out codigo)) return;

            Session[ClaveTipoSel] = partes[0];
            Session[ClaveCodigoSel] = codigo;

            Mensaje = null;

            // Al elegir otro contenido sí se preselecciona su nivel actual: es
            // el punto de partida de la decisión.
            MostrarSeleccion(true);
        }

        // ---------------------------------------------------- Selección

        /// <summary>
        /// Busca en la bandeja ya cargada el contenido seleccionado. Se resuelve
        /// contra la lista y no contra la sesión, para que el panel muestre
        /// siempre el nivel que tiene el contenido ahora y no el que tenía
        /// cuando se hizo clic.
        /// </summary>
        private void MostrarSeleccion(bool preseleccionarNivel)
        {
            _seleccion = null;

            string tipo = Convert.ToString(Session[ClaveTipoSel]);
            object codigoGuardado = Session[ClaveCodigoSel];

            if (!string.IsNullOrEmpty(tipo) && codigoGuardado != null && _bandeja != null)
            {
                int codigo = Convert.ToInt32(codigoGuardado);

                foreach (ItemVerificacion item in _bandeja)
                {
                    if (item.TipoObjeto == tipo && item.CodigoObjeto == codigo)
                    {
                        _seleccion = item;
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

            if (!preseleccionarNivel) return;

            ListItem actual = ddlNivel.Items.FindByValue(_seleccion.CodigoVerificacion.ToString());
            if (actual != null)
            {
                ddlNivel.ClearSelection();
                actual.Selected = true;
            }
        }

        private void LimpiarSeleccion()
        {
            Session.Remove(ClaveTipoSel);
            Session.Remove(ClaveCodigoSel);
            phDecision.Visible = false;
            txtMotivo.Text = string.Empty;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarSeleccion();
        }

        // ----------------------------------------------------- Guardado

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero el contenido que vas a revisar.", false);
                return;
            }

            int nivel;
            if (!int.TryParse(ddlNivel.SelectedValue, out nivel))
            {
                MostrarMensaje("Elegí el nivel de verificación.", false);
                return;
            }

            Resultado r = Contenido.Datos.CambiarVerificacion(
                Sesion.CodigoUsuario, _seleccion.TipoObjeto, _seleccion.CodigoObjeto,
                nivel, txtMotivo.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            LimpiarSeleccion();

            // La lista se vuelve a pedir: el contenido recién verificado puede
            // tener que salir de la bandeja de pendientes.
            CargarBandeja();
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
                int n = _bandeja == null ? 0 : _bandeja.Count;
                return Vista.Plural(n, "registro", "registros");
            }
        }

        protected string TituloVacio
        {
            get
            {
                return SoloPendientes
                    ? "No hay contenido pendiente de revisión"
                    : "No hay contenido registrado";
            }
        }

        protected string TextoVacio
        {
            get
            {
                return SoloPendientes
                    ? "Todo lo registrado en la plataforma ya tiene su nivel de verificación asignado. Cambiá el filtro para revisar también lo verificado."
                    : "Todavía no hay candidaturas, propuestas ni publicaciones que revisar.";
            }
        }

        protected string SeleccionTitulo
        {
            get { return _seleccion == null ? string.Empty : _seleccion.Titulo; }
        }

        protected string SeleccionResumen
        {
            get { return _seleccion == null ? string.Empty : _seleccion.Resumen; }
        }

        protected string SeleccionTipo
        {
            get { return _seleccion == null ? string.Empty : _seleccion.TipoTexto; }
        }

        protected string SeleccionCandidato
        {
            get { return _seleccion == null ? string.Empty : _seleccion.Candidato; }
        }

        protected string SeleccionVerificacion
        {
            get { return _seleccion == null ? string.Empty : _seleccion.Verificacion; }
        }

        protected string SeleccionVerificacionClase
        {
            get { return _seleccion == null ? string.Empty : _seleccion.VerificacionClase; }
        }

        protected string SeleccionUrl
        {
            get { return _seleccion == null ? "#" : ResolveUrl(_seleccion.Url); }
        }
    }
}
