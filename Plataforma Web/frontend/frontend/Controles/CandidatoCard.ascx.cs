using System;
using System.Web.UI;
using frontend.Modelos;

namespace frontend.Controles
{
    /// <summary>
    /// Tarjeta de candidato para listados, con barra de participación.
    ///
    /// La tarjeta dejó de ser un enlace completo para poder alojar botones
    /// propios: un botón dentro de un enlace no es marcado válido y el clic
    /// quedaría disputado entre los dos. Ahora el enlace está en la fotografía
    /// y en el nombre.
    ///
    /// La página que la use debe enlazar su repetidor en cada carga, incluidos
    /// los postbacks.
    /// </summary>
    public partial class CandidatoCard : UserControl
    {
        private Candidato _item;

        public Candidato Item
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

            inter.TipoObjeto = TiposObjeto.Candidato;
            inter.CodigoObjeto = _item.Id;
            inter.MeGusta = _item.MeGusta;
            inter.NoMeGusta = _item.NoMeGusta;
            inter.Comentarios = _item.Comentarios;

            // El hilo completo no cabe en una tarjeta de listado, así que el
            // botón de comentar lleva al perfil, donde sí está.
            inter.UrlComentarios = _item.Url + "&t=opiniones";
        }

        protected string Url
        {
            get { return ResolveUrl(Item.Url); }
        }

        protected string UrlPartido
        {
            get { return ResolveUrl(Item.UrlPartido); }
        }

        protected string Iniciales
        {
            get { return Item.TieneFoto ? string.Empty : Item.Iniciales; }
        }

        protected string EstiloAvatar
        {
            get { return Item.TieneFoto ? Vista.EstiloAvatar(ResolveUrl(Item.FotoUrl)) : string.Empty; }
        }

        protected string TextoPropuestas
        {
            get { return Item.TotalPropuestas == 1 ? "propuesta" : "propuestas"; }
        }

        protected string TextoPublicaciones
        {
            get { return Item.TotalPublicaciones == 1 ? "publicación" : "publicaciones"; }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Visible = Item != null;
            base.OnPreRender(e);
        }
    }
}
