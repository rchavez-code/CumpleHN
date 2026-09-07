/* ============================================================
   CumpleHN — 12. Asistente de consulta en lenguaje natural
   ------------------------------------------------------------
   Los objetos de base de datos que el asistente puede ejecutar,
   y nada más que esos.

   El modelo de lenguaje no escribe SQL. Recibe un conjunto
   cerrado de herramientas, cada una de las cuales es uno de
   estos procedimientos con parámetros tipados, y lo único que
   ve de la plataforma es la fila que el procedimiento le
   devuelve. Por eso el control de lo que puede saber no está en
   los permisos sino acá: en las columnas que cada SELECT nombra
   una por una.

   Tres reglas que sostienen el diseño:

     1. Ningún procedimiento de este script proyecta con *.
        Las columnas van nombradas para que agregar una a una
        tabla no la exponga sola al asistente. Quedan fuera a
        propósito el correo y el teléfono de las candidaturas, y
        toda columna de la tabla Usuarios.

     2. El asistente no ve participación individual.
        Valoraciones y Comentarios vinculan a una persona con su
        preferencia política, que es el dato más sensible de la
        base. Al asistente le llegan solo los agregados que ya
        calculan las vistas del script 07. Comentarios queda
        además fuera de su alcance porque su texto lo escribe
        cualquier ciudadano — leerlo sería dejar que un
        comentario le dé instrucciones al modelo.

     3. Todo resultado viene acotado. Lo que devuelve un
        procedimiento entra al contexto del modelo y se paga por
        token, así que las búsquedas tienen tope de filas y los
        textos largos viajan recortados. El detalle completo se
        pide de a una propuesta.

   La tabla ConsultasIA registra cada pregunta. Guarda lo que una
   persona identificada preguntó sobre política, así que tiene el
   mismo cuidado que Valoraciones: la escribe el backend con su
   propia conexión, no el asistente, y no se expone en ninguna
   pantalla pública.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* Misma razón que en los scripts 09 y 10: un procedimiento guarda
   para siempre el valor que tenían estas opciones al crearse, y
   sqlcmd trae QUOTED_IDENTIFIER apagado a diferencia de SSMS. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Registro de consultas

   Sirve para tres cosas: sostener la cuota diaria, dar evidencia
   del funcionamiento del asistente en el capítulo de resultados,
   y poder revisar qué se le preguntó cuando una respuesta salga
   mal.

   Guarda la respuesta completa a propósito. Sin ella no se puede
   evaluar el asistente después, que es justamente lo que el
   capítulo de resultados necesita medir.
   ============================================================ */

IF OBJECT_ID('dbo.ConsultasIA') IS NULL
CREATE TABLE dbo.ConsultasIA
(
    codigoConsulta INT            NOT NULL IDENTITY(1,1),
    codigoUsuario  INT            NOT NULL,
    pregunta       NVARCHAR(1000) NOT NULL,
    respuesta      NVARCHAR(MAX)  NULL,   -- NULL cuando la llamada falló
    respondio      BIT            NOT NULL CONSTRAINT DF_ConsultasIA_respondio DEFAULT (0),
    herramientas   NVARCHAR(300)  NULL,   -- las que invocó el modelo, separadas por coma
    modelo         NVARCHAR(60)   NULL,
    tokensEntrada  INT            NULL,
    tokensSalida   INT            NULL,
    milisegundos   INT            NULL,
    error          NVARCHAR(300)  NULL,
    fecha          DATETIME2(0)   NOT NULL CONSTRAINT DF_ConsultasIA_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_ConsultasIA PRIMARY KEY (codigoConsulta),
    CONSTRAINT FK_ConsultasIA_Usuarios
        FOREIGN KEY (codigoUsuario) REFERENCES dbo.Usuarios (codigoUsuario)
);
GO

/* La cuota diaria consulta por usuario y fecha. Sin este índice
   sería un recorrido completo de la tabla en cada pregunta. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ConsultasIA_usuarioFecha')
CREATE INDEX IX_ConsultasIA_usuarioFecha ON dbo.ConsultasIA (codigoUsuario, fecha DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ConsultasIA_fecha')
CREATE INDEX IX_ConsultasIA_fecha ON dbo.ConsultasIA (fecha DESC);
GO

/* ============================================================
   2. Cuota diaria

   Una caja de texto abierta al público contra un modelo de
   lenguaje es una factura sin techo. El tope vive acá y no en la
   página porque una validación que solo existe en el formulario
   se salta llamando al servicio.

   Devuelve una fila con lo usado y lo que queda. El backend la
   consulta antes de llamar al modelo — nunca después.
   ============================================================ */

IF OBJECT_ID('dbo.spIACuotaDisponible') IS NOT NULL
    DROP PROCEDURE dbo.spIACuotaDisponible;
GO

CREATE PROCEDURE dbo.spIACuotaDisponible
    @codigoUsuario INT,
    @limiteDiario  INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @usadas INT;

    SELECT @usadas = COUNT(*)
    FROM dbo.ConsultasIA
    WHERE codigoUsuario = @codigoUsuario
      AND CAST(fecha AS DATE) = CAST(SYSDATETIME() AS DATE);

    /* Una cuenta inactiva no consulta, aunque le queden preguntas.
       Es la misma comprobación que hace el resto del servicio: lo
       que el frontend sabe de su sesión decide qué botones muestra,
       nunca qué se permite. */
    DECLARE @activo BIT = 0;

    SELECT @activo = CAST(activo AS BIT)
    FROM dbo.Usuarios
    WHERE codigoUsuario = @codigoUsuario;

    SELECT
        @usadas       AS usadas,
        @limiteDiario AS limite,
        CASE WHEN @limiteDiario - @usadas < 0
             THEN 0
             ELSE @limiteDiario - @usadas END AS restantes,
        CASE WHEN ISNULL(@activo, 0) = 1 AND @usadas < @limiteDiario
             THEN CAST(1 AS BIT)
             ELSE CAST(0 AS BIT) END AS permitido;
END
GO

/* ============================================================
   3. Registro de una consulta

   La escribe el backend con la conexión normal, no el asistente.
   Así el login del asistente queda de lectura pura y no hay que
   explicar una excepción de escritura en el anexo de seguridad.
   ============================================================ */

IF OBJECT_ID('dbo.spIARegistrarConsulta') IS NOT NULL
    DROP PROCEDURE dbo.spIARegistrarConsulta;
GO

CREATE PROCEDURE dbo.spIARegistrarConsulta
    @codigoUsuario INT,
    @pregunta      NVARCHAR(1000),
    @respuesta     NVARCHAR(MAX)  = NULL,
    @respondio     BIT            = 0,
    @herramientas  NVARCHAR(300)  = NULL,
    @modelo        NVARCHAR(60)   = NULL,
    @tokensEntrada INT            = NULL,
    @tokensSalida  INT            = NULL,
    @milisegundos  INT            = NULL,
    @error         NVARCHAR(300)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ConsultasIA
        (codigoUsuario, pregunta, respuesta, respondio, herramientas,
         modelo, tokensEntrada, tokensSalida, milisegundos, error)
    VALUES
        (@codigoUsuario, @pregunta, @respuesta, @respondio, @herramientas,
         @modelo, @tokensEntrada, @tokensSalida, @milisegundos, @error);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS codigoConsulta;
END
GO

/* ============================================================
   4. Búsqueda de propuestas

   La herramienta con la que el asistente responde sobre
   contenido concreto, que es lo que el tablero agregado no puede
   hacer.

   Dos modos en un solo procedimiento:

     - Con @codigoPropuesta, devuelve esa sola propuesta con su
       texto completo. Es el detalle que se pide después de una
       búsqueda.
     - Sin él, devuelve el listado que coincide con los filtros,
       con los textos recortados y con tope de filas.

   El recorte no es tacañería: veinte descripciones completas no
   caben en una respuesta razonable y se pagan por token. El
   modelo pide el detalle de la que le interesa.

   El orden es cronológico y no es un ranking. Ordenar por nivel
   de verificación o por estado de cumplimiento sería la
   plataforma opinando sobre cuál propuesta importa más, que es
   justo lo que el proyecto no hace.

   Quedan fuera las candidaturas inactivas y toda columna de
   contacto.
   ============================================================ */

IF OBJECT_ID('dbo.spIABuscarPropuestas') IS NOT NULL
    DROP PROCEDURE dbo.spIABuscarPropuestas;
GO

CREATE PROCEDURE dbo.spIABuscarPropuestas
    @codigoPropuesta INT           = NULL,
    @texto           NVARCHAR(200) = NULL,
    @campanaSlug     NVARCHAR(80)  = NULL,
    @codigoCategoria INT           = NULL,
    @estado          NVARCHAR(40)  = NULL,
    @candidatoSlug   NVARCHAR(120) = NULL,
    @codigoPartido   INT           = NULL,
    @limite          INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    /* Tope duro. El parámetro puede pedir menos, nunca más: lo que
       vuelve de acá entra al contexto del modelo. */
    IF @limite IS NULL OR @limite > 50 SET @limite = 50;
    IF @limite < 1 SET @limite = 1;

    DECLARE @detalle BIT = CASE WHEN @codigoPropuesta IS NULL THEN 0 ELSE 1 END;

    SELECT TOP (@limite)
        v.codigoPropuesta,
        v.propuesta,
        v.campanaSlug,
        v.categoria,
        v.estado,
        v.verificacion,
        v.candidato,
        v.candidatoSlug,
        v.partido,
        v.partidoSiglas,
        v.departamento,
        v.nivelGobierno,
        v.fecha,
        p.ubicacion,
        p.periodoEjecucion,
        p.beneficiarios,
        /* Completo cuando se pide una sola, recortado en el listado.
           Recortado y no en NULL: la búsqueda por texto mira también
           problema y objetivo, así que devolverlos vacíos le daría al
           modelo una fila sin nada que explique por qué salió. Una
           coincidencia que no se puede señalar es peor que no
           encontrarla — el modelo pide el detalle si necesita más. */
        CASE WHEN @detalle = 1 THEN p.descripcion
             ELSE LEFT(p.descripcion, 400) END AS descripcion,
        CASE WHEN @detalle = 1 THEN p.problema
             ELSE LEFT(p.problema, 200) END AS problema,
        CASE WHEN @detalle = 1 THEN p.objetivo
             ELSE LEFT(p.objetivo, 200) END AS objetivo
    FROM dbo.vwAnaliticaPropuestas v
    INNER JOIN dbo.Propuestas p ON p.codigoPropuesta = v.codigoPropuesta
    INNER JOIN dbo.Candidatos  k ON k.codigoCandidato = v.codigoCandidato
    WHERE k.activo = 1
      AND (@codigoPropuesta IS NULL OR v.codigoPropuesta = @codigoPropuesta)
      AND (@campanaSlug     IS NULL OR v.campanaSlug     = @campanaSlug)
      AND (@codigoCategoria IS NULL OR v.codigoCategoria = @codigoCategoria)
      AND (@estado          IS NULL OR v.estado          = @estado)
      AND (@candidatoSlug   IS NULL OR v.candidatoSlug   = @candidatoSlug)
      AND (@codigoPartido   IS NULL OR v.codigoPartido   = @codigoPartido)
      AND (@texto IS NULL
           OR v.propuesta   LIKE N'%' + @texto + N'%'
           OR p.descripcion LIKE N'%' + @texto + N'%'
           OR p.problema    LIKE N'%' + @texto + N'%'
           OR p.objetivo    LIKE N'%' + @texto + N'%')
    ORDER BY v.fecha DESC, v.propuesta;
END
GO

/* ============================================================
   5. Ficha de una candidatura

   Devuelve dos resultados: la ficha pública y el listado de sus
   propuestas. Van juntos porque casi toda pregunta sobre una
   candidatura termina siendo una pregunta sobre lo que propuso,
   y así se resuelve en una sola herramienta en vez de dos.

   Las columnas de narrativa (titular, biografía, información
   profesional, descripción de la candidatura) las escribe la
   propia candidatura. Viajan como dato citado y el nivel de
   verificación viaja con ellas, para que la respuesta pueda
   decir que son declaradas y no comprobadas.

   No devuelve correo, teléfono ni redes sociales. Nada de eso
   hace falta para responder sobre promesas.
   ============================================================ */

IF OBJECT_ID('dbo.spIAFichaCandidato') IS NOT NULL
    DROP PROCEDURE dbo.spIAFichaCandidato;
GO

CREATE PROCEDURE dbo.spIAFichaCandidato
    @candidatoSlug NVARCHAR(120)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.candidatoSlug,
        v.candidato,
        v.campanaSlug,
        v.partido,
        v.partidoSiglas,
        v.cargo,
        v.nivelGobierno,
        v.departamento,
        k.municipio,
        v.verificacion,
        k.titular,
        LEFT(k.biografia, 1200)              AS biografia,
        LEFT(k.informacionProfesional, 1200) AS informacionProfesional,
        LEFT(k.descripcionCandidatura, 1200) AS descripcionCandidatura
    FROM dbo.vwAnaliticaCandidaturas v
    INNER JOIN dbo.Candidatos k ON k.codigoCandidato = v.codigoCandidato
    WHERE v.candidatoSlug = @candidatoSlug
      AND k.activo = 1;

    SELECT
        v.codigoPropuesta,
        v.propuesta,
        v.categoria,
        v.estado,
        v.verificacion,
        v.fecha,
        LEFT(p.descripcion, 300) AS descripcion
    FROM dbo.vwAnaliticaPropuestas v
    INNER JOIN dbo.Propuestas p ON p.codigoPropuesta = v.codigoPropuesta
    WHERE v.candidatoSlug = @candidatoSlug
    ORDER BY v.fecha DESC, v.propuesta;
END
GO

/* ============================================================
   6. Campaña sobre la que se responde

   El tablero necesita el slug y el nombre de la campaña antes de
   pedir ningún indicador, y sin campaña indicada usa la
   destacada.

   Existe como procedimiento y no como un par de SELECT en el
   backend por una razón concreta: el login del asistente no
   tiene permiso de lectura sobre ninguna tabla, ni siquiera
   sobre Campanas. La primera versión resolvía esto con dos
   consultas sueltas y falló en la primera consulta real, con el
   modelo informando correctamente que no podía dar la cifra.

   Vale como recordatorio de la regla del encabezado: cuando el
   asistente necesita un dato nuevo se le agrega un procedimiento
   y su GRANT, nunca una consulta suelta.
   ============================================================ */

IF OBJECT_ID('dbo.spIACampana') IS NOT NULL
    DROP PROCEDURE dbo.spIACampana;
GO

CREATE PROCEDURE dbo.spIACampana
    @campanaSlug NVARCHAR(80) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @campanaSlug IS NULL OR LTRIM(RTRIM(@campanaSlug)) = N''
        SELECT TOP (1) slug, nombre
        FROM dbo.Campanas
        WHERE esActual = 1
        ORDER BY codigoCampana;
    ELSE
        SELECT slug, nombre
        FROM dbo.Campanas
        WHERE slug = @campanaSlug;
END
GO

PRINT '12_asistente.sql aplicado.';
GO
