/* ============================================================
   CumpleHN — 15. Datos de demostración de las encuestas
   ------------------------------------------------------------
   Deja una encuesta abierta con sus opciones y algunos votos,
   para poder ver el módulo funcionando sin tener que registrarla
   a mano cada vez que se reconstruye la base.

   DATOS DE PRUEBA. Los votos se reparten de forma determinista
   entre las cuentas ciudadanas del script 06 — no simulan
   opinión real, solo dejan la tarjeta con algo que mostrar.

   Este script es prescindible. La plataforma funciona sin él: la
   encuesta se registra desde Administración → Encuestas.

   El alta no se hace con INSERT sino llamando al procedimiento,
   para que el script de datos pase por las mismas validaciones
   que la pantalla y no pueda dejar una fila que la aplicación
   nunca habría aceptado.

   No hace nada si ya hay encuestas cargadas.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF EXISTS (SELECT 1 FROM dbo.Encuestas)
BEGIN
    PRINT 'Ya hay encuestas cargadas. No se hizo nada.';
    RETURN;
END
GO

SET NOCOUNT ON;

DECLARE @admin   INT = (SELECT codigoUsuario FROM dbo.Usuarios WHERE login = N'admin');
DECLARE @campana INT = (SELECT TOP (1) codigoCampana FROM dbo.Campanas WHERE esActual = 1);

IF @admin IS NULL OR @campana IS NULL
BEGIN
    PRINT 'Falta la cuenta admin o la campana destacada. Ejecuta antes el script 04.';
    RETURN;
END

/* La pregunta contrasta con el eje que ya usa el tablero: el
   interés ciudadano por categoría que se midió en la encuesta
   del proyecto (n = 150). Con la encuesta viva, ese eje deja de
   depender de una cifra fija de junio.

   Las opciones repiten las seis categorías de la taxonomía COFOG
   y suman «Ninguna en particular», para que quien no priorice
   ninguna no quede obligado a elegir una. */

DECLARE @opciones NVARCHAR(MAX) =
    N'Seguridad y orden público' + CHAR(10) +
    N'Salud' + CHAR(10) +
    N'Educación' + CHAR(10) +
    N'Economía y empleo' + CHAR(10) +
    N'Infraestructura' + CHAR(10) +
    N'Transparencia y gobernanza' + CHAR(10) +
    N'Ninguna en particular';

DECLARE @resultado TABLE (ok BIT, mensaje NVARCHAR(300), codigo INT);

INSERT INTO @resultado (ok, mensaje, codigo)
EXEC dbo.spAdminGuardarEncuesta
    @codigoUsuario   = @admin,
    @codigoEncuesta  = 0,
    @codigoCampana   = @campana,
    @pregunta        = N'¿Cuál debería ser la prioridad del próximo gobierno?',
    @descripcion     = N'Elegí el área que considerás más urgente. El resultado se muestra al votar, y sirve para contrastar lo que la ciudadanía prioriza con lo que las candidaturas están proponiendo.',
    @codigoCategoria = 0,
    @fechaInicio     = '2026-09-01 08:00:00',
    @fechaCierre     = NULL,
    @opciones        = @opciones;

DECLARE @encuesta INT = (SELECT TOP (1) codigo FROM @resultado);

IF ISNULL(@encuesta, 0) = 0
BEGIN
    PRINT 'No se pudo registrar la encuesta:';
    SELECT mensaje FROM @resultado;
    RETURN;
END

/* Votos de las cuentas ciudadanas del script 06. Cada persona
   vota la opción que le toca por su número de fila, de modo que
   el reparto sea distinto entre opciones y reproducible. */

DECLARE @ciudadanos TABLE (fila INT, codigoUsuario INT);

INSERT INTO @ciudadanos (fila, codigoUsuario)
SELECT ROW_NUMBER() OVER (ORDER BY u.codigoUsuario), u.codigoUsuario
FROM dbo.Usuarios u
INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol
WHERE r.nombre = N'Ciudadano' AND u.activo = 1;

DECLARE @lista TABLE (fila INT, codigoOpcion INT);

INSERT INTO @lista (fila, codigoOpcion)
SELECT ROW_NUMBER() OVER (ORDER BY o.orden), o.codigoOpcion
FROM dbo.EncuestaOpciones o
WHERE o.codigoEncuesta = @encuesta;

DECLARE @nOpciones INT = (SELECT COUNT(*) FROM @lista);

INSERT INTO dbo.EncuestaVotos (codigoEncuesta, codigoOpcion, codigoUsuario, fecha)
SELECT @encuesta, l.codigoOpcion, c.codigoUsuario,
       DATEADD(HOUR, -((c.fila * 7) % 60), SYSDATETIME())
FROM @ciudadanos c
INNER JOIN @lista l
        ON l.fila = ((c.fila * c.fila) % @nOpciones) + 1;

PRINT 'Script 15 aplicado: encuesta de demostracion registrada.';
GO
