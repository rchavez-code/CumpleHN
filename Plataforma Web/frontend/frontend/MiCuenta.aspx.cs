using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Página propia de la cuenta ciudadana: sus datos, el formulario para
    /// proponer una iniciativa y la lista de las que ya propuso.
    ///
    /// Hereda de <see cref="PaginaCiudadano"/>, así que solo entra el rol
    /// Ciudadano. Lo que la página decide es qué mostrar: si se puede
    /// proponer, si se puede editar y qué mensaje dar lo resuelve el Web
    /// Service contra la base, y esta página transporta la respuesta tal cual.
    /// </summary>
    public partial class MiCuenta : PaginaCiudadano
    {
        private const string TodoElPais = "Todo el país";

        /// <summary>
        /// La lista de iniciativas propias se enlaza en <c>OnInit</c>, no en
        /// <c>Page_Load</c>.
        ///
        /// Las tarjetas llevan un hilo de comentarios, y su caja de texto y su
        /// botón «Publicar» viven dentro de un repetidor. Si el repetidor se
        /// enlazara en Page_Load, ese DataBind correría después de que ASP.NET
        /// ya cargó los valores del formulario y decidió el destino del
        /// postback, y recrearía la tarjeta desde cero: el texto escrito se
        /// perdería, el hilo volvería a quedar oculto y el botón caería en un
        /// contenedor invisible, que es lo que rompe la validación de eventos.
        /// Enlazar en OnInit reconstruye el árbol antes de esos pasos, así que
        /// el postback del hilo llega a un control que existe y conserva lo
        /// tecleado.
        ///
        /// El «me gusta» funciona en las dos formas porque su enlace está
        /// siempre visible. El hilo no, y por eso obliga a enlazar temprano.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CargarMias();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            phConfirmada.Visible = Sesion.CorreoConfirmado;
            phPendiente.Visible = !Sesion.CorreoConfirmado;

            // Con el módulo apagado el formulario se reemplaza por el aviso.
            // Es la misma comprobación que hace el Web Service antes de
            // guardar: acá solo evita que alguien escriba para chocar después.
            bool abierto = Modulos.Visible(Modulos.Iniciativas);
            phFormulario.Visible = abierto;
            phCerrado.Visible = !abierto;

            if (!IsPostBack) CargarCatalogos();
        }

        // ------------------------------------------------------ Catálogos

        private void CargarCatalogos()
        {
            ddlCategoria.Items.Clear();
            ddlCategoria.Items.Add(new ListItem("Seleccioná una categoría", "0"));
            foreach (OpcionCatalogo c in Contenido.Datos.ObtenerCategoriasConCodigo())
            {
                ddlCategoria.Items.Add(new ListItem(c.Nombre, c.Codigo.ToString()));
            }

            ddlDepartamento.Items.Clear();
            ddlDepartamento.Items.Add(new ListItem(TodoElPais, "0"));
            foreach (OpcionCatalogo d in Contenido.Datos.ObtenerDepartamentosConCodigo())
            {
                ddlDepartamento.Items.Add(new ListItem(d.Nombre, d.Codigo.ToString()));
            }
        }

        // ----------------------------------------------------- Formulario

        private int IdEnEdicion
        {
            get
            {
                int id;
                return int.TryParse(hdnId.Value, out id) ? id : 0;
            }
            set { hdnId.Value = value.ToString(); }
        }

        protected string TituloFormulario
        {
            get { return IdEnEdicion > 0 ? "Editar iniciativa" : "Proponer una iniciativa"; }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            string titulo = txtTitulo.Text.Trim();
            string descripcion = txtDescripcion.Text.Trim();
            int categoria = Entero(ddlCategoria.SelectedValue);
            int departamento = Entero(ddlDepartamento.SelectedValue);

            // Las mismas reglas que exige el procedimiento, comprobadas antes
            // para no gastar un viaje en un rechazo seguro. La que manda es la
            // del backend.
            if (titulo.Length < 8)
            {
                MostrarError("Escribí un título de al menos ocho caracteres.");
                return;
            }

            if (descripcion.Length < 20)
            {
                MostrarError("Contá la iniciativa con un poco más de detalle: al menos veinte caracteres.");
                return;
            }

            if (descripcion.Length > 1500)
            {
                MostrarError("La descripción no puede pasar de mil quinientos caracteres.");
                return;
            }

            if (categoria <= 0)
            {
                MostrarError("Elegí la categoría a la que pertenece la iniciativa.");
                return;
            }

            ResultadoGuardado r = Contenido.Datos.GuardarIniciativa(
                Sesion.CodigoUsuario, IdEnEdicion, titulo, descripcion, categoria, departamento);

            if (!r.Ok)
            {
                MostrarError(r.Mensaje);
                return;
            }

            LimpiarFormulario();
            litOk.Text = Server.HtmlEncode(r.Mensaje);
            phOk.Visible = true;
            phError.Visible = false;

            CargarMias();
        }

        protected void lnkCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            IdEnEdicion = 0;
            txtTitulo.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            ddlCategoria.SelectedIndex = 0;
            ddlDepartamento.SelectedIndex = 0;
            btnGuardar.Text = "Publicar iniciativa";
            lnkCancelar.Visible = false;
            phOk.Visible = false;
            phError.Visible = false;
        }

        private void MostrarError(string mensaje)
        {
            litError.Text = Server.HtmlEncode(mensaje);
            phError.Visible = true;
            phOk.Visible = false;
        }

        private static int Entero(string valor)
        {
            int n;
            return int.TryParse(valor, out n) ? n : 0;
        }

        // ------------------------------------------------ Mis iniciativas

        private IList<Iniciativa> _mias;

        private void CargarMias()
        {
            _mias = Contenido.Datos.ObtenerIniciativasDeUsuario(Sesion.CodigoUsuario);

            rptMias.DataSource = _mias;
            rptMias.DataBind();

            phSinIniciativas.Visible = _mias.Count == 0;

            int activas = 0;
            foreach (Iniciativa i in _mias)
            {
                if (i.Activa) activas++;
            }

            litResumen.Text = _mias.Count == 0
                ? "Las propuestas que publiques aparecen acá, con las reacciones que reciban."
                : Server.HtmlEncode(Vista.Plural(activas, "iniciativa publicada", "iniciativas publicadas")
                    + (_mias.Count > activas
                        ? " y " + Vista.Plural(_mias.Count - activas, "retirada", "retiradas")
                        : string.Empty)
                    + ".");
        }

        protected void rptMias_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Entero(Convert.ToString(e.CommandArgument));
            if (id <= 0) return;

            if (e.CommandName == "editar")
            {
                Editar(id);
                return;
            }

            if (e.CommandName == "retirar")
            {
                Resultado r = Contenido.Datos.RetirarIniciativaPropia(Sesion.CodigoUsuario, id);

                litAvisoLista.Text = Server.HtmlEncode(r.Mensaje);
                phAvisoLista.Visible = true;

                // Si se estaba editando justo la que se retiró, el formulario
                // queda vacío: guardar lo que había sería editar una retirada.
                if (r.Ok && IdEnEdicion == id) LimpiarFormulario();

                CargarMias();
            }
        }

        /// <summary>
        /// Lleva una iniciativa propia al formulario. Solo llega acá si la lista
        /// mostró el botón, y el backend vuelve a comprobar al guardar que siga
        /// sin reacciones.
        /// </summary>
        private void Editar(int id)
        {
            Iniciativa item = null;
            foreach (Iniciativa i in _mias)
            {
                if (i.Id == id) item = i;
            }

            if (item == null || !item.PuedeEditar) return;

            IdEnEdicion = id;
            txtTitulo.Text = item.Titulo;
            txtDescripcion.Text = item.Descripcion;
            Seleccionar(ddlCategoria, item.CodigoCategoria);
            Seleccionar(ddlDepartamento, item.CodigoDepartamento);
            btnGuardar.Text = "Guardar cambios";
            lnkCancelar.Visible = true;
            phOk.Visible = false;
            phError.Visible = false;

            // Enfocar el título lleva la vista al formulario, que queda arriba.
            txtTitulo.Focus();
        }

        private static void Seleccionar(DropDownList lista, int codigo)
        {
            ListItem item = lista.Items.FindByValue(codigo.ToString());
            lista.ClearSelection();
            if (item != null) item.Selected = true;
        }

        // ------------------------------------------------------- Cuenta

        protected string Nombre
        {
            get { return Sesion.Nombre; }
        }

        protected string Correo
        {
            get { return Sesion.Correo; }
        }

        protected string Iniciales
        {
            get { return Sesion.Iniciales; }
        }
    }
}
