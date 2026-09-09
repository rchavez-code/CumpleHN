/* ============================================================
   CumpleHN — 16. Registro público de cuentas ciudadanas

   Hasta acá las únicas cuentas que existían se creaban desde la
   administración (script 10) o venían sembradas por el script 06.
   Este script agrega el alta que hace la propia persona desde la
   página pública de registro.

   Es el prerrequisito del módulo de encuestas y del de
   participación: sin registro abierto, las únicas cuentas que
   pueden opinar son las diez de prueba.

   Requiere: 02_tablas, 03_catalogos, 10_catalogos_admin (fnSlug).
   ============================================================ */

USE BDCUMPLEHN;
GO

SET ANSI_NULLS ON;
GO

/* Igual que en los scripts 09 y 10. Un procedimiento guarda para
   siempre el valor que esta opción tenía al crearse, y sqlcmd la
   trae apagada. Con la opción apagada, escribir en una tabla que
   participe de un índice filtrado falla con el error 1934, y el
   fallo aparece solo al ejecutarlo, nunca al compilarlo. */
SET QUOTED_IDENTIFIER ON;
GO


/* ============================================================
   1. Alta de una cuenta ciudadana

   La contraseña NO llega en claro: llega ya cifrada por
   EncriptarSHA256 del backend. Es distinto de
   spAdminCrearCuentaCandidato, que recibe la clave en texto y la
   cifra con HASHBYTES, y la diferencia es deliberada.

   HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), @clave)) convierte
   a la página de códigos de la base — Modern_Spanish_CI_AI, que
   es la 1252 — mientras que EncriptarSHA256 trabaja sobre UTF-8.
   Para una contraseña de solo ASCII los dos coinciden, pero en
   cuanto lleva una tilde o una eñe dan hashes distintos y la
   cuenta queda imposible de usar: se crea bien y el acceso la
   rechaza siempre. Acá la contraseña la elige cualquiera, así
   que se cifra en un solo lugar, el mismo que después la valida.

   Del lado de la base queda comprobado el formato del hash. La
   longitud mínima de la contraseña la exige el Web Service, que
   es lo único que llega a verla.
   ============================================================ */

IF OBJECT_ID('dbo.spRegistrarCiudadano') IS NOT NULL
    DROP PROCEDURE dbo.spRegistrarCiudadano;
GO

CREATE PROCEDURE dbo.spRegistrarCiudadano
    @nombres   NVARCHAR(80),
    @apellidos NVARCHAR(80),
    @correo    NVARCHAR(160),
    @claveHash CHAR(64)
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
        SELECT CAST(0 AS BIT) AS ok,
               N'Ingresá tus nombres y apellidos.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo, N'' AS rol;
        RETURN;
    END

    IF @correo NOT LIKE N'%_@_%._%'
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El correo no tiene un formato válido.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo, N'' AS rol;
        RETURN;
    END

    IF LEN(ISNULL(@claveHash, N'')) <> 64
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La contraseña no llegó en el formato esperado.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo, N'' AS rol;
        RETURN;
    END

    /* Decir que el correo ya está registrado revela que existe una
       cuenta con ese correo, y el acceso a propósito no lo revela.
       Acá no hay alternativa sin un paso de confirmación por
       correo: callarlo dejaría a quien ya tiene cuenta sin saber
       por qué falla. Queda anotado para el anexo OWASP. */
    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correo = @correo)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Ese correo ya está registrado. Probá acceder con él.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo, N'' AS rol;
        RETURN;
    END

    DECLARE @rolCiudadano INT = (SELECT codigoRol FROM dbo.Roles WHERE nombre = N'Ciudadano');

    IF @rolCiudadano IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El catálogo de roles no tiene el rol Ciudadano.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo, N'' AS rol;
        RETURN;
    END

    /* ------------------------------------ Usuario de la cuenta

       El formulario público no pide un nombre de usuario: pedirlo
       obliga a inventarlo y a chocar con los ya ocupados antes de
       poder registrarse, cuando el correo ya identifica la cuenta
       y ValidarLogin acepta los dos. Así que se deriva del nombre,
       con la misma forma que tienen las cuentas sembradas del
       script 06 — inicial del nombre más el primer apellido,
       jlopez — y se le agrega un número si ya está ocupado. */

    DECLARE @primerApellido NVARCHAR(80) =
        LEFT(@apellidos, CHARINDEX(N' ', @apellidos + N' ') - 1);

    DECLARE @base NVARCHAR(60) =
        REPLACE(dbo.fnSlug(LEFT(@nombres, 1) + N' ' + @primerApellido), N'-', N'');

    /* fnSlug translitera las tildes y quita los signos más
       comunes. Lo que quede fuera del alfabeto se descarta acá: un
       login con caracteres raros se escribe mal al teclearlo, que
       es para lo único que sirve. La comparación va en binario
       porque la base es CI_AI, y con esa intercalación el patrón
       no distinguiría un carácter acentuado de su letra. */
    WHILE PATINDEX(N'%[^a-z0-9]%', @base COLLATE Latin1_General_BIN2) > 0
        SET @base = STUFF(@base, PATINDEX(N'%[^a-z0-9]%', @base COLLATE Latin1_General_BIN2), 1, N'');

    IF LEN(@base) < 4
        SET @base = @base + REPLACE(dbo.fnSlug(@nombres), N'-', N'');

    IF LEN(@base) < 4
        SET @base = N'ciudadano';

    /* Se recorta para que quepa el sufijo sin pasarse de los
       sesenta caracteres de la columna. */
    SET @base = LEFT(@base, 54);

    DECLARE @login NVARCHAR(60) = @base;
    DECLARE @n INT = 1;

    WHILE EXISTS (SELECT 1 FROM dbo.Usuarios WHERE login = @login)
    BEGIN
        SET @n = @n + 1;
        SET @login = @base + CAST(@n AS NVARCHAR(10));
    END

    /* ---------------------------------------------------- Alta

       Entre la comprobación de arriba y este INSERT caben dos
       registros simultáneos con el mismo correo. Lo que impide que
       entren los dos son las restricciones únicas de la tabla, no
       el orden de las consultas, así que el choque se atrapa acá y
       se devuelve como mensaje en lugar de como excepción. */

    BEGIN TRY
        INSERT INTO dbo.Usuarios (login, clave, nombre, correo, codigoRol, codigoCandidato, activo)
        VALUES (@login, LOWER(@claveHash), @nombre, @correo, @rolCiudadano, NULL, 1);
    END TRY
    BEGIN CATCH
        SELECT CAST(0 AS BIT) AS ok,
               N'No se pudo crear la cuenta. Intentalo de nuevo.' AS mensaje,
               0 AS codigoUsuario, N'' AS login, N'' AS nombre, N'' AS correo, N'' AS rol;
        RETURN;
    END CATCH

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    /* No se escribe en Auditoria a propósito. Esa bitácora registra
       lo que hace quien administra la plataforma, y es lo que se
       lee para revisar decisiones de verificación y moderación.
       Registrarse no es una acción de administración, y mezclarlo
       dejaría la bitácora llena de filas que nadie revisa. Cuándo
       se creó la cuenta ya lo guarda Usuarios.fechaRegistro. */

    /* La respuesta trae los mismos campos que ValidarLogin para
       que el frontend abra la sesión sin pedir el acceso otra vez:
       crear la cuenta y que lo primero que pase sea un formulario
       de acceso es un paso de más sobre algo que se acaba de
       comprobar. */
    SELECT CAST(1 AS BIT) AS ok,
           N'La cuenta quedó creada.' AS mensaje,
           @nuevo AS codigoUsuario,
           @login AS login,
           @nombre AS nombre,
           @correo AS correo,
           N'Ciudadano' AS rol;
END
GO

PRINT 'Script 16 aplicado: registro publico de cuentas ciudadanas.';
GO
