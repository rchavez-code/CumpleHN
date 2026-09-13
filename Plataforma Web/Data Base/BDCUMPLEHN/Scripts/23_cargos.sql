/* ============================================================
   CumpleHN — 23. Cargos por espacio
   ------------------------------------------------------------
   El 02 dejó Cargos con codigoEspacio en NULL como «compartido
   por todos los espacios». Salió mal en la práctica: al dar de
   alta una candidatura en el espacio de una organización, el
   desplegable ofrecía Presidencia de la República, Diputación y
   Alcaldía, que no le sirven a una junta directiva, y no había
   manera de crear «Presidente», «Tesorero» o «Vocal».

   Se corrige de raíz:

     1. Los cargos son de cada espacio. Los seis de la plataforma
        pasan a ser suyos (codigoEspacio = plataforma) y dejan de
        aparecerle a nadie más. Un espacio nuevo arranca sin
        cargos y crea los que necesita. NULL deja de usarse en
        esta tabla, aunque la columna lo admita.

     2. Cada espacio administra sus cargos desde la pantalla de
        administración, con las reglas de siempre: alta y edición
        en un procedimiento, baja lógica con activo (un cargo con
        candidaturas no se borra), bitácora en cada escritura y
        fnEsAdministradorDe contra el espacio.

     3. El alta de candidatura (spAdminGuardarCandidato, del 10)
        exige que el cargo sea del espacio y esté activo. La
        columna activo nace en el 02, con las demás, para que el
        10 pueda referenciarla en una base nueva.

   Las categorías temáticas siguen compartidas (COFOG) y cada
   espacio puede sumar las suyas más adelante con el mismo
   patrón. No se tocan acá.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Migración
   ============================================================ */

/* Los cargos sin espacio son los de la plataforma. */
UPDATE dbo.Cargos
   SET codigoEspacio = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1)
 WHERE codigoEspacio IS NULL;
GO

MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Cargo', N'dbo.Cargos', N'Cargo al que se presenta una candidatura, propio de cada espacio.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

/* ============================================================
   2. Procedimientos
   ============================================================ */

IF OBJECT_ID('dbo.spAdminCargos') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCargos;
GO

CREATE PROCEDURE dbo.spAdminCargos
    @codigoEspacio INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.codigoCargo, c.nombre, c.nivelGobierno, c.orden, c.activo,
           (SELECT COUNT(*) FROM dbo.Candidatos k
             WHERE k.codigoCargo = c.codigoCargo AND k.activo = 1) AS candidaturas
    FROM dbo.Cargos c
    WHERE c.codigoEspacio = @codigoEspacio
    ORDER BY c.activo DESC, c.orden, c.nombre;
END
GO

IF OBJECT_ID('dbo.spAdminGuardarCargo') IS NOT NULL
    DROP PROCEDURE dbo.spAdminGuardarCargo;
GO

/* Alta y edición. @codigoEspacio cuenta solo en el alta: al editar
   se lee de la fila, como en partidos y campañas. */

CREATE PROCEDURE dbo.spAdminGuardarCargo
    @codigoUsuario INT,
    @codigoEspacio INT,
    @codigoCargo   INT,
    @nombre        NVARCHAR(80),
    @nivelGobierno NVARCHAR(20),
    @orden         INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @codigoCargo > 0
        SET @codigoEspacio = (SELECT codigoEspacio FROM dbo.Cargos WHERE codigoCargo = @codigoCargo);

    IF dbo.fnEsAdministradorDe(@codigoUsuario, @codigoEspacio) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no tiene permiso para administrar cargos en este espacio.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @nombre = LTRIM(RTRIM(ISNULL(@nombre, N'')));
    SET @nivelGobierno = LTRIM(RTRIM(ISNULL(@nivelGobierno, N'')));
    IF @orden IS NULL OR @orden < 0 SET @orden = 0;

    IF LEN(@nombre) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El nombre del cargo es obligatorio.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @nivelGobierno NOT IN (N'Nacional', N'Departamental', N'Municipal', N'Institucional')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El nivel del cargo no es válido.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* El nombre identifica al cargo en pantalla: único por espacio. */
    IF EXISTS (SELECT 1 FROM dbo.Cargos
                WHERE nombre = @nombre AND codigoEspacio = @codigoEspacio
                  AND codigoCargo <> @codigoCargo)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ya existe un cargo con ese nombre en este espacio.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Cargo');

    IF @codigoCargo > 0
    BEGIN
        UPDATE dbo.Cargos
           SET nombre = @nombre, nivelGobierno = @nivelGobierno, orden = @orden
         WHERE codigoCargo = @codigoCargo;

        INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle)
        VALUES (@codigoEspacio, @codigoUsuario, N'Edicion', @tipo, @codigoCargo, N'Cargo editado: ' + @nombre);

        SELECT CAST(1 AS BIT) AS ok, N'El cargo quedó actualizado.' AS mensaje, @codigoCargo AS codigo;
        RETURN;
    END

    INSERT INTO dbo.Cargos (nombre, nivelGobierno, orden, codigoEspacio, activo)
    VALUES (@nombre, @nivelGobierno, @orden, @codigoEspacio, 1);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle)
    VALUES (@codigoEspacio, @codigoUsuario, N'Alta', @tipo, @nuevo, N'Cargo registrado: ' + @nombre);

    SELECT CAST(1 AS BIT) AS ok, N'El cargo quedó registrado.' AS mensaje, @nuevo AS codigo;
END
GO

IF OBJECT_ID('dbo.spAdminEstadoCargo') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEstadoCargo;
GO

/* Desactivar un cargo lo saca del desplegable de candidaturas
   nuevas. Las que ya lo tienen no cambian: el cargo sigue siendo
   el que fue. */

CREATE PROCEDURE dbo.spAdminEstadoCargo
    @codigoUsuario INT,
    @codigoCargo   INT,
    @activo        BIT,
    @motivo        NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @motivo = NULLIF(LTRIM(RTRIM(ISNULL(@motivo, N''))), N'');

    DECLARE @nombre NVARCHAR(80), @codigoEspacio INT, @actual BIT;
    SELECT @nombre = nombre, @codigoEspacio = codigoEspacio, @actual = activo
      FROM dbo.Cargos WHERE codigoCargo = @codigoCargo;

    IF dbo.fnEsAdministradorDe(@codigoUsuario, @codigoEspacio) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no tiene permiso para administrar este cargo.' AS mensaje;
        RETURN;
    END

    IF @actual = @activo
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               CASE WHEN @activo = 1 THEN N'El cargo ya está activo.' ELSE N'El cargo ya estaba desactivado.' END AS mensaje;
        RETURN;
    END

    UPDATE dbo.Cargos SET activo = @activo WHERE codigoCargo = @codigoCargo;

    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Cargo');

    INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoEspacio, @codigoUsuario,
            CASE WHEN @activo = 0 THEN N'Baja' ELSE N'Alta' END, @tipo, @codigoCargo,
            CASE WHEN @activo = 0 THEN N'Cargo desactivado: ' ELSE N'Cargo reactivado: ' END + @nombre,
            @motivo);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @activo = 0
                THEN N'El cargo quedó desactivado. Las candidaturas que ya lo tienen no cambian.'
                ELSE N'El cargo volvió a estar disponible.' END AS mensaje;
END
GO

PRINT 'Script 23 aplicado: cargos por espacio.';
GO
