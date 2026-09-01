namespace backend.Modelos
{
    /* ================================================================
       Objetos del módulo de analítica.

       El diseño anterior transportaba todo en un único tipo genérico
       «etiqueta y dos valores». Servía mientras el tablero solo dibujaba
       barras, pero cuando la página tiene que redactar hallazgos —«la
       brecha más grande está en salud», «ninguna de las 11 propuestas
       tiene fuente verificable»— ese tipo obliga a recordar qué
       significaba valorSecundario en cada gráfico.

       Acá cada indicador tiene su fila con nombres propios. El costo son
       unos tipos más; la ganancia es que el código que interpreta los
       datos se lee como la frase que va a mostrar.
       ================================================================ */

    /// <summary>
    /// Filtros del tablero. Todos son opcionales y viajan juntos.
    ///
    /// Se usa cero y cadena vacía como «sin filtrar» en lugar de tipos
    /// anulables porque son los valores que el desplegable de una página
    /// Web Forms produce naturalmente cuando la opción elegida es «Todas».
    /// </summary>
    public class FiltroAnalitica
    {
        /// <summary>Campaña. Vacío usa la campaña destacada.</summary>
        public string campanaSlug { get; set; }

        /// <summary>Categoría temática. Cero son todas.</summary>
        public int codigoCategoria { get; set; }

        /// <summary>Partido político. Cero son todos.</summary>
        public int codigoPartido { get; set; }

        /// <summary>Departamento. Cero son todos.</summary>
        public int codigoDepartamento { get; set; }

        /// <summary>Nacional, Departamental o Municipal. Vacío son todos.</summary>
        public string nivelGobierno { get; set; }

        /// <summary>Inicio del rango, en formato aaaa-MM-dd. Vacío no acota.</summary>
        public string desde { get; set; }

        /// <summary>Fin del rango, en formato aaaa-MM-dd. Vacío no acota.</summary>
        public string hasta { get; set; }
    }

    /// <summary>Opción de un desplegable de filtro.</summary>
    public class OpcionFiltro
    {
        /// <summary>Campana, Categoria, Partido, Departamento o NivelGobierno.</summary>
        public string grupo { get; set; }

        /// <summary>Valor que se envía de vuelta al filtrar.</summary>
        public string valor { get; set; }

        /// <summary>Texto que se muestra.</summary>
        public string texto { get; set; }
    }

    /// <summary>
    /// Cifras de encabezado.
    ///
    /// Cada magnitud viaja con su denominador. Un tablero que muestra
    /// «0 propuestas verificadas» sin decir sobre cuántas no permite
    /// concluir nada, y el denominador no se puede reconstruir después
    /// sin volver a consultar.
    /// </summary>
    public class ResumenAnalitica
    {
        public int propuestas { get; set; }
        public int candidaturas { get; set; }
        public int partidos { get; set; }
        public int publicaciones { get; set; }

        public int categoriasConOferta { get; set; }
        public int categoriasTotal { get; set; }

        /// <summary>Propuestas con estado distinto de Declarada.</summary>
        public int propuestasEvaluadas { get; set; }
        public int propuestasVerificadas { get; set; }
        public int candidaturasVerificadas { get; set; }

        public int departamentosConCandidatura { get; set; }
        public int departamentosTotal { get; set; }

        public int valoraciones { get; set; }
        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }
        public int personasParticipando { get; set; }

        /// <summary>
        /// Fecha del registro más reciente que entra en la selección, en
        /// formato aaaa-MM-dd. Vacía cuando la selección no tiene datos.
        ///
        /// Es la fecha de los datos, no la hora del servidor: el encabezado
        /// dice hasta cuándo llega la información, no cuándo se dibujó.
        /// </summary>
        public string ultimoRegistro { get; set; }
    }

    /// <summary>Oferta programática y demanda ciudadana de una categoría.</summary>
    public class FilaCategoria
    {
        public int codigoCategoria { get; set; }
        public string categoria { get; set; }

        /// <summary>Porcentaje de interés medido en la encuesta (n = 150).</summary>
        public decimal interesEncuesta { get; set; }

        public int propuestas { get; set; }

        /// <summary>Candidaturas distintas que proponen en esta categoría.</summary>
        public int candidaturas { get; set; }
    }

    /// <summary>Propuestas en un estado de cumplimiento.</summary>
    public class FilaEstado
    {
        public string estado { get; set; }
        public string descripcion { get; set; }

        /// <summary>Peso del estado en el cálculo del cumplimiento.</summary>
        public decimal ponderacion { get; set; }

        public int propuestas { get; set; }
    }

    /// <summary>Nivel de verificación alcanzado por un tipo de entidad.</summary>
    public class FilaVerificacion
    {
        /// <summary>Candidaturas, Propuestas o Publicaciones.</summary>
        public string entidad { get; set; }

        /// <summary>Declarado, En revisión o Verificado.</summary>
        public string nivel { get; set; }

        public int total { get; set; }
    }

    /// <summary>Participación recibida por un tipo de contenido.</summary>
    public class FilaParticipacion
    {
        public string tipoObjeto { get; set; }
        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }
    }

    /// <summary>Fila del ranking de candidaturas.</summary>
    public class FilaCandidato
    {
        public string candidatoSlug { get; set; }
        public string candidato { get; set; }
        public string partidoSiglas { get; set; }
        public string departamento { get; set; }
        public string cargo { get; set; }
        public string nivelGobierno { get; set; }
        public string verificacion { get; set; }

        public int propuestas { get; set; }
        public int meGusta { get; set; }
        public int noMeGusta { get; set; }

        /// <summary>A favor menos en contra. Es el criterio de orden.</summary>
        public int saldo { get; set; }

        public int comentarios { get; set; }
    }

    /// <summary>Fila del ranking de partidos.</summary>
    public class FilaPartido
    {
        /// <summary>Cero agrupa a las candidaturas independientes.</summary>
        public int codigoPartido { get; set; }

        public string partidoSiglas { get; set; }
        public string partido { get; set; }

        public int candidaturas { get; set; }
        public int propuestas { get; set; }

        /// <summary>
        /// Propuestas divididas entre candidaturas. Sin este cociente el
        /// gráfico solo mide cuántas candidaturas inscribió cada partido.
        /// </summary>
        public decimal propuestasPorCandidatura { get; set; }

        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
    }

    /// <summary>Cobertura de un departamento. Vienen los 18 siempre.</summary>
    public class FilaDepartamento
    {
        public int codigoDepartamento { get; set; }
        public string departamento { get; set; }
        public int candidaturas { get; set; }
        public int propuestas { get; set; }
        public int valoraciones { get; set; }
    }

    /// <summary>Actividad de un día.</summary>
    public class FilaActividad
    {
        /// <summary>Formato aaaa-MM-dd.</summary>
        public string fecha { get; set; }

        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }
    }

    /// <summary>
    /// Todo el tablero en un solo objeto.
    ///
    /// Viaja completo en una llamada, no una por gráfico. Con este volumen
    /// de filas —decenas, no miles— una sola respuesta agregada cuesta
    /// menos que nueve viajes, y garantiza que todos los indicadores de la
    /// pantalla correspondan al mismo instante de los datos.
    /// </summary>
    public class Analitica
    {
        public string campanaSlug { get; set; }
        public string campanaNombre { get; set; }

        /// <summary>Momento en que el backend resolvió la consulta, aaaa-MM-dd HH:mm.</summary>
        public string generado { get; set; }

        /// <summary>Verdadero cuando la selección no devolvió ningún registro.</summary>
        public bool sinDatos { get; set; }

        public ResumenAnalitica resumen { get; set; }
        public OpcionFiltro[] opciones { get; set; }

        public FilaCategoria[] categorias { get; set; }
        public FilaEstado[] estados { get; set; }
        public FilaVerificacion[] verificacion { get; set; }
        public FilaParticipacion[] participacion { get; set; }
        public FilaCandidato[] candidatos { get; set; }
        public FilaPartido[] partidos { get; set; }
        public FilaDepartamento[] territorio { get; set; }
        public FilaActividad[] actividad { get; set; }
    }
}
