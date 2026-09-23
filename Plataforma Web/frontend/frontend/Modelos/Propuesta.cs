using System;

namespace frontend.Modelos
{
    /// <summary>
    /// Proyecto o propuesta de campaña.
    ///
    /// Es deliberadamente la misma entidad que la promesa política del marco
    /// teórico del proyecto: nace declarada por el candidato y más adelante
    /// admite un estado de cumplimiento y evidencias que la respalden. Modelarla
    /// una sola vez es lo que permite que el seguimiento de cumplimiento se
    /// construya encima sin duplicar el modelo de datos.
    /// </summary>
    public class Propuesta
    {
        public Propuesta()
        {
            Estado = EstadoPropuesta.Declarada;
            Verificacion = NivelVerificacion.Declarado;
        }

        public int Id { get; set; }
        public string EspacioSlug { get; set; }

        public string CandidatoSlug { get; set; }

        public string CampanaSlug { get; set; }

        // ------------------------------------------------------ Contenido

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        /// <summary>Problema que la propuesta busca solucionar.</summary>
        public string Problema { get; set; }

        public string Objetivo { get; set; }

        /// <summary>A quién beneficia y en qué magnitud.</summary>
        public string Beneficiarios { get; set; }

        /// <summary>Categoría temática según la taxonomía del proyecto.</summary>
        public string Categoria { get; set; }

        /// <summary>Ubicación cuando la propuesta es territorial.</summary>
        public string Ubicacion { get; set; }

        /// <summary>Período estimado de ejecución en texto.</summary>
        public string PeriodoEjecucion { get; set; }

        public EstadoPropuesta Estado { get; set; }

        /// <summary>Imagen o material de apoyo.</summary>
        public string ImagenUrl { get; set; }

        public string InformacionAdicional { get; set; }

        public NivelVerificacion Verificacion { get; set; }

        public DateTime FechaRegistro { get; set; }

        /* Participación ciudadana acumulada sobre la propuesta. */

        public int MeGusta { get; set; }

        public int NoMeGusta { get; set; }

        public int Comentarios { get; set; }

        // --------------------------------------------------- Presentación

        public string EstadoTexto
        {
            get { return Vista.TextoEstado(Estado); }
        }

        public string EstadoClase
        {
            get { return Vista.ClaseEstado(Estado); }
        }

        public string CategoriaClase
        {
            get { return Vista.ClaseCategoria(Categoria); }
        }

        public string VerificacionTexto
        {
            get { return Vista.TextoVerificacion(Verificacion); }
        }

        public string VerificacionClase
        {
            get { return Vista.ClaseVerificacion(Verificacion); }
        }

        public bool TieneImagen
        {
            get { return !string.IsNullOrEmpty(ImagenUrl); }
        }

        public string UbicacionTexto
        {
            get { return string.IsNullOrEmpty(Ubicacion) ? "Cobertura nacional" : Ubicacion; }
        }

        public string Url
        {
            get { return "~/Propuesta?id=" + Id; }
        }
    }

    /// <summary>
    /// Documento que la candidatura adjunta para respaldar un proyecto
    /// (script 25). Lo aporta la propia candidatura, así que es declarado
    /// como el resto del proyecto: la plataforma no lo revisó.
    /// </summary>
    public class ArchivoRespaldo
    {
        public int Id { get; set; }

        /// <summary>El nombre con que se subió. El nombre en disco no sale del backend.</summary>
        public string NombreOriginal { get; set; }

        public string TipoContenido { get; set; }

        public int TamanoBytes { get; set; }

        public DateTime FechaRegistro { get; set; }

        /// <summary>Dirección del manejador del frontend que entrega el archivo.</summary>
        public string Url
        {
            get { return frontend.Servicios.Archivos.Url(Id); }
        }

        /// <summary>«PDF» o «Imagen», para la lista.</summary>
        public string TipoTexto
        {
            get { return TipoContenido == "application/pdf" ? "PDF" : "Imagen"; }
        }

        /// <summary>El tamaño en KB o MB, con un decimal.</summary>
        public string TamanoTexto
        {
            get
            {
                return TamanoBytes >= 1024 * 1024
                    ? (TamanoBytes / 1024.0 / 1024.0).ToString("0.0") + " MB"
                    : Math.Max(1, TamanoBytes / 1024) + " KB";
            }
        }
    }
}
