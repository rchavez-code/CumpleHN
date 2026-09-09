using System;

namespace backend.Modelos
{
    /// <summary>
    /// Encuesta de percepción tal como la ve el sitio público.
    ///
    /// Lleva sus opciones adentro. La portada dibuja varias encuestas en la
    /// misma respuesta —dos a la vista y el resto tras un «Ver más»—, así que
    /// pedirlas por separado serían tantos viajes como encuestas para responder
    /// siempre lo mismo. Después de responder, en cambio, solo vuelven las
    /// opciones de la encuesta tocada, que es lo único que cambió.
    /// </summary>
    public class EncuestaPublica
    {
        public int codigoEncuesta { get; set; }
        public string campanaSlug { get; set; }
        public string pregunta { get; set; }
        public string descripcion { get; set; }
        public string categoria { get; set; }
        public DateTime fechaInicio { get; set; }

        /// <summary>
        /// Fecha de cierre. Va como texto vacío cuando la encuesta no tiene
        /// cierre programado — un DateTime no puede representar «ninguna», y
        /// el año cero disfrazado de fecha sería peor.
        /// </summary>
        public string fechaCierre { get; set; }

        /// <summary>Abierta, Cerrada, Programada o Retirada, derivado de las fechas.</summary>
        public string estado { get; set; }

        /// <summary>Total de personas que respondieron. Se muestra siempre.</summary>
        public int votos { get; set; }

        /// <summary>Opción elegida por quien consulta, o cero si todavía no votó.</summary>
        public int miOpcion { get; set; }

        /// <summary>Opciones entre las que se elige, ya con su resultado.</summary>
        public OpcionEncuesta[] opciones { get; set; }
    }

    /// <summary>
    /// Una de las opciones entre las que se elige, con su resultado.
    ///
    /// El conteo viaja siempre, haya respondido o no quien consulta. Hubo una
    /// columna <c>revelar</c> que lo reservaba hasta después de votar y se
    /// quitó: una encuesta que esconde su resultado hasta que participes
    /// convierte el dato en un peaje.
    /// </summary>
    public class OpcionEncuesta
    {
        public int codigoOpcion { get; set; }
        public string texto { get; set; }
        public int orden { get; set; }
        public int votos { get; set; }

        /// <summary>Si es la opción que eligió quien consulta.</summary>
        public bool miVoto { get; set; }
    }

    /// <summary>
    /// Resultado de registrar un voto, con las opciones ya actualizadas para
    /// que la página no tenga que pedirlas en una segunda llamada.
    /// </summary>
    public class RespuestaEncuesta
    {
        public bool ok { get; set; }
        public string mensaje { get; set; }
        public int votos { get; set; }
        public OpcionEncuesta[] opciones { get; set; }
    }

    /// <summary>
    /// Encuesta vista desde la administración: incluye las retiradas, las
    /// programadas y las cerradas, que es justamente lo que el público no ve.
    /// </summary>
    public class EncuestaAdmin
    {
        public int codigoEncuesta { get; set; }
        public int codigoCampana { get; set; }
        public string campanaSlug { get; set; }
        public string campana { get; set; }
        public string pregunta { get; set; }
        public string descripcion { get; set; }
        public int codigoCategoria { get; set; }
        public string categoria { get; set; }
        public DateTime fechaInicio { get; set; }
        public string fechaCierre { get; set; }
        public bool activo { get; set; }
        public string motivoBaja { get; set; }
        public string estado { get; set; }
        public int opciones { get; set; }
        public int votos { get; set; }
    }
}
