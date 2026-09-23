/* ============================================================
   CumpleHN — 24. Panel de la candidatura
   ------------------------------------------------------------
   El panel del candidato validaba sus dos formularios y no
   guardaba nada: el proyecto de campaña y el perfil público.
   Este script agrega los dos procedimientos que escriben, más
   las funciones que deciden qué se puede editar.

   La candidatura nunca se recibe de quien llama. Se deriva de
   la cuenta (Usuarios.codigoCandidato) con fnCandidatoDeUsuario,
   por la misma razón por la que el espacio de un objeto se
   deriva del objeto: recibirla sería dejar que el cliente diga
   sobre qué ficha tiene permiso.

   Reparto con la administración, que ya estaba escrito en el
   10: la plataforma registra la identificación de la candidatura
   (nombre, campaña, cargo, partido, departamento, municipio) y
   la candidatura redacta el resto. Por eso spPanelGuardarPerfil
   no recibe ninguno de esos campos.

   Cuatro reglas que sostienen el seguimiento de cumplimiento:

     1. El candidato solo declara «Declarada» o «En proceso». Los
        estados de cumplimiento los asigna la plataforma.

     2. Una propuesta con un estado de cumplimiento asignado ya no
        se edita. Cambiarla dejaría la evaluación apuntando a una
        promesa distinta de la que se evaluó.

     3. El texto de una propuesta con reacciones no se edita, la
        regla de las iniciativas y de las opciones de encuesta: un
        voto o un comentario sobre algo que después cambió apunta
        a algo que nadie leyó. El estado declarado sí se puede
        seguir actualizando, porque no cambia lo prometido.

     4. Cambiar el texto de algo verificado lo devuelve a
        «Declarado». La verificación respalda un texto concreto, y
        conservarla sobre otro sería prestarle el sello de la
        plataforma a algo que nadie revisó. Vale para la propuesta
        y para la presentación del perfil, no para el contacto.

   Además, nada se escribe en una campaña cerrada ni en un
   espacio sin suscripción vigente.

   No escribe en Auditoria: esa bitácora registra lo que hace
   quien administra, igual que en el registro ciudadano.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. De la cuenta a la candidatura

   Cero cuando la cuenta no existe, está desactivada, no tiene
   el rol Candidato o su candidatura fue retirada.
   ============================================================ */

CREATE OR ALTER FUNCTION dbo.fnCandidatoDeUsuario (@codigoUsuario INT)
RETURNS INT
AS
BEGIN
    DECLARE @codigo INT;

    SELECT @codigo = c.codigoCandidato
      FROM dbo.Usuarios u
      INNER JOIN dbo.Roles r      ON r.codigoRol = u.codigoRol
      INNER JOIN dbo.Candidatos c ON c.codigoCandidato = u.codigoCandidato
     WHERE u.codigoUsuario = @codigoUsuario
       AND u.activo = 1
       AND r.nombre = N'Candidato'
       AND c.activo = 1;

    RETURN ISNULL(@codigo, 0);
END
GO

/* ============================================================
   2. Qué se puede editar

   fnPanelBloqueo responde por qué la candidatura no puede
   escribir, o NULL si puede. Con propuesta en cero responde
   por el alta y por el perfil, con una propuesta suma lo que
   es propio de ella. La leen la pantalla, para mostrar el
   formulario cerrado antes de que alguien lo llene, y los dos
   procedimientos, para rechazar: así no pueden discrepar.

   fnPropuestaConReacciones es aparte porque no cierra el
   formulario, solo el texto.
   ============================================================ */

CREATE OR ALTER FUNCTION dbo.fnPropuestaConReacciones (@codigoPropuesta INT)
RETURNS BIT
AS
BEGIN
    DECLARE @tipo INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Propuesta');

    IF EXISTS (SELECT 1 FROM dbo.Valoraciones
                WHERE codigoTipoObjeto = @tipo AND codigoObjeto = @codigoPropuesta)
       OR EXISTS (SELECT 1 FROM dbo.Comentarios
                   WHERE codigoTipoObjeto = @tipo AND codigoObjeto = @codigoPropuesta)
        RETURN 1;

    RETURN 0;
END
GO

CREATE OR ALTER FUNCTION dbo.fnPanelBloqueo (@codigoCandidato INT, @codigoPropuesta INT)
RETURNS NVARCHAR(300)
AS
BEGIN
    DECLARE @estadoCampana NVARCHAR(20), @codigoEspacio INT;

    SELECT @estadoCampana = ca.estado, @codigoEspacio = ca.codigoEspacio
      FROM dbo.Candidatos c
      INNER JOIN dbo.Campanas ca ON ca.codigoCampana = c.codigoCampana
     WHERE c.codigoCandidato = @codigoCandidato AND c.activo = 1;

    IF @codigoEspacio IS NULL
        RETURN N'La cuenta no está vinculada a una candidatura activa.';

    IF dbo.fnEspacioVigente(@codigoEspacio) = 0
        RETURN N'El espacio de esta campaña no tiene una suscripción vigente: se puede consultar, pero no editar.';

    IF @estadoCampana = N'Cerrada'
        RETURN N'La campaña está cerrada. Lo registrado queda como constancia de lo que se prometió y ya no se edita.';

    IF ISNULL(@codigoPropuesta, 0) > 0
    BEGIN
        DECLARE @duena INT, @estado NVARCHAR(40);

        SELECT @duena = p.codigoCandidato, @estado = e.nombre
          FROM dbo.Propuestas p
          INNER JOIN dbo.EstadosPropuesta e ON e.codigoEstado = p.codigoEstado
         WHERE p.codigoPropuesta = @codigoPropuesta;

        /* «No existe» y «no es tuya» se responden igual. */
        IF @duena IS NULL OR @duena <> @codigoCandidato
            RETURN N'No se encontró el proyecto.';

        IF @estado NOT IN (N'Declarada', N'En proceso')
            RETURN N'La plataforma ya le asignó un estado de cumplimiento a este proyecto ('
                 + @estado + N'). Desde ese momento no se edita: cambiarlo dejaría la evaluación '
                 + N'apuntando a una promesa distinta de la que se evaluó.';
    END

    RETURN NULL;
END
GO

/* ============================================================
   3. Guardar un proyecto de campaña

   Alta con código cero, edición con el código. Devuelve la fila
   de siempre, ok, mensaje y codigo.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spPanelGuardarPropuesta
    @codigoUsuario        INT,
    @codigoPropuesta      INT = 0,
    @nombre               NVARCHAR(200),
    @descripcion          NVARCHAR(MAX),
    @problema             NVARCHAR(MAX),
    @objetivo             NVARCHAR(MAX),
    @beneficiarios        NVARCHAR(400) = NULL,
    @codigoCategoria      INT,
    @ubicacion            NVARCHAR(200) = NULL,
    @periodoEjecucion     NVARCHAR(120) = NULL,
    @estado               NVARCHAR(40),
    @informacionAdicional NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @codigoCandidato INT = dbo.fnCandidatoDeUsuario(@codigoUsuario);

    IF @codigoCandidato = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no está vinculada a una candidatura activa.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @codigoPropuesta = ISNULL(@codigoPropuesta, 0);

    DECLARE @bloqueo NVARCHAR(300) = dbo.fnPanelBloqueo(@codigoCandidato, @codigoPropuesta);
    IF @bloqueo IS NOT NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, @bloqueo AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @nombre        = LTRIM(RTRIM(ISNULL(@nombre, N'')));
    SET @descripcion   = LTRIM(RTRIM(ISNULL(@descripcion, N'')));
    SET @problema      = LTRIM(RTRIM(ISNULL(@problema, N'')));
    SET @objetivo      = LTRIM(RTRIM(ISNULL(@objetivo, N'')));
    SET @beneficiarios = NULLIF(LTRIM(RTRIM(ISNULL(@beneficiarios, N''))), N'');
    SET @ubicacion     = NULLIF(LTRIM(RTRIM(ISNULL(@ubicacion, N''))), N'');
    SET @periodoEjecucion     = NULLIF(LTRIM(RTRIM(ISNULL(@periodoEjecucion, N''))), N'');
    SET @informacionAdicional = NULLIF(LTRIM(RTRIM(ISNULL(@informacionAdicional, N''))), N'');

    /* Los mínimos son los de las iniciativas. Sirven para que un
       relleno como «N/A» no pase por una descripción: un proyecto
       sin contenido no se puede comparar ni verificar después. */
    IF LEN(@nombre) < 8
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Escribí un nombre de al menos ocho caracteres.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF LEN(@descripcion) < 20
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Describí el proyecto con un poco más de detalle: al menos veinte caracteres.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    IF LEN(@problema) < 10 OR LEN(@objetivo) < 10
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Indicá el problema que busca solucionar y el objetivo, con al menos diez caracteres cada uno.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    DECLARE @codigoCampana INT, @codigoEspacio INT;
    SELECT @codigoCampana = c.codigoCampana, @codigoEspacio = ca.codigoEspacio
      FROM dbo.Candidatos c
      INNER JOIN dbo.Campanas ca ON ca.codigoCampana = c.codigoCampana
     WHERE c.codigoCandidato = @codigoCandidato;

    /* Compartida (la taxonomía COFOG) o propia del espacio. */
    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias
                    WHERE codigoCategoria = @codigoCategoria
                      AND (codigoEspacio IS NULL OR codigoEspacio = @codigoEspacio))
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Elegí el área o categoría del proyecto.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Solo los dos estados que le corresponde declarar. */
    IF @estado NOT IN (N'Declarada', N'En proceso')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La candidatura solo puede declarar el proyecto como «Declarada» o «En proceso».' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    DECLARE @codigoEstado INT =
        (SELECT codigoEstado FROM dbo.EstadosPropuesta WHERE nombre = @estado);
    DECLARE @declarado INT =
        (SELECT codigoVerificacion FROM dbo.NivelesVerificacion WHERE nombre = N'Declarado');

    /* -------------------------------------------------- Alta */

    IF @codigoPropuesta = 0
    BEGIN
        INSERT INTO dbo.Propuestas
            (codigoCandidato, codigoCampana, nombre, descripcion, problema, objetivo,
             beneficiarios, codigoCategoria, ubicacion, periodoEjecucion, codigoEstado,
             informacionAdicional, codigoVerificacion)
        VALUES
            (@codigoCandidato, @codigoCampana, @nombre, @descripcion, @problema, @objetivo,
             @beneficiarios, @codigoCategoria, @ubicacion, @periodoEjecucion, @codigoEstado,
             @informacionAdicional, @declarado);

        SELECT CAST(1 AS BIT) AS ok,
               N'El proyecto quedó publicado. Figura como declarado hasta que la plataforma lo contraste con una fuente.' AS mensaje,
               CAST(SCOPE_IDENTITY() AS INT) AS codigo;
        RETURN;
    END

    /* ----------------------------------------------- Edición */

    /* ¿Cambió lo prometido? La comparación es binaria para que
       una tilde corregida cuente como cambio: la intercalación de
       la base no distingue acentos, y el texto publicado sí. */
    DECLARE @cambioTexto BIT = 0;

    IF EXISTS (
        SELECT 1 FROM dbo.Propuestas
         WHERE codigoPropuesta = @codigoPropuesta
           AND NOT (    nombre COLLATE Latin1_General_BIN2 = @nombre
                    AND descripcion COLLATE Latin1_General_BIN2 = @descripcion
                    AND ISNULL(problema, N'') COLLATE Latin1_General_BIN2 = @problema
                    AND ISNULL(objetivo, N'') COLLATE Latin1_General_BIN2 = @objetivo
                    AND ISNULL(beneficiarios, N'') COLLATE Latin1_General_BIN2 = ISNULL(@beneficiarios, N'')
                    AND codigoCategoria = @codigoCategoria
                    AND ISNULL(ubicacion, N'') COLLATE Latin1_General_BIN2 = ISNULL(@ubicacion, N'')
                    AND ISNULL(periodoEjecucion, N'') COLLATE Latin1_General_BIN2 = ISNULL(@periodoEjecucion, N'')
                    AND ISNULL(informacionAdicional, N'') COLLATE Latin1_General_BIN2 = ISNULL(@informacionAdicional, N'')))
        SET @cambioTexto = 1;

    IF @cambioTexto = 1 AND dbo.fnPropuestaConReacciones(@codigoPropuesta) = 1
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Este proyecto ya recibió valoraciones o comentarios, así que su contenido no se edita: '
             + N'las reacciones quedarían apuntando a un texto que nadie leyó. El estado declarado sí se puede actualizar.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    DECLARE @verificacionAnterior INT =
        (SELECT codigoVerificacion FROM dbo.Propuestas WHERE codigoPropuesta = @codigoPropuesta);

    UPDATE dbo.Propuestas
       SET nombre = @nombre,
           descripcion = @descripcion,
           problema = @problema,
           objetivo = @objetivo,
           beneficiarios = @beneficiarios,
           codigoCategoria = @codigoCategoria,
           ubicacion = @ubicacion,
           periodoEjecucion = @periodoEjecucion,
           informacionAdicional = @informacionAdicional,
           codigoEstado = @codigoEstado,
           codigoVerificacion = CASE WHEN @cambioTexto = 1 THEN @declarado ELSE codigoVerificacion END
     WHERE codigoPropuesta = @codigoPropuesta;

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @cambioTexto = 1 AND @verificacionAnterior <> @declarado
                THEN N'El proyecto quedó actualizado. Como cambió su contenido, volvió a figurar como declarado '
                   + N'hasta que la plataforma lo revise de nuevo.'
                ELSE N'El proyecto quedó actualizado.' END AS mensaje,
           @codigoPropuesta AS codigo;
END
GO

/* ============================================================
   4. Guardar el perfil

   Solo lo que redacta la candidatura: presentación, contacto y
   redes. Vacío es «no publicar», así que se guarda como NULL.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spPanelGuardarPerfil
    @codigoUsuario          INT,
    @titular                NVARCHAR(200) = NULL,
    @biografia              NVARCHAR(MAX) = NULL,
    @informacionProfesional NVARCHAR(MAX) = NULL,
    @descripcionCandidatura NVARCHAR(MAX) = NULL,
    @correoPublico          NVARCHAR(160) = NULL,
    @telefono               NVARCHAR(40)  = NULL,
    @sitioWeb               NVARCHAR(200) = NULL,
    @facebook               NVARCHAR(120) = NULL,
    @x                      NVARCHAR(120) = NULL,
    @instagram              NVARCHAR(120) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @codigoCandidato INT = dbo.fnCandidatoDeUsuario(@codigoUsuario);

    IF @codigoCandidato = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no está vinculada a una candidatura activa.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @bloqueo NVARCHAR(300) = dbo.fnPanelBloqueo(@codigoCandidato, 0);
    IF @bloqueo IS NOT NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, @bloqueo AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @titular                = NULLIF(LTRIM(RTRIM(ISNULL(@titular, N''))), N'');
    SET @biografia              = NULLIF(LTRIM(RTRIM(ISNULL(@biografia, N''))), N'');
    SET @informacionProfesional = NULLIF(LTRIM(RTRIM(ISNULL(@informacionProfesional, N''))), N'');
    SET @descripcionCandidatura = NULLIF(LTRIM(RTRIM(ISNULL(@descripcionCandidatura, N''))), N'');
    SET @correoPublico          = NULLIF(LTRIM(RTRIM(ISNULL(@correoPublico, N''))), N'');
    SET @telefono               = NULLIF(LTRIM(RTRIM(ISNULL(@telefono, N''))), N'');
    SET @sitioWeb               = NULLIF(LTRIM(RTRIM(ISNULL(@sitioWeb, N''))), N'');
    SET @facebook               = NULLIF(LTRIM(RTRIM(ISNULL(@facebook, N''))), N'');
    SET @x                      = NULLIF(LTRIM(RTRIM(ISNULL(@x, N''))), N'');
    SET @instagram              = NULLIF(LTRIM(RTRIM(ISNULL(@instagram, N''))), N'');

    IF @correoPublico IS NOT NULL
       AND (@correoPublico NOT LIKE N'_%@_%._%' OR @correoPublico LIKE N'% %')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El correo de contacto no tiene un formato válido.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Solo direcciones web. Cualquier otro esquema (javascript:,
       data:) no tiene nada que hacer en una ficha pública, aunque
       hoy se muestre como texto: el día que alguien lo convierta
       en enlace, la regla ya está en la base. */
    IF @sitioWeb IS NOT NULL
       AND (@sitioWeb NOT LIKE N'http://_%' AND @sitioWeb NOT LIKE N'https://_%'
            OR @sitioWeb LIKE N'% %')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El sitio web tiene que empezar con https:// o http://.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Las redes se guardan como usuario, sin la arroba inicial,
       que es como las muestra la ficha. */
    IF LEFT(@facebook, 1)  = N'@' SET @facebook  = NULLIF(SUBSTRING(@facebook, 2, 120), N'');
    IF LEFT(@x, 1)         = N'@' SET @x         = NULLIF(SUBSTRING(@x, 2, 120), N'');
    IF LEFT(@instagram, 1) = N'@' SET @instagram = NULLIF(SUBSTRING(@instagram, 2, 120), N'');

    /* La verificación respalda la presentación, no el teléfono:
       solo la devuelve a declarada un cambio en lo que se afirma. */
    DECLARE @cambioPresentacion BIT = 0;

    IF EXISTS (
        SELECT 1 FROM dbo.Candidatos
         WHERE codigoCandidato = @codigoCandidato
           AND NOT (    ISNULL(titular, N'') COLLATE Latin1_General_BIN2 = ISNULL(@titular, N'')
                    AND ISNULL(biografia, N'') COLLATE Latin1_General_BIN2 = ISNULL(@biografia, N'')
                    AND ISNULL(informacionProfesional, N'') COLLATE Latin1_General_BIN2 = ISNULL(@informacionProfesional, N'')
                    AND ISNULL(descripcionCandidatura, N'') COLLATE Latin1_General_BIN2 = ISNULL(@descripcionCandidatura, N'')))
        SET @cambioPresentacion = 1;

    DECLARE @declarado INT =
        (SELECT codigoVerificacion FROM dbo.NivelesVerificacion WHERE nombre = N'Declarado');
    DECLARE @verificacionAnterior INT =
        (SELECT codigoVerificacion FROM dbo.Candidatos WHERE codigoCandidato = @codigoCandidato);

    UPDATE dbo.Candidatos
       SET titular = @titular,
           biografia = @biografia,
           informacionProfesional = @informacionProfesional,
           descripcionCandidatura = @descripcionCandidatura,
           correoPublico = @correoPublico,
           telefono = @telefono,
           sitioWeb = @sitioWeb,
           facebook = @facebook,
           x = @x,
           instagram = @instagram,
           codigoVerificacion = CASE WHEN @cambioPresentacion = 1 THEN @declarado ELSE codigoVerificacion END
     WHERE codigoCandidato = @codigoCandidato;

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @cambioPresentacion = 1 AND @verificacionAnterior <> @declarado
                THEN N'El perfil quedó actualizado. Como cambió tu presentación, volvió a figurar como declarado '
                   + N'hasta que la plataforma lo revise de nuevo.'
                ELSE N'El perfil quedó actualizado.' END AS mensaje,
           @codigoCandidato AS codigo;
END
GO

/* ============================================================
   5. Lo que la pantalla necesita saber antes de guardar

   Una fila: si el formulario se puede usar, si el texto de la
   propuesta se puede cambiar, y el motivo cuando no. Las mismas
   funciones que usan los procedimientos, para que la pantalla
   no ofrezca algo que después se rechaza.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spPanelEdicion
    @codigoUsuario   INT,
    @codigoPropuesta INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @codigoCandidato INT = dbo.fnCandidatoDeUsuario(@codigoUsuario);
    DECLARE @bloqueo NVARCHAR(300) =
        CASE WHEN @codigoCandidato = 0
             THEN N'La cuenta no está vinculada a una candidatura activa.'
             ELSE dbo.fnPanelBloqueo(@codigoCandidato, ISNULL(@codigoPropuesta, 0)) END;

    DECLARE @conReacciones BIT =
        CASE WHEN ISNULL(@codigoPropuesta, 0) > 0
             THEN dbo.fnPropuestaConReacciones(@codigoPropuesta) ELSE 0 END;

    SELECT CAST(CASE WHEN @bloqueo IS NULL THEN 1 ELSE 0 END AS BIT) AS editable,
           CAST(CASE WHEN @bloqueo IS NULL AND @conReacciones = 0 THEN 1 ELSE 0 END AS BIT) AS textoEditable,
           CASE WHEN @bloqueo IS NOT NULL THEN @bloqueo
                WHEN @conReacciones = 1
                THEN N'Este proyecto ya recibió valoraciones o comentarios, así que su contenido no se edita: '
                   + N'las reacciones quedarían apuntando a un texto que nadie leyó. El estado declarado sí se puede actualizar.'
                ELSE N'' END AS motivo;
END
GO
