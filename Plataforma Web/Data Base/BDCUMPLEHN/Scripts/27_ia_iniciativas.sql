/* ============================================================
   CumpleHN — 27. El asistente consulta las iniciativas ciudadanas
   ------------------------------------------------------------
   Una quinta herramienta para el asistente, consultar_iniciativas,
   resuelta por un solo procedimiento: spIAIniciativas.

   Qué entrega y qué no, y por qué:

     - Entrega el TÍTULO, la categoría, el departamento, la fecha
       y los apoyos de cada iniciativa, más los conteos por
       categoría y por departamento.
     - NO entrega la descripción. Es texto libre de hasta 1,500
       caracteres que escribió cualquier cuenta ciudadana, sin
       moderación previa, y ahí cabe una instrucción dirigida al
       modelo. Es la misma razón por la que Comentarios queda
       fuera del asistente. El título también lo escribe la
       ciudadanía, pero son 120 caracteres y el prompt ya trata
       todo lo que entra por herramientas como dato.
     - NO entrega quién la propuso. vwIniciativas vincula a una
       persona con lo que propuso, y por eso el script 18 le niega
       la vista y la tabla a cumplehn_ia. Esos DENY se conservan:
       el procedimiento llega a ellas por encadenamiento de
       propiedad y proyecta solo las columnas de arriba.

   Como el resto del asistente, solo lee el espacio de la
   plataforma, y solo las iniciativas activas. Si la
   administración ocultó el módulo de iniciativas, no devuelve
   nada: el asistente no debe mostrar lo que el sitio escondió.

   El GRANT va acá y no en el 13 para no tener que volver a
   correr aquel, que pide la clave del login. Por el mismo motivo
   el procedimiento va con CREATE OR ALTER: un DROP se llevaría
   el GRANT en silencio.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Cuatro resultados, en este orden:

     1. Resumen: iniciativas que coinciden con los filtros.
     2. Por categoría, con iniciativas y apoyos.
     3. Por departamento. Las que no nombran uno son de todo el
        país, y así se rotulan.
     4. El listado, de la más apoyada a la menos, con el mismo
        criterio de la portada: saldo y, a igual saldo, la más
        reciente primero.

   Los tres primeros no dependen de @limite, para que los conteos
   no mientan cuando el listado viene recortado. */

CREATE OR ALTER PROCEDURE dbo.spIAIniciativas
    @texto              NVARCHAR(120) = NULL,
    @codigoCategoria    INT           = NULL,
    @codigoDepartamento INT           = NULL,
    @limite             INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @plataforma INT = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1);

    /* Módulo oculto: los cuatro resultados salen vacíos, con la
       misma forma, para que quien los lee no tenga un caso aparte. */
    DECLARE @visible BIT = ISNULL((SELECT visible FROM dbo.vwModulosEfectivos
                                    WHERE clave = N'iniciativas'), 1);

    IF @limite IS NULL OR @limite > 50 SET @limite = 50;
    IF @limite < 1 SET @limite = 1;

    SELECT i.codigoIniciativa,
           i.titulo,
           i.categoria,
           CASE WHEN i.codigoDepartamento = 0 THEN N'Todo el país'
                ELSE i.departamento END AS departamento,
           i.meGusta,
           i.noMeGusta,
           i.comentarios,
           i.saldo,
           i.fechaRegistro
      INTO #filtradas
      FROM dbo.vwIniciativas i
     WHERE @visible = 1
       AND i.activo = 1
       AND i.codigoEspacio = @plataforma
       AND (@codigoCategoria    IS NULL OR i.codigoCategoria    = @codigoCategoria)
       AND (@codigoDepartamento IS NULL OR i.codigoDepartamento = @codigoDepartamento)
       AND (@texto IS NULL OR i.titulo LIKE N'%' + @texto + N'%');

    SELECT COUNT(*)                   AS iniciativas,
           ISNULL(SUM(meGusta), 0)    AS apoyos,
           ISNULL(SUM(noMeGusta), 0)  AS enContra
      FROM #filtradas;

    SELECT categoria,
           COUNT(*)       AS iniciativas,
           SUM(meGusta)   AS apoyos,
           SUM(noMeGusta) AS enContra
      FROM #filtradas
     GROUP BY categoria
     ORDER BY COUNT(*) DESC, categoria;

    SELECT departamento,
           COUNT(*)       AS iniciativas,
           SUM(meGusta)   AS apoyos,
           SUM(noMeGusta) AS enContra
      FROM #filtradas
     GROUP BY departamento
     ORDER BY COUNT(*) DESC, departamento;

    SELECT TOP (@limite)
           codigoIniciativa,
           titulo,
           categoria,
           departamento,
           meGusta,
           noMeGusta,
           comentarios,
           CONVERT(CHAR(10), fechaRegistro, 23) AS fecha
      FROM #filtradas
     ORDER BY saldo DESC, fechaRegistro DESC;
END
GO

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'cumplehn_ia')
    GRANT EXECUTE ON dbo.spIAIniciativas TO cumplehn_ia;
GO

PRINT '27_ia_iniciativas.sql aplicado.';
GO
