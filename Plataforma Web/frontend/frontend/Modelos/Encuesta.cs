using System;
using System.Collections.Generic;

namespace frontend.Modelos
{
    /// <summary>
    /// Encuesta de percepción publicada por la plataforma.
    ///
    /// A diferencia de una publicación, no tiene candidatura autora: la escribe
    /// la administración y la tarjeta lo dice. Confundir las dos cosas sería
    /// prestarle la voz de la plataforma a una parte interesada.
    /// </summary>
    public class Encuesta
    {
        public Encuesta()
        {
            Opciones = new List<OpcionEncuesta>();
        }

        public int Id { get; set; }

        public string CampanaSlug { get; set; }

        public string Pregunta { get; set; }

        public string Descripcion { get; set; }

        /// <summary>Categoría temática, cuando la pregunta se refiere a un área concreta.</summary>
        public string Categoria { get; set; }

        public DateTime FechaInicio { get; set; }

        /// <summary>Texto vacío cuando no tiene cierre programado.</summary>
        public string FechaCierre { get; set; }

        /// <summary>Abierta, Cerrada, Programada o Retirada.</summary>
        public string Estado { get; set; }

        /// <summary>Total de personas que respondieron. Se muestra siempre.</summary>
        public int Votos { get; set; }

        /// <summary>Opción elegida por quien consulta, o cero si todavía no votó.</summary>
        public int MiOpcion { get; set; }

        public IList<OpcionEncuesta> Opciones { get; set; }

        // --------------------------------------------------- Presentación

        public bool EstaAbierta
        {
            get { return string.Equals(Estado, "Abierta", StringComparison.OrdinalIgnoreCase); }
        }

        public bool YaVoto
        {
            get { return MiOpcion > 0; }
        }

        public string TotalTexto
        {
            get { return Vista.Plural(Votos, "respuesta", "respuestas"); }
        }

        public bool TieneDescripcion
        {
            get { return !string.IsNullOrEmpty(Descripcion); }
        }

        public bool TieneCategoria
        {
            get { return !string.IsNullOrEmpty(Categoria); }
        }

        public string CategoriaClase
        {
            get { return Vista.ClaseCategoria(Categoria); }
        }

        /// <summary>
        /// Cuándo cierra, en palabras. Vacío cuando no tiene cierre
        /// programado, que es un caso normal y no un dato que falte.
        ///
        /// El verbo cambia con el estado. Una encuesta que ya cerró
        /// anunciando «cierra el» estaría prometiendo un plazo que no existe.
        /// </summary>
        public string CierreTexto
        {
            get
            {
                if (string.IsNullOrEmpty(FechaCierre)) return string.Empty;

                DateTime cierre;
                if (!DateTime.TryParse(FechaCierre, out cierre)) return string.Empty;

                return (EstaAbierta ? "Cierra el " : "Cerró el ") + Vista.Fecha(cierre);
            }
        }
    }

    /// <summary>
    /// Una de las opciones entre las que se elige.
    ///
    /// El conteo llega siempre, haya respondido o no quien consulta. Hubo una
    /// bandera <c>Revelar</c> que lo reservaba hasta después de votar y se
    /// quitó: una encuesta que esconde su resultado hasta que participes
    /// convierte el dato en un peaje.
    /// </summary>
    public class OpcionEncuesta
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public int Orden { get; set; }
        public int Votos { get; set; }

        /// <summary>Si es la opción que eligió quien consulta.</summary>
        public bool MiVoto { get; set; }

        /// <summary>
        /// Porcentaje sobre el total de la encuesta. Se calcula en la vista y
        /// no viaja desde la base por la misma razón que los contadores no se
        /// guardan: un porcentaje almacenado puede contradecir a las filas.
        /// </summary>
        public int Porcentaje(int total)
        {
            if (total <= 0) return 0;
            return (int)Math.Round((Votos * 100.0) / total);
        }
    }

    /// <summary>
    /// Resultado de responder una encuesta.
    ///
    /// Trae las opciones ya actualizadas para que la tarjeta no tenga que
    /// pedirlas en una segunda llamada, y las trae también cuando el voto se
    /// rechazó: si la encuesta cerró mientras la persona la tenía abierta,
    /// mostrarle el resultado explica el rechazo mejor que el mensaje solo.
    /// </summary>
    public class ResultadoEncuesta
    {
        public ResultadoEncuesta()
        {
            Opciones = new List<OpcionEncuesta>();
        }

        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public int Votos { get; set; }
        public IList<OpcionEncuesta> Opciones { get; set; }
    }

    /// <summary>
    /// Encuesta vista desde la administración: incluye las retiradas, las
    /// programadas y las cerradas, que es justamente lo que el público no ve.
    /// </summary>
    public class EncuestaAdmin
    {
        public int Codigo { get; set; }
        public int CodigoCampana { get; set; }
        public string CampanaSlug { get; set; }
        public string Campana { get; set; }
        public string Pregunta { get; set; }
        public string Descripcion { get; set; }
        public int CodigoCategoria { get; set; }
        public string Categoria { get; set; }
        public DateTime FechaInicio { get; set; }
        public string FechaCierre { get; set; }
        public bool Activo { get; set; }
        public string MotivoBaja { get; set; }
        public string Estado { get; set; }
        public int Opciones { get; set; }
        public int Votos { get; set; }

        public bool EstaAbierta
        {
            get { return string.Equals(Estado, "Abierta", StringComparison.OrdinalIgnoreCase); }
        }

        public bool TieneVotos
        {
            get { return Votos > 0; }
        }

        /// <summary>
        /// Clase del distintivo de estado. Reusa los chips que ya visten los
        /// estados de propuesta en lugar de estrenar una familia de clases:
        /// verde para la que está recogiendo respuestas, ámbar para la que
        /// todavía no empieza, escarlata para la retirada y gris para la que
        /// terminó.
        /// </summary>
        public string EstadoClase
        {
            get
            {
                switch ((Estado ?? string.Empty).ToLowerInvariant())
                {
                    case "abierta": return "gc-chip gc-chip--cumplida";
                    case "programada": return "gc-chip gc-chip--declarada";
                    case "retirada": return "gc-chip gc-chip--incumplida";
                    default: return "gc-chip gc-chip--estancada";
                }
            }
        }

        public string FechaInicioTexto
        {
            get { return Vista.FechaCorta(FechaInicio); }
        }

        public string FechaCierreTexto
        {
            get
            {
                if (string.IsNullOrEmpty(FechaCierre)) return "Sin cierre programado";

                DateTime cierre;
                if (!DateTime.TryParse(FechaCierre, out cierre)) return FechaCierre;

                return Vista.FechaCorta(cierre);
            }
        }

        public string VotosTexto
        {
            get { return Vista.Plural(Votos, "respuesta", "respuestas"); }
        }
    }
}
