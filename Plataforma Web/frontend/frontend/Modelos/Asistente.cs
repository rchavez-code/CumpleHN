using System;
using System.Collections.Generic;

namespace frontend.Modelos
{
    /// <summary>
    /// Lo que el asistente le devuelve a la página.
    ///
    /// La forma viene de la maqueta que existía antes de conectar el modelo:
    /// un cuerpo redactado y una lista de fuentes. No se cambió al conectar
    /// el modelo de lenguaje, y esa continuidad es deliberada — es la forma
    /// que la encuesta dejó como condición de confianza, donde el 77.4 %
    /// pidió poder ver de dónde sale cada cifra.
    ///
    /// Cuando <see cref="Ok"/> es falso, <see cref="Mensaje"/> explica por
    /// qué en palabras que se muestran tal cual: sin sesión, sin cuota,
    /// módulo apagado o falla de comunicación. La página no interpreta
    /// códigos ni arma su propio texto de error.
    /// </summary>
    public class RespuestaAsistente
    {
        public bool Ok { get; set; }

        /// <summary>Cuerpo de la respuesta, en párrafos.</summary>
        public string Respuesta { get; set; }

        /// <summary>
        /// Los procedimientos de los que salió cada cifra. El backend los
        /// arma con las herramientas que realmente se ejecutaron, no con las
        /// que el modelo diga haber usado.
        /// </summary>
        public IList<string> Fuentes { get; set; }

        /// <summary>Motivo cuando <see cref="Ok"/> es falso.</summary>
        public string Mensaje { get; set; }

        /// <summary>Consultas que le quedan hoy a esta persona.</summary>
        public int Restantes { get; set; }

        public RespuestaAsistente()
        {
            Respuesta = string.Empty;
            Mensaje = string.Empty;
            Fuentes = new List<string>();
        }
    }

    /// <summary>
    /// Una consulta ya respondida, para el reporte de quien la hizo.
    ///
    /// <see cref="Respuesta"/> es el HTML tal como lo redactó el modelo, sin
    /// limpiar. La página no lo escribe nunca en el marcado: lo entrega como
    /// texto y cumplehn-respuesta-ia.js lo reconstruye con la misma lista
    /// blanca que usa el tablero. En el historial para elegir viene vacía.
    /// </summary>
    public class ConsultaAsistente
    {
        public int Codigo { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta { get; set; }
        public DateTime Fecha { get; set; }

        public ConsultaAsistente()
        {
            Pregunta = string.Empty;
            Respuesta = string.Empty;
        }
    }
}
