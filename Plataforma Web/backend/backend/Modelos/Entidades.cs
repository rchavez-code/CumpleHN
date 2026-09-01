using System;

namespace backend.Modelos
{
    /* ================================================================
       Objetos que viajan entre el backend y el frontend.

       Requisitos para que ASMX los serialice, tanto en SOAP como en JSON:
       clase pública, constructor sin parámetros y propiedades públicas de
       lectura y escritura. Nada de campos privados ni de propiedades
       calculadas: lo que no se pueda serializar no llega al otro lado.
       ================================================================ */

    /// <summary>
    /// Campaña electoral. Entidad raíz: toda candidatura, propuesta y
    /// publicación pertenece a una campaña.
    /// </summary>
    public class Campana
    {
        public int codigoCampana { get; set; }
        public string slug { get; set; }
        public string nombre { get; set; }
        public string resumen { get; set; }
        public string descripcion { get; set; }
        public string alcance { get; set; }
        public DateTime fechaInicio { get; set; }
        public DateTime fechaEleccion { get; set; }
        public string estado { get; set; }
        public bool esActual { get; set; }

        /* Contadores derivados, calculados en la consulta. */
        public int totalCandidatos { get; set; }
        public int totalPropuestas { get; set; }
        public int totalPublicaciones { get; set; }
    }

    /// <summary>
    /// Candidato de una campaña. El nivel de gobierno se hereda del cargo.
    /// </summary>
    public class Candidato
    {
        public int codigoCandidato { get; set; }
        public string slug { get; set; }
        public int codigoCampana { get; set; }
        public string campanaSlug { get; set; }
        public string campanaNombre { get; set; }

        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string partido { get; set; }
        public string partidoSiglas { get; set; }
        public string partidoSlug { get; set; }
        public string cargo { get; set; }
        public string nivelGobierno { get; set; }
        public string departamento { get; set; }
        public string municipio { get; set; }

        public string fotoUrl { get; set; }
        public string titular { get; set; }
        public string biografia { get; set; }
        public string informacionProfesional { get; set; }
        public string descripcionCandidatura { get; set; }

        public string correoPublico { get; set; }
        public string telefono { get; set; }
        public string sitioWeb { get; set; }
        public string facebook { get; set; }
        public string x { get; set; }
        public string instagram { get; set; }

        public string verificacion { get; set; }
        public int totalPropuestas { get; set; }
        public int totalPublicaciones { get; set; }

        /* Participación ciudadana acumulada sobre el perfil. */
        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }
    }

    /// <summary>
    /// Proyecto de campaña. Es la misma entidad que la promesa política del
    /// marco teórico: nace declarada y después recibe un estado de
    /// cumplimiento respaldado por evidencia.
    /// </summary>
    public class Propuesta
    {
        public int codigoPropuesta { get; set; }
        public int codigoCandidato { get; set; }
        public string candidatoSlug { get; set; }
        public string candidatoNombre { get; set; }
        public string campanaSlug { get; set; }

        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string problema { get; set; }
        public string objetivo { get; set; }
        public string beneficiarios { get; set; }
        public string categoria { get; set; }
        public string ubicacion { get; set; }
        public string periodoEjecucion { get; set; }
        public string estado { get; set; }
        public string imagenUrl { get; set; }
        public string informacionAdicional { get; set; }
        public string verificacion { get; set; }
        public DateTime fechaRegistro { get; set; }

        /* Participación ciudadana acumulada sobre la propuesta. */
        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }
    }

    /// <summary>
    /// Publicación del feed. Lleva los datos del autor desnormalizados porque
    /// el feed se arma en una sola consulta.
    /// </summary>
    public class Publicacion
    {
        public int codigoPublicacion { get; set; }
        public string campanaSlug { get; set; }

        public int codigoCandidato { get; set; }
        public string candidatoSlug { get; set; }
        public string candidatoNombre { get; set; }
        public string candidatoCargo { get; set; }
        public string candidatoFotoUrl { get; set; }

        public DateTime fecha { get; set; }
        public string texto { get; set; }
        public string imagenUrl { get; set; }
        public string categoria { get; set; }
        public int codigoPropuesta { get; set; }
        public string propuestaNombre { get; set; }
        public string verificacion { get; set; }

        /* Se calculan desde Valoraciones y Comentarios. Antes eran columnas de
           la tabla Publicaciones, lo que permitía que el número mostrado
           contradijera a las filas reales. */
        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }
    }

    /// <summary>
    /// Elemento de catálogo. Sirve para cargos, departamentos y categorías,
    /// que solo necesitan código y nombre.
    /// </summary>
    public class Catalogo
    {
        public int codigo { get; set; }
        public string nombre { get; set; }
        public string detalle { get; set; }
    }
}
