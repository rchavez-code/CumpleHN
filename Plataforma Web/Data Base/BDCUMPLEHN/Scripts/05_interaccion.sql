/* ============================================================
   CumpleHN — 05. Módulo de interacción ciudadana
   ------------------------------------------------------------
   Agrega valoraciones (me gusta / no me gusta) y comentarios
   sobre cuatro tipos de objeto: publicaciones, candidatos,
   partidos políticos y propuestas.

   Incluye la normalización del partido político, que hasta
   ahora era una columna de texto dentro de Candidatos. Sin una
   tabla propia no existe una entidad sobre la cual registrar
   valoraciones ni comentarios.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* ============================================ PARTIDOS POLÍTICOS */

IF OBJECT_ID('dbo.Partidos') IS NULL
CREATE TABLE dbo.Partidos
(
    codigoPartido INT            NOT NULL IDENTITY(1,1),
    slug          NVARCHAR(80)   NOT NULL,
    nombre        NVARCHAR(120)  NOT NULL,
    siglas        NVARCHAR(20)   NULL,
    descripcion   NVARCHAR(MAX)  NULL,
    activo        BIT            NOT NULL CONSTRAINT DF_Partidos_activo DEFAULT (1),
    CONSTRAINT PK_Partidos PRIMARY KEY (codigoPartido),
    CONSTRAINT UQ_Partidos_slug UNIQUE (slug),
    CONSTRAINT UQ_Partidos_nombre UNIQUE (nombre)
);
GO

/* Se pueblan los partidos a partir de los que ya están escritos en
   Candidatos. El slug se deriva del nombre en minúsculas con guiones. */
INSERT INTO dbo.Partidos (slug, nombre, siglas)
SELECT
    LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
        c.partido, 'á','a'), 'é','e'), 'í','i'), 'ó','o'), 'ú','u'), 'ñ','n'), ' ','-'), '.','')),
    c.partido,
    MAX(c.partidoSiglas)
FROM dbo.Candidatos c
WHERE c.partido IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Partidos p WHERE p.nombre = c.partido)
GROUP BY c.partido;
GO

/* Enlace de Candidatos con Partidos. Queda nulo en las candidaturas
   independientes, que es información y no un dato faltante. */
IF COL_LENGTH('dbo.Candidatos', 'codigoPartido') IS NULL
BEGIN
    ALTER TABLE dbo.Candidatos ADD codigoPartido INT NULL;
END
GO

UPDATE c
   SET c.codigoPartido = p.codigoPartido
FROM dbo.Candidatos c
INNER JOIN dbo.Partidos p ON p.nombre = c.partido
WHERE c.codigoPartido IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Candidatos_Partidos')
ALTER TABLE dbo.Candidatos
    ADD CONSTRAINT FK_Candidatos_Partidos
        FOREIGN KEY (codigoPartido) REFERENCES dbo.Partidos (codigoPartido);
GO

/* ============================================== TIPOS DE OBJETO */

/* Catálogo de las entidades sobre las que se puede opinar.
   Valoraciones y Comentarios apuntan a cualquiera de ellas mediante el
   par (codigoTipoObjeto, codigoObjeto).

   Nota de diseño: esa referencia no puede tener llave foránea, porque el
   destino cambia según el tipo. La alternativa era crear cuatro tablas de
   valoraciones y cuatro de comentarios, con la misma estructura repetida
   ocho veces. Se eligió el catálogo, y la existencia del objeto la
   verifica el Web Service antes de insertar. */
IF OBJECT_ID('dbo.TiposObjeto') IS NULL
CREATE TABLE dbo.TiposObjeto
(
    codigoTipoObjeto INT           NOT NULL IDENTITY(1,1),
    nombre           NVARCHAR(40)  NOT NULL,
    tablaDestino     NVARCHAR(60)  NOT NULL,
    descripcion      NVARCHAR(200) NULL,
    CONSTRAINT PK_TiposObjeto PRIMARY KEY (codigoTipoObjeto),
    CONSTRAINT UQ_TiposObjeto_nombre UNIQUE (nombre)
);
GO

MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Publicacion', N'dbo.Publicaciones', N'Actualización publicada por una candidatura.'),
    (N'Candidato',   N'dbo.Candidatos',    N'Perfil de una candidatura.'),
    (N'Partido',     N'dbo.Partidos',      N'Partido político.'),
    (N'Propuesta',   N'dbo.Propuestas',    N'Proyecto o propuesta de campaña.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

/* ================================================ VALORACIONES */

/* Una fila por usuario y por objeto. El valor 1 es me gusta y el -1 es no
   me gusta. La restricción única es la que impide que una misma persona
   vote dos veces el mismo objeto, y hace que cambiar de opinión sea una
   actualización en lugar de una fila nueva. */
IF OBJECT_ID('dbo.Valoraciones') IS NULL
CREATE TABLE dbo.Valoraciones
(
    codigoValoracion INT          NOT NULL IDENTITY(1,1),
    codigoTipoObjeto INT          NOT NULL,
    codigoObjeto     INT          NOT NULL,
    codigoUsuario    INT          NOT NULL,
    valor            SMALLINT     NOT NULL,
    fecha            DATETIME2(0) NOT NULL CONSTRAINT DF_Valoraciones_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Valoraciones PRIMARY KEY (codigoValoracion),
    CONSTRAINT FK_Valoraciones_TiposObjeto
        FOREIGN KEY (codigoTipoObjeto) REFERENCES dbo.TiposObjeto (codigoTipoObjeto),
    CONSTRAINT FK_Valoraciones_Usuarios
        FOREIGN KEY (codigoUsuario) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT CK_Valoraciones_valor CHECK (valor IN (-1, 1)),
    CONSTRAINT UQ_Valoraciones_unaPorUsuario
        UNIQUE (codigoTipoObjeto, codigoObjeto, codigoUsuario)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Valoraciones_objeto')
CREATE INDEX IX_Valoraciones_objeto
    ON dbo.Valoraciones (codigoTipoObjeto, codigoObjeto) INCLUDE (valor);
GO

/* ================================================= COMENTARIOS */

IF OBJECT_ID('dbo.Comentarios') IS NULL
CREATE TABLE dbo.Comentarios
(
    codigoComentario INT           NOT NULL IDENTITY(1,1),
    codigoTipoObjeto INT           NOT NULL,
    codigoObjeto     INT           NOT NULL,
    codigoUsuario    INT           NOT NULL,
    texto            NVARCHAR(1200) NOT NULL,
    fecha            DATETIME2(0)  NOT NULL CONSTRAINT DF_Comentarios_fecha DEFAULT (SYSDATETIME()),
    /* Queda visible al publicarse. La bandera existe para poder ocultar un
       comentario sin borrarlo, cuando exista moderación. */
    aprobado         BIT           NOT NULL CONSTRAINT DF_Comentarios_aprobado DEFAULT (1),
    CONSTRAINT PK_Comentarios PRIMARY KEY (codigoComentario),
    CONSTRAINT FK_Comentarios_TiposObjeto
        FOREIGN KEY (codigoTipoObjeto) REFERENCES dbo.TiposObjeto (codigoTipoObjeto),
    CONSTRAINT FK_Comentarios_Usuarios
        FOREIGN KEY (codigoUsuario) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT CK_Comentarios_texto CHECK (LEN(LTRIM(RTRIM(texto))) >= 2)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Comentarios_objeto')
CREATE INDEX IX_Comentarios_objeto
    ON dbo.Comentarios (codigoTipoObjeto, codigoObjeto, fecha DESC);
GO

/* ======================= LIMPIEZA DE CONTADORES DUPLICADOS =======

   Publicaciones tenía las columnas apoyos y comentarios con números
   escritos a mano. Ahora esas cifras se derivan de Valoraciones y
   Comentarios, así que mantener las columnas permitiría que el número
   mostrado contradiga a las filas reales.                          */

IF COL_LENGTH('dbo.Publicaciones', 'apoyos') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Publicaciones_apoyos')
        ALTER TABLE dbo.Publicaciones DROP CONSTRAINT DF_Publicaciones_apoyos;
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Publicaciones_apoyos')
        ALTER TABLE dbo.Publicaciones DROP CONSTRAINT CK_Publicaciones_apoyos;
    ALTER TABLE dbo.Publicaciones DROP COLUMN apoyos;
END
GO

IF COL_LENGTH('dbo.Publicaciones', 'comentarios') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Publicaciones_coment')
        ALTER TABLE dbo.Publicaciones DROP CONSTRAINT DF_Publicaciones_coment;
    ALTER TABLE dbo.Publicaciones DROP COLUMN comentarios;
END
GO

PRINT 'Modulo de interaccion creado.';
GO
