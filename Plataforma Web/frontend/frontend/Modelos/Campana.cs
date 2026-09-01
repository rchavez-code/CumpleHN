using System;

namespace frontend.Modelos
{
    /// <summary>
    /// Campaña electoral. Es la entidad raíz de la navegación pública: toda
    /// candidatura, propuesta y publicación pertenece a una campaña.
    /// </summary>
    public class Campana
    {
        public Campana()
        {
            Estado = EstadoCampana.Proxima;
        }

        /// <summary>Identificador legible usado en la URL (p. ej. "generales-2029").</summary>
        public string Slug { get; set; }

        public string Nombre { get; set; }

        /// <summary>Frase corta que describe la campaña.</summary>
        public string Resumen { get; set; }

        public string Descripcion { get; set; }

        /// <summary>Ámbito territorial y de cargos que cubre la campaña.</summary>
        public string Alcance { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaEleccion { get; set; }

        public EstadoCampana Estado { get; set; }

        /// <summary>Campaña que se muestra destacada en la portada.</summary>
        public bool EsActual { get; set; }

        public int TotalCandidatos { get; set; }

        public int TotalPropuestas { get; set; }

        public int TotalPublicaciones { get; set; }

        public string EstadoTexto
        {
            get { return Vista.TextoEstadoCampana(Estado); }
        }

        public string ClaseTarjeta
        {
            get
            {
                switch (Estado)
                {
                    case EstadoCampana.Activa: return "gc-campana";
                    case EstadoCampana.Proxima: return "gc-campana gc-campana--proxima";
                    default: return "gc-campana gc-campana--cerrada";
                }
            }
        }

        /// <summary>Período en texto, para listados y encabezados.</summary>
        public string Periodo
        {
            get { return FechaInicio.Year + " – " + FechaEleccion.Year; }
        }

        public string Url
        {
            get { return "~/Campana?c=" + Slug; }
        }
    }
}
