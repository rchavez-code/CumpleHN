using System;

namespace frontend.Modelos
{
    /// <summary>
    /// Niveles de verificación, tal como los guarda el catálogo
    /// NivelesVerificacion. Se declaran una sola vez acá para que ninguna
    /// página compare contra una cadena escrita a mano.
    /// </summary>
    public static class NivelesVerificacion
    {
        public const string Declarado = "Declarado";
        public const string EnRevision = "En revisión";
        public const string Verificado = "Verificado";
    }

    /// <summary>
    /// Par código y nombre de una entrada de catálogo.
    ///
    /// Los catálogos de las páginas públicas devuelven solo el nombre, porque
    /// allá se filtra por texto. Acá hace falta el código: la verificación se
    /// guarda por código, no por la etiqueta que se lee en pantalla.
    /// </summary>
    public class OpcionCatalogo
    {
        public int Codigo { get; set; }
        public string Nombre { get; set; }
    }


    /// <summary>
    /// Resultado de un guardado, con el código del registro afectado. En un
    /// alta es el código recién creado: la página lo usa para encadenar el
    /// siguiente paso sin volver a buscar el registro.
    /// </summary>
    public class ResultadoGuardado
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public int Codigo { get; set; }
    }

    /// <summary>
    /// Partido visto desde la administración: incluye los desactivados y
    /// cuenta sus candidaturas activas.
    /// </summary>
    public class PartidoAdmin
    {
        public int Codigo { get; set; }
        public string Slug { get; set; }
        public string Nombre { get; set; }
        public string Siglas { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public int Candidaturas { get; set; }

        public string EstadoTexto
        {
            get { return Activo ? "Activo" : "Desactivado"; }
        }

        public string EstadoClase
        {
            get { return Activo ? "gc-chip gc-chip--cumplida" : "gc-chip gc-chip--estancada"; }
        }

        public string SiglasTexto
        {
            get { return string.IsNullOrEmpty(Siglas) ? "—" : Siglas; }
        }

        public string Url
        {
            get { return "~/Partido?id=" + Slug; }
        }
    }

    /// <summary>
    /// Campaña vista desde la administración.
    /// </summary>
    public class CampanaAdmin
    {
        public int Codigo { get; set; }
        public string Slug { get; set; }
        public string Nombre { get; set; }
        public string Resumen { get; set; }
        public string Descripcion { get; set; }
        public string Alcance { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaEleccion { get; set; }
        public string Estado { get; set; }
        public bool EsActual { get; set; }
        public int Candidaturas { get; set; }
        public int Propuestas { get; set; }

        public string PeriodoTexto
        {
            get { return Vista.FechaCorta(FechaInicio) + " — " + Vista.FechaCorta(FechaEleccion); }
        }

        public string EstadoTexto
        {
            get
            {
                if (Estado == "Activa") return "En curso";
                if (Estado == "Proxima") return "Próxima";
                return "Cerrada";
            }
        }

        public string EstadoClase
        {
            get
            {
                if (Estado == "Activa") return "gc-chip gc-chip--cumplida";
                if (Estado == "Proxima") return "gc-chip gc-chip--declarada";
                return "gc-chip gc-chip--estancada";
            }
        }

        public string Url
        {
            get { return "~/Campana?id=" + Slug; }
        }
    }

    /// <summary>
    /// Candidatura vista desde la administración. <see cref="Login"/> queda
    /// vacío cuando todavía no tiene cuenta de acceso, y sin cuenta no puede
    /// administrar su propio perfil ni registrar propuestas.
    /// </summary>
    public class CandidatoAdmin
    {
        public int Codigo { get; set; }
        public string Slug { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string NombreCompleto { get; set; }

        public int CodigoCampana { get; set; }
        public string CampanaSlug { get; set; }
        public string Campana { get; set; }

        public int CodigoPartido { get; set; }
        public string Partido { get; set; }

        public int CodigoCargo { get; set; }
        public string Cargo { get; set; }

        public int CodigoDepartamento { get; set; }
        public string Departamento { get; set; }
        public string Municipio { get; set; }
        public string Titular { get; set; }

        public string Verificacion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }

        public string Login { get; set; }
        public int Propuestas { get; set; }

        public bool TieneCuenta
        {
            get { return !string.IsNullOrEmpty(Login); }
        }

        /// <summary>
        /// Partido en texto. Una candidatura independiente no es un dato
        /// faltante: es información, y se dice con todas sus letras.
        /// </summary>
        public string PartidoTexto
        {
            get { return string.IsNullOrEmpty(Partido) ? "Independiente" : Partido; }
        }

        public string CuentaTexto
        {
            get { return TieneCuenta ? Login : "Sin cuenta"; }
        }

        public string CuentaClase
        {
            get { return TieneCuenta ? "gc-chip gc-chip--cumplida" : "gc-chip gc-chip--proceso"; }
        }

        public string EstadoTexto
        {
            get { return Activo ? "Activa" : "Retirada"; }
        }

        public string EstadoClase
        {
            get { return Activo ? "gc-chip gc-chip--cumplida" : "gc-chip gc-chip--incumplida"; }
        }

        public string VerificacionClase
        {
            get { return Vista.ClaseVerificacion(Vista.NivelDe(Verificacion)); }
        }

        public string Url
        {
            get { return "~/Candidato?id=" + Slug; }
        }
    }


    /// <summary>
    /// Estado de un elemento apagable, tal como lo consulta el sitio.
    /// Deliberadamente mínimo: viaja en cada carga de página.
    /// </summary>
    public class EstadoModulo
    {
        public string Clave { get; set; }
        public bool Visible { get; set; }
    }

    /// <summary>
    /// Un elemento apagable visto desde la administración.
    ///
    /// <see cref="Habilitado"/> es su propio interruptor y <see cref="Visible"/>
    /// ya tiene en cuenta al padre. Un gráfico encendido dentro de un tablero
    /// apagado no está visible, y la pantalla tiene que poder explicarlo en
    /// lugar de mostrar dos estados que se contradicen.
    /// </summary>
    public class ModuloAdmin
    {
        public int Codigo { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Grupo { get; set; }
        public string ClavePadre { get; set; }

        public bool Habilitado { get; set; }
        public bool Visible { get; set; }
        public bool ApagadoPorPadre { get; set; }

        public DateTime FechaCambio { get; set; }
        public string CambiadoPor { get; set; }

        public bool EsHijo
        {
            get { return !string.IsNullOrEmpty(ClavePadre); }
        }

        public string EstadoTexto
        {
            get
            {
                if (ApagadoPorPadre) return "Oculto por su módulo";
                return Habilitado ? "Visible" : "Oculto";
            }
        }

        public string EstadoClase
        {
            get
            {
                if (ApagadoPorPadre) return "gc-chip gc-chip--estancada";
                return Habilitado ? "gc-chip gc-chip--cumplida" : "gc-chip gc-chip--incumplida";
            }
        }

        /// <summary>
        /// Cuándo y quién lo cambió por última vez. Vacío si nunca se tocó, que
        /// es distinto de no saberlo.
        /// </summary>
        public string UltimoCambio
        {
            get
            {
                if (FechaCambio == DateTime.MinValue) return string.Empty;
                return Vista.FechaHora(FechaCambio) + " · " + CambiadoPor;
            }
        }
    }

    /// <summary>
    /// Una fila de la bandeja de verificación. Puede ser una candidatura, una
    /// propuesta o una publicación: los tres tipos que llevan nivel de
    /// verificación llegan en la misma cola de trabajo, ordenados por lo más
    /// antiguo sin verificar.
    /// </summary>
    public class ItemVerificacion
    {
        public string TipoObjeto { get; set; }
        public int CodigoObjeto { get; set; }

        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Slug { get; set; }

        public string Candidato { get; set; }
        public string CandidatoSlug { get; set; }
        public string CampanaSlug { get; set; }

        public int CodigoVerificacion { get; set; }
        public string Verificacion { get; set; }
        public int VerificacionOrden { get; set; }

        public DateTime Fecha { get; set; }

        public bool EstaVerificado
        {
            get { return VerificacionOrden >= 3; }
        }

        /// <summary>Clase del distintivo, reutilizando la del resto del sitio.</summary>
        public string VerificacionClase
        {
            get { return Vista.ClaseVerificacion(Vista.NivelDe(Verificacion)); }
        }

        public string FechaTexto
        {
            get { return Vista.FechaCorta(Fecha); }
        }

        /// <summary>
        /// Nombre del tipo para mostrar. La base lo guarda sin tilde porque es
        /// una clave del catálogo, no un texto de pantalla.
        /// </summary>
        public string TipoTexto
        {
            get
            {
                if (TipoObjeto == TiposObjeto.Publicacion) return "Publicación";
                if (TipoObjeto == TiposObjeto.Propuesta) return "Propuesta";
                if (TipoObjeto == TiposObjeto.Candidato) return "Candidatura";
                return TipoObjeto;
            }
        }

        /// <summary>
        /// Dirección de la página pública del contenido, para poder revisarlo
        /// antes de decidir. Verificar sin leer no sería verificar.
        /// </summary>
        public string Url
        {
            get
            {
                if (TipoObjeto == TiposObjeto.Candidato)
                    return "~/Candidato?id=" + CandidatoSlug;

                if (TipoObjeto == TiposObjeto.Propuesta)
                    return "~/Propuesta?id=" + CodigoObjeto;

                // Las publicaciones no tienen página propia: se leen en el
                // perfil de la candidatura que las publicó.
                return "~/Candidato?id=" + CandidatoSlug;
            }
        }
    }

    /// <summary>
    /// Publicación vista desde la moderación. A diferencia de las del feed,
    /// incluye las retiradas con el motivo y el responsable del retiro: sin
    /// poder verlas, un retiro por error sería irreversible en la práctica.
    /// </summary>
    public class PublicacionModerada
    {
        public int CodigoPublicacion { get; set; }
        public string Texto { get; set; }
        public DateTime Fecha { get; set; }

        public bool Activa { get; set; }
        public string MotivoBaja { get; set; }
        public string RetiradaPor { get; set; }

        public string Candidato { get; set; }
        public string CandidatoSlug { get; set; }
        public string CampanaSlug { get; set; }
        public string Categoria { get; set; }
        public string Verificacion { get; set; }

        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }
        public int Comentarios { get; set; }

        public string FechaTexto
        {
            get { return Vista.FechaCorta(Fecha); }
        }

        public string VerificacionClase
        {
            get { return Vista.ClaseVerificacion(Vista.NivelDe(Verificacion)); }
        }

        public bool TieneCategoria
        {
            get { return !string.IsNullOrEmpty(Categoria); }
        }

        /// <summary>
        /// Participación acumulada. Se muestra junto a la publicación porque
        /// retirar contenido con mucha participación no es lo mismo que
        /// retirar algo que nadie leyó, y quien modera debería saberlo antes
        /// de decidir.
        /// </summary>
        public int Participacion
        {
            get { return MeGusta + NoMeGusta + Comentarios; }
        }

        public string Extracto
        {
            get
            {
                if (string.IsNullOrEmpty(Texto)) return string.Empty;
                return Texto.Length <= 220 ? Texto : Texto.Substring(0, 220) + "…";
            }
        }
    }

    /// <summary>
    /// Una línea de la bitácora de administración.
    /// </summary>
    public class RegistroAuditoria
    {
        public int Codigo { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string TipoObjeto { get; set; }
        public int CodigoObjeto { get; set; }
        public string Detalle { get; set; }
        public string Motivo { get; set; }

        public string FechaTexto
        {
            get { return Vista.FechaHora(Fecha); }
        }

        /// <summary>
        /// Acción en texto de pantalla. La base la guarda sin tildes porque es
        /// una clave de la restricción CHECK.
        /// </summary>
        public string AccionTexto
        {
            get
            {
                if (Accion == "Verificacion") return "Verificación";
                if (Accion == "Retiro") return "Retiro";
                if (Accion == "Restauracion") return "Restauración";
                return Accion;
            }
        }

        /// <summary>
        /// Color según lo que hizo la acción: quitar contenido de la vista
        /// pública se distingue de devolverlo o de verificarlo.
        /// </summary>
        public string AccionClase
        {
            get
            {
                if (Accion == "Retiro") return "gc-chip gc-chip--incumplida";
                if (Accion == "Restauracion") return "gc-chip gc-chip--cumplida";
                return "gc-chip gc-chip--declarada";
            }
        }
    }
}
