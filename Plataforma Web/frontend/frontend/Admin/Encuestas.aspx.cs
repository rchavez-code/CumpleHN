using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Admin
{
    /// <summary>
    /// Administración de las encuestas de percepción.
    ///
    /// Cuatro acciones sobre una encuesta, y no dos, porque cerrar y retirar no
    /// son lo mismo: la cerrada terminó su votación y sigue a la vista con su
    /// resultado, la retirada desaparece del sitio público. Las dos exigen
    /// motivo, y quien lo exige es el procedimiento almacenado — una validación
    /// que solo viviera en este formulario se saltaría llamando al servicio.
    ///
    /// Nada se borra. Una encuesta eliminada se llevaría sus respuestas, y la
    /// plataforma quedaría contando participación sin filas detrás.
    /// </summary>
    public partial class EncuestasPagina : PaginaAdmin
    {
        private IList<EncuestaAdmin> _encuestas;
        private EncuestaAdmin _seleccion;
        private IList<OpcionEncuesta> _resultado;

        /// <summary>
        /// Qué está haciendo la página. Vive en la sesión porque los paneles se
        /// abren desde la lista y tienen que sobrevivir al postback.
        /// </summary>
        private const string ClaveModo = "admin.encuestas.modo";
        private const string ClaveCodigo = "admin.encuestas.codigo";
        private const string ClaveAccion = "admin.encuestas.accion";

        private const string ModoForm = "form";
        private const string ModoEstado = "estado";
        private const string ModoResultado = "resultado";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Siempre, también en postback: la fila que recibe el clic necesita
            // su modelo para resolver el CommandArgument.
            CargarEncuestas();
            MostrarPaneles(!IsPostBack);
        }

        // -------------------------------------------------------- Lista

        private void CargarEncuestas()
        {
            _encuestas = Contenido.Datos.ObtenerEncuestasAdmin(Sesion.CodigoUsuario, null, null);

            bool hay = _encuestas.Count > 0;

            phLista.Visible = hay;
            phVacio.Visible = !hay;

            if (hay)
            {
                rptEncuestas.DataSource = _encuestas;
                rptEncuestas.DataBind();
            }
        }

        protected void rptEncuestas_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int codigo;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out codigo)) return;

            EncuestaAdmin elegida = BuscarEncuesta(codigo);
            if (elegida == null) return;

            switch (e.CommandName)
            {
                case "editar":
                    Session[ClaveModo] = ModoForm;
                    break;

                case "resultado":
                    Session[ClaveModo] = ModoResultado;
                    break;

                case "cierre":
                    Session[ClaveModo] = ModoEstado;
                    Session[ClaveAccion] = elegida.EstaAbierta ? "cerrar" : "reabrir";
                    break;

                case "baja":
                    Session[ClaveModo] = ModoEstado;
                    Session[ClaveAccion] = elegida.Activo ? "retirar" : "restaurar";
                    break;

                default:
                    return;
            }

            Session[ClaveCodigo] = codigo;
            Mensaje = null;

            // Al abrir un panel sí se cargan los campos con lo que hay: es el
            // punto de partida de la edición.
            MostrarPaneles(true);
        }

        protected void btnNueva_Click(object sender, EventArgs e)
        {
            Session[ClaveModo] = ModoForm;
            Session[ClaveCodigo] = 0;

            Mensaje = null;
            MostrarPaneles(true);
        }

        // ------------------------------------------------------ Paneles

        /// <summary>
        /// Decide qué panel se ve y, cuando <paramref name="cargarCampos"/> es
        /// verdadero, los llena con los datos actuales.
        ///
        /// En el postback que guarda, los campos no se tocan: Page_Load corre
        /// antes que el evento del botón, así que rellenarlos acá borraría lo
        /// que la persona acaba de escribir y se guardaría siempre el valor
        /// anterior.
        /// </summary>
        private void MostrarPaneles(bool cargarCampos)
        {
            string modo = Convert.ToString(Session[ClaveModo]);
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            _seleccion = BuscarEncuesta(codigo);

            // Un código que ya no está en la lista deja el panel sin sentido.
            if (codigo > 0 && _seleccion == null) modo = string.Empty;

            phForm.Visible = modo == ModoForm;
            phEstado.Visible = modo == ModoEstado;
            phResultado.Visible = modo == ModoResultado;

            phOpcionesBloqueadas.Visible =
                modo == ModoForm && _seleccion != null && _seleccion.TieneVotos;

            if (modo == ModoResultado) CargarResultado();

            if (!cargarCampos) return;

            if (modo == ModoForm) CargarFormulario();

            if (modo == ModoEstado)
            {
                txtMotivo.Text = string.Empty;
                btnConfirmarEstado.Text = TituloEstado;
            }
        }

        private void CargarFormulario()
        {
            LlenarCampanas();
            LlenarCategorias();

            if (_seleccion == null)
            {
                txtPregunta.Text = string.Empty;
                txtDescripcion.Text = string.Empty;
                txtOpciones.Text = string.Empty;
                txtInicio.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtCierre.Text = string.Empty;
                return;
            }

            txtPregunta.Text = _seleccion.Pregunta;
            txtDescripcion.Text = _seleccion.Descripcion;
            txtInicio.Text = _seleccion.FechaInicio.ToString("yyyy-MM-dd");
            txtCierre.Text = SoloFecha(_seleccion.FechaCierre);
            txtOpciones.Text = OpcionesComoTexto(_seleccion.Codigo);

            Elegir(ddlCampana, _seleccion.CodigoCampana);
            Elegir(ddlCategoria, _seleccion.CodigoCategoria);
        }

        /// <summary>
        /// Las opciones se editan como texto, una por línea, y así es también
        /// como viajan al procedimiento. Presentarlas en una rejilla de campos
        /// numerados obligaría a fijar de antemano cuántas caben.
        /// </summary>
        private string OpcionesComoTexto(int codigoEncuesta)
        {
            IList<OpcionEncuesta> opciones =
                Contenido.Datos.ObtenerOpcionesEncuestaAdmin(Sesion.CodigoUsuario, codigoEncuesta);

            StringBuilder sb = new StringBuilder();

            foreach (OpcionEncuesta o in opciones)
            {
                if (sb.Length > 0) sb.Append(Environment.NewLine);
                sb.Append(o.Texto);
            }

            return sb.ToString();
        }

        private void LlenarCampanas()
        {
            if (ddlCampana.Items.Count > 0) return;

            foreach (CampanaAdmin c in Contenido.Datos.ObtenerCampanasAdmin(Sesion.CodigoUsuario))
            {
                ddlCampana.Items.Add(new ListItem(c.Nombre, c.Codigo.ToString()));
            }
        }

        private void LlenarCategorias()
        {
            if (ddlCategoria.Items.Count > 0) return;

            // El cero no es un dato faltante: es una encuesta que no se refiere
            // a un área concreta, que es el caso de la pregunta por prioridades.
            ddlCategoria.Items.Add(new ListItem("Sin categoría", "0"));

            foreach (OpcionCatalogo o in Contenido.Datos.ObtenerCategoriasConCodigo())
            {
                ddlCategoria.Items.Add(new ListItem(o.Nombre, o.Codigo.ToString()));
            }
        }

        private static void Elegir(DropDownList lista, int codigo)
        {
            ListItem item = lista.Items.FindByValue(codigo.ToString());
            if (item != null) lista.SelectedValue = item.Value;
        }

        private void CargarResultado()
        {
            if (_seleccion == null) return;

            _resultado = Contenido.Datos.ObtenerOpcionesEncuestaAdmin(
                Sesion.CodigoUsuario, _seleccion.Codigo);

            rptResultado.DataSource = _resultado;
            rptResultado.DataBind();
        }

        private EncuestaAdmin BuscarEncuesta(int codigo)
        {
            if (codigo <= 0 || _encuestas == null) return null;

            foreach (EncuestaAdmin e in _encuestas)
            {
                if (e.Codigo == codigo) return e;
            }

            return null;
        }

        private void Cerrar()
        {
            Session.Remove(ClaveModo);
            Session.Remove(ClaveCodigo);
            Session.Remove(ClaveAccion);

            phForm.Visible = false;
            phEstado.Visible = false;
            phResultado.Visible = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Cerrar();
        }

        // ----------------------------------------------------- Guardado

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int codigo = Session[ClaveCodigo] == null ? 0 : Convert.ToInt32(Session[ClaveCodigo]);

            DateTime inicio;
            if (!DateTime.TryParse(txtInicio.Text.Trim(), CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out inicio))
            {
                MostrarMensaje("Indicá la fecha en la que la encuesta abre.", false);
                return;
            }

            ResultadoGuardado r = Contenido.Datos.GuardarEncuesta(
                Sesion.CodigoUsuario, codigo,
                NumeroDe(ddlCampana),
                txtPregunta.Text.Trim(),
                txtDescripcion.Text.Trim(),
                NumeroDe(ddlCategoria),
                inicio,
                txtCierre.Text.Trim(),
                txtOpciones.Text);

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarEncuestas();
        }

        private static int NumeroDe(DropDownList lista)
        {
            int valor;
            return int.TryParse(lista.SelectedValue, out valor) ? valor : 0;
        }

        protected void btnConfirmarEstado_Click(object sender, EventArgs e)
        {
            if (_seleccion == null)
            {
                MostrarMensaje("Elegí primero la encuesta.", false);
                return;
            }

            Resultado r = Contenido.Datos.CambiarEstadoEncuesta(
                Sesion.CodigoUsuario, _seleccion.Codigo, Accion, txtMotivo.Text.Trim());

            MostrarMensaje(r.Mensaje, r.Ok);

            if (!r.Ok) return;

            Cerrar();
            CargarEncuestas();
        }

        // ------------------------------------------------- Presentación

        private string Accion
        {
            get { return Convert.ToString(Session[ClaveAccion]); }
        }

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
                int n = _encuestas == null ? 0 : _encuestas.Count;
                return Vista.Plural(n, "encuesta", "encuestas");
            }
        }

        protected string TituloForm
        {
            get { return _seleccion == null ? "Registrar encuesta" : "Editar encuesta"; }
        }

        protected string TituloEstado
        {
            get
            {
                switch (Accion)
                {
                    case "cerrar": return "Cerrar encuesta";
                    case "reabrir": return "Reabrir encuesta";
                    case "retirar": return "Retirar encuesta";
                    case "restaurar": return "Restaurar encuesta";
                    default: return string.Empty;
                }
            }
        }

        protected string TextoEstado
        {
            get
            {
                switch (Accion)
                {
                    case "cerrar":
                        return "Deja de admitir respuestas y el resultado pasa a verse sin necesidad de "
                             + "haber participado. La encuesta sigue consultable.";

                    case "reabrir":
                        return "Vuelve a admitir respuestas. Quien ya respondió conserva la suya y puede "
                             + "cambiarla.";

                    case "retirar":
                        return "Desaparece del sitio público. Las respuestas se conservan y vos la seguís "
                             + "viendo desde acá: retirar no es borrar.";

                    case "restaurar":
                        return "Vuelve al sitio público con las respuestas que ya tenía.";

                    default:
                        return string.Empty;
                }
            }
        }

        protected string ResumenResultado
        {
            get { return _seleccion == null ? string.Empty : _seleccion.VotosTexto; }
        }

        /// <summary>
        /// Proporción sobre el total de respuestas. Se calcula acá y no viaja
        /// desde la base por la misma razón por la que los contadores no se
        /// guardan: un porcentaje almacenado puede terminar contradiciendo a
        /// las filas que lo sostienen.
        /// </summary>
        protected string Proporcion(object dato)
        {
            OpcionEncuesta o = (OpcionEncuesta)dato;

            int total = _seleccion == null ? 0 : _seleccion.Votos;
            if (total <= 0) return "—";

            return o.Porcentaje(total) + " %";
        }

        /// <summary>
        /// Recorta la hora de una fecha que viaja como texto, porque el campo
        /// del formulario es de tipo fecha y no acepta la hora.
        /// </summary>
        private static string SoloFecha(string fecha)
        {
            if (string.IsNullOrEmpty(fecha)) return string.Empty;

            DateTime leida;
            if (!DateTime.TryParse(fecha, out leida)) return string.Empty;

            return leida.ToString("yyyy-MM-dd");
        }
    }
}
