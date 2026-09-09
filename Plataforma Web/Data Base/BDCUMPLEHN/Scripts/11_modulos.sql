/* ============================================================
   CumpleHN — 11. Interruptores de módulos
   ------------------------------------------------------------
   Permite que la administración oculte del sitio público un
   módulo entero o un gráfico concreto del tablero, sin tocar
   código y sin borrar nada.

   Qué significa deshabilitar: el elemento desaparece para quien
   consulta el sitio, y sigue visible para el rol Administrador
   con un aviso de que está oculto. Así se puede revisar antes de
   publicar, o retirar algo que no está listo sin perder la
   capacidad de verlo.

   Un solo mecanismo para dos niveles de detalle. Cada fila es
   algo apagable, y clavePadre expresa la jerarquía: si un padre
   está apagado, sus hijos quedan apagados aunque tengan su
   propio interruptor encendido. Apagar el tablero completo no
   debería obligar a apagar sus doce gráficos uno por uno.

   Las claves no son texto libre: el código las busca por su
   valor exacto. Agregar una fila acá no habilita nada por sí
   sola — el elemento tiene que consultar su clave.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* Igual que en los scripts 09 y 10: sqlcmd trae QUOTED_IDENTIFIER
   apagado y un procedimiento conserva para siempre el valor que
   tenía al crearse. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Catálogo de elementos apagables
   ============================================================ */

IF OBJECT_ID('dbo.Modulos') IS NULL
CREATE TABLE dbo.Modulos
(
    codigoModulo        INT           NOT NULL IDENTITY(1,1),
    clave               NVARCHAR(60)  NOT NULL,
    nombre              NVARCHAR(120) NOT NULL,
    descripcion         NVARCHAR(400) NULL,
    /* Agrupa la lista en la pantalla de administración. */
    grupo               NVARCHAR(60)  NOT NULL,
    /* NULL en los elementos de primer nivel. */
    clavePadre          NVARCHAR(60)  NULL,
    habilitado          BIT           NOT NULL CONSTRAINT DF_Modulos_habilitado DEFAULT (1),
    orden               INT           NOT NULL CONSTRAINT DF_Modulos_orden DEFAULT (0),
    fechaCambio         DATETIME2(0)  NULL,
    codigoUsuarioCambio INT           NULL,
    CONSTRAINT PK_Modulos PRIMARY KEY (codigoModulo),
    CONSTRAINT UQ_Modulos_clave UNIQUE (clave),
    CONSTRAINT FK_Modulos_Usuarios
        FOREIGN KEY (codigoUsuarioCambio) REFERENCES dbo.Usuarios (codigoUsuario),
    /* La jerarquía es de un solo nivel a propósito: un árbol más
       profundo complicaría la resolución sin ganar nada acá. */
    CONSTRAINT FK_Modulos_Padre
        FOREIGN KEY (clavePadre) REFERENCES dbo.Modulos (clave)
);
GO

/* La bitácora suma la acción de encender y apagar. */

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auditoria_accion')
    ALTER TABLE dbo.Auditoria DROP CONSTRAINT CK_Auditoria_accion;
GO

ALTER TABLE dbo.Auditoria
    ADD CONSTRAINT CK_Auditoria_accion
        CHECK (accion IN ('Verificacion', 'Retiro', 'Restauracion',
                          'Alta', 'Edicion', 'Baja', 'Cuenta', 'Modulo'));
GO

MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Modulo', N'dbo.Modulos', N'Módulo o gráfico que se puede ocultar del sitio público.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

/* ============================================================
   2. Elementos registrados

   Solo se registran elementos que el código consulta de verdad.
   Un interruptor que no apaga nada es peor que no tenerlo: en la
   pantalla se ve igual que los que sí funcionan.

   Por eso no hay interruptor para «consulta ciudadana con
   filtros»: no es una página sino una capacidad repartida por
   todo el sitio, y apagarla no tendría un efecto verificable.
   ============================================================ */

/* Los MERGE dejan intacto el estado habilitado de lo que ya
   existe. Volver a ejecutar el script no vuelve a encender lo
   que la administración apagó. */

MERGE dbo.Modulos AS destino
USING (VALUES
    /* ------------------------------------------- Módulos */
    (N'campanas',    N'Campañas electorales',
     N'Listado y ficha pública de cada campaña.',
     N'Módulos del sitio', NULL, 10),

    (N'perfiles',    N'Candidaturas y partidos',
     N'Listados y fichas públicas de candidaturas y de partidos políticos.',
     N'Módulos del sitio', NULL, 20),

    (N'propuestas',  N'Propuestas y seguimiento',
     N'Listado y ficha de las propuestas, con su categoría y su estado de cumplimiento.',
     N'Módulos del sitio', NULL, 30),

    (N'interaccion', N'Participación ciudadana',
     N'Valoraciones y comentarios. Al apagarla, lo ya registrado sigue visible y deja de admitirse participación nueva.',
     N'Módulos del sitio', NULL, 40),

    (N'analitica',   N'Tablero de analítica',
     N'El tablero completo. Al apagarlo se apagan también sus gráficos.',
     N'Módulos del sitio', NULL, 50),

    /* ------------------------------- Bloques del tablero */
    (N'analitica.kpi',          N'Indicadores principales',
     N'La fila de cifras grandes que encabeza el tablero.',
     N'Tablero de analítica', N'analitica', 100),

    (N'analitica.hallazgos',    N'Principales hallazgos',
     N'Las conclusiones calculadas sobre la selección activa.',
     N'Tablero de analítica', N'analitica', 110),

    (N'analitica.brecha',       N'Prioridad ciudadana frente a oferta',
     N'Compara el interés medido en la encuesta con las propuestas registradas.',
     N'Tablero de analítica', N'analitica', 120),

    (N'analitica.estados',      N'Estado de cumplimiento',
     N'Distribución de las propuestas por estado.',
     N'Tablero de analítica', N'analitica', 130),

    (N'analitica.partidos',     N'Propuestas por partido',
     N'Conteo de propuestas de cada partido.',
     N'Tablero de analítica', N'analitica', 140),

    (N'analitica.densidad',     N'Densidad programática',
     N'Propuestas por candidatura.',
     N'Tablero de analítica', N'analitica', 150),

    (N'analitica.signo',        N'Signo de la participación',
     N'Proporción de apoyo y rechazo sobre el total de valoraciones.',
     N'Tablero de analítica', N'analitica', 160),

    (N'analitica.tipos',        N'Participación por tipo de contenido',
     N'Cómo se reparte la participación entre publicaciones, candidaturas, partidos y propuestas.',
     N'Tablero de analítica', N'analitica', 170),

    (N'analitica.ranking',      N'Ranking de candidaturas',
     N'Ordena las candidaturas por saldo de valoraciones.',
     N'Tablero de analítica', N'analitica', 180),

    (N'analitica.actividad',    N'Actividad por día',
     N'Registros de participación a lo largo del tiempo.',
     N'Tablero de analítica', N'analitica', 190),

    (N'analitica.territorio',   N'Cobertura territorial',
     N'Qué departamentos tienen candidaturas registradas.',
     N'Tablero de analítica', N'analitica', 200),

    (N'analitica.verificacion', N'Verificación por tipo de contenido',
     N'Cuánto del contenido está verificado, en revisión o declarado.',
     N'Tablero de analítica', N'analitica', 210),

    (N'analitica.asistente',    N'Asistente de consulta',
     N'El asistente en lenguaje natural que responde sobre las cifras del tablero.',
     N'Tablero de analítica', N'analitica', 220)
) AS origen (clave, nombre, descripcion, grupo, clavePadre, orden)
    ON destino.clave = origen.clave
WHEN NOT MATCHED THEN
    INSERT (clave, nombre, descripcion, grupo, clavePadre, orden, habilitado)
    VALUES (origen.clave, origen.nombre, origen.descripcion, origen.grupo,
            origen.clavePadre, origen.orden, 1)
/* El texto se actualiza, el interruptor no: cambiar una
   descripción no debe reencender lo que alguien apagó. */
WHEN MATCHED THEN
    UPDATE SET destino.nombre = origen.nombre,
               destino.descripcion = origen.descripcion,
               destino.grupo = origen.grupo,
               destino.clavePadre = origen.clavePadre,
               destino.orden = origen.orden;
GO

/* ============================================================
   3. Estado efectivo

   Un elemento está apagado si lo apagaron a él o si apagaron a
   su padre. La resolución vive acá y no en el frontend: si cada
   página la calculara por su cuenta, bastaría con que una lo
   hiciera distinto para que el tablero se contradijera.
   ============================================================ */

IF OBJECT_ID('dbo.vwModulosEfectivos') IS NOT NULL
    DROP VIEW dbo.vwModulosEfectivos;
GO

CREATE VIEW dbo.vwModulosEfectivos
AS
SELECT
    m.codigoModulo,
    m.clave,
    m.nombre,
    ISNULL(m.descripcion, N'') AS descripcion,
    m.grupo,
    ISNULL(m.clavePadre, N'')  AS clavePadre,
    m.habilitado,
    /* Lo que consulta el sitio. Distinto de habilitado: este ya
       tiene en cuenta al padre. */
    CAST(CASE WHEN m.habilitado = 1 AND ISNULL(p.habilitado, 1) = 1
              THEN 1 ELSE 0 END AS BIT) AS visible,
    /* Para que la pantalla de administración pueda explicar por
       qué algo está apagado sin que su propio interruptor lo esté. */
    CAST(CASE WHEN m.habilitado = 1 AND ISNULL(p.habilitado, 1) = 0
              THEN 1 ELSE 0 END AS BIT) AS apagadoPorPadre,
    m.orden,
    m.fechaCambio,
    ISNULL(u.nombre, N'')      AS cambiadoPor
FROM dbo.Modulos m
LEFT JOIN dbo.Modulos p  ON p.clave = m.clavePadre
LEFT JOIN dbo.Usuarios u ON u.codigoUsuario = m.codigoUsuarioCambio;
GO

/* ============================================================
   4. Consulta pública

   La usa cada página del sitio para saber qué mostrar, así que
   no exige rol: saber que el tablero está apagado no es
   información reservada, y pedir credenciales para consultarlo
   obligaría a autenticar al visitante anónimo.

   Devuelve solo lo indispensable, porque se llama en cada carga.
   ============================================================ */

IF OBJECT_ID('dbo.spModulosVisibles') IS NOT NULL
    DROP PROCEDURE dbo.spModulosVisibles;
GO

CREATE PROCEDURE dbo.spModulosVisibles
AS
BEGIN
    SET NOCOUNT ON;

    SELECT clave, visible
    FROM dbo.vwModulosEfectivos
    ORDER BY orden;
END
GO

/* ============================================================
   5. Administración
   ============================================================ */

IF OBJECT_ID('dbo.spAdminModulos') IS NOT NULL
    DROP PROCEDURE dbo.spAdminModulos;
GO

CREATE PROCEDURE dbo.spAdminModulos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        codigoModulo,
        clave,
        nombre,
        descripcion,
        grupo,
        clavePadre,
        habilitado,
        visible,
        apagadoPorPadre,
        orden,
        fechaCambio,
        cambiadoPor
    FROM dbo.vwModulosEfectivos
    ORDER BY orden;
END
GO

IF OBJECT_ID('dbo.spAdminCambiarModulo') IS NOT NULL
    DROP PROCEDURE dbo.spAdminCambiarModulo;
GO

CREATE PROCEDURE dbo.spAdminCambiarModulo
    @codigoUsuario INT,
    @clave         NVARCHAR(60),
    @habilitado    BIT,
    @motivo        NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @motivo = N'' SET @motivo = NULL;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no tiene permiso para configurar los módulos.' AS mensaje;
        RETURN;
    END

    DECLARE @codigoModulo INT;
    DECLARE @nombre NVARCHAR(120);
    DECLARE @actual BIT;

    SELECT @codigoModulo = codigoModulo, @nombre = nombre, @actual = habilitado
      FROM dbo.Modulos
     WHERE clave = @clave;

    IF @codigoModulo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'No se encontró el módulo indicado.' AS mensaje;
        RETURN;
    END

    IF @actual = @habilitado
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               CASE WHEN @habilitado = 1
                    THEN N'Ese elemento ya está visible.'
                    ELSE N'Ese elemento ya estaba oculto.' END AS mensaje;
        RETURN;
    END

    /* Apagar algo que se consulta requiere decir por qué. Encender
       no: volver al estado normal no necesita justificación. */
    IF @habilitado = 0 AND @motivo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Hay que registrar el motivo para ocultar un elemento.' AS mensaje;
        RETURN;
    END

    UPDATE dbo.Modulos
       SET habilitado = @habilitado,
           fechaCambio = SYSDATETIME(),
           codigoUsuarioCambio = @codigoUsuario
     WHERE codigoModulo = @codigoModulo;

    DECLARE @codigoTipoObjeto INT =
        (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Modulo');

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle, motivo)
    VALUES (@codigoUsuario, N'Modulo', @codigoTipoObjeto, @codigoModulo,
            CASE WHEN @habilitado = 0
                 THEN N'Oculto del sitio público: '
                 ELSE N'Vuelto visible: ' END + @nombre,
            @motivo);

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @habilitado = 0
                THEN @nombre + N' quedó oculto para quien consulta el sitio. La administración lo sigue viendo.'
                ELSE @nombre + N' volvió a estar visible.' END AS mensaje;
END
GO

PRINT 'Script 11 aplicado: interruptores de modulos y graficos.';
GO
