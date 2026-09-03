using System;

namespace backend.Modelos
{
    /// <summary>
    /// Resultado de una acción de administración.
    ///
    /// Es la misma forma que devuelven los procedimientos del script 09: una
    /// fila con <c>ok</c> y <c>mensaje</c>. El Web Service la transporta tal
    /// cual, sin reescribir el texto, para que el motivo del rechazo quede
    /// documentado en un solo lugar — la base.
    /// </summary>
    public class RespuestaAdmin
    {
        public bool ok { get; set; }
        public string mensaje { get; set; }
    }

    /// <summary>
    /// Una fila de la bandeja de verificación. Puede ser una candidatura, una
    /// propuesta o una publicación: los tres tipos que llevan nivel de
    /// verificación llegan en la misma cola de trabajo.
    /// </summary>
    public class ItemVerificacion
    {
        public string tipoObjeto { get; set; }
        public int codigoObjeto { get; set; }

        public string titulo { get; set; }
        public string resumen { get; set; }

        /* Slug del objeto cuando lo tiene (candidaturas). En propuestas y
           publicaciones llega el código como texto, que es lo que sirve para
           armar el enlace a su página pública. */
        public string slug { get; set; }

        public string candidato { get; set; }
        public string candidatoSlug { get; set; }
        public string campanaSlug { get; set; }

        public int codigoVerificacion { get; set; }
        public string verificacion { get; set; }
        public int verificacionOrden { get; set; }

        public DateTime fecha { get; set; }
    }

    /// <summary>
    /// Publicación vista desde la moderación. A diferencia de
    /// <see cref="Publicacion"/>, incluye las retiradas y los datos del retiro:
    /// sin poder verlas, un retiro por error sería irreversible en la práctica.
    /// </summary>
    public class PublicacionModerada
    {
        public int codigoPublicacion { get; set; }
        public string texto { get; set; }
        public DateTime fecha { get; set; }

        public bool activo { get; set; }
        public string motivoBaja { get; set; }
        public string retiradaPor { get; set; }

        public string candidato { get; set; }
        public string candidatoSlug { get; set; }
        public string campanaSlug { get; set; }
        public string categoria { get; set; }
        public string verificacion { get; set; }

        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }
    }

    /// <summary>
    /// Una línea de la bitácora de administración.
    /// </summary>
    public class RegistroAuditoria
    {
        public int codigoAuditoria { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; }
        public string accion { get; set; }
        public string tipoObjeto { get; set; }
        public int codigoObjeto { get; set; }
        public string detalle { get; set; }
        public string motivo { get; set; }
    }
}
