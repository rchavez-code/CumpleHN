using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using frontend.Modelos;
using frontend.Servicios;

namespace frontend.Controles
{
    /// <summary>
    /// Encuesta de percepción en la portada.
    ///
    /// La tarjeta tiene dos caras y la misma lista sirve para las dos: mientras
    /// la persona no ha respondido, cada opción es un botón. Después de
    /// responder —o si la encuesta ya cerró— las mismas filas muestran el
    /// reparto. Se hace con un solo repetidor y no con dos bloques porque las
    /// opciones no cambian de lugar al votar, y verlas moverse rompería la
    /// relación entre lo que se eligió y lo que salió.
    ///
    /// El reparto solo llega cuando corresponde mostrarlo. Lo decide el
    /// procedimiento almacenado, no esta clase: mientras la votación sigue
    /// abierta y la persona no respondió, los conteos vienen en cero.
    ///
    /// La página que use esta tarjeta debe enlazarla en cada carga, también en
    /// los postbacks, o el clic sobre una opción llega sin modelo.
    /// </summary>
    public partial class EncuestaCard : UserControl
    {
        private Encuesta _item;

        public Encuesta Item
        {
            get { return _item; }
            set
            {
                _item = value;
                Enlazar();
            }
        }

        /// <summary>
        /// Se dispara después de registrar una respuesta, para que la página
        /// pueda refrescar lo que dependa de la encuesta.
        /// </summary>
        public event EventHandler Respondida;

        private void Enlazar()
        {
            if (_item == null) return;

            rptOpciones.DataSource = _item.Opciones;
            rptOpciones.DataBind();

            litPie.Text = Server.HtmlEncode(TextoPie);
        }

        protected override void OnPreRender(EventArgs e)
        {
            Visible = _item != null;
            base.OnPreRender(e);
        }

        // ------------------------------------------------------- Respuesta

        protected void rptOpciones_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "votar" || _item == null) return;

            int opcion;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out opcion)) return;

            // Sin sesión se pide acceso conservando el destino, igual que al
            // intentar valorar o comentar. Pedir cuenta no debería costarle a
            // nadie perder dónde estaba.
            if (!Sesion.Autenticado)
            {
                Response.Redirect(Sesion.UrlAccesoDeVuelta());
                return;
            }

            ResultadoEncuesta r = Contenido.Datos.ResponderEncuesta(
                _item.Id, opcion, Sesion.CodigoUsuario);

            // El resultado llega también cuando la respuesta se rechazó — si la
            // encuesta cerró mientras la persona la tenía abierta, ver el
            // reparto explica el rechazo mejor que el mensaje solo.
            if (r.Opciones.Count > 0)
            {
                _item.Opciones = r.Opciones;
                _item.Votos = r.Votos;
                _item.MiOpcion = OpcionElegida(r);
            }

            litAviso.Text = Server.HtmlEncode(r.Mensaje);
            phAviso.Visible = !string.IsNullOrEmpty(r.Mensaje);
            phAviso.Attributes["class"] = r.Ok ? "gc-ok gc-enc__aviso" : "gc-alert gc-enc__aviso";

            Enlazar();

            if (r.Ok && Respondida != null) Respondida(this, EventArgs.Empty);
        }

        private static int OpcionElegida(ResultadoEncuesta r)
        {
            foreach (OpcionEncuesta o in r.Opciones)
            {
                if (o.MiVoto) return o.Id;
            }

            return 0;
        }

        // ---------------------------------------------------- Presentación

        /// <summary>
        /// Si las opciones siguen siendo botones. Una encuesta cerrada, o el
        /// módulo apagado, dejan la lista como puro resultado.
        ///
        /// Un visitante sin cuenta sí puede pulsar: el clic lo lleva al acceso
        /// y lo devuelve acá. Deshabilitar los botones para quien no tiene
        /// sesión escondería que la encuesta admite participación.
        /// </summary>
        protected bool PuedeResponder
        {
            get
            {
                return _item != null
                    && _item.EstaAbierta
                    && Modulos.Habilitado(Modulos.Encuestas);
            }
        }

        /// <summary>Si las filas muestran el reparto en lugar de invitar a elegir.</summary>
        private bool Resuelta
        {
            get { return _item != null && _item.Revelado; }
        }

        /// <summary>
        /// Cuántos colores tiene la paleta de opciones. Coincide con las clases
        /// gc-enc__op--c1 a c8 de la hoja de estilos, y con el máximo de
        /// opciones que acepta el procedimiento de guardado, de modo que
        /// ninguna encuesta llegue a repetir color.
        /// </summary>
        private const int ColoresDisponibles = 8;

        /// <summary>
        /// Clase de la fila, con su color asignado por posición.
        ///
        /// El color va por clase y no por estilo en línea para que la paleta
        /// viva entera en la hoja de estilos: si algún día hay que revisarla
        /// por contraste, se revisa en un solo archivo.
        /// </summary>
        protected string ClaseOpcion(object dato, int indice)
        {
            OpcionEncuesta o = (OpcionEncuesta)dato;

            string clase = "gc-enc__op gc-enc__op--c" + ((indice % ColoresDisponibles) + 1);

            if (Resuelta) clase += " is-res";
            if (o.MiVoto) clase += " is-mia";

            return clase;
        }

        /// <summary>
        /// Texto alternativo de la fila. Repite en palabras lo que la barra
        /// dice en color, porque el color no lo lee un lector de pantalla.
        /// </summary>
        protected string TituloOpcion(object dato)
        {
            OpcionEncuesta o = (OpcionEncuesta)dato;

            if (!Resuelta) return "Elegir: " + o.Texto;

            string cuenta = Vista.Plural(o.Votos, "respuesta", "respuestas");
            string propia = o.MiVoto ? ", tu respuesta" : string.Empty;

            return o.Texto + ": " + cuenta + ", " + o.Porcentaje(_item.Votos) + " por ciento" + propia;
        }

        /// <summary>
        /// Ancho de la barra de la opción. Va en el estilo y no en una clase
        /// porque es un valor continuo: una clase por cada porcentaje serían
        /// ciento un reglas para dibujar una sola cosa.
        /// </summary>
        protected string EstiloPista(object dato)
        {
            if (!Resuelta) return "width:0";

            OpcionEncuesta o = (OpcionEncuesta)dato;
            return "width:" + o.Porcentaje(_item.Votos) + "%";
        }

        /// <summary>
        /// Cada barra lleva su cifra al lado, en columna propia. Ninguna
        /// depende de pasar el mouse por encima, que es la regla que sigue
        /// también el tablero.
        ///
        /// Van el conteo y el porcentaje, no uno de los dos. El porcentaje
        /// solo esconde de cuánta gente sale —el sesenta por ciento de cinco
        /// respuestas no es el sesenta por ciento de quinientas— y el conteo
        /// solo obliga a dividir de cabeza para comparar dos opciones.
        /// </summary>
        protected string TextoResultado(object dato)
        {
            if (!Resuelta) return string.Empty;

            OpcionEncuesta o = (OpcionEncuesta)dato;

            return Vista.Numero(o.Votos) + " · " + o.Porcentaje(_item.Votos) + " %";
        }

        /// <summary>
        /// El pie dice de qué es y de qué no es evidencia este resultado.
        ///
        /// No es un adorno legal. Quien vota en la plataforma se eligió a sí
        /// mismo para participar, así que el reparto describe a quien
        /// respondió y a nadie más. Callarlo dejaría una cifra con apariencia
        /// de encuesta representativa, que es justo lo que este proyecto no
        /// debería producir.
        /// </summary>
        private string TextoPie
        {
            get
            {
                string aviso =
                    "Participan quienes deciden hacerlo, así que el resultado describe a "
                  + "las personas que respondieron y no a la población hondureña.";

                if (_item == null) return aviso;

                if (!_item.EstaAbierta)
                    return "Esta encuesta ya cerró. " + aviso;

                if (!Modulos.Habilitado(Modulos.Encuestas))
                    return "La participación está temporalmente cerrada. " + aviso;

                if (!Sesion.Autenticado)
                    return "Elegí una opción y te pedimos acceso para registrarla. " + aviso;

                if (_item.YaVoto)
                    return "Ya respondiste. Podés cambiar tu respuesta mientras siga abierta. " + aviso;

                return aviso;
            }
        }

        /// <summary>
        /// Marca la tarjeta cuando se está mostrando solo porque quien mira
        /// administra. Sin ese aviso, confundiría lo que ve él con lo que ve el
        /// resto, que es el error que vuelve inútil un interruptor.
        /// </summary>
        protected string ClaseOculta
        {
            get { return Modulos.OcultoAlPublico(Modulos.Encuestas) ? "gc-enc--oculta" : string.Empty; }
        }
    }
}
