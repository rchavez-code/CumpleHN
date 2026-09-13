using System;

namespace backend.Modelos
{
    /* ================================================================
       Espacios.

       Un espacio es un cliente que usa la plataforma para su propio
       proceso electoral. El sitio público CumpleHN es también un
       espacio, el único con esPlataforma en verdadero. Todo lo que el
       Web Service lista para el público se lista dentro de un espacio,
       y todo lo que administra se administra dentro de uno: es lo que
       impide que el contenido de un cliente aparezca en el sitio de
       otro.

       Un solo tipo para la consulta pública y la de administración.
       Los campos de administración (propietario, conteos, motivo de
       baja) viajan vacíos en la pública, que solo necesita la marca y
       las reglas del espacio.
       ================================================================ */

    public class Espacio
    {
        public int codigoEspacio { get; set; }
        public string slug { get; set; }
        public string nombre { get; set; }
        public string organizacion { get; set; }
        public string descripcion { get; set; }
        public bool esPlataforma { get; set; }

        /// <summary>Solo participa quien esté en el padrón del espacio.</summary>
        public bool padronCerrado { get; set; }

        /// <summary>Cómo llama el espacio a la agrupación de candidaturas: Partido, Planilla, Lista.</summary>
        public string terminoAgrupacion { get; set; }

        public bool activo { get; set; }

        /// <summary>Plataforma, Activo o Retirado. Lo deriva la base.</summary>
        public string estado { get; set; }

        /* --- Solo administración */
        public string motivoBaja { get; set; }
        public DateTime fechaCreacion { get; set; }
        public int codigoUsuarioPropietario { get; set; }
        public string propietario { get; set; }
        public string propietarioLogin { get; set; }
        public string propietarioCorreo { get; set; }
        public int campanas { get; set; }
        public int candidaturas { get; set; }
        public int administradores { get; set; }

        /* Hasta cuándo está (o estuvo) vigente. Sin suscripción viaja en
           DateTime.MinValue, y pagos en cero. */
        public DateTime vigenteHasta { get; set; }
        public int pagos { get; set; }
    }

    /// <summary>
    /// Un correo del padrón de un espacio, con lo que la plataforma sabe de
    /// él: si ya hay una cuenta con ese correo y si puede participar. Del
    /// titular de la cuenta solo viaja el nombre.
    /// </summary>
    public class Miembro
    {
        public int codigoMiembro { get; set; }
        public int codigoEspacio { get; set; }
        public string correo { get; set; }
        public bool activo { get; set; }
        public DateTime fechaAlta { get; set; }
        public string nombre { get; set; }
        public bool tieneCuenta { get; set; }
        public bool puedeParticipar { get; set; }
    }

    /// <summary>
    /// Resultado de cargar una lista al padrón: el resumen en el mensaje y,
    /// aparte, los correos que no se tomaron por no tener forma de correo,
    /// para que la pantalla los muestre y se corrijan.
    /// </summary>
    public class RespuestaPadron
    {
        public bool ok { get; set; }
        public string mensaje { get; set; }
        public string rechazados { get; set; }
    }

    /// <summary>Un pago registrado y el período que cubre.</summary>
    public class Suscripcion
    {
        public int codigoSuscripcion { get; set; }
        public int codigoEspacio { get; set; }
        public string plan { get; set; }
        public DateTime vigenteDesde { get; set; }
        public DateTime vigenteHasta { get; set; }
        public decimal monto { get; set; }
        public string moneda { get; set; }
        public string referenciaPago { get; set; }
        public string notas { get; set; }
        public string registradoPor { get; set; }
        public DateTime fechaRegistro { get; set; }
        public bool vigente { get; set; }
    }
}
