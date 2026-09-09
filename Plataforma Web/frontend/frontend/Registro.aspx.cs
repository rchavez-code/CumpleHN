using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Registro de cuenta, ciudadana o de candidato.
    ///
    /// El tipo de cuenta viaja en la dirección (<c>?tipo=</c>) en lugar de en el
    /// estado de un control, de modo que el enlace «Soy candidato» del encabezado
    /// llegue directo al formulario correcto.
    ///
    /// La cuenta ciudadana se crea acá, contra el Web Service. La contraseña no
    /// se cifra en el frontend: viaja al backend, que es el único que conoce el
    /// algoritmo con el que después se valida el acceso.
    ///
    /// La candidatura no se registra por esta vía. La da de alta la
    /// administración de la plataforma, junto con su cuenta, porque una ficha
    /// pública que nadie revisó antes de publicarse es lo contrario de lo que
    /// sostiene la neutralidad del sitio.
    /// </summary>
    public partial class Registro : Page
    {
        private const string TipoCandidato = "candidato";
        private const string TipoCiudadano = "ciudadano";

        private string _tipo;

        protected void Page_Load(object sender, EventArgs e)
        {
            _tipo = NormalizarTipo(Request.QueryString["tipo"]);

            phCandidato.Visible = _tipo == TipoCandidato;

            Page.Title = _tipo == TipoCandidato ? "Registrar candidatura" : "Crear cuenta";

            if (!IsPostBack)
            {
                CargarBeneficios();

                if (phCandidato.Visible) CargarCatalogos();
            }
        }

        private static string NormalizarTipo(string valor)
        {
            if (!string.IsNullOrEmpty(valor)
                && string.Equals(valor, TipoCandidato, StringComparison.OrdinalIgnoreCase))
                return TipoCandidato;

            return TipoCiudadano;
        }

        private void CargarCatalogos()
        {
            // Solo campañas con registro abierto.
            foreach (Campana c in Contenido.Datos.ObtenerCampanas())
            {
                if (c.Estado != EstadoCampana.Activa) continue;
                ddlCampana.Items.Add(new ListItem(c.Nombre, c.Slug));
            }

            if (ddlCampana.Items.Count == 0)
            {
                ddlCampana.Items.Add(new ListItem("No hay campañas con registro abierto", string.Empty));
                ddlCampana.Enabled = false;
            }

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

        private void CargarBeneficios()
        {
            List<string> items = new List<string>();

            if (_tipo == TipoCandidato)
            {
                items.Add("Perfil público con tu biografía, tu cargo y tu contacto");
                items.Add("Registro de tus proyectos y propuestas de campaña");
                items.Add("Publicaciones para informar avances a la ciudadanía");
                items.Add("Panel privado para administrar todo tu contenido");
            }
            else
            {
                items.Add("Apoyar publicaciones de las candidaturas");
                items.Add("Comentar y participar en votaciones de percepción");
                items.Add("Guardar candidaturas y propuestas de tu interés");
                items.Add("Consultar la plataforma completa, siempre sin costo");
            }

            rptBeneficios.DataSource = items;
            rptBeneficios.DataBind();
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombres.Text.Trim()) || string.IsNullOrEmpty(txtApellidos.Text.Trim()))
            {
                MostrarError("Ingresá tus nombres y apellidos.");
                return;
            }

            string correo = txtCorreo.Text.Trim();
            if (string.IsNullOrEmpty(correo) || correo.IndexOf('@') < 0)
            {
                MostrarError("Ingresá un correo electrónico válido.");
                return;
            }

            if (txtClave.Text.Length < 8)
            {
                MostrarError("La contraseña debe tener al menos ocho caracteres.");
                return;
            }

            if (txtClave.Text != txtClave2.Text)
            {
                MostrarError("Las contraseñas no coinciden.");
                return;
            }

            if (_tipo == TipoCandidato)
            {
                if (string.IsNullOrEmpty(ddlCampana.SelectedValue))
                {
                    MostrarError("Seleccioná la campaña electoral en la que participás.");
                    return;
                }

                if (string.IsNullOrEmpty(ddlCargo.SelectedValue))
                {
                    MostrarError("Seleccioná el cargo al que aspirás.");
                    return;
                }
            }

            if (!chkTerminos.Checked)
            {
                MostrarError("Confirmá que la información que registres es veraz para continuar.");
                return;
            }

            if (_tipo == TipoCandidato)
            {
                MostrarAviso("La candidatura la registra la administración de la plataforma, junto con su "
                    + "cuenta de acceso. Escribí a la dirección de contacto con estos datos y te la damos "
                    + "de alta.");
                return;
            }

            CrearCuentaCiudadana(correo);
        }

        /// <summary>
        /// Alta de la cuenta contra el Web Service.
        ///
        /// El backend valida de nuevo todo lo que ya validó el formulario, y su
        /// palabra es la que vale: quien llame al servicio directamente se salta
        /// esta página entera.
        /// </summary>
        private void CrearCuentaCiudadana(string correo)
        {
            var cliente = new webservices.WebServiceGlobalSoapClient();

            webservices.RespuestaLogin respuesta;
            try
            {
                respuesta = cliente.RegistrarCiudadano(
                    txtNombres.Text.Trim(), txtApellidos.Text.Trim(), correo, txtClave.Text);
                cliente.Close();
            }
            catch (Exception ex)
            {
                cliente.Abort();
                MostrarError("No se pudo contactar al servidor. " + ex.Message);
                return;
            }

            if (!respuesta.ok || respuesta.usuario == null)
            {
                MostrarError(respuesta.mensaje);
                return;
            }

            // La cuenta acaba de crearse con estas credenciales, así que mandar
            // ahora al formulario de acceso sería pedir que se compruebe algo
            // que se acaba de comprobar. Se entra directo.
            Sesion.Iniciar(respuesta.usuario);

            // Lo que el backend contestó sobre el envío del correo se guarda
            // para la página siguiente. Si muriera con este redirect, quien se
            // registre mientras el servidor de correo está caído se quedaría
            // esperando un mensaje que nadie mandó.
            Sesion.DejarAviso(respuesta.mensaje);

            // Si llegó al registro desde una publicación en la que quiso
            // participar, vuelve a esa misma página.
            string volver = Request.QueryString["volver"];
            if (Sesion.EsDestinoSeguro(volver))
            {
                Response.Redirect(volver);
                return;
            }

            Response.Redirect(Autorizacion.InicioDe(respuesta.usuario.rol));
        }

        private void MostrarAviso(string mensaje)
        {
            litError.Text = Server.HtmlEncode(mensaje);
            phError.Visible = true;
        }

        private void MostrarError(string mensaje)
        {
            litError.Text = Server.HtmlEncode(mensaje);
            phError.Visible = true;
        }

        /// <summary>
        /// Enlaces de la propia página que conservan el destino con el que se
        /// llegó, por la misma razón que en el acceso: perderlo deja a la
        /// persona en la portada después de registrarse, buscando de nuevo la
        /// publicación en la que estaba.
        /// </summary>
        protected string UrlConDestino(string ruta)
        {
            string volver = Request.QueryString["volver"];
            if (!Sesion.EsDestinoSeguro(volver)) return ResolveUrl(ruta);

            string separador = ruta.IndexOf('?') >= 0 ? "&" : "?";
            return ResolveUrl(ruta + separador + "volver=" + Server.UrlEncode(volver));
        }

        // ---------------------------------------------------- Presentación

        protected string ClaseTipo(string tipo)
        {
            return _tipo == tipo ? "is-sel" : string.Empty;
        }

        protected string TituloAside
        {
            get
            {
                return _tipo == TipoCandidato
                    ? "Documentá tus propuestas y dejalas al alcance de cualquiera."
                    : "Consultar es libre. La cuenta es para participar.";
            }
        }

        protected string TextoAside
        {
            get
            {
                return _tipo == TipoCandidato
                    ? "Registrá tu candidatura en la campaña actual, publicá tus proyectos con metas claras y "
                      + "mantené tu perfil al día. Todo lo que registres queda disponible para consulta pública."
                    : "La información de campañas, candidaturas y propuestas es pública y no requiere registro. "
                      + "La cuenta habilita las funciones de participación.";
            }
        }
    }
}
