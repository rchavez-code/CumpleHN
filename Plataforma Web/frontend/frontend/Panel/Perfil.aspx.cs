using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Panel
{
    /// <summary>
    /// Edición del perfil del candidato. Es la contraparte privada de la ficha
    /// pública en <c>Candidato.aspx</c>: lo que se llena acá es exactamente lo
    /// que se publica allá.
    ///
    /// Se guarda solo lo que redacta la candidatura: presentación, contacto y
    /// redes (<c>spPanelGuardarPerfil</c>, script 24). El nombre, el cargo, el
    /// partido, el departamento y el municipio se muestran pero no se editan:
    /// los registra la administración, que es la que los contrasta.
    /// </summary>
    public partial class Perfil : PaginaPanel
    {
        private EdicionPanel _edicion;

        protected void Page_Load(object sender, EventArgs e)
        {
            // El cargo se muestra con los catálogos del espacio de la
            // candidatura, que no siempre es la plataforma.
            Espacios.Fijar(CandidatoActual.EspacioSlug);

            _edicion = Contenido.Datos.ObtenerEdicionPanel(Sesion.CodigoUsuario, 0);

            if (!IsPostBack)
            {
                CargarCatalogos();
                CargarDatos();

                // Lo deja el guardado antes de volver a cargar la página.
                string aviso = Sesion.TomarAviso();
                if (!string.IsNullOrEmpty(aviso)) Ok(aviso);
            }

            AplicarBloqueo();
        }

        private void CargarCatalogos()
        {
            ddlCargo.Items.Add(new ListItem("Seleccioná un cargo", string.Empty));
            foreach (string cargo in Contenido.Datos.ObtenerCargos())
            {
                ddlCargo.Items.Add(new ListItem(cargo, cargo));
            }

            ddlDepartamento.Items.Add(new ListItem("Seleccioná un departamento", string.Empty));
            foreach (string dep in Contenido.Datos.ObtenerDepartamentos())
            {
                ddlDepartamento.Items.Add(new ListItem(dep, dep));
            }
        }

        private void CargarDatos()
        {
            txtNombres.Text = CandidatoActual.Nombres;
            txtApellidos.Text = CandidatoActual.Apellidos;
            txtPartido.Text = CandidatoActual.Partido;
            txtMunicipio.Text = CandidatoActual.Municipio;
            txtTitular.Text = CandidatoActual.Titular;
            txtBiografia.Text = CandidatoActual.Biografia;
            txtProfesional.Text = CandidatoActual.InformacionProfesional;
            txtCandidatura.Text = CandidatoActual.DescripcionCandidatura;
            txtCorreo.Text = CandidatoActual.CorreoPublico;
            txtTelefono.Text = CandidatoActual.Telefono;
            txtSitio.Text = CandidatoActual.SitioWeb;
            txtFacebook.Text = CandidatoActual.Facebook;
            txtX.Text = CandidatoActual.X;
            txtInstagram.Text = CandidatoActual.Instagram;

            Seleccionar(ddlCargo, CandidatoActual.Cargo);
            Seleccionar(ddlDepartamento, CandidatoActual.Departamento);
        }

        private static void Seleccionar(DropDownList lista, string valor)
        {
            ListItem item = lista.Items.FindByValue(valor ?? string.Empty);
            if (item != null) lista.SelectedValue = item.Value;
        }

        /// <summary>
        /// Con la campaña cerrada o el espacio vencido el perfil se consulta
        /// pero no se edita. Lo decide la base con la misma función que usa el
        /// procedimiento de guardado.
        /// </summary>
        private void AplicarBloqueo()
        {
            if (_edicion.Editable) return;

            foreach (TextBox t in new[] { txtTitular, txtBiografia, txtProfesional, txtCandidatura,
                                          txtCorreo, txtTelefono, txtSitio, txtFacebook, txtX, txtInstagram })
            {
                t.ReadOnly = true;
            }

            btnGuardar.Visible = false;
            fuFoto.Enabled = false;
            btnSubirFoto.Visible = false;
            litBloqueo.Text = Server.HtmlEncode(_edicion.Motivo);
            phBloqueo.Visible = true;
        }

        /// <summary>
        /// Sube la fotografía. Va aparte de «Guardar cambios» para que un
        /// problema con la imagen no se mezcle con los del resto del perfil.
        ///
        /// El archivo llega en este postback y se reenvía al backend
        /// (Servicios/Archivos.cs). El backend comprueba el tipo real, el
        /// tamaño y que la cuenta sea de una candidatura, y da de baja la
        /// foto anterior.
        /// </summary>
        protected void btnSubirFoto_Click(object sender, EventArgs e)
        {
            if (!_edicion.Editable) return;

            if (!fuFoto.HasFile)
            {
                MostrarError("Elegí una fotografía antes de subirla.");
                return;
            }

            // Se comprueba acá para no mandar al backend algo que va a
            // rechazar. El backend lo vuelve a comprobar de todos modos.
            if (fuFoto.PostedFile.ContentLength > Archivos.MaximoFoto)
            {
                MostrarError("La fotografía supera los 2 MB permitidos.");
                return;
            }

            ResultadoGuardado r = Archivos.Subir(Sesion.CodigoUsuario, "Foto", 0,
                                                 fuFoto.FileName, fuFoto.FileBytes);

            if (!r.Ok)
            {
                MostrarError(r.Mensaje);
                return;
            }

            // Se recarga para que el avatar y la verificación muestren la
            // foto nueva, igual que al guardar el resto del perfil.
            Sesion.DejarAviso(r.Mensaje);
            Response.Redirect("~/Panel/Perfil");
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!_edicion.Editable) return;

            string correo = txtCorreo.Text.Trim();
            if (!string.IsNullOrEmpty(correo) && correo.IndexOf('@') < 0)
            {
                MostrarError("El correo de contacto no tiene un formato válido.");
                return;
            }

            ResultadoGuardado r = Contenido.Datos.GuardarPerfilPanel(Sesion.CodigoUsuario,
                txtTitular.Text.Trim(), txtBiografia.Text.Trim(), txtProfesional.Text.Trim(),
                txtCandidatura.Text.Trim(), correo, txtTelefono.Text.Trim(), txtSitio.Text.Trim(),
                txtFacebook.Text.Trim(), txtX.Text.Trim(), txtInstagram.Text.Trim());

            if (!r.Ok)
            {
                MostrarError(r.Mensaje);
                return;
            }

            // Se vuelve a cargar la página para que la verificación y el
            // avance del perfil reflejen lo guardado: la candidatura de esta
            // petición se leyó antes del clic.
            Sesion.DejarAviso(r.Mensaje);
            Response.Redirect("~/Panel/Perfil");
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

        protected int PerfilCompleto
        {
            get { return CandidatoActual.PerfilCompleto; }
        }

        protected string VerificacionTexto
        {
            get { return CandidatoActual.VerificacionTexto; }
        }

        protected string VerificacionClase
        {
            get { return CandidatoActual.VerificacionClase; }
        }

        protected string Iniciales
        {
            get { return CandidatoActual.TieneFoto ? string.Empty : CandidatoActual.Iniciales; }
        }

        protected string EstiloAvatar
        {
            get
            {
                return CandidatoActual.TieneFoto
                    ? Vista.EstiloAvatar(ResolveUrl(CandidatoActual.FotoUrl))
                    : string.Empty;
            }
        }

        protected string UrlPerfilPublico
        {
            get { return ResolveUrl(CandidatoActual.Url); }
        }
    }
}
