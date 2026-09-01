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
    /// Listado general de propuestas con búsqueda y filtro por categoría y
    /// campaña. Los filtros viajan en la dirección para que el resultado se
    /// pueda compartir.
    /// </summary>
    public partial class PropuestasPagina : Page
    {
        private string _q;
        private string _categoria;
        private string _campana;
        private int _total;

        protected void Page_Load(object sender, EventArgs e)
        {
            _q = (Request.QueryString["q"] ?? string.Empty).Trim();
            _categoria = Request.QueryString["cat"] ?? string.Empty;
            _campana = Request.QueryString["c"] ?? string.Empty;

            if (!IsPostBack)
            {
                CargarFiltros();
                txtBuscar.Text = _q;
            }

            CargarResultados();
        }

        private void CargarFiltros()
        {
            ddlCategoria.Items.Add(new ListItem("Todas las categorías", string.Empty));
            foreach (string cat in Contenido.Datos.ObtenerCategorias())
            {
                ddlCategoria.Items.Add(new ListItem(cat, cat));
            }
            Seleccionar(ddlCategoria, _categoria);

            ddlCampana.Items.Add(new ListItem("Todas las campañas", string.Empty));
            foreach (Campana c in Contenido.Datos.ObtenerCampanas())
            {
                ddlCampana.Items.Add(new ListItem(c.Nombre, c.Slug));
            }
            Seleccionar(ddlCampana, _campana);
        }

        private static void Seleccionar(DropDownList lista, string valor)
        {
            ListItem item = lista.Items.FindByValue(valor ?? string.Empty);
            if (item != null) lista.SelectedValue = item.Value;
        }

        private void CargarResultados()
        {
            IList<Propuesta> todas = Contenido.Datos.ObtenerPropuestasDeCampana(_campana);
            List<Propuesta> filtradas = new List<Propuesta>();

            foreach (Propuesta p in todas)
            {
                if (!string.IsNullOrEmpty(_categoria)
                    && !string.Equals(p.Categoria, _categoria, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!CoincideTexto(p, _q)) continue;

                filtradas.Add(p);
            }

            _total = filtradas.Count;

            rptPropuestas.DataSource = filtradas;
            rptPropuestas.DataBind();

            phVacio.Visible = _total == 0;
        }

        private static bool CoincideTexto(Propuesta p, string termino)
        {
            if (string.IsNullOrEmpty(termino)) return true;

            return Contiene(p.Nombre, termino)
                || Contiene(p.Descripcion, termino)
                || Contiene(p.Problema, termino)
                || Contiene(p.Objetivo, termino)
                || Contiene(p.Ubicacion, termino);
        }

        private static bool Contiene(string texto, string termino)
        {
            if (string.IsNullOrEmpty(texto)) return false;
            return texto.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string destino = "~/Propuestas?q=" + HttpUtility.UrlEncode(txtBuscar.Text.Trim())
                + "&cat=" + HttpUtility.UrlEncode(ddlCategoria.SelectedValue)
                + "&c=" + HttpUtility.UrlEncode(ddlCampana.SelectedValue);

            Response.Redirect(destino);
        }

        // ---------------------------------------------------- Presentación

        protected string TotalTexto
        {
            get { return Vista.Plural(_total, "propuesta", "propuestas"); }
        }

        protected string DescripcionFiltro
        {
            get
            {
                List<string> partes = new List<string>();

                if (!string.IsNullOrEmpty(_q)) partes.Add("que coinciden con «" + _q + "»");
                if (!string.IsNullOrEmpty(_categoria)) partes.Add("en " + _categoria.ToLowerInvariant());

                if (!string.IsNullOrEmpty(_campana))
                {
                    Campana c = Contenido.Datos.ObtenerCampana(_campana);
                    if (c != null) partes.Add("de " + c.Nombre);
                }

                if (partes.Count == 0) return "Todos los compromisos registrados en la plataforma.";

                return "Resultados " + string.Join(", ", partes.ToArray()) + ".";
            }
        }
    }
}
