/* ============================================================
   CumpleHN — 25. Archivos: foto de perfil y documentos de respaldo
   ------------------------------------------------------------
   La candidatura puede subir su fotografía y adjuntar documentos
   que respalden cada proyecto de campaña.

   Cómo se reparte el trabajo:

     - El archivo vive en disco, en la carpeta App_Data/Archivos
       del backend. IIS nunca sirve App_Data directamente.
     - La base guarda solo el NOMBRE con el que quedó en disco,
       más los datos para mostrarlo (nombre original, tipo,
       tamaño) y a quién pertenece.
     - Para leer un archivo, el backend busca su fila acá
       (spArchivoObtener) y con el nombre arma la ruta. Si la
       fila no existe o está dada de baja, el archivo no se
       entrega aunque siga en disco.

   El nombre en disco lo genera el backend (un GUID con su
   extensión) y nunca es el que trae la persona: un nombre como
   «..\..\Web.config» no debe poder convertirse en una ruta.

   Reglas:

     1. Nada se borra. Quitar un archivo es baja lógica (activo
        = 0), y el archivo se queda en disco.
     2. Una sola foto activa por candidatura. Subir otra da de
        baja la anterior.
     3. La foto es parte de la presentación del perfil: cambiarla
        lo devuelve a «Declarado», como cambiar la biografía
        (regla 4 del script 24).
     4. Un documento de respaldo no cambia lo prometido, así que
        no toca la verificación de la propuesta y se puede
        adjuntar aunque ya tenga reacciones.
     5. Hasta cinco respaldos activos por propuesta.
     6. Se escribe con las mismas puertas del panel: la
        candidatura sale de la cuenta (fnCandidatoDeUsuario) y
        fnPanelBloqueo decide si todavía se puede editar. Las dos
        funciones son del script 24.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Tabla
   ============================================================ */

IF OBJECT_ID('dbo.Archivos') IS NULL
CREATE TABLE dbo.Archivos
(
    codigoArchivo   INT            NOT NULL IDENTITY(1,1),
    nombreArchivo   NVARCHAR(60)   NOT NULL,   -- nombre en disco: GUID + extensión
    nombreOriginal  NVARCHAR(200)  NOT NULL,   -- solo para mostrarlo
    tipoContenido   NVARCHAR(60)   NOT NULL,   -- image/jpeg, image/png, application/pdf
    tamanoBytes     INT            NOT NULL,
    proposito       NVARCHAR(20)   NOT NULL,   -- Foto o Respaldo
    codigoCandidato INT            NOT NULL,
    codigoPropuesta INT            NULL,       -- NULL en la foto
    codigoUsuario   INT            NOT NULL,   -- quién lo subió
    fechaRegistro   DATETIME2(0)   NOT NULL CONSTRAINT DF_Archivos_fecha DEFAULT (SYSDATETIME()),
    activo          BIT            NOT NULL CONSTRAINT DF_Archivos_activo DEFAULT (1),
    fechaBaja       DATETIME2(0)   NULL,
    CONSTRAINT PK_Archivos PRIMARY KEY (codigoArchivo),
    CONSTRAINT UQ_Archivos_nombre UNIQUE (nombreArchivo),
    CONSTRAINT CK_Archivos_proposito CHECK (proposito IN ('Foto', 'Respaldo')),
    /* La foto no cuelga de una propuesta y el respaldo siempre sí. */
    CONSTRAINT CK_Archivos_propuesta CHECK (
        (proposito = 'Foto' AND codigoPropuesta IS NULL)
     OR (proposito = 'Respaldo' AND codigoPropuesta IS NOT NULL)),
    CONSTRAINT FK_Archivos_Candidatos
        FOREIGN KEY (codigoCandidato) REFERENCES dbo.Candidatos (codigoCandidato),
    CONSTRAINT FK_Archivos_Propuestas
        FOREIGN KEY (codigoPropuesta) REFERENCES dbo.Propuestas (codigoPropuesta),
    CONSTRAINT FK_Archivos_Usuarios
        FOREIGN KEY (codigoUsuario) REFERENCES dbo.Usuarios (codigoUsuario)
);
GO

/* Una sola foto activa por candidatura. Lo garantiza la base, no
   solo el procedimiento. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Archivos_unaFoto')
CREATE UNIQUE INDEX UQ_Archivos_unaFoto
    ON dbo.Archivos (codigoCandidato)
    WHERE proposito = 'Foto' AND activo = 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Archivos_propuesta')
CREATE INDEX IX_Archivos_propuesta
    ON dbo.Archivos (codigoPropuesta, activo);
GO

/* ============================================================
   2. La foto de una candidatura

   El código de su foto activa, o 0 si no tiene. La usan las
   consultas del Web Service que dibujan la ficha y el avatar.
   ============================================================ */

CREATE OR ALTER FUNCTION dbo.fnFotoCandidato (@codigoCandidato INT)
RETURNS INT
AS
BEGIN
    RETURN ISNULL((SELECT TOP 1 codigoArchivo
                     FROM dbo.Archivos
                    WHERE codigoCandidato = @codigoCandidato
                      AND proposito = 'Foto'
                      AND activo = 1), 0);
END
GO

/* ============================================================
   3. Registrar un archivo recién subido

   Lo llama el manejador SubirArchivo.ashx DESPUÉS de escribir el
   archivo en disco. Si este procedimiento rechaza, el manejador
   borra el archivo: así no queda en disco nada sin su fila.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spArchivoRegistrar
    @codigoUsuario   INT,
    @proposito       NVARCHAR(20),
    @codigoPropuesta INT = 0,
    @nombreArchivo   NVARCHAR(60),
    @nombreOriginal  NVARCHAR(200),
    @tipoContenido   NVARCHAR(60),
    @tamanoBytes     INT
AS
BEGIN
    SET NOCOUNT ON;

    /* Paso 1: ¿de qué candidatura es la cuenta? Nunca se recibe
       de quien llama. */
    DECLARE @codigoCandidato INT = dbo.fnCandidatoDeUsuario(@codigoUsuario);

    IF @codigoCandidato = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no está vinculada a una candidatura activa.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @proposito NOT IN (N'Foto', N'Respaldo')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El tipo de archivo no es válido.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Paso 2: ¿todavía se puede editar? Con la propuesta en cero
       pregunta por el perfil, con una propuesta pregunta por ella
       (y de paso comprueba que sea de esta candidatura). */
    SET @codigoPropuesta = CASE WHEN @proposito = N'Foto' THEN 0 ELSE ISNULL(@codigoPropuesta, 0) END;

    IF @proposito = N'Respaldo' AND @codigoPropuesta = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Guardá el proyecto antes de adjuntarle documentos.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @bloqueo NVARCHAR(300) = dbo.fnPanelBloqueo(@codigoCandidato, @codigoPropuesta);
    IF @bloqueo IS NOT NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, @bloqueo AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Paso 3: el tope de respaldos por propuesta. */
    IF @proposito = N'Respaldo'
       AND (SELECT COUNT(*) FROM dbo.Archivos
             WHERE codigoPropuesta = @codigoPropuesta AND activo = 1) >= 5
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Un proyecto admite hasta cinco documentos de respaldo. Quitá uno para adjuntar otro.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    /* Paso 4: guardar la fila. La foto anterior se da de baja en la
       misma transacción, para que nunca haya dos activas ni
       ninguna por un error a mitad de camino. */
    BEGIN TRANSACTION;

    IF @proposito = N'Foto'
    BEGIN
        UPDATE dbo.Archivos
           SET activo = 0, fechaBaja = SYSDATETIME()
         WHERE codigoCandidato = @codigoCandidato
           AND proposito = N'Foto'
           AND activo = 1;

        /* La foto es parte de la presentación: vuelve a declarado. */
        UPDATE dbo.Candidatos
           SET codigoVerificacion = (SELECT codigoVerificacion FROM dbo.NivelesVerificacion
                                      WHERE nombre = N'Declarado')
         WHERE codigoCandidato = @codigoCandidato;
    END

    INSERT INTO dbo.Archivos
        (nombreArchivo, nombreOriginal, tipoContenido, tamanoBytes, proposito,
         codigoCandidato, codigoPropuesta, codigoUsuario)
    VALUES
        (@nombreArchivo, LEFT(ISNULL(@nombreOriginal, N'archivo'), 200), @tipoContenido, @tamanoBytes,
         @proposito, @codigoCandidato, NULLIF(@codigoPropuesta, 0), @codigoUsuario);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    COMMIT TRANSACTION;

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @proposito = N'Foto'
                THEN N'La fotografía quedó publicada. El perfil figura como declarado hasta que la plataforma lo revise.'
                ELSE N'El documento quedó adjunto al proyecto.' END AS mensaje,
           @nuevo AS codigo;
END
GO

/* ============================================================
   4. Quitar un archivo

   Baja lógica. Solo quien es dueño de la candidatura, y solo
   mientras el perfil o la propuesta se puedan editar.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spArchivoQuitar
    @codigoUsuario INT,
    @codigoArchivo INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @codigoCandidato INT = dbo.fnCandidatoDeUsuario(@codigoUsuario);
    DECLARE @duena INT, @codigoPropuesta INT;

    SELECT @duena = codigoCandidato, @codigoPropuesta = ISNULL(codigoPropuesta, 0)
      FROM dbo.Archivos
     WHERE codigoArchivo = @codigoArchivo AND activo = 1;

    /* «No existe» y «no es tuyo» se responden igual. */
    IF @codigoCandidato = 0 OR @duena IS NULL OR @duena <> @codigoCandidato
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró el archivo.' AS mensaje;
        RETURN;
    END

    DECLARE @bloqueo NVARCHAR(300) = dbo.fnPanelBloqueo(@codigoCandidato, @codigoPropuesta);
    IF @bloqueo IS NOT NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, @bloqueo AS mensaje;
        RETURN;
    END

    UPDATE dbo.Archivos
       SET activo = 0, fechaBaja = SYSDATETIME()
     WHERE codigoArchivo = @codigoArchivo;

    SELECT CAST(1 AS BIT) AS ok, N'El archivo se quitó.' AS mensaje;
END
GO

/* ============================================================
   5. Obtener un archivo para entregarlo

   Lo llama VerArchivo.ashx. Devuelve el nombre en disco solo si
   el archivo está activo y su candidatura también: lo mismo que
   el público ve en la ficha. Sin fila, no hay archivo.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spArchivoObtener
    @codigoArchivo INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.nombreArchivo, a.nombreOriginal, a.tipoContenido
      FROM dbo.Archivos a
      INNER JOIN dbo.Candidatos c ON c.codigoCandidato = a.codigoCandidato
     WHERE a.codigoArchivo = @codigoArchivo
       AND a.activo = 1
       AND c.activo = 1;
END
GO

/* ============================================================
   6. Los respaldos de una propuesta

   Para el panel (con su «Quitar») y para la ficha pública.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spArchivosPropuesta
    @codigoPropuesta INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT codigoArchivo, nombreOriginal, tipoContenido, tamanoBytes, fechaRegistro
      FROM dbo.Archivos
     WHERE codigoPropuesta = @codigoPropuesta
       AND proposito = N'Respaldo'
       AND activo = 1
     ORDER BY fechaRegistro;
END
GO
