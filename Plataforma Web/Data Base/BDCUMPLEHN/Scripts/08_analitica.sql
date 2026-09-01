/* ============================================================
   CumpleHN — 08. Procedimientos del módulo de analítica
   ------------------------------------------------------------
   Todo el cálculo del tablero vive acá. El Web Service no
   escribe SQL: solo invoca estos procedimientos con los filtros
   que eligió la persona usuaria y transporta el resultado.

   Por qué procedimientos y no vistas: el tablero filtra por
   campaña, categoría, partido, departamento, nivel de gobierno
   y rango de fechas de forma combinada. Una vista no acepta
   parámetros, así que la alternativa era armar el WHERE por
   concatenación dentro del Web Service — exactamente lo que el
   anexo OWASP del proyecto se compromete a no hacer.

   Convenciones de todos los procedimientos:

     - Cada parámetro es opcional. NULL o cadena vacía significa
       «sin filtrar por esta dimensión». El patrón
       (@p IS NULL OR columna = @p) lo resuelve en una línea y
       deja un solo plan de consulta por procedimiento.

     - La agregación se hace en la base, nunca en el navegador.
       Cada procedimiento devuelve decenas de filas, no miles.

     - Ninguno concatena texto dentro del SQL. El valor recibido
       llega siempre como parámetro tipado.

   Se puede volver a ejecutar: cada procedimiento se recrea.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* ============================================================
   1. Catálogos de los filtros

   Devuelve en una sola llamada todo lo que llena los
   desplegables del tablero. Se hace así para no abrir seis
   consultas cuando la página carga.

   Solo se ofrecen valores que existen en los datos, salvo los
   departamentos: ahí interesa mostrar los 18 para que la
   ausencia de candidaturas en 15 de ellos sea visible.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaCatalogos') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaCatalogos;
GO

CREATE PROCEDURE dbo.spAnaliticaCatalogos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT N'Campana' AS grupo, CAST(codigoCampana AS NVARCHAR(20)) AS codigo,
           slug AS valor, nombre AS texto, codigoCampana AS orden
    FROM dbo.Campanas

    UNION ALL
    SELECT N'Categoria', CAST(codigoCategoria AS NVARCHAR(20)),
           CAST(codigoCategoria AS NVARCHAR(20)), nombre, orden
    FROM dbo.Categorias

    UNION ALL
    SELECT N'Partido', CAST(codigoPartido AS NVARCHAR(20)),
           CAST(codigoPartido AS NVARCHAR(20)), ISNULL(siglas + N' — ', N'') + nombre, codigoPartido
    FROM dbo.Partidos
    WHERE activo = 1

    UNION ALL
    SELECT N'Departamento', CAST(codigoDepartamento AS NVARCHAR(20)),
           CAST(codigoDepartamento AS NVARCHAR(20)), nombre, codigoDepartamento
    FROM dbo.Departamentos

    UNION ALL
    SELECT DISTINCT N'NivelGobierno', nivelGobierno, nivelGobierno, nivelGobierno,
           CASE nivelGobierno WHEN N'Nacional' THEN 1 WHEN N'Departamental' THEN 2 ELSE 3 END
    FROM dbo.Cargos

    ORDER BY grupo, orden;
END
GO

/* ============================================================
   2. Resumen

   Las cifras de encabezado del tablero. Cada una viene con su
   denominador, porque el número solo no concluye nada: «0
   propuestas verificadas» no dice lo mismo que «0 de 11».

   Devuelve además la fecha del hecho más reciente que entra en
   la selección, que es la que el encabezado muestra como
   «última actualización». Es un dato de los registros, no la
   hora del servidor.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaResumen') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaResumen;
GO

CREATE PROCEDURE dbo.spAnaliticaResumen
    @campanaSlug       NVARCHAR(80)  = NULL,
    @codigoCategoria   INT           = NULL,
    @codigoPartido     INT           = NULL,
    @codigoDepartamento INT          = NULL,
    @nivelGobierno     NVARCHAR(20)  = NULL,
    @desde             DATE          = NULL,
    @hasta             DATE          = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    /* Candidaturas que sobreviven al filtro. Es el universo sobre el que
       se calculan los promedios por candidatura. */
    DECLARE @candidaturas TABLE (codigoCandidato INT PRIMARY KEY, codigoDepartamento INT, verificacion NVARCHAR(40));

    INSERT INTO @candidaturas (codigoCandidato, codigoDepartamento, verificacion)
    SELECT codigoCandidato, codigoDepartamento, verificacion
    FROM dbo.vwAnaliticaCandidaturas
    WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
      AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
      AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
      AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno);

    /* Propuestas que sobreviven al filtro. */
    DECLARE @propuestas TABLE (codigoPropuesta INT PRIMARY KEY, codigoCategoria INT,
                               estado NVARCHAR(40), ponderacion DECIMAL(4,2), verificacion NVARCHAR(40));

    INSERT INTO @propuestas (codigoPropuesta, codigoCategoria, estado, ponderacion, verificacion)
    SELECT codigoPropuesta, codigoCategoria, estado, ponderacion, verificacion
    FROM dbo.vwAnaliticaPropuestas
    WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
      AND (@codigoCategoria    IS NULL OR codigoCategoria    = @codigoCategoria)
      AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
      AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
      AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno);

    SELECT
        /* --- Oferta programática */
        (SELECT COUNT(*) FROM @propuestas)                                       AS propuestas,
        (SELECT COUNT(*) FROM @candidaturas)                                     AS candidaturas,
        (SELECT COUNT(DISTINCT codigoCategoria) FROM @propuestas)                AS categoriasConOferta,
        (SELECT COUNT(*) FROM dbo.Categorias)                                    AS categoriasTotal,
        (SELECT COUNT(*) FROM @propuestas WHERE estado <> N'Declarada')          AS propuestasEvaluadas,
        (SELECT COUNT(*) FROM @propuestas WHERE verificacion = N'Verificado')    AS propuestasVerificadas,

        /* --- Confianza */
        (SELECT COUNT(*) FROM @candidaturas WHERE verificacion = N'Verificado')  AS candidaturasVerificadas,

        /* --- Cobertura territorial: los 18 departamentos son el denominador */
        (SELECT COUNT(DISTINCT codigoDepartamento) FROM @candidaturas
          WHERE codigoDepartamento IS NOT NULL)                                  AS departamentosConCandidatura,
        (SELECT COUNT(*) FROM dbo.Departamentos)                                 AS departamentosTotal,
        (SELECT COUNT(DISTINCT codigoPartido) FROM dbo.vwAnaliticaCandidaturas
          WHERE codigoCandidato IN (SELECT codigoCandidato FROM @candidaturas)
            AND codigoPartido IS NOT NULL)                                       AS partidos,

        /* --- Participación ciudadana */
        v.valoraciones, v.meGusta, v.noMeGusta, v.personas,
        /* Los comentarios se filtran por las mismas dimensiones que las
           valoraciones. Si solo respetaran la campaña, la tarjeta mostraría
           «29 valoraciones · 16 comentarios» para una categoría en la que los
           16 comentarios no son todos de esa categoría. */
        (SELECT COUNT(*) FROM dbo.vwAnaliticaComentarios c
          WHERE (@campanaSlug        IS NULL OR c.campanaSlug        = @campanaSlug)
            AND (@codigoCategoria    IS NULL OR c.codigoCategoria    = @codigoCategoria)
            AND (@codigoPartido      IS NULL OR c.codigoPartido      = @codigoPartido)
            AND (@codigoDepartamento IS NULL OR c.codigoDepartamento = @codigoDepartamento)
            AND (@nivelGobierno      IS NULL OR c.nivelGobierno      = @nivelGobierno)
            AND (@desde              IS NULL OR c.fecha             >= @desde)
            AND (@hasta              IS NULL OR c.fecha             <= @hasta))   AS comentarios,

        (SELECT COUNT(*) FROM dbo.vwAnaliticaPublicaciones b
          WHERE (@campanaSlug        IS NULL OR b.campanaSlug        = @campanaSlug)
            AND (@codigoPartido      IS NULL OR b.codigoPartido      = @codigoPartido)
            AND (@codigoDepartamento IS NULL OR b.codigoDepartamento = @codigoDepartamento)
            AND (@nivelGobierno      IS NULL OR b.nivelGobierno      = @nivelGobierno))
                                                                                 AS publicaciones,

        /* --- Fecha del hecho más reciente que entra en la selección */
        (SELECT MAX(f) FROM (
            SELECT MAX(fecha) AS f FROM dbo.vwAnaliticaValoraciones
             WHERE (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
            UNION ALL
            SELECT MAX(fecha) FROM dbo.vwAnaliticaComentarios
             WHERE (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
            UNION ALL
            SELECT MAX(fecha) FROM dbo.vwAnaliticaPublicaciones
             WHERE (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
            UNION ALL
            SELECT MAX(fecha) FROM dbo.vwAnaliticaPropuestas
             WHERE (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
         ) AS u)                                                                 AS ultimoRegistro
    FROM (
        /* Los ISNULL no son decorativos: sobre un conjunto vacío —una campaña
           todavía sin actividad— SUM devuelve NULL, y el tablero tiene que
           mostrar cero, que es el dato verdadero. */
        SELECT
            COUNT(*)                                                      AS valoraciones,
            ISNULL(SUM(CASE WHEN valor =  1 THEN 1 ELSE 0 END), 0)        AS meGusta,
            ISNULL(SUM(CASE WHEN valor = -1 THEN 1 ELSE 0 END), 0)        AS noMeGusta,
            COUNT(DISTINCT codigoUsuario)                                 AS personas
        FROM dbo.vwAnaliticaValoraciones
        WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
          AND (@codigoCategoria    IS NULL OR codigoCategoria    = @codigoCategoria)
          AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno)
          AND (@desde              IS NULL OR fecha             >= @desde)
          AND (@hasta              IS NULL OR fecha             <= @hasta)
    ) AS v;
END
GO

/* ============================================================
   3. Oferta por categoría

   El indicador central del proyecto: contrasta lo que la
   ciudadanía dijo que le importa (encuesta, n = 150) con lo que
   las candidaturas efectivamente proponen.

   Devuelve el conteo crudo. La conversión a porcentaje sobre el
   total se hace en el frontend, porque depende del subconjunto
   que quedó tras el filtro y tiene que sumar 100 % en pantalla.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaCategorias') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaCategorias;
GO

CREATE PROCEDURE dbo.spAnaliticaCategorias
    @campanaSlug        NVARCHAR(80) = NULL,
    @codigoPartido      INT          = NULL,
    @codigoDepartamento INT          = NULL,
    @nivelGobierno      NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    SELECT
        cat.codigoCategoria,
        cat.nombre                          AS categoria,
        cat.orden,
        ISNULL(cat.interesEncuesta, 0)      AS interesEncuesta,
        COUNT(p.codigoPropuesta)            AS propuestas,
        COUNT(DISTINCT p.codigoCandidato)   AS candidaturas
    FROM dbo.Categorias cat
    LEFT JOIN dbo.vwAnaliticaPropuestas p
           ON p.codigoCategoria = cat.codigoCategoria
          AND (@campanaSlug        IS NULL OR p.campanaSlug        = @campanaSlug)
          AND (@codigoPartido      IS NULL OR p.codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR p.codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR p.nivelGobierno      = @nivelGobierno)
    GROUP BY cat.codigoCategoria, cat.nombre, cat.orden, cat.interesEncuesta
    ORDER BY cat.orden;
END
GO

/* ============================================================
   4. Estados de cumplimiento

   Devuelve los siete estados siempre, incluso los que están en
   cero. Un estado ausente del gráfico se leería como que no
   existe en el esquema, cuando lo que ocurre es que ninguna
   propuesta llegó todavía a él.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaEstados') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaEstados;
GO

CREATE PROCEDURE dbo.spAnaliticaEstados
    @campanaSlug        NVARCHAR(80) = NULL,
    @codigoCategoria    INT          = NULL,
    @codigoPartido      INT          = NULL,
    @codigoDepartamento INT          = NULL,
    @nivelGobierno      NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    SELECT
        ep.codigoEstado,
        ep.nombre                   AS estado,
        ep.descripcion,
        ep.orden,
        ep.ponderacion,
        COUNT(p.codigoPropuesta)    AS propuestas
    FROM dbo.EstadosPropuesta ep
    LEFT JOIN dbo.vwAnaliticaPropuestas p
           ON p.codigoEstado = ep.codigoEstado
          AND (@campanaSlug        IS NULL OR p.campanaSlug        = @campanaSlug)
          AND (@codigoCategoria    IS NULL OR p.codigoCategoria    = @codigoCategoria)
          AND (@codigoPartido      IS NULL OR p.codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR p.codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR p.nivelGobierno      = @nivelGobierno)
    GROUP BY ep.codigoEstado, ep.nombre, ep.descripcion, ep.orden, ep.ponderacion
    ORDER BY ep.orden;
END
GO

/* ============================================================
   5. Verificación por tipo de entidad

   Una fila por combinación de entidad y nivel. Muestra de un
   vistazo qué proporción de lo publicado está respaldado por
   una fuente y qué proporción sigue siendo solo la palabra de
   la candidatura. Es el indicador que sostiene la neutralidad
   declarada del proyecto.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaVerificacion') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaVerificacion;
GO

CREATE PROCEDURE dbo.spAnaliticaVerificacion
    @campanaSlug        NVARCHAR(80) = NULL,
    @codigoCategoria    INT          = NULL,
    @codigoPartido      INT          = NULL,
    @codigoDepartamento INT          = NULL,
    @nivelGobierno      NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    SELECT e.entidad, e.entidadOrden, nv.nombre AS nivel, nv.orden,
           ISNULL(x.total, 0) AS total
    FROM dbo.NivelesVerificacion nv
    CROSS JOIN (VALUES (N'Candidaturas', 1), (N'Propuestas', 2), (N'Publicaciones', 3))
               AS e (entidad, entidadOrden)
    LEFT JOIN (
        SELECT N'Candidaturas' AS entidad, verificacion, COUNT(*) AS total
        FROM dbo.vwAnaliticaCandidaturas
        WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
          AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno)
        GROUP BY verificacion

        UNION ALL

        SELECT N'Propuestas', verificacion, COUNT(*)
        FROM dbo.vwAnaliticaPropuestas
        WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
          AND (@codigoCategoria    IS NULL OR codigoCategoria    = @codigoCategoria)
          AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno)
        GROUP BY verificacion

        UNION ALL

        SELECT N'Publicaciones', verificacion, COUNT(*)
        FROM dbo.vwAnaliticaPublicaciones
        WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
          AND (@codigoCategoria    IS NULL OR codigoCategoria    = @codigoCategoria)
          AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno)
        GROUP BY verificacion
    ) AS x ON x.entidad = e.entidad AND x.verificacion = nv.nombre
    ORDER BY e.entidadOrden, nv.orden;
END
GO

/* ============================================================
   6. Participación por tipo de contenido

   Sobre qué tipo de objeto opina la ciudadanía y con qué signo.
   Responde dónde se concentra la conversación de la plataforma.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaParticipacion') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaParticipacion;
GO

CREATE PROCEDURE dbo.spAnaliticaParticipacion
    @campanaSlug        NVARCHAR(80) = NULL,
    @codigoCategoria    INT          = NULL,
    @codigoPartido      INT          = NULL,
    @codigoDepartamento INT          = NULL,
    @nivelGobierno      NVARCHAR(20) = NULL,
    @desde              DATE         = NULL,
    @hasta              DATE         = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    SELECT
        t.nombre                        AS tipoObjeto,
        t.codigoTipoObjeto,
        ISNULL(v.meGusta, 0)            AS meGusta,
        ISNULL(v.noMeGusta, 0)          AS noMeGusta,
        ISNULL(c.comentarios, 0)        AS comentarios
    FROM dbo.TiposObjeto t
    LEFT JOIN (
        SELECT tipoObjeto,
               SUM(CASE WHEN valor =  1 THEN 1 ELSE 0 END) AS meGusta,
               SUM(CASE WHEN valor = -1 THEN 1 ELSE 0 END) AS noMeGusta
        FROM dbo.vwAnaliticaValoraciones
        WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
          AND (@codigoCategoria    IS NULL OR codigoCategoria    = @codigoCategoria)
          AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno)
          AND (@desde              IS NULL OR fecha             >= @desde)
          AND (@hasta              IS NULL OR fecha             <= @hasta)
        GROUP BY tipoObjeto
    ) AS v ON v.tipoObjeto = t.nombre
    LEFT JOIN (
        SELECT tipoObjeto, COUNT(*) AS comentarios
        FROM dbo.vwAnaliticaComentarios
        WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
          AND (@codigoCategoria    IS NULL OR codigoCategoria    = @codigoCategoria)
          AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno)
          AND (@desde              IS NULL OR fecha             >= @desde)
          AND (@hasta              IS NULL OR fecha             <= @hasta)
        GROUP BY tipoObjeto
    ) AS c ON c.tipoObjeto = t.nombre
    ORDER BY (ISNULL(v.meGusta, 0) + ISNULL(v.noMeGusta, 0)) DESC, t.nombre;
END
GO

/* ============================================================
   7. Ranking de candidaturas

   Compara candidatura contra candidatura en las tres medidas
   que la base sostiene: cuánto propone, cuánta reacción genera
   y con qué signo.

   El saldo (a favor menos en contra) es la columna por la que
   se ordena. Un ranking por total de valoraciones premiaría a
   quien genera rechazo igual que a quien genera respaldo.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaCandidatos') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaCandidatos;
GO

CREATE PROCEDURE dbo.spAnaliticaCandidatos
    @campanaSlug        NVARCHAR(80) = NULL,
    @codigoCategoria    INT          = NULL,
    @codigoPartido      INT          = NULL,
    @codigoDepartamento INT          = NULL,
    @nivelGobierno      NVARCHAR(20) = NULL,
    @desde              DATE         = NULL,
    @hasta              DATE         = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    SELECT
        k.codigoCandidato,
        k.candidatoSlug,
        k.candidato,
        ISNULL(k.partidoSiglas, N'Independiente')   AS partidoSiglas,
        ISNULL(k.departamento, N'Sin departamento') AS departamento,
        k.cargo,
        k.nivelGobierno,
        k.verificacion,
        ISNULL(p.propuestas, 0)                     AS propuestas,
        ISNULL(v.meGusta, 0)                        AS meGusta,
        ISNULL(v.noMeGusta, 0)                      AS noMeGusta,
        ISNULL(v.meGusta, 0) - ISNULL(v.noMeGusta, 0) AS saldo,
        ISNULL(c.comentarios, 0)                    AS comentarios
    FROM dbo.vwAnaliticaCandidaturas k
    LEFT JOIN (
        SELECT codigoCandidato, COUNT(*) AS propuestas
        FROM dbo.vwAnaliticaPropuestas
        WHERE (@codigoCategoria IS NULL OR codigoCategoria = @codigoCategoria)
        GROUP BY codigoCandidato
    ) AS p ON p.codigoCandidato = k.codigoCandidato
    LEFT JOIN (
        SELECT codigoCandidato,
               SUM(CASE WHEN valor =  1 THEN 1 ELSE 0 END) AS meGusta,
               SUM(CASE WHEN valor = -1 THEN 1 ELSE 0 END) AS noMeGusta
        FROM dbo.vwAnaliticaValoraciones
        WHERE codigoCandidato IS NOT NULL
          AND (@codigoCategoria IS NULL OR codigoCategoria = @codigoCategoria)
          AND (@desde           IS NULL OR fecha          >= @desde)
          AND (@hasta           IS NULL OR fecha          <= @hasta)
        GROUP BY codigoCandidato
    ) AS v ON v.codigoCandidato = k.codigoCandidato
    LEFT JOIN (
        SELECT codigoCandidato, COUNT(*) AS comentarios
        FROM dbo.vwAnaliticaComentarios
        WHERE codigoCandidato IS NOT NULL
          AND (@codigoCategoria IS NULL OR codigoCategoria = @codigoCategoria)
          AND (@desde           IS NULL OR fecha          >= @desde)
          AND (@hasta           IS NULL OR fecha          <= @hasta)
        GROUP BY codigoCandidato
    ) AS c ON c.codigoCandidato = k.codigoCandidato
    WHERE (@campanaSlug        IS NULL OR k.campanaSlug        = @campanaSlug)
      AND (@codigoPartido      IS NULL OR k.codigoPartido      = @codigoPartido)
      AND (@codigoDepartamento IS NULL OR k.codigoDepartamento = @codigoDepartamento)
      AND (@nivelGobierno      IS NULL OR k.nivelGobierno      = @nivelGobierno)
    ORDER BY saldo DESC, propuestas DESC, k.candidato;
END
GO

/* ============================================================
   8. Ranking de partidos

   Además del conteo bruto devuelve las propuestas por
   candidatura. Sin ese cociente, el gráfico solo mide cuántas
   candidaturas inscribió cada partido y no la densidad
   programática de ninguno.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaPartidos') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaPartidos;
GO

CREATE PROCEDURE dbo.spAnaliticaPartidos
    @campanaSlug        NVARCHAR(80) = NULL,
    @codigoCategoria    INT          = NULL,
    @codigoDepartamento INT          = NULL,
    @nivelGobierno      NVARCHAR(20) = NULL,
    @desde              DATE         = NULL,
    @hasta              DATE         = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    /* Las candidaturas independientes no tienen partido. Se agrupan bajo una
       fila propia para que su oferta no desaparezca del total. */
    ;WITH base AS (
        SELECT ISNULL(k.codigoPartido, 0)                   AS codigoPartido,
               ISNULL(k.partidoSiglas, N'Independientes')   AS partidoSiglas,
               ISNULL(k.partido, N'Candidaturas independientes') AS partido,
               k.codigoCandidato
        FROM dbo.vwAnaliticaCandidaturas k
        WHERE (@campanaSlug        IS NULL OR k.campanaSlug        = @campanaSlug)
          AND (@codigoDepartamento IS NULL OR k.codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR k.nivelGobierno      = @nivelGobierno)
    )
    SELECT
        b.codigoPartido,
        b.partidoSiglas,
        b.partido,
        COUNT(DISTINCT b.codigoCandidato)               AS candidaturas,
        ISNULL(SUM(p.propuestas), 0)                    AS propuestas,
        CAST(CASE WHEN COUNT(DISTINCT b.codigoCandidato) = 0 THEN 0
                  ELSE ISNULL(SUM(p.propuestas), 0) * 1.0
                       / COUNT(DISTINCT b.codigoCandidato) END AS DECIMAL(6,2))
                                                        AS propuestasPorCandidatura,
        ISNULL(SUM(v.meGusta), 0)                       AS meGusta,
        ISNULL(SUM(v.noMeGusta), 0)                     AS noMeGusta
    FROM base b
    LEFT JOIN (
        SELECT codigoCandidato, COUNT(*) AS propuestas
        FROM dbo.vwAnaliticaPropuestas
        WHERE (@codigoCategoria IS NULL OR codigoCategoria = @codigoCategoria)
        GROUP BY codigoCandidato
    ) AS p ON p.codigoCandidato = b.codigoCandidato
    LEFT JOIN (
        SELECT codigoCandidato,
               SUM(CASE WHEN valor =  1 THEN 1 ELSE 0 END) AS meGusta,
               SUM(CASE WHEN valor = -1 THEN 1 ELSE 0 END) AS noMeGusta
        FROM dbo.vwAnaliticaValoraciones
        WHERE codigoCandidato IS NOT NULL
          AND (@codigoCategoria IS NULL OR codigoCategoria = @codigoCategoria)
          AND (@desde           IS NULL OR fecha          >= @desde)
          AND (@hasta           IS NULL OR fecha          <= @hasta)
        GROUP BY codigoCandidato
    ) AS v ON v.codigoCandidato = b.codigoCandidato
    GROUP BY b.codigoPartido, b.partidoSiglas, b.partido
    ORDER BY propuestas DESC, candidaturas DESC, b.partidoSiglas;
END
GO

/* ============================================================
   9. Cobertura territorial

   Devuelve los 18 departamentos siempre, con cero cuando no
   tienen candidaturas. El hallazgo del indicador no está en los
   departamentos cubiertos sino en los que faltan, y una
   consulta que solo trajera los que tienen datos escondería
   justamente eso.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaTerritorio') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaTerritorio;
GO

CREATE PROCEDURE dbo.spAnaliticaTerritorio
    @campanaSlug     NVARCHAR(80) = NULL,
    @codigoCategoria INT          = NULL,
    @codigoPartido   INT          = NULL,
    @nivelGobierno   NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    SELECT
        d.codigoDepartamento,
        d.nombre                            AS departamento,
        ISNULL(k.candidaturas, 0)           AS candidaturas,
        ISNULL(p.propuestas, 0)             AS propuestas,
        ISNULL(v.valoraciones, 0)           AS valoraciones
    FROM dbo.Departamentos d
    LEFT JOIN (
        SELECT codigoDepartamento, COUNT(*) AS candidaturas
        FROM dbo.vwAnaliticaCandidaturas
        WHERE codigoDepartamento IS NOT NULL
          AND (@campanaSlug   IS NULL OR campanaSlug   = @campanaSlug)
          AND (@codigoPartido IS NULL OR codigoPartido = @codigoPartido)
          AND (@nivelGobierno IS NULL OR nivelGobierno = @nivelGobierno)
        GROUP BY codigoDepartamento
    ) AS k ON k.codigoDepartamento = d.codigoDepartamento
    LEFT JOIN (
        SELECT codigoDepartamento, COUNT(*) AS propuestas
        FROM dbo.vwAnaliticaPropuestas
        WHERE codigoDepartamento IS NOT NULL
          AND (@campanaSlug     IS NULL OR campanaSlug     = @campanaSlug)
          AND (@codigoCategoria IS NULL OR codigoCategoria = @codigoCategoria)
          AND (@codigoPartido   IS NULL OR codigoPartido   = @codigoPartido)
          AND (@nivelGobierno   IS NULL OR nivelGobierno   = @nivelGobierno)
        GROUP BY codigoDepartamento
    ) AS p ON p.codigoDepartamento = d.codigoDepartamento
    LEFT JOIN (
        SELECT codigoDepartamento, COUNT(*) AS valoraciones
        FROM dbo.vwAnaliticaValoraciones
        WHERE codigoDepartamento IS NOT NULL
          AND (@campanaSlug   IS NULL OR campanaSlug   = @campanaSlug)
          AND (@codigoPartido IS NULL OR codigoPartido = @codigoPartido)
          AND (@nivelGobierno IS NULL OR nivelGobierno = @nivelGobierno)
        GROUP BY codigoDepartamento
    ) AS v ON v.codigoDepartamento = d.codigoDepartamento
    ORDER BY candidaturas DESC, propuestas DESC, d.nombre;
END
GO

/* ============================================================
   10. Actividad registrada por día

   Serie diaria de la participación. La escala de fechas se
   genera sobre el rango realmente observado en los datos, de
   modo que un día sin actividad aparezca como cero y no se
   omita de la línea.

   No es una serie de tendencia: con el volumen actual describe
   los días registrados, y así lo declara la página. El
   procedimiento igual está escrito para un rango de cualquier
   tamaño, con un tope de un año para que un dato erróneo no
   genere una serie infinita.
   ============================================================ */

IF OBJECT_ID('dbo.spAnaliticaActividad') IS NOT NULL
    DROP PROCEDURE dbo.spAnaliticaActividad;
GO

CREATE PROCEDURE dbo.spAnaliticaActividad
    @campanaSlug        NVARCHAR(80) = NULL,
    @codigoPartido      INT          = NULL,
    @codigoDepartamento INT          = NULL,
    @nivelGobierno      NVARCHAR(20) = NULL,
    @desde              DATE         = NULL,
    @hasta              DATE         = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @campanaSlug   = NULLIF(LTRIM(RTRIM(@campanaSlug)), N'');
    SET @nivelGobierno = NULLIF(LTRIM(RTRIM(@nivelGobierno)), N'');

    /* Sin rango explícito se usa el que abarcan los propios datos. */
    IF @desde IS NULL
        SELECT @desde = MIN(f) FROM (
            SELECT MIN(fecha) AS f FROM dbo.vwAnaliticaValoraciones
             WHERE (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
            UNION ALL
            SELECT MIN(fecha) FROM dbo.vwAnaliticaComentarios
             WHERE (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
        ) AS u;

    IF @hasta IS NULL
        SELECT @hasta = MAX(f) FROM (
            SELECT MAX(fecha) AS f FROM dbo.vwAnaliticaValoraciones
             WHERE (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
            UNION ALL
            SELECT MAX(fecha) FROM dbo.vwAnaliticaComentarios
             WHERE (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
        ) AS u;

    IF @desde IS NULL OR @hasta IS NULL OR @hasta < @desde
    BEGIN
        /* Sin actividad registrada la serie es vacía, no cero. */
        SELECT CAST(NULL AS DATE) AS fecha, 0 AS meGusta, 0 AS noMeGusta, 0 AS comentarios
        WHERE 1 = 0;
        RETURN;
    END

    IF DATEDIFF(DAY, @desde, @hasta) > 366
        SET @desde = DATEADD(DAY, -366, @hasta);

    ;WITH dias AS (
        SELECT @desde AS fecha
        UNION ALL
        SELECT DATEADD(DAY, 1, fecha) FROM dias WHERE fecha < @hasta
    )
    SELECT
        d.fecha,
        ISNULL(v.meGusta, 0)     AS meGusta,
        ISNULL(v.noMeGusta, 0)   AS noMeGusta,
        ISNULL(c.comentarios, 0) AS comentarios
    FROM dias d
    LEFT JOIN (
        SELECT fecha,
               SUM(CASE WHEN valor =  1 THEN 1 ELSE 0 END) AS meGusta,
               SUM(CASE WHEN valor = -1 THEN 1 ELSE 0 END) AS noMeGusta
        FROM dbo.vwAnaliticaValoraciones
        WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
          AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno)
        GROUP BY fecha
    ) AS v ON v.fecha = d.fecha
    LEFT JOIN (
        SELECT fecha, COUNT(*) AS comentarios
        FROM dbo.vwAnaliticaComentarios
        WHERE (@campanaSlug        IS NULL OR campanaSlug        = @campanaSlug)
          AND (@codigoPartido      IS NULL OR codigoPartido      = @codigoPartido)
          AND (@codigoDepartamento IS NULL OR codigoDepartamento = @codigoDepartamento)
          AND (@nivelGobierno      IS NULL OR nivelGobierno      = @nivelGobierno)
        GROUP BY fecha
    ) AS c ON c.fecha = d.fecha
    ORDER BY d.fecha
    OPTION (MAXRECURSION 400);
END
GO

PRINT 'Procedimientos de analitica creados.';
GO
