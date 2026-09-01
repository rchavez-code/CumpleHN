using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Panel
{
    /// <summary>
    /// Alta y edición de un proyecto de campaña. Sin <c>id</c> en la dirección
    /// se comporta como alta, con <c>id</c> como edición.
    ///
    /// Nota de diseño: el candidato solo puede declarar el estado inicial de su
    /// proyecto. Los estados de cumplimiento (cumplida, incumplida y los
    /// intermedios) los asigna la plataforma cuando existe evidencia, porque de
    /// lo contrario la candidatura se calificaría a sí misma.
    /// </summary>
    public partial class Proyecto : Page
    {
        private Candidato _candidato;
        private Propuesta _propuesta;
        private bool _esNuevo;

        protected void Page_Load(object sender, EventArgs e)
        {
            _candidato = Contenido.Datos.ObtenerCandidatoAutenticado();

            if (_candidato == null)
            {
                Response.Redirect("~/Acceso");
                return;
            }

            int id;
            if (int.TryParse(Request.QueryString["id"], out id))
            {
                _propuesta = Contenido.Datos.ObtenerPropuesta(id);

                // Nadie edita el proyecto de otra candidatura.
                if (_propuesta != null && _propuesta.CandidatoSlug != _candidato.Slug)
                {
                    Response.Redirect("~/Panel/Proyectos");
                    return;
                }
            }

            _esNuevo = _propuesta == null;

            Page.Title = _esNuevo ? "Nuevo proyecto" : "Editar proyecto";

            if (!IsPostBack)
            {
                CargarCatalogos();

                if (!_esNuevo) CargarDatos();
            }
        }

        private void CargarCatalogos()
        {
            ddlCategoria.Items.Add(new ListItem("Seleccioná una categoría", string.Empty));
            foreach (string cat in Contenido.Datos.ObtenerCategorias())
            {
                ddlCategoria.Items.Add(new ListItem(cat, cat));
            }

            // Solo los estados que le corresponde declarar a la candidatura.
            ddlEstado.Items.Add(new ListItem("Declarada", EstadoPropuesta.Declarada.ToString()));
            ddlEstado.Items.Add(new ListItem("En proceso", EstadoPropuesta.EnProceso.ToString()));
        }

        private void CargarDatos()
        {
            txtNombre.Text = _propuesta.Nombre;
            txtDescripcion.Text = _propuesta.Descripcion;
            txtProblema.Text = _propuesta.Problema;
            txtObjetivo.Text = _propuesta.Objetivo;
            txtBeneficiarios.Text = _propuesta.Beneficiarios;
            txtUbicacion.Text = _propuesta.Ubicacion;
            txtPeriodo.Text = _propuesta.PeriodoEjecucion;
            txtAdicional.Text = _propuesta.InformacionAdicional;

            Seleccionar(ddlCategoria, _propuesta.Categoria);
            Seleccionar(ddlEstado, _propuesta.Estado.ToString());
        }

        private static void Seleccionar(DropDownList lista, string valor)
        {
            ListItem item = lista.Items.FindByValue(valor ?? string.Empty);
            if (item != null) lista.SelectedValue = item.Value;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombre.Text.Trim()))
            {
                MostrarError("El nombre del proyecto es obligatorio.");
                return;
            }

            if (string.IsNullOrEmpty(txtDescripcion.Text.Trim()))
            {
                MostrarError("Agregá una descripción del proyecto.");
                return;
            }

            if (string.IsNullOrEmpty(ddlCategoria.SelectedValue))
            {
                MostrarError("Seleccioná el área o categoría del proyecto.");
                return;
            }

            if (string.IsNullOrEmpty(txtProblema.Text.Trim()))
            {
                MostrarError("Indicá qué problema busca solucionar el proyecto.");
                return;
            }

            if (string.IsNullOrEmpty(txtObjetivo.Text.Trim()))
            {
                MostrarError("Indicá el objetivo del proyecto.");
                return;
            }

            // Acá irá la llamada al Web Service de propuestas para persistir.
            Ok("Los datos son válidos. La persistencia se habilita al conectar el Web Service de propuestas.");
        }

        private void Ok(string mensaje)
        {
            litOk.Text = Server.HtmlEncode(mensaje);
            phOk.Visible = true;
            phError.Visible = false;
        }

        private void MostrarError(string mensaje)
        {
            litError.Text = Server.HtmlEncode(mensaje);
            phError.Visible = true;
            phOk.Visible = false;
        }

        // ---------------------------------------------------- Presentación

        protected string Encabezado
        {
            get { return _esNuevo ? "Nuevo proyecto de campaña" : "Editar proyecto"; }
        }

        /// <summary>
        /// La vista previa del riel refleja lo que ya está escrito en el
        /// formulario, para que el candidato vea el resultado antes de guardar.
        /// </summary>
        protected string NombreVista
        {
            get
            {
                string v = txtNombre.Text.Trim();
                return string.IsNullOrEmpty(v) ? "Nombre del proyecto" : v;
            }
        }

        protected string DescripcionVista
        {
            get
            {
                string v = txtDescripcion.Text.Trim();
                return string.IsNullOrEmpty(v)
                    ? "La descripción aparecerá acá conforme la escribas."
                    : Vista.Resumen(v, 150);
            }
        }

        protected string CategoriaVista
        {
            get
            {
                string v = ddlCategoria.SelectedValue;
                return string.IsNullOrEmpty(v) ? "Sin categoría" : v;
            }
        }

        protected string CategoriaClase
        {
            get { return Vista.ClaseCategoria(ddlCategoria.SelectedValue); }
        }

        protected string EstadoVista
        {
            get { return Vista.TextoEstado(EstadoSeleccionado); }
        }

        protected string EstadoClase
        {
            get { return Vista.ClaseEstado(EstadoSeleccionado); }
        }

        private EstadoPropuesta EstadoSeleccionado
        {
            get
            {
                if (string.Equals(ddlEstado.SelectedValue, EstadoPropuesta.EnProceso.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                    return EstadoPropuesta.EnProceso;

                return EstadoPropuesta.Declarada;
            }
        }
    }
}
