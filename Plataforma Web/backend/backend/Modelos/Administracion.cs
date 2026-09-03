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
    /// Resultado de un guardado, con el código del registro afectado. En un
    /// alta es el código recién creado, que la página necesita para encadenar
    /// el siguiente paso — registrar una candidatura y crearle la cuenta sin
    /// tener que volver a buscarla.
    /// </summary>
    public class RespuestaGuardado
    {
        public bool ok { get; set; }
        public string mensaje { get; set; }
        public int codigo { get; set; }
    }

    /// <summary>
    /// Partido político visto desde la administración: incluye los
    /// desactivados y cuenta sus candidaturas activas.
    /// </summary>
    public class PartidoAdmin
    {
        public int codigoPartido { get; set; }
        public string slug { get; set; }
        public string nombre { get; set; }
        public string siglas { get; set; }
        public string descripcion { get; set; }
        public bool activo { get; set; }
        public int candidaturas { get; set; }
    }

    /// <summary>
    /// Campaña electoral vista desde la administración.
    /// </summary>
    public class CampanaAdmin
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
        public int candidaturas { get; set; }
        public int propuestas { get; set; }
    }

    /// <summary>
    /// Candidatura vista desde la administración. El campo login queda vacío
    /// cuando todavía no tiene cuenta de acceso: sin cuenta, la candidatura no
    /// puede administrar su propio perfil.
    /// </summary>
    public class CandidatoAdmin
    {
        public int codigoCandidato { get; set; }
        public string slug { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string nombreCompleto { get; set; }

        public int codigoCampana { get; set; }
        public string campanaSlug { get; set; }
        public string campana { get; set; }

        public int codigoPartido { get; set; }
        public string partido { get; set; }

        public int codigoCargo { get; set; }
        public string cargo { get; set; }

        public int codigoDepartamento { get; set; }
        public string departamento { get; set; }
        public string municipio { get; set; }
        public string titular { get; set; }

        public string verificacion { get; set; }
        public bool activo { get; set; }
        public DateTime fechaRegistro { get; set; }

        public string login { get; set; }
        public int propuestas { get; set; }
    }


    /// <summary>
    /// Estado efectivo de un elemento apagable, tal como lo consulta el sitio.
    /// Es deliberadamente mínimo: viaja en cada carga de página.
    /// </summary>
    public class EstadoModulo
    {
        public string clave { get; set; }
        public bool visible { get; set; }
    }

    /// <summary>
    /// Un elemento apagable visto desde la administración.
    ///
    /// Distingue <c>habilitado</c> — su propio interruptor — de
    /// <c>visible</c>, que ya tiene en cuenta al padre. Un gráfico con su
    /// interruptor encendido dentro de un tablero apagado no está visible, y
    /// la pantalla tiene que poder explicarlo en lugar de mostrar dos estados
    /// que se contradicen.
    /// </summary>
    public class ModuloAdmin
    {
        public int codigoModulo { get; set; }
        public string clave { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string grupo { get; set; }
        public string clavePadre { get; set; }

        public bool habilitado { get; set; }
        public bool visible { get; set; }
        public bool apagadoPorPadre { get; set; }

        public DateTime fechaCambio { get; set; }
        public string cambiadoPor { get; set; }
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
