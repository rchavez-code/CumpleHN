/* ============================================================
   CumpleHN — 26. Reporte de las consultas al asistente
   ------------------------------------------------------------
   Quien le preguntó al asistente puede volver a sus consultas,
   elegir las que le sirven y exportarlas como un reporte con la
   pregunta, la respuesta y la fecha de cada una.

   No agrega tablas: todo sale de ConsultasIA (script 12), que ya
   guarda cada pregunta con su respuesta.

   Reglas:

     1. Cada persona ve solo sus consultas. El filtro por
        codigoUsuario va dentro de los dos procedimientos, no en
        el Web Service: pedir el reporte con códigos de consulta
        ajenos devuelve solo las propias, y las ajenas se
        descartan en silencio.
     2. Solo entran las consultas respondidas. Una que falló no
        tiene respuesta, y en un reporte sería una pregunta
        huérfana.
     3. El reporte va en orden cronológico, de la más antigua a
        la más reciente, para que se lea como la conversación que
        fue. El historial para elegir va al revés, porque lo que
        se busca casi siempre es lo último que se preguntó.
     4. Ningún procedimiento de este script se le concede a
        cumplehn_ia. ConsultasIA vincula a una persona con lo que
        preguntó sobre política, y el 13 le niega esa tabla al
        asistente. Los usa el backend con su conexión normal.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. Historial para elegir

   Solo la pregunta y la fecha. La respuesta puede ocupar varios
   kilobytes cada una, y para elegir no hace falta.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spIAHistorialUsuario
    @codigoUsuario INT,
    @maximo        INT = 200
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@maximo)
           c.codigoConsulta,
           c.pregunta,
           c.fecha
      FROM dbo.ConsultasIA c
     WHERE c.codigoUsuario = @codigoUsuario
       AND c.respondio = 1
     ORDER BY c.fecha DESC, c.codigoConsulta DESC;
END;
GO

/* ============================================================
   2. Contenido del reporte

   @codigos es la lista de consultas elegidas, separadas por
   coma. Un valor que no sea entero se descarta con TRY_CONVERT
   en lugar de hacer fallar el reporte entero.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spIAReporteConsultas
    @codigoUsuario INT,
    @codigos       NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.codigoConsulta,
           c.pregunta,
           c.respuesta,
           c.fecha
      FROM dbo.ConsultasIA c
     WHERE c.codigoUsuario = @codigoUsuario
       AND c.respondio = 1
       AND c.codigoConsulta IN (SELECT TRY_CONVERT(INT, s.value)
                                  FROM STRING_SPLIT(@codigos, ',') s)
     ORDER BY c.fecha, c.codigoConsulta;
END;
GO
