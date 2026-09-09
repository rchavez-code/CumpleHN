using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Bitácora de administración: solo lectura.
    ///
    /// No hay acción para editar ni borrar registros, y no la va a haber. Una
    /// bitácora que su propio responsable puede corregir no sirve para
    /// auditarlo.
    /// </summary>
    public partial class Bitacora : PaginaAdmin
    {
        private IList<RegistroAuditoria> _registros;

        /// <summary>
        /// Tope de la consulta. Con más registros habrá que paginar, pero
        /// paginar sobre una bitácora vacía sería resolver un problema que
        /// todavía no existe.
        /// </summary>
        private const int Limite = 200;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) CargarFiltros();

            CargarBitacora();
        }

        private void CargarFiltros()
        {
            ddlAccion.Items.Add(new ListItem("Todas las acciones", string.Empty));
            ddlAccion.Items.Add(new ListItem("Verificaciones", "Verificacion"));
            ddlAccion.Items.Add(new ListItem("Retiros", "Retiro"));
            ddlAccion.Items.Add(new ListItem("Restauraciones", "Restauracion"));
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarBitacora();
        }

        private void CargarBitacora()
        {
            _registros = Contenido.Datos.ObtenerAuditoria(
                Sesion.CodigoUsuario, ddlAccion.SelectedValue, Limite);

            bool hay = _registros.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptBitacora.DataSource = _registros;
                rptBitacora.DataBind();
            }
        }

        protected string TotalTexto
        {
            get
            {
                int n = _registros == null ? 0 : _registros.Count;
                return Vista.Plural(n, "registro", "registros");
            }
        }
    }
}
