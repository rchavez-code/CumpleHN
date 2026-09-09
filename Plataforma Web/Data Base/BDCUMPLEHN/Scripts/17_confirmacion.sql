/* ============================================================
   CumpleHN — 17. Confirmación de correo del registro ciudadano

   El script 16 abrió el registro público. Este agrega la única
   comprobación que la plataforma puede hacer por su cuenta sobre
   quien se registra: que controle el buzón que declaró.

   Lo que esto sí resuelve:
     - que la cuenta controle el correo que escribió,
     - que un mismo buzón disfrazado (puntos y etiquetas de
       Gmail) no abra varias cuentas,
     - que los dominios desechables conocidos queden fuera.

   Lo que NO resuelve, y hay que declararlo en el anexo OWASP en
   lugar de dejarlo implícito: la unicidad de persona. Un buzón
   real de un dominio que no esté en la lista sigue sirviendo
   para abrir otra cuenta. Después de este script la plataforma
   puede afirmar que cada cuenta controla un buzón distinto, no
   que detrás de cada cuenta hay una persona distinta.

   El SMS se evaluó como alternativa más fuerte —CONATEL exige
   registro biométrico con DNI para conservar una línea, así que
   controlar un número +504 se acerca mucho más a la identidad—
   y se descartó por costo: 0.3296 USD por mensaje a Honduras.

   Requiere: 02_tablas, 03_catalogos, 16_registro.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET ANSI_NULLS ON;
GO

/* Igual que en los scripts 09, 10 y 16. Un procedimiento guarda
   para siempre el valor que esta opción tenía al crearse, y
   sqlcmd la trae apagada. */
SET QUOTED_IDENTIFIER ON;
GO


/* ============================================================
   1. Catálogo de dominios de correo

   Una sola tabla hace los dos trabajos, porque los dos son la
   misma pregunta hecha al dominio.

   Quitar los puntos del nombre de usuario es una regla de
   Gmail, no de todos los proveedores. Aplicarla a ciegas
   fundiría en una sola dirección dos buzones distintos de
   cualquier otro servidor, y el síntoma sería que a alguien le
   dicen que su correo ya está registrado cuando no lo está. Por
   eso la regla la decide el dominio y no el código.

   La lista de desechables está incompleta por definición: nacen
   dominios nuevos todo el tiempo. Vive en una tabla, y no en el
   código, justamente para poder ampliarla sin recompilar ni
   volver a desplegar.
   ============================================================ */

IF OBJECT_ID('dbo.DominiosCorreo') IS NULL
CREATE TABLE dbo.DominiosCorreo
(
    dominio       NVARCHAR(160) NOT NULL,
    canonico      NVARCHAR(160) NULL,   -- a qué dominio equivale, si es un alias
    quitaPuntos   BIT           NOT NULL CONSTRAINT DF_DominiosCorreo_puntos   DEFAULT (0),
    quitaEtiqueta BIT           NOT NULL CONSTRAINT DF_DominiosCorreo_etiqueta DEFAULT (0),
    desechable    BIT           NOT NULL CONSTRAINT DF_DominiosCorreo_desech   DEFAULT (0),
    CONSTRAINT PK_DominiosCorreo PRIMARY KEY (dominio)
);
GO

/* Proveedores cuyo comportamiento está documentado. Gmail ignora
   los puntos y todo lo que siga a un signo más. Los demás de la
   lista aceptan la etiqueta pero sí distinguen los puntos. */

MERGE dbo.DominiosCorreo AS destino
USING (VALUES
    (N'gmail.com',       NULL,           1, 1, 0),
    (N'googlemail.com',  N'gmail.com',   1, 1, 0),
    (N'outlook.com',     NULL,           0, 1, 0),
    (N'outlook.es',      NULL,           0, 1, 0),
    (N'hotmail.com',     NULL,           0, 1, 0),
    (N'hotmail.es',      NULL,           0, 1, 0),
    (N'live.com',        NULL,           0, 1, 0),
    (N'icloud.com',      NULL,           0, 1, 0),
    (N'me.com',          N'icloud.com',  0, 1, 0),
    (N'proton.me',       NULL,           0, 1, 0),
    (N'protonmail.com',  N'proton.me',   0, 1, 0),
    (N'fastmail.com',    NULL,           0, 1, 0)
) AS origen (dominio, canonico, quitaPuntos, quitaEtiqueta, desechable)
    ON destino.dominio = origen.dominio
WHEN NOT MATCHED THEN
    INSERT (dominio, canonico, quitaPuntos, quitaEtiqueta, desechable)
    VALUES (origen.dominio, origen.canonico, origen.quitaPuntos,
            origen.quitaEtiqueta, origen.desechable);
GO

/* Buzones de usar y tirar. Registrarse con uno de estos es
   decir de antemano que el correo no sirve para volver a
   contactar a nadie. */

MERGE dbo.DominiosCorreo AS destino
USING (VALUES
    (N'mailinator.com'), (N'guerrillamail.com'), (N'sharklasers.com'),
    (N'10minutemail.com'), (N'tempmail.com'), (N'temp-mail.org'),
    (N'yopmail.com'), (N'throwawaymail.com'), (N'trashmail.com'),
    (N'getnada.com'), (N'maildrop.cc'), (N'dispostable.com'),
    (N'fakeinbox.com'), (N'mohmal.com'), (N'correotemporal.org'),
    (N'mailnesia.com'), (N'spam4.me'), (N'emailondeck.com'),
    (N'moakt.com'), (N'tempr.email')
) AS origen (dominio)
    ON destino.dominio = origen.dominio
WHEN NOT MATCHED THEN
    INSERT (dominio, canonico, quitaPuntos, quitaEtiqueta, desechable)
    VALUES (origen.dominio, NULL, 0, 0, 1);
GO


/* ============================================================
   2. Normalización

   Devuelve la forma con la que se compara la unicidad. No es la
   dirección a la que se escribe: para eso está la columna
   correo, tal como la tecleó la persona. Mandarle el mensaje a
   la forma normalizada sería escribirle a una dirección que
   nunca escribió.
   ============================================================ */

IF OBJECT_ID('dbo.fnCorreoNormalizado') IS NOT NULL
    DROP FUNCTION dbo.fnCorreoNormalizado;
GO

CREATE FUNCTION dbo.fnCorreoNormalizado (@correo NVARCHAR(160))
RETURNS NVARCHAR(160)
AS
BEGIN
    DECLARE @s NVARCHAR(160) = LOWER(LTRIM(RTRIM(ISNULL(@correo, N''))));

    DECLARE @arroba INT = CHARINDEX(N'@', @s);

    /* Sin arroba no hay nada que normalizar. El formato lo
       rechaza la validación, no esta función. */
    IF @arroba <= 1 RETURN @s;

    DECLARE @local   NVARCHAR(160) = LEFT(@s, @arroba - 1);
    DECLARE @dominio NVARCHAR(160) = SUBSTRING(@s, @arroba + 1, 160);

    DECLARE @canonico NVARCHAR(160);
    DECLARE @puntos   BIT;
    DECLARE @etiqueta BIT;

    SELECT @canonico = ISNULL(d.canonico, d.dominio),
           @puntos   = d.quitaPuntos,
           @etiqueta = d.quitaEtiqueta
    FROM dbo.DominiosCorreo d
    WHERE d.dominio = @dominio;

    /* Dominio que no está en el catálogo: se deja intacto el
       nombre de usuario. Suponer reglas de un servidor que no
       conocemos es la manera de rechazar direcciones legítimas. */
    IF @canonico IS NULL
    BEGIN
        SET @canonico = @dominio;
        SET @puntos   = 0;
        SET @etiqueta = 0;
    END

    IF @etiqueta = 1 AND CHARINDEX(N'+', @local) > 0
        SET @local = LEFT(@local, CHARINDEX(N'+', @local) - 1);

    IF @puntos = 1
        SET @local = REPLACE(@local, N'.', N'');

    /* "+etiqueta@gmail.com" se quedaría sin nombre de usuario.
       Antes que devolver algo vacío se devuelve lo original y
       que lo rechace la validación de formato. */
    IF LEN(@local) = 0 RETURN @s;

    RETURN LEFT(@local + N'@' + @canonico, 160);
END
GO


/* ============================================================
   3. Columnas nuevas de Usuarios

   activo y correoConfirmado son cosas distintas y no se
   mezclan. activo = 0 ya significa «cuenta dada de baja por la
   administración», y ValidarLogin lo exige. Si una cuenta sin
   confirmar naciera inactiva, el acceso la rechazaría con el
   mensaje genérico y la persona no tendría manera de enterarse
   de que solo le falta abrir un enlace.

   La cuenta nace activa y sin confirmar: entra, ve la
   plataforma, y lo único que no puede es participar.
   ============================================================ */

IF COL_LENGTH('dbo.Usuarios', 'correoNormalizado') IS NULL
    ALTER TABLE dbo.Usuarios ADD correoNormalizado NVARCHAR(160) NULL;
GO

IF COL_LENGTH('dbo.Usuarios', 'correoConfirmado') IS NULL
    ALTER TABLE dbo.Usuarios ADD correoConfirmado BIT NOT NULL
        CONSTRAINT DF_Usuarios_correoConfirmado DEFAULT (0);
GO

IF COL_LENGTH('dbo.Usuarios', 'fechaConfirmacion') IS NULL
    ALTER TABLE dbo.Usuarios ADD fechaConfirmacion DATETIME2(0) NULL;
GO

/* Relleno de la forma normalizada. Es idempotente: solo toca
   las filas que todavía no la tienen. */
UPDATE dbo.Usuarios
   SET correoNormalizado = dbo.fnCorreoNormalizado(correo)
 WHERE correoNormalizado IS NULL;
GO

/* Las cuentas que ya existían nacen confirmadas.

   Todas fueron sembradas por los scripts de datos o creadas
   desde administración, que entregó la contraseña en mano. Si
   la migración las dejara sin confirmar, los datos de
   demostración dejarían de poder participar y el tablero se
   quedaría sin nada que contar.

   La condición mira si ConfirmacionesCorreo todavía no existe,
   que es la única manera de distinguir la migración inicial de
   una segunda ejecución del script. Sin esa guarda, volver a
   correrlo confirmaría de golpe todas las cuentas pendientes. */

IF OBJECT_ID('dbo.ConfirmacionesCorreo') IS NULL
BEGIN
    UPDATE dbo.Usuarios
       SET correoConfirmado  = 1,
           fechaConfirmacion = fechaRegistro
     WHERE correoConfirmado = 0;

    PRINT 'Cuentas anteriores marcadas como confirmadas.';
END
GO

ALTER TABLE dbo.Usuarios ALTER COLUMN correoNormalizado NVARCHAR(160) NOT NULL;
GO

/* La restricción única se agrega solo si los datos la admiten.
   Dos cuentas que hoy normalicen igual harían fallar el índice
   con un error que no dice cuáles son, así que se comprueba
   antes y se avisa con nombre y apellido. */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Usuarios_correoNormalizado')
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Usuarios
                GROUP BY correoNormalizado HAVING COUNT(*) > 1)
    BEGIN
        PRINT '*** No se creó UQ_Usuarios_correoNormalizado: hay cuentas que colisionan.';
        PRINT '*** Resolvelas y volvé a correr el script.';

        SELECT correoNormalizado,
               COUNT(*)                      AS cuentas,
               STRING_AGG(login, N', ')      AS logins
        FROM dbo.Usuarios
        GROUP BY correoNormalizado
        HAVING COUNT(*) > 1;
    END
    ELSE
    BEGIN
        ALTER TABLE dbo.Usuarios
            ADD CONSTRAINT UQ_Usuarios_correoNormalizado UNIQUE (correoNormalizado);

        PRINT 'Restriccion UQ_Usuarios_correoNormalizado creada.';
    END
END
GO

/* UQ_Usuarios_correo se conserva. Podría parecer redundante con
   la de arriba y no lo es: aquella impide dos cuentas que se
   reduzcan al mismo buzón, esta impide dos cuentas con la misma
   dirección exacta aunque el catálogo de dominios cambie de
   reglas más adelante. Quitarla no ahorraría nada. */


/* ============================================================
   4. Tokens de confirmación

   El token no se guarda en claro. El backend lo genera, se lo
   entrega a la persona por correo y a la base solo le manda el
   SHA-256 — la misma decisión que se tomó con la contraseña, y
   por la misma razón: quien lea la tabla no puede confirmar
   cuentas ajenas con lo que ve.

   El resultado del envío se guarda aunque falle. Una bitácora
   que solo registra los envíos buenos no sirve para revisar los
   malos, que es lo que ya se decidió con ConsultasIA.
   ============================================================ */

IF OBJECT_ID('dbo.ConfirmacionesCorreo') IS NULL
CREATE TABLE dbo.ConfirmacionesCorreo
(
    codigoConfirmacion INT           NOT NULL IDENTITY(1,1),
    codigoUsuario      INT           NOT NULL,
    tokenHash          CHAR(64)      NOT NULL,   -- SHA-256 en hexadecimal minúscula
    fechaEnvio         DATETIME2(0)  NOT NULL
        CONSTRAINT DF_ConfirmacionesCorreo_fecha DEFAULT (SYSDATETIME()),
    fechaExpira        DATETIME2(0)  NOT NULL,
    fechaUso           DATETIME2(0)  NULL,
    enviado            BIT           NOT NULL
        CONSTRAINT DF_ConfirmacionesCorreo_enviado DEFAULT (0),
    error              NVARCHAR(300) NULL,
    CONSTRAINT PK_ConfirmacionesCorreo PRIMARY KEY (codigoConfirmacion),
    CONSTRAINT UQ_ConfirmacionesCorreo_token UNIQUE (tokenHash),
    CONSTRAINT FK_ConfirmacionesCorreo_Usuarios
        FOREIGN KEY (codigoUsuario) REFERENCES dbo.Usuarios (codigoUsuario)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ConfirmacionesCorreo_usuario')
CREATE INDEX IX_ConfirmacionesCorreo_usuario
    ON dbo.ConfirmacionesCorreo (codigoUsuario, fechaEnvio DESC);
GO


/* ============================================================
   5. Alta de la cuenta ciudadana

   Reemplaza al procedimiento del script 16. Cambia en tres
   cosas: normaliza el correo antes de comprobar la unicidad,
   rechaza los dominios desechables, y deja la cuenta sin
   confirmar con su token ya creado.
   ============================================================ */

IF OBJECT_ID('dbo.spRegistrarCiudadano') IS NOT NULL
    DROP PROCEDURE dbo.spRegistrarCiudadano;
GO

CREATE PROCEDURE dbo.spRegistrarCiudadano
    @nombres       NVARCHAR(80),
    @apellidos     NVARCHAR(80),
    @correo        NVARCHAR(160),
    @claveHash     CHAR(64),
    @tokenHash     CHAR(64),
    @horasVigencia INT = 48
AS
BEGIN
    SET NOCOUNT ON;

    SET @nombres   = LTRIM(RTRIM(ISNULL(@nombres, N'')));
    SET @apellidos = LTRIM(RTRIM(ISNULL(@apellidos, N'')));
    SET @correo    = LOWER(LTRIM(RTRIM(ISNULL(@correo, N''))));

    DECLARE @nombre NVARCHAR(160) = LEFT(@nombres + N' ' + @apellidos, 160);

    /* ------------------------------------------- Validaciones */

    IF LEN(@nombres) < 2 OR LEN(@apellidos) < 2
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ingresá tus nombres y apellidos.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo,
               N'' AS rol, 0 AS codigoConfirmacion;
        RETURN;
    END

    IF @correo NOT LIKE N'%_@_%._%'
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El correo no tiene un formato válido.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo,
               N'' AS rol, 0 AS codigoConfirmacion;
        RETURN;
    END

    IF LEN(ISNULL(@claveHash, N'')) <> 64 OR LEN(ISNULL(@tokenHash, N'')) <> 64
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La solicitud no llegó en el formato esperado.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo,
               N'' AS rol, 0 AS codigoConfirmacion;
        RETURN;
    END

    DECLARE @dominio NVARCHAR(160) =
        SUBSTRING(@correo, CHARINDEX(N'@', @correo) + 1, 160);

    IF EXISTS (SELECT 1 FROM dbo.DominiosCorreo
                WHERE dominio = @dominio AND desechable = 1)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Ese servicio de correo es temporal. Usá una dirección a la que puedas volver.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo,
               N'' AS rol, 0 AS codigoConfirmacion;
        RETURN;
    END

    DECLARE @normalizado NVARCHAR(160) = dbo.fnCorreoNormalizado(@correo);

    /* Decir que el correo ya está registrado revela que existe
       una cuenta con ese correo, y el acceso a propósito no lo
       revela. Acá no hay alternativa: callarlo dejaría a quien
       ya tiene cuenta sin saber por qué falla. Queda anotado
       para el anexo OWASP. */
    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correoNormalizado = @normalizado)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Ese correo ya está registrado. Probá acceder con él.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo,
               N'' AS rol, 0 AS codigoConfirmacion;
        RETURN;
    END

    DECLARE @rolCiudadano INT = (SELECT codigoRol FROM dbo.Roles WHERE nombre = N'Ciudadano');

    IF @rolCiudadano IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El catálogo de roles no tiene el rol Ciudadano.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo,
               N'' AS rol, 0 AS codigoConfirmacion;
        RETURN;
    END

    /* ------------------------------------ Usuario de la cuenta

       El formulario público no pide un nombre de usuario:
       pedirlo obliga a inventarlo y a chocar con los ya
       ocupados antes de poder registrarse, cuando el correo ya
       identifica la cuenta y ValidarLogin acepta los dos. Se
       deriva del nombre, con la misma forma que tienen las
       cuentas sembradas del script 06 — inicial del nombre más
       el primer apellido, jlopez — y se le agrega un número si
       ya está ocupado. */

    DECLARE @primerApellido NVARCHAR(80) =
        LEFT(@apellidos, CHARINDEX(N' ', @apellidos + N' ') - 1);

    DECLARE @base NVARCHAR(60) =
        REPLACE(dbo.fnSlug(LEFT(@nombres, 1) + N' ' + @primerApellido), N'-', N'');

    /* La comparación va en binario porque la base es CI_AI, y
       con esa intercalación el patrón no distinguiría un
       carácter acentuado de su letra. */
    WHILE PATINDEX(N'%[^a-z0-9]%', @base COLLATE Latin1_General_BIN2) > 0
        SET @base = STUFF(@base, PATINDEX(N'%[^a-z0-9]%', @base COLLATE Latin1_General_BIN2), 1, N'');

    IF LEN(@base) < 4
        SET @base = @base + REPLACE(dbo.fnSlug(@nombres), N'-', N'');

    IF LEN(@base) < 4
        SET @base = N'ciudadano';

    SET @base = LEFT(@base, 54);

    DECLARE @login NVARCHAR(60) = @base;
    DECLARE @n INT = 1;

    WHILE EXISTS (SELECT 1 FROM dbo.Usuarios WHERE login = @login)
    BEGIN
        SET @n = @n + 1;
        SET @login = @base + CAST(@n AS NVARCHAR(10));
    END

    /* ---------------------------------------------------- Alta

       La cuenta y su token se crean juntos o no se crea
       ninguno. Una cuenta sin token nace sin manera de
       confirmarse, y un token sin cuenta no apunta a nada.

       El choque de dos registros simultáneos con el mismo
       correo lo impiden las restricciones únicas, no el orden
       de las consultas, así que se atrapa acá y se devuelve
       como mensaje en lugar de como excepción. */

    DECLARE @nuevo INT;
    DECLARE @confirmacion INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Usuarios
            (login, clave, nombre, correo, correoNormalizado,
             codigoRol, codigoCandidato, activo, correoConfirmado)
        VALUES
            (@login, LOWER(@claveHash), @nombre, @correo, @normalizado,
             @rolCiudadano, NULL, 1, 0);

        SET @nuevo = SCOPE_IDENTITY();

        INSERT INTO dbo.ConfirmacionesCorreo
            (codigoUsuario, tokenHash, fechaExpira)
        VALUES
            (@nuevo, LOWER(@tokenHash), DATEADD(HOUR, @horasVigencia, SYSDATETIME()));

        SET @confirmacion = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SELECT CAST(0 AS BIT) AS ok,
               N'No se pudo crear la cuenta. Intentalo de nuevo.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo,
               N'' AS rol, 0 AS codigoConfirmacion;
        RETURN;
    END CATCH

    /* No se escribe en Auditoria a propósito. Esa bitácora
       registra lo que hace quien administra la plataforma.
       Registrarse no es una acción de administración, y
       mezclarlo la llenaría de filas que nadie revisa. */

    SELECT CAST(1 AS BIT) AS ok,
           N'La cuenta quedó creada.' AS mensaje,
           @nuevo         AS codigoUsuario,
           @login         AS login,
           @nombre        AS nombre,
           @correo        AS correo,
           N'Ciudadano'   AS rol,
           @confirmacion  AS codigoConfirmacion;
END
GO


/* ============================================================
   6. Confirmar

   Un token desconocido, uno vencido y uno ya usado devuelven el
   mismo mensaje, por la misma razón por la que el acceso no
   dice si falló el usuario o la contraseña: distinguirlos le
   confirma a quien prueba tokens cuáles existieron alguna vez.
   ============================================================ */

IF OBJECT_ID('dbo.spConfirmarCorreo') IS NOT NULL
    DROP PROCEDURE dbo.spConfirmarCorreo;
GO

CREATE PROCEDURE dbo.spConfirmarCorreo
    @tokenHash CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @rechazo NVARCHAR(200) =
        N'El enlace no es válido o ya venció. Pedí uno nuevo desde tu cuenta.';

    IF LEN(ISNULL(@tokenHash, N'')) <> 64
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, @rechazo AS mensaje;
        RETURN;
    END

    DECLARE @codigoUsuario INT;
    DECLARE @codigoConfirmacion INT;

    SELECT @codigoConfirmacion = c.codigoConfirmacion,
           @codigoUsuario      = c.codigoUsuario
    FROM dbo.ConfirmacionesCorreo c
    WHERE c.tokenHash = LOWER(@tokenHash)
      AND c.fechaUso IS NULL
      AND c.fechaExpira > SYSDATETIME();

    IF @codigoConfirmacion IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, @rechazo AS mensaje;
        RETURN;
    END

    /* Si la cuenta ya estaba confirmada se gasta el token igual
       y se responde que sí. Abrir dos veces el mismo enlace es
       lo más normal del mundo, y contestar que falló algo
       cuando no falló nada solo asusta. */

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.ConfirmacionesCorreo
           SET fechaUso = SYSDATETIME()
         WHERE codigoConfirmacion = @codigoConfirmacion;

        UPDATE dbo.Usuarios
           SET correoConfirmado  = 1,
               fechaConfirmacion = ISNULL(fechaConfirmacion, SYSDATETIME())
         WHERE codigoUsuario = @codigoUsuario;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SELECT CAST(0 AS BIT) AS ok,
               N'No se pudo confirmar la cuenta. Intentalo de nuevo.' AS mensaje;
        RETURN;
    END CATCH

    SELECT CAST(1 AS BIT) AS ok,
           N'Tu correo quedó confirmado. Ya podés participar.' AS mensaje;
END
GO


/* ============================================================
   7. Reenviar

   El tope no es la fricción anti-bot que se descartó, es otra
   cosa: el destinatario de este mensaje lo eligió quien se
   registró, así que un botón de reenviar sin límite es una
   herramienta para llenarle el buzón a un tercero. El tope lo
   exige el procedimiento y no el formulario, porque una
   validación que solo vive en la página se salta llamando al
   servicio.
   ============================================================ */

IF OBJECT_ID('dbo.spSolicitarConfirmacion') IS NOT NULL
    DROP PROCEDURE dbo.spSolicitarConfirmacion;
GO

CREATE PROCEDURE dbo.spSolicitarConfirmacion
    @codigoUsuario  INT,
    @tokenHash      CHAR(64),
    @horasVigencia  INT = 48,
    @segundosEspera INT = 90,
    @topeDiario     INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @correo NVARCHAR(160), @nombre NVARCHAR(160), @confirmado BIT;

    SELECT @correo     = u.correo,
           @nombre     = u.nombre,
           @confirmado = u.correoConfirmado
    FROM dbo.Usuarios u
    WHERE u.codigoUsuario = @codigoUsuario AND u.activo = 1;

    IF @correo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no está disponible.' AS mensaje,
               0 AS codigoConfirmacion, N'' AS correo, N'' AS nombre;
        RETURN;
    END

    IF @confirmado = 1
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Tu correo ya está confirmado.' AS mensaje,
               0 AS codigoConfirmacion, N'' AS correo, N'' AS nombre;
        RETURN;
    END

    IF LEN(ISNULL(@tokenHash, N'')) <> 64
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La solicitud no llegó en el formato esperado.' AS mensaje,
               0 AS codigoConfirmacion, N'' AS correo, N'' AS nombre;
        RETURN;
    END

    DECLARE @ultimo DATETIME2(0) =
        (SELECT MAX(fechaEnvio) FROM dbo.ConfirmacionesCorreo
          WHERE codigoUsuario = @codigoUsuario);

    IF @ultimo IS NOT NULL AND DATEDIFF(SECOND, @ultimo, SYSDATETIME()) < @segundosEspera
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Acabamos de enviarte un enlace. Revisá tu correo antes de pedir otro.' AS mensaje,
               0 AS codigoConfirmacion, N'' AS correo, N'' AS nombre;
        RETURN;
    END

    DECLARE @hoy INT =
        (SELECT COUNT(*) FROM dbo.ConfirmacionesCorreo
          WHERE codigoUsuario = @codigoUsuario
            AND fechaEnvio >= DATEADD(HOUR, -24, SYSDATETIME()));

    IF @hoy >= @topeDiario
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Llegaste al límite de envíos por hoy. Probá de nuevo mañana.' AS mensaje,
               0 AS codigoConfirmacion, N'' AS correo, N'' AS nombre;
        RETURN;
    END

    DECLARE @confirmacion INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        /* Los enlaces anteriores dejan de servir. Que convivan
           varios válidos multiplica sin motivo las ventanas
           abiertas para confirmar la misma cuenta. */
        UPDATE dbo.ConfirmacionesCorreo
           SET fechaExpira = SYSDATETIME()
         WHERE codigoUsuario = @codigoUsuario
           AND fechaUso IS NULL
           AND fechaExpira > SYSDATETIME();

        INSERT INTO dbo.ConfirmacionesCorreo
            (codigoUsuario, tokenHash, fechaExpira)
        VALUES
            (@codigoUsuario, LOWER(@tokenHash), DATEADD(HOUR, @horasVigencia, SYSDATETIME()));

        SET @confirmacion = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        SELECT CAST(0 AS BIT) AS ok,
               N'No se pudo generar el enlace. Intentalo de nuevo.' AS mensaje,
               0 AS codigoConfirmacion, N'' AS correo, N'' AS nombre;
        RETURN;
    END CATCH

    SELECT CAST(1 AS BIT) AS ok,
           N'Te enviamos un enlace nuevo.' AS mensaje,
           @confirmacion AS codigoConfirmacion,
           @correo       AS correo,
           @nombre       AS nombre;
END
GO


/* ============================================================
   8. Resultado del envío

   Lo llama el backend después de intentar mandar el mensaje. Se
   guarda igual cuando falla, con el motivo, porque el caso que
   hay que poder revisar después es justamente ese.
   ============================================================ */

IF OBJECT_ID('dbo.spConfirmacionEnvio') IS NOT NULL
    DROP PROCEDURE dbo.spConfirmacionEnvio;
GO

CREATE PROCEDURE dbo.spConfirmacionEnvio
    @codigoConfirmacion INT,
    @enviado            BIT,
    @error              NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ConfirmacionesCorreo
       SET enviado = @enviado,
           error   = LEFT(@error, 300)
     WHERE codigoConfirmacion = @codigoConfirmacion;
END
GO


/* ============================================================
   9. Las cuentas de candidatura nacen confirmadas

   Las crea la administración, que entrega la contraseña en
   mano. Pedirle a la candidatura que además confirme un correo
   sería un paso sin nada que comprobar: la cuenta ya pasó por
   una persona que la revisó.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminCrearCuentaCandidato') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCrearCuentaCandidato;
GO

CREATE PROCEDURE dbo.spAdminCrearCuentaCandidato
    @codigoUsuario   INT,
    @codigoCandidato INT,
    @login           NVARCHAR(60),
    @correo          NVARCHAR(160),
    @clave           NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no tiene permiso para crear cuentas de acceso.' AS mensaje;
        RETURN;
    END

    SET @login  = LOWER(LTRIM(RTRIM(ISNULL(@login, N''))));
    SET @correo = LOWER(LTRIM(RTRIM(ISNULL(@correo, N''))));

    DECLARE @nombre NVARCHAR(200) =
        (SELECT nombres + N' ' + apellidos FROM dbo.Candidatos WHERE codigoCandidato = @codigoCandidato);

    IF @nombre IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró la candidatura.' AS mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE codigoCandidato = @codigoCandidato)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Esta candidatura ya tiene una cuenta de acceso.' AS mensaje;
        RETURN;
    END

    IF LEN(@login) < 4
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El usuario debe tener al menos cuatro caracteres.' AS mensaje;
        RETURN;
    END

    IF @correo NOT LIKE N'%_@_%._%'
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El correo no tiene un formato válido.' AS mensaje;
        RETURN;
    END

    IF LEN(@clave) < 8
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La contraseña debe tener al menos ocho caracteres.' AS mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE login = @login)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ese usuario ya está ocupado.' AS mensaje;
        RETURN;
    END

    DECLARE @normalizado NVARCHAR(160) = dbo.fnCorreoNormalizado(@correo);

    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correoNormalizado = @normalizado)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ese correo ya está registrado.' AS mensaje;
        RETURN;
    END

    DECLARE @rolCandidato INT = (SELECT codigoRol FROM dbo.Roles WHERE nombre = N'Candidato');

    /* Sigue cifrando con HASHBYTES sobre VARCHAR, que es la
       deuda anotada: no coincide con EncriptarSHA256 del
       backend en cuanto la contraseña lleva tilde o eñe, y esa
       cuenta se crea bien y no puede entrar nunca. Se arregla
       igual que en el registro ciudadano, mandando el hash
       desde el backend. Queda para su propia tanda. */
    DECLARE @hash CHAR(64) =
        LOWER(CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), @clave)), 2));

    INSERT INTO dbo.Usuarios
        (login, clave, nombre, correo, correoNormalizado, codigoRol,
         codigoCandidato, activo, correoConfirmado, fechaConfirmacion)
    VALUES
        (@login, @hash, @nombre, @correo, @normalizado, @rolCandidato,
         @codigoCandidato, 1, 1, SYSDATETIME());

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Usuario');

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario, N'Cuenta', @codigoTipoObjeto, @nuevo,
            N'Cuenta de acceso creada para ' + @nombre + N' (' + @login + N')', NULL);

    SELECT CAST(1 AS BIT) AS ok,
           N'La cuenta quedó creada. Entregale la contraseña a la candidatura por un medio seguro.' AS mensaje;
END
GO


PRINT 'Script 17 aplicado: confirmacion de correo y normalizacion.';
GO
