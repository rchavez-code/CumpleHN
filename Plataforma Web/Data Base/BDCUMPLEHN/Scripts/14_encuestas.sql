/* ============================================================
   CumpleHN — 14. Encuestas de percepción
   ------------------------------------------------------------
   La tercera pata del módulo de participación ciudadana. El
   FO-GR-013 aprobado describe ese módulo como «comentarios,
   valoraciones o votaciones de percepción»: las dos primeras
   están en el script 05, esta es la tercera.

   Cuatro decisiones sostienen todo lo de acá:

     1. No se reusan las Valoraciones. Aquella tabla guarda un
        voto de -1 o 1, y una encuesta es una opción entre N.
        Forzarla ahí ensuciaría el gráfico de signo de la
        participación, que hoy significa una sola cosa.

     2. Los conteos no se guardan. El resultado de cada opción
        se deriva con COUNT sobre EncuestaVotos. Una columna
        votos en la opción es exactamente la contradicción que
        se eliminó de Publicaciones en el script 05.

     3. El estado se deriva de las fechas. No hay columna de
        estado: una columna que diga «abierta» sobre una
        encuesta cuya fecha de cierre ya pasó es una mentira
        esperando a ocurrir. Lo resuelve vwEncuestas.

     4. El resultado se muestra siempre, se haya respondido o
        no. Hubo una versión que lo reservaba hasta después de
        votar, para no empujar a nadie hacia la opción que iba
        ganando, y se quitó a pedido: esconder el resultado hasta
        que participes convierte el dato en un peaje. Lo que
        queda de aquella decisión es el aviso del pie de la
        tarjeta, que dice a quién describe el resultado.

   Varias encuestas pueden estar abiertas a la vez. La portada
   muestra las dos primeras y deja el resto tras un «Ver más»,
   pero cuántas se ven es una decisión de presentación: acá se
   devuelven todas las abiertas.

   Convención de los procedimientos de escritura, igual que en
   los scripts 09 y 10: devuelven una fila con ok (BIT) y
   mensaje (NVARCHAR), y el Web Service la transporta tal cual.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* Un procedimiento guarda para siempre el valor que tenían estas
   dos opciones al crearse, y sqlcmd trae QUOTED_IDENTIFIER
   apagado a diferencia de SSMS. Con la opción apagada, cualquier
   escritura sobre una tabla con índice filtrado falla con el
   error 1934. Ver la nota del script 09. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Tablas
   ============================================================ */

/* La encuesta pertenece a una campaña, igual que todo lo demás
   de la plataforma. Sin campaña no habría dónde mostrarla ni
   contra qué contrastar el resultado.

   No se borra: se retira con activo, motivoBaja, fechaBaja y
   codigoUsuarioBaja, el mismo juego de columnas que
   Publicaciones. Un DELETE se llevaría los votos, y la
   plataforma quedaría contando participación que ya no tiene
   filas detrás. */

IF OBJECT_ID('dbo.Encuestas') IS NULL
CREATE TABLE dbo.Encuestas
(
    codigoEncuesta     INT            NOT NULL IDENTITY(1,1),
    codigoCampana      INT            NOT NULL,
    pregunta           NVARCHAR(300)  NOT NULL,
    descripcion        NVARCHAR(600)  NULL,
    /* Opcional. Cuando la pregunta se refiere a un área temática
       concreta, permite contrastarla con la oferta programática
       de esa misma categoría. */
    codigoCategoria    INT            NULL,
    fechaInicio        DATETIME2(0)   NOT NULL,
    /* NULL es una encuesta sin fecha de cierre: sigue abierta
       hasta que la administración la cierre a mano. */
    fechaCierre        DATETIME2(0)   NULL,
    activo             BIT            NOT NULL CONSTRAINT DF_Encuestas_activo DEFAULT (1),
    motivoBaja         NVARCHAR(300)  NULL,
    fechaBaja          DATETIME2(0)   NULL,
    codigoUsuarioBaja  INT            NULL,
    codigoUsuarioAlta  INT            NOT NULL,
    fechaRegistro      DATETIME2(0)   NOT NULL CONSTRAINT DF_Encuestas_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Encuestas PRIMARY KEY (codigoEncuesta),
    CONSTRAINT FK_Encuestas_Campanas
        FOREIGN KEY (codigoCampana) REFERENCES dbo.Campanas (codigoCampana),
    CONSTRAINT FK_Encuestas_Categorias
        FOREIGN KEY (codigoCategoria) REFERENCES dbo.Categorias (codigoCategoria),
    CONSTRAINT FK_Encuestas_UsuarioAlta
        FOREIGN KEY (codigoUsuarioAlta) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT FK_Encuestas_UsuarioBaja
        FOREIGN KEY (codigoUsuarioBaja) REFERENCES dbo.Usuarios (codigoUsuario),
    CONSTRAINT CK_Encuestas_fechas
        CHECK (fechaCierre IS NULL OR fechaCierre > fechaInicio)
);
GO

/* Las opciones entre las que se elige.

   La restricción única sobre (codigoEncuesta, codigoOpcion)
   parece redundante con la llave primaria, y no lo es: es lo que
   permite que EncuestaVotos apunte al par completo con una llave
   foránea compuesta. Sin ella, un voto podría guardar una opción
   que pertenece a otra encuesta, y ninguna restricción lo
   impediría. */

IF OBJECT_ID('dbo.EncuestaOpciones') IS NULL
CREATE TABLE dbo.EncuestaOpciones
(
    codigoOpcion   INT           NOT NULL IDENTITY(1,1),
    codigoEncuesta INT           NOT NULL,
    texto          NVARCHAR(160) NOT NULL,
    orden          INT           NOT NULL CONSTRAINT DF_EncuestaOpciones_orden DEFAULT (0),
    CONSTRAINT PK_EncuestaOpciones PRIMARY KEY (codigoOpcion),
    CONSTRAINT UQ_EncuestaOpciones_par UNIQUE (codigoEncuesta, codigoOpcion),
    CONSTRAINT UQ_EncuestaOpciones_texto UNIQUE (codigoEncuesta, texto),
    CONSTRAINT FK_EncuestaOpciones_Encuestas
        FOREIGN KEY (codigoEncuesta) REFERENCES dbo.Encuestas (codigoEncuesta)
);
GO

/* Un voto por persona y por encuesta, garantizado por la
   restricción única y no solo por el código, igual que
   UQ_Valoraciones_unaPorUsuario en el script 05. Cambiar de
   opinión mientras la encuesta sigue abierta es una
   actualización de esta fila, nunca una fila nueva. */

IF OBJECT_ID('dbo.EncuestaVotos') IS NULL
CREATE TABLE dbo.EncuestaVotos
(
    codigoVoto     INT          NOT NULL IDENTITY(1,1),
    codigoEncuesta INT          NOT NULL,
    codigoOpcion   INT          NOT NULL,
    codigoUsuario  INT          NOT NULL,
    fecha          DATETIME2(0) NOT NULL CONSTRAINT DF_EncuestaVotos_fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_EncuestaVotos PRIMARY KEY (codigoVoto),
    CONSTRAINT UQ_EncuestaVotos_unoPorUsuario UNIQUE (codigoEncuesta, codigoUsuario),
    CONSTRAINT FK_EncuestaVotos_Usuarios
        FOREIGN KEY (codigoUsuario) REFERENCES dbo.Usuarios (codigoUsuario),
    /* Llave compuesta: la opción tiene que ser de esta encuesta. */
    CONSTRAINT FK_EncuestaVotos_Opciones
        FOREIGN KEY (codigoEncuesta, codigoOpcion)
        REFERENCES dbo.EncuestaOpciones (codigoEncuesta, codigoOpcion)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EncuestaVotos_encuesta')
CREATE INDEX IX_EncuestaVotos_encuesta
    ON dbo.EncuestaVotos (codigoEncuesta) INCLUDE (codigoOpcion);
GO

/* ============================================================
   2. Catálogos que suma este módulo
   ============================================================ */

/* La encuesta también se puede valorar y comentar, y eso sale
   gratis por el diseño polimórfico del script 05: basta con
   registrarla como tipo de objeto. */

MERGE dbo.TiposObjeto AS destino
USING (VALUES
    (N'Encuesta', N'dbo.Encuestas', N'Encuesta de percepción publicada por la plataforma.')
) AS origen (nombre, tablaDestino, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, tablaDestino, descripcion)
    VALUES (origen.nombre, origen.tablaDestino, origen.descripcion);
GO

/* Cerrar una encuesta no es darla de baja ni editarla, así que
   la bitácora necesita sus propias acciones para poder
   distinguirlas al leerla después. */

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auditoria_accion')
    ALTER TABLE dbo.Auditoria DROP CONSTRAINT CK_Auditoria_accion;
GO

ALTER TABLE dbo.Auditoria
    ADD CONSTRAINT CK_Auditoria_accion
        CHECK (accion IN ('Verificacion', 'Retiro', 'Restauracion',
                          'Alta', 'Edicion', 'Baja', 'Cuenta', 'Modulo',
                          'Cierre', 'Reapertura'));
GO

/* El interruptor del módulo. Al apagarlo, la encuesta desaparece
   de la portada para el público y el Web Service deja de aceptar
   votos — las dos cosas, porque cerrar solo la página se saltaría
   llamando al servicio directamente. */

MERGE dbo.Modulos AS destino
USING (VALUES
    (N'encuestas', N'Encuestas de percepción',
     N'La encuesta abierta en la portada. Al apagarla, lo ya votado se conserva y deja de admitirse participación nueva.',
     N'Módulos del sitio', NULL, 45)
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
   3. Estado efectivo de cada encuesta

   Cuatro estados derivados, ninguno guardado:

     Retirada    la administración la dio de baja
     Programada  su fecha de inicio todavía no llega
     Cerrada     su fecha de cierre ya pasó
     Abierta     admite votos ahora mismo

   La vista no proyecta codigoUsuarioAlta ni codigoUsuarioBaja.
   Quien administró una encuesta queda en la bitácora, que es
   donde hace falta, y no en una vista que cualquier consulta
   puede leer.
   ============================================================ */

IF OBJECT_ID('dbo.vwEncuestas') IS NOT NULL
    DROP VIEW dbo.vwEncuestas;
GO

CREATE VIEW dbo.vwEncuestas
AS
SELECT
    e.codigoEncuesta,
    e.codigoCampana,
    ca.slug                      AS campanaSlug,
    ca.nombre                    AS campana,
    e.pregunta,
    ISNULL(e.descripcion, N'')   AS descripcion,
    ISNULL(e.codigoCategoria, 0) AS codigoCategoria,
    ISNULL(cat.nombre, N'')      AS categoria,
    e.fechaInicio,
    e.fechaCierre,
    e.activo,
    ISNULL(e.motivoBaja, N'')    AS motivoBaja,
    CASE
        WHEN e.activo = 0                                                 THEN N'Retirada'
        WHEN SYSDATETIME() < e.fechaInicio                                THEN N'Programada'
        WHEN e.fechaCierre IS NOT NULL AND SYSDATETIME() >= e.fechaCierre THEN N'Cerrada'
        ELSE N'Abierta'
    END AS estado,
    (SELECT COUNT(*) FROM dbo.EncuestaOpciones o
      WHERE o.codigoEncuesta = e.codigoEncuesta)  AS opciones,
    (SELECT COUNT(*) FROM dbo.EncuestaVotos v
      WHERE v.codigoEncuesta = e.codigoEncuesta)  AS votos,
    e.fechaRegistro
FROM dbo.Encuestas e
INNER JOIN dbo.Campanas ca    ON ca.codigoCampana = e.codigoCampana
LEFT  JOIN dbo.Categorias cat ON cat.codigoCategoria = e.codigoCategoria;
GO

/* ============================================================
   4. Consulta pública
   ============================================================ */

/* Las encuestas que se muestran en la portada.

   Devuelve todas las abiertas, de la más reciente a la más
   antigua. Con la campaña vacía usa la destacada, que es como
   llega desde la portada.

   Solo abiertas. Una encuesta cerrada terminó su votación y se
   consulta desde administración: dejarla en la portada junto a
   las que sí admiten respuesta obligaría a quien mira a
   distinguir cuáles puede contestar antes de intentarlo.

   Cuántas se ven a la vez no se decide acá. Este procedimiento
   las devuelve todas y la portada muestra las dos primeras,
   porque es una decisión de presentación y puede cambiar sin
   tocar la base.

   El parámetro del usuario es lo que permite saber si ya votó en
   cada una, y con ello si le corresponde ver el resultado. Con
   cero es un visitante sin cuenta.                              */

IF OBJECT_ID('dbo.spEncuestaVigente') IS NOT NULL
    DROP PROCEDURE dbo.spEncuestaVigente;
GO

IF OBJECT_ID('dbo.spEncuestasVigentes') IS NOT NULL
    DROP PROCEDURE dbo.spEncuestasVigentes;
GO

CREATE PROCEDURE dbo.spEncuestasVigentes
    @campanaSlug   NVARCHAR(80) = NULL,
    @codigoUsuario INT          = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.codigoEncuesta,
        e.campanaSlug,
        e.pregunta,
        e.descripcion,
        e.categoria,
        e.fechaInicio,
        e.fechaCierre,
        e.estado,
        e.votos,
        ISNULL((SELECT v.codigoOpcion FROM dbo.EncuestaVotos v
                 WHERE v.codigoEncuesta = e.codigoEncuesta
                   AND v.codigoUsuario  = @codigoUsuario), 0) AS miOpcion
    FROM dbo.vwEncuestas e
    WHERE e.opciones > 0
      AND e.estado = N'Abierta'
      AND (@campanaSlug IS NULL OR e.campanaSlug = @campanaSlug)
      AND (@campanaSlug IS NOT NULL OR EXISTS (
              SELECT 1 FROM dbo.Campanas c
               WHERE c.codigoCampana = e.codigoCampana AND c.esActual = 1))
    ORDER BY e.fechaInicio DESC, e.codigoEncuesta DESC;
END
GO

/* Las opciones de todas las encuestas abiertas, en una sola
   consulta.

   Existe para que la portada no abra una llamada por encuesta.
   Con dos a la vista y el resto detrás de «Ver más», todas se
   dibujan en la misma respuesta, así que pedir sus opciones una
   por una serían ocho viajes para responder ocho veces la misma
   pregunta.

   Del usuario solo hace falta saber qué eligió en cada una, para
   que la tarjeta pueda marcar su respuesta. El conteo va
   completo, igual que en spEncuestaOpciones.                    */

IF OBJECT_ID('dbo.spEncuestasVigentesOpciones') IS NOT NULL
    DROP PROCEDURE dbo.spEncuestasVigentesOpciones;
GO

CREATE PROCEDURE dbo.spEncuestasVigentesOpciones
    @campanaSlug   NVARCHAR(80) = NULL,
    @codigoUsuario INT          = 0
AS
BEGIN
    SET NOCOUNT ON;

    /* Las encuestas en juego y qué eligió esta persona en cada
       una. Cero es que todavía no respondió. */
    DECLARE @encuestas TABLE (codigoEncuesta INT, miOpcion INT);

    INSERT INTO @encuestas (codigoEncuesta, miOpcion)
    SELECT e.codigoEncuesta, ISNULL(v.codigoOpcion, 0)
    FROM dbo.vwEncuestas e
    LEFT JOIN dbo.EncuestaVotos v
           ON v.codigoEncuesta = e.codigoEncuesta
          AND v.codigoUsuario  = @codigoUsuario
    WHERE e.opciones > 0
      AND e.estado = N'Abierta'
      AND (@campanaSlug IS NULL OR e.campanaSlug = @campanaSlug)
      AND (@campanaSlug IS NOT NULL OR EXISTS (
              SELECT 1 FROM dbo.Campanas c
               WHERE c.codigoCampana = e.codigoCampana AND c.esActual = 1));

    SELECT
        o.codigoEncuesta,
        o.codigoOpcion,
        o.texto,
        o.orden,
        (SELECT COUNT(*) FROM dbo.EncuestaVotos v
          WHERE v.codigoOpcion = o.codigoOpcion)                AS votos,
        CASE WHEN o.codigoOpcion = x.miOpcion THEN 1 ELSE 0 END AS miVoto
    FROM dbo.EncuestaOpciones o
    INNER JOIN @encuestas x ON x.codigoEncuesta = o.codigoEncuesta
    ORDER BY o.codigoEncuesta, o.orden, o.codigoOpcion;
END
GO

/* Las opciones de una encuesta, con su resultado.

   El conteo se devuelve siempre, haya respondido o no quien
   consulta. Hubo una versión que lo reservaba hasta después de
   votar, para no empujar a nadie hacia la opción que iba
   ganando, y se quitó a pedido: una encuesta que esconde su
   resultado hasta que participes convierte el dato en un peaje.

   Lo que se conserva de aquella decisión es el aviso del pie de
   la tarjeta, que dice a quién describe el resultado. El sesgo
   de arrastre no desaparece porque se muestre el reparto, pero
   quien lee sabe de qué muestra sale.                          */

IF OBJECT_ID('dbo.spEncuestaOpciones') IS NOT NULL
    DROP PROCEDURE dbo.spEncuestaOpciones;
GO

CREATE PROCEDURE dbo.spEncuestaOpciones
    @codigoEncuesta INT,
    @codigoUsuario  INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @miOpcion INT =
        ISNULL((SELECT codigoOpcion FROM dbo.EncuestaVotos
                 WHERE codigoEncuesta = @codigoEncuesta
                   AND codigoUsuario  = @codigoUsuario), 0);

    SELECT
        o.codigoOpcion,
        o.texto,
        o.orden,
        (SELECT COUNT(*) FROM dbo.EncuestaVotos v
          WHERE v.codigoOpcion = o.codigoOpcion)               AS votos,
        CASE WHEN o.codigoOpcion = @miOpcion THEN 1 ELSE 0 END AS miVoto
    FROM dbo.EncuestaOpciones o
    WHERE o.codigoEncuesta = @codigoEncuesta
    ORDER BY o.orden, o.codigoOpcion;
END
GO

/* Registra el voto de una persona.

   Mientras la encuesta está abierta se puede cambiar de opción,
   que es el mismo criterio de las valoraciones del script 05.
   Elegir la misma opción otra vez no hace nada: a diferencia del
   me gusta, retirar el voto de una encuesta dejaría a la persona
   sin poder ver un resultado que ya vio.                        */

IF OBJECT_ID('dbo.spEncuestaVotar') IS NOT NULL
    DROP PROCEDURE dbo.spEncuestaVotar;
GO

CREATE PROCEDURE dbo.spEncuestaVotar
    @codigoEncuesta INT,
    @codigoOpcion   INT,
    @codigoUsuario  INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @estado NVARCHAR(20) =
        (SELECT estado FROM dbo.vwEncuestas WHERE codigoEncuesta = @codigoEncuesta);

    IF @estado IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La encuesta no existe.' AS mensaje;
        RETURN;
    END

    IF @estado <> N'Abierta'
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Esta encuesta ya no admite votos.' AS mensaje;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.EncuestaOpciones
                    WHERE codigoOpcion   = @codigoOpcion
                      AND codigoEncuesta = @codigoEncuesta)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La opción elegida no pertenece a esta encuesta.' AS mensaje;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios
                    WHERE codigoUsuario = @codigoUsuario AND activo = 1)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Necesitás una cuenta activa para participar.' AS mensaje;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.EncuestaVotos
                WHERE codigoEncuesta = @codigoEncuesta
                  AND codigoUsuario  = @codigoUsuario)
    BEGIN
        UPDATE dbo.EncuestaVotos
           SET codigoOpcion = @codigoOpcion,
               fecha        = SYSDATETIME()
         WHERE codigoEncuesta = @codigoEncuesta
           AND codigoUsuario  = @codigoUsuario;

        SELECT CAST(1 AS BIT) AS ok, N'Cambiaste tu respuesta.' AS mensaje;
        RETURN;
    END

    INSERT INTO dbo.EncuestaVotos (codigoEncuesta, codigoOpcion, codigoUsuario)
    VALUES (@codigoEncuesta, @codigoOpcion, @codigoUsuario);

    SELECT CAST(1 AS BIT) AS ok, N'Gracias por participar.' AS mensaje;
END
GO

/* ============================================================
   5. Administración

   Mismas reglas del script 09: fnEsAdministrador dentro de cada
   escritura aunque el Web Service ya lo haya comprobado, y una
   fila en la bitácora por cada acción.
   ============================================================ */

/* Listado para la pantalla de administración. Incluye las
   retiradas y las programadas, que es justamente lo que el
   público no ve y quien administra necesita ver.

   @estado acepta uno de los cuatro estados o vacío para todos. */

IF OBJECT_ID('dbo.spAdminEncuestas') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEncuestas;
GO

CREATE PROCEDURE dbo.spAdminEncuestas
    @codigoUsuario INT,
    @campanaSlug   NVARCHAR(80) = NULL,
    @estado        NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0 RETURN;

    SELECT
        e.codigoEncuesta,
        e.codigoCampana,
        e.campanaSlug,
        e.campana,
        e.pregunta,
        e.descripcion,
        e.codigoCategoria,
        e.categoria,
        e.fechaInicio,
        e.fechaCierre,
        e.activo,
        e.motivoBaja,
        e.estado,
        e.opciones,
        e.votos
    FROM dbo.vwEncuestas e
    WHERE (@campanaSlug IS NULL OR e.campanaSlug = @campanaSlug)
      AND (@estado      IS NULL OR e.estado      = @estado)
    ORDER BY e.fechaInicio DESC, e.codigoEncuesta DESC;
END
GO

/* Las opciones de una encuesta para la pantalla de
   administración.

   Devuelve las mismas columnas que spEncuestaOpciones, porque el
   Web Service lee las dos con el mismo lector. Se diferencia en
   que exige rol y en que no le interesa qué votó quien consulta:
   acá se mira el resultado, no se participa. */

IF OBJECT_ID('dbo.spAdminEncuestaOpciones') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEncuestaOpciones;
GO

CREATE PROCEDURE dbo.spAdminEncuestaOpciones
    @codigoUsuario  INT,
    @codigoEncuesta INT
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0 RETURN;

    SELECT
        o.codigoOpcion,
        o.texto,
        o.orden,
        (SELECT COUNT(*) FROM dbo.EncuestaVotos v
          WHERE v.codigoOpcion = o.codigoOpcion) AS votos,
        CAST(0 AS BIT)                           AS miVoto
    FROM dbo.EncuestaOpciones o
    WHERE o.codigoEncuesta = @codigoEncuesta
    ORDER BY o.orden, o.codigoOpcion;
END
GO

/* Alta y edición.

   Las opciones llegan en un solo texto, una por línea, y se
   separan acá. Va así y no como parámetro por opción porque el
   número de opciones lo decide quien escribe la pregunta, y un
   procedimiento con diez parámetros de texto obligaría a inventar
   un tope arbitrario. El orden se conserva con el ordinal de
   STRING_SPLIT, que es lo único que garantiza que la primera
   línea escrita sea la primera opción mostrada.

   Dos reglas que el procedimiento hace cumplir:

     · Las opciones solo se pueden cambiar mientras la encuesta
       no tenga votos. Cambiarlas después dejaría votos apuntando
       a una pregunta que ya no es la que se respondió.

     · Dos encuestas activas de la misma campaña no pueden estar
       abiertas a la vez. Con dos compitiendo, ninguna junta
       participación suficiente y la portada tendría que elegir
       una por su cuenta.                                        */

IF OBJECT_ID('dbo.spAdminGuardarEncuesta') IS NOT NULL
    DROP PROCEDURE dbo.spAdminGuardarEncuesta;
GO

CREATE PROCEDURE dbo.spAdminGuardarEncuesta
    @codigoUsuario   INT,
    @codigoEncuesta  INT,
    @codigoCampana   INT,
    @pregunta        NVARCHAR(300),
    @descripcion     NVARCHAR(600) = NULL,
    @codigoCategoria INT           = 0,
    @fechaInicio     DATETIME2(0),
    @fechaCierre     DATETIME2(0)  = NULL,
    @opciones        NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no tiene permiso para administrar encuestas.' AS mensaje,
               0 AS codigo;
        RETURN;
    END

    SET @pregunta    = LTRIM(RTRIM(ISNULL(@pregunta, N'')));
    SET @descripcion = NULLIF(LTRIM(RTRIM(ISNULL(@descripcion, N''))), N'');

    IF LEN(@pregunta) < 8
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Escribí la pregunta de la encuesta.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Campanas WHERE codigoCampana = @codigoCampana)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Elegí la campaña a la que pertenece la encuesta.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @fechaCierre IS NOT NULL AND @fechaCierre <= @fechaInicio
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La fecha de cierre tiene que ser posterior a la de apertura.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Las opciones, ya separadas, numeradas y sin líneas vacías.
       El CHAR(13) se quita porque un textarea del navegador manda
       el salto de línea como CR LF. */
    DECLARE @lista TABLE (orden INT, texto NVARCHAR(160));

    INSERT INTO @lista (orden, texto)
    SELECT s.ordinal, LTRIM(RTRIM(REPLACE(s.value, CHAR(13), N'')))
    FROM STRING_SPLIT(ISNULL(@opciones, N''), CHAR(10), 1) s
    WHERE LEN(LTRIM(RTRIM(REPLACE(s.value, CHAR(13), N'')))) > 0;

    DECLARE @cuantas INT = (SELECT COUNT(*) FROM @lista);

    IF @cuantas < 2
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Una encuesta necesita al menos dos opciones, una por línea.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF @cuantas > 8
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Ocho opciones es el máximo: con más, la tarjeta deja de leerse.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    IF EXISTS (SELECT texto FROM @lista GROUP BY texto HAVING COUNT(*) > 1)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'Hay dos opciones con el mismo texto.' AS mensaje, 0 AS codigo;
        RETURN;
    END

    /* Varias encuestas pueden estar abiertas a la vez en la misma
       campaña. Hubo una restricción que lo impedía, con el
       argumento de que dos compitiendo se reparten la
       participación: se quitó porque la portada ahora muestra
       varias, con las dos primeras a la vista y el resto detrás de
       «Ver más». Lo que era una regla de negocio pasó a ser una
       decisión de presentación, que es donde le corresponde estar. */

    DECLARE @nuevo INT = @codigoEncuesta;
    DECLARE @accion NVARCHAR(40) = N'Edicion';
    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Encuesta');

    BEGIN TRY
        BEGIN TRANSACTION;

        IF ISNULL(@codigoEncuesta, 0) = 0
        BEGIN
            SET @accion = N'Alta';

            INSERT INTO dbo.Encuestas
                (codigoCampana, pregunta, descripcion, codigoCategoria,
                 fechaInicio, fechaCierre, codigoUsuarioAlta)
            VALUES
                (@codigoCampana, @pregunta, @descripcion, NULLIF(@codigoCategoria, 0),
                 @fechaInicio, @fechaCierre, @codigoUsuario);

            SET @nuevo = CAST(SCOPE_IDENTITY() AS INT);
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.Encuestas WHERE codigoEncuesta = @codigoEncuesta)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT CAST(0 AS BIT) AS ok, N'La encuesta no existe.' AS mensaje, 0 AS codigo;
                RETURN;
            END

            UPDATE dbo.Encuestas
               SET codigoCampana   = @codigoCampana,
                   pregunta        = @pregunta,
                   descripcion     = @descripcion,
                   codigoCategoria = NULLIF(@codigoCategoria, 0),
                   fechaInicio     = @fechaInicio,
                   fechaCierre     = @fechaCierre
             WHERE codigoEncuesta = @codigoEncuesta;
        END

        /* Las opciones se reescriben solo si nadie votó todavía.
           Con votos, el texto de la pregunta y las fechas se
           pueden corregir, las opciones no. */
        IF NOT EXISTS (SELECT 1 FROM dbo.EncuestaVotos WHERE codigoEncuesta = @nuevo)
        BEGIN
            DELETE FROM dbo.EncuestaOpciones WHERE codigoEncuesta = @nuevo;

            INSERT INTO dbo.EncuestaOpciones (codigoEncuesta, texto, orden)
            SELECT @nuevo, l.texto, l.orden FROM @lista l;
        END

        INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, detalle)
        VALUES (@codigoUsuario, @accion, @tipo, @nuevo,
                LEFT(N'Encuesta: ' + @pregunta, 300));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        SELECT CAST(0 AS BIT) AS ok,
               N'No se pudo guardar la encuesta: ' + ERROR_MESSAGE() AS mensaje, 0 AS codigo;
        RETURN;
    END CATCH

    DECLARE @conVotos BIT =
        CASE WHEN EXISTS (SELECT 1 FROM dbo.EncuestaVotos WHERE codigoEncuesta = @nuevo)
             THEN 1 ELSE 0 END;

    SELECT CAST(1 AS BIT) AS ok,
           CASE WHEN @accion = N'Alta' THEN N'Encuesta registrada.'
                WHEN @conVotos = 1
                    THEN N'Encuesta actualizada. Las opciones no se tocaron porque ya tiene votos.'
                ELSE N'Encuesta actualizada.' END AS mensaje,
           @nuevo AS codigo;
END
GO

/* Cerrar, reabrir, retirar y restaurar.

   Cuatro acciones y no dos, porque cerrar y retirar no son lo
   mismo: una encuesta cerrada terminó su votación y sigue a la
   vista con su resultado, una retirada desaparece del sitio
   público. El motivo es obligatorio en las dos que quitan algo
   de la vista.

   Cerrar es fijar la fecha de cierre en este instante, no una
   columna aparte: dos maneras de decir lo mismo terminarían
   contradiciéndose. */

IF OBJECT_ID('dbo.spAdminEstadoEncuesta') IS NOT NULL
    DROP PROCEDURE dbo.spAdminEstadoEncuesta;
GO

CREATE PROCEDURE dbo.spAdminEstadoEncuesta
    @codigoUsuario  INT,
    @codigoEncuesta INT,
    @accion         NVARCHAR(20),
    @motivo         NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF dbo.fnEsAdministrador(@codigoUsuario) = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'La cuenta no tiene permiso para administrar encuestas.' AS mensaje;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Encuestas WHERE codigoEncuesta = @codigoEncuesta)
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La encuesta no existe.' AS mensaje;
        RETURN;
    END

    SET @accion = LOWER(LTRIM(RTRIM(ISNULL(@accion, N''))));
    SET @motivo = NULLIF(LTRIM(RTRIM(ISNULL(@motivo, N''))), N'');

    IF @accion NOT IN (N'cerrar', N'reabrir', N'retirar', N'restaurar')
    BEGIN
        SELECT CAST(0 AS BIT) AS ok, N'La acción no es válida.' AS mensaje;
        RETURN;
    END

    IF @accion IN (N'cerrar', N'retirar') AND @motivo IS NULL
    BEGIN
        SELECT CAST(0 AS BIT) AS ok,
               N'El motivo es obligatorio: queda en la bitácora junto a tu nombre.' AS mensaje;
        RETURN;
    END

    DECLARE @tipo INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Encuesta');
    DECLARE @registro NVARCHAR(40);
    DECLARE @mensaje NVARCHAR(200);

    /* El instante de cierre, truncado al segundo en lugar de
       redondeado.

       DATETIME2(0) redondea al segundo más cercano, así que guardar
       SYSDATETIME() directamente puede dejar la fecha de cierre medio
       segundo en el futuro — y durante esa fracción, vwEncuestas
       sigue diciendo que la encuesta está abierta. La pantalla que
       acaba de confirmar el cierre mostraba «Abierta» en la misma
       respuesta. Convertir a texto con el estilo 126 corta los
       decimales sin redondear. */
    DECLARE @ahora DATETIME2(0) =
        CONVERT(DATETIME2(0), CONVERT(VARCHAR(19), SYSDATETIME(), 126));

    IF @accion = N'cerrar'
    BEGIN
        UPDATE dbo.Encuestas
           SET fechaCierre = @ahora
         WHERE codigoEncuesta = @codigoEncuesta;

        SET @registro = N'Cierre';
        /* El mensaje dice dónde queda el resultado, y no que quede
           «visible para todo el mundo»: la portada solo muestra las
           abiertas, así que al cerrarla sale de ahí y se consulta
           desde esta misma pantalla. */
        SET @mensaje  = N'Encuesta cerrada. Sale de la portada y su resultado se consulta desde acá.';
    END
    ELSE IF @accion = N'reabrir'
    BEGIN
        UPDATE dbo.Encuestas
           SET fechaCierre = NULL
         WHERE codigoEncuesta = @codigoEncuesta;

        SET @registro = N'Reapertura';
        SET @mensaje  = N'Encuesta reabierta. Vuelve a admitir votos.';
    END
    ELSE IF @accion = N'retirar'
    BEGIN
        UPDATE dbo.Encuestas
           SET activo            = 0,
               motivoBaja        = @motivo,
               fechaBaja         = @ahora,
               codigoUsuarioBaja = @codigoUsuario
         WHERE codigoEncuesta = @codigoEncuesta;

        SET @registro = N'Retiro';
        SET @mensaje  = N'Encuesta retirada del sitio público. Los votos se conservan.';
    END
    ELSE
    BEGIN
        UPDATE dbo.Encuestas
           SET activo            = 1,
               motivoBaja        = NULL,
               fechaBaja         = NULL,
               codigoUsuarioBaja = NULL
         WHERE codigoEncuesta = @codigoEncuesta;

        SET @registro = N'Restauracion';
        SET @mensaje  = N'Encuesta restaurada.';
    END

    INSERT INTO dbo.Auditoria (codigoUsuario, accion, codigoTipoObjeto, codigoObjeto, motivo)
    VALUES (@codigoUsuario, @registro, @tipo, @codigoEncuesta, @motivo);

    SELECT CAST(1 AS BIT) AS ok, @mensaje AS mensaje;
END
GO

/* ============================================================
   6. Alcance del asistente

   El login cumplehn_ia no pertenece a db_datareader y no tiene
   permiso sobre ninguna tabla, así que las tres tablas de este
   script le quedan fuera de alcance sin hacer nada. El DENY se
   escribe igual, por la misma razón que los del script 13:
   EncuestaVotos vincula a una persona con una preferencia, que
   es la clase de dato que más daño haría filtrado, y un DENY
   sobrevive a que alguien conceda un permiso amplio por
   descuido.

   No se concede EXECUTE sobre ningún procedimiento de acá. El
   asistente todavía no responde sobre encuestas: cuando lo haga,
   será con un procedimiento propio que devuelva solo agregados,
   nunca quién votó qué.
   ============================================================ */

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'cumplehn_ia')
BEGIN
    DENY SELECT ON dbo.EncuestaVotos TO cumplehn_ia;
    DENY SELECT ON dbo.Encuestas     TO cumplehn_ia;
END
GO

PRINT 'Script 14 aplicado: encuestas de percepcion.';
GO
