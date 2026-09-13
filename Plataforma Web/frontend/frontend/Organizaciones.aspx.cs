using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// La oferta para organizaciones: qué es un espacio, qué incluye, los
    /// planes con su precio y el formulario de solicitud.
    ///
    /// Los planes salen de la base (spPlanes), no de la página: el precio se
    /// cambia sin recompilar y el pago registrado dice qué plan se vendió.
    /// El formulario no crea nada por sí solo: deja una solicitud en la
    /// bandeja de la plataforma, que cobra por fuera y activa el espacio al
    /// registrar el pago. No hay pasarela, y la página lo dice.
    ///
    /// No hereda de PaginaDeModulo: la oferta no es un módulo apagable.
    /// </summary>
    public partial class OrganizacionesPagina : System.Web.UI.Page
    {
        private IList<Plan> _planes;

        protected void Page_Load(object sender, EventArgs e)
        {
            _planes = Contenido.Datos.ObtenerPlanes();

            phPlanes.Visible = _planes.Count > 0;
            phSinPlanes.Visible = _planes.Count == 0;

            rptPlanes.DataSource = _planes;
            rptPlanes.DataBind();

            // El espacio de ejemplo se configura, no se escribe acá: hoy es el
            // de la demostración y mañana puede ser un cliente real que acepte
            // servir de muestra. Sin ajuste, el botón no aparece.
            string demo = ConfigurationManager.AppSettings["EspacioDemo"];
            Espacio ejemplo = string.IsNullOrEmpty(demo) ? null : Contenido.Datos.ObtenerEspacio(demo);
            lnkDemo.Visible = ejemplo != null;
            if (ejemplo != null) lnkDemo.NavigateUrl = ResolveUrl(ejemplo.Url);

            if (IsPostBack) return;

            ddlPlan.Items.Clear();
            ddlPlan.Items.Add(new ListItem("Todavía no lo sé", "0"));
            foreach (Plan p in _planes)
            {
                ddlPlan.Items.Add(new ListItem(p.Nombre + " · " + p.PrecioTexto + " por " + p.DuracionTexto, p.Codigo.ToString()));
            }
        }

        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            // Un bot llena el campo escondido. Una persona no lo ve. Se
            // responde como si hubiera salido bien para no darle pistas.
            if (!string.IsNullOrEmpty(txtSitio.Text))
            {
                MostrarMensaje("Recibimos la solicitud.", true);
                phForm.Visible = false;
                return;
            }

            int plan;
            int.TryParse(ddlPlan.SelectedValue, out plan);

            ResultadoGuardado r = Contenido.Datos.EnviarSolicitud(
                txtOrganizacion.Text.Trim(), txtContacto.Text.Trim(), txtCorreo.Text.Trim(),
                txtTelefono.Text.Trim(), plan, txtProceso.Text.Trim(), txtFecha.Text.Trim(),
                txtMensaje.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            // Con la solicitud recibida, el formulario se retira: dejarlo
            // invitaría a mandarla dos veces.
            if (r.Ok) phForm.Visible = false;
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
    }
}
