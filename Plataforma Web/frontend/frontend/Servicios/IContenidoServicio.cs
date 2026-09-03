using System.Collections.Generic;
using frontend.Modelos;

namespace frontend.Servicios
{
    /// <summary>
    /// Contrato único por el que las páginas obtienen contenido.
    ///
    /// Ninguna página consulta datos por su cuenta: todas pasan por acá. Esa es
    /// la razón de existir de esta interfaz — cuando el Web Service ASMX del
    /// backend esté listo, se agrega una implementación que lo consuma y se
    /// cambia una sola línea en <see cref="Contenido"/>, sin tocar el frontend.
    /// </summary>
    public interface IContenidoServicio
    {
        // ------------------------------------------------------- Campañas

        IList<Campana> ObtenerCampanas();

        /// <summary>Campaña destacada en la portada. Puede devolver null.</summary>
        Campana ObtenerCampanaActual();

        /// <summary>Campaña por slug. Devuelve null si no existe.</summary>
        Campana ObtenerCampana(string slug);

        // ----------------------------------------------------- Candidatos

        /// <summary>Candidatos de una campaña. Slug vacío devuelve todos.</summary>
        IList<Candidato> ObtenerCandidatos(string campanaSlug);

        Candidato ObtenerCandidato(string slug);

        /// <summary>
        /// Candidato autenticado en el panel privado. Mientras no exista
        /// autenticación real, devuelve un candidato de demostración.
        /// </summary>
        Candidato ObtenerCandidatoAutenticado();

        // ----------------------------------------------------- Propuestas

        /// <summary>Propuestas de un candidato.</summary>
        IList<Propuesta> ObtenerPropuestas(string candidatoSlug);

        /// <summary>Propuestas de una campaña completa.</summary>
        IList<Propuesta> ObtenerPropuestasDeCampana(string campanaSlug);

        Propuesta ObtenerPropuesta(int id);

        // --------------------------------------------------- Publicaciones

        /// <summary>Feed de una campaña, ordenado de la más reciente a la más antigua.</summary>
        IList<Publicacion> ObtenerFeed(string campanaSlug);

        IList<Publicacion> ObtenerPublicaciones(string candidatoSlug);

        // ------------------------------------------------------- Partidos

        IList<Partido> ObtenerPartidos();

        Partido ObtenerPartido(string slug);

        /// <summary>Candidaturas afiliadas a un partido.</summary>
        IList<Candidato> ObtenerCandidatosDePartido(string partidoSlug);

        // --------------------------------------------------- Interacción

        /// <summary>
        /// Participación acumulada sobre un objeto. Con
        /// <paramref name="codigoUsuario"/> en cero devuelve solo los totales,
        /// que es el caso de un visitante sin cuenta.
        /// </summary>
        Interaccion ObtenerInteraccion(string tipoObjeto, int codigoObjeto, int codigoUsuario);

        /// <summary>
        /// Registra un me gusta (valor 1) o un no me gusta (valor -1). Repetir
        /// el mismo voto lo retira y votar lo contrario lo cambia.
        /// </summary>
        Resultado Valorar(string tipoObjeto, int codigoObjeto, int codigoUsuario, int valor);

        IList<Comentario> ObtenerComentarios(string tipoObjeto, int codigoObjeto);

        Resultado AgregarComentario(string tipoObjeto, int codigoObjeto, int codigoUsuario, string texto);

        // ----------------------------------------------------- Analítica

        /// <summary>
        /// Tablero completo para una selección de filtros. Con la campaña
        /// vacía usa la destacada.
        ///
        /// Devuelve siempre un objeto: cuando el backend no responde llega con
        /// <c>Error</c> en verdadero, y cuando la selección no tiene registros
        /// llega con <c>SinDatos</c>. La página distingue los dos casos, porque
        /// «no hay datos» y «no se pudo consultar» piden mensajes distintos.
        /// </summary>
        Analitica ObtenerAnalitica(FiltroAnalitica filtro);

        // ------------------------------------------------ Administración

        /* Todo lo de esta sección exige rol Administrador. El código de usuario
           viaja como parámetro y el backend confirma el rol contra la base: lo
           que el frontend sabe de su sesión decide qué se muestra, nunca qué se
           permite. Cuando la cuenta no tiene el rol, las consultas devuelven
           listas vacías y las acciones un Resultado en falso. */

        /// <summary>
        /// Cola de trabajo de la verificación, con candidaturas, propuestas y
        /// publicaciones juntas. Con <paramref name="soloPendientes"/> deja
        /// fuera lo ya verificado.
        /// </summary>
        IList<ItemVerificacion> ObtenerBandejaVerificacion(
            int codigoUsuario, string tipoObjeto, string campanaSlug, bool soloPendientes);

        /// <summary>
        /// Asigna el nivel de verificación. El motivo es donde queda anotada la
        /// fuente, y el backend lo exige al marcar como verificado.
        /// </summary>
        Resultado CambiarVerificacion(
            int codigoUsuario, string tipoObjeto, int codigoObjeto,
            int codigoVerificacion, string motivo);

        /// <summary>
        /// Publicaciones para moderación, incluidas las retiradas.
        /// <paramref name="estado"/> acepta Activas, Retiradas o vacío.
        /// </summary>
        IList<PublicacionModerada> ObtenerPublicacionesModeracion(
            int codigoUsuario, string campanaSlug, string estado);

        /// <summary>
        /// Retira una publicación de la consulta pública, o la restaura. Es una
        /// baja lógica: la fila se conserva con su motivo y su responsable.
        /// </summary>
        Resultado ModerarPublicacion(
            int codigoUsuario, int codigoPublicacion, bool activa, string motivo);

        /// <summary>Bitácora de administración, de lo más reciente a lo más antiguo.</summary>
        IList<RegistroAuditoria> ObtenerAuditoria(int codigoUsuario, string accion, int limite);

        // ------------------------------------------------------ Catálogos

        /// <summary>Categorías temáticas disponibles para clasificar propuestas.</summary>
        IList<string> ObtenerCategorias();

        /// <summary>Cargos de elección popular a los que puede aspirar un candidato.</summary>
        IList<string> ObtenerCargos();

        IList<string> ObtenerDepartamentos();

        /// <summary>
        /// Niveles de verificación con su código, que es lo que necesita el
        /// desplegable de la bandeja. Los otros catálogos devuelven solo el
        /// nombre porque las páginas públicas filtran por texto.
        /// </summary>
        IList<OpcionCatalogo> ObtenerNivelesVerificacion();
    }
}
