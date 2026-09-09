using System;
using System.Web.UI;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Controles
{
    /// <summary>
    /// Barra de participación: me gusta, no me gusta y comentarios.
    ///
    /// Los contadores se ven siempre, con cuenta o sin ella, porque el interés
    /// del sitio es que cualquier persona pueda consultar la opinión acumulada.
    /// Al intentar votar sin sesión se redirige al acceso, y desde ahí se
    /// regresa a la misma página.
    ///
    /// El estado del objeto vive en ViewState y no en las propiedades enlazadas,
    /// para que el control siga sabiendo a qué objeto pertenece cuando el
    /// contenedor no se vuelve a enlazar en el postback.
    /// </summary>
    public partial class Interaccion : UserControl
    {
        /// <summary>Tipo de objeto valorado. Ver <see cref="TiposObjeto"/>.</summary>
        public string TipoObjeto
        {
            get { return Convert.ToString(ViewState["tipo"]); }
            set { ViewState["tipo"] = value; }
        }

        public int CodigoObjeto
        {
            get { return ViewState["codigo"] == null ? 0 : (int)ViewState["codigo"]; }
            set { ViewState["codigo"] = value; }
        }

        /// <summary>Contadores iniciales, tomados de la consulta del listado.</summary>
        public int MeGusta
        {
            get { return ViewState["mg"] == null ? 0 : (int)ViewState["mg"]; }
            set { ViewState["mg"] = value; }
        }

        public int NoMeGusta
        {
            get { return ViewState["nmg"] == null ? 0 : (int)ViewState["nmg"]; }
            set { ViewState["nmg"] = value; }
        }

        public int Comentarios
        {
            get { return ViewState["com"] == null ? 0 : (int)ViewState["com"]; }
            set { ViewState["com"] = value; }
        }

        /// <summary>Voto del usuario en sesión: 1, -1 o 0.</summary>
        private int MiValoracion
        {
            get { return ViewState["mv"] == null ? 0 : (int)ViewState["mv"]; }
            set { ViewState["mv"] = value; }
        }

        /// <summary>
        /// Cuando se define, el botón de comentar es un enlace a esa dirección en
        /// lugar de un botón que despliega el hilo en el mismo lugar. Sirve para
        /// las tarjetas de listado, donde el hilo completo no cabe.
        /// </summary>
        public string UrlComentarios { get; set; }

        /// <summary>Oculta la barra de porcentaje de apoyo.</summary>
        public bool MostrarApoyo { get; set; }

        /// <summary>
        /// Se dispara al pulsar comentar cuando no hay <see cref="UrlComentarios"/>,
        /// para que el contenedor despliegue u oculte el hilo.
        /// </summary>
        public event EventHandler ComentarClic;

        public Interaccion()
        {
            MostrarApoyo = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo se consulta el voto propio cuando hay sesión. Para un
            // visitante anónimo los contadores del listado ya alcanzan, y así
            // no se hace una llamada al servicio por cada tarjeta.
            if (!IsPostBack && Sesion.Autenticado && CodigoObjeto > 0)
            {
                Refrescar();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            // Con la participación apagada, los contadores siguen a la vista y
            // los botones de votar desaparecen. Lo que ya se registró es un
            // dato de la plataforma: esconderlo sería reescribir el pasado, y
            // el tablero seguiría contándolo.
            if (!Modulos.Visible(Modulos.Interaccion))
            {
                lnkMeGusta.Enabled = false;
                lnkNoMeGusta.Enabled = false;
                lnkMeGusta.ToolTip = "La participación está temporalmente cerrada";
                lnkNoMeGusta.ToolTip = lnkMeGusta.ToolTip;
            }

            bool comoEnlace = !string.IsNullOrEmpty(UrlComentarios);

            lnkComentar.Visible = !comoEnlace;
            lnkComentarEnlace.Visible = comoEnlace;

            if (comoEnlace) lnkComentarEnlace.NavigateUrl = ResolveUrl(UrlComentarios);

            // El estado activo del voto propio se marca con una clase.
            lnkMeGusta.CssClass = MiValoracion == 1 ? "gc-act gc-act--si is-on" : "gc-act gc-act--si";
            lnkNoMeGusta.CssClass = MiValoracion == -1 ? "gc-act gc-act--no is-on" : "gc-act gc-act--no";

            phApoyo.Visible = MostrarApoyo && (MeGusta + NoMeGusta) > 0;

            base.OnPreRender(e);
        }

        /// <summary>Vuelve a leer los contadores y el voto propio.</summary>
        public void Refrescar()
        {
            Modelos.Interaccion i = Contenido.Datos.ObtenerInteraccion(
                TipoObjeto, CodigoObjeto, Sesion.CodigoUsuario);

            MeGusta = i.MeGusta;
            NoMeGusta = i.NoMeGusta;
            Comentarios = i.Comentarios;
            MiValoracion = i.MiValoracion;
        }

        protected void lnkMeGusta_Click(object sender, EventArgs e)
        {
            Votar(1);
        }

        protected void lnkNoMeGusta_Click(object sender, EventArgs e)
        {
            Votar(-1);
        }

        private void Votar(int valor)
        {
            if (!Sesion.Autenticado)
            {
                Response.Redirect(Sesion.UrlAccesoDeVuelta());
                return;
            }

            Resultado r = Contenido.Datos.Valorar(
                TipoObjeto, CodigoObjeto, Sesion.CodigoUsuario, valor);

            if (!r.Ok)
            {
                litAviso.Text = Server.HtmlEncode(r.Mensaje);
                phAviso.Visible = true;
                return;
            }

            phAviso.Visible = false;
            Refrescar();
        }

        protected void lnkComentar_Click(object sender, EventArgs e)
        {
            if (ComentarClic != null) ComentarClic(this, EventArgs.Empty);
        }

        // ---------------------------------------------------- Presentación

        protected string TextoMeGusta
        {
            get { return Vista.Numero(MeGusta); }
        }

        protected string TextoNoMeGusta
        {
            get { return Vista.Numero(NoMeGusta); }
        }

        protected string TextoComentarios
        {
            get { return Vista.Numero(Comentarios); }
        }

        protected string TextoApoyo
        {
            get
            {
                int total = MeGusta + NoMeGusta;
                if (total == 0) return string.Empty;

                int pct = (int)Math.Round((MeGusta * 100.0) / total);
                return pct + " % de apoyo";
            }
        }

        protected string EstiloApoyo
        {
            get
            {
                int total = MeGusta + NoMeGusta;
                if (total == 0) return "width:0";

                int pct = (int)Math.Round((MeGusta * 100.0) / total);
                return "width:" + pct + "%";
            }
        }
    }
}
