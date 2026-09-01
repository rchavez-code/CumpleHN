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

        // ------------------------------------------------------ Catálogos

        /// <summary>Categorías temáticas disponibles para clasificar propuestas.</summary>
        IList<string> ObtenerCategorias();

        /// <summary>Cargos de elección popular a los que puede aspirar un candidato.</summary>
        IList<string> ObtenerCargos();

        IList<string> ObtenerDepartamentos();
    }
}
