using System;

namespace frontend.Modelos
{
    /// <summary>
    /// Espacio: un cliente que usa la plataforma para su propio proceso
    /// electoral. El sitio público CumpleHN es el espacio de la plataforma.
    ///
    /// Un solo modelo para la ficha pública (lo que la plantilla necesita
    /// para decir de quién es lo que se ve) y para la administración (los
    /// conteos, el propietario y el motivo de baja, que viajan vacíos en la
    /// pública).
    /// </summary>
    public class Espacio
    {
        public int Codigo { get; set; }
        public string Slug { get; set; }
        public string Nombre { get; set; }
        public string Organizacion { get; set; }
        public string Descripcion { get; set; }
        public bool EsPlataforma { get; set; }

        /// <summary>Solo participa quien esté en el padrón del espacio.</summary>
        public bool PadronCerrado { get; set; }

        /// <summary>Partido, Planilla, Lista: cómo llama el espacio a la agrupación de candidaturas.</summary>
        public string TerminoAgrupacion { get; set; }

        public bool Activo { get; set; }

        /// <summary>Plataforma, Activo o Retirado. Lo deriva la base.</summary>
        public string Estado { get; set; }

        /* --- Solo administración */
        public string MotivoBaja { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int CodigoPropietario { get; set; }
        public string Propietario { get; set; }
        public string PropietarioLogin { get; set; }
        public string PropietarioCorreo { get; set; }
        public int Campanas { get; set; }
        public int Candidaturas { get; set; }
        public int Administradores { get; set; }

        /// <summary>La dirección pública del espacio. Vacía para la plataforma, que es la raíz.</summary>
        public string Url
        {
            get { return EsPlataforma ? "~/" : "~/e/" + Slug; }
        }
    }
}
