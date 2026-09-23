using System;
using System.Collections.Generic;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend
{
    /// <summary>
    /// Historial de consultas al asistente y reporte exportable.
    ///
    /// Dos momentos en la misma página. Primero la persona ve sus consultas
    /// respondidas y marca las que quiere. Al generar, la misma dirección
    /// dibuja el reporte con la pregunta, la respuesta y la fecha de cada una,
    /// listo para imprimir o guardar como PDF desde el navegador.
    ///
    /// Por qué el PDF lo hace el navegador y no el servidor. Generarlo en el
    /// backend obligaba a sumar una librería de PDF, y la respuesta del modelo
    /// es HTML que ya tiene una sola manera segura de dibujarse: la lista
    /// blanca de cumplehn-respuesta-ia.js. Un generador de PDF tendría que
    /// interpretar ese mismo HTML con otras reglas, y serían dos garantías que
    /// mantener en lugar de una.
    ///
    /// Pertenece al módulo del asistente: con el módulo oculto la página
    /// tampoco se sirve. Exige además sesión, porque las consultas son de
    /// quien las hizo. Qué consultas son de quién lo decide la base, no esta
    /// página.
    /// </summary>
    public partial class ReporteAsistente : PaginaDeModulo
    {
        private HashSet<int> _elegidas = new HashSet<int>();
        private int _total;
        private int _incluidas;

        protected override string ModuloRequerido
        {
            get { return Modulos.AnaliticaAsistente; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!Sesion.Autenticado)
                Response.Redirect(Sesion.UrlAccesoDeVuelta());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // En el postback de generar se vuelve a dibujar la lista con lo que
            // la persona marcó, por si hay que mostrarle un error.
            if (IsPostBack) _elegidas = LeerElegidas();

            CargarLista();
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            if (_elegidas.Count == 0)
            {
                MostrarError("Marcá al menos una consulta para armar el reporte.");
                return;
            }

            IList<ConsultaAsistente> reporte = Contenido.Datos.ObtenerReporteAsistente(
                Sesion.CodigoUsuario, new List<int>(_elegidas));

            // Vacío con consultas marcadas es que el backend no respondió, o
            // que los códigos no eran de esta persona.
            if (reporte.Count == 0)
            {
                MostrarError("No se pudo armar el reporte. Intentá de nuevo en un momento.");
                return;
            }

            _incluidas = reporte.Count;

            rptReporte.DataSource = reporte;
            rptReporte.DataBind();

            phElegir.Visible = false;
            phReporte.Visible = true;
        }

        // =============================================================
        //  Auxiliares
        // =============================================================

        private void CargarLista()
        {
            IList<ConsultaAsistente> consultas =
                Contenido.Datos.ObtenerConsultasAsistente(Sesion.CodigoUsuario);

            _total = consultas.Count;

            phLista.Visible = _total > 0;
            phVacio.Visible = _total == 0;

            rptConsultas.DataSource = consultas;
            rptConsultas.DataBind();
        }

        /// <summary>
        /// Las casillas son inputs simples y no controles de servidor: para
        /// leer una lista de números no hace falta estado de vista. Lo que no
        /// sea un entero positivo se descarta acá, y lo que no sea de esta
        /// persona lo descarta la base.
        /// </summary>
        private HashSet<int> LeerElegidas()
        {
            HashSet<int> codigos = new HashSet<int>();
            string[] valores = Request.Form.GetValues("consulta");

            if (valores == null) return codigos;

            foreach (string v in valores)
            {
                int c;
                if (int.TryParse(v, out c) && c > 0) codigos.Add(c);
            }

            return codigos;
        }

        private void MostrarError(string mensaje)
        {
            litError.Text = Server.HtmlEncode(mensaje);
            phError.Visible = true;
        }

        protected bool Elegida(int codigo)
        {
            return _elegidas.Contains(codigo);
        }

        protected string TextoTotal
        {
            get { return _total == 1 ? "1 consulta respondida" : _total + " consultas respondidas"; }
        }

        protected string TextoIncluidas
        {
            get { return _incluidas == 1 ? "1 consulta" : _incluidas + " consultas"; }
        }

        protected string Generado
        {
            get { return Vista.FechaHora(DateTime.Now); }
        }
    }
}
