using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// El padrón del espacio administrado: quién puede participar cuando el
    /// padrón está cerrado.
    ///
    /// Se carga por correo, pegando una lista, y se quita por baja lógica.
    /// Lo que la página muestra sobre cada correo (si ya hay cuenta, si
    /// puede participar) lo calcula la base en vwEspacioMiembros. La puerta
    /// de verdad no está acá sino en MotivoSinParticipacion, en el Web
    /// Service, que es quien rechaza a quien no está en la lista.
    /// </summary>
    public partial class PadronPagina : PaginaAdmin
    {
        private IList<Miembro> _miembros;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Siempre, también en postback: la fila que recibe el clic
            // necesita su modelo para resolver el CommandArgument.
            CargarPadron();

            // Con el padrón abierto la lista no decide nada. Se avisa y no se
            // ofrece cargar: sería una lista que parece funcionar y no hace.
            Espacio actual = EspacioAdministrado;
            bool abierto = actual != null && !actual.PadronCerrado;

            phAbierto.Visible = abierto;
            phCarga.Visible = !abierto;
        }

        /// <summary>
        /// La ficha del espacio administrado. La sesión guarda solo el código y
        /// el nombre, así que se busca en la lista de espacios que la cuenta
        /// puede ver, que para la de un cliente es solo el suyo.
        /// </summary>
        private Espacio EspacioAdministrado
        {
            get
            {
                foreach (Espacio x in Contenido.Datos.ObtenerEspacios(Sesion.CodigoUsuario, false))
                {
                    if (x.Codigo == Sesion.CodigoEspacio) return x;
                }
                return null;
            }
        }

        private void CargarPadron()
        {
            _miembros = Contenido.Datos.ObtenerPadron(Sesion.CodigoUsuario, Sesion.CodigoEspacio);

            bool hay = _miembros.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptMiembros.DataSource = _miembros;
                rptMiembros.DataBind();
            }
        }

        protected void btnCargar_Click(object sender, EventArgs e)
        {
            string correos = txtCorreos.Text ?? string.Empty;

            if (correos.Trim().Length == 0)
            {
                MostrarMensaje("Pegá al menos un correo.", false);
                return;
            }

            ResultadoPadron r = Contenido.Datos.CargarPadron(
                Sesion.CodigoUsuario, Sesion.CodigoEspacio, correos);

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            // Lo rechazado se deja en la caja para corregirlo. Lo aceptado ya
            // está en la lista de abajo.
            txtCorreos.Text = r.Rechazados.Replace(", ", "\n");
            CargarPadron();
        }

        protected void rptMiembros_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int codigo;
            if (e.CommandName != "estado" || !int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            Miembro m = null;
            foreach (Miembro x in _miembros)
            {
                if (x.Codigo == codigo) { m = x; break; }
            }
            if (m == null) return;

            Resultado r = Contenido.Datos.CambiarEstadoMiembro(Sesion.CodigoUsuario, codigo, !m.Activo);

            MostrarMensaje(r.Mensaje, r.Ok);

            if (r.Ok) CargarPadron();
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

        protected string TotalTexto
        {
            get
            {
                int activos = 0, pueden = 0;
                if (_miembros != null)
                {
                    foreach (Miembro x in _miembros)
                    {
                        if (!x.Activo) continue;
                        activos++;
                        if (x.PuedeParticipar) pueden++;
                    }
                }
                return Vista.Plural(activos, "correo en el padrón", "correos en el padrón")
                     + " · " + pueden + " ya con cuenta confirmada";
            }
        }
    }
}
