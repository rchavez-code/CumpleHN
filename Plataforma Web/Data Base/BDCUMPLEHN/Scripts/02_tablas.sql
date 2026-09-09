/* ============================================================
   CumpleHN — 02. Tablas
   ------------------------------------------------------------
   Este script es la fuente del modelo entidad-relación y del
   diccionario de datos del capítulo IX. Se puede volver a
   ejecutar: cada objeto se crea solo si no existe.

   Orden de dependencias:
     Catálogos  ->  Campanas  ->  Candidatos  ->  Propuestas
                                             ->  Publicaciones
     Roles      ->  Usuarios
   ============================================================ */

USE BDCUMPLEHN;
GO

/* ------------------------------------------------ CATÁLOGOS */

/* Roles de usuario. Define qué puede hacer cada cuenta. */
IF OBJECT_ID('dbo.Roles') IS NULL
CREATE TABLE dbo.Roles
(
    codigoRol   INT           NOT NULL IDENTITY(1,1),
    nombre      NVARCHAR(40)  NOT NULL,
    descripcion NVARCHAR(200) NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (codigoRol),
    CONSTRAINT UQ_Roles_nombre UNIQUE (nombre)
);
GO

/* Cargos de elección popular. El nivel de gobierno se hereda de acá,
   no se repite en cada candidato. */
IF OBJECT_ID('dbo.Cargos') IS NULL
CREATE TABLE dbo.Cargos
(
    codigoCargo    INT          NOT NULL IDENTITY(1,1),
    nombre         NVARCHAR(80) NOT NULL,
    nivelGobierno  NVARCHAR(20) NOT NULL,
    orden          INT          NOT NULL CONSTRAINT DF_Cargos_orden DEFAULT (0),
    CONSTRAINT PK_Cargos PRIMARY KEY (codigoCargo),
    CONSTRAINT UQ_Cargos_nombre UNIQUE (nombre),
    CONSTRAINT CK_Cargos_nivel CHECK (nivelGobierno IN ('Nacional','Departamental','Municipal'))
);
GO

/* Los 18 departamentos de Honduras. */
IF OBJECT_ID('dbo.Departamentos') IS NULL
CREATE TABLE dbo.Departamentos
(
    codigoDepartamento INT          NOT NULL IDENTITY(1,1),
    nombre             NVARCHAR(60) NOT NULL,
    CONSTRAINT PK_Departamentos PRIMARY KEY (codigoDepartamento),
    CONSTRAINT UQ_Departamentos_nombre UNIQUE (nombre)
);
GO

/* Taxonomía temática derivada de la Clasificación de las Funciones del
   Gobierno (ONU, 2000), adoptada en el marco teórico. El campo orden
   refleja el interés medido en la encuesta del proyecto. */
IF OBJECT_ID('dbo.Categorias') IS NULL
CREATE TABLE dbo.Categorias
(
    codigoCategoria INT           NOT NULL IDENTITY(1,1),
    nombre          NVARCHAR(60)  NOT NULL,
    descripcion     NVARCHAR(300) NULL,
    orden           INT           NOT NULL CONSTRAINT DF_Categorias_orden DEFAULT (0),
    CONSTRAINT PK_Categorias PRIMARY KEY (codigoCategoria),
    CONSTRAINT UQ_Categorias_nombre UNIQUE (nombre)
);
GO

/* Estados de cumplimiento. Replican el esquema del rastreador de promesas
   de PolitiFact (2018). La ponderación es el valor con el que cada estado
   entra en el cálculo del porcentaje de cumplimiento. */
IF OBJECT_ID('dbo.EstadosPropuesta') IS NULL
CREATE TABLE dbo.EstadosPropuesta
(
    codigoEstado INT           NOT NULL IDENTITY(1,1),
    nombre       NVARCHAR(40)  NOT NULL,
    descripcion  NVARCHAR(300) NULL,
    ponderacion  DECIMAL(4,2)  NOT NULL CONSTRAINT DF_EstadosPropuesta_pond DEFAULT (0),
    orden        INT           NOT NULL CONSTRAINT DF_EstadosPropuesta_orden DEFAULT (0),
    CONSTRAINT PK_EstadosPropuesta PRIMARY KEY (codigoEstado),
    CONSTRAINT UQ_EstadosPropuesta_nombre UNIQUE (nombre),
    CONSTRAINT CK_EstadosPropuesta_pond CHECK (ponderacion BETWEEN 0 AND 1)
);
GO

/* Distingue lo que el candidato afirma de lo que la plataforma respalda.
   Es la garantía de neutralidad del proyecto. */
IF OBJECT_ID('dbo.NivelesVerificacion') IS NULL
CREATE TABLE dbo.NivelesVerificacion
(
    codigoVerificacion INT           NOT NULL IDENTITY(1,1),
    nombre             NVARCHAR(40)  NOT NULL,
    descripcion        NVARCHAR(300) NULL,
    orden              INT           NOT NULL CONSTRAINT DF_NivelesVerificacion_orden DEFAULT (0),
    CONSTRAINT PK_NivelesVerificacion PRIMARY KEY (codigoVerificacion),
    CONSTRAINT UQ_NivelesVerificacion_nombre UNIQUE (nombre)
);
GO

/* ------------------------------------------------- CAMPAÑAS */

/* Campaña electoral. Es la entidad raíz: toda candidatura, propuesta y
   publicación pertenece a una campaña. */
IF OBJECT_ID('dbo.Campanas') IS NULL
CREATE TABLE dbo.Campanas
(
    codigoCampana INT            NOT NULL IDENTITY(1,1),
    slug          NVARCHAR(80)   NOT NULL,
    nombre        NVARCHAR(160)  NOT NULL,
    resumen       NVARCHAR(400)  NULL,
    descripcion   NVARCHAR(MAX)  NULL,
    alcance       NVARCHAR(200)  NULL,
    fechaInicio   DATE           NOT NULL,
    fechaEleccion DATE           NOT NULL,
    estado        NVARCHAR(20)   NOT NULL,
    esActual      BIT            NOT NULL CONSTRAINT DF_Campanas_esActual DEFAULT (0),
    CONSTRAINT PK_Campanas PRIMARY KEY (codigoCampana),
    CONSTRAINT UQ_Campanas_slug UNIQUE (slug),
    CONSTRAINT CK_Campanas_estado CHECK (estado IN ('Activa','Proxima','Cerrada')),
    CONSTRAINT CK_Campanas_fechas CHECK (fechaEleccion >= fechaInicio)
);
GO

/* Solo una campaña puede estar destacada en la portada a la vez. El índice
   filtrado lo obliga a nivel de base, no de aplicación. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Campanas_unicaActual')
CREATE UNIQUE INDEX UQ_Campanas_unicaActual
    ON dbo.Campanas (esActual) WHERE esActual = 1;
GO

/* ----------------------------------------------- CANDIDATOS */

IF OBJECT_ID('dbo.Candidatos') IS NULL
CREATE TABLE dbo.Candidatos
(
    codigoCandidato         INT            NOT NULL IDENTITY(1,1),
    slug                    NVARCHAR(120)  NOT NULL,
    codigoCampana           INT            NOT NULL,
    nombres                 NVARCHAR(80)   NOT NULL,
    apellidos               NVARCHAR(80)   NOT NULL,
    partido                 NVARCHAR(120)  NULL,   -- NULL = candidatura independiente
    partidoSiglas           NVARCHAR(20)   NULL,
    codigoCargo             INT            NOT NULL,
    codigoDepartamento      INT            NULL,
    municipio               NVARCHAR(120)  NULL,
    fotoUrl                 NVARCHAR(300)  NULL,
    titular                 NVARCHAR(200)  NULL,
    biografia               NVARCHAR(MAX)  NULL,
    informacionProfesional  NVARCHAR(MAX)  NULL,
    descripcionCandidatura  NVARCHAR(MAX)  NULL,
    correoPublico           NVARCHAR(160)  NULL,
    telefono                NVARCHAR(40)   NULL,
    sitioWeb                NVARCHAR(200)  NULL,
    facebook                NVARCHAR(120)  NULL,
    x                       NVARCHAR(120)  NULL,
    instagram               NVARCHAR(120)  NULL,
    codigoVerificacion      INT            NOT NULL,
    activo                  BIT            NOT NULL CONSTRAINT DF_Candidatos_activo DEFAULT (1),
    fechaRegistro           DATETIME2(0)   NOT NULL CONSTRAINT DF_Candidatos_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Candidatos PRIMARY KEY (codigoCandidato),
    CONSTRAINT UQ_Candidatos_slug UNIQUE (slug),
    CONSTRAINT FK_Candidatos_Campanas
        FOREIGN KEY (codigoCampana) REFERENCES dbo.Campanas (codigoCampana),
    CONSTRAINT FK_Candidatos_Cargos
        FOREIGN KEY (codigoCargo) REFERENCES dbo.Cargos (codigoCargo),
    CONSTRAINT FK_Candidatos_Departamentos
        FOREIGN KEY (codigoDepartamento) REFERENCES dbo.Departamentos (codigoDepartamento),
    CONSTRAINT FK_Candidatos_Verificacion
        FOREIGN KEY (codigoVerificacion) REFERENCES dbo.NivelesVerificacion (codigoVerificacion)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Candidatos_campana')
CREATE INDEX IX_Candidatos_campana ON dbo.Candidatos (codigoCampana, apellidos, nombres);
GO

/* ------------------------------------------------- USUARIOS */

/* Cuenta de acceso. Un usuario con rol Candidato queda enlazado a su ficha
   en Candidatos. Los ciudadanos y administradores no tienen ficha. */
IF OBJECT_ID('dbo.Usuarios') IS NULL
CREATE TABLE dbo.Usuarios
(
    codigoUsuario   INT           NOT NULL IDENTITY(1,1),
    login           NVARCHAR(60)  NOT NULL,
    clave           CHAR(64)      NOT NULL,   -- SHA-256 en hexadecimal minúscula
    nombre          NVARCHAR(160) NOT NULL,
    correo          NVARCHAR(160) NOT NULL,
    codigoRol       INT           NOT NULL,
    codigoCandidato INT           NULL,
    activo          BIT           NOT NULL CONSTRAINT DF_Usuarios_activo DEFAULT (1),
    fechaRegistro   DATETIME2(0)  NOT NULL CONSTRAINT DF_Usuarios_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Usuarios PRIMARY KEY (codigoUsuario),
    CONSTRAINT UQ_Usuarios_login UNIQUE (login),
    CONSTRAINT UQ_Usuarios_correo UNIQUE (correo),
    CONSTRAINT FK_Usuarios_Roles
        FOREIGN KEY (codigoRol) REFERENCES dbo.Roles (codigoRol),
    CONSTRAINT FK_Usuarios_Candidatos
        FOREIGN KEY (codigoCandidato) REFERENCES dbo.Candidatos (codigoCandidato)
);
GO

/* ------------------------------------------------ PROPUESTAS */

/* Proyecto de campaña. Es deliberadamente la misma entidad que la promesa
   política del marco teórico: nace declarada por el candidato y más
   adelante recibe un estado de cumplimiento respaldado por evidencia. */
IF OBJECT_ID('dbo.Propuestas') IS NULL
CREATE TABLE dbo.Propuestas
(
    codigoPropuesta      INT            NOT NULL IDENTITY(1,1),
    codigoCandidato      INT            NOT NULL,
    codigoCampana        INT            NOT NULL,
    nombre               NVARCHAR(200)  NOT NULL,
    descripcion          NVARCHAR(MAX)  NOT NULL,
    problema             NVARCHAR(MAX)  NULL,
    objetivo             NVARCHAR(MAX)  NULL,
    beneficiarios        NVARCHAR(400)  NULL,
    codigoCategoria      INT            NOT NULL,
    ubicacion            NVARCHAR(200)  NULL,   -- NULL = cobertura nacional
    periodoEjecucion     NVARCHAR(120)  NULL,
    codigoEstado         INT            NOT NULL,
    imagenUrl            NVARCHAR(300)  NULL,
    informacionAdicional NVARCHAR(MAX)  NULL,
    codigoVerificacion   INT            NOT NULL,
    fechaRegistro        DATETIME2(0)   NOT NULL CONSTRAINT DF_Propuestas_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Propuestas PRIMARY KEY (codigoPropuesta),
    CONSTRAINT FK_Propuestas_Candidatos
        FOREIGN KEY (codigoCandidato) REFERENCES dbo.Candidatos (codigoCandidato),
    CONSTRAINT FK_Propuestas_Campanas
        FOREIGN KEY (codigoCampana) REFERENCES dbo.Campanas (codigoCampana),
    CONSTRAINT FK_Propuestas_Categorias
        FOREIGN KEY (codigoCategoria) REFERENCES dbo.Categorias (codigoCategoria),
    CONSTRAINT FK_Propuestas_Estados
        FOREIGN KEY (codigoEstado) REFERENCES dbo.EstadosPropuesta (codigoEstado),
    CONSTRAINT FK_Propuestas_Verificacion
        FOREIGN KEY (codigoVerificacion) REFERENCES dbo.NivelesVerificacion (codigoVerificacion)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Propuestas_candidato')
CREATE INDEX IX_Propuestas_candidato ON dbo.Propuestas (codigoCandidato, fechaRegistro DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Propuestas_campana')
CREATE INDEX IX_Propuestas_campana ON dbo.Propuestas (codigoCampana, codigoCategoria);
GO

/* ---------------------------------------------- PUBLICACIONES */

IF OBJECT_ID('dbo.Publicaciones') IS NULL
CREATE TABLE dbo.Publicaciones
(
    codigoPublicacion  INT            NOT NULL IDENTITY(1,1),
    codigoCandidato    INT            NOT NULL,
    codigoCampana      INT            NOT NULL,
    fecha              DATETIME2(0)   NOT NULL CONSTRAINT DF_Publicaciones_fecha DEFAULT (SYSDATETIME()),
    texto              NVARCHAR(MAX)  NOT NULL,
    imagenUrl          NVARCHAR(300)  NULL,
    codigoCategoria    INT            NULL,
    codigoPropuesta    INT            NULL,   -- propuesta a la que hace referencia
    codigoVerificacion INT            NOT NULL,
    apoyos             INT            NOT NULL CONSTRAINT DF_Publicaciones_apoyos DEFAULT (0),
    comentarios        INT            NOT NULL CONSTRAINT DF_Publicaciones_coment DEFAULT (0),
    CONSTRAINT PK_Publicaciones PRIMARY KEY (codigoPublicacion),
    CONSTRAINT FK_Publicaciones_Candidatos
        FOREIGN KEY (codigoCandidato) REFERENCES dbo.Candidatos (codigoCandidato),
    CONSTRAINT FK_Publicaciones_Campanas
        FOREIGN KEY (codigoCampana) REFERENCES dbo.Campanas (codigoCampana),
    CONSTRAINT FK_Publicaciones_Categorias
        FOREIGN KEY (codigoCategoria) REFERENCES dbo.Categorias (codigoCategoria),
    CONSTRAINT FK_Publicaciones_Propuestas
        FOREIGN KEY (codigoPropuesta) REFERENCES dbo.Propuestas (codigoPropuesta),
    CONSTRAINT FK_Publicaciones_Verificacion
        FOREIGN KEY (codigoVerificacion) REFERENCES dbo.NivelesVerificacion (codigoVerificacion),
    CONSTRAINT CK_Publicaciones_apoyos CHECK (apoyos >= 0 AND comentarios >= 0)
);
GO

/* Baja lógica de las publicaciones.

   Una publicación inadecuada se retira, no se borra. Un DELETE
   se llevaría por delante sus valoraciones y sus comentarios, y
   dejaría al tablero de analítica contando totales que ya no
   cuadran con las filas que quedan. Retirada, la fila sigue
   existiendo con el motivo y el responsable del retiro, que es
   lo que permite auditar la moderación.

   Va como ALTER y no dentro del CREATE TABLE de arriba porque
   ese CREATE solo corre en una base nueva: en la que ya existe
   estas columnas se agregan acá.

   La consulta pública filtra activo = 1. Solo el área de
   administración ve las retiradas. */

IF COL_LENGTH('dbo.Publicaciones', 'activo') IS NULL
    ALTER TABLE dbo.Publicaciones
        ADD activo BIT NOT NULL CONSTRAINT DF_Publicaciones_activo DEFAULT (1);
GO

IF COL_LENGTH('dbo.Publicaciones', 'motivoBaja') IS NULL
    ALTER TABLE dbo.Publicaciones ADD motivoBaja NVARCHAR(300) NULL;
GO

IF COL_LENGTH('dbo.Publicaciones', 'fechaBaja') IS NULL
    ALTER TABLE dbo.Publicaciones ADD fechaBaja DATETIME2(0) NULL;
GO

/* Quién la retiró. Sin llave foránea a Usuarios no habría manera
   de responder por una moderación. */
IF COL_LENGTH('dbo.Publicaciones', 'codigoUsuarioBaja') IS NULL
BEGIN
    ALTER TABLE dbo.Publicaciones ADD codigoUsuarioBaja INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Publicaciones_UsuarioBaja')
    ALTER TABLE dbo.Publicaciones
        ADD CONSTRAINT FK_Publicaciones_UsuarioBaja
            FOREIGN KEY (codigoUsuarioBaja) REFERENCES dbo.Usuarios (codigoUsuario);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Publicaciones_campana')
CREATE INDEX IX_Publicaciones_campana ON dbo.Publicaciones (codigoCampana, fecha DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Publicaciones_candidato')
CREATE INDEX IX_Publicaciones_candidato ON dbo.Publicaciones (codigoCandidato, fecha DESC);
GO
