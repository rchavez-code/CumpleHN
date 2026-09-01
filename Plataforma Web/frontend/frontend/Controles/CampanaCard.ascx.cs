using System;
using System.Web.UI;
using frontend.Modelos;

namespace frontend.Controles
{
    /// <summary>
    /// Tarjeta de campaña electoral para listados.
    /// </summary>
    public partial class CampanaCard : UserControl
    {
        public Campana Item { get; set; }

        protected string Url
        {
            get { return ResolveUrl(Item.Url); }
        }

        protected string FechaEleccion
        {
            get { return Vista.FechaCorta(Item.FechaEleccion); }
        }

        protected string TextoCandidatos
        {
            get { return Vista.Plural(Item.TotalCandidatos, "candidato", "candidatos"); }
        }

        protected string TextoPropuestas
        {
            get { return Vista.Plural(Item.TotalPropuestas, "propuesta", "propuestas"); }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Visible = Item != null;
            base.OnPreRender(e);
        }
    }
}
