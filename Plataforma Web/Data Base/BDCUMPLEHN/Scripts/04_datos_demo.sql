/* ============================================================
   CumpleHN — 04. Datos de demostración
   ------------------------------------------------------------
   DATOS DE PRUEBA. Sirven para validar la plataforma mientras no
   hay contenido real.

   Las personas y los partidos son FICTICIOS a propósito. Usar
   partidos reales con candidatos inventados haría que la
   plataforma insinuara afiliaciones que nadie declaró, y eso
   contradice su carácter neutral.

   El script no hace nada si ya hay campañas cargadas.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* El guard y las inserciones van en el mismo lote a propósito: RETURN solo
   corta el lote en que aparece, así que un GO acá dejaría pasar todo lo
   que sigue y duplicaría los datos. */
IF EXISTS (SELECT 1 FROM dbo.Campanas)
BEGIN
    PRINT 'Ya hay datos cargados. No se hizo nada.';
    RETURN;
END

SET NOCOUNT ON;

DECLARE @hoy DATETIME2(0) = SYSDATETIME();

/* Códigos de catálogo, resueltos una sola vez por nombre. */
DECLARE @vDeclarado  INT = (SELECT codigoVerificacion FROM dbo.NivelesVerificacion WHERE nombre = N'Declarado');
DECLARE @vRevision   INT = (SELECT codigoVerificacion FROM dbo.NivelesVerificacion WHERE nombre = N'En revisión');
DECLARE @vVerificado INT = (SELECT codigoVerificacion FROM dbo.NivelesVerificacion WHERE nombre = N'Verificado');
DECLARE @eDeclarada  INT = (SELECT codigoEstado FROM dbo.EstadosPropuesta WHERE nombre = N'Declarada');

DECLARE @rolAdmin     INT = (SELECT codigoRol FROM dbo.Roles WHERE nombre = N'Administrador');
DECLARE @rolCandidato INT = (SELECT codigoRol FROM dbo.Roles WHERE nombre = N'Candidato');

/* Contraseña de todas las cuentas de prueba: CumpleHN2026
   Se calcula igual que en el backend: SHA-256 en hexadecimal minúscula. */
DECLARE @claveDemo CHAR(64) =
    LOWER(CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), 'CumpleHN2026')), 2));

/* =========================================== CAMPAÑAS */

INSERT INTO dbo.Campanas (slug, nombre, resumen, descripcion, alcance, fechaInicio, fechaEleccion, estado, esActual)
VALUES
(N'generales-2029', N'Elecciones Generales 2029',
 N'Presidencia de la República, Congreso Nacional y corporaciones municipales.',
 N'Proceso electoral en el que se eligen la Presidencia de la República, las 128 diputaciones al Congreso Nacional, las diputaciones al Parlamento Centroamericano y las 298 corporaciones municipales del país. En CumpleHN el registro de candidaturas y el seguimiento de propuestas permanecen abiertos durante todo el ciclo, de modo que los compromisos queden documentados desde el momento en que se formulan y no solo al final de la campaña.',
 N'Nacional, departamental y municipal', '2026-03-01', '2029-11-25', N'Activa', 1),

(N'primarias-2029', N'Elecciones Primarias 2029',
 N'Elección interna de las candidaturas de cada partido político.',
 N'Proceso interno mediante el cual cada partido político define las candidaturas que presentará en las elecciones generales. El registro de precandidaturas en la plataforma abre con la convocatoria oficial.',
 N'Interno de cada partido político', '2028-09-03', '2029-03-11', N'Proxima', 0),

(N'generales-2025', N'Elecciones Generales 2025',
 N'Ciclo cerrado. Disponible para consultar el cumplimiento de lo prometido.',
 N'Ciclo electoral concluido. Su valor en la plataforma es el seguimiento: las propuestas registradas durante la campaña quedan disponibles para contrastarlas con lo que efectivamente se ejecutó, con las evidencias que respaldan cada estado de cumplimiento.',
 N'Nacional, departamental y municipal', '2025-01-20', '2025-11-30', N'Cerrada', 0);

DECLARE @cGenerales2029 INT = (SELECT codigoCampana FROM dbo.Campanas WHERE slug = N'generales-2029');

/* ========================================= CANDIDATOS */

DECLARE @cargoPresidencia INT = (SELECT codigoCargo FROM dbo.Cargos WHERE nombre = N'Presidencia de la República');
DECLARE @cargoDiputacion  INT = (SELECT codigoCargo FROM dbo.Cargos WHERE nombre = N'Diputación al Congreso Nacional');
DECLARE @cargoAlcaldia    INT = (SELECT codigoCargo FROM dbo.Cargos WHERE nombre = N'Alcaldía municipal');

DECLARE @depFM        INT = (SELECT codigoDepartamento FROM dbo.Departamentos WHERE nombre = N'Francisco Morazán');
DECLARE @depCortes    INT = (SELECT codigoDepartamento FROM dbo.Departamentos WHERE nombre = N'Cortés');
DECLARE @depCholuteca INT = (SELECT codigoDepartamento FROM dbo.Departamentos WHERE nombre = N'Choluteca');

INSERT INTO dbo.Candidatos
    (slug, codigoCampana, nombres, apellidos, partido, partidoSiglas, codigoCargo,
     codigoDepartamento, municipio, titular, biografia, informacionProfesional,
     descripcionCandidatura, correoPublico, sitioWeb, facebook, x, instagram, codigoVerificacion)
VALUES
(N'ana-molina-caballero', @cGenerales2029, N'Ana Lucía', N'Molina Caballero',
 N'Movimiento Cívico Nacional', N'MCN', @cargoPresidencia, @depFM, NULL,
 N'Gestión pública abierta y datos verificables como política de Estado.',
 N'Economista con veinte años de trayectoria en administración pública y evaluación de programas sociales. Ha coordinado unidades de planificación en el nivel central y acompañado procesos de presupuesto abierto en gobiernos locales.',
 N'Licenciatura en Economía. Maestría en Políticas Públicas. Docente universitaria en evaluación de programas.',
 N'Una candidatura centrada en que cada compromiso público tenga una meta medible, un responsable con nombre y un plazo verificable, publicados desde el primer día.',
 N'contacto@analuciamolina.hn', N'https://analuciamolina.hn', N'analuciamolinahn', N'analuciamolina', NULL, @vVerificado),

(N'rodrigo-nunez-berrios', @cGenerales2029, N'Rodrigo', N'Núñez Berríos',
 N'Partido Progreso Ciudadano', N'PPC', @cargoPresidencia, @depCortes, NULL,
 N'Empleo formal, seguridad en los barrios y salud que no se caiga a pedazos.',
 N'Ingeniero civil y empresario del sector construcción. Fue presidente de una cámara de comercio local y participó en mesas de seguridad ciudadana en el norte del país.',
 N'Ingeniería Civil. Especialización en gestión de proyectos.',
 N'Propuestas medibles en generación de empleo formal, reducción de homicidios y abastecimiento hospitalario, con metas anuales publicadas.',
 N'prensa@rodrigonunez.hn', NULL, N'rodrigonunezhn', NULL, NULL, @vVerificado),

(N'carmen-villeda-lopez', @cGenerales2029, N'Carmen', N'Villeda López',
 N'Alianza Democrática Hondureña', N'ADH', @cargoAlcaldia, @depFM, N'Distrito Central',
 N'La ciudad se arregla cuadra por cuadra, con presupuesto a la vista.',
 N'Arquitecta y gestora urbana. Trabajó doce años en planificación municipal y en programas de mejoramiento de barrios en la capital.',
 N'Arquitectura. Maestría en Planificación Urbana.',
 N'Un plan de ciudad con obras georreferenciadas, costos publicados y avance actualizado cada trimestre.',
 N'carmen@villedaparalaciudad.hn', NULL, NULL, NULL, N'villedaparalaciudad', @vRevision),

(N'hector-paz-mejia', @cGenerales2029, N'Héctor', N'Paz Mejía',
 NULL, NULL, @cargoDiputacion, @depFM, NULL,
 N'Candidatura independiente por la transparencia legislativa.',
 N'Abogado especializado en derecho administrativo. Ha litigado solicitudes de acceso a la información pública y acompañado a organizaciones civiles en auditorías sociales.',
 N'Licenciatura en Derecho. Especialidad en Derecho Administrativo.',
 N'Voto público razonado, agenda legislativa abierta y rendición de cuentas del uso del fondo departamental.',
 N'hector.paz@independiente.hn', NULL, NULL, NULL, NULL, @vDeclarado),

(N'gabriela-fonseca-ordonez', @cGenerales2029, N'Gabriela', N'Fonseca Ordóñez',
 N'Frente Renovador', N'FR', @cargoAlcaldia, @depCortes, N'San Pedro Sula',
 N'Agua potable, drenaje y escuelas dignas antes que obra vistosa.',
 N'Médica salubrista con experiencia en atención primaria y coordinación de brigadas en comunidades del valle de Sula.',
 N'Doctorado en Medicina. Maestría en Salud Pública.',
 N'Prioridad al saneamiento básico, la red de agua y la infraestructura escolar, con indicadores de cobertura publicados por colonia.',
 N'contacto@gabrielafonseca.hn', NULL, N'gabrielafonsecahn', N'gfonsecahn', NULL, @vVerificado),

(N'marlon-castellanos-rivas', @cGenerales2029, N'Marlon', N'Castellanos Rivas',
 N'Partido Progreso Ciudadano', N'PPC', @cargoDiputacion, @depCholuteca, NULL,
 N'El sur también cuenta: riego, carretera y empleo joven.',
 N'Ingeniero agrónomo. Dirigió cooperativas de productores en la zona sur y programas de tecnificación de riego.',
 N'Ingeniería Agronómica. Diplomado en Desarrollo Rural.',
 N'Agenda legislativa enfocada en infraestructura productiva del sur y en empleo para población joven rural.',
 N'marlon@castellanosporelsur.hn', NULL, NULL, NULL, NULL, @vDeclarado);

DECLARE @kAna     INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'ana-molina-caballero');
DECLARE @kRodrigo INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'rodrigo-nunez-berrios');
DECLARE @kCarmen  INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'carmen-villeda-lopez');
DECLARE @kHector  INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'hector-paz-mejia');
DECLARE @kGabi    INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'gabriela-fonseca-ordonez');
DECLARE @kMarlon  INT = (SELECT codigoCandidato FROM dbo.Candidatos WHERE slug = N'marlon-castellanos-rivas');

/* =========================================== USUARIOS */

INSERT INTO dbo.Usuarios (login, clave, nombre, correo, codigoRol, codigoCandidato)
VALUES
(N'admin',   @claveDemo, N'Administrador CumpleHN', N'admin@cumplehn.hn',              @rolAdmin,     NULL),
(N'amolina', @claveDemo, N'Ana Lucía Molina Caballero', N'contacto@analuciamolina.hn', @rolCandidato, @kAna),
(N'rnunez',  @claveDemo, N'Rodrigo Núñez Berríos', N'prensa@rodrigonunez.hn',          @rolCandidato, @kRodrigo),
(N'cvilleda',@claveDemo, N'Carmen Villeda López', N'carmen@villedaparalaciudad.hn',    @rolCandidato, @kCarmen),
(N'hpaz',    @claveDemo, N'Héctor Paz Mejía', N'hector.paz@independiente.hn',          @rolCandidato, @kHector),
(N'gfonseca',@claveDemo, N'Gabriela Fonseca Ordóñez', N'contacto@gabrielafonseca.hn',  @rolCandidato, @kGabi),
(N'mcastellanos', @claveDemo, N'Marlon Castellanos Rivas', N'marlon@castellanosporelsur.hn', @rolCandidato, @kMarlon);

/* ========================================= PROPUESTAS */

DECLARE @catSeguridad      INT = (SELECT codigoCategoria FROM dbo.Categorias WHERE nombre = N'Seguridad y orden público');
DECLARE @catSalud          INT = (SELECT codigoCategoria FROM dbo.Categorias WHERE nombre = N'Salud');
DECLARE @catEducacion      INT = (SELECT codigoCategoria FROM dbo.Categorias WHERE nombre = N'Educación');
DECLARE @catEconomia       INT = (SELECT codigoCategoria FROM dbo.Categorias WHERE nombre = N'Economía y empleo');
DECLARE @catInfraestructura INT = (SELECT codigoCategoria FROM dbo.Categorias WHERE nombre = N'Infraestructura');
DECLARE @catTransparencia  INT = (SELECT codigoCategoria FROM dbo.Categorias WHERE nombre = N'Transparencia y gobernanza');

INSERT INTO dbo.Propuestas
    (codigoCandidato, codigoCampana, nombre, descripcion, problema, objetivo, beneficiarios,
     codigoCategoria, ubicacion, periodoEjecucion, codigoEstado, codigoVerificacion, fechaRegistro)
VALUES
(@kAna, @cGenerales2029, N'Portal de compromisos con metas medibles',
 N'Publicar en un portal único todos los compromisos del plan de gobierno con su meta cuantitativa, la institución responsable, el presupuesto asignado y el avance actualizado cada trimestre.',
 N'Los compromisos públicos se anuncian sin meta ni responsable, lo que hace imposible verificar después si se cumplieron.',
 N'Que el 100 % de los compromisos del plan de gobierno tenga meta, responsable, presupuesto y plazo publicados durante el primer año de gestión.',
 N'Toda la ciudadanía, con énfasis en organizaciones de auditoría social',
 @catTransparencia, NULL, N'Primer año de gestión', @eDeclarada, @vDeclarado, DATEADD(DAY, -42, @hoy)),

(@kAna, @cGenerales2029, N'Abastecimiento continuo de medicamentos esenciales',
 N'Sistema nacional de trazabilidad de inventarios que reporte en línea la existencia de medicamentos del listado básico en cada hospital y centro de salud.',
 N'El desabastecimiento de medicamentos se detecta cuando el paciente ya está en la ventanilla, sin datos previos que permitan anticiparlo.',
 N'Reducir a menos del 10 % los eventos de desabastecimiento del listado básico en hospitales públicos al cierre del segundo año.',
 N'Pacientes del sistema público de salud',
 @catSalud, NULL, N'Primeros dos años de gestión', @eDeclarada, @vDeclarado, DATEADD(DAY, -38, @hoy)),

(@kAna, @cGenerales2029, N'Presupuesto educativo con seguimiento por centro',
 N'Asignación transparente del presupuesto de infraestructura escolar, con ficha pública por centro educativo que detalle obra, monto y estado de ejecución.',
 N'No existe un registro público consolidado que permita saber cuánto se invirtió en cada centro educativo y en qué.',
 N'Ficha pública de inversión para el 100 % de centros educativos oficiales.',
 N'Comunidad educativa: estudiantes, docentes y familias',
 @catEducacion, NULL, N'2030 – 2032', @eDeclarada, @vDeclarado, DATEADD(DAY, -21, @hoy)),

(@kRodrigo, @cGenerales2029, N'Programa de primer empleo formal para jóvenes',
 N'Incentivo temporal a la contratación formal de personas de 18 a 29 años sin experiencia laboral registrada, con verificación mensual de permanencia en planilla.',
 N'La población joven entra al mercado laboral por la vía informal y sin cobertura de seguridad social.',
 N'Cincuenta mil nuevos empleos formales para jóvenes en cuatro años.',
 N'Personas de 18 a 29 años que buscan su primer empleo',
 @catEconomia, NULL, N'2030 – 2033', @eDeclarada, @vDeclarado, DATEADD(DAY, -33, @hoy)),

(@kRodrigo, @cGenerales2029, N'Plan de reducción de homicidios por municipio',
 N'Metas de reducción diferenciadas por municipio según su tasa base, con publicación mensual de cifras y evaluación semestral independiente.',
 N'Las metas de seguridad se anuncian a nivel nacional, lo que oculta municipios donde la situación empeora.',
 N'Reducir la tasa nacional de homicidios en un 25 % en cuatro años.',
 N'Población de los veinte municipios con mayor incidencia',
 @catSeguridad, N'Veinte municipios priorizados', N'2030 – 2033', @eDeclarada, @vDeclarado, DATEADD(DAY, -27, @hoy)),

(@kCarmen, @cGenerales2029, N'Mil cuadras: repavimentación con avance público',
 N'Intervención de mil cuadras priorizadas por estado de la vía y densidad de tránsito, con mapa público de avance y costo por cuadra.',
 N'La obra vial se anuncia por kilómetros totales, sin que el vecino pueda saber si su cuadra está incluida ni cuándo.',
 N'Mil cuadras repavimentadas en cuatro años, con mapa de avance actualizado.',
 N'Residentes de las colonias priorizadas del Distrito Central',
 @catInfraestructura, N'Distrito Central, Francisco Morazán', N'2030 – 2033', @eDeclarada, @vDeclarado, DATEADD(DAY, -18, @hoy)),

(@kCarmen, @cGenerales2029, N'Presupuesto municipal abierto y auditable',
 N'Publicación mensual de ingresos, egresos y contrataciones de la municipalidad en formato de datos abiertos reutilizables.',
 N'La información financiera municipal se publica en formatos que no permiten análisis ni comparación entre períodos.',
 N'Publicación mensual en datos abiertos desde el primer trimestre de gestión.',
 N'Contribuyentes del municipio y organizaciones de auditoría social',
 @catTransparencia, N'Distrito Central, Francisco Morazán', N'Primer trimestre de gestión', @eDeclarada, @vDeclarado, DATEADD(DAY, -12, @hoy)),

(@kGabi, @cGenerales2029, N'Agua potable continua en colonias sin servicio',
 N'Ampliación de la red de agua potable en colonias con abastecimiento intermitente, con medición pública de horas de servicio por sector.',
 N'Buena parte de las colonias recibe agua por horas o por cisterna, sin registro público que permita exigir continuidad.',
 N'Servicio continuo en cuarenta colonias actualmente sin cobertura estable.',
 N'Aproximadamente ciento veinte mil habitantes',
 @catInfraestructura, N'San Pedro Sula, Cortés', N'2030 – 2032', @eDeclarada, @vDeclarado, DATEADD(DAY, -24, @hoy)),

(@kGabi, @cGenerales2029, N'Red de atención primaria en salud por sector',
 N'Un equipo de atención primaria por cada sector definido, con horario extendido y reporte mensual de consultas atendidas y referencias.',
 N'La atención primaria se concentra en pocos centros, lo que traslada a los hospitales casos que podrían resolverse antes.',
 N'Cobertura de atención primaria para el 90 % de los sectores del municipio.',
 N'Población sin acceso cercano a atención primaria',
 @catSalud, N'San Pedro Sula, Cortés', N'2030 – 2033', @eDeclarada, @vDeclarado, DATEADD(DAY, -9, @hoy)),

(@kHector, @cGenerales2029, N'Voto legislativo público y razonado',
 N'Publicar el sentido de cada voto en el pleno y en comisiones, acompañado de una justificación escrita antes de la siguiente sesión.',
 N'El electorado no puede contrastar el discurso de campaña con el comportamiento legislativo posterior.',
 N'Registro público del 100 % de los votos emitidos, con justificación.',
 N'Electorado del departamento de Francisco Morazán',
 @catTransparencia, NULL, N'Todo el período legislativo', @eDeclarada, @vDeclarado, DATEADD(DAY, -6, @hoy)),

(@kMarlon, @cGenerales2029, N'Tecnificación de riego para pequeños productores del sur',
 N'Programa de riego tecnificado con asistencia técnica y seguimiento de rendimiento por parcela beneficiada.',
 N'La producción del sur depende de lluvia irregular, con pérdidas recurrentes y sin medición del efecto de los apoyos entregados.',
 N'Cinco mil hectáreas con riego tecnificado y rendimiento medido.',
 N'Pequeños productores agrícolas de la zona sur',
 @catEconomia, N'Choluteca y Valle', N'2030 – 2033', @eDeclarada, @vDeclarado, DATEADD(DAY, -4, @hoy));

/* ======================================= PUBLICACIONES */

DECLARE @pPortal   INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Portal de compromisos con metas medibles');
DECLARE @pMedicam  INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Abastecimiento continuo de medicamentos esenciales');
DECLARE @pEmpleo   INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Programa de primer empleo formal para jóvenes');
DECLARE @pCuadras  INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Mil cuadras: repavimentación con avance público');
DECLARE @pAgua     INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Agua potable continua en colonias sin servicio');
DECLARE @pPrimaria INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Red de atención primaria en salud por sector');
DECLARE @pVoto     INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Voto legislativo público y razonado');
DECLARE @pRiego    INT = (SELECT codigoPropuesta FROM dbo.Propuestas WHERE nombre = N'Tecnificación de riego para pequeños productores del sur');

INSERT INTO dbo.Publicaciones
    (codigoCandidato, codigoCampana, fecha, texto, imagenUrl, codigoCategoria,
     codigoPropuesta, codigoVerificacion, apoyos, comentarios)
VALUES
(@kAna, @cGenerales2029, DATEADD(HOUR, -3, @hoy),
 N'Publiqué la ficha completa del portal de compromisos: cada meta con su indicador, su responsable institucional y el presupuesto que requiere. Si algo de esto no se puede medir, no debería estar en un plan de gobierno. Queda abierto a revisión.',
 NULL, @catTransparencia, @pPortal, @vVerificado, 342, 57),

(@kGabi, @cGenerales2029, DATEADD(HOUR, -9, @hoy),
 N'Estuvimos en cuatro colonias del sector sur levantando el registro de horas de servicio de agua. El dato que encontramos no coincide con el reporte oficial. Vamos a publicar la medición completa por sector la próxima semana.',
 N'demo', @catInfraestructura, @pAgua, @vDeclarado, 218, 34),

(@kRodrigo, @cGenerales2029, DATEADD(HOUR, -26, @hoy),
 N'Sobre el programa de primer empleo: la meta es cincuenta mil plazas formales en cuatro años, verificables en planilla del seguro social. No cuento como empleo lo que no aparezca cotizando.',
 NULL, @catEconomia, @pEmpleo, @vVerificado, 411, 88),

(@kCarmen, @cGenerales2029, DATEADD(DAY, -2, @hoy),
 N'Ya está el mapa preliminar de las mil cuadras priorizadas, con el criterio técnico de selección a la vista: estado de la vía, tránsito diario y población servida. Si su cuadra no aparece, el formulario de observación está abierto.',
 N'demo', @catInfraestructura, @pCuadras, @vDeclarado, 176, 62),

(@kHector, @cGenerales2029, DATEADD(HOUR, -77, @hoy),
 N'Como candidatura independiente no tengo estructura de partido, así que mi compromiso es el que puedo sostener solo: publicar cada voto y su razón. Empiezo desde ahora con mi posición sobre los proyectos de ley en discusión.',
 NULL, @catTransparencia, @pVoto, @vDeclarado, 129, 41),

(@kAna, @cGenerales2029, DATEADD(HOUR, -103, @hoy),
 N'El desabastecimiento de medicamentos no se resuelve con anuncios, se resuelve con inventarios en línea. Presenté la propuesta con la meta concreta: menos del 10 % de eventos de desabastecimiento del listado básico al cierre del segundo año.',
 NULL, @catSalud, @pMedicam, @vVerificado, 505, 103),

(@kMarlon, @cGenerales2029, DATEADD(HOUR, -123, @hoy),
 N'Reunión con cooperativas de Choluteca y Valle. La demanda es la misma de hace diez años: riego y camino de acceso. La diferencia esta vez es que la meta queda escrita acá, con hectáreas y rendimiento medido.',
 N'demo', @catEconomia, @pRiego, @vDeclarado, 97, 19),

(@kGabi, @cGenerales2029, DATEADD(DAY, -7, @hoy),
 N'Atención primaria significa resolver cerca de la casa lo que hoy termina en emergencia del hospital. Un equipo por sector, horario extendido y reporte mensual de consultas. Está publicado con su indicador de cobertura.',
 NULL, @catSalud, @pPrimaria, @vVerificado, 264, 45);

PRINT 'Datos de demostración cargados.';
GO
