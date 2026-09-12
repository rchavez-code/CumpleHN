/* ============================================================
   CumpleHN — 18. Iniciativas ciudadanas
   ------------------------------------------------------------
   Hasta acá la ciudadanía solo podía reaccionar: valorar,
   comentar y responder encuestas. Este script le da la otra
   mitad del módulo de participación que describe el Avance#3:
   proponer. Una iniciativa es una propuesta de proyecto escrita
   por una cuenta con rol Ciudadano, que el resto valora y
   comenta con el mismo par polimórfico del script 05.

   Se llama Iniciativa y no Propuesta a propósito. Propuestas ya
   es la promesa de campaña de una candidatura, con su estado de
   cumplimiento y su nivel de verificación, y una iniciativa no
   es nada de eso: no la prometió nadie y no hay nada que
   verificar. Mezclarlas en una tabla habría contaminado el
   seguimiento de cumplimiento del capítulo IX.

   Decisiones que sostienen todo lo de acá:

     1. Los contadores no se guardan. Me gusta, no me gusta y
        comentarios se derivan de Valoraciones y Comentarios,
        igual que en Publicaciones. La popularidad es el saldo
        (meGusta − noMeGusta) y se calcula en un solo lugar,
        la vista vwIniciativas: la portada y la administración
        leen la misma cifra.

     2. Solo el rol Ciudadano propone. Una candidatura o la
        administración escribiendo «como ciudadano» sería una
        parte interesada dentro de una plataforma que se declara
        neutral — el mismo argumento por el que las encuestas
        las redacta solo la administración. Lo exige el
        procedimiento, no el formulario.

     3. Se edita solo mientras nadie reaccionó. Con un voto o un
        comentario encima, cambiar el texto dejaría reacciones
        apuntando a algo que nadie leyó. Es la regla de las
        opciones de encuesta del script 14, aplicada al texto.

     4. Nada se borra. Quien la propuso puede retirarla cuando
        quiera y la administración puede retirarla o restaurarla
        con motivo, siempre por baja lógica con las mismas cuatro
        columnas de Publicaciones. Un DELETE se llevaría las
        valoraciones y los comentarios.

     5. No pertenece a una campaña. Es una demanda ciudadana, no
        un compromiso de un ciclo electoral: atarla a la campaña
        destacada la haría desaparecer al cerrarse la campaña.
        Lleva categoría (obligatoria) y departamento (opcional),
        que son las dimensiones por las que el tablero la cruza.

   Las valoraciones y los comentarios sobre iniciativas entran al
   tablero por la quinta rama de vwAnaliticaValoraciones y
   vwAnaliticaComentarios, que vive en el script 07 junto a las
   otras cuatro. Como esas vistas apuntan a esta tabla, en una
   base nueva el script 07 se ejecuta después de este.

   Convención de los procedimientos de escritura, igual que en
   los scripts 09, 10 y 14: devuelven una fila con ok (BIT) y
   mensaje (NVARCHAR), y el Web Service la transporta tal cual.

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

IF OBJECT_ID('dbo.Iniciativas') IS NULL
CREATE TABLE dbo.Iniciativas
(
    codigoIniciativa   INT            NOT NULL IDENTITY(1,1),
    /* Quien la propuso. Es la única referencia a la persona y no
       se proyecta hacia el público: la vista muestra el nombre,
       nunca el login ni el correo. */
    codigoUsuario      INT            NOT NULL,
    titulo             NVARCHAR(120)  NOT NULL,
    descripcion        NVARCHAR(1500) NOT NULL,
    codigoCategoria    INT            NOT NULL,
    codigoDepartamento INT            NULL,
    activo             BIT            NOT NULL CONSTRAINT DF_Iniciativas_activo DEFAULT (1),
    motivoBaja         NVARCHAR(300)  NULL,
    fechaBaja          DATETIME2(0)   NULL,
    codigoUsuarioBaja  INT            NULL,
    fechaRegistro      DATETIME2(0)   NOT NULL CONSTRAINT DF_Iniciativas_fecha DEFAULT (SYSDATETIME()),
    fechaEdicion       DATETIME2(0)   NULL,
    CONSTRAINT PK_Iniciativas PRIMARY KEY (codigoIniciativa),
    CONSTRAINT FK_Iniciativas_Usuarios
        FOREIGN KEY (codigoUsuario) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT FK_Iniciativas_Categorias
        FOREIGN KEY (codigoCategoria) REFERENCES dbo.Categorias (codigoCategoria),
    CONSTRAINT FK_Iniciativas_Departamentos
        FOREIGN KEY (codigoDepartamento) REFERENCES dbo.Departamentos (codigoDepartamento),
    CONSTRAINT FK_Iniciativas_UsuarioBaja
        FOREIGN KEY (codigoUsuarioBaja) REFERENCES dbo.Usuarios (codigoUsuario),
    /* Los mínimos viven acá y no solo en el procedimiento: una
       fila que entre por otro camino queda igual de sujeta. */
    CONSTRAINT CK_Iniciativas_titulo      CHECK (LEN(LTRIM(RTRIM(titulo))) >= 8),
    CONSTRAINT CK_Iniciativas_descripcion CHECK (LEN(LTRIM(RTRIM(descripcion))) >= 20)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Iniciativas_activo')
CREATE INDEX IX_Iniciativas_activo
    ON dbo.Iniciativas (activo, fechaRegistro DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Iniciativas_usuario')
CREATE INDEX IX_Iniciativas_usuario
    ON dbo.Iniciativas (codigoUsuario, fechaRegistro DESC);
GO

/* ============================================================
   2. Catálogos que suma este módulo
   ============================================================ */

/* Registrarla como tipo de objeto es lo que la deja recibir
   valoraciones y comentarios: el par (codigoTipoObjeto,
   codigoObjeto) del script 05 no necesita nada más. */

MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Iniciativa', N'dbo.Iniciativas', N'Propuesta de proyecto escrita por una persona con cuenta ciudadana.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

/* El interruptor del módulo. Al apagarlo la sección desaparece
   de la portada para el público y el Web Service deja de aceptar
   iniciativas nuevas — las dos cosas, porque cerrar solo la
   página se saltaría llamando al servicio directamente. Lo ya
   escrito se conserva y quien administra lo sigue viendo. */

MERGE dbo.Modulos AS destino
USING (VALUES
    (N'iniciativas', N'Iniciativas ciudadanas',
     N'Las propuestas de proyecto que escribe la ciudadanía, en la portada y en «Mi cuenta». Al apagarlo se conserva lo escrito y deja de admitirse iniciativas nuevas.',
     N'Módulos del sitio', NULL, 46)
) AS origen (clave, nombre, descripcion, grupo, clavePadre, orden)
    ON destino.clave = origen.clave
WHEN NOT MATCHED THEN
    INSERT (clave, nombre, descripcion, grupo, clavePadre, orden, habilitado)
    VALUES (origen.clave, origen.nombre, origen.descripcion, origen.grupo,
            origen.clavePadre, origen.orden, 1)
WHEN MATCHED THEN
    UPDATE SET destino.nombre = origen.nombre,
               destino.descripcion = origen.descripcion,
               destino.grupo = origen.grupo,
               destino.orden = origen.orden;
GO

/* ============================================================
   3. Vista

   Una fila por iniciativa con sus dimensiones y sus contadores
   derivados. Es el único lugar donde se calcula el saldo, así que
   la portada, «Mi cuenta» y la administración ordenan por la
   misma cifra.

   Proyecta codigoUsuario porque «Mi cuenta» y las comprobaciones
   de autoría lo necesitan. Por eso mismo lleva DENY para el
   asistente, como las otras vistas que vinculan a una persona
   con lo que hizo. Del usuario solo se expone el nombre: ni el
   login ni el correo tienen por qué salir de Usuarios.

   CREATE OR ALTER en lugar de DROP + CREATE: alterar conserva
   los permisos del objeto, y borrarlo se llevaría el DENY que se
   aplica al final de este mismo script la próxima vez que se
   ejecute.
   ============================================================ */

CREATE OR ALTER VIEW dbo.vwIniciativas
AS
SELECT
    i.codigoIniciativa,
    i.codigoUsuario,
    u.nombre                        AS autora,
    i.titulo,
    i.descripcion,
    i.codigoCategoria,
    c.nombre                        AS categoria,
    ISNULL(i.codigoDepartamento, 0) AS codigoDepartamento,
    ISNULL(d.nombre, N'')           AS departamento,
    (SELECT COUNT(*) FROM dbo.Valoraciones v
      INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto
      WHERE t.nombre = N'Iniciativa' AND v.codigoObjeto = i.codigoIniciativa AND v.valor = 1)  AS meGusta,
    (SELECT COUNT(*) FROM dbo.Valoraciones v
      INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto
      WHERE t.nombre = N'Iniciativa' AND v.codigoObjeto = i.codigoIniciativa AND v.valor = -1) AS noMeGusta,
    (SELECT COUNT(*) FROM dbo.Comentarios cm
      INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = cm.codigoTipoObjeto
      WHERE t.nombre = N'Iniciativa' AND cm.codigoObjeto = i.codigoIniciativa AND cm.aprobado = 1) AS comentarios,
    (SELECT ISNULL(SUM(v.valor), 0) FROM dbo.Valoraciones v
      INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto
      WHERE t.nombre = N'Iniciativa' AND v.codigoObjeto = i.codigoIniciativa) AS saldo,
    i.activo,
    ISNULL(i.motivoBaja, N'')       AS motivoBaja,
    i.fechaRegistro,
    i.fechaEdicion
FROM dbo.Iniciativas i
INNER JOIN dbo.Usuarios u        ON u.codigoUsuario      = i.codigoUsuario
INNER JOIN dbo.Categorias c      ON c.codigoCategoria    = i.codigoCategoria
LEFT  JOIN dbo.Departamentos d   ON d.codigoDepartamento = i.codigoDepartamento;
GO

/* ============================================================
   4. Consulta pública
   ============================================================ */

/* Las iniciativas de la portada: todas las activas, de la más
   popular a la menos. El desempate es la más reciente primero,
   para que entre dos con el mismo saldo la nueva no quede
   enterrada bajo la vieja.

   Devuelve todas. Cuántas se ven a la vez lo decide la portada,
   que muestra cuatro y despliega de cuatro en cuatro: es una
   decisión de presentación y puede cambiar sin tocar la base.

   El usuario sirve para dos cosas: marcar su voto en cada
   tarjeta y saber cuáles son suyas. Con cero es un visitante. */

CREATE OR ALTER PROCEDURE dbo.spIniciativasPublicas
    @codigoUsuario INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Iniciativa');

    SELECT
        i.codigoIniciativa,
        i.autora,
        i.titulo,
        i.descripcion,
        i.codigoCategoria,
        i.categoria,
        i.codigoDepartamento,
        i.departamento,
        i.meGusta,
        i.noMeGusta,
        i.comentarios,
        i.saldo,
        ISNULL((SELECT v.valor FROM dbo.Valoraciones v
                 WHERE v.codigoTipoObjeto = @tipo
                   AND v.codigoObjeto     = i.codigoIniciativa
                   AND v.codigoUsuario    = @codigoUsuario), 0) AS miValoracion,
        CAST(CASE WHEN i.codigoUsuario = @codigoUsuario THEN 1 ELSE 0 END AS BIT) AS esMia,
        i.activo,
        i.motivoBaja,
        CAST(0 AS BIT) AS puedeEditar,
        i.fechaRegistro,
        i.fechaEdicion
    FROM dbo.vwIniciativas i
    WHERE i.activo = 1
    ORDER BY i.saldo DESC, i.fechaRegistro DESC, i.codigoIniciativa DESC;
END
GO

/* Las iniciativas de una persona para «Mi cuenta», activas y
   retiradas, de la más reciente a la más antigua.

   puedeEditar dice si el texto todavía se puede cambiar: solo
   mientras nadie haya valorado ni comentado. Se calcula acá para
   que la pantalla y el procedimiento de guardado no puedan
   discrepar en qué significa «sin reacciones».

   Devuelve las mismas columnas que spIniciativasPublicas, para
   que el Web Service las lea con el mismo lector. */

CREATE OR ALTER PROCEDURE dbo.spIniciativasDeUsuario
    @codigoUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Iniciativa');

    SELECT
        i.codigoIniciativa,
        i.autora,
        i.titulo,
        i.descripcion,
        i.codigoCategoria,
        i.categoria,
        i.codigoDepartamento,
        i.departamento,
        i.meGusta,
        i.noMeGusta,
        i.comentarios,
        i.saldo,
        ISNULL((SELECT v.valor FROM dbo.Valoraciones v
                 WHERE v.codigoTipoObjeto = @tipo
                   AND v.codigoObjeto     = i.codigoIniciativa
                   AND v.codigoUsuario    = @codigoUsuario), 0) AS miValoracion,
        CAST(1 AS BIT) AS esMia,
        i.activo,
        i.motivoBaja,
        CAST(CASE WHEN i.activo = 1 AND i.meGusta + i.noMeGusta + i.comentarios = 0
                  THEN 1 ELSE 0 END AS BIT) AS puedeEditar,
        i.fechaRegistro,
        i.fechaEdicion
    FROM dbo.vwIniciativas i
    WHERE i.codigoUsuario = @codigoUsuario
    ORDER BY i.fechaRegistro DESC, i.codigoIniciativa DESC;
END
GO

/* Alta y edición.

   Tres reglas que hace cumplir el procedimiento y no la pantalla,
   porque una validación que solo vive en el formulario se salta
   llamando al servicio:

     · Solo una cuenta activa con rol Ciudadano. La confirmación
       del correo la comprueba el Web Service con la misma puerta
       de votar y comentar (MotivoSinParticipacion).

     · En edición, la iniciativa tiene que ser de quien llama.

     · En edición, no puede tener reacciones. Con un voto o un
       comentario encima el texto queda fijo: lo que se puede
       hacer es retirarla y escribir otra.

   @codigoDepartamento en cero es «sin departamento», que se
   guarda como NULL: una iniciativa de alcance nacional no es un
   dato faltante. */

CREATE OR ALTER PROCEDURE dbo.spIniciativaGuardar
    @codigoUsuario      INT,
    @codigoIniciativa   INT           = 0,
    @titulo             NVARCHAR(120),
    @descripcion        NVARCHAR(1500),
    @codigoCategoria    INT,
    @codigoDepartamento INT           = 0
AS
BEGIN
    SET NOCOUNT ON;

    SET @titulo      = LTRIM(RTRIM(ISNULL(@titulo, N'')));
    SET @descripcion = LTRIM(RTRIM(ISNULL(@descripcion, N'')));

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios u
                   INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol
                   WHERE u.codigoUsuario = @codigoUsuario
                     AND u.activo = 1
                     AND r.nombre = N'Ciudadano')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Las iniciativas las proponen las cuentas ciudadanas.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    IF LEN(@titulo) < 8
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Escribí un título de al menos ocho caracteres.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF LEN(@descripcion) < 20
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Contá la iniciativa con un poco más de detalle: al menos veinte caracteres.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE codigoCategoria = @codigoCategoria)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Elegí la categoría a la que pertenece la iniciativa.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    SET @codigoDepartamento = NULLIF(ISNULL(@codigoDepartamento, 0), 0);

    IF @codigoDepartamento IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Departamentos WHERE codigoDepartamento = @codigoDepartamento)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El departamento elegido no existe.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* -------------------------------------------------- Alta */

    IF ISNULL(@codigoIniciativa, 0) = 0
    BEGIN
        INSERT INTO dbo.Iniciativas
            (codigoUsuario, titulo, descripcion, codigoCategoria, codigoDepartamento)
        VALUES
            (@codigoUsuario, @titulo, @descripcion, @codigoCategoria, @codigoDepartamento);

        SELECT CAST(1 AS BIT) AS ok,
               N'Tu iniciativa quedó publicada.' AS mensaje,
               CAST(SCOPE_IDENTITY() AS INT) AS codigo;
        RETURN;
    END

    /* ----------------------------------------------- Edición */

    DECLARE @duena INT, @activa BIT;

    SELECT @duena = codigoUsuario, @activa = activo
    FROM dbo.Iniciativas
    WHERE codigoIniciativa = @codigoIniciativa;

    IF @duena IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La iniciativa no existe.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @duena <> @codigoUsuario
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Solo quien propuso la iniciativa puede editarla.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @activa = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Una iniciativa retirada no se edita.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.vwIniciativas
               WHERE codigoIniciativa = @codigoIniciativa
                 AND meGusta + noMeGusta + comentarios > 0)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Esta iniciativa ya recibió reacciones y su texto queda fijo. Si querés cambiarla, retirala y proponé otra.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    UPDATE dbo.Iniciativas
       SET titulo             = @titulo,
           descripcion        = @descripcion,
           codigoCategoria    = @codigoCategoria,
           codigoDepartamento = @codigoDepartamento,
           fechaEdicion       = SYSDATETIME()
     WHERE codigoIniciativa = @codigoIniciativa;

    SELECT CAST(1 AS BIT) AS ok, N'Iniciativa actualizada.' AS mensaje, @codigoIniciativa AS codigo;
END
GO

/* Retiro por quien la propuso.

   Baja lógica con las mismas columnas que usa la moderación, de
   modo que la administración vea quién la retiró y cuándo sin
   otra tabla. No escribe en Auditoria: esa bitácora registra lo
   que hace quien administra, y retirar lo propio no es una
   decisión de administración. */

CREATE OR ALTER PROCEDURE dbo.spIniciativaRetirarPropia
    @codigoUsuario    INT,
    @codigoIniciativa INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @duena INT, @activa BIT;

    SELECT @duena = codigoUsuario, @activa = activo
    FROM dbo.Iniciativas
    WHERE codigoIniciativa = @codigoIniciativa;

    IF @duena IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La iniciativa no existe.' AS mensaje;
        RETURN;
    END

    IF @duena <> @codigoUsuario
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Solo quien propuso la iniciativa puede retirarla.' AS mensaje;
        RETURN;
    END

    IF @activa = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La iniciativa ya estaba retirada.' AS mensaje;
        RETURN;
    END

    UPDATE dbo.Iniciativas
       SET activo            = 0,
           motivoBaja        = N'Retirada por quien la propuso',
           fechaBaja         = SYSDATETIME(),
           codigoUsuarioBaja = @codigoUsuario
     WHERE codigoIniciativa = @codigoIniciativa;

    SELECT CAST(1 AS BIT) AS ok,
           N'Iniciativa retirada. Las reacciones que recibió se conservan.' AS mensaje;
END
GO

/* ============================================================
   5. Administración

   Mismas reglas del script 09: fnEsAdministrador dentro de cada
   escritura aunque el Web Service ya lo haya comprobado, motivo
   obligatorio, y una fila en la bitácora por cada acción.
   ============================================================ */

/* Listado para moderación. Incluye las retiradas, que es lo que
   el público no ve y quien administra necesita ver.

   @estado: 'Activas', 'Retiradas' o vacío para todas, igual que
   spAdminPublicaciones. */

CREATE OR ALTER PROCEDURE dbo.spAdminIniciativas
    @codigoUsuario INT,
    @estado        NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0 RETURN;

    IF @estado = N'' SET @estado = NULL;

    SELECT
        i.codigoIniciativa,
        i.autora,
        i.titulo,
        i.descripcion,
        i.codigoCategoria,
        i.categoria,
        i.codigoDepartamento,
        i.departamento,
        i.meGusta,
        i.noMeGusta,
        i.comentarios,
        i.saldo,
        0              AS miValoracion,
        CAST(0 AS BIT) AS esMia,
        i.activo,
        i.motivoBaja,
        CAST(0 AS BIT) AS puedeEditar,
        i.fechaRegistro,
        i.fechaEdicion
    FROM dbo.vwIniciativas i
    WHERE @estado IS NULL
       OR (@estado = N'Activas'   AND i.activo = 1)
       OR (@estado = N'Retiradas' AND i.activo = 0)
    ORDER BY i.fechaRegistro DESC, i.codigoIniciativa DESC;
END
GO

/* Retirar o restaurar. Un solo procedimiento para las dos
   direcciones, calcado de spAdminModerarPublicacion: son la
   misma decisión y comparten toda la comprobación. */

CREATE OR ALTER PROCEDURE dbo.spAdminModerarIniciativa
    @codigoUsuario    INT,
    @codigoIniciativa INT,
    @activo           BIT,
    @motivo           NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;

    SET @motivo = NULLIF(LTRIM(RTRIM(ISNULL(@motivo, N''))), N'');

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no tiene permiso para moderar iniciativas.' AS mensaje;
        RETURN;
    END

    IF @motivo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Hay que registrar el motivo de la decisión.' AS mensaje;
        RETURN;
    END

    DECLARE @actual BIT =
        (SELECT activo FROM dbo.Iniciativas WHERE codigoIniciativa = @codigoIniciativa);

    IF @actual IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró la iniciativa.' AS mensaje;
        RETURN;
    END

    IF @actual = @activo
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               CASE WHEN @activo = 1
                    THEN N'La iniciativa ya está visible.'
                    ELSE N'La iniciativa ya estaba retirada.' END AS mensaje;
        RETURN;
    END

    IF @activo = 0
        UPDATE dbo.Iniciativas
           SET activo            = 0,
               motivoBaja        = @motivo,
               fechaBaja         = SYSDATETIME(),
               codigoUsuarioBaja = @codigoUsuario
         WHERE codigoIniciativa = @codigoIniciativa;
    ELSE
        /* Al restaurar se limpian los datos del retiro anterior.
           El rastro queda en la bitácora. */
        UPDATE dbo.Iniciativas
           SET activo            = 1,
               motivoBaja        = NULL,
               fechaBaja         = NULL,
               codigoUsuarioBaja = NULL
         WHERE codigoIniciativa = @codigoIniciativa;

    DECLARE @tipo INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Iniciativa');

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario,
            CASE WHEN @activo = 0 THEN N'Retiro' ELSE N'Restauracion' END,
            @tipo, @codigoIniciativa,
            CASE WHEN @activo = 0
                 THEN N'Iniciativa retirada de la consulta pública'
                 ELSE N'Iniciativa restaurada' END,
            @motivo);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @activo = 0
                THEN N'Iniciativa retirada. Las reacciones que recibió se conservan.'
                ELSE N'Iniciativa restaurada.' END AS mensaje;
END
GO

/* ============================================================
   6. Alcance del asistente

   El login cumplehn_ia no pertenece a db_datareader ni tiene
   permiso sobre ninguna tabla, así que la tabla y la vista ya le
   quedaban fuera. El DENY se escribe igual, por la misma razón
   que los del script 13: la vista vincula a una persona con lo
   que propuso, y un DENY sobrevive a que alguien conceda un
   permiso amplio por descuido.

   No se concede EXECUTE sobre ningún procedimiento de acá. Si el
   asistente responde algún día sobre iniciativas será con un
   procedimiento propio que devuelva solo agregados por categoría
   y departamento, nunca quién propuso qué.
   ============================================================ */

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'cumplehn_ia')
BEGIN
    DENY SELECT ON dbo.Iniciativas   TO cumplehn_ia;
    DENY SELECT ON dbo.vwIniciativas TO cumplehn_ia;
END
GO

PRINT 'Script 18 aplicado: iniciativas ciudadanas.';
GO
