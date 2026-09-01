/* ============================================================
   CumpleHN — 03. Carga de catálogos
   ------------------------------------------------------------
   Datos de referencia del sistema. No son datos de prueba: la
   plataforma los necesita para funcionar.

   Se puede volver a ejecutar sin duplicar filas.
   ============================================================ */

USE BDCUMPLEHN;
GO

/* ----------------------------------------------------- Roles */

MERGE dbo.Roles AS destino
USING (VALUES
    (N'Administrador', N'Administra la plataforma, los catálogos y la verificación de contenido.'),
    (N'Candidato',     N'Administra su propio perfil, sus proyectos y sus publicaciones.'),
    (N'Ciudadano',     N'Apoya publicaciones, comenta y participa en votaciones de percepción.')
) AS origen (nombre, descripcion)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, descripcion) VALUES (origen.nombre, origen.descripcion);
GO

/* ---------------------------------------------------- Cargos */

MERGE dbo.Cargos AS destino
USING (VALUES
    (N'Presidencia de la República',                N'Nacional',      1),
    (N'Designación presidencial',                   N'Nacional',      2),
    (N'Diputación al Congreso Nacional',            N'Departamental', 3),
    (N'Diputación al Parlamento Centroamericano',   N'Nacional',      4),
    (N'Alcaldía municipal',                         N'Municipal',     5),
    (N'Regiduría municipal',                        N'Municipal',     6)
) AS origen (nombre, nivelGobierno, orden)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, nivelGobierno, orden)
    VALUES (origen.nombre, origen.nivelGobierno, origen.orden);
GO

/* -------------------------------------------- Departamentos */

MERGE dbo.Departamentos AS destino
USING (VALUES
    (N'Atlántida'), (N'Choluteca'), (N'Colón'), (N'Comayagua'), (N'Copán'),
    (N'Cortés'), (N'El Paraíso'), (N'Francisco Morazán'), (N'Gracias a Dios'),
    (N'Intibucá'), (N'Islas de la Bahía'), (N'La Paz'), (N'Lempira'),
    (N'Ocotepeque'), (N'Olancho'), (N'Santa Bárbara'), (N'Valle'), (N'Yoro')
) AS origen (nombre)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre) VALUES (origen.nombre);
GO

/* ------------------------------------------------ Categorías

   Taxonomía derivada de la Clasificación de las Funciones del Gobierno
   (ONU, 2000). El orden refleja el interés medido en la encuesta del
   proyecto: seguridad 25.3 %, salud 23.3 %, educación 18.0 %,
   economía 14.7 %, infraestructura 14.0 %.                          */

MERGE dbo.Categorias AS destino
USING (VALUES
    (N'Seguridad y orden público',   N'Prevención del delito, fortalecimiento policial y justicia.',        1),
    (N'Salud',                       N'Infraestructura hospitalaria, medicamentos y atención primaria.',    2),
    (N'Educación',                   N'Cobertura, infraestructura escolar, becas y calidad educativa.',     3),
    (N'Economía y empleo',           N'Generación de empleo, apoyo productivo y desarrollo económico.',     4),
    (N'Infraestructura',             N'Vialidad, agua potable, saneamiento y electrificación.',             5),
    (N'Transparencia y gobernanza',  N'Acceso a la información, rendición de cuentas y anticorrupción.',    6)
) AS origen (nombre, descripcion, orden)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, descripcion, orden)
    VALUES (origen.nombre, origen.descripcion, origen.orden);
GO

/* ------------------------------------ Estados de cumplimiento

   Esquema del rastreador de promesas de PolitiFact (2018). La ponderación
   es el peso con el que cada estado entra en el cálculo del porcentaje de
   cumplimiento de un candidato, partido o período.

   Declarada no pondera: es el estado en que nace el compromiso, antes de
   que exista cualquier evaluación.                                      */

MERGE dbo.EstadosPropuesta AS destino
USING (VALUES
    (N'Declarada',         N'Registrada por la candidatura. Todavía sin evaluación de cumplimiento.', 0.00, 1),
    (N'Sin avance',        N'Evaluada y sin ninguna acción verificable hacia su cumplimiento.',       0.00, 2),
    (N'En proceso',        N'Con acciones verificables en curso hacia su cumplimiento.',              0.50, 3),
    (N'Estancada',         N'Iniciada y luego detenida, sin avance verificable reciente.',            0.25, 4),
    (N'Cumplida a medias', N'Cumplida de forma parcial respecto de lo que se prometió.',              0.50, 5),
    (N'Cumplida',          N'Cumplida en los términos en que fue formulada.',                         1.00, 6),
    (N'Incumplida',        N'Concluido el plazo sin haberse cumplido.',                               0.00, 7)
) AS origen (nombre, descripcion, ponderacion, orden)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, descripcion, ponderacion, orden)
    VALUES (origen.nombre, origen.descripcion, origen.ponderacion, origen.orden);
GO

/* --------------------------------- Niveles de verificación */

MERGE dbo.NivelesVerificacion AS destino
USING (VALUES
    (N'Declarado',   N'Afirmado por la candidatura. La plataforma no lo respalda todavía.',      1),
    (N'En revisión', N'En proceso de contraste contra fuentes.',                                 2),
    (N'Verificado',  N'Respaldado por al menos una fuente verificable registrada.',              3)
) AS origen (nombre, descripcion, orden)
    ON destino.nombre = origen.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, descripcion, orden)
    VALUES (origen.nombre, origen.descripcion, origen.orden);
GO

PRINT 'Catálogos cargados.';
GO
