using System;
using System.Globalization;

namespace frontend.Modelos
{
    /// <summary>
    /// Traducciones de enumeraciones y datos a texto y clases CSS para la
    /// presentación. Se concentran acá para que las páginas no repitan lógica
    /// de formato y para que un cambio de vocabulario visual sea un solo lugar.
    /// </summary>
    public static class Vista
    {
        private static readonly CultureInfo Hn = new CultureInfo("es-HN");

        // ------------------------------------------------------- Categorías

        /// <summary>
        /// Clase de chip por categoría temática. Las categorías provienen de la
        /// taxonomía COFOG (ONU, 2000) adoptada en el marco teórico.
        /// </summary>
        public static string ClaseCategoria(string categoria)
        {
            if (string.IsNullOrEmpty(categoria)) return "gc-chip";

            string c = categoria.ToLowerInvariant();
            if (c.StartsWith("salud")) return "gc-chip gc-chip--salud";
            if (c.StartsWith("educaci")) return "gc-chip gc-chip--educacion";
            if (c.StartsWith("seguridad")) return "gc-chip gc-chip--seguridad";
            if (c.StartsWith("econom")) return "gc-chip gc-chip--economia";
            if (c.StartsWith("infraestructura")) return "gc-chip gc-chip--infraestructura";
            if (c.StartsWith("transparencia")) return "gc-chip gc-chip--transparencia";
            return "gc-chip";
        }

        // --------------------------------------------- Estado de propuesta

        public static string TextoEstado(EstadoPropuesta estado)
        {
            switch (estado)
            {
                case EstadoPropuesta.Declarada: return "Declarada";
                case EstadoPropuesta.SinAvance: return "Sin avance";
                case EstadoPropuesta.EnProceso: return "En proceso";
                case EstadoPropuesta.Estancada: return "Estancada";
                case EstadoPropuesta.CumplidaAMedias: return "Cumplida a medias";
                case EstadoPropuesta.Cumplida: return "Cumplida";
                case EstadoPropuesta.Incumplida: return "Incumplida";
                default: return "Declarada";
            }
        }

        public static string ClaseEstado(EstadoPropuesta estado)
        {
            switch (estado)
            {
                case EstadoPropuesta.Cumplida: return "gc-chip gc-chip--cumplida";
                case EstadoPropuesta.CumplidaAMedias: return "gc-chip gc-chip--proceso";
                case EstadoPropuesta.EnProceso: return "gc-chip gc-chip--proceso";
                case EstadoPropuesta.Incumplida: return "gc-chip gc-chip--incumplida";
                case EstadoPropuesta.Estancada: return "gc-chip gc-chip--estancada";
                case EstadoPropuesta.SinAvance: return "gc-chip gc-chip--estancada";
                default: return "gc-chip gc-chip--declarada";
            }
        }

        // -------------------------------------------------- Verificación

        public static string TextoVerificacion(NivelVerificacion nivel)
        {
            switch (nivel)
            {
                case NivelVerificacion.Verificado: return "Verificado";
                case NivelVerificacion.EnRevision: return "En revisión";
                default: return "Declarado por el candidato";
            }
        }

        public static string ClaseVerificacion(NivelVerificacion nivel)
        {
            switch (nivel)
            {
                case NivelVerificacion.Verificado: return "gc-verif gc-verif--ok";
                case NivelVerificacion.EnRevision: return "gc-verif gc-verif--rev";
                default: return "gc-verif";
            }
        }

        /// <summary>
        /// Nivel de verificación a partir del nombre que guarda el catálogo
        /// NivelesVerificacion. Lo necesitan las pantallas de administración,
        /// que reciben el nivel como texto y no como enumeración.
        /// </summary>
        public static NivelVerificacion NivelDe(string nombre)
        {
            if (string.Equals(nombre, "Verificado", StringComparison.OrdinalIgnoreCase))
                return NivelVerificacion.Verificado;

            if (string.Equals(nombre, "En revisión", StringComparison.OrdinalIgnoreCase))
                return NivelVerificacion.EnRevision;

            return NivelVerificacion.Declarado;
        }

        // ----------------------------------------------------- Campañas

        public static string TextoEstadoCampana(EstadoCampana estado)
        {
            switch (estado)
            {
                case EstadoCampana.Activa: return "En curso";
                case EstadoCampana.Proxima: return "Próxima";
                default: return "Cerrada";
            }
        }

        public static string TextoNivel(NivelGobierno nivel)
        {
            switch (nivel)
            {
                case NivelGobierno.Nacional: return "Nacional";
                case NivelGobierno.Departamental: return "Departamental";
                default: return "Municipal";
            }
        }

        // ------------------------------------------------------- Formato

        public static string Fecha(DateTime f)
        {
            return f.ToString("d 'de' MMMM 'de' yyyy", Hn);
        }

        public static string FechaCorta(DateTime f)
        {
            return f.ToString("d MMM yyyy", Hn);
        }

        /// <summary>
        /// Fecha con hora. La bitácora la necesita: dos acciones del mismo día
        /// sobre el mismo contenido se distinguen por la hora.
        /// </summary>
        public static string FechaHora(DateTime f)
        {
            return f.ToString("d MMM yyyy, HH:mm", Hn);
        }

        public static string Numero(int n)
        {
            return n.ToString("N0", Hn);
        }

        /// <summary>
        /// Cifra acompañada de su sustantivo en el número correcto. Evita los
        /// «1 propuestas» que quedan cuando se concatena a mano.
        /// </summary>
        public static string Plural(int n, string singular, string plural)
        {
            return n == 1
                ? "1 " + singular
                : Numero(n) + " " + plural;
        }

        /// <summary>
        /// Tiempo relativo para el feed ("hace 3 h", "hace 2 d").
        /// </summary>
        public static string TiempoRelativo(DateTime f)
        {
            TimeSpan t = DateTime.Now - f;

            if (t.TotalMinutes < 1) return "ahora";
            if (t.TotalMinutes < 60) return "hace " + (int)t.TotalMinutes + " min";
            if (t.TotalHours < 24) return "hace " + (int)t.TotalHours + " h";
            if (t.TotalDays < 7) return "hace " + (int)t.TotalDays + " d";
            if (t.TotalDays < 30) return "hace " + (int)(t.TotalDays / 7) + " sem";
            return FechaCorta(f);
        }

        /// <summary>
        /// Días restantes hasta la elección, en texto listo para mostrar.
        /// </summary>
        public static string CuentaRegresiva(DateTime eleccion)
        {
            int dias = (int)Math.Ceiling((eleccion.Date - DateTime.Now.Date).TotalDays);

            if (dias > 1) return "Faltan " + Numero(dias) + " días";
            if (dias == 1) return "Falta 1 día";
            if (dias == 0) return "Es hoy";
            return "Finalizada";
        }

        /// <summary>
        /// Iniciales a partir de un nombre completo en una sola cadena, para los
        /// avatares de comentarios y del feed.
        /// </summary>
        public static string InicialesDe(string nombreCompleto)
        {
            if (string.IsNullOrEmpty(nombreCompleto)) return string.Empty;

            string[] partes = nombreCompleto.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return string.Empty;
            if (partes.Length == 1) return partes[0].Substring(0, 1).ToUpperInvariant();

            return (partes[0].Substring(0, 1) + partes[partes.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        /// <summary>
        /// Iniciales para el avatar cuando no hay fotografía cargada.
        /// </summary>
        public static string Iniciales(string nombres, string apellidos)
        {
            string a = string.IsNullOrEmpty(nombres) ? "" : nombres.Trim().Substring(0, 1);
            string b = string.IsNullOrEmpty(apellidos) ? "" : apellidos.Trim().Substring(0, 1);
            return (a + b).ToUpperInvariant();
        }

        /// <summary>
        /// Estilo en línea del avatar cuando hay fotografía. Recibe la ruta ya
        /// resuelta por el control que la va a mostrar.
        /// </summary>
        public static string EstiloAvatar(string urlResuelta)
        {
            if (string.IsNullOrEmpty(urlResuelta)) return string.Empty;
            return "background-image:url('" + urlResuelta.Replace("'", "%27") + "')";
        }

        /// <summary>
        /// Recorta un texto en el último espacio antes del límite.
        /// </summary>
        public static string Resumen(string texto, int largo)
        {
            if (string.IsNullOrEmpty(texto) || texto.Length <= largo) return texto;

            int corte = texto.LastIndexOf(' ', largo);
            if (corte < largo / 2) corte = largo;
            return texto.Substring(0, corte) + "…";
        }
    }
}
