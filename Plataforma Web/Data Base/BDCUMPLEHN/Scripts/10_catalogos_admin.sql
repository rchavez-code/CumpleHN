/* ============================================================
   CumpleHN — 10. Catálogos administrados
   ------------------------------------------------------------
   Alta y edición de partidos, campañas y candidaturas, más la
   creación de la cuenta de acceso de una candidatura.

   Sigue las mismas reglas del script 09: cada procedimiento de
   escritura comprueba el rol con fnEsAdministrador, registra lo
   que hizo en Auditoria y devuelve una fila con ok y mensaje.

   Tres decisiones que conviene tener presentes:

     1. Los slugs los genera la base, no el formulario, y solo al
        dar de alta. Son la dirección pública de cada ficha:
        regenerarlos al editar rompería todo enlace que alguien ya
        haya compartido, y además los slugs que ya existen no
        siempre coinciden con lo que la función derivaría del
        nombre — la campaña «Elecciones Generales 2029» tiene el
        slug «generales-2029». Corregir un nombre mal escrito no
        debe mudar la ficha de dirección.

     2. Candidatos conserva las columnas de texto partido y
        partidoSiglas además de codigoPartido. Están
        desnormalizadas a propósito desde antes, y las consultas
        del Web Service las leen. El procedimiento de guardado
        las sincroniza desde Partidos en la misma operación: como
        es el único que escribe, no pueden quedar en desacuerdo.

     3. Nada se borra. Partidos y Candidatos se desactivan con su
        columna activo, y las campañas se cierran con su estado.
        Borrar un partido dejaría candidaturas huérfanas y
        borraría de la historia a quién se presentó por él.

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
   1. Ampliación de la bitácora

   El script 09 dejó la restricción con las tres acciones de
   revisión de contenido. Administrar catálogos agrega tres más.
   ============================================================ */

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auditoria_accion')
    ALTER TABLE dbo.Auditoria DROP CONSTRAINT CK_Auditoria_accion;
GO

ALTER TABLE dbo.Auditoria
    ADD CONSTRAINT CK_Auditoria_accion
        CHECK (accion IN ('Verificacion', 'Retiro', 'Restauracion',
                          'Alta', 'Edicion', 'Baja', 'Cuenta'));
GO

/* Las campañas no estaban en el catálogo de tipos: hasta ahora
   nadie podía opinar sobre una campaña, así que no hacía falta.
   La bitácora sí necesita poder referirse a ellas. */

MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Campana', N'dbo.Campanas', N'Campaña electoral.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

/* Y los usuarios, para poder registrar la creación de una cuenta. */

MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Usuario', N'dbo.Usuarios', N'Cuenta de acceso a la plataforma.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

/* ============================================================
   2. Generación de slugs

   El slug es parte de la dirección pública de cada ficha. Se
   deriva del nombre con esta función y no se pide en el
   formulario: dos personas escribiendo el mismo partido
   producirían dos direcciones distintas.
   ============================================================ */

IF OBJECT_ID('dbo.fnSlug') IS NOT NULL
    DROP FUNCTION dbo.fnSlug;
GO

CREATE FUNCTION dbo.fnSlug (@texto NVARCHAR(300))
RETURNS NVARCHAR(160)
AS
BEGIN
    IF @texto IS NULL RETURN N'';

    DECLARE @s NVARCHAR(300) = LOWER(LTRIM(RTRIM(@texto)));

    /* Las tildes y la eñe se transliteran: un slug con caracteres
       fuera del ASCII se escapa en la barra de direcciones y deja
       de ser legible, que es lo único que justifica tener slug. */
    SET @s = REPLACE(@s, N'á', N'a');
    SET @s = REPLACE(@s, N'é', N'e');
    SET @s = REPLACE(@s, N'í', N'i');
    SET @s = REPLACE(@s, N'ó', N'o');
    SET @s = REPLACE(@s, N'ú', N'u');
    SET @s = REPLACE(@s, N'ü', N'u');
    SET @s = REPLACE(@s, N'ñ', N'n');

    /* Signos que no aportan a una dirección. */
    SET @s = REPLACE(@s, N'.', N'');
    SET @s = REPLACE(@s, N',', N'');
    SET @s = REPLACE(@s, N'''', N'');
    SET @s = REPLACE(@s, N'"', N'');
    SET @s = REPLACE(@s, N'(', N'');
    SET @s = REPLACE(@s, N')', N'');
    SET @s = REPLACE(@s, N'/', N'-');
    SET @s = REPLACE(@s, N'_', N'-');

    SET @s = REPLACE(@s, N' ', N'-');

    /* Varios guiones seguidos quedan en uno. Tres pasadas alcanzan
       para los nombres de esta escala. */
    SET @s = REPLACE(@s, N'---', N'-');
    SET @s = REPLACE(@s, N'--', N'-');
    SET @s = REPLACE(@s, N'--', N'-');

    RETURN LEFT(@s, 160);
END
GO

/* ============================================================
   3. Partidos
   ============================================================ */

IF OBJECT_ID('dbo.spAdminPartidos') IS NOT NULL
    DROP PROCEDURE dbo.spAdminPartidos;
GO

CREATE PROCEDURE dbo.spAdminPartidos
    @soloActivos BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.codigoPartido,
        p.slug,
        p.nombre,
        ISNULL(p.siglas, N'')      AS siglas,
        ISNULL(p.descripcion, N'') AS descripcion,
        p.activo,
        (SELECT COUNT(*) FROM dbo.Candidatos c
          WHERE c.codigoPartido = p.codigoPartido AND c.activo = 1) AS candidaturas
    FROM dbo.Partidos p
    WHERE (@soloActivos = 0 OR p.activo = 1)
    ORDER BY p.nombre;
END
GO

IF OBJECT_ID('dbo.spAdminGuardarPartido') IS NOT NULL
    DROP PROCEDURE dbo.spAdminGuardarPartido;
GO

/* Un solo procedimiento para alta y edición: con @codigoPartido en
   cero es alta. Son la misma validación y el mismo conjunto de
   campos, así que separarlos duplicaría las dos cosas. */

CREATE PROCEDURE dbo.spAdminGuardarPartido
    @codigoUsuario INT,
    @codigoPartido INT,
    @nombre        NVARCHAR(120),
    @siglas        NVARCHAR(20)  = NULL,
    @descripcion   NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no tiene permiso para administrar partidos.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @nombre = LTRIM(RTRIM(ISNULL(@nombre, N'')));
    IF @siglas = N'' SET @siglas = NULL;
    IF @descripcion = N'' SET @descripcion = NULL;

    IF LEN(@nombre) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El nombre del partido es obligatorio.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @slug NVARCHAR(160) = dbo.fnSlug(@nombre);

    /* El nombre identifica al partido en pantalla. Dos partidos con
       el mismo nombre serían indistinguibles para quien consulta. */
    IF EXISTS (SELECT 1 FROM dbo.Partidos
                WHERE nombre = @nombre AND codigoPartido <> @codigoPartido)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ya existe un partido con ese nombre.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* El slug solo se comprueba en el alta, que es la única vez que
       se asigna. Al editar se conserva el que ya tiene. */
    IF @codigoPartido = 0 AND EXISTS (SELECT 1 FROM dbo.Partidos WHERE slug = @slug)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Ese nombre produce una dirección que ya está en uso.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Partido');

    IF @codigoPartido > 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Partidos WHERE codigoPartido = @codigoPartido)
        BEGIN
            SELECT CAST(0 AS BIT) AS ok, N'No se encontró el partido.' AS mensaje, 0 AS codigo;
            RETURN;
        END

        /* El slug no se toca: es la dirección pública que ya puede
           estar compartida en otro lado. */
        UPDATE dbo.Partidos
           SET nombre = @nombre,
               siglas = @siglas,
               descripcion = @descripcion
         WHERE codigoPartido = @codigoPartido;

        /* El nombre desnormalizado que guardan las candidaturas se
           actualiza con el partido. Si no, la ficha del candidato
           seguiría mostrando el nombre viejo. */
        UPDATE dbo.Candidatos
           SET partido = @nombre,
               partidoSiglas = @siglas
         WHERE codigoPartido = @codigoPartido;

        INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
        VALUES (@codigoUsuario, N'Edicion', @codigoTipoObjeto, @codigoPartido,
                N'Partido editado: ' + @nombre, NULL);

        SELECT CAST(1 AS BIT) AS ok, N'El partido quedó actualizado.' AS mensaje, @codigoPartido AS codigo;
        RETURN;
    END

    INSERT INTO dbo.Partidos (slug, nombre, siglas, descripcion, activo)
    VALUES (@slug, @nombre, @siglas, @descripcion, 1);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario, N'Alta', @codigoTipoObjeto, @nuevo,
            N'Partido registrado: ' + @nombre, NULL);

    SELECT CAST(1 AS BIT) AS ok, N'El partido quedó registrado.' AS mensaje, @nuevo AS codigo;
END
GO

IF OBJECT_ID('dbo.spAdminEstadoPartido') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEstadoPartido;
GO

CREATE PROCEDURE dbo.spAdminEstadoPartido
    @codigoUsuario INT,
    @codigoPartido INT,
    @activo        BIT,
    @motivo        NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @motivo = N'' SET @motivo = NULL;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no tiene permiso para administrar partidos.' AS mensaje;
        RETURN;
    END

    DECLARE @nombre NVARCHAR(120) =
        (SELECT nombre FROM dbo.Partidos WHERE codigoPartido = @codigoPartido);

    IF @nombre IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró el partido.' AS mensaje;
        RETURN;
    END

    /* Desactivar un partido con candidaturas activas escondería de
       la consulta pública a gente que sí se presentó por él. */
    IF @activo = 0 AND EXISTS (SELECT 1 FROM dbo.Candidatos
                                WHERE codigoPartido = @codigoPartido AND activo = 1)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El partido tiene candidaturas activas. Desactivalas primero.' AS mensaje;
        RETURN;
    END

    UPDATE dbo.Partidos SET activo = @activo WHERE codigoPartido = @codigoPartido;

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Partido');

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario,
            CASE WHEN @activo = 0 THEN N'Baja' ELSE N'Alta' END,
            @codigoTipoObjeto, @codigoPartido,
            CASE WHEN @activo = 0 THEN N'Partido desactivado: ' ELSE N'Partido reactivado: ' END + @nombre,
            @motivo);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @activo = 0
                THEN N'El partido quedó desactivado.'
                ELSE N'El partido volvió a estar activo.' END AS mensaje;
END
GO

/* ============================================================
   4. Campañas
   ============================================================ */

IF OBJECT_ID('dbo.spAdminCampanas') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCampanas;
GO

CREATE PROCEDURE dbo.spAdminCampanas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.codigoCampana,
        c.slug,
        c.nombre,
        ISNULL(c.resumen, N'')     AS resumen,
        ISNULL(c.descripcion, N'') AS descripcion,
        ISNULL(c.alcance, N'')     AS alcance,
        c.fechaInicio,
        c.fechaEleccion,
        c.estado,
        c.esActual,
        (SELECT COUNT(*) FROM dbo.Candidatos x
          WHERE x.codigoCampana = c.codigoCampana AND x.activo = 1) AS candidaturas,
        (SELECT COUNT(*) FROM dbo.Propuestas x
          WHERE x.codigoCampana = c.codigoCampana) AS propuestas
    FROM dbo.Campanas c
    ORDER BY c.fechaEleccion DESC;
END
GO

IF OBJECT_ID('dbo.spAdminGuardarCampana') IS NOT NULL
    DROP PROCEDURE dbo.spAdminGuardarCampana;
GO

CREATE PROCEDURE dbo.spAdminGuardarCampana
    @codigoUsuario INT,
    @codigoCampana INT,
    @nombre        NVARCHAR(160),
    @resumen       NVARCHAR(400) = NULL,
    @descripcion   NVARCHAR(MAX) = NULL,
    @alcance       NVARCHAR(200) = NULL,
    @fechaInicio   DATE,
    @fechaEleccion DATE,
    @estado        NVARCHAR(20),
    @esActual      BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no tiene permiso para administrar campañas.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @nombre = LTRIM(RTRIM(ISNULL(@nombre, N'')));
    IF @resumen = N'' SET @resumen = NULL;
    IF @descripcion = N'' SET @descripcion = NULL;
    IF @alcance = N'' SET @alcance = NULL;

    IF LEN(@nombre) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El nombre de la campaña es obligatorio.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @estado NOT IN (N'Activa', N'Proxima', N'Cerrada')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El estado de la campaña no es válido.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @fechaEleccion < @fechaInicio
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La fecha de elección no puede ser anterior al inicio de la campaña.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Una campaña cerrada no puede ser la destacada de la portada. */
    IF @estado = N'Cerrada' SET @esActual = 0;

    DECLARE @slug NVARCHAR(160) = dbo.fnSlug(@nombre);

    IF EXISTS (SELECT 1 FROM dbo.Campanas
                WHERE nombre = @nombre AND codigoCampana <> @codigoCampana)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ya existe una campaña con ese nombre.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Igual que en partidos: el slug solo se asigna al dar de alta. */
    IF @codigoCampana = 0 AND EXISTS (SELECT 1 FROM dbo.Campanas WHERE slug = @slug)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Ese nombre produce una dirección que ya está en uso.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Campana');

    /* Solo una campaña puede estar destacada. El índice filtrado
       UQ_Campanas_unicaActual lo impone, así que hay que apagar la
       anterior antes de encender esta o el UPDATE falla. */
    IF @esActual = 1
        UPDATE dbo.Campanas SET esActual = 0
         WHERE esActual = 1 AND codigoCampana <> @codigoCampana;

    IF @codigoCampana > 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Campanas WHERE codigoCampana = @codigoCampana)
        BEGIN
            SELECT CAST(0 AS BIT) AS ok, N'No se encontró la campaña.' AS mensaje, 0 AS codigo;
            RETURN;
        END

        UPDATE dbo.Campanas
           SET nombre = @nombre,
               resumen = @resumen,
               descripcion = @descripcion,
               alcance = @alcance,
               fechaInicio = @fechaInicio,
               fechaEleccion = @fechaEleccion,
               estado = @estado,
               esActual = @esActual
         WHERE codigoCampana = @codigoCampana;

        INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
        VALUES (@codigoUsuario, N'Edicion', @codigoTipoObjeto, @codigoCampana,
                N'Campaña editada: ' + @nombre, NULL);

        SELECT CAST(1 AS BIT) AS ok, N'La campaña quedó actualizada.' AS mensaje, @codigoCampana AS codigo;
        RETURN;
    END

    INSERT INTO dbo.Campanas (slug, nombre, resumen, descripcion, alcance,
                              fechaInicio, fechaEleccion, estado, esActual)
    VALUES (@slug, @nombre, @resumen, @descripcion, @alcance,
            @fechaInicio, @fechaEleccion, @estado, @esActual);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario, N'Alta', @codigoTipoObjeto, @nuevo,
            N'Campaña registrada: ' + @nombre, NULL);

    SELECT CAST(1 AS BIT) AS ok, N'La campaña quedó registrada.' AS mensaje, @nuevo AS codigo;
END
GO

/* ============================================================
   5. Candidaturas
   ============================================================ */

IF OBJECT_ID('dbo.spAdminCandidatos') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCandidatos;
GO

CREATE PROCEDURE dbo.spAdminCandidatos
    @campanaSlug NVARCHAR(80) = NULL,
    @soloActivos BIT          = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @campanaSlug = N'' SET @campanaSlug = NULL;

    SELECT
        c.codigoCandidato,
        c.slug,
        c.nombres,
        c.apellidos,
        (c.nombres + N' ' + c.apellidos) AS nombreCompleto,
        c.codigoCampana,
        ca.slug                    AS campanaSlug,
        ca.nombre                  AS campana,
        ISNULL(c.codigoPartido, 0) AS codigoPartido,
        ISNULL(c.partido, N'')     AS partido,
        c.codigoCargo,
        cg.nombre                  AS cargo,
        ISNULL(c.codigoDepartamento, 0) AS codigoDepartamento,
        ISNULL(d.nombre, N'')      AS departamento,
        ISNULL(c.municipio, N'')   AS municipio,
        ISNULL(c.titular, N'')     AS titular,
        nv.nombre                  AS verificacion,
        c.activo,
        c.fechaRegistro,
        /* Si ya tiene cuenta de acceso. Una candidatura sin cuenta
           no puede administrar su propio perfil. */
        ISNULL((SELECT TOP 1 u.login FROM dbo.Usuarios u
                 WHERE u.codigoCandidato = c.codigoCandidato), N'') AS login,
        (SELECT COUNT(*) FROM dbo.Propuestas p
          WHERE p.codigoCandidato = c.codigoCandidato) AS propuestas
    FROM dbo.Candidatos c
    INNER JOIN dbo.Campanas ca            ON ca.codigoCampana = c.codigoCampana
    INNER JOIN dbo.Cargos cg              ON cg.codigoCargo = c.codigoCargo
    INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = c.codigoVerificacion
    LEFT  JOIN dbo.Departamentos d        ON d.codigoDepartamento = c.codigoDepartamento
    WHERE (@campanaSlug IS NULL OR ca.slug = @campanaSlug)
      AND (@soloActivos = 0 OR c.activo = 1)
    ORDER BY c.apellidos, c.nombres;
END
GO

IF OBJECT_ID('dbo.spAdminGuardarCandidato') IS NOT NULL
    DROP PROCEDURE dbo.spAdminGuardarCandidato;
GO

/* Registra los datos de identificación de una candidatura. El
   resto del perfil (biografía, contacto, redes) lo llena la propia
   candidatura desde su panel: la plataforma la registra, no la
   redacta. */

CREATE PROCEDURE dbo.spAdminGuardarCandidato
    @codigoUsuario      INT,
    @codigoCandidato    INT,
    @nombres            NVARCHAR(80),
    @apellidos          NVARCHAR(80),
    @codigoCampana      INT,
    @codigoCargo        INT,
    @codigoPartido      INT = 0,   -- cero es candidatura independiente
    @codigoDepartamento INT = 0,
    @municipio          NVARCHAR(120) = NULL,
    @titular            NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no tiene permiso para administrar candidaturas.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @nombres = LTRIM(RTRIM(ISNULL(@nombres, N'')));
    SET @apellidos = LTRIM(RTRIM(ISNULL(@apellidos, N'')));
    IF @municipio = N'' SET @municipio = NULL;
    IF @titular = N'' SET @titular = NULL;

    IF LEN(@nombres) < 2 OR LEN(@apellidos) < 2
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El nombre y los apellidos son obligatorios.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Campanas WHERE codigoCampana = @codigoCampana)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Elegí la campaña a la que se presenta.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Cargos WHERE codigoCargo = @codigoCargo)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Elegí el cargo al que aspira.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* El partido queda nulo en las candidaturas independientes. Es
       información, no un dato faltante. */
    DECLARE @partidoNombre NVARCHAR(120) = NULL;
    DECLARE @partidoSiglas NVARCHAR(20) = NULL;

    IF @codigoPartido > 0
    BEGIN
        SELECT @partidoNombre = nombre, @partidoSiglas = siglas
          FROM dbo.Partidos
         WHERE codigoPartido = @codigoPartido AND activo = 1;

        IF @partidoNombre IS NULL
        BEGIN
            SELECT CAST(0 AS BIT) AS ok, N'El partido elegido no existe o está desactivado.' AS mensaje, 0 AS codigo;
            RETURN;
        END
    END
    ELSE
        SET @codigoPartido = NULL;

    IF @codigoDepartamento = 0 SET @codigoDepartamento = NULL;

    DECLARE @nombreCompleto NVARCHAR(200) = @nombres + N' ' + @apellidos;
    DECLARE @slug NVARCHAR(160) = dbo.fnSlug(@nombreCompleto);

    /* En el alta el slug tiene que ser nuevo. Al editar se conserva
       el que ya tiene: la ficha pública de una candidatura no debe
       mudarse de dirección porque se corrigió una tilde del nombre. */
    IF @codigoCandidato = 0 AND EXISTS (SELECT 1 FROM dbo.Candidatos WHERE slug = @slug)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Ya hay una candidatura registrada con ese nombre.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Candidato');

    IF @codigoCandidato > 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Candidatos WHERE codigoCandidato = @codigoCandidato)
        BEGIN
            SELECT CAST(0 AS BIT) AS ok, N'No se encontró la candidatura.' AS mensaje, 0 AS codigo;
            RETURN;
        END

        UPDATE dbo.Candidatos
           SET nombres = @nombres,
               apellidos = @apellidos,
               codigoCampana = @codigoCampana,
               codigoCargo = @codigoCargo,
               codigoPartido = @codigoPartido,
               partido = @partidoNombre,
               partidoSiglas = @partidoSiglas,
               codigoDepartamento = @codigoDepartamento,
               municipio = @municipio,
               titular = @titular
         WHERE codigoCandidato = @codigoCandidato;

        INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
        VALUES (@codigoUsuario, N'Edicion', @codigoTipoObjeto, @codigoCandidato,
                N'Candidatura editada: ' + @nombreCompleto, NULL);

        SELECT CAST(1 AS BIT) AS ok, N'La candidatura quedó actualizada.' AS mensaje, @codigoCandidato AS codigo;
        RETURN;
    END

    /* Nace como declarada: lo que la plataforma sabe de ella es lo
       que alguien acaba de escribir, y eso todavía no está
       contrastado contra ninguna fuente. */
    DECLARE @declarado INT =
        (SELECT codigoVerificacion FROM dbo.NivelesVerificacion WHERE nombre = N'Declarado');

    INSERT INTO dbo.Candidatos
        (slug, codigoCampana, nombres, apellidos, partido, partidoSiglas,
         codigoCargo, codigoDepartamento, municipio, titular,
         codigoVerificacion, activo, codigoPartido)
    VALUES
        (@slug, @codigoCampana, @nombres, @apellidos, @partidoNombre, @partidoSiglas,
         @codigoCargo, @codigoDepartamento, @municipio, @titular,
         @declarado, 1, @codigoPartido);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario, N'Alta', @codigoTipoObjeto, @nuevo,
            N'Candidatura registrada: ' + @nombreCompleto, NULL);

    SELECT CAST(1 AS BIT) AS ok, N'La candidatura quedó registrada.' AS mensaje, @nuevo AS codigo;
END
GO

IF OBJECT_ID('dbo.spAdminEstadoCandidato') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEstadoCandidato;
GO

CREATE PROCEDURE dbo.spAdminEstadoCandidato
    @codigoUsuario   INT,
    @codigoCandidato INT,
    @activo          BIT,
    @motivo          NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;

    IF @motivo = N'' SET @motivo = NULL;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La cuenta no tiene permiso para administrar candidaturas.' AS mensaje;
        RETURN;
    END

    IF @motivo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Hay que registrar el motivo de la decisión.' AS mensaje;
        RETURN;
    END

    DECLARE @nombre NVARCHAR(200) =
        (SELECT nombres + N' ' + apellidos FROM dbo.Candidatos WHERE codigoCandidato = @codigoCandidato);

    IF @nombre IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró la candidatura.' AS mensaje;
        RETURN;
    END

    UPDATE dbo.Candidatos SET activo = @activo WHERE codigoCandidato = @codigoCandidato;

    /* La cuenta de acceso sigue al estado de la candidatura: una
       candidatura retirada que aún puede publicar sería una puerta
       abierta sin ficha visible detrás. */
    UPDATE dbo.Usuarios SET activo = @activo WHERE codigoCandidato = @codigoCandidato;

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Candidato');

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario,
            CASE WHEN @activo = 0 THEN N'Baja' ELSE N'Alta' END,
            @codigoTipoObjeto, @codigoCandidato,
            CASE WHEN @activo = 0
                 THEN N'Candidatura retirada de la consulta pública: '
                 ELSE N'Candidatura reactivada: ' END + @nombre,
            @motivo);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @activo = 0
                THEN N'La candidatura quedó fuera de la consulta pública.'
                ELSE N'La candidatura volvió a la consulta pública.' END AS mensaje;
END
GO

/* ============================================================
   6. Cuenta de acceso de una candidatura

   Sin cuenta, una candidatura registrada no puede administrar su
   perfil ni registrar sus propuestas: solo existe como ficha que
   alguien más llenó.

   El hash se calcula igual que en el backend, SHA-256 en
   hexadecimal minúscula sobre VARCHAR. Si se cambia uno de los
   dos hay que cambiar el otro o ningún acceso funciona.
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

    SET @login = LOWER(LTRIM(RTRIM(ISNULL(@login, N''))));
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

    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correo = @correo)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ese correo ya está registrado.' AS mensaje;
        RETURN;
    END

    DECLARE @rolCandidato INT = (SELECT codigoRol FROM dbo.Roles WHERE nombre = N'Candidato');

    DECLARE @hash CHAR(64) =
        LOWER(CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), @clave)), 2));

    INSERT INTO dbo.Usuarios (login, clave, nombre, correo, codigoRol, codigoCandidato, activo)
    VALUES (@login, @hash, @nombre, @correo, @rolCandidato, @codigoCandidato, 1);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Usuario');

    /* La bitácora registra que se creó la cuenta y para quién. La
       contraseña no aparece por ninguna parte, ni siquiera acá. */
    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario, N'Cuenta', @codigoTipoObjeto, @nuevo,
            N'Cuenta de acceso creada para ' + @nombre + N' (' + @login + N')', NULL);

    SELECT CAST(1 AS BIT) AS ok,
           N'La cuenta quedó creada. Entregale la contraseña a la candidatura por un medio seguro.' AS mensaje;
END
GO

PRINT 'Script 10 aplicado: partidos, campanas, candidaturas y cuentas de acceso.';
GO
