using System;
using System.Collections.Generic;
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
    ///
    /// Qué se puede editar lo decide la base (<c>spPanelEdicion</c>, script
    /// 24) con las mismas funciones que usa el procedimiento de guardado: un
    /// proyecto con estado de cumplimiento asignado o de una campaña cerrada no
    /// se edita, y uno con reacciones solo admite cambiar el estado declarado.
    /// </summary>
    public partial class Proyecto : PaginaPanel
    {
        private const string Declarada = "Declarada";
        private const string EnProceso = "En proceso";

        private Propuesta _propuesta;
        private bool _esNuevo;
        private EdicionPanel _edicion;
        private IList<OpcionCatalogo> _categorias;

        protected void Page_Load(object sender, EventArgs e)
        {
            // El panel es de la candidatura, y la candidatura es de un
            // espacio: las categorías que se ofrecen son las de ese espacio.
            Espacios.Fijar(CandidatoActual.EspacioSlug);

            int id;
            if (int.TryParse(Request.QueryString["id"], out id))
            {
                _propuesta = Contenido.Datos.ObtenerPropuesta(id);

                // Nadie edita el proyecto de otra candidatura. El procedimiento
                // lo vuelve a comprobar: esto solo evita mostrar el formulario.
                if (_propuesta != null && _propuesta.CandidatoSlug != CandidatoActual.Slug)
                {
                    Response.Redirect("~/Panel/Proyectos");
                    return;
                }
            }

            _esNuevo = _propuesta == null;
            _categorias = Contenido.Datos.ObtenerCategoriasConCodigo();
            _edicion = Contenido.Datos.ObtenerEdicionPanel(
                Sesion.CodigoUsuario, _esNuevo ? 0 : _propuesta.Id);

            Page.Title = _esNuevo ? "Nuevo proyecto" : "Editar proyecto";

            if (!IsPostBack)
            {
                CargarCatalogos();

                if (!_esNuevo) CargarDatos();

                // Lo deja el adjuntar o el quitar un documento antes de
                // volver a cargar la página.
                string aviso = Sesion.TomarAviso();
                if (!string.IsNullOrEmpty(aviso)) Ok(aviso);

                // Solo en la primera carga: en un postback el repetidor se
                // reconstruye solo desde el ViewState, que es lo que permite
                // que el clic en «Quitar» llegue a rptRespaldos_ItemCommand.
                CargarRespaldos();
            }

            AplicarBloqueo();
        }

        // ------------------------------------------- Documentos de respaldo

        /// <summary>
        /// Adjuntar y quitar exige un proyecto ya guardado y que se pueda
        /// editar. No depende de que el texto esté cerrado por reacciones: un
        /// documento respalda lo prometido, no lo cambia (script 25).
        /// </summary>
        protected bool PuedeAdjuntar
        {
            get { return !_esNuevo && _edicion.Editable; }
        }

        private void CargarRespaldos()
        {
            if (_esNuevo) return;

            rptRespaldos.DataSource = Contenido.Datos.ObtenerArchivosPropuesta(_propuesta.Id);
            rptRespaldos.DataBind();
        }

        /// <summary>
        /// Adjunta un documento. El archivo llega en este postback y se reenvía
        /// al backend, que comprueba el tipo real, el tamaño, el tope de cinco
        /// y que el proyecto sea de esta candidatura.
        /// </summary>
        protected void btnAdjuntar_Click(object sender, EventArgs e)
        {
            if (!PuedeAdjuntar) return;

            if (!fuRespaldo.HasFile)
            {
                MostrarError("Elegí un documento antes de adjuntarlo.");
                return;
            }

            // Se comprueba acá para no mandar al backend algo que va a
            // rechazar. El backend lo vuelve a comprobar de todos modos.
            if (fuRespaldo.PostedFile.ContentLength > Archivos.MaximoRespaldo)
            {
                MostrarError("El documento supera los 5 MB permitidos.");
                return;
            }

            ResultadoGuardado r = Archivos.Subir(Sesion.CodigoUsuario, "Respaldo", _propuesta.Id,
                                                 fuRespaldo.FileName, fuRespaldo.FileBytes);

            if (!r.Ok)
            {
                MostrarError(r.Mensaje);
                return;
            }

            VolverConAviso(r.Mensaje);
        }

        /// <summary>«Quitar» de un documento: baja lógica en la base.</summary>
        protected void rptRespaldos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Quitar" || !PuedeAdjuntar) return;

            int codigoArchivo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigoArchivo)) return;

            Resultado r = Contenido.Datos.QuitarArchivo(Sesion.CodigoUsuario, codigoArchivo);

            if (!r.Ok)
            {
                MostrarError(r.Mensaje);
                return;
            }

            VolverConAviso(r.Mensaje);
        }

        /// <summary>
        /// Vuelve a cargar la misma página con un aviso. Así la lista de
        /// documentos se lee de nuevo y un refresco del navegador no repite
        /// la subida.
        /// </summary>
        private void VolverConAviso(string mensaje)
        {
            Sesion.DejarAviso(mensaje);
            Response.Redirect("~/Panel/Proyecto?id=" + _propuesta.Id);
        }

        private void CargarCatalogos()
        {
            ddlCategoria.Items.Add(new ListItem("Seleccioná una categoría", string.Empty));
            foreach (OpcionCatalogo cat in _categorias)
            {
                ddlCategoria.Items.Add(new ListItem(cat.Nombre, cat.Codigo.ToString()));
            }

            // Solo los estados que le corresponde declarar a la candidatura.
            ddlEstado.Items.Add(new ListItem(Declarada, Declarada));
            ddlEstado.Items.Add(new ListItem(EnProceso, EnProceso));
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

            ListItem cat = ddlCategoria.Items.FindByText(_propuesta.Categoria ?? string.Empty);
            if (cat != null) ddlCategoria.SelectedValue = cat.Value;

            ListItem estado = ddlEstado.Items.FindByValue(
                _propuesta.Estado == EstadoPropuesta.EnProceso ? EnProceso : Declarada);
            if (estado != null) ddlEstado.SelectedValue = estado.Value;
        }

        /// <summary>
        /// Cierra lo que no se puede cambiar antes de que la persona lo llene.
        /// Corre en cada carga porque <c>ReadOnly</c> y <c>Enabled</c> no se
        /// conservan solos entre postbacks si la página no los vuelve a fijar.
        /// </summary>
        private void AplicarBloqueo()
        {
            bool textoAbierto = _edicion.Editable && _edicion.TextoEditable;

            foreach (TextBox t in new[] { txtNombre, txtDescripcion, txtProblema, txtObjetivo,
                                          txtBeneficiarios, txtUbicacion, txtPeriodo, txtAdicional })
            {
                t.ReadOnly = !textoAbierto;
            }

            ddlCategoria.Enabled = textoAbierto;
            ddlEstado.Enabled = _edicion.Editable;
            btnGuardar.Visible = _edicion.Editable;

            // Documentos: en un proyecto nuevo solo el aviso de guardar
            // primero. Con el formulario cerrado, la lista sin «Adjuntar».
            phRespaldoNuevo.Visible = _esNuevo;
            phRespaldos.Visible = !_esNuevo;
            phAdjuntar.Visible = PuedeAdjuntar;

            if (!textoAbierto && !string.IsNullOrEmpty(_edicion.Motivo))
            {
                litBloqueo.Text = Server.HtmlEncode(_edicion.Motivo);
                phBloqueo.Visible = true;
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!_edicion.Editable) return;

            string nombre, descripcion, problema, objetivo, beneficiarios, ubicacion, periodo, adicional;
            int codigoCategoria;

            if (_edicion.TextoEditable)
            {
                nombre = txtNombre.Text.Trim();
                descripcion = txtDescripcion.Text.Trim();
                problema = txtProblema.Text.Trim();
                objetivo = txtObjetivo.Text.Trim();
                beneficiarios = txtBeneficiarios.Text.Trim();
                ubicacion = txtUbicacion.Text.Trim();
                periodo = txtPeriodo.Text.Trim();
                adicional = txtAdicional.Text.Trim();
                int.TryParse(ddlCategoria.SelectedValue, out codigoCategoria);

                if (string.IsNullOrEmpty(nombre))
                {
                    MostrarError("El nombre del proyecto es obligatorio.");
                    return;
                }

                if (string.IsNullOrEmpty(descripcion))
                {
                    MostrarError("Agregá una descripción del proyecto.");
                    return;
                }

                if (codigoCategoria <= 0)
                {
                    MostrarError("Seleccioná el área o categoría del proyecto.");
                    return;
                }

                if (string.IsNullOrEmpty(problema))
                {
                    MostrarError("Indicá qué problema busca solucionar el proyecto.");
                    return;
                }

                if (string.IsNullOrEmpty(objetivo))
                {
                    MostrarError("Indicá el objetivo del proyecto.");
                    return;
                }
            }
            else
            {
                // Con el texto cerrado solo cambia el estado. Se reenvía el
                // contenido tal como está guardado, no lo que diga el
                // formulario: un control deshabilitado no viaja en el
                // postback, y el procedimiento compara contra la base.
                nombre = _propuesta.Nombre;
                descripcion = _propuesta.Descripcion;
                problema = _propuesta.Problema;
                objetivo = _propuesta.Objetivo;
                beneficiarios = _propuesta.Beneficiarios;
                ubicacion = _propuesta.Ubicacion;
                periodo = _propuesta.PeriodoEjecucion;
                adicional = _propuesta.InformacionAdicional;
                codigoCategoria = CodigoCategoria(_propuesta.Categoria);
            }

            ResultadoGuardado r = Contenido.Datos.GuardarPropuestaPanel(
                Sesion.CodigoUsuario, _esNuevo ? 0 : _propuesta.Id,
                nombre, descripcion, problema, objetivo, beneficiarios,
                codigoCategoria, ubicacion, periodo, ddlEstado.SelectedValue, adicional);

            if (!r.Ok)
            {
                MostrarError(r.Mensaje);
                return;
            }

            // El mensaje del procedimiento dice si el proyecto volvió a
            // declarado, así que es el que se muestra en el listado.
            Sesion.DejarAviso(r.Mensaje);
            Response.Redirect("~/Panel/Proyectos");
        }

        private int CodigoCategoria(string nombre)
        {
            foreach (OpcionCatalogo c in _categorias)
            {
                if (string.Equals(c.Nombre, nombre, StringComparison.OrdinalIgnoreCase)) return c.Codigo;
            }

            return 0;
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
            get { return string.IsNullOrEmpty(CategoriaNombre) ? "Sin categoría" : CategoriaNombre; }
        }

        protected string CategoriaClase
        {
            get { return Vista.ClaseCategoria(CategoriaNombre); }
        }

        /// <summary>El desplegable guarda el código, la vista necesita el nombre.</summary>
        private string CategoriaNombre
        {
            get
            {
                return string.IsNullOrEmpty(ddlCategoria.SelectedValue) || ddlCategoria.SelectedItem == null
                    ? string.Empty
                    : ddlCategoria.SelectedItem.Text;
            }
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
                return ddlEstado.SelectedValue == EnProceso
                    ? EstadoPropuesta.EnProceso
                    : EstadoPropuesta.Declarada;
            }
        }
    }
}
