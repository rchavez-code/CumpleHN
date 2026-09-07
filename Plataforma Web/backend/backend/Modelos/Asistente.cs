namespace backend.Modelos
{
    /* ================================================================
       Objetos del asistente de consulta en lenguaje natural.

       Son dos familias distintas y conviene no confundirlas.

       Las primeras son lo que devuelven los procedimientos del script
       12 cuando el modelo invoca una herramienta. Nunca llegan al
       frontend: se serializan a JSON, se le entregan al modelo como
       resultado de la herramienta, y ahí termina su vida. Por eso sus
       nombres son los de las columnas y no los de la pantalla.

       La última, RespuestaAsistente, sí viaja al frontend, y su forma
       está copiada de la maqueta que ya existe en Analitica.aspx: un
       cuerpo redactado y una lista de fuentes. Esa forma no se cambia
       al conectar el modelo, porque es la que la encuesta dejó como
       condición de confianza — el 77.4 % pidió ver de dónde sale cada
       cifra.
       ================================================================ */

    /// <summary>
    /// Una opción válida de filtro, como se la ofrece al modelo.
    ///
    /// No se reusa Catalogo, que es el tipo de las páginas: aquel lleva
    /// un código entero y un nombre, y acá hacen falta las tres partes
    /// por separado. El modelo recibe la pregunta en palabras
    /// («propuestas de seguridad») y tiene que llegar al valor que el
    /// procedimiento espera, así que necesita ver a qué grupo pertenece
    /// cada opción y con qué valor se filtra.
    /// </summary>
    public class CatalogoIA
    {
        /// <summary>Campana, Categoria, Partido, Departamento o NivelGobierno.</summary>
        public string grupo { get; set; }

        /// <summary>Lo que se le pasa al procedimiento como filtro.</summary>
        public string valor { get; set; }

        /// <summary>Cómo se llama en pantalla.</summary>
        public string texto { get; set; }
    }

    /// <summary>
    /// Una propuesta como la ve el asistente. En el listado los textos
    /// vienen recortados por el procedimiento y en el detalle completos.
    /// </summary>
    public class PropuestaIA
    {
        public int codigoPropuesta { get; set; }
        public string propuesta { get; set; }
        public string campanaSlug { get; set; }
        public string categoria { get; set; }
        public string estado { get; set; }

        /// <summary>
        /// Declarado, En revisión o Verificado. Viaja siempre, porque la
        /// respuesta tiene que poder decir que algo es afirmación de la
        /// candidatura y no hecho comprobado.
        /// </summary>
        public string verificacion { get; set; }

        public string candidato { get; set; }
        public string candidatoSlug { get; set; }
        public string partido { get; set; }
        public string partidoSiglas { get; set; }
        public string departamento { get; set; }
        public string nivelGobierno { get; set; }
        public string fecha { get; set; }
        public string ubicacion { get; set; }
        public string periodoEjecucion { get; set; }
        public string beneficiarios { get; set; }
        public string descripcion { get; set; }
        public string problema { get; set; }
        public string objetivo { get; set; }
    }

    /// <summary>Ficha pública de una candidatura. Sin datos de contacto.</summary>
    public class FichaIA
    {
        public string candidatoSlug { get; set; }
        public string candidato { get; set; }
        public string campanaSlug { get; set; }
        public string partido { get; set; }
        public string partidoSiglas { get; set; }
        public string cargo { get; set; }
        public string nivelGobierno { get; set; }
        public string departamento { get; set; }
        public string municipio { get; set; }
        public string verificacion { get; set; }
        public string titular { get; set; }
        public string biografia { get; set; }
        public string informacionProfesional { get; set; }
        public string descripcionCandidatura { get; set; }

        /// <summary>Las propuestas de esta candidatura, en resumen.</summary>
        public PropuestaIA[] propuestas { get; set; }
    }

    /// <summary>
    /// Lo que el asistente le devuelve al frontend.
    ///
    /// Cuando ok es falso, mensaje explica por qué en palabras que se
    /// pueden mostrar tal cual: sin sesión, sin cuota, módulo apagado o
    /// falla de comunicación. La página no interpreta códigos.
    /// </summary>
    public class RespuestaAsistente
    {
        public bool ok { get; set; }

        /// <summary>Texto de la respuesta, ya en HTML simple de párrafos.</summary>
        public string respuesta { get; set; }

        /// <summary>
        /// De dónde salió cada cifra. Se arman con los procedimientos que
        /// el modelo llegó a invocar, no con lo que dice haber usado.
        /// </summary>
        public string[] fuentes { get; set; }

        /// <summary>Motivo cuando ok es falso. Vacío cuando todo salió bien.</summary>
        public string mensaje { get; set; }

        /// <summary>Preguntas que le quedan hoy a esta persona.</summary>
        public int restantes { get; set; }
    }
}
