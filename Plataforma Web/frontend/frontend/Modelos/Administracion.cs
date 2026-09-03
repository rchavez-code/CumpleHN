using System;

namespace frontend.Modelos
{
    /// <summary>
    /// Niveles de verificación, tal como los guarda el catálogo
    /// NivelesVerificacion. Se declaran una sola vez acá para que ninguna
    /// página compare contra una cadena escrita a mano.
    /// </summary>
    public static class NivelesVerificacion
    {
        public const string Declarado = "Declarado";
        public const string EnRevision = "En revisión";
        public const string Verificado = "Verificado";
    }

    /// <summary>
    /// Par código y nombre de una entrada de catálogo.
    ///
    /// Los catálogos de las páginas públicas devuelven solo el nombre, porque
    /// allá se filtra por texto. Acá hace falta el código: la verificación se
    /// guarda por código, no por la etiqueta que se lee en pantalla.
    /// </summary>
    public class OpcionCatalogo
    {
        public int Codigo { get; set; }
        public string Nombre { get; set; }
    }

    /// <summary>
    /// Una fila de la bandeja de verificación. Puede ser una candidatura, una
    /// propuesta o una publicación: los tres tipos que llevan nivel de
    /// verificación llegan en la misma cola de trabajo, ordenados por lo más
    /// antiguo sin verificar.
    /// </summary>
    public class ItemVerificacion
    {
        public string TipoObjeto { get; set; }
        public int CodigoObjeto { get; set; }

        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Slug { get; set; }

        public string Candidato { get; set; }
        public string CandidatoSlug { get; set; }
        public string CampanaSlug { get; set; }

        public int CodigoVerificacion { get; set; }
        public string Verificacion { get; set; }
        public int VerificacionOrden { get; set; }

        public DateTime Fecha { get; set; }

        public bool EstaVerificado
        {
            get { return VerificacionOrden >= 3; }
        }

        /// <summary>Clase del distintivo, reutilizando la del resto del sitio.</summary>
        public string VerificacionClase
        {
            get { return Vista.ClaseVerificacion(Vista.NivelDe(Verificacion)); }
        }

        public string FechaTexto
        {
            get { return Vista.FechaCorta(Fecha); }
        }

        /// <summary>
        /// Nombre del tipo para mostrar. La base lo guarda sin tilde porque es
        /// una clave del catálogo, no un texto de pantalla.
        /// </summary>
        public string TipoTexto
        {
            get
            {
                if (TipoObjeto == TiposObjeto.Publicacion) return "Publicación";
                if (TipoObjeto == TiposObjeto.Propuesta) return "Propuesta";
                if (TipoObjeto == TiposObjeto.Candidato) return "Candidatura";
                return TipoObjeto;
            }
        }

        /// <summary>
        /// Dirección de la página pública del contenido, para poder revisarlo
        /// antes de decidir. Verificar sin leer no sería verificar.
        /// </summary>
        public string Url
        {
            get
            {
                if (TipoObjeto == TiposObjeto.Candidato)
                    return "~/Candidato?id=" + CandidatoSlug;

                if (TipoObjeto == TiposObjeto.Propuesta)
                    return "~/Propuesta?id=" + CodigoObjeto;

                // Las publicaciones no tienen página propia: se leen en el
                // perfil de la candidatura que las publicó.
                return "~/Candidato?id=" + CandidatoSlug;
            }
        }
    }

    /// <summary>
    /// Publicación vista desde la moderación. A diferencia de las del feed,
    /// incluye las retiradas con el motivo y el responsable del retiro: sin
    /// poder verlas, un retiro por error sería irreversible en la práctica.
    /// </summary>
    public class PublicacionModerada
    {
        public int CodigoPublicacion { get; set; }
        public string Texto { get; set; }
        public DateTime Fecha { get; set; }

        public bool Activa { get; set; }
        public string MotivoBaja { get; set; }
        public string RetiradaPor { get; set; }

        public string Candidato { get; set; }
        public string CandidatoSlug { get; set; }
        public string CampanaSlug { get; set; }
        public string Categoria { get; set; }
        public string Verificacion { get; set; }

        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }
        public int Comentarios { get; set; }

        public string FechaTexto
        {
            get { return Vista.FechaCorta(Fecha); }
        }

        public string VerificacionClase
        {
            get { return Vista.ClaseVerificacion(Vista.NivelDe(Verificacion)); }
        }

        public bool TieneCategoria
        {
            get { return !string.IsNullOrEmpty(Categoria); }
        }

        /// <summary>
        /// Participación acumulada. Se muestra junto a la publicación porque
        /// retirar contenido con mucha participación no es lo mismo que
        /// retirar algo que nadie leyó, y quien modera debería saberlo antes
        /// de decidir.
        /// </summary>
        public int Participacion
        {
            get { return MeGusta + NoMeGusta + Comentarios; }
        }

        public string Extracto
        {
            get
            {
                if (string.IsNullOrEmpty(Texto)) return string.Empty;
                return Texto.Length <= 220 ? Texto : Texto.Substring(0, 220) + "…";
            }
        }
    }

    /// <summary>
    /// Una línea de la bitácora de administración.
    /// </summary>
    public class RegistroAuditoria
    {
        public int Codigo { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string TipoObjeto { get; set; }
        public int CodigoObjeto { get; set; }
        public string Detalle { get; set; }
        public string Motivo { get; set; }

        public string FechaTexto
        {
            get { return Vista.FechaHora(Fecha); }
        }

        /// <summary>
        /// Acción en texto de pantalla. La base la guarda sin tildes porque es
        /// una clave de la restricción CHECK.
        /// </summary>
        public string AccionTexto
        {
            get
            {
                if (Accion == "Verificacion") return "Verificación";
                if (Accion == "Retiro") return "Retiro";
                if (Accion == "Restauracion") return "Restauración";
                return Accion;
            }
        }

        /// <summary>
        /// Color según lo que hizo la acción: quitar contenido de la vista
        /// pública se distingue de devolverlo o de verificarlo.
        /// </summary>
        public string AccionClase
        {
            get
            {
                if (Accion == "Retiro") return "gc-chip gc-chip--incumplida";
                if (Accion == "Restauracion") return "gc-chip gc-chip--cumplida";
                return "gc-chip gc-chip--declarada";
            }
        }
    }
}
