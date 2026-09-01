using System;

namespace frontend.Modelos
{
    /// <summary>
    /// Tipos de objeto sobre los que se puede opinar. Los valores tienen que
    /// coincidir exactamente con la tabla TiposObjeto de la base de datos, así
    /// que se declaran una sola vez acá y no se escriben a mano en las páginas.
    /// </summary>
    public static class TiposObjeto
    {
        public const string Publicacion = "Publicacion";
        public const string Candidato = "Candidato";
        public const string Partido = "Partido";
        public const string Propuesta = "Propuesta";
    }

    /// <summary>
    /// Participación acumulada sobre un objeto, más el voto de quien consulta.
    /// </summary>
    public class Interaccion
    {
        public string TipoObjeto { get; set; }
        public int CodigoObjeto { get; set; }

        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }
        public int Comentarios { get; set; }

        /// <summary>1 si dio me gusta, -1 si dio no me gusta, 0 si no votó o no tiene sesión.</summary>
        public int MiValoracion { get; set; }

        public bool VotoAFavor
        {
            get { return MiValoracion == 1; }
        }

        public bool VotoEnContra
        {
            get { return MiValoracion == -1; }
        }

        /// <summary>
        /// Balance entre apoyo y rechazo, en porcentaje de apoyo sobre el total
        /// de votos. Devuelve -1 cuando todavía nadie votó, para que la vista
        /// pueda distinguir «sin votos» de «cero por ciento de apoyo».
        /// </summary>
        public int PorcentajeApoyo
        {
            get
            {
                int total = MeGusta + NoMeGusta;
                if (total == 0) return -1;
                return (int)Math.Round((MeGusta * 100.0) / total);
            }
        }
    }

    /// <summary>
    /// Comentario de una persona sobre un objeto de la plataforma.
    /// </summary>
    public class Comentario
    {
        public int Id { get; set; }
        public int CodigoUsuario { get; set; }
        public string Autor { get; set; }
        public string Rol { get; set; }
        public DateTime Fecha { get; set; }
        public string Texto { get; set; }

        public string TiempoRelativo
        {
            get { return Vista.TiempoRelativo(Fecha); }
        }

        public string Iniciales
        {
            get { return Vista.InicialesDe(Autor); }
        }

        /// <summary>
        /// Distingue a una candidatura de un ciudadano en el hilo, porque no es
        /// lo mismo que opine un tercero que quien es dueño del contenido.
        /// </summary>
        public bool EsCandidato
        {
            get { return string.Equals(Rol, "Candidato", StringComparison.OrdinalIgnoreCase); }
        }
    }

    /// <summary>
    /// Partido político. Se modela como entidad propia para poder recibir
    /// valoraciones y comentarios igual que cualquier otro objeto.
    /// </summary>
    public class Partido
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Nombre { get; set; }
        public string Siglas { get; set; }
        public string Descripcion { get; set; }

        public int TotalCandidatos { get; set; }
        public int TotalPropuestas { get; set; }

        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }
        public int Comentarios { get; set; }

        public string SiglasONombre
        {
            get { return string.IsNullOrEmpty(Siglas) ? Nombre : Siglas; }
        }

        public string Url
        {
            get { return "~/Partido?id=" + Slug; }
        }
    }

    /// <summary>Resultado de registrar una valoración o un comentario.</summary>
    public class Resultado
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
    }
}
