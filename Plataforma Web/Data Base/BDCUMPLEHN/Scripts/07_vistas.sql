/* ============================================================
   CumpleHN — 07. Vistas de detalle del módulo de analítica
   ------------------------------------------------------------
   El tablero necesita filtrar por campaña, categoría, partido,
   departamento y nivel de gobierno al mismo tiempo. Una vista no
   acepta parámetros, así que el filtro vive en los
   procedimientos del script 08.

   Lo que sí resuelve una vista es el otro problema: dejar cada
   hecho de la base ya cruzado con todas sus dimensiones, una
   sola vez y en un objeto documentable. Cada vista de acá
   devuelve UNA FILA POR HECHO — una propuesta, una candidatura,
   una valoración — con las columnas por las que el tablero
   puede agrupar o filtrar.

   Los procedimientos del script 08 se reducen entonces a un
   WHERE y un GROUP BY sobre estas vistas. Ese es el reparto:
   la vista define qué significa cada cifra, el procedimiento
   define sobre qué subconjunto se calcula.

   Se puede volver a ejecutar: cada vista se recrea.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* ============================================================
   El interés ciudadano medido en la encuesta del proyecto
   (n = 150) se guarda junto a la categoría, de modo que el
   tablero pueda contrastar lo que la ciudadanía prioriza con lo
   que las candidaturas efectivamente proponen.
   ============================================================ */

IF COL_LENGTH('dbo.Categorias', 'interesEncuesta') IS NULL
BEGIN
    ALTER TABLE dbo.Categorias ADD interesEncuesta DECIMAL(5,2) NULL;
END
GO

UPDATE dbo.Categorias SET interesEncuesta = 25.30 WHERE nombre = N'Seguridad y orden público';
UPDATE dbo.Categorias SET interesEncuesta = 23.30 WHERE nombre = N'Salud';
UPDATE dbo.Categorias SET interesEncuesta = 18.00 WHERE nombre = N'Educación';
UPDATE dbo.Categorias SET interesEncuesta = 14.70 WHERE nombre = N'Economía y empleo';
UPDATE dbo.Categorias SET interesEncuesta = 14.00 WHERE nombre = N'Infraestructura';
/* Transparencia no se ofreció como opción cerrada en la encuesta: quedó dentro
   de «Otro», con 4.7 %. Se registra ese valor para no dejar el dato en blanco. */
UPDATE dbo.Categorias SET interesEncuesta = 4.70 WHERE nombre = N'Transparencia y gobernanza';
GO

/* ============================================================
   Las vistas del diseño anterior calculaban cada indicador ya
   agregado y solo por campaña. Se reemplazan por las vistas de
   detalle de abajo, que permiten agregar por cualquier
   dimensión sin escribir una vista nueva por gráfico.
   ============================================================ */

IF OBJECT_ID('dbo.vwPropuestasPorCategoria')  IS NOT NULL DROP VIEW dbo.vwPropuestasPorCategoria;
IF OBJECT_ID('dbo.vwPropuestasPorEstado')     IS NOT NULL DROP VIEW dbo.vwPropuestasPorEstado;
IF OBJECT_ID('dbo.vwCumplimientoPorCandidato') IS NOT NULL DROP VIEW dbo.vwCumplimientoPorCandidato;
IF OBJECT_ID('dbo.vwVerificacionCandidatos')  IS NOT NULL DROP VIEW dbo.vwVerificacionCandidatos;
IF OBJECT_ID('dbo.vwParticipacionPorTipo')    IS NOT NULL DROP VIEW dbo.vwParticipacionPorTipo;
IF OBJECT_ID('dbo.vwApoyoPorCandidato')       IS NOT NULL DROP VIEW dbo.vwApoyoPorCandidato;
IF OBJECT_ID('dbo.vwPropuestasPorPartido')    IS NOT NULL DROP VIEW dbo.vwPropuestasPorPartido;
GO

/* ==================================== 1. Candidaturas

   Una fila por candidatura, con todas las dimensiones por las
   que el tablero filtra. Es la vista de la que salen la
   cobertura territorial, la verificación de perfiles y el
   denominador de casi todos los promedios.

   El partido queda nulo en las candidaturas independientes.
   Eso es información, no un dato faltante, así que las
   consultas lo tratan como una categoría más y no lo descartan.
   ==================================================== */

IF OBJECT_ID('dbo.vwAnaliticaCandidaturas') IS NOT NULL
    DROP VIEW dbo.vwAnaliticaCandidaturas;
GO

CREATE VIEW dbo.vwAnaliticaCandidaturas
AS
SELECT
    k.codigoCandidato,
    k.slug                              AS candidatoSlug,
    (k.nombres + ' ' + k.apellidos)     AS candidato,
    ca.codigoCampana,
    ca.slug                             AS campanaSlug,
    k.codigoPartido,
    pa.nombre                           AS partido,
    ISNULL(pa.siglas, pa.nombre)        AS partidoSiglas,
    k.codigoDepartamento,
    d.nombre                            AS departamento,
    g.codigoCargo,
    g.nombre                            AS cargo,
    g.nivelGobierno,
    k.codigoVerificacion,
    nv.nombre                           AS verificacion,
    nv.orden                            AS verificacionOrden,
    k.activo
FROM dbo.Candidatos k
INNER JOIN dbo.Campanas ca            ON ca.codigoCampana = k.codigoCampana
INNER JOIN dbo.Cargos g               ON g.codigoCargo = k.codigoCargo
INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = k.codigoVerificacion
LEFT  JOIN dbo.Partidos pa            ON pa.codigoPartido = k.codigoPartido
LEFT  JOIN dbo.Departamentos d        ON d.codigoDepartamento = k.codigoDepartamento;
GO

/* ====================================== 2. Propuestas

   Una fila por propuesta. Hereda del candidato el partido, el
   departamento y el nivel de gobierno, porque una propuesta no
   los guarda por su cuenta y el tablero necesita poder cortar
   la oferta programática por esas dimensiones.

   La ponderación del estado es la que convierte el conteo de
   propuestas en porcentaje de cumplimiento.
   ==================================================== */

IF OBJECT_ID('dbo.vwAnaliticaPropuestas') IS NOT NULL
    DROP VIEW dbo.vwAnaliticaPropuestas;
GO

CREATE VIEW dbo.vwAnaliticaPropuestas
AS
SELECT
    p.codigoPropuesta,
    p.nombre                            AS propuesta,
    ca.slug                             AS campanaSlug,
    p.codigoCategoria,
    cat.nombre                          AS categoria,
    cat.orden                           AS categoriaOrden,
    ISNULL(cat.interesEncuesta, 0)      AS interesEncuesta,
    p.codigoEstado,
    ep.nombre                           AS estado,
    ep.orden                            AS estadoOrden,
    ep.ponderacion,
    p.codigoVerificacion,
    nv.nombre                           AS verificacion,
    nv.orden                            AS verificacionOrden,
    k.codigoCandidato,
    k.candidatoSlug,
    k.candidato,
    k.codigoPartido,
    k.partido,
    k.partidoSiglas,
    k.codigoDepartamento,
    k.departamento,
    k.nivelGobierno,
    CAST(p.fechaRegistro AS DATE)       AS fecha
FROM dbo.Propuestas p
INNER JOIN dbo.Campanas ca               ON ca.codigoCampana = p.codigoCampana
INNER JOIN dbo.Categorias cat            ON cat.codigoCategoria = p.codigoCategoria
INNER JOIN dbo.EstadosPropuesta ep       ON ep.codigoEstado = p.codigoEstado
INNER JOIN dbo.NivelesVerificacion nv    ON nv.codigoVerificacion = p.codigoVerificacion
INNER JOIN dbo.vwAnaliticaCandidaturas k ON k.codigoCandidato = p.codigoCandidato;
GO

/* =================================== 3. Publicaciones */

IF OBJECT_ID('dbo.vwAnaliticaPublicaciones') IS NOT NULL
    DROP VIEW dbo.vwAnaliticaPublicaciones;
GO

CREATE VIEW dbo.vwAnaliticaPublicaciones
AS
SELECT
    b.codigoPublicacion,
    ca.slug                             AS campanaSlug,
    b.codigoCategoria,
    cat.nombre                          AS categoria,
    b.codigoVerificacion,
    nv.nombre                           AS verificacion,
    nv.orden                            AS verificacionOrden,
    k.codigoCandidato,
    k.candidatoSlug,
    k.candidato,
    k.codigoPartido,
    k.partidoSiglas,
    k.codigoDepartamento,
    k.departamento,
    k.nivelGobierno,
    CAST(b.fecha AS DATE)               AS fecha
FROM dbo.Publicaciones b
INNER JOIN dbo.Campanas ca               ON ca.codigoCampana = b.codigoCampana
INNER JOIN dbo.NivelesVerificacion nv    ON nv.codigoVerificacion = b.codigoVerificacion
INNER JOIN dbo.vwAnaliticaCandidaturas k ON k.codigoCandidato = b.codigoCandidato
LEFT  JOIN dbo.Categorias cat            ON cat.codigoCategoria = b.codigoCategoria;
GO

/* ==================================== 4. Valoraciones

   Acá se resuelve la referencia polimórfica. Valoraciones
   apunta a cualquier objeto con el par (codigoTipoObjeto,
   codigoObjeto), y el tablero necesita saber a qué candidatura
   y a qué campaña pertenece cada voto para poder filtrarlo.

   La resolución se escribe como una UNION de cuatro ramas, una
   por tipo, en vez de un CASE con subconsultas: cada rama deja
   explícito por qué columna se enlaza, que es justamente lo que
   el Manual Técnico tiene que documentar.

   Las valoraciones sobre un partido no tienen candidatura
   asociada, así que candidato queda nulo. Las consultas que
   agrupan por candidatura las descartan sin perder el total,
   que se sigue leyendo de la vista completa.
   ==================================================== */

IF OBJECT_ID('dbo.vwAnaliticaValoraciones') IS NOT NULL
    DROP VIEW dbo.vwAnaliticaValoraciones;
GO

CREATE VIEW dbo.vwAnaliticaValoraciones
AS
/* --- Voto sobre el perfil de una candidatura */
SELECT  v.codigoValoracion, N'Candidato' AS tipoObjeto, v.codigoObjeto,
        v.codigoUsuario, v.valor, CAST(v.fecha AS DATE) AS fecha,
        k.codigoCandidato, k.candidato, k.candidatoSlug, k.campanaSlug,
        k.codigoPartido, k.partidoSiglas, k.codigoDepartamento, k.departamento,
        k.nivelGobierno, NULL AS codigoCategoria
FROM dbo.Valoraciones v
INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto AND t.nombre = N'Candidato'
INNER JOIN dbo.vwAnaliticaCandidaturas k ON k.codigoCandidato = v.codigoObjeto

UNION ALL

/* --- Voto sobre una propuesta: hereda la candidatura que la registró */
SELECT  v.codigoValoracion, N'Propuesta', v.codigoObjeto,
        v.codigoUsuario, v.valor, CAST(v.fecha AS DATE),
        p.codigoCandidato, p.candidato, p.candidatoSlug, p.campanaSlug,
        p.codigoPartido, p.partidoSiglas, p.codigoDepartamento, p.departamento,
        p.nivelGobierno, p.codigoCategoria
FROM dbo.Valoraciones v
INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto AND t.nombre = N'Propuesta'
INNER JOIN dbo.vwAnaliticaPropuestas p ON p.codigoPropuesta = v.codigoObjeto

UNION ALL

/* --- Voto sobre una publicación: hereda la candidatura que la publicó */
SELECT  v.codigoValoracion, N'Publicacion', v.codigoObjeto,
        v.codigoUsuario, v.valor, CAST(v.fecha AS DATE),
        b.codigoCandidato, b.candidato, b.candidatoSlug, b.campanaSlug,
        b.codigoPartido, b.partidoSiglas, b.codigoDepartamento, b.departamento,
        b.nivelGobierno, b.codigoCategoria
FROM dbo.Valoraciones v
INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto AND t.nombre = N'Publicacion'
INNER JOIN dbo.vwAnaliticaPublicaciones b ON b.codigoPublicacion = v.codigoObjeto

UNION ALL

/* --- Voto sobre un partido: no pertenece a ninguna candidatura */
SELECT  v.codigoValoracion, N'Partido', v.codigoObjeto,
        v.codigoUsuario, v.valor, CAST(v.fecha AS DATE),
        NULL, NULL, NULL, NULL,
        pa.codigoPartido, ISNULL(pa.siglas, pa.nombre), NULL, NULL,
        NULL, NULL
FROM dbo.Valoraciones v
INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto AND t.nombre = N'Partido'
INNER JOIN dbo.Partidos pa ON pa.codigoPartido = v.codigoObjeto;
GO

/* ===================================== 5. Comentarios

   Misma resolución polimórfica que las valoraciones. Solo
   entran los comentarios aprobados: un comentario oculto por
   moderación no debe contar como participación.
   ==================================================== */

IF OBJECT_ID('dbo.vwAnaliticaComentarios') IS NOT NULL
    DROP VIEW dbo.vwAnaliticaComentarios;
GO

CREATE VIEW dbo.vwAnaliticaComentarios
AS
SELECT  c.codigoComentario, N'Candidato' AS tipoObjeto, c.codigoObjeto,
        c.codigoUsuario, CAST(c.fecha AS DATE) AS fecha,
        k.codigoCandidato, k.candidatoSlug, k.campanaSlug,
        k.codigoPartido, k.codigoDepartamento, k.nivelGobierno,
        CAST(NULL AS INT) AS codigoCategoria
FROM dbo.Comentarios c
INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = c.codigoTipoObjeto AND t.nombre = N'Candidato'
INNER JOIN dbo.vwAnaliticaCandidaturas k ON k.codigoCandidato = c.codigoObjeto
WHERE c.aprobado = 1

UNION ALL

SELECT  c.codigoComentario, N'Propuesta', c.codigoObjeto,
        c.codigoUsuario, CAST(c.fecha AS DATE),
        p.codigoCandidato, p.candidatoSlug, p.campanaSlug,
        p.codigoPartido, p.codigoDepartamento, p.nivelGobierno,
        p.codigoCategoria
FROM dbo.Comentarios c
INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = c.codigoTipoObjeto AND t.nombre = N'Propuesta'
INNER JOIN dbo.vwAnaliticaPropuestas p ON p.codigoPropuesta = c.codigoObjeto
WHERE c.aprobado = 1

UNION ALL

SELECT  c.codigoComentario, N'Publicacion', c.codigoObjeto,
        c.codigoUsuario, CAST(c.fecha AS DATE),
        b.codigoCandidato, b.candidatoSlug, b.campanaSlug,
        b.codigoPartido, b.codigoDepartamento, b.nivelGobierno,
        b.codigoCategoria
FROM dbo.Comentarios c
INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = c.codigoTipoObjeto AND t.nombre = N'Publicacion'
INNER JOIN dbo.vwAnaliticaPublicaciones b ON b.codigoPublicacion = c.codigoObjeto
WHERE c.aprobado = 1

UNION ALL

SELECT  c.codigoComentario, N'Partido', c.codigoObjeto,
        c.codigoUsuario, CAST(c.fecha AS DATE),
        NULL, NULL, NULL,
        pa.codigoPartido, NULL, NULL,
        NULL
FROM dbo.Comentarios c
INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = c.codigoTipoObjeto AND t.nombre = N'Partido'
INNER JOIN dbo.Partidos pa ON pa.codigoPartido = c.codigoObjeto
WHERE c.aprobado = 1;
GO

PRINT 'Vistas de detalle de analitica creadas.';
GO
