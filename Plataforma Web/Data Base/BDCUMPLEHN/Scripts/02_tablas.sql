/* ============================================================
   CumpleHN — 02. Tablas
   ------------------------------------------------------------
   Este script es la fuente del modelo entidad-relación y del
   diccionario de datos del capítulo IX. Se puede volver a
   ejecutar: cada objeto se crea solo si no existe.

   Orden de dependencias:
     Espacios   ->  Catálogos  ->  Campanas  ->  Candidatos  ->  Propuestas
                                                            ->  Publicaciones
     Roles      ->  Usuarios

   Desde el script 19 la plataforma admite espacios: un espacio
   es un cliente que compra el uso de la plataforma para su
   propio proceso electoral (la junta directiva de un colegio
   profesional, por ejemplo). Las columnas codigoEspacio nacen
   acá, junto con sus tablas, y no en el 19: en una base nueva
   las vistas del 07 y los procedimientos del 08 en adelante las
   referencian, y fallarían si la columna se agregara al final.
   El 19 solo migra lo existente al espacio de la plataforma y
   fija los NOT NULL.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* Los índices filtrados de este script (UQ_Espacios_plataforma y
   UQ_Campanas_unicaActual) exigen QUOTED_IDENTIFIER, y sqlcmd lo
   trae apagado. Ver la nota del script 09. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------- ESPACIOS */

/* Un espacio es el cliente que administra su propio proceso
   electoral dentro de la plataforma. El sitio público CumpleHN
   es también un espacio, el único con esPlataforma en 1, para
   que ninguna consulta tenga que tratar el caso «sin espacio»
   como algo distinto.

   padronCerrado: en 1, solo participa quien esté en el padrón
   del espacio (script 20). En la plataforma va en 0, donde
   participa cualquiera con correo confirmado.

   terminoAgrupacion: cómo llama el espacio a la agrupación de
   candidaturas. En la plataforma es «Partido», en una elección
   interna suele ser «Planilla» o «Lista». La tabla Partidos no
   cambia de nombre por esto. */
IF OBJECT_ID('dbo.Espacios') IS NULL
CREATE TABLE dbo.Espacios
(
    codigoEspacio            INT            NOT NULL IDENTITY(1,1),
    slug                     NVARCHAR(80)   NOT NULL,
    nombre                   NVARCHAR(160)  NOT NULL,
    organizacion             NVARCHAR(200)  NOT NULL,
    descripcion              NVARCHAR(1000) NULL,
    esPlataforma             BIT            NOT NULL CONSTRAINT DF_Espacios_esPlataforma DEFAULT (0),
    padronCerrado            BIT            NOT NULL CONSTRAINT DF_Espacios_padron DEFAULT (1),
    terminoAgrupacion        NVARCHAR(40)   NOT NULL CONSTRAINT DF_Espacios_termino DEFAULT (N'Partido'),
    codigoUsuarioPropietario INT            NULL,
    activo                   BIT            NOT NULL CONSTRAINT DF_Espacios_activo DEFAULT (1),
    motivoBaja               NVARCHAR(300)  NULL,
    fechaBaja                DATETIME2(0)   NULL,
    codigoUsuarioBaja        INT            NULL,
    fechaCreacion            DATETIME2(0)   NOT NULL CONSTRAINT DF_Espacios_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Espacios PRIMARY KEY (codigoEspacio),
    CONSTRAINT UQ_Espacios_slug UNIQUE (slug),
    CONSTRAINT UQ_Espacios_nombre UNIQUE (nombre)
);
GO

/* Un solo espacio puede ser la plataforma. Índice filtrado, como
   el de la campaña destacada. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Espacios_plataforma')
CREATE UNIQUE INDEX UQ_Espacios_plataforma
    ON dbo.Espacios (esPlataforma) WHERE esPlataforma = 1;
GO

/* El espacio de la plataforma se siembra acá y no en el 03: no es
   un catálogo, es la fila a la que se cuelga todo lo existente. */
IF NOT EXISTS (SELECT 1 FROM dbo.Espacios WHERE esPlataforma = 1)
INSERT INTO dbo.Espacios (slug, nombre, organizacion, descripcion, esPlataforma, padronCerrado, terminoAgrupacion)
VALUES (N'cumplehn', N'CumpleHN', N'CumpleHN',
        N'Sitio público de seguimiento ciudadano de promesas políticas en Honduras.',
        1, 0, N'Partido');
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
   no se repite en cada candidato.

   Cada espacio tiene sus propios cargos: la plataforma los de elección
   popular, una organización «Presidente de junta» o «Tesorero». Nacen
   con codigoEspacio nulo y el 23 los asigna a la plataforma. El nombre
   es único por espacio y no en toda la tabla. */
IF OBJECT_ID('dbo.Cargos') IS NULL
CREATE TABLE dbo.Cargos
(
    codigoCargo    INT          NOT NULL IDENTITY(1,1),
    nombre         NVARCHAR(80) NOT NULL,
    nivelGobierno  NVARCHAR(20) NOT NULL,
    orden          INT          NOT NULL CONSTRAINT DF_Cargos_orden DEFAULT (0),
    codigoEspacio  INT          NULL,
    activo         BIT          NOT NULL CONSTRAINT DF_Cargos_activo DEFAULT (1),
    CONSTRAINT PK_Cargos PRIMARY KEY (codigoCargo),
    CONSTRAINT UQ_Cargos_espacioNombre UNIQUE (codigoEspacio, nombre),
    CONSTRAINT FK_Cargos_Espacios FOREIGN KEY (codigoEspacio) REFERENCES dbo.Espacios (codigoEspacio),
    CONSTRAINT CK_Cargos_nivel CHECK (nivelGobierno IN ('Nacional','Departamental','Municipal','Institucional'))
);
GO

/* Base existente: la columna se agrega con el mismo patrón que usó
   el 05 para Candidatos.codigoPartido. La restricción única y el
   CHECK del nivel se reemplazan en el 19, después de la migración. */
IF COL_LENGTH('dbo.Cargos', 'codigoEspacio') IS NULL
    ALTER TABLE dbo.Cargos ADD codigoEspacio INT NULL
        CONSTRAINT FK_Cargos_Espacios REFERENCES dbo.Espacios (codigoEspacio);
GO

/* Baja lógica del cargo (script 23): un cargo con candidaturas no se
   borra, se saca del desplegable. */
IF COL_LENGTH('dbo.Cargos', 'activo') IS NULL
    ALTER TABLE dbo.Cargos ADD activo BIT NOT NULL CONSTRAINT DF_Cargos_activo DEFAULT (1);
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
    /* NULL es compartida (la taxonomía COFOG). Las de un espacio
       solo se listan dentro de él, y el interés de la encuesta
       (script 08) solo tiene sentido en las compartidas. */
    codigoEspacio   INT           NULL,
    CONSTRAINT PK_Categorias PRIMARY KEY (codigoCategoria),
    CONSTRAINT UQ_Categorias_espacioNombre UNIQUE (codigoEspacio, nombre),
    CONSTRAINT FK_Categorias_Espacios FOREIGN KEY (codigoEspacio) REFERENCES dbo.Espacios (codigoEspacio)
);
GO

IF COL_LENGTH('dbo.Categorias', 'codigoEspacio') IS NULL
    ALTER TABLE dbo.Categorias ADD codigoEspacio INT NULL
        CONSTRAINT FK_Categorias_Espacios REFERENCES dbo.Espacios (codigoEspacio);
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

/* Campaña electoral. Es la entidad raíz de un espacio: toda candidatura,
   propuesta, publicación y encuesta pertenece a una campaña, y por ella
   hereda el espacio. Es lo que hace que aislar un espacio cueste una sola
   columna acá y no una en cada tabla.

   El slug sigue siendo único en toda la plataforma y no por espacio: la
   dirección /Campana/{slug} identifica la campaña sin prefijo, y la
   campaña dice a qué espacio pertenece. */
IF OBJECT_ID('dbo.Campanas') IS NULL
CREATE TABLE dbo.Campanas
(
    codigoCampana INT            NOT NULL IDENTITY(1,1),
    codigoEspacio INT            NULL,   -- NOT NULL desde el 19
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
    CONSTRAINT FK_Campanas_Espacios FOREIGN KEY (codigoEspacio) REFERENCES dbo.Espacios (codigoEspacio),
    CONSTRAINT CK_Campanas_estado CHECK (estado IN ('Activa','Proxima','Cerrada')),
    CONSTRAINT CK_Campanas_fechas CHECK (fechaEleccion >= fechaInicio)
);
GO

IF COL_LENGTH('dbo.Campanas', 'codigoEspacio') IS NULL
    ALTER TABLE dbo.Campanas ADD codigoEspacio INT NULL
        CONSTRAINT FK_Campanas_Espacios REFERENCES dbo.Espacios (codigoEspacio);
GO

/* Solo una campaña por espacio puede estar destacada en su portada a la
   vez. El índice filtrado lo obliga a nivel de base, no de aplicación.
   En una base anterior al 19 el índice existe sin la columna del
   espacio, y es el 19 quien lo reemplaza. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Campanas_unicaActual')
CREATE UNIQUE INDEX UQ_Campanas_unicaActual
    ON dbo.Campanas (codigoEspacio, esActual) WHERE esActual = 1;
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
    /* Alcance de una cuenta con rol Administrador: NULL administra
       la plataforma entera y todos los espacios, un valor administra
       solo ese espacio (es la cuenta del cliente que lo compró). En
       los demás roles va siempre en NULL: la candidatura ya dice su
       campaña y la campaña su espacio, y el ciudadano tiene cuenta
       global y participa donde el padrón lo admita. No hace falta un
       cuarto rol: fnEsAdministradorDe (script 09) lee esta columna. */
    codigoEspacio   INT           NULL,
    activo          BIT           NOT NULL CONSTRAINT DF_Usuarios_activo DEFAULT (1),
    fechaRegistro   DATETIME2(0)  NOT NULL CONSTRAINT DF_Usuarios_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Usuarios PRIMARY KEY (codigoUsuario),
    CONSTRAINT UQ_Usuarios_login UNIQUE (login),
    CONSTRAINT UQ_Usuarios_correo UNIQUE (correo),
    CONSTRAINT FK_Usuarios_Roles
        FOREIGN KEY (codigoRol) REFERENCES dbo.Roles (codigoRol),
    CONSTRAINT FK_Usuarios_Candidatos
        FOREIGN KEY (codigoCandidato) REFERENCES dbo.Candidatos (codigoCandidato),
    CONSTRAINT FK_Usuarios_Espacios
        FOREIGN KEY (codigoEspacio) REFERENCES dbo.Espacios (codigoEspacio)
);
GO

IF COL_LENGTH('dbo.Usuarios', 'codigoEspacio') IS NULL
    ALTER TABLE dbo.Usuarios ADD codigoEspacio INT NULL
        CONSTRAINT FK_Usuarios_Espacios REFERENCES dbo.Espacios (codigoEspacio);
GO

/* El propietario del espacio es una cuenta, y la cuenta apunta a su
   espacio: la referencia es circular y por eso la llave foránea de
   Espacios se agrega acá, cuando ya existen las dos tablas. */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Espacios_Propietario')
    ALTER TABLE dbo.Espacios ADD
        CONSTRAINT FK_Espacios_Propietario
            FOREIGN KEY (codigoUsuarioPropietario) REFERENCES dbo.Usuarios (codigoUsuario),
        CONSTRAINT FK_Espacios_UsuarioBaja
            FOREIGN KEY (codigoUsuarioBaja) REFERENCES dbo.Usuarios (codigoUsuario);
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
