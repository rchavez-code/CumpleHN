using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Configuración de qué partes del sitio ve el público.
    ///
    /// Ocultar no borra ni desactiva nada: el contenido sigue en la base y la
    /// administración lo sigue viendo. Es la diferencia entre retirar algo de
    /// la vista y darlo de baja.
    /// </summary>
    public partial class ModulosPagina : PaginaAdmin
    {
        private IList<ModuloAdmin> _modulos;
        private ModuloAdmin _seleccion;

        private const string ClaveSeleccion = "admin.modulos.clave";

        protected void Page_Load(object sender, EventArgs e)
        {
            CargarModulos();
            MostrarSeleccion();
        }

        // ------------------------------------------------------- Lista

        private void CargarModulos()
        {
            _modulos = Contenido.Datos.ObtenerModulosAdmin(Sesion.CodigoUsuario);

            rptGrupos.DataSource = AgruparPorSeccion();
            rptGrupos.DataBind();
        }

        /// <summary>
        /// Arma los grupos conservando el orden en que llegan de la base. Un
        /// diccionario los reordenaría, y el orden dice de qué depende qué: el
        /// tablero antes que sus gráficos.
        /// </summary>
        private IList<GrupoModulos> AgruparPorSeccion()
        {
            List<GrupoModulos> grupos = new List<GrupoModulos>();

            foreach (ModuloAdmin m in _modulos)
            {
                GrupoModulos grupo = null;

                foreach (GrupoModulos g in grupos)
                {
                    if (g.Nombre == m.Grupo) { grupo = g; break; }
                }

                if (grupo == null)
                {
                    grupo = new GrupoModulos { Nombre = m.Grupo };
                    grupos.Add(grupo);
                }

                grupo.Elementos.Add(m);
            }

            return grupos;
        }

        protected void rptGrupos_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem) return;

            GrupoModulos grupo = (GrupoModulos)e.Item.DataItem;

            Repeater interno = (Repeater)e.Item.FindControl("rptModulos");
            interno.DataSource = grupo.Elementos;
            interno.DataBind();
        }

        protected void rptModulos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "cambiar") return;

            string clave = Convert.ToString(e.CommandArgument);
            if (string.IsNullOrEmpty(clave)) return;

            Session[ClaveSeleccion] = clave;

            Mensaje = null;
            MostrarSeleccion();
        }

        // ---------------------------------------------------- Selección

        private void MostrarSeleccion()
        {
            _seleccion = null;

            string clave = Convert.ToString(Session[ClaveSeleccion]);

            if (!string.IsNullOrEmpty(clave) && _modulos != null)
            {
                foreach (ModuloAdmin m in _modulos)
                {
                    if (m.Clave == clave) { _seleccion = m; break; }
                }
            }

            phDecision.Visible = _seleccion != null;

            if (_seleccion == null)
            {
                Cerrar();
                return;
            }

            // El motivo solo se pide para ocultar. Volver a mostrar algo es
            // regresar al estado normal, y eso no necesita justificación.
            phMotivo.Visible = _seleccion.Habilitado;
            btnConfirmar.Text = _seleccion.Habilitado ? "Ocultar del sitio" : "Volver a mostrar";
        }

        private void Cerrar()
        {
            Session.Remove(ClaveSeleccion);
            phDecision.Visible = false;
            txtMotivo.Text = string.Empty;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Cerrar();
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero el elemento.", false);
                return;
            }

            Resultado r = Contenido.Datos.CambiarEstadoModulo(
                Sesion.CodigoUsuario, _seleccion.Clave, !_seleccion.Habilitado, txtMotivo.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarModulos();
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
                if (_modulos == null) return string.Empty;

                int ocultos = 0;
                foreach (ModuloAdmin m in _modulos)
                {
                    if (!m.Visible) ocultos++;
                }

                if (ocultos == 0) return "Todo visible";

                return Vista.Plural(ocultos, "elemento oculto", "elementos ocultos");
            }
        }

        protected string TituloDecision
        {
            get
            {
                if (_seleccion == null) return string.Empty;
                return _seleccion.Habilitado ? "Ocultar del sitio público" : "Volver a mostrar";
            }
        }

        protected string TextoDecision
        {
            get
            {
                if (_seleccion == null) return string.Empty;

                if (!_seleccion.Habilitado)
                    return _seleccion.Nombre + " volverá a estar a la vista de quien consulta el sitio.";

                string texto = _seleccion.Nombre + " dejará de verse en el sitio público. "
                             + "Vos lo seguís viendo, con un aviso de que está oculto. No se borra nada.";

                // Apagar un padre apaga a sus hijos, y conviene decirlo antes y
                // no que se descubra después.
                if (!_seleccion.EsHijo)
                {
                    int hijos = 0;
                    foreach (ModuloAdmin m in _modulos)
                    {
                        if (m.ClavePadre == _seleccion.Clave) hijos++;
                    }

                    if (hijos > 0)
                        texto += " Se ocultan también sus " + hijos + " elementos, aunque tengan su propio interruptor encendido.";
                }

                return texto;
            }
        }
    }

    /// <summary>
    /// Grupo de elementos para el repetidor externo. Existe solo para dibujar
    /// la pantalla, así que vive junto a ella y no en Modelos.
    /// </summary>
    public class GrupoModulos
    {
        public GrupoModulos()
        {
            Elementos = new List<ModuloAdmin>();
        }

        public string Nombre { get; set; }
        public IList<ModuloAdmin> Elementos { get; private set; }
    }
}
