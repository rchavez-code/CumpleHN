using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Panel
{
    /// <summary>
    /// Administración de los proyectos de campaña del candidato.
    /// </summary>
    public partial class Proyectos : PaginaPanel
    {
        private string _q;
        private string _categoria;
        private int _total;
        private bool _hayAlguno;

        protected void Page_Load(object sender, EventArgs e)
        {
            _q = (Request.QueryString["q"] ?? string.Empty).Trim();
            _categoria = Request.QueryString["cat"] ?? string.Empty;

            if (!IsPostBack)
            {
                CargarFiltros();
                txtBuscar.Text = _q;
            }

            CargarProyectos();
        }

        private void CargarFiltros()
        {
            ddlCategoria.Items.Add(new ListItem("Todas las categorías", string.Empty));
            foreach (string cat in Contenido.Datos.ObtenerCategorias())
            {
                ddlCategoria.Items.Add(new ListItem(cat, cat));
            }

            ListItem item = ddlCategoria.Items.FindByValue(_categoria);
            if (item != null) ddlCategoria.SelectedValue = item.Value;
        }

        private void CargarProyectos()
        {
            IList<Propuesta> todos = Contenido.Datos.ObtenerPropuestas(CandidatoActual.Slug);
            _hayAlguno = todos.Count > 0;

            List<Propuesta> filtrados = new List<Propuesta>();
            foreach (Propuesta p in todos)
            {
                if (!string.IsNullOrEmpty(_categoria)
                    && !string.Equals(p.Categoria, _categoria, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!CoincideTexto(p, _q)) continue;

                filtrados.Add(p);
            }

            _total = filtrados.Count;

            phLista.Visible = _total > 0;
            phVacio.Visible = _total == 0;

            if (_total > 0)
            {
                rptProyectos.DataSource = filtrados;
                rptProyectos.DataBind();
            }
        }

        private static bool CoincideTexto(Propuesta p, string termino)
        {
            if (string.IsNullOrEmpty(termino)) return true;

            return Contiene(p.Nombre, termino)
                || Contiene(p.Descripcion, termino)
                || Contiene(p.Objetivo, termino);
        }

        private static bool Contiene(string texto, string termino)
        {
            if (string.IsNullOrEmpty(texto)) return false;
            return texto.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string destino = "~/Panel/Proyectos?q=" + HttpUtility.UrlEncode(txtBuscar.Text.Trim())
                + "&cat=" + HttpUtility.UrlEncode(ddlCategoria.SelectedValue);

            Response.Redirect(destino);
        }

        /// <summary>Recorta la descripción para la vista de tabla.</summary>
        protected string Resumir(object texto)
        {
            return Vista.Resumen(Convert.ToString(texto), 90);
        }

        // ---------------------------------------------------- Presentación

        protected string TotalTexto
        {
            get { return Vista.Plural(_total, "proyecto", "proyectos"); }
        }

        /// <summary>
        /// El vacío por filtro y el vacío real son situaciones distintas y el
        /// mensaje tiene que distinguirlas.
        /// </summary>
        protected string TituloVacio
        {
            get
            {
                return _hayAlguno
                    ? "Ningún proyecto coincide con el filtro"
                    : "Todavía no registrás proyectos";
            }
        }

        protected string TextoVacio
        {
            get
            {
                return _hayAlguno
                    ? "Probá con otros términos o quitá el filtro de categoría para ver todos tus proyectos."
                    : "Registrá tus propuestas de campaña indicando qué problema resuelven, cuál es su objetivo "
                      + "y a quién benefician. Es lo que la ciudadanía consulta en tu perfil.";
            }
        }
    }
}
