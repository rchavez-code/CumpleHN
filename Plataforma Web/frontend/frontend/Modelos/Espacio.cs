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

        /* Hasta cuándo está (o estuvo) vigente. MinValue sin suscripción. */
        public DateTime VigenteHasta { get; set; }
        public int Pagos { get; set; }

        /// <summary>El espacio admite participación y administración ahora.</summary>
        public bool Vigente
        {
            get { return EsPlataforma || Estado == "Vigente"; }
        }

        public string VigenteTexto
        {
            get
            {
                if (EsPlataforma) return "—";
                if (VigenteHasta == DateTime.MinValue) return "Sin pagos";
                return (Vigente ? "Hasta el " : "Venció el ") + VigenteHasta.ToString("d MMM yyyy");
            }
        }

        /// <summary>La dirección pública del espacio. Vacía para la plataforma, que es la raíz.</summary>
        public string Url
        {
            get { return EsPlataforma ? "~/" : "~/e/" + Slug; }
        }
    }

    /// <summary>Un correo del padrón de un espacio y lo que la plataforma sabe de él.</summary>
    public class Miembro
    {
        public int Codigo { get; set; }
        public string Correo { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public string Nombre { get; set; }
        public bool TieneCuenta { get; set; }
        public bool PuedeParticipar { get; set; }

        public string EstadoTexto
        {
            get
            {
                if (!Activo) return "Fuera del padrón";
                if (PuedeParticipar) return "Puede participar";
                if (TieneCuenta) return "Cuenta sin confirmar";
                return "Sin cuenta todavía";
            }
        }

        public string EstadoClase
        {
            get
            {
                if (!Activo) return "gc-chip gc-chip--estancada";
                if (PuedeParticipar) return "gc-chip gc-chip--cumplida";
                if (TieneCuenta) return "gc-chip gc-chip--proceso";
                return "gc-chip gc-chip--declarada";
            }
        }
    }

    /// <summary>Resultado de cargar una lista al padrón.</summary>
    public class ResultadoPadron
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public string Rechazados { get; set; }
    }

    /// <summary>Un pago registrado y el período que cubre.</summary>
    public class Suscripcion
    {
        public int Codigo { get; set; }
        public string Plan { get; set; }
        public DateTime VigenteDesde { get; set; }
        public DateTime VigenteHasta { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; }
        public string Referencia { get; set; }
        public string Notas { get; set; }
        public string RegistradoPor { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Vigente { get; set; }

        public string PeriodoTexto
        {
            get { return VigenteDesde.ToString("d MMM yyyy") + " – " + VigenteHasta.ToString("d MMM yyyy"); }
        }

        public string MontoTexto
        {
            get { return Moneda + " " + Monto.ToString("N2"); }
        }
    }

    /// <summary>Un plan de la oferta: precio en lempiras, duración y tope del padrón.</summary>
    public class Plan
    {
        public int Codigo { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Lema { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string Moneda { get; set; }
        public int Dias { get; set; }
        /// <summary>Cero es sin tope.</summary>
        public int MaxMiembros { get; set; }
        public bool Destacado { get; set; }

        public string PrecioTexto
        {
            get { return "L " + Precio.ToString("N0"); }
        }

        public string DuracionTexto
        {
            get
            {
                if (Dias % 365 == 0) return Dias / 365 == 1 ? "un año" : (Dias / 365) + " años";
                if (Dias % 30 == 0 && Dias >= 60) return (Dias / 30) + " meses";
                return Dias + " días";
            }
        }

        public string MiembrosTexto
        {
            get { return MaxMiembros <= 0 ? "Padrón sin tope" : "Padrón de hasta " + MaxMiembros.ToString("N0") + " miembros"; }
        }
    }

    /// <summary>Lo que una organización dejó en el formulario público, y cómo se resolvió.</summary>
    public class Solicitud
    {
        public int Codigo { get; set; }
        public string Organizacion { get; set; }
        public string Contacto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public int CodigoPlan { get; set; }
        public string Plan { get; set; }
        public string Proceso { get; set; }
        public DateTime FechaAproximada { get; set; }
        public string Mensaje { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int CodigoEspacio { get; set; }
        public string Espacio { get; set; }
        public string AtendidaPor { get; set; }
        public DateTime FechaAtencion { get; set; }
        public string Notas { get; set; }

        public bool Nueva { get { return Estado == "Nueva"; } }

        public string FechaAproximadaTexto
        {
            get { return FechaAproximada == DateTime.MinValue ? "sin fecha" : FechaAproximada.ToString("d MMM yyyy"); }
        }

        public string EstadoClase
        {
            get
            {
                switch (Estado)
                {
                    case "Nueva":     return "gc-chip gc-chip--proceso";
                    case "Atendida":  return "gc-chip gc-chip--cumplida";
                    default:          return "gc-chip gc-chip--estancada";
                }
            }
        }
    }
}
