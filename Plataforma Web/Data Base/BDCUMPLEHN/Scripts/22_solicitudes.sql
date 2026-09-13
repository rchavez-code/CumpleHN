/* ============================================================
   CumpleHN — 22. Planes y solicitudes de espacio
   ------------------------------------------------------------
   Cómo se contrata un espacio. Hasta acá el servicio existía
   pero no se veía desde el sitio: la organización tenía que
   enterarse por otro canal. Este script le da lo que le falta
   al flujo comercial:

     1. Un catálogo de planes, con precio en lempiras, duración
        y tamaño máximo del padrón. Es tabla y no texto en la
        página para que el precio se cambie sin recompilar y
        para que el pago registrado (script 21) diga qué plan
        se vendió.

     2. Las solicitudes: lo que deja la organización en el
        formulario público «Para organizaciones». Llega a la
        bandeja de la plataforma, que la convierte en espacio o
        la descarta, con nota. Nada se borra: una solicitud
        descartada sigue diciendo quién pidió qué y cuándo.

     3. El tope del padrón. Un plan que dijera «hasta 250
        miembros» sin que nada lo hiciera cumplir sería una
        regla de mentira. spAdminCargarPadron lo comprueba contra
        el plan de la suscripción vigente.

   Lo que sigue sin existir a propósito: el pago en línea. El
   formulario pide, la plataforma responde y cobra por fuera, y
   registra el pago recibido. Ni un dato de tarjeta pasa por
   acá.

   Depende del 20 (padrón) y del 21 (suscripciones).

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Planes
   ============================================================ */

IF OBJECT_ID('dbo.Planes') IS NULL
CREATE TABLE dbo.Planes
(
    codigoPlan   INT            NOT NULL IDENTITY(1,1),
    clave        NVARCHAR(30)   NOT NULL,
    nombre       NVARCHAR(60)   NOT NULL,
    /* Lo que se ve en la tarjeta de la página, en una línea. */
    lema         NVARCHAR(120)  NOT NULL,
    descripcion  NVARCHAR(600)  NULL,
    precio       DECIMAL(10,2)  NOT NULL,
    moneda       CHAR(3)        NOT NULL CONSTRAINT DF_Planes_moneda DEFAULT ('HNL'),
    /* Cuántos días de vigencia da cada pago. */
    dias         INT            NOT NULL,
    /* Tope del padrón. NULL es sin tope. */
    maxMiembros  INT            NULL,
    destacado    BIT            NOT NULL CONSTRAINT DF_Planes_destacado DEFAULT (0),
    orden        INT            NOT NULL CONSTRAINT DF_Planes_orden DEFAULT (0),
    activo       BIT            NOT NULL CONSTRAINT DF_Planes_activo DEFAULT (1),
    CONSTRAINT PK_Planes PRIMARY KEY (codigoPlan),
    CONSTRAINT UQ_Planes_clave UNIQUE (clave),
    CONSTRAINT CK_Planes_precio CHECK (precio >= 0),
    CONSTRAINT CK_Planes_dias CHECK (dias > 0)
);
GO

/* Los tres planes. El precio se fija acá y se muestra desde acá:
   la página no lo escribe. Se dejan en MERGE para poder ajustar
   precio o tope sin perder lo ya vendido, que quedó copiado en la
   suscripción. */
MERGE dbo.Planes AS destino
USING (VALUES
    (N'proceso',     N'Proceso',     N'Una elección, de la convocatoria al escrutinio.',
     N'Para una junta directiva, una asamblea o una consulta puntual. El espacio queda en línea 45 días: alcanza para inscribir planillas, publicar propuestas, abrir la participación y cerrar con resultados.',
     1500.00, 45, 250, 0, 10),
    (N'institucion', N'Institución', N'Un proceso completo, con tiempo para la campaña.',
     N'Para colegios profesionales, cooperativas, sindicatos y asociaciones con más miembros y una campaña de semanas. 90 días de vigencia y padrón de hasta mil personas.',
     3500.00, 90, 1000, 1, 20),
    (N'anual',       N'Anual',       N'Todos los procesos del año en el mismo espacio.',
     N'Para organizaciones que eligen más de una vez al año o quieren mantener el seguimiento de compromisos entre elecciones. Doce meses de vigencia, sin límite de procesos y padrón de hasta cinco mil personas.',
     12000.00, 365, 5000, 0, 30)
) AS origen (clave, nombre, lema, descripcion, precio, dias, maxMiembros, destacado, orden)
    ON destino.clave = origen.clave
WHEN NOT MATCHED THEN
    INSERT (clave, nombre, lema, descripcion, precio, dias, maxMiembros, destacado, orden)
    VALUES (origen.clave, origen.nombre, origen.lema, origen.descripcion, origen.precio,
            origen.dias, origen.maxMiembros, origen.destacado, origen.orden);
GO

/* La suscripción dice qué plan se vendió y qué tope trae. Se copian
   y no se leen del plan: si el precio o el tope cambian después, lo
   ya vendido no cambia. */
IF COL_LENGTH('dbo.Suscripciones', 'codigoPlan') IS NULL
    ALTER TABLE dbo.Suscripciones ADD codigoPlan INT NULL
        CONSTRAINT FK_Suscripciones_Planes REFERENCES dbo.Planes (codigoPlan);
GO

IF COL_LENGTH('dbo.Suscripciones', 'maxMiembros') IS NULL
    ALTER TABLE dbo.Suscripciones ADD maxMiembros INT NULL;
GO

/* ============================================================
   2. Solicitudes
   ============================================================ */

IF OBJECT_ID('dbo.SolicitudesEspacio') IS NULL
CREATE TABLE dbo.SolicitudesEspacio
(
    codigoSolicitud    INT            NOT NULL IDENTITY(1,1),
    organizacion       NVARCHAR(200)  NOT NULL,
    nombreContacto     NVARCHAR(160)  NOT NULL,
    correo             NVARCHAR(160)  NOT NULL,
    telefono           NVARCHAR(40)   NULL,
    codigoPlan         INT            NULL,
    /* Qué van a elegir y cuándo, en sus palabras. */
    proceso            NVARCHAR(300)  NOT NULL,
    fechaAproximada    DATE           NULL,
    mensaje            NVARCHAR(1000) NULL,
    estado             NVARCHAR(20)   NOT NULL CONSTRAINT DF_Solicitudes_estado DEFAULT (N'Nueva'),
    fechaRegistro      DATETIME2(0)   NOT NULL CONSTRAINT DF_Solicitudes_fecha DEFAULT (SYSDATETIME()),
    /* Cómo se resolvió. */
    codigoEspacio      INT            NULL,
    codigoUsuarioAtencion INT         NULL,
    fechaAtencion      DATETIME2(0)   NULL,
    notas              NVARCHAR(500)  NULL,
    CONSTRAINT PK_SolicitudesEspacio PRIMARY KEY (codigoSolicitud),
    CONSTRAINT FK_Solicitudes_Planes
        FOREIGN KEY (codigoPlan) REFERENCES dbo.Planes (codigoPlan),
    CONSTRAINT FK_Solicitudes_Espacios
        FOREIGN KEY (codigoEspacio) REFERENCES dbo.Espacios (codigoEspacio),
    CONSTRAINT FK_Solicitudes_UsuarioAtencion
        FOREIGN KEY (codigoUsuarioAtencion) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT CK_Solicitudes_estado CHECK (estado IN (N'Nueva', N'Atendida', N'Descartada'))
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Solicitudes_estado')
CREATE INDEX IX_Solicitudes_estado ON dbo.SolicitudesEspacio (estado, fechaRegistro DESC);
GO

MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Solicitud', N'dbo.SolicitudesEspacio', N'Solicitud de un espacio, dejada desde el sitio público.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auditoria_accion')
    ALTER TABLE dbo.Auditoria DROP CONSTRAINT CK_Auditoria_accion;
GO

ALTER TABLE dbo.Auditoria
    WITH NOCHECK ADD CONSTRAINT CK_Auditoria_accion
        CHECK (accion IN ('Verificacion', 'Retiro', 'Restauracion',
                          'Alta', 'Edicion', 'Baja', 'Cuenta', 'Modulo',
                          'Cierre', 'Reapertura', 'Padron', 'Pago', 'Solicitud'));
GO

/* ============================================================
   3. Procedimientos
   ============================================================ */

/* Los planes, para la página pública y para el desplegable de
   pagos. Sin permiso: es la oferta, y es pública. */
IF OBJECT_ID('dbo.spPlanes') IS NOT NULL
    DROP PROCEDURE dbo.spPlanes;
GO

CREATE PROCEDURE dbo.spPlanes
AS
BEGIN
    SET NOCOUNT ON;

    SELECT codigoPlan, clave, nombre, lema, ISNULL(descripcion, N'') AS descripcion,
           precio, moneda, dias, ISNULL(maxMiembros, 0) AS maxMiembros, destacado, orden
    FROM dbo.Planes
    WHERE activo = 1
    ORDER BY orden;
END
GO

/* Alta de una solicitud desde el sitio público. Sin cuenta: es el
   primer contacto. Lo único que se valida es lo que hace falta para
   responder. */
IF OBJECT_ID('dbo.spSolicitudCrear') IS NOT NULL
    DROP PROCEDURE dbo.spSolicitudCrear;
GO

CREATE PROCEDURE dbo.spSolicitudCrear
    @organizacion    NVARCHAR(200),
    @nombreContacto  NVARCHAR(160),
    @correo          NVARCHAR(160),
    @telefono        NVARCHAR(40)   = NULL,
    @codigoPlan      INT            = 0,
    @proceso         NVARCHAR(300),
    @fechaAproximada DATE           = NULL,
    @mensaje         NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @organizacion   = LTRIM(RTRIM(ISNULL(@organizacion, N'')));
    SET @nombreContacto = LTRIM(RTRIM(ISNULL(@nombreContacto, N'')));
    SET @correo         = LOWER(LTRIM(RTRIM(ISNULL(@correo, N''))));
    SET @telefono       = NULLIF(LTRIM(RTRIM(ISNULL(@telefono, N''))), N'');
    SET @proceso        = LTRIM(RTRIM(ISNULL(@proceso, N'')));
    SET @mensaje        = NULLIF(LTRIM(RTRIM(ISNULL(@mensaje, N''))), N'');

    IF LEN(@organizacion) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Indicá el nombre de la organización.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF LEN(@nombreContacto) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Indicá con quién nos comunicamos.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @correo NOT LIKE N'%_@_%._%'
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El correo no tiene un formato válido.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF LEN(@proceso) < 8
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Contanos en una línea qué van a elegir.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @codigoPlan <= 0 OR NOT EXISTS (SELECT 1 FROM dbo.Planes WHERE codigoPlan = @codigoPlan AND activo = 1)
        SET @codigoPlan = NULL;

    /* La misma organización con el mismo correo y una solicitud sin
       atender no abre otra: se le responde la que ya está. */
    IF EXISTS (SELECT 1 FROM dbo.SolicitudesEspacio
                WHERE correo = @correo AND estado = N'Nueva')
    BEGIN
        SELECT CAST(1 AS BIT) AS ok,
               N'Ya tenemos una solicitud tuya pendiente. Te escribimos a ' + @correo + N' en cuanto la revisemos.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    INSERT INTO dbo.SolicitudesEspacio
        (organizacion, nombreContacto, correo, telefono, codigoPlan, proceso, fechaAproximada, mensaje)
    VALUES
        (@organizacion, @nombreContacto, @correo, @telefono, @codigoPlan, @proceso, @fechaAproximada, @mensaje);

    SELECT CAST(1 AS BIT) AS ok,
           N'Recibimos la solicitud. Te escribimos a ' + @correo + N' con los pasos para activar el espacio.' AS mensaje,
           CAST(SCOPE_IDENTITY() AS INT) AS codigo;
END
GO

/* La bandeja de la plataforma. */
IF OBJECT_ID('dbo.spAdminSolicitudes') IS NOT NULL
    DROP PROCEDURE dbo.spAdminSolicitudes;
GO

CREATE PROCEDURE dbo.spAdminSolicitudes
    @codigoUsuario INT,
    @estado        NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0 RETURN;
    IF @estado = N'' SET @estado = NULL;

    SELECT s.codigoSolicitud, s.organizacion, s.nombreContacto, s.correo,
           ISNULL(s.telefono, N'') AS telefono,
           ISNULL(s.codigoPlan, 0) AS codigoPlan, ISNULL(p.nombre, N'') AS nombrePlan,
           s.proceso, s.fechaAproximada, ISNULL(s.mensaje, N'') AS mensaje,
           s.estado, s.fechaRegistro,
           ISNULL(s.codigoEspacio, 0) AS codigoEspacio, ISNULL(e.nombre, N'') AS espacio,
           ISNULL(u.nombre, N'') AS atendidaPor, s.fechaAtencion, ISNULL(s.notas, N'') AS notas
    FROM dbo.SolicitudesEspacio s
    LEFT JOIN dbo.Planes p   ON p.codigoPlan = s.codigoPlan
    LEFT JOIN dbo.Espacios e ON e.codigoEspacio = s.codigoEspacio
    LEFT JOIN dbo.Usuarios u ON u.codigoUsuario = s.codigoUsuarioAtencion
    WHERE (@estado IS NULL OR s.estado = @estado)
    ORDER BY CASE s.estado WHEN N'Nueva' THEN 0 ELSE 1 END, s.fechaRegistro DESC;
END
GO

/* Resolver una solicitud: atendida (con el espacio que se creó a
   partir de ella) o descartada, con nota. Solo la plataforma. */
IF OBJECT_ID('dbo.spAdminAtenderSolicitud') IS NOT NULL
    DROP PROCEDURE dbo.spAdminAtenderSolicitud;
GO

CREATE PROCEDURE dbo.spAdminAtenderSolicitud
    @codigoUsuario   INT,
    @codigoSolicitud INT,
    @estado          NVARCHAR(20),
    @codigoEspacio   INT           = 0,
    @notas           NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Solo la administración de la plataforma atiende solicitudes.' AS mensaje;
        RETURN;
    END

    SET @notas = NULLIF(LTRIM(RTRIM(ISNULL(@notas, N''))), N'');

    IF @estado NOT IN (N'Atendida', N'Descartada')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El estado no es válido.' AS mensaje;
        RETURN;
    END

    DECLARE @organizacion NVARCHAR(200) =
        (SELECT organizacion FROM dbo.SolicitudesEspacio WHERE codigoSolicitud = @codigoSolicitud);

    IF @organizacion IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró la solicitud.' AS mensaje;
        RETURN;
    END

    IF @estado = N'Descartada' AND @notas IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Para descartar hay que anotar el motivo.' AS mensaje;
        RETURN;
    END

    IF @codigoEspacio <= 0 SET @codigoEspacio = NULL;

    UPDATE dbo.SolicitudesEspacio
       SET estado = @estado,
           codigoEspacio = @codigoEspacio,
           codigoUsuarioAtencion = @codigoUsuario,
           fechaAtencion = SYSDATETIME(),
           notas = @notas
     WHERE codigoSolicitud = @codigoSolicitud;

    DECLARE @plataforma INT = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1);
    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Solicitud');

    INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@plataforma, @codigoUsuario, N'Solicitud', @tipo, @codigoSolicitud,
            N'Solicitud de ' + @organizacion + N': ' + LOWER(@estado), @notas);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @estado = N'Atendida' THEN N'La solicitud quedó atendida.'
                ELSE N'La solicitud quedó descartada.' END AS mensaje;
END
GO

/* ============================================================
   4. El pago registra el plan y su tope

   spAdminRegistrarPago (del 21) gana @codigoPlan y @maxMiembros.
   Con plan, el tope se copia del plan salvo que se indique otro.
   Sin plan, es lo que se escriba (cero es sin tope).
   ============================================================ */

IF OBJECT_ID('dbo.spAdminRegistrarPago') IS NOT NULL
    DROP PROCEDURE dbo.spAdminRegistrarPago;
GO

CREATE PROCEDURE dbo.spAdminRegistrarPago
    @codigoUsuario  INT,
    @codigoEspacio  INT,
    @nombrePlan     NVARCHAR(60),
    @vigenteDesde   DATE,
    @vigenteHasta   DATE,
    @monto          DECIMAL(10,2),
    @moneda         CHAR(3)       = 'HNL',
    @referenciaPago NVARCHAR(120),
    @notas          NVARCHAR(500) = NULL,
    @codigoPlan     INT           = 0,
    @maxMiembros    INT           = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Solo la administración de la plataforma registra pagos.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @nombrePlan     = LTRIM(RTRIM(ISNULL(@nombrePlan, N'')));
    SET @referenciaPago = LTRIM(RTRIM(ISNULL(@referenciaPago, N'')));
    SET @notas          = NULLIF(LTRIM(RTRIM(ISNULL(@notas, N''))), N'');
    SET @moneda         = UPPER(ISNULL(NULLIF(LTRIM(RTRIM(@moneda)), ''), 'HNL'));

    IF @codigoPlan <= 0 OR NOT EXISTS (SELECT 1 FROM dbo.Planes WHERE codigoPlan = @codigoPlan)
        SET @codigoPlan = NULL;

    IF @codigoPlan IS NOT NULL AND ISNULL(@maxMiembros, 0) <= 0
        SET @maxMiembros = (SELECT maxMiembros FROM dbo.Planes WHERE codigoPlan = @codigoPlan);

    IF ISNULL(@maxMiembros, 0) <= 0 SET @maxMiembros = NULL;

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
        (codigoEspacio, nombrePlan, vigenteDesde, vigenteHasta, monto, moneda, referenciaPago, notas,
         codigoUsuarioRegistro, codigoPlan, maxMiembros)
    VALUES
        (@codigoEspacio, @nombrePlan, @vigenteDesde, @vigenteHasta, @monto, @moneda, @referenciaPago, @notas,
         @codigoUsuario, @codigoPlan, @maxMiembros);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    DECLARE @plataforma INT = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1);
    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Espacio');

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
   5. El tope del padrón se hace cumplir

   spAdminCargarPadron (del 20) comprueba, antes de insertar, que
   los activos más los nuevos no pasen el tope de la suscripción
   vigente. El tope es el mayor entre las vigentes, por si se
   solapan dos. Sin suscripción vigente no hay tope acá: la
   participación ya está cerrada por la vigencia, y cargar la
   lista antes de pagar es una manera razonable de preparar el
   proceso.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminCargarPadron') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCargarPadron;
GO

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

    /* Cuántos entrarían de verdad: los válidos que no están activos ya. */
    DECLARE @entrarian INT =
        (SELECT COUNT(*) FROM @lista l
          WHERE l.valido = 1
            AND NOT EXISTS (SELECT 1 FROM dbo.EspacioMiembros m
                             WHERE m.codigoEspacio = @codigoEspacio
                               AND m.correoNormalizado = l.normalizado AND m.activo = 1));

    DECLARE @activos INT =
        (SELECT COUNT(*) FROM dbo.EspacioMiembros WHERE codigoEspacio = @codigoEspacio AND activo = 1);

    DECLARE @tope INT =
        (SELECT MAX(maxMiembros) FROM dbo.Suscripciones
          WHERE codigoEspacio = @codigoEspacio
            AND CAST(SYSDATETIME() AS DATE) BETWEEN vigenteDesde AND vigenteHasta);

    IF @tope IS NOT NULL AND @activos + @entrarian > @tope
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El plan vigente admite hasta ' + CAST(@tope AS NVARCHAR(10)) + N' miembros en el padrón y ya hay '
               + CAST(@activos AS NVARCHAR(10)) + N'. Con esta lista serían '
               + CAST(@activos + @entrarian AS NVARCHAR(10)) + N'. Para ampliarlo hay que pasar a un plan mayor.' AS mensaje,
               ISNULL(@rechazados, N'') AS rechazados;
        RETURN;
    END

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
        DECLARE @tipoEsp INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Espacio');

        INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle)
        VALUES (@codigoEspacio, @codigoUsuario, N'Padron', @tipoEsp, @codigoEspacio,
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

/* ============================================================
   6. Lo que el asistente no puede ver
   ============================================================ */

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'cumplehn_ia')
BEGIN
    DENY SELECT ON dbo.SolicitudesEspacio TO cumplehn_ia;
END
GO

PRINT 'Script 22 aplicado: planes, solicitudes y tope del padron.';
GO
