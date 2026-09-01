using System;
using System.Web.UI;
using frontend.Modelos;

namespace frontend.Controles
{
    /// <summary>
    /// Publicación del feed de una campaña, con su barra de participación y el
    /// hilo de comentarios desplegable.
    ///
    /// La página que use esta tarjeta debe enlazar su repetidor en cada carga,
    /// incluidos los postbacks. Si solo enlaza cuando no hay postback, la
    /// tarjeta se queda sin <see cref="Item"/> al votar o comentar.
    /// </summary>
    public partial class PublicacionCard : UserControl
    {
        private Publicacion _item;

        public Publicacion Item
        {
            get { return _item; }
            set
            {
                _item = value;
                Configurar();
            }
        }

        /// <summary>
        /// Traslada los datos de la publicación a los controles de interacción.
        /// Se hace en el asignador y no en Page_Load porque el control de
        /// valoración necesita saber a qué objeto pertenece antes de su propio
        /// ciclo de carga.
        /// </summary>
        private void Configurar()
        {
            if (_item == null) return;

            inter.TipoObjeto = TiposObjeto.Publicacion;
            inter.CodigoObjeto = _item.Id;
            inter.MeGusta = _item.MeGusta;
            inter.NoMeGusta = _item.NoMeGusta;
            inter.Comentarios = _item.Comentarios;

            coments.TipoObjeto = TiposObjeto.Publicacion;
            coments.CodigoObjeto = _item.Id;
        }

        protected override void OnInit(EventArgs e)
        {
            inter.ComentarClic += Inter_ComentarClic;
            coments.ComentarioPublicado += Coments_ComentarioPublicado;
            base.OnInit(e);
        }

        /// <summary>Despliega u oculta el hilo, como en cualquier feed social.</summary>
        private void Inter_ComentarClic(object sender, EventArgs e)
        {
            phComentarios.Visible = !phComentarios.Visible;

            if (phComentarios.Visible) coments.Cargar();
        }

        /// <summary>Mantiene el contador de la barra al día tras comentar.</summary>
        private void Coments_ComentarioPublicado(object sender, EventArgs e)
        {
            inter.Refrescar();
        }

        protected string UrlCandidato
        {
            get { return ResolveUrl(Item.CandidatoUrl); }
        }

        protected string UrlPropuesta
        {
            get { return ResolveUrl("~/Propuesta?id=" + Item.PropuestaId); }
        }

        protected string Iniciales
        {
            get { return Item.TieneFoto ? string.Empty : Item.CandidatoIniciales; }
        }

        protected string EstiloAvatar
        {
            get { return Item.TieneFoto ? Vista.EstiloAvatar(ResolveUrl(Item.CandidatoFotoUrl)) : string.Empty; }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Visible = Item != null;
            base.OnPreRender(e);
        }
    }
}
