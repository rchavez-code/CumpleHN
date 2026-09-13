/* ============================================================
   CumpleHN — 21. Suscripciones
   ------------------------------------------------------------
   La vigencia de un espacio: desde cuándo y hasta cuándo un
   cliente tiene el uso de la plataforma, y qué pagó por él. Es
   lo que hace sostenible el proyecto: el sitio público sigue
   siendo gratuito, y lo que se vende es el espacio.

   Decisiones que sostienen el módulo:

     1. No hay pasarela de pago, y no hay botón que la simule. La
        administración de la plataforma registra el pago que
        recibió (transferencia, depósito) con su referencia, y el
        espacio queda vigente por el período que cubre. Ni un
        dato de tarjeta pasa por la plataforma. La pasarela va
        como trabajo futuro con su costo por transacción, como el
        SMS en el 17.

     2. El estado se deriva, no se guarda. vwEspacios responde
        Plataforma, Vigente, Vencido, Sin suscripción o Retirado
        a partir de las fechas y de activo. Una columna «vigente»
        sobre un espacio cuya fecha ya pasó sería la misma mentira
        esperando a ocurrir que se evitó en vwEncuestas.

     3. Renovar es insertar otra fila. El historial de pagos queda
        entero y cada fila dice qué período cubrió.

     4. Vencido no es retirado. El espacio vencido se sigue
        leyendo (sus fichas están en línea), pero no se participa
        en él ni su cuenta lo administra hasta que se registre
        otro pago. Retirado, además, sale de toda consulta. Las
        dos cosas en un solo lugar:

          · fnEspacioVigente responde si el espacio admite
            participación y administración ahora.
          · fnEsAdministradorDe (del 09) se vuelve a definir acá
            para exigirla a la cuenta del cliente. La de la
            plataforma administra igual un espacio vencido: es
            quien registra el pago que lo reactiva.

        El Web Service consulta las mismas dos funciones, así que
        la puerta es una sola.

   Depende del 19 (Espacios) y redefine fnEsAdministradorDe y
   vwEspacios, por lo que en una base nueva se ejecuta después
   del 09 y del 19, en su orden.

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

IF OBJECT_ID('dbo.Suscripciones') IS NULL
CREATE TABLE dbo.Suscripciones
(
    codigoSuscripcion     INT            NOT NULL IDENTITY(1,1),
    codigoEspacio         INT            NOT NULL,
    /* Cómo se vendió: «Proceso electoral», «Mensual», «Anual».
       Texto y no catálogo: es lo que dice el recibo. */
    nombrePlan            NVARCHAR(60)   NOT NULL,
    vigenteDesde          DATE           NOT NULL,
    vigenteHasta          DATE           NOT NULL,
    monto                 DECIMAL(10,2)  NOT NULL,
    moneda                CHAR(3)        NOT NULL CONSTRAINT DF_Suscripciones_moneda DEFAULT ('HNL'),
    /* Número de transferencia, depósito o recibo. Obligatoria: es
       lo que permite cotejar el pago con el banco. */
    referenciaPago        NVARCHAR(120)  NOT NULL,
    notas                 NVARCHAR(500)  NULL,
    codigoUsuarioRegistro INT            NOT NULL,
    fechaRegistro         DATETIME2(0)   NOT NULL CONSTRAINT DF_Suscripciones_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Suscripciones PRIMARY KEY (codigoSuscripcion),
    CONSTRAINT FK_Suscripciones_Espacios
        FOREIGN KEY (codigoEspacio) REFERENCES dbo.Espacios (codigoEspacio),
    CONSTRAINT FK_Suscripciones_Usuarios
        FOREIGN KEY (codigoUsuarioRegistro) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT CK_Suscripciones_fechas CHECK (vigenteHasta >= vigenteDesde),
    CONSTRAINT CK_Suscripciones_monto  CHECK (monto >= 0)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Suscripciones_espacio')
CREATE INDEX IX_Suscripciones_espacio
    ON dbo.Suscripciones (codigoEspacio, vigenteHasta DESC);
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auditoria_accion')
    ALTER TABLE dbo.Auditoria DROP CONSTRAINT CK_Auditoria_accion;
GO

ALTER TABLE dbo.Auditoria
    WITH NOCHECK ADD CONSTRAINT CK_Auditoria_accion
        CHECK (accion IN ('Verificacion', 'Retiro', 'Restauracion',
                          'Alta', 'Edicion', 'Baja', 'Cuenta', 'Modulo',
                          'Cierre', 'Reapertura', 'Padron', 'Pago'));
GO

/* ============================================================
   2. Vigencia

   Uno cuando el espacio admite participación y administración
   ahora: es la plataforma, o está activo y alguna suscripción
   cubre la fecha de hoy. Con espacio nulo o inexistente, cero.
   ============================================================ */

IF OBJECT_ID('dbo.fnEspacioVigente') IS NOT NULL
    DROP FUNCTION dbo.fnEspacioVigente;
GO

CREATE FUNCTION dbo.fnEspacioVigente (@codigoEspacio INT)
RETURNS BIT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Espacios
                WHERE codigoEspacio = @codigoEspacio AND esPlataforma = 1)
        RETURN 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.Espacios e
        INNER JOIN dbo.Suscripciones s ON s.codigoEspacio = e.codigoEspacio
        WHERE e.codigoEspacio = @codigoEspacio
          AND e.activo = 1
          AND CAST(SYSDATETIME() AS DATE) BETWEEN s.vigenteDesde AND s.vigenteHasta
    )
        RETURN 1;

    RETURN 0;
END
GO

/* fnEsAdministradorDe, del 09, con la vigencia. La cuenta de la
   plataforma (codigoEspacio en NULL) administra siempre, la del
   cliente solo mientras su espacio esté vigente. Es la misma
   función que consultan el Web Service y todos los procedimientos
   de escritura, así que la regla entra por un solo lugar. */

IF OBJECT_ID('dbo.fnEsAdministradorDe') IS NOT NULL
    DROP FUNCTION dbo.fnEsAdministradorDe;
GO

CREATE FUNCTION dbo.fnEsAdministradorDe (@codigoUsuario INT, @codigoEspacio INT)
RETURNS BIT
AS
BEGIN
    DECLARE @ok BIT = 0;

    IF @codigoEspacio IS NULL RETURN 0;

    IF EXISTS (
        SELECT 1
        FROM dbo.Usuarios u
        INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol
        WHERE u.codigoUsuario = @codigoUsuario
          AND u.activo = 1
          AND r.nombre = N'Administrador'
          AND (u.codigoEspacio IS NULL
               OR (u.codigoEspacio = @codigoEspacio AND dbo.fnEspacioVigente(@codigoEspacio) = 1))
    )
        SET @ok = 1;

    RETURN @ok;
END
GO

/* ============================================================
   3. Vista de espacios, con la vigencia

   Reemplaza la del 19: mismas columnas más el estado completo,
   hasta cuándo está vigente y cuántos pagos lleva.
   ============================================================ */

CREATE OR ALTER VIEW dbo.vwEspacios
AS
SELECT
    e.codigoEspacio,
    e.slug,
    e.nombre,
    e.organizacion,
    ISNULL(e.descripcion, N'')    AS descripcion,
    e.esPlataforma,
    e.padronCerrado,
    e.terminoAgrupacion,
    ISNULL(e.codigoUsuarioPropietario, 0) AS codigoUsuarioPropietario,
    ISNULL(up.nombre, N'')        AS propietario,
    ISNULL(up.login, N'')         AS propietarioLogin,
    ISNULL(up.correo, N'')        AS propietarioCorreo,
    e.activo,
    ISNULL(e.motivoBaja, N'')     AS motivoBaja,
    e.fechaCreacion,
    CASE
        WHEN e.esPlataforma = 1 THEN N'Plataforma'
        WHEN e.activo = 0       THEN N'Retirado'
        WHEN EXISTS (SELECT 1 FROM dbo.Suscripciones s
                      WHERE s.codigoEspacio = e.codigoEspacio
                        AND CAST(SYSDATETIME() AS DATE) BETWEEN s.vigenteDesde AND s.vigenteHasta)
                                THEN N'Vigente'
        WHEN EXISTS (SELECT 1 FROM dbo.Suscripciones s
                      WHERE s.codigoEspacio = e.codigoEspacio)
                                THEN N'Vencido'
        ELSE                         N'Sin suscripción'
    END                           AS estado,
    /* Hasta cuándo está (o estuvo) vigente. NULL sin suscripción. */
    (SELECT MAX(s.vigenteHasta) FROM dbo.Suscripciones s
      WHERE s.codigoEspacio = e.codigoEspacio)                        AS vigenteHasta,
    (SELECT COUNT(*) FROM dbo.Suscripciones s
      WHERE s.codigoEspacio = e.codigoEspacio)                        AS pagos,
    (SELECT COUNT(*) FROM dbo.Campanas c
      WHERE c.codigoEspacio = e.codigoEspacio)                        AS campanas,
    (SELECT COUNT(*) FROM dbo.Candidatos k
      INNER JOIN dbo.Campanas c ON c.codigoCampana = k.codigoCampana
      WHERE c.codigoEspacio = e.codigoEspacio AND k.activo = 1)       AS candidaturas,
    (SELECT COUNT(*) FROM dbo.Usuarios u
      INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol
      WHERE r.nombre = N'Administrador' AND u.activo = 1
        AND ((e.esPlataforma = 1 AND u.codigoEspacio IS NULL)
             OR u.codigoEspacio = e.codigoEspacio))                   AS administradores
FROM dbo.Espacios e
LEFT JOIN dbo.Usuarios up ON up.codigoUsuario = e.codigoUsuarioPropietario;
GO

/* ============================================================
   4. Procedimientos

   Registrar un pago es solo de la plataforma (fnEsAdministrador):
   es quien recibió el dinero. Consultar el historial lo puede
   hacer también la cuenta del cliente, para ver qué pagó y hasta
   cuándo, aunque su espacio esté vencido — para eso la lista no
   pasa por fnEsAdministradorDe sino por la pertenencia directa.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminSuscripciones') IS NOT NULL
    DROP PROCEDURE dbo.spAdminSuscripciones;
GO

CREATE PROCEDURE dbo.spAdminSuscripciones
    @codigoUsuario INT,
    @codigoEspacio INT
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
       AND NOT EXISTS (SELECT 1 FROM dbo.Usuarios
                        WHERE codigoUsuario = @codigoUsuario AND activo = 1
                          AND codigoEspacio = @codigoEspacio)
        RETURN;

    SELECT s.codigoSuscripcion, s.codigoEspacio, s.nombrePlan, s.vigenteDesde, s.vigenteHasta,
           s.monto, s.moneda, s.referenciaPago, ISNULL(s.notas, N'') AS notas,
           u.nombre AS registradoPor, s.fechaRegistro,
           CAST(CASE WHEN CAST(SYSDATETIME() AS DATE) BETWEEN s.vigenteDesde AND s.vigenteHasta
                     THEN 1 ELSE 0 END AS BIT) AS vigente
    FROM dbo.Suscripciones s
    INNER JOIN dbo.Usuarios u ON u.codigoUsuario = s.codigoUsuarioRegistro
    WHERE s.codigoEspacio = @codigoEspacio
    ORDER BY s.vigenteHasta DESC, s.codigoSuscripcion DESC;
END
GO

IF OBJECT_ID('dbo.spAdminRegistrarPago') IS NOT NULL
    DROP PROCEDURE dbo.spAdminRegistrarPago;
GO

CREATE PROCEDURE dbo.spAdminRegistrarPago
    @codigoUsuario  INT,
    @codigoEspacio  INT,
    @nombrePlan           NVARCHAR(60),
    @vigenteDesde   DATE,
    @vigenteHasta   DATE,
    @monto          DECIMAL(10,2),
    @moneda         CHAR(3)       = 'HNL',
    @referenciaPago NVARCHAR(120),
    @notas          NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Solo la administración de la plataforma registra pagos.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @nombrePlan           = LTRIM(RTRIM(ISNULL(@nombrePlan, N'')));
    SET @referenciaPago = LTRIM(RTRIM(ISNULL(@referenciaPago, N'')));
    SET @notas          = NULLIF(LTRIM(RTRIM(ISNULL(@notas, N''))), N'');
    SET @moneda         = UPPER(ISNULL(NULLIF(LTRIM(RTRIM(@moneda)), ''), 'HNL'));

    DECLARE @nombre NVARCHAR(160), @esPlataforma BIT, @activo BIT;
    SELECT @nombre = nombre, @esPlataforma = esPlataforma, @activo = activo
      FROM dbo.Espacios WHERE codigoEspacio = @codigoEspacio;

    IF @nombre IS NULL OR @esPlataforma = 1
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El espacio no existe o es la plataforma.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @activo = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El espacio está retirado. Restauralo antes de registrarle un pago.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF LEN(@nombrePlan) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Indicá el plan que se vendió.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @vigenteDesde IS NULL OR @vigenteHasta IS NULL OR @vigenteHasta < @vigenteDesde
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El período tiene que tener inicio y fin, y el fin no puede ser anterior al inicio.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @monto IS NULL OR @monto < 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El monto no es válido.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF LEN(@referenciaPago) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La referencia del pago es obligatoria: es lo que permite cotejarlo con el banco.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    INSERT INTO dbo.Suscripciones
        (codigoEspacio, nombrePlan, vigenteDesde, vigenteHasta, monto, moneda, referenciaPago, notas, codigoUsuarioRegistro)
    VALUES
        (@codigoEspacio, @nombrePlan, @vigenteDesde, @vigenteHasta, @monto, @moneda, @referenciaPago, @notas, @codigoUsuario);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    DECLARE @plataforma INT = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1);
    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Espacio');

    /* Se anota en el espacio de la plataforma, que es quien cobra, y
       queda el monto y la referencia en el detalle para cotejarlo. */
    INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@plataforma, @codigoUsuario, N'Pago', @tipo, @codigoEspacio,
            N'Pago registrado para ' + @nombre + N': ' + @nombrePlan + N', '
            + CONVERT(NVARCHAR(10), @vigenteDesde, 23) + N' a ' + CONVERT(NVARCHAR(10), @vigenteHasta, 23)
            + N', ' + @moneda + N' ' + CONVERT(NVARCHAR(20), @monto) + N', ref. ' + @referenciaPago,
            @notas);

    SELECT CAST(1 AS BIT) AS ok,
           N'El pago quedó registrado. El espacio está vigente hasta el '
           + CONVERT(NVARCHAR(10), @vigenteHasta, 23) + N'.' AS mensaje,
           @nuevo AS codigo;
END
GO

/* ============================================================
   5. Lo que el asistente no puede ver
   ============================================================ */

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'cumplehn_ia')
BEGIN
    DENY SELECT ON dbo.Suscripciones TO cumplehn_ia;
    DENY SELECT ON dbo.vwEspacios    TO cumplehn_ia;
END
GO

PRINT 'Script 21 aplicado: suscripciones y vigencia de espacios.';
GO
