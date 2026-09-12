using System;

namespace frontend.Modelos
{
    /// <summary>
    /// Iniciativa ciudadana: una propuesta de proyecto escrita por una cuenta
    /// con rol Ciudadano, que el resto valora y comenta.
    ///
    /// No es una <see cref="Propuesta"/>. Aquella es la promesa de una
    /// candidatura, con estado de cumplimiento y nivel de verificación. Esta
    /// no la prometió nadie y no hay nada que verificar: la tarjeta la
    /// identifica siempre como propuesta ciudadana, que es lo que sostiene la
    /// neutralidad frente a un texto que escribió cualquiera.
    /// </summary>
    public class Iniciativa
    {
        public int Id { get; set; }

        /// <summary>Nombre de quien la propuso. Nunca su login ni su correo.</summary>
        public string Autora { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public int CodigoCategoria { get; set; }

        public string Categoria { get; set; }

        /// <summary>Cero es «sin departamento»: alcance nacional.</summary>
        public int CodigoDepartamento { get; set; }

        public string Departamento { get; set; }

        /// <summary>Contadores derivados de las filas reales, nunca guardados.</summary>
        public int MeGusta { get; set; }

        public int NoMeGusta { get; set; }

        public int Comentarios { get; set; }

        /// <summary>MeGusta − NoMeGusta. Es lo que ordena la portada.</summary>
        public int Saldo { get; set; }

        /// <summary>Voto de quien consulta: 1, −1 o 0.</summary>
        public int MiValoracion { get; set; }

        /// <summary>Si la propuso quien consulta.</summary>
        public bool EsMia { get; set; }

        public bool Activa { get; set; }

        public string MotivoBaja { get; set; }

        /// <summary>
        /// Si el texto todavía se puede cambiar. Lo decide la base: solo
        /// mientras nadie valoró ni comentó.
        /// </summary>
        public bool PuedeEditar { get; set; }

        public DateTime FechaRegistro { get; set; }

        /// <summary>Vacío si nunca se editó.</summary>
        public string FechaEdicion { get; set; }

        // --------------------------------------------------- Presentación

        public string CategoriaClase
        {
            get { return Vista.ClaseCategoria(Categoria); }
        }

        /// <summary>
        /// Modificador de la tarjeta según la categoría (gc-inic--salud,
        /// gc-inic--educacion…), derivado de la misma tabla que decide el
        /// chip, para que el filo y la etiqueta nunca digan colores distintos.
        /// Vacío cuando la categoría no tiene color asignado.
        /// </summary>
        public string ClaseCategoriaTarjeta
        {
            get
            {
                string chip = Vista.ClaseCategoria(Categoria);
                int i = chip.IndexOf("gc-chip--", StringComparison.Ordinal);
                if (i < 0) return string.Empty;

                return "gc-inic--" + chip.Substring(i + "gc-chip--".Length);
            }
        }

        public bool TieneDepartamento
        {
            get { return !string.IsNullOrEmpty(Departamento); }
        }

        public string AutoraIniciales
        {
            get { return Vista.InicialesDe(Autora); }
        }

        public string TiempoRelativo
        {
            get { return Vista.TiempoRelativo(FechaRegistro); }
        }

        public bool FueEditada
        {
            get { return !string.IsNullOrEmpty(FechaEdicion); }
        }

        public bool Retirada
        {
            get { return !Activa; }
        }
    }
}
