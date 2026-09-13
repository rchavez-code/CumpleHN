/* ============================================================
   CumpleHN — 20. Padrón de un espacio
   ------------------------------------------------------------
   Quién puede participar en un espacio. En la plataforma
   participa cualquiera con correo confirmado. En el espacio de
   un cliente —la elección de junta directiva de un colegio, por
   ejemplo— solo sus miembros, y la organización es quien sabe
   quiénes son. Sin esto el espacio no sirve para una elección
   interna: una persona ajena podría votar en ella.

   Decisiones que sostienen el módulo:

     1. El padrón es de correos, no de cuentas. La organización
        carga la lista antes de que la gente se registre, y la
        pertenencia se resuelve al participar, cruzando el
        correo normalizado del padrón con el de la cuenta
        (fnCorreoNormalizado, script 17). No se guarda ningún
        enlace a Usuarios: se calcularía dos veces lo mismo y
        podría quedar desactualizado.

     2. Es una capa más de la misma puerta. MotivoSinParticipacion
        (Web Service) ya exige cuenta activa y correo confirmado
        para valorar, comentar, responder encuestas y proponer.
        Acá gana una causa más, «no estás en el padrón», y solo
        cuando Espacios.padronCerrado está en 1. Las páginas no
        deciden nada: llamar al ASMX directamente se topa con la
        misma puerta.

     3. Nada se borra. Quitar a alguien del padrón es activo en 0:
        lo que ya votó sigue contando, por la misma regla de
        «cerrar la participación no reescribe el pasado» de los
        interruptores de módulos.

     4. La lista se carga pegada, un correo por línea, y el
        procedimiento la separa y la normaliza, como las opciones
        de una encuesta en el script 14. Devuelve cuántos
        entraron, cuántos ya estaban y cuáles no eran correos.

   Lo que el padrón no resuelve, y hay que declarar: dice que el
   buzón está en la lista, no que quien lo controla sea la
   persona que la organización cree. Es el mismo límite que la
   confirmación de correo del 17.

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

IF OBJECT_ID('dbo.EspacioMiembros') IS NULL
CREATE TABLE dbo.EspacioMiembros
(
    codigoMiembro      INT           NOT NULL IDENTITY(1,1),
    codigoEspacio      INT           NOT NULL,
    /* Tal como se cargó, para mostrarlo. */
    correo             NVARCHAR(160) NOT NULL,
    /* Con el que se compara. Único por espacio: el mismo buzón no
       entra dos veces aunque se escriba distinto. */
    correoNormalizado  NVARCHAR(160) NOT NULL,
    activo             BIT           NOT NULL CONSTRAINT DF_EspacioMiembros_activo DEFAULT (1),
    fechaAlta          DATETIME2(0)  NOT NULL CONSTRAINT DF_EspacioMiembros_fecha DEFAULT (SYSDATETIME()),
    codigoUsuarioAlta  INT           NOT NULL,
    fechaBaja          DATETIME2(0)  NULL,
    codigoUsuarioBaja  INT           NULL,
    CONSTRAINT PK_EspacioMiembros PRIMARY KEY (codigoMiembro),
    CONSTRAINT UQ_EspacioMiembros_correo UNIQUE (codigoEspacio, correoNormalizado),
    CONSTRAINT FK_EspacioMiembros_Espacios
        FOREIGN KEY (codigoEspacio) REFERENCES dbo.Espacios (codigoEspacio),
    CONSTRAINT FK_EspacioMiembros_UsuarioAlta
        FOREIGN KEY (codigoUsuarioAlta) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT FK_EspacioMiembros_UsuarioBaja
        FOREIGN KEY (codigoUsuarioBaja) REFERENCES dbo.Usuarios (codigoUsuario)
);
GO

/* Lo que consulta la puerta: si este correo está en este padrón. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EspacioMiembros_correo')
CREATE INDEX IX_EspacioMiembros_correo
    ON dbo.EspacioMiembros (correoNormalizado, codigoEspacio) INCLUDE (activo);
GO

/* La bitácora suma la carga y la baja del padrón. */
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auditoria_accion')
    ALTER TABLE dbo.Auditoria DROP CONSTRAINT CK_Auditoria_accion;
GO

ALTER TABLE dbo.Auditoria
    WITH NOCHECK ADD CONSTRAINT CK_Auditoria_accion
        CHECK (accion IN ('Verificacion', 'Retiro', 'Restauracion',
                          'Alta', 'Edicion', 'Baja', 'Cuenta', 'Modulo',
                          'Cierre', 'Reapertura', 'Padron'));
GO

/* ============================================================
   2. La pregunta que hace la puerta

   Uno cuando la cuenta puede participar en el espacio por lo
   que al padrón respecta: el espacio tiene el padrón abierto, o
   el correo de la cuenta está en su padrón y activo. Lo demás
   (cuenta activa, correo confirmado) lo sigue mirando
   MotivoSinParticipacion, que es quien llama a esta función.

   Con espacio nulo o inexistente responde uno: el objeto no
   existe y otra comprobación lo rechaza por su lado, sin dar a
   entender que fue el padrón.
   ============================================================ */

IF OBJECT_ID('dbo.fnEsMiembro') IS NOT NULL
    DROP FUNCTION dbo.fnEsMiembro;
GO

CREATE FUNCTION dbo.fnEsMiembro (@codigoUsuario INT, @codigoEspacio INT)
RETURNS BIT
AS
BEGIN
    DECLARE @cerrado BIT =
        (SELECT padronCerrado FROM dbo.Espacios WHERE codigoEspacio = @codigoEspacio);

    IF @cerrado IS NULL OR @cerrado = 0 RETURN 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.EspacioMiembros m
        INNER JOIN dbo.Usuarios u ON u.correoNormalizado = m.correoNormalizado
        WHERE u.codigoUsuario = @codigoUsuario
          AND m.codigoEspacio = @codigoEspacio
          AND m.activo = 1
    )
        RETURN 1;

    RETURN 0;
END
GO

/* ============================================================
   3. Vista del padrón para la administración

   Cada correo del padrón con lo que la plataforma sabe de él: si
   ya hay una cuenta con ese correo y si la confirmó. Es lo que
   le dice a la organización cuántos de sus miembros ya pueden
   votar, sin exponer nada más de la cuenta que el nombre.
   ============================================================ */

CREATE OR ALTER VIEW dbo.vwEspacioMiembros
AS
SELECT
    m.codigoMiembro,
    m.codigoEspacio,
    m.correo,
    m.correoNormalizado,
    m.activo,
    m.fechaAlta,
    ISNULL(u.nombre, N'')                              AS nombre,
    CAST(CASE WHEN u.codigoUsuario IS NULL THEN 0 ELSE 1 END AS BIT) AS tieneCuenta,
    CAST(CASE WHEN u.codigoUsuario IS NOT NULL AND u.correoConfirmado = 1 AND u.activo = 1
              THEN 1 ELSE 0 END AS BIT)                AS puedeParticipar
FROM dbo.EspacioMiembros m
LEFT JOIN dbo.Usuarios u ON u.correoNormalizado = m.correoNormalizado;
GO

/* ============================================================
   4. Procedimientos

   Mismas reglas de los scripts 09 y 10: fnEsAdministradorDe
   contra el espacio, bitácora en cada escritura, y una fila con
   ok y mensaje de vuelta.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminPadron') IS NOT NULL
    DROP PROCEDURE dbo.spAdminPadron;
GO

CREATE PROCEDURE dbo.spAdminPadron
    @codigoUsuario INT,
    @codigoEspacio INT
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministradorDe(@codigoUsuario, @codigoEspacio) = 0 RETURN;

    SELECT codigoMiembro, codigoEspacio, correo, activo, fechaAlta,
           nombre, tieneCuenta, puedeParticipar
    FROM dbo.vwEspacioMiembros
    WHERE codigoEspacio = @codigoEspacio
    ORDER BY activo DESC, correo;
END
GO

IF OBJECT_ID('dbo.spAdminCargarPadron') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCargarPadron;
GO

/* Carga una lista pegada, un correo por línea. Los que ya estaban
   y fueron dados de baja vuelven a activarse: cargar de nuevo la
   lista es la manera natural de decir «estos son». Devuelve el
   resumen en el mensaje y los correos rechazados en una segunda
   columna, para que la pantalla los muestre y se corrijan. */

CREATE PROCEDURE dbo.spAdminCargarPadron
    @codigoUsuario INT,
    @codigoEspacio INT,
    @correos       NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministradorDe(@codigoUsuario, @codigoEspacio) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no tiene permiso para administrar el padrón de este espacio.' AS mensaje,
               N'' AS rechazados;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Espacios WHERE codigoEspacio = @codigoEspacio AND esPlataforma = 1)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La plataforma no tiene padrón: participa cualquiera con correo confirmado.' AS mensaje,
               N'' AS rechazados;
        RETURN;
    END

    /* Separadas, limpias y sin repetidas dentro de la misma carga. El
       CHAR(13) se quita porque un textarea manda CR LF. */
    DECLARE @lista TABLE (correo NVARCHAR(160), normalizado NVARCHAR(160), valido BIT);

    INSERT INTO @lista (correo, normalizado, valido)
    SELECT correo,
           dbo.fnCorreoNormalizado(correo),
           CASE WHEN correo LIKE N'%_@_%._%' AND correo NOT LIKE N'% %' THEN 1 ELSE 0 END
    FROM (
        SELECT DISTINCT LOWER(LTRIM(RTRIM(REPLACE(REPLACE(s.value, CHAR(13), N''), CHAR(9), N'')))) AS correo
        FROM STRING_SPLIT(ISNULL(@correos, N''), CHAR(10)) s
    ) x
    WHERE LEN(correo) > 0;

    DECLARE @rechazados NVARCHAR(MAX) =
        (SELECT STRING_AGG(correo, N', ') FROM @lista WHERE valido = 0);

    DECLARE @nuevos INT = 0, @reactivados INT = 0, @yaEstaban INT = 0;

    SELECT @yaEstaban = COUNT(*)
    FROM @lista l
    INNER JOIN dbo.EspacioMiembros m
        ON m.codigoEspacio = @codigoEspacio AND m.correoNormalizado = l.normalizado
    WHERE l.valido = 1 AND m.activo = 1;

    UPDATE m
       SET activo = 1, fechaBaja = NULL, codigoUsuarioBaja = NULL
      FROM dbo.EspacioMiembros m
      INNER JOIN @lista l ON l.normalizado = m.correoNormalizado AND l.valido = 1
     WHERE m.codigoEspacio = @codigoEspacio AND m.activo = 0;
    SET @reactivados = @@ROWCOUNT;

    INSERT INTO dbo.EspacioMiembros (codigoEspacio, correo, correoNormalizado, codigoUsuarioAlta)
    SELECT @codigoEspacio, l.correo, l.normalizado, @codigoUsuario
    FROM @lista l
    WHERE l.valido = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.EspacioMiembros m
                       WHERE m.codigoEspacio = @codigoEspacio AND m.correoNormalizado = l.normalizado);
    SET @nuevos = @@ROWCOUNT;

    IF @nuevos + @reactivados > 0
    BEGIN
        DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Espacio');

        INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle)
        VALUES (@codigoEspacio, @codigoUsuario, N'Padron', @tipo, @codigoEspacio,
                N'Padrón: ' + CAST(@nuevos AS NVARCHAR(10)) + N' correos agregados, '
                            + CAST(@reactivados AS NVARCHAR(10)) + N' reactivados');
    END

    SELECT CAST(1 AS BIT) AS ok,
           CAST(@nuevos AS NVARCHAR(10)) + N' correos agregados, '
           + CAST(@reactivados AS NVARCHAR(10)) + N' reactivados y '
           + CAST(@yaEstaban AS NVARCHAR(10)) + N' que ya estaban.'
           + CASE WHEN @rechazados IS NULL THEN N''
                  ELSE N' No se tomaron por no ser correos: ' + @rechazados END AS mensaje,
           ISNULL(@rechazados, N'') AS rechazados;
END
GO

IF OBJECT_ID('dbo.spAdminEstadoMiembro') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEstadoMiembro;
GO

CREATE PROCEDURE dbo.spAdminEstadoMiembro
    @codigoUsuario INT,
    @codigoMiembro INT,
    @activo        BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @codigoEspacio INT, @correo NVARCHAR(160), @actual BIT;
    SELECT @codigoEspacio = codigoEspacio, @correo = correo, @actual = activo
      FROM dbo.EspacioMiembros WHERE codigoMiembro = @codigoMiembro;

    IF dbo.fnEsAdministradorDe(@codigoUsuario, @codigoEspacio) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no tiene permiso para administrar este padrón.' AS mensaje;
        RETURN;
    END

    IF @actual = @activo
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               CASE WHEN @activo = 1 THEN N'Ese correo ya está en el padrón.'
                    ELSE N'Ese correo ya estaba fuera del padrón.' END AS mensaje;
        RETURN;
    END

    UPDATE dbo.EspacioMiembros
       SET activo = @activo,
           fechaBaja = CASE WHEN @activo = 0 THEN SYSDATETIME() ELSE NULL END,
           codigoUsuarioBaja = CASE WHEN @activo = 0 THEN @codigoUsuario ELSE NULL END
     WHERE codigoMiembro = @codigoMiembro;

    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Espacio');

    INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle)
    VALUES (@codigoEspacio, @codigoUsuario, N'Padron', @tipo, @codigoEspacio,
            CASE WHEN @activo = 0 THEN N'Padrón: quitado ' ELSE N'Padrón: restaurado ' END + @correo);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @activo = 0
                THEN N'El correo quedó fuera del padrón. Lo que ya votó se conserva.'
                ELSE N'El correo volvió al padrón.' END AS mensaje;
END
GO

/* ============================================================
   5. Lo que el asistente no puede ver

   El padrón vincula correos con una organización y una elección.
   No es materia del asistente.
   ============================================================ */

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'cumplehn_ia')
BEGIN
    DENY SELECT ON dbo.EspacioMiembros   TO cumplehn_ia;
    DENY SELECT ON dbo.vwEspacioMiembros TO cumplehn_ia;
END
GO

PRINT 'Script 20 aplicado: padron de espacios.';
GO
