using System;

namespace frontend.Modelos
{
    /// <summary>
    /// Publicación de un candidato dentro del feed de una campaña.
    ///
    /// Lleva los datos del autor desnormalizados porque el feed se arma en una
    /// sola consulta y no debe resolver el candidato una vez por tarjeta.
    /// </summary>
    public class Publicacion
    {
        public Publicacion()
        {
            Verificacion = NivelVerificacion.Declarado;
        }

        public int Id { get; set; }

        public string CampanaSlug { get; set; }

        // ---------------------------------------------------------- Autor

        public string CandidatoSlug { get; set; }

        public string CandidatoNombre { get; set; }

        public string CandidatoCargo { get; set; }

        public string CandidatoFotoUrl { get; set; }

        public string CandidatoIniciales { get; set; }

        // ------------------------------------------------------ Contenido

        public DateTime Fecha { get; set; }

        public string Texto { get; set; }

        public string ImagenUrl { get; set; }

        /// <summary>Categoría temática cuando la publicación aborda un área concreta.</summary>
        public string Categoria { get; set; }

        /// <summary>Propuesta a la que hace referencia, si corresponde.</summary>
        public int PropuestaId { get; set; }

        public string PropuestaNombre { get; set; }

        public NivelVerificacion Verificacion { get; set; }

        // ------------------------------------------------- Interacciones

        /// <summary>
        /// Contadores derivados de las tablas de interacción. Alimentan la
        /// tarjeta del feed sin tener que consultar cada publicación aparte.
        /// </summary>
        public int MeGusta { get; set; }

        public int NoMeGusta { get; set; }

        public int Comentarios { get; set; }

        // --------------------------------------------------- Presentación

        public string TiempoRelativo
        {
            get { return Vista.TiempoRelativo(Fecha); }
        }

        public bool TieneImagen
        {
            get { return !string.IsNullOrEmpty(ImagenUrl); }
        }

        public bool TieneFoto
        {
            get { return !string.IsNullOrEmpty(CandidatoFotoUrl); }
        }

        public bool TienePropuesta
        {
            get { return PropuestaId > 0 && !string.IsNullOrEmpty(PropuestaNombre); }
        }

        public string CategoriaClase
        {
            get { return Vista.ClaseCategoria(Categoria); }
        }

        public string VerificacionTexto
        {
            get { return Vista.TextoVerificacion(Verificacion); }
        }

        public string VerificacionClase
        {
            get { return Vista.ClaseVerificacion(Verificacion); }
        }

        public string CandidatoUrl
        {
            get { return "~/Candidato?id=" + CandidatoSlug; }
        }
    }
}
