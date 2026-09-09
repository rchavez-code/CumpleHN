/* ============================================================
   CumpleHN — 09. Módulo de administración
   ------------------------------------------------------------
   Verificación de contenido y moderación de publicaciones, las
   dos facultades que ejerce el rol Administrador.

   Dos decisiones sostienen todo lo de acá:

     1. Retirar una publicación es una baja lógica. La columna
        activo de Publicaciones (script 02) marca el retiro y
        conserva el motivo, la fecha y el responsable. Un DELETE
        se llevaría las valoraciones y los comentarios de la
        publicación, y el tablero quedaría contando totales que
        ya no cuadran con las filas existentes.

     2. Ninguna acción de administración queda sin registro. La
        tabla Auditoria guarda quién, qué, sobre qué objeto,
        cuándo y con qué motivo. Una plataforma que le pide
        cuentas a otros tiene que poder responder por lo que
        hace su propio administrador — es lo que sostiene la
        neutralidad declarada del proyecto.

   Los procedimientos que escriben comprueban el rol contra la
   base antes de tocar nada, aunque el Web Service ya lo haya
   comprobado. Es deliberado: el control no debe depender de un
   solo punto, y así queda documentado en la base para el
   Manual Técnico del capítulo IX.

   Convención de los procedimientos de escritura: devuelven una
   fila con ok (BIT) y mensaje (NVARCHAR). El Web Service la lee
   y la transporta tal cual. No lanzan errores para los casos
   previstos, solo para los inesperados.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* Un procedimiento guarda para siempre el valor que tenían estas dos
   opciones cuando se creó. sqlcmd trae QUOTED_IDENTIFIER apagado, a
   diferencia de SSMS, y con esa opción apagada cualquier escritura
   sobre una tabla con índice filtrado falla con el error 1934 —
   Campanas tiene uno, UQ_Campanas_unicaActual. Se fija acá para que
   el script produzca lo mismo se ejecute desde donde se ejecute. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Bitácora de administración
   ============================================================ */

/* El objeto sobre el que se actúa se identifica con el mismo par
   polimórfico que ya usan Valoraciones y Comentarios:
   (codigoTipoObjeto, codigoObjeto), con el tipo tomado del
   catálogo TiposObjeto. Igual que allá, el par no lleva llave
   foránea porque el destino cambia según el tipo.

   La diferencia con Valoraciones es que acá la fila se conserva
   aunque el objeto desaparezca: una bitácora que se borra sola
   no sirve como bitácora. */

IF OBJECT_ID('dbo.Auditoria') IS NULL
CREATE TABLE dbo.Auditoria
(
    codigoAuditoria  INT            NOT NULL IDENTITY(1,1),
    codigoUsuario    INT            NOT NULL,
    accion           NVARCHAR(40)   NOT NULL,
    codigoTipoObjeto INT            NOT NULL,
    codigoObjeto     INT            NOT NULL,
    detalle          NVARCHAR(300)  NULL,   -- qué cambió, en palabras
    motivo           NVARCHAR(300)  NULL,   -- por qué, lo escribe quien administra
    fecha            DATETIME2(0)   NOT NULL CONSTRAINT DF_Auditoria_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Auditoria PRIMARY KEY (codigoAuditoria),
    CONSTRAINT FK_Auditoria_Usuarios
        FOREIGN KEY (codigoUsuario) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT FK_Auditoria_TiposObjeto
        FOREIGN KEY (codigoTipoObjeto) REFERENCES dbo.TiposObjeto (codigoTipoObjeto),
    CONSTRAINT CK_Auditoria_accion
        CHECK (accion IN ('Verificacion', 'Retiro', 'Restauracion'))
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auditoria_fecha')
CREATE INDEX IX_Auditoria_fecha ON dbo.Auditoria (fecha DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auditoria_objeto')
CREATE INDEX IX_Auditoria_objeto ON dbo.Auditoria (codigoTipoObjeto, codigoObjeto);
GO

/* ============================================================
   2. Comprobación del rol

   Función y no procedimiento, para poder usarla dentro de un IF
   en cada procedimiento de escritura sin variables de salida.
   ============================================================ */

IF OBJECT_ID('dbo.fnEsAdministrador') IS NOT NULL
    DROP FUNCTION dbo.fnEsAdministrador;
GO

CREATE FUNCTION dbo.fnEsAdministrador (@codigoUsuario INT)
RETURNS BIT
AS
BEGIN
    DECLARE @ok BIT = 0;

    IF EXISTS (
        SELECT 1
        FROM dbo.Usuarios u
        INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol
        WHERE u.codigoUsuario = @codigoUsuario
          AND u.activo = 1
          AND r.nombre = N'Administrador'
    )
        SET @ok = 1;

    RETURN @ok;
END
GO

/* ============================================================
   3. Bandeja de verificación

   Una fila por objeto verificable, de los tres tipos que llevan
   nivel de verificación: candidaturas, propuestas y
   publicaciones. La bandeja los une para que la revisión sea una
   sola cola de trabajo y no tres pantallas separadas.

   Por qué una UNION y no tres procedimientos: quien revisa
   necesita ver primero lo más antiguo sin verificar, sin
   importar de qué tipo sea.

   Parámetros opcionales con el mismo patrón del script 08.
   @soloPendientes en 1 deja fuera lo ya verificado.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminBandejaVerificacion') IS NOT NULL
    DROP PROCEDURE dbo.spAdminBandejaVerificacion;
GO

CREATE PROCEDURE dbo.spAdminBandejaVerificacion
    @tipoObjeto     NVARCHAR(40) = NULL,
    @campanaSlug    NVARCHAR(80) = NULL,
    @soloPendientes BIT          = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @tipoObjeto = N'' SET @tipoObjeto = NULL;
    IF @campanaSlug = N'' SET @campanaSlug = NULL;

    ;WITH bandeja AS
    (
        /* ---------------------------------------- Candidaturas */
        SELECT
            N'Candidato'                    AS tipoObjeto,
            c.codigoCandidato               AS codigoObjeto,
            c.nombres + N' ' + c.apellidos  AS titulo,
            ISNULL(c.titular, N'')          AS resumen,
            c.slug                          AS slug,
            c.nombres + N' ' + c.apellidos  AS candidato,
            c.slug                          AS candidatoSlug,
            ca.slug                         AS campanaSlug,
            nv.codigoVerificacion,
            nv.nombre                       AS verificacion,
            nv.orden                        AS verificacionOrden,
            c.fechaRegistro                 AS fecha
        FROM dbo.Candidatos c
        INNER JOIN dbo.Campanas ca            ON ca.codigoCampana = c.codigoCampana
        INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = c.codigoVerificacion
        WHERE c.activo = 1

        UNION ALL

        /* ------------------------------------------- Propuestas */
        SELECT
            N'Propuesta',
            p.codigoPropuesta,
            p.nombre,
            LEFT(ISNULL(p.objetivo, p.descripcion), 220),
            CAST(p.codigoPropuesta AS NVARCHAR(80)),
            c.nombres + N' ' + c.apellidos,
            c.slug,
            ca.slug,
            nv.codigoVerificacion,
            nv.nombre,
            nv.orden,
            p.fechaRegistro
        FROM dbo.Propuestas p
        INNER JOIN dbo.Candidatos c           ON c.codigoCandidato = p.codigoCandidato
        INNER JOIN dbo.Campanas ca            ON ca.codigoCampana = p.codigoCampana
        INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = p.codigoVerificacion

        UNION ALL

        /* ---------------------------------------- Publicaciones */
        SELECT
            N'Publicacion',
            b.codigoPublicacion,
            LEFT(b.texto, 90),
            LEFT(b.texto, 220),
            CAST(b.codigoPublicacion AS NVARCHAR(80)),
            c.nombres + N' ' + c.apellidos,
            c.slug,
            ca.slug,
            nv.codigoVerificacion,
            nv.nombre,
            nv.orden,
            b.fecha
        FROM dbo.Publicaciones b
        INNER JOIN dbo.Candidatos c           ON c.codigoCandidato = b.codigoCandidato
        INNER JOIN dbo.Campanas ca            ON ca.codigoCampana = b.codigoCampana
        INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = b.codigoVerificacion
        WHERE b.activo = 1
    )
    SELECT
        tipoObjeto,
        codigoObjeto,
        titulo,
        resumen,
        slug,
        candidato,
        candidatoSlug,
        campanaSlug,
        codigoVerificacion,
        verificacion,
        verificacionOrden,
        fecha
    FROM bandeja
    WHERE (@tipoObjeto  IS NULL OR tipoObjeto  = @tipoObjeto)
      AND (@campanaSlug IS NULL OR campanaSlug = @campanaSlug)
      AND (@soloPendientes = 0 OR verificacionOrden < 3)
    ORDER BY verificacionOrden, fecha;
END
GO

/* ============================================================
   4. Cambiar el nivel de verificación

   El nivel lo asigna solo la plataforma. La candidatura declara
   su contenido y este queda identificado como declarado hasta
   que alguien con el rol lo contraste contra una fuente.

   El motivo es obligatorio al marcar como Verificado: ahí es
   donde queda anotada la fuente que respalda la decisión. Sin
   eso la bitácora registraría el cambio pero no su fundamento.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminCambiarVerificacion') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCambiarVerificacion;
GO

CREATE PROCEDURE dbo.spAdminCambiarVerificacion
    @codigoUsuario      INT,
    @tipoObjeto         NVARCHAR(40),
    @codigoObjeto       INT,
    @codigoVerificacion INT,
    @motivo             NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @motivo = N'' SET @motivo = NULL;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no tiene permiso para verificar contenido.' AS mensaje;
        RETURN;
    END

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = @tipoObjeto);

    IF @codigoTipoObjeto IS NULL OR @tipoObjeto = N'Partido'
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El tipo de contenido no admite verificación.' AS mensaje;
        RETURN;
    END

    DECLARE @nivel NVARCHAR(40) =
        (SELECT nombre FROM dbo.NivelesVerificacion WHERE codigoVerificacion = @codigoVerificacion);

    IF @nivel IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El nivel de verificación no existe.' AS mensaje;
        RETURN;
    END

    IF @nivel = N'Verificado' AND @motivo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Para marcar como verificado hay que registrar la fuente que lo respalda.' AS mensaje;
        RETURN;
    END

    DECLARE @filas INT = 0;

    IF @tipoObjeto = N'Candidato'
    BEGIN
        UPDATE dbo.Candidatos
           SET codigoVerificacion = @codigoVerificacion
         WHERE codigoCandidato = @codigoObjeto;
        SET @filas = @@ROWCOUNT;
    END
    ELSE IF @tipoObjeto = N'Propuesta'
    BEGIN
        UPDATE dbo.Propuestas
           SET codigoVerificacion = @codigoVerificacion
         WHERE codigoPropuesta = @codigoObjeto;
        SET @filas = @@ROWCOUNT;
    END
    ELSE IF @tipoObjeto = N'Publicacion'
    BEGIN
        UPDATE dbo.Publicaciones
           SET codigoVerificacion = @codigoVerificacion
         WHERE codigoPublicacion = @codigoObjeto;
        SET @filas = @@ROWCOUNT;
    END

    IF @filas = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró el contenido indicado.' AS mensaje;
        RETURN;
    END

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario, N'Verificacion', @codigoTipoObjeto, @codigoObjeto,
            N'Nivel de verificación: ' + @nivel, @motivo);

    SELECT CAST(1 AS BIT) AS ok,
           N'El contenido quedó como ' + LOWER(@nivel) + N'.' AS mensaje;
END
GO

/* ============================================================
   5. Publicaciones para moderación

   Devuelve también las retiradas, que es lo que distingue esta
   consulta de la del feed público. Sin verlas, un retiro por
   error sería irreversible en la práctica.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminPublicaciones') IS NOT NULL
    DROP PROCEDURE dbo.spAdminPublicaciones;
GO

CREATE PROCEDURE dbo.spAdminPublicaciones
    @campanaSlug NVARCHAR(80) = NULL,
    @estado      NVARCHAR(20) = NULL   -- Activas, Retiradas, o NULL para todas
AS
BEGIN
    SET NOCOUNT ON;

    IF @campanaSlug = N'' SET @campanaSlug = NULL;
    IF @estado = N'' SET @estado = NULL;

    SELECT
        b.codigoPublicacion,
        b.texto,
        b.fecha,
        b.activo,
        ISNULL(b.motivoBaja, N'')       AS motivoBaja,
        ISNULL(ub.nombre, N'')          AS retiradaPor,
        c.nombres + N' ' + c.apellidos  AS candidato,
        c.slug                          AS candidatoSlug,
        ca.slug                         AS campanaSlug,
        ISNULL(cat.nombre, N'')         AS categoria,
        nv.nombre                       AS verificacion,
        (SELECT COUNT(*) FROM dbo.Valoraciones v
          INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto
          WHERE t.nombre = N'Publicacion' AND v.codigoObjeto = b.codigoPublicacion
            AND v.valor = 1)            AS meGusta,
        (SELECT COUNT(*) FROM dbo.Valoraciones v
          INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto
          WHERE t.nombre = N'Publicacion' AND v.codigoObjeto = b.codigoPublicacion
            AND v.valor = -1)           AS noMeGusta,
        (SELECT COUNT(*) FROM dbo.Comentarios m
          INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = m.codigoTipoObjeto
          WHERE t.nombre = N'Publicacion' AND m.codigoObjeto = b.codigoPublicacion)
                                        AS comentarios
    FROM dbo.Publicaciones b
    INNER JOIN dbo.Candidatos c           ON c.codigoCandidato = b.codigoCandidato
    INNER JOIN dbo.Campanas ca            ON ca.codigoCampana = b.codigoCampana
    INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = b.codigoVerificacion
    LEFT  JOIN dbo.Categorias cat         ON cat.codigoCategoria = b.codigoCategoria
    LEFT  JOIN dbo.Usuarios ub            ON ub.codigoUsuario = b.codigoUsuarioBaja
    WHERE (@campanaSlug IS NULL OR ca.slug = @campanaSlug)
      AND (@estado IS NULL
           OR (@estado = N'Activas'   AND b.activo = 1)
           OR (@estado = N'Retiradas' AND b.activo = 0))
    ORDER BY b.fecha DESC;
END
GO

/* ============================================================
   6. Retirar o restaurar una publicación

   Un solo procedimiento para las dos direcciones: son la misma
   decisión de moderación y comparten toda la comprobación. El
   motivo es obligatorio siempre — retirar contenido sin decir
   por qué es exactamente la opacidad que la plataforma le
   reprocha a los demás.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminModerarPublicacion') IS NOT NULL
    DROP PROCEDURE dbo.spAdminModerarPublicacion;
GO

CREATE PROCEDURE dbo.spAdminModerarPublicacion
    @codigoUsuario     INT,
    @codigoPublicacion INT,
    @activo            BIT,
    @motivo            NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;

    IF @motivo = N'' SET @motivo = NULL;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no tiene permiso para moderar publicaciones.' AS mensaje;
        RETURN;
    END

    IF @motivo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Hay que registrar el motivo de la decisión.' AS mensaje;
        RETURN;
    END

    DECLARE @actual BIT =
        (SELECT activo FROM dbo.Publicaciones WHERE codigoPublicacion = @codigoPublicacion);

    IF @actual IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró la publicación.' AS mensaje;
        RETURN;
    END

    IF @actual = @activo
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               CASE WHEN @activo = 1
                    THEN N'La publicación ya está visible.'
                    ELSE N'La publicación ya estaba retirada.' END AS mensaje;
        RETURN;
    END

    IF @activo = 0
        UPDATE dbo.Publicaciones
           SET activo = 0,
               motivoBaja = @motivo,
               fechaBaja = SYSDATETIME(),
               codigoUsuarioBaja = @codigoUsuario
         WHERE codigoPublicacion = @codigoPublicacion;
    ELSE
        /* Al restaurar se limpian los datos del retiro anterior. El
           rastro no se pierde: queda en la bitácora, que es donde
           corresponde. */
        UPDATE dbo.Publicaciones
           SET activo = 1,
               motivoBaja = NULL,
               fechaBaja = NULL,
               codigoUsuarioBaja = NULL
         WHERE codigoPublicacion = @codigoPublicacion;

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Publicacion');

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario,
            CASE WHEN @activo = 0 THEN N'Retiro' ELSE N'Restauracion' END,
            @codigoTipoObjeto, @codigoPublicacion,
            CASE WHEN @activo = 0
                 THEN N'Publicación retirada de la consulta pública'
                 ELSE N'Publicación restaurada' END,
            @motivo);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @activo = 0
                THEN N'La publicación quedó retirada de la consulta pública.'
                ELSE N'La publicación volvió a la consulta pública.' END AS mensaje;
END
GO

/* ============================================================
   7. Bitácora

   Se lee entera, sin filtrar por usuario: quien administra tiene
   que poder ver también lo que hicieron los demás. Esa es la
   diferencia entre un registro y una bitácora.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminAuditoria') IS NOT NULL
    DROP PROCEDURE dbo.spAdminAuditoria;
GO

CREATE PROCEDURE dbo.spAdminAuditoria
    @accion NVARCHAR(40) = NULL,
    @limite INT          = 100
AS
BEGIN
    SET NOCOUNT ON;

    IF @accion = N'' SET @accion = NULL;
    IF @limite IS NULL OR @limite <= 0 SET @limite = 100;

    SELECT TOP (@limite)
        a.codigoAuditoria,
        a.fecha,
        u.nombre               AS usuario,
        a.accion,
        t.nombre               AS tipoObjeto,
        a.codigoObjeto,
        ISNULL(a.detalle, N'') AS detalle,
        ISNULL(a.motivo, N'')  AS motivo
    FROM dbo.Auditoria a
    INNER JOIN dbo.Usuarios u    ON u.codigoUsuario = a.codigoUsuario
    INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = a.codigoTipoObjeto
    WHERE (@accion IS NULL OR a.accion = @accion)
    ORDER BY a.fecha DESC, a.codigoAuditoria DESC;
END
GO

PRINT 'Script 09 aplicado: bitacora de administracion, bandeja de verificacion y moderacion.';
GO
