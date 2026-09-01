/* ============================================================
   CumpleHN — 06. Datos de demostración de la interacción
   ------------------------------------------------------------
   Crea cuentas ciudadanas de prueba y siembra valoraciones y
   comentarios, para que la interacción se pueda ver funcionando
   sin tener que registrar personas a mano.

   DATOS DE PRUEBA. Las personas son ficticias.
   Contraseña de todas las cuentas: CumpleHN2026

   El script no hace nada si ya hay valoraciones cargadas.
   ============================================================ */

USE BDCUMPLEHN;
GO

IF EXISTS (SELECT 1 FROM dbo.Valoraciones)
BEGIN
    PRINT 'Ya hay interaccion cargada. No se hizo nada.';
    RETURN;
END

SET NOCOUNT ON;

DECLARE @hoy DATETIME2(0) = SYSDATETIME();
DECLARE @rolCiudadano INT = (SELECT codigoRol FROM dbo.Roles WHERE nombre = N'Ciudadano');
DECLARE @clave CHAR(64) =
    LOWER(CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), 'CumpleHN2026')), 2));

/* ================================== CUENTAS CIUDADANAS DE PRUEBA */

INSERT INTO dbo.Usuarios (login, clave, nombre, correo, codigoRol, codigoCandidato)
SELECT v.login, @clave, v.nombre, v.correo, @rolCiudadano, NULL
FROM (VALUES
    (N'jlopez',    N'José Alberto López',     N'jlopez@correo.hn'),
    (N'mramirez',  N'María Fernanda Ramírez', N'mramirez@correo.hn'),
    (N'cflores',   N'Carlos Flores',          N'cflores@correo.hn'),
    (N'aportillo', N'Andrea Portillo',        N'aportillo@correo.hn'),
    (N'dsierra',   N'Daniel Sierra',          N'dsierra@correo.hn'),
    (N'kmejia',    N'Karla Mejía',            N'kmejia@correo.hn'),
    (N'rvasquez',  N'Roberto Vásquez',        N'rvasquez@correo.hn'),
    (N'lmartinez', N'Lucía Martínez',         N'lmartinez@correo.hn'),
    (N'oaguilar',  N'Óscar Aguilar',          N'oaguilar@correo.hn'),
    (N'pzelaya',   N'Patricia Zelaya',        N'pzelaya@correo.hn')
) AS v (login, nombre, correo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuarios u WHERE u.login = v.login);

/* Códigos de tipo de objeto */
DECLARE @tPublicacion INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Publicacion');
DECLARE @tCandidato   INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Candidato');
DECLARE @tPartido     INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Partido');
DECLARE @tPropuesta   INT = (SELECT codigoTipoObjeto FROM dbo.TiposObjeto WHERE nombre = N'Propuesta');

/* Los diez ciudadanos, numerados para poder repartir los votos */
DECLARE @ciudadanos TABLE (fila INT, codigoUsuario INT);
INSERT INTO @ciudadanos (fila, codigoUsuario)
SELECT ROW_NUMBER() OVER (ORDER BY u.codigoUsuario), u.codigoUsuario
FROM dbo.Usuarios u
INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol
WHERE r.nombre = N'Ciudadano';

/* ==================================================== VALORACIONES

   Se reparten de forma determinista: cada ciudadano valora los objetos
   cuyo código combinado con su número de fila da residuo par, y el signo
   depende de otro residuo. No busca simular opinión real, solo dejar
   contadores distintos de cero en toda la plataforma.                */

-- Publicaciones
INSERT INTO dbo.Valoraciones (codigoTipoObjeto, codigoObjeto, codigoUsuario, valor, fecha)
SELECT @tPublicacion, b.codigoPublicacion, c.codigoUsuario,
       CASE WHEN (b.codigoPublicacion + c.fila) % 5 = 0 THEN -1 ELSE 1 END,
       DATEADD(HOUR, -((b.codigoPublicacion * 3 + c.fila) % 72), @hoy)
FROM dbo.Publicaciones b
CROSS JOIN @ciudadanos c
WHERE (b.codigoPublicacion * 2 + c.fila) % 3 <> 0;

-- Candidatos
INSERT INTO dbo.Valoraciones (codigoTipoObjeto, codigoObjeto, codigoUsuario, valor, fecha)
SELECT @tCandidato, k.codigoCandidato, c.codigoUsuario,
       CASE WHEN (k.codigoCandidato + c.fila) % 4 = 0 THEN -1 ELSE 1 END,
       DATEADD(HOUR, -((k.codigoCandidato * 5 + c.fila) % 120), @hoy)
FROM dbo.Candidatos k
CROSS JOIN @ciudadanos c
WHERE (k.codigoCandidato + c.fila * 2) % 3 <> 1;

-- Partidos
INSERT INTO dbo.Valoraciones (codigoTipoObjeto, codigoObjeto, codigoUsuario, valor, fecha)
SELECT @tPartido, p.codigoPartido, c.codigoUsuario,
       CASE WHEN (p.codigoPartido + c.fila) % 3 = 0 THEN -1 ELSE 1 END,
       DATEADD(HOUR, -((p.codigoPartido * 7 + c.fila) % 200), @hoy)
FROM dbo.Partidos p
CROSS JOIN @ciudadanos c
WHERE (p.codigoPartido * 3 + c.fila) % 4 <> 2;

-- Propuestas
INSERT INTO dbo.Valoraciones (codigoTipoObjeto, codigoObjeto, codigoUsuario, valor, fecha)
SELECT @tPropuesta, r.codigoPropuesta, c.codigoUsuario,
       CASE WHEN (r.codigoPropuesta + c.fila) % 6 = 0 THEN -1 ELSE 1 END,
       DATEADD(HOUR, -((r.codigoPropuesta * 2 + c.fila) % 150), @hoy)
FROM dbo.Propuestas r
CROSS JOIN @ciudadanos c
WHERE (r.codigoPropuesta + c.fila) % 3 <> 2;

/* ===================================================== COMENTARIOS */

DECLARE @u1 INT = (SELECT codigoUsuario FROM @ciudadanos WHERE fila = 1);
DECLARE @u2 INT = (SELECT codigoUsuario FROM @ciudadanos WHERE fila = 2);
DECLARE @u3 INT = (SELECT codigoUsuario FROM @ciudadanos WHERE fila = 3);
DECLARE @u4 INT = (SELECT codigoUsuario FROM @ciudadanos WHERE fila = 4);
DECLARE @u5 INT = (SELECT codigoUsuario FROM @ciudadanos WHERE fila = 5);

DECLARE @pPortal   INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Portal de compromisos con metas medibles');
DECLARE @pMedicam  INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Abastecimiento continuo de medicamentos esenciales');
DECLARE @pAgua     INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Agua potable continua en colonias sin servicio');
DECLARE @pEmpleo   INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Programa de primer empleo formal para jóvenes');

DECLARE @kAna    INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'ana-molina-caballero');
DECLARE @kGabi   INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'gabriela-fonseca-ordonez');
DECLARE @kHector INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'hector-paz-mejia');

INSERT INTO dbo.Comentarios (codigoTipoObjeto, codigoObjeto, codigoUsuario, texto, fecha)
VALUES
/* Sobre propuestas */
(@tPropuesta, @pPortal, @u1,
 N'Me parece la propuesta más concreta que he leído acá. Lo importante va a ser que el portal siga actualizado después del primer año, no solo al inicio.',
 DATEADD(HOUR, -30, @hoy)),
(@tPropuesta, @pPortal, @u3,
 N'¿Quién verifica que los datos publicados en ese portal sean correctos? Sin auditoría externa cualquiera puede reportar el avance que le convenga.',
 DATEADD(HOUR, -22, @hoy)),
(@tPropuesta, @pMedicam, @u2,
 N'En el hospital de mi ciudad el desabastecimiento es constante. Si el inventario fuera público al menos uno sabría antes de perder el viaje.',
 DATEADD(HOUR, -47, @hoy)),
(@tPropuesta, @pMedicam, @u5,
 N'La meta de menos del 10 por ciento me suena razonable. Ojalá se publique el dato mes a mes y no solo al final del período.',
 DATEADD(HOUR, -19, @hoy)),
(@tPropuesta, @pAgua, @u4,
 N'En mi colonia el agua llega dos veces por semana. Medir las horas de servicio por sector sería el primer paso para poder reclamar con datos.',
 DATEADD(HOUR, -55, @hoy)),
(@tPropuesta, @pEmpleo, @u1,
 N'Cincuenta mil empleos formales en cuatro años es una cifra alta. Me gustaría ver de dónde saldría el incentivo y cuánto costaría al presupuesto.',
 DATEADD(HOUR, -12, @hoy)),

/* Sobre candidatos */
(@tCandidato, @kAna, @u2,
 N'Es la única candidatura que hasta ahora puso metas con número y plazo en todas sus propuestas. Eso ya permite compararla más adelante.',
 DATEADD(HOUR, -40, @hoy)),
(@tCandidato, @kAna, @u4,
 N'Buen perfil en el papel. Me quedo esperando a ver si las fuentes que respalden cada compromiso aparecen o no.',
 DATEADD(HOUR, -16, @hoy)),
(@tCandidato, @kGabi, @u3,
 N'Priorizar agua y saneamiento antes que obra vistosa me parece lo correcto para San Pedro Sula.',
 DATEADD(HOUR, -33, @hoy)),
(@tCandidato, @kHector, @u5,
 N'Que una candidatura independiente se comprometa a publicar cada voto con su justificación es algo que se puede verificar sin depender de nadie.',
 DATEADD(HOUR, -27, @hoy));

/* Comentarios sobre publicaciones del feed */
INSERT INTO dbo.Comentarios (codigoTipoObjeto, codigoObjeto, codigoUsuario, texto, fecha)
SELECT @tPublicacion, b.codigoPublicacion, @u1,
       N'Gracias por publicar el detalle. Sería útil que quedara el enlace al documento completo.',
       DATEADD(HOUR, -4, b.fecha)
FROM dbo.Publicaciones b
WHERE b.codigoPublicacion % 2 = 1;

INSERT INTO dbo.Comentarios (codigoTipoObjeto, codigoObjeto, codigoUsuario, texto, fecha)
SELECT @tPublicacion, b.codigoPublicacion, @u4,
       N'Seguimos pendientes de la medición que mencionan. Ojalá se publique completa y no solo un resumen.',
       DATEADD(HOUR, -2, b.fecha)
FROM dbo.Publicaciones b
WHERE b.codigoPublicacion % 3 = 0;

/* Comentario sobre un partido */
INSERT INTO dbo.Comentarios (codigoTipoObjeto, codigoObjeto, codigoUsuario, texto, fecha)
SELECT TOP (1) @tPartido, p.codigoPartido, @u2,
       N'Me interesa ver el plan de gobierno completo del partido y no solo las propuestas de cada candidatura por separado.',
       DATEADD(HOUR, -8, @hoy)
FROM dbo.Partidos p
ORDER BY p.codigoPartido;

PRINT 'Interaccion de demostracion cargada.';
GO
