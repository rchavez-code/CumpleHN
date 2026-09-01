using System;

namespace backend.Modelos
{
    /* ================================================================
       Objetos del módulo de interacción ciudadana.
       ================================================================ */

    /// <summary>
    /// Resumen de la participación sobre un objeto: cuántos me gusta y no me
    /// gusta acumula, cuántos comentarios tiene y qué votó el usuario que
    /// consulta.
    /// </summary>
    public class Interaccion
    {
        public string tipoObjeto { get; set; }
        public int codigoObjeto { get; set; }

        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }

        /// <summary>1 si el usuario dio me gusta, -1 si dio no me gusta, 0 si no votó.</summary>
        public int miValoracion { get; set; }
    }

    /// <summary>
    /// Comentario de una persona sobre un objeto de la plataforma.
    /// </summary>
    public class Comentario
    {
        public int codigoComentario { get; set; }
        public int codigoUsuario { get; set; }
        public string autor { get; set; }

        /// <summary>Rol de quien comenta. Permite distinguir a una candidatura de un ciudadano.</summary>
        public string rol { get; set; }

        public DateTime fecha { get; set; }
        public string texto { get; set; }
    }

    /// <summary>
    /// Resultado de registrar una valoración. Devuelve el estado actualizado
    /// para que el frontend no tenga que consultar de nuevo.
    /// </summary>
    public class RespuestaInteraccion
    {
        public bool ok { get; set; }
        public string mensaje { get; set; }
        public Interaccion interaccion { get; set; }
    }

    /// <summary>
    /// Resultado de publicar un comentario.
    /// </summary>
    public class RespuestaComentario
    {
        public bool ok { get; set; }
        public string mensaje { get; set; }
        public Comentario comentario { get; set; }
        public int total { get; set; }
    }

    /// <summary>
    /// Partido político. Se normalizó en su propia tabla para poder recibir
    /// valoraciones y comentarios como cualquier otro objeto.
    /// </summary>
    public class Partido
    {
        public int codigoPartido { get; set; }
        public string slug { get; set; }
        public string nombre { get; set; }
        public string siglas { get; set; }
        public string descripcion { get; set; }

        public int totalCandidatos { get; set; }
        public int totalPropuestas { get; set; }

        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }
    }
}
