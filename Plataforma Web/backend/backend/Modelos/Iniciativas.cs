using System;

namespace backend.Modelos
{
    /// <summary>
    /// Iniciativa ciudadana tal como viaja al frontend.
    ///
    /// Una sola clase para las tres consultas —portada, «Mi cuenta» y
    /// moderación— porque los tres procedimientos devuelven las mismas
    /// columnas y el Web Service las lee con el mismo lector. Lo que cambia
    /// entre una y otra es qué filas entran, no qué se sabe de cada una.
    ///
    /// De quien la propuso solo viaja el nombre. El login y el correo no
    /// salen de Usuarios: la ficha pública de una iniciativa no es un
    /// directorio de personas.
    /// </summary>
    public class IniciativaPublica
    {
        public int codigoIniciativa { get; set; }
        public string autora { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public int codigoCategoria { get; set; }
        public string categoria { get; set; }

        /// <summary>Cero es «sin departamento»: alcance nacional, no dato faltante.</summary>
        public int codigoDepartamento { get; set; }
        public string departamento { get; set; }

        /// <summary>Contadores derivados de Valoraciones y Comentarios, nunca guardados.</summary>
        public int meGusta { get; set; }
        public int noMeGusta { get; set; }
        public int comentarios { get; set; }

        /// <summary>meGusta − noMeGusta. Es lo que ordena la portada.</summary>
        public int saldo { get; set; }

        /// <summary>Voto de quien consulta: 1, −1 o 0.</summary>
        public int miValoracion { get; set; }

        /// <summary>Si la propuso quien consulta.</summary>
        public bool esMia { get; set; }

        public bool activo { get; set; }
        public string motivoBaja { get; set; }

        /// <summary>
        /// Si el texto todavía se puede cambiar: solo mientras nadie valoró
        /// ni comentó. Lo calcula la base, para que la pantalla y el
        /// procedimiento de guardado no discrepen en qué es «sin reacciones».
        /// </summary>
        public bool puedeEditar { get; set; }

        public DateTime fechaRegistro { get; set; }

        /// <summary>Vacío si nunca se editó. Ver la nota de EncuestaPublica.fechaCierre.</summary>
        public string fechaEdicion { get; set; }
    }
}
