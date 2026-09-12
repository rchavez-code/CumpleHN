using System;
using System.Web.UI;
using frontend.Modelos;

namespace frontend.Controles
{
    /// <summary>
    /// Tarjeta de una iniciativa ciudadana, con su barra de participación y el
    /// hilo de comentarios desplegable. Calcada de <see cref="PublicacionCard"/>:
    /// cambia el tipo de objeto y la cabecera, y el módulo de interacción hace
    /// el resto.
    ///
    /// La misma tarjeta sirve en la portada y en «Mi cuenta». Lo que cada
    /// página agrega alrededor (editar, retirar) es de la página, no de la
    /// tarjeta, para que lo público y lo privado no se mezclen en un control.
    ///
    /// La página que la use debe enlazar su repetidor en cada carga, incluidos
    /// los postbacks. Si solo enlaza cuando no hay postback, la tarjeta se
    /// queda sin <see cref="Item"/> al votar o comentar.
    /// </summary>
    public partial class IniciativaCard : UserControl
    {
        private Iniciativa _item;

        public Iniciativa Item
        {
            get { return _item; }
            set
            {
                _item = value;
                Configurar();
            }
        }

        private void Configurar()
        {
            if (_item == null) return;

            inter.TipoObjeto = TiposObjeto.Iniciativa;
            inter.CodigoObjeto = _item.Id;
            inter.MeGusta = _item.MeGusta;
            inter.NoMeGusta = _item.NoMeGusta;
            inter.Comentarios = _item.Comentarios;

            coments.TipoObjeto = TiposObjeto.Iniciativa;
            coments.CodigoObjeto = _item.Id;
        }

        protected override void OnInit(EventArgs e)
        {
            inter.ComentarClic += Inter_ComentarClic;
            coments.ComentarioPublicado += Coments_ComentarioPublicado;
            base.OnInit(e);
        }

        private void Inter_ComentarClic(object sender, EventArgs e)
        {
            phComentarios.Visible = !phComentarios.Visible;

            if (phComentarios.Visible) coments.Cargar();
        }

        private void Coments_ComentarioPublicado(object sender, EventArgs e)
        {
            inter.Refrescar();
        }

        /// <summary>
        /// Clases del artículo. El modificador de categoría pinta el filo
        /// izquierdo, para que la tarjeta tenga identidad aunque todavía no
        /// tenga ni un voto.
        /// </summary>
        protected string ClaseTarjeta
        {
            get
            {
                string clase = "gc-card gc-post gc-inic " + Item.ClaseCategoriaTarjeta;
                if (Item.Retirada) clase += " is-retirada";
                return clase;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Visible = Item != null;

            if (Item != null && Item.Retirada)
            {
                phRetirada.Visible = true;
                litMotivo.Text = Server.HtmlEncode(Item.MotivoBaja);
            }

            base.OnPreRender(e);
        }
    }
}
