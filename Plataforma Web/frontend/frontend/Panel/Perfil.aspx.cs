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
    /// Pendiente: el guardado corresponde al Web Service de candidatos. La
    /// validación de entrada ya está acá porque también debe existir del lado
    /// del servidor.
    /// </summary>
    public partial class Perfil : PaginaPanel
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCatalogos();
                CargarDatos();
            }
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

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombres.Text.Trim()) || string.IsNullOrEmpty(txtApellidos.Text.Trim()))
            {
                MostrarError("Los nombres y apellidos son obligatorios.");
                return;
            }

            if (string.IsNullOrEmpty(ddlCargo.SelectedValue))
            {
                MostrarError("Seleccioná el cargo al que aspirás.");
                return;
            }

            string correo = txtCorreo.Text.Trim();
            if (!string.IsNullOrEmpty(correo) && correo.IndexOf('@') < 0)
            {
                MostrarError("El correo de contacto no tiene un formato válido.");
                return;
            }

            // Acá irá la llamada al Web Service de candidatos para persistir.
            Ok("Los datos son válidos. La persistencia se habilita al conectar el Web Service de candidatos.");
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
