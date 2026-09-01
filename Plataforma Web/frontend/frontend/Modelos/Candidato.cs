namespace frontend.Modelos
{
    /// <summary>
    /// Candidato de una campaña electoral. El propio candidato administra estos
    /// datos desde su panel, por lo que todo el contenido nace con nivel de
    /// verificación <see cref="NivelVerificacion.Declarado"/>.
    /// </summary>
    public class Candidato
    {
        public Candidato()
        {
            Verificacion = NivelVerificacion.Declarado;
            Nivel = NivelGobierno.Nacional;
        }

        /// <summary>
        /// Código numérico. Hace falta para las valoraciones y los comentarios,
        /// que referencian el objeto por su código y no por su slug.
        /// </summary>
        public int Id { get; set; }

        /// <summary>Identificador legible usado en la URL.</summary>
        public string Slug { get; set; }

        public string CampanaSlug { get; set; }

        // ------------------------------------------------ Identificación

        public string Nombres { get; set; }

        public string Apellidos { get; set; }

        /// <summary>Vacío cuando la candidatura es independiente.</summary>
        public string Partido { get; set; }

        public string PartidoSiglas { get; set; }

        /// <summary>Identificador del partido en la URL. Vacío si es independiente.</summary>
        public string PartidoSlug { get; set; }

        public string Cargo { get; set; }

        public NivelGobierno Nivel { get; set; }

        public string Departamento { get; set; }

        public string Municipio { get; set; }

        // ---------------------------------------------------- Contenido

        /// <summary>Ruta de la fotografía. Si está vacía se muestran las iniciales.</summary>
        public string FotoUrl { get; set; }

        /// <summary>Una línea que resume la candidatura.</summary>
        public string Titular { get; set; }

        public string Biografia { get; set; }

        public string InformacionProfesional { get; set; }

        public string DescripcionCandidatura { get; set; }

        // ----------------------- Contacto que el candidato hace público

        public string CorreoPublico { get; set; }

        public string Telefono { get; set; }

        public string SitioWeb { get; set; }

        public string Facebook { get; set; }

        public string X { get; set; }

        public string Instagram { get; set; }

        // ------------------------------------------------------ Estado

        public NivelVerificacion Verificacion { get; set; }

        public int TotalPropuestas { get; set; }

        public int TotalPublicaciones { get; set; }

        /* Participación ciudadana acumulada sobre el perfil. */

        public int MeGusta { get; set; }

        public int NoMeGusta { get; set; }

        public int Comentarios { get; set; }

        public bool TienePartido
        {
            get { return !string.IsNullOrEmpty(PartidoSlug); }
        }

        public string UrlPartido
        {
            get { return "~/Partido?id=" + PartidoSlug; }
        }

        // ------------------------------------------------- Presentación

        public string NombreCompleto
        {
            get { return (Nombres + " " + Apellidos).Trim(); }
        }

        public string Iniciales
        {
            get { return Vista.Iniciales(Nombres, Apellidos); }
        }

        public bool TieneFoto
        {
            get { return !string.IsNullOrEmpty(FotoUrl); }
        }

        public string PartidoTexto
        {
            get { return string.IsNullOrEmpty(Partido) ? "Candidatura independiente" : Partido; }
        }

        /// <summary>Territorio al que corresponde el cargo, si aplica.</summary>
        public string Territorio
        {
            get
            {
                if (!string.IsNullOrEmpty(Municipio) && !string.IsNullOrEmpty(Departamento))
                    return Municipio + ", " + Departamento;
                if (!string.IsNullOrEmpty(Departamento))
                    return Departamento;
                return "Honduras";
            }
        }

        public string VerificacionTexto
        {
            get { return Vista.TextoVerificacion(Verificacion); }
        }

        public string VerificacionClase
        {
            get { return Vista.ClaseVerificacion(Verificacion); }
        }

        public string Url
        {
            get { return "~/Candidato?id=" + Slug; }
        }

        /// <summary>
        /// Porcentaje de campos del perfil que ya tienen contenido. Alimenta el
        /// medidor de avance del panel del candidato.
        /// </summary>
        public int PerfilCompleto
        {
            get
            {
                string[] campos =
                {
                    Nombres, Apellidos, Cargo, Titular, Biografia,
                    InformacionProfesional, DescripcionCandidatura,
                    CorreoPublico, FotoUrl, Departamento
                };

                int llenos = 0;
                for (int i = 0; i < campos.Length; i++)
                {
                    if (!string.IsNullOrEmpty(campos[i])) llenos++;
                }

                return (int)((llenos * 100.0) / campos.Length);
            }
        }
    }
}
