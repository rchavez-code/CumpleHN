/* ============================================================
   CumpleHN — 19. Espacios
   ------------------------------------------------------------
   La plataforma pasa a admitir espacios: un espacio es un
   cliente que compra el uso de CumpleHN para su propio proceso
   electoral (la junta directiva de un colegio profesional, de
   una cooperativa, de una asociación de estudiantes). Es la
   base del modelo de sostenibilidad del proyecto: el sitio
   público sigue siendo gratuito y neutral, y lo que se vende es
   el mismo motor como espacio privado que administra la
   organización.

   Qué hay en este script y qué no:

     · La tabla Espacios y las columnas codigoEspacio NO viven
       acá. Nacen con sus tablas en los scripts 02, 05, 09 y 18,
       porque las vistas del 07 y los procedimientos del 08 en
       adelante ya las referencian: en una base nueva fallarían
       si la columna se agregara al final. Este script recibe
       una base donde las columnas existen y están en NULL.

     · Acá se migra todo lo existente al espacio de la
       plataforma, se fijan los NOT NULL, se reemplazan las
       restricciones únicas que eran globales y pasan a ser por
       espacio, y se crean la vista y los procedimientos con los
       que la plataforma administra sus espacios.

   Decisiones que sostienen el módulo:

     1. Un solo mecanismo de aislamiento: la campaña lleva el
        espacio, y candidaturas, propuestas, publicaciones y
        encuestas lo heredan por ella. Lo que no cuelga de
        campaña (partidos, iniciativas, bitácora) lo lleva
        directo. Todo procedimiento de lectura de un espacio
        recibe @codigoEspacio y ningún NULL significa «todos».

     2. El permiso se comprueba contra el espacio del objeto,
        derivado del objeto y nunca recibido (fnEsAdministradorDe
        en el script 09). Quien administra un espacio es una
        cuenta con rol Administrador y Usuarios.codigoEspacio
        igual a ese espacio. Con la columna en NULL administra
        la plataforma entera. No hace falta un cuarto rol.

     3. Los slugs siguen siendo únicos en toda la plataforma:
        /Campana/{slug} y /Candidato/{slug} identifican la ficha
        sin prefijo, y la ficha dice a qué espacio pertenece. Lo
        que sí pasa a ser único por espacio es el nombre, porque
        dos organizaciones pueden tener una «Planilla Azul».

     4. El asistente de IA solo lee el espacio de la plataforma,
        y lo fijan los procedimientos del 12, no el backend. Los
        espacios son clientes: su lista y quién los administra
        no es materia del asistente, y llevan DENY para su login.

     5. Nada se borra. Un espacio se retira con activo y motivo,
        como todo lo demás, y su contenido queda fuera de toda
        consulta pública sin desaparecer de la base.

   Lo que queda para los scripts siguientes: el padrón (quién
   puede participar en un espacio con padrón cerrado, script 20)
   y la suscripción (vigencia y pagos registrados, script 21).

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* Índices filtrados: ver la nota del script 09. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Migración de lo existente al espacio de la plataforma

   Todo lo que ya había pertenece a CumpleHN. Después de esto
   ninguna columna codigoEspacio queda en NULL, salvo las que lo
   admiten a propósito: Usuarios (administra la plataforma) y
   los catálogos Cargos y Categorias (compartido por todos).
   ============================================================ */

DECLARE @plataforma INT = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1);

UPDATE dbo.Campanas    SET codigoEspacio = @plataforma WHERE codigoEspacio IS NULL;
UPDATE dbo.Partidos    SET codigoEspacio = @plataforma WHERE codigoEspacio IS NULL;
UPDATE dbo.Iniciativas SET codigoEspacio = @plataforma WHERE codigoEspacio IS NULL;
UPDATE dbo.Auditoria   SET codigoEspacio = @plataforma WHERE codigoEspacio IS NULL;
GO

ALTER TABLE dbo.Campanas    ALTER COLUMN codigoEspacio INT NOT NULL;
ALTER TABLE dbo.Partidos    ALTER COLUMN codigoEspacio INT NOT NULL;
ALTER TABLE dbo.Iniciativas ALTER COLUMN codigoEspacio INT NOT NULL;
ALTER TABLE dbo.Auditoria   ALTER COLUMN codigoEspacio INT NOT NULL;
GO

/* ============================================================
   2. Restricciones que pasan de globales a por espacio

   Cada bloque quita la restricción anterior si todavía existe y
   crea la nueva si todavía no. En una base nueva la nueva ya
   viene del CREATE TABLE y no hay nada que hacer.
   ============================================================ */

/* Una campaña destacada por espacio, no por plataforma. El
   índice anterior era sobre (esActual) a secas. */
IF EXISTS (SELECT 1 FROM sys.indexes i
           WHERE i.name = 'UQ_Campanas_unicaActual'
             AND NOT EXISTS (SELECT 1 FROM sys.index_columns ic
                             INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                             WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
                               AND c.name = 'codigoEspacio'))
    DROP INDEX UQ_Campanas_unicaActual ON dbo.Campanas;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Campanas_unicaActual')
CREATE UNIQUE INDEX UQ_Campanas_unicaActual
    ON dbo.Campanas (codigoEspacio, esActual) WHERE esActual = 1;
GO

/* Nombres únicos por espacio en partidos, cargos y categorías. */
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Partidos_nombre')
    ALTER TABLE dbo.Partidos DROP CONSTRAINT UQ_Partidos_nombre;
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Partidos_espacioNombre')
    ALTER TABLE dbo.Partidos ADD CONSTRAINT UQ_Partidos_espacioNombre UNIQUE (codigoEspacio, nombre);
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Cargos_nombre')
    ALTER TABLE dbo.Cargos DROP CONSTRAINT UQ_Cargos_nombre;
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Cargos_espacioNombre')
    ALTER TABLE dbo.Cargos ADD CONSTRAINT UQ_Cargos_espacioNombre UNIQUE (codigoEspacio, nombre);
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Categorias_nombre')
    ALTER TABLE dbo.Categorias DROP CONSTRAINT UQ_Categorias_nombre;
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Categorias_espacioNombre')
    ALTER TABLE dbo.Categorias ADD CONSTRAINT UQ_Categorias_espacioNombre UNIQUE (codigoEspacio, nombre);
GO

/* Un cargo de una elección interna no es nacional, departamental
   ni municipal. El nivel «Institucional» existe para ellos, y el
   tablero lo trata como un nivel más. */
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Cargos_nivel')
    ALTER TABLE dbo.Cargos DROP CONSTRAINT CK_Cargos_nivel;
GO

ALTER TABLE dbo.Cargos
    ADD CONSTRAINT CK_Cargos_nivel
        CHECK (nivelGobierno IN ('Nacional','Departamental','Municipal','Institucional'));
GO

/* Índices para el filtro por espacio, que ahora va en casi
   toda consulta. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Campanas_espacio')
CREATE INDEX IX_Campanas_espacio ON dbo.Campanas (codigoEspacio, esActual);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Partidos_espacio')
CREATE INDEX IX_Partidos_espacio ON dbo.Partidos (codigoEspacio, activo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Iniciativas_espacio')
CREATE INDEX IX_Iniciativas_espacio ON dbo.Iniciativas (codigoEspacio, activo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auditoria_espacio')
CREATE INDEX IX_Auditoria_espacio ON dbo.Auditoria (codigoEspacio, fecha DESC);
GO

/* ============================================================
   3. Catálogos que suma este módulo
   ============================================================ */

/* El espacio entra al catálogo de tipos para que la bitácora
   pueda referirse a él. No para valorarlo ni comentarlo: el
   Web Service no lo acepta en TipoValido. */
MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Espacio', N'dbo.Espacios', N'Espacio de un cliente de la plataforma.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

/* ============================================================
   4. El espacio de un objeto

   El par polimórfico (tipo, código) resuelto a su espacio. Es lo
   que el Web Service usa para saber en qué espacio cae una
   valoración o un comentario antes de aplicar el padrón, y lo
   que cualquier procedimiento nuevo debe usar en lugar de
   recibir el espacio de quien llama.

   Va acá y no en el 09 porque referencia Encuestas (14) e
   Iniciativas (18), que en una base nueva todavía no existen
   cuando corre el 09.

   Devuelve NULL cuando el objeto no existe, y fnEsAdministradorDe
   responde 0 ante NULL, así que «no existe» y «no es tuyo» se
   rechazan por el mismo camino, sin revelar cuál de los dos fue.
   ============================================================ */

IF OBJECT_ID('dbo.fnEspacioDeObjeto') IS NOT NULL
    DROP FUNCTION dbo.fnEspacioDeObjeto;
GO

CREATE FUNCTION dbo.fnEspacioDeObjeto (@tipoObjeto NVARCHAR(40), @codigoObjeto INT)
RETURNS INT
AS
BEGIN
    DECLARE @e INT = NULL;

    IF @tipoObjeto = N'Campana'
        SELECT @e = codigoEspacio FROM dbo.Campanas WHERE codigoCampana = @codigoObjeto;
    ELSE IF @tipoObjeto = N'Candidato'
        SELECT @e = ca.codigoEspacio FROM dbo.Candidatos c
         INNER JOIN dbo.Campanas ca ON ca.codigoCampana = c.codigoCampana
         WHERE c.codigoCandidato = @codigoObjeto;
    ELSE IF @tipoObjeto = N'Propuesta'
        SELECT @e = ca.codigoEspacio FROM dbo.Propuestas p
         INNER JOIN dbo.Campanas ca ON ca.codigoCampana = p.codigoCampana
         WHERE p.codigoPropuesta = @codigoObjeto;
    ELSE IF @tipoObjeto = N'Publicacion'
        SELECT @e = ca.codigoEspacio FROM dbo.Publicaciones b
         INNER JOIN dbo.Campanas ca ON ca.codigoCampana = b.codigoCampana
         WHERE b.codigoPublicacion = @codigoObjeto;
    ELSE IF @tipoObjeto = N'Partido'
        SELECT @e = codigoEspacio FROM dbo.Partidos WHERE codigoPartido = @codigoObjeto;
    ELSE IF @tipoObjeto = N'Encuesta'
        SELECT @e = ca.codigoEspacio FROM dbo.Encuestas e
         INNER JOIN dbo.Campanas ca ON ca.codigoCampana = e.codigoCampana
         WHERE e.codigoEncuesta = @codigoObjeto;
    ELSE IF @tipoObjeto = N'Iniciativa'
        SELECT @e = codigoEspacio FROM dbo.Iniciativas WHERE codigoIniciativa = @codigoObjeto;
    ELSE IF @tipoObjeto = N'Espacio'
        SELECT @e = codigoEspacio FROM dbo.Espacios WHERE codigoEspacio = @codigoObjeto;

    RETURN @e;
END
GO

/* ============================================================
   5. Vista de espacios

   Una fila por espacio con su estado derivado y sus conteos.
   El estado no se guarda: se calcula de activo y esPlataforma,
   y el script 21 le suma Vigente y Vencido cuando existan las
   suscripciones. Una columna «estado» escrita a mano sería la
   misma mentira esperando a ocurrir que se evitó en vwEncuestas.

   CREATE OR ALTER, para que volver a ejecutar el script no se
   lleve el DENY del bloque 7.
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
        ELSE                         N'Activo'
    END                           AS estado,
    (SELECT COUNT(*) FROM dbo.Campanas c
      WHERE c.codigoEspacio = e.codigoEspacio)                        AS campanas,
    (SELECT COUNT(*) FROM dbo.Candidatos k
      INNER JOIN dbo.Campanas c ON c.codigoCampana = k.codigoCampana
      WHERE c.codigoEspacio = e.codigoEspacio AND k.activo = 1)       AS candidaturas,
    /* Los de la plataforma llevan codigoEspacio en NULL. */
    (SELECT COUNT(*) FROM dbo.Usuarios u
      INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol
      WHERE r.nombre = N'Administrador' AND u.activo = 1
        AND ((e.esPlataforma = 1 AND u.codigoEspacio IS NULL)
             OR u.codigoEspacio = e.codigoEspacio))                   AS administradores
FROM dbo.Espacios e
LEFT JOIN dbo.Usuarios up ON up.codigoUsuario = e.codigoUsuarioPropietario;
GO

/* ============================================================
   6. Administración de espacios

   Solo la plataforma da de alta, edita y retira espacios: es
   fnEsAdministrador (alcance plataforma) y no la versión por
   espacio. Un cliente no puede crear otros clientes.

   Misma convención de los scripts 09 y 10: una fila con ok,
   mensaje y código de vuelta, y bitácora en cada escritura,
   anotada en el espacio de la plataforma porque es una acción
   sobre la plataforma y no dentro del espacio.
   ============================================================ */

IF OBJECT_ID('dbo.spAdminEspacios') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEspacios;
GO

CREATE PROCEDURE dbo.spAdminEspacios
    @codigoUsuario INT,
    @soloActivos   BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    /* Quien administra la plataforma los ve todos. Quien
       administra un espacio ve solo el suyo, que es lo que le
       hace falta para leer su propia ficha. */
    DECLARE @propio INT = (SELECT codigoEspacio FROM dbo.Usuarios WHERE codigoUsuario = @codigoUsuario);

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0 AND @propio IS NULL RETURN;

    SELECT *
    FROM dbo.vwEspacios v
    WHERE (@propio IS NULL OR v.codigoEspacio = @propio)
      AND (@soloActivos = 0 OR v.activo = 1)
    ORDER BY v.esPlataforma DESC, v.nombre;
END
GO

IF OBJECT_ID('dbo.spAdminGuardarEspacio') IS NOT NULL
    DROP PROCEDURE dbo.spAdminGuardarEspacio;
GO

/* Alta y edición en uno, con @codigoEspacio en cero para el
   alta. El slug se asigna solo al dar de alta, por lo mismo que
   en campañas y partidos: es la dirección pública /e/{slug}. */

CREATE PROCEDURE dbo.spAdminGuardarEspacio
    @codigoUsuario     INT,
    @codigoEspacio     INT,
    @nombre            NVARCHAR(160),
    @organizacion      NVARCHAR(200),
    @descripcion       NVARCHAR(1000) = NULL,
    @padronCerrado     BIT            = 1,
    @terminoAgrupacion NVARCHAR(40)   = N'Partido'
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Solo la administración de la plataforma puede registrar espacios.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @nombre            = LTRIM(RTRIM(ISNULL(@nombre, N'')));
    SET @organizacion      = LTRIM(RTRIM(ISNULL(@organizacion, N'')));
    SET @descripcion       = NULLIF(LTRIM(RTRIM(ISNULL(@descripcion, N''))), N'');
    SET @terminoAgrupacion = LTRIM(RTRIM(ISNULL(@terminoAgrupacion, N'')));

    IF LEN(@nombre) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El nombre del espacio es obligatorio.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF LEN(@organizacion) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Indicá la organización que administra el espacio.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF LEN(@terminoAgrupacion) < 3 SET @terminoAgrupacion = N'Partido';

    IF EXISTS (SELECT 1 FROM dbo.Espacios
                WHERE nombre = @nombre AND codigoEspacio <> @codigoEspacio)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ya existe un espacio con ese nombre.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @slug NVARCHAR(160) = dbo.fnSlug(@nombre);

    IF @codigoEspacio = 0 AND EXISTS (SELECT 1 FROM dbo.Espacios WHERE slug = @slug)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Ese nombre produce una dirección que ya está en uso.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    DECLARE @plataforma INT = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1);
    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Espacio');

    IF @codigoEspacio > 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Espacios WHERE codigoEspacio = @codigoEspacio)
        BEGIN
            SELECT CAST(0 AS BIT) AS ok, N'No se encontró el espacio.' AS mensaje, 0 AS codigo;
            RETURN;
        END

        /* La plataforma no tiene padrón: participa cualquiera con
           correo confirmado. El interruptor no se le puede cambiar. */
        IF @codigoEspacio = @plataforma SET @padronCerrado = 0;

        UPDATE dbo.Espacios
           SET nombre            = @nombre,
               organizacion      = @organizacion,
               descripcion       = @descripcion,
               padronCerrado     = @padronCerrado,
               terminoAgrupacion = @terminoAgrupacion
         WHERE codigoEspacio = @codigoEspacio;

        INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
        VALUES (@plataforma, @codigoUsuario, N'Edicion', @codigoTipoObjeto, @codigoEspacio,
                N'Espacio editado: ' + @nombre, NULL);

        SELECT CAST(1 AS BIT) AS ok, N'El espacio quedó actualizado.' AS mensaje, @codigoEspacio AS codigo;
        RETURN;
    END

    INSERT INTO dbo.Espacios (slug, nombre, organizacion, descripcion, esPlataforma, padronCerrado, terminoAgrupacion)
    VALUES (@slug, @nombre, @organizacion, @descripcion, 0, @padronCerrado, @terminoAgrupacion);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@plataforma, @codigoUsuario, N'Alta', @codigoTipoObjeto, @nuevo,
            N'Espacio registrado: ' + @nombre + N' (' + @organizacion + N')', NULL);

    SELECT CAST(1 AS BIT) AS ok, N'El espacio quedó registrado.' AS mensaje, @nuevo AS codigo;
END
GO

IF OBJECT_ID('dbo.spAdminEstadoEspacio') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEstadoEspacio;
GO

/* Retirar o restaurar. Al retirar, las cuentas que administran
   el espacio se desactivan con él, por lo mismo que la cuenta de
   una candidatura sigue a la candidatura: un espacio retirado
   que todavía puede administrarse sería una puerta abierta sin
   nada detrás. Al restaurar vuelven. */

CREATE PROCEDURE dbo.spAdminEstadoEspacio
    @codigoUsuario INT,
    @codigoEspacio INT,
    @activo        BIT,
    @motivo        NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;

    SET @motivo = NULLIF(LTRIM(RTRIM(ISNULL(@motivo, N''))), N'');

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Solo la administración de la plataforma puede retirar espacios.' AS mensaje;
        RETURN;
    END

    IF @motivo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Hay que registrar el motivo de la decisión.' AS mensaje;
        RETURN;
    END

    DECLARE @nombre NVARCHAR(160), @actual BIT, @esPlataforma BIT;
    SELECT @nombre = nombre, @actual = activo, @esPlataforma = esPlataforma
      FROM dbo.Espacios WHERE codigoEspacio = @codigoEspacio;

    IF @nombre IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró el espacio.' AS mensaje;
        RETURN;
    END

    IF @esPlataforma = 1
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El espacio de la plataforma no se puede retirar.' AS mensaje;
        RETURN;
    END

    IF @actual = @activo
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               CASE WHEN @activo = 1 THEN N'El espacio ya está activo.'
                    ELSE N'El espacio ya estaba retirado.' END AS mensaje;
        RETURN;
    END

    IF @activo = 0
        UPDATE dbo.Espacios
           SET activo = 0, motivoBaja = @motivo,
               fechaBaja = SYSDATETIME(), codigoUsuarioBaja = @codigoUsuario
         WHERE codigoEspacio = @codigoEspacio;
    ELSE
        UPDATE dbo.Espacios
           SET activo = 1, motivoBaja = NULL, fechaBaja = NULL, codigoUsuarioBaja = NULL
         WHERE codigoEspacio = @codigoEspacio;

    UPDATE dbo.Usuarios SET activo = @activo WHERE codigoEspacio = @codigoEspacio;

    DECLARE @plataforma INT = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1);
    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Espacio');

    INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@plataforma, @codigoUsuario,
            CASE WHEN @activo = 0 THEN N'Retiro' ELSE N'Restauracion' END,
            @codigoTipoObjeto, @codigoEspacio,
            CASE WHEN @activo = 0 THEN N'Espacio retirado: ' ELSE N'Espacio restaurado: ' END + @nombre,
            @motivo);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @activo = 0
                THEN N'El espacio quedó retirado y sus cuentas de administración, desactivadas.'
                ELSE N'El espacio volvió a estar activo.' END AS mensaje;
END
GO

IF OBJECT_ID('dbo.spAdminCrearCuentaEspacio') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCrearCuentaEspacio;
GO

/* La cuenta con la que el cliente administra su espacio: rol
   Administrador con codigoEspacio fijado. La primera que se crea
   queda como propietaria del espacio.

   Recibe el hash y no la contraseña. Es la diferencia con
   spAdminCrearCuentaCandidato, que cifra con HASHBYTES sobre un
   CONVERT a VARCHAR y produce un hash distinto del backend en
   cuanto la contraseña lleva una tilde o una eñe (ver el script
   16). Acá cifra el backend, que es el mismo que después valida,
   y el procedimiento solo comprueba que lo recibido tenga forma
   de SHA-256. Nace confirmada, como las cuentas de candidatura:
   la crea la administración y entrega la contraseña en mano. */

CREATE PROCEDURE dbo.spAdminCrearCuentaEspacio
    @codigoUsuario INT,
    @codigoEspacio INT,
    @login         NVARCHAR(60),
    @nombre        NVARCHAR(160),
    @correo        NVARCHAR(160),
    @claveHash     CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Solo la administración de la plataforma puede crear cuentas de espacio.' AS mensaje;
        RETURN;
    END

    SET @login  = LOWER(LTRIM(RTRIM(ISNULL(@login, N''))));
    SET @nombre = LTRIM(RTRIM(ISNULL(@nombre, N'')));
    SET @correo = LOWER(LTRIM(RTRIM(ISNULL(@correo, N''))));

    DECLARE @espacioNombre NVARCHAR(160), @esPlataforma BIT, @activo BIT;
    SELECT @espacioNombre = nombre, @esPlataforma = esPlataforma, @activo = activo
      FROM dbo.Espacios WHERE codigoEspacio = @codigoEspacio;

    IF @espacioNombre IS NULL OR @esPlataforma = 1 OR @activo = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El espacio no existe, está retirado o es la plataforma.' AS mensaje;
        RETURN;
    END

    IF LEN(@login) < 4
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El usuario debe tener al menos cuatro caracteres.' AS mensaje;
        RETURN;
    END

    IF LEN(@nombre) < 3
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El nombre de quien administra es obligatorio.' AS mensaje;
        RETURN;
    END

    IF @correo NOT LIKE N'%_@_%._%'
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'El correo no tiene un formato válido.' AS mensaje;
        RETURN;
    END

    IF @claveHash IS NULL OR LEN(@claveHash) <> 64 OR @claveHash LIKE N'%[^0-9a-f]%'
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La contraseña no llegó cifrada como se esperaba.' AS mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE login = @login)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ese usuario ya está ocupado.' AS mensaje;
        RETURN;
    END

    DECLARE @correoNormalizado NVARCHAR(160) = dbo.fnCorreoNormalizado(@correo);

    IF EXISTS (SELECT 1 FROM dbo.Usuarios
                WHERE correo = @correo OR correoNormalizado = @correoNormalizado)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'Ese correo ya está registrado.' AS mensaje;
        RETURN;
    END

    DECLARE @rolAdmin INT = (SELECT codigoRol FROM dbo.Roles WHERE nombre = N'Administrador');

    INSERT INTO dbo.Usuarios
        (login, clave, nombre, correo, correoNormalizado, correoConfirmado,
         codigoRol, codigoCandidato, codigoEspacio, activo)
    VALUES
        (@login, @claveHash, @nombre, @correo, @correoNormalizado, 1,
         @rolAdmin, NULL, @codigoEspacio, 1);

    DECLARE @nuevo INT = SCOPE_IDENTITY();

    /* La primera cuenta del espacio es la propietaria. */
    UPDATE dbo.Espacios
       SET codigoUsuarioPropietario = @nuevo
     WHERE codigoEspacio = @codigoEspacio AND codigoUsuarioPropietario IS NULL;

    DECLARE @plataforma INT = (SELECT codigoEspacio FROM dbo.Espacios WHERE esPlataforma = 1);
    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Usuario');

    /* La contraseña no aparece por ninguna parte, ni siquiera acá. */
    INSERT INTO dbo.Auditoria (codigoEspacio, codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@plataforma, @codigoUsuario, N'Cuenta', @codigoTipoObjeto, @nuevo,
            N'Cuenta de administración creada para el espacio ' + @espacioNombre + N' (' + @login + N')', NULL);

    SELECT CAST(1 AS BIT) AS ok,
           N'La cuenta quedó creada. Entregale la contraseña al cliente por un medio seguro.' AS mensaje;
END
GO

/* ============================================================
   7. Lo que el asistente no puede ver

   El 13 ya niega Espacios. La vista se niega acá porque se crea
   acá. No se concede EXECUTE sobre ningún procedimiento de este
   script.
   ============================================================ */

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'cumplehn_ia')
BEGIN
    DENY SELECT ON dbo.vwEspacios TO cumplehn_ia;
END
GO

PRINT 'Script 19 aplicado: espacios, migracion al espacio de la plataforma y administracion de espacios.';
GO
