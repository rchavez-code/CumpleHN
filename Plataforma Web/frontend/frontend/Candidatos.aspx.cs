using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Listado de candidaturas con búsqueda y filtros.
    ///
    /// Los filtros viajan en la dirección (parámetros <c>q</c>, <c>c</c> y
    /// <c>n</c>) para que cualquier resultado se pueda compartir o marcar como
    /// favorito, en lugar de quedar atrapado en el estado de un formulario.
    /// </summary>
    public partial class CandidatosPagina : Page
    {
        private const string TodasLasCampanas = "";
        private const string TodosLosNiveles = "";

        private string _q;
        private string _campana;
        private string _nivel;
        private int _total;

        protected void Page_Load(object sender, EventArgs e)
        {
            _q = (Request.QueryString["q"] ?? string.Empty).Trim();
            _campana = Request.QueryString["c"] ?? TodasLasCampanas;
            _nivel = Request.QueryString["n"] ?? TodosLosNiveles;

            if (!IsPostBack)
            {
                CargarFiltros();
                txtBuscar.Text = _q;
            }

            CargarResultados();
        }

        private void CargarFiltros()
        {
            ddlCampana.Items.Add(new ListItem("Todas las campañas", TodasLasCampanas));
            foreach (Campana c in Contenido.Datos.ObtenerCampanas())
            {
                ddlCampana.Items.Add(new ListItem(c.Nombre, c.Slug));
            }
            Seleccionar(ddlCampana, _campana);

            ddlNivel.Items.Add(new ListItem("Todos los niveles", TodosLosNiveles));
            ddlNivel.Items.Add(new ListItem("Nacional", "Nacional"));
            ddlNivel.Items.Add(new ListItem("Departamental", "Departamental"));
            ddlNivel.Items.Add(new ListItem("Municipal", "Municipal"));
            Seleccionar(ddlNivel, _nivel);
        }

        private static void Seleccionar(DropDownList lista, string valor)
        {
            ListItem item = lista.Items.FindByValue(valor ?? string.Empty);
            if (item != null) lista.SelectedValue = item.Value;
        }

        private void CargarResultados()
        {
            IList<Candidato> todos = Contenido.Datos.ObtenerCandidatos(_campana);
            List<Candidato> filtrados = new List<Candidato>();

            foreach (Candidato c in todos)
            {
                if (!CoincideTexto(c, _q)) continue;
                if (!CoincideNivel(c, _nivel)) continue;

                filtrados.Add(c);
            }

            _total = filtrados.Count;

            rptCandidatos.DataSource = filtrados;
            rptCandidatos.DataBind();

            phVacio.Visible = _total == 0;
        }

        private static bool CoincideTexto(Candidato c, string termino)
        {
            if (string.IsNullOrEmpty(termino)) return true;

            return Contiene(c.NombreCompleto, termino)
                || Contiene(c.Cargo, termino)
                || Contiene(c.Partido, termino)
                || Contiene(c.Territorio, termino);
        }

        private static bool Contiene(string texto, string termino)
        {
            if (string.IsNullOrEmpty(texto)) return false;
            return texto.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool CoincideNivel(Candidato c, string nivel)
        {
            if (string.IsNullOrEmpty(nivel)) return true;
            return string.Equals(Vista.TextoNivel(c.Nivel), nivel, StringComparison.OrdinalIgnoreCase);
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string destino = "~/Candidatos?q=" + HttpUtility.UrlEncode(txtBuscar.Text.Trim())
                + "&c=" + HttpUtility.UrlEncode(ddlCampana.SelectedValue)
                + "&n=" + HttpUtility.UrlEncode(ddlNivel.SelectedValue);

            Response.Redirect(destino);
        }

        // ---------------------------------------------------- Presentación

        protected string TotalTexto
        {
            get { return Vista.Plural(_total, "candidatura", "candidaturas"); }
        }

        protected string DescripcionFiltro
        {
            get
            {
                List<string> partes = new List<string>();

                if (!string.IsNullOrEmpty(_q)) partes.Add("que coinciden con «" + _q + "»");

                if (!string.IsNullOrEmpty(_campana))
                {
                    Campana c = Contenido.Datos.ObtenerCampana(_campana);
                    if (c != null) partes.Add("en " + c.Nombre);
                }

                if (!string.IsNullOrEmpty(_nivel)) partes.Add("de nivel " + _nivel.ToLowerInvariant());

                if (partes.Count == 0) return "Todas las candidaturas registradas en la plataforma.";

                return "Resultados " + string.Join(", ", partes.ToArray()) + ".";
            }
        }
    }
}
