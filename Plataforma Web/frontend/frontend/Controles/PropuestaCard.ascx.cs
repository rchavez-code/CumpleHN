using System;
using System.Web.UI;
using frontend.Modelos;

namespace frontend.Controles
{
    /// <summary>
    /// Tarjeta de propuesta o proyecto de campaña, con barra de participación.
    ///
    /// La página que la use debe enlazar su repetidor en cada carga, incluidos
    /// los postbacks.
    /// </summary>
    public partial class PropuestaCard : UserControl
    {
        private Propuesta _item;

        public Propuesta Item
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

            inter.TipoObjeto = TiposObjeto.Propuesta;
            inter.CodigoObjeto = _item.Id;
            inter.MeGusta = _item.MeGusta;
            inter.NoMeGusta = _item.NoMeGusta;
            inter.Comentarios = _item.Comentarios;

            // El hilo vive en la ficha de la propuesta.
            inter.UrlComentarios = _item.Url;
        }

        protected string Url
        {
            get { return ResolveUrl(Item.Url); }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Visible = Item != null;
            base.OnPreRender(e);
        }
    }
}
