/* ============================================================
   CumpleHN — 13. Permisos del asistente
   ------------------------------------------------------------
   El login con el que el backend abre la conexión del asistente,
   y solo esa conexión.

   Por qué existe. El script 12 ya limita lo que el asistente
   puede saber, porque el modelo no escribe SQL y cada
   procedimiento nombra sus columnas una por una. Este script es
   la segunda capa: la que sigue en pie si mañana alguien agrega
   una consulta descuidada en el camino del asistente. Es la
   misma doctrina que el resto del proyecto ya aplica al rol
   Administrador, donde el permiso se comprueba en el Web
   Service y otra vez dentro del procedimiento — el control de
   acceso no debe depender de un solo punto.

   Cómo funciona sin darle SELECT a nada. El login no pertenece a
   db_datareader ni tiene permiso sobre ninguna tabla. Solo tiene
   EXECUTE sobre los procedimientos de consulta. Lee las tablas a
   través de ellos gracias al encadenamiento de propiedad: como
   los procedimientos, las vistas y las tablas pertenecen todos a
   dbo, SQL Server no evalúa los permisos de los objetos
   referidos cuando la lectura entra por el procedimiento.

   De ahí que los DENY de más abajo no rompan nada. Bloquean la
   consulta directa — un SELECT escrito a mano contra Usuarios o
   contra Valoraciones — sin estorbarle a spAnaliticaParticipacion,
   que llega a esas mismas filas por la cadena de propiedad y ya
   agregadas.

   Qué protege y qué no. Protege de que el camino del asistente
   escriba, y de que lea algo que ningún procedimiento le expuso.
   No protege de que un procedimiento proyecte una columna que no
   debía — eso se cuida en el script 12 — ni de que el modelo
   llame a una herramienta legítima con malas intenciones, que se
   cuida en el prompt del backend.

   Cómo se ejecuta. La contraseña no vive en el archivo, entra
   como variable de sqlcmd, para que el script se pueda versionar
   sin secreto adentro:

     sqlcmd -S "localhost\SQLEXPRESS" -d BDCUMPLEHN -E -C -b
            -f 65001 -v claveIA="LA_QUE_SEA" -i 13_permisos_ia.sql

   La misma contraseña va después en el secrets.config del
   backend, que está fuera de git.

   Requiere que la instancia acepte autenticación de SQL Server.
   SQL Server Express se instaló acá solo con autenticación de
   Windows, así que el login se crea pero no puede entrar hasta
   habilitar el modo mixto. Se comprueba con:

     SELECT SERVERPROPERTY('IsIntegratedSecurityOnly');

   Un 1 significa solo Windows. Se cambia en el registro y toma
   efecto al reiniciar el servicio — desde PowerShell como
   administrador, y en una sola línea porque la ruta lleva el
   identificador de instancia:

     Set-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQLServer' -Name LoginMode -Value 2
     Restart-Service 'MSSQL$SQLEXPRESS'

   MSSQL17.SQLEXPRESS es el identificador de esta máquina. En otra
   sale de la clave Instance Names\SQL, y cambia con la versión de
   SQL Server, así que no se puede copiar a ciegas.

   El bloque final del script vuelve a verificarlo y avisa.

   Se puede volver a ejecutar sin duplicar nada.
   ============================================================ */

USE BDCUMPLEHN;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ============================================================
   1. El login y su usuario en la base
   ============================================================ */

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'cumplehn_ia')
BEGIN
    CREATE LOGIN cumplehn_ia
        WITH PASSWORD = '$(claveIA)',
             CHECK_POLICY = ON,
             DEFAULT_DATABASE = BDCUMPLEHN;

    PRINT 'Login cumplehn_ia creado.';
END
ELSE
    PRINT 'Login cumplehn_ia ya existía, no se toca su contraseña.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'cumplehn_ia')
BEGIN
    CREATE USER cumplehn_ia FOR LOGIN cumplehn_ia;
    PRINT 'Usuario cumplehn_ia creado en BDCUMPLEHN.';
END
ELSE
    PRINT 'Usuario cumplehn_ia ya existía en BDCUMPLEHN.';
GO

/* Sin db_datareader a propósito. Que quede escrito acá para que
   nadie lo agregue después pensando que falta: si el asistente
   necesita un dato nuevo, se le agrega un procedimiento, no
   permiso de lectura sobre las tablas. */

/* Nada de escritura, y explícito. Los procedimientos que se le
   conceden no escriben, así que esto no le quita ninguna
   capacidad — deja constancia de la intención. */
ALTER ROLE db_denydatawriter ADD MEMBER cumplehn_ia;
GO

/* ============================================================
   2. Lo que sí puede ejecutar

   Los diez procedimientos del tablero, que ya trabajan sobre
   vistas agregadas, más los dos del script 12 que responden
   sobre contenido concreto.
   ============================================================ */

GRANT EXECUTE ON dbo.spAnaliticaCatalogos     TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaResumen       TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaCategorias    TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaEstados       TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaVerificacion  TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaParticipacion TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaCandidatos    TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaPartidos      TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaTerritorio    TO cumplehn_ia;
GRANT EXECUTE ON dbo.spAnaliticaActividad     TO cumplehn_ia;
GRANT EXECUTE ON dbo.spIABuscarPropuestas     TO cumplehn_ia;
GRANT EXECUTE ON dbo.spIAFichaCandidato       TO cumplehn_ia;
GO

/* spIACuotaDisponible y spIARegistrarConsulta quedan fuera a
   propósito. Los ejecuta el backend con la conexión normal: la
   cuota y la bitácora son decisiones del servicio y no algo que
   el modelo pueda provocar. Así este login queda de lectura pura
   y no hay excepción que explicar en el anexo de seguridad. */

/* ============================================================
   3. Lo que no puede leer ni por accidente

   Redundante mientras el asistente entre solo por los
   procedimientos de arriba. Está para el día en que alguien
   escriba una consulta suelta en ese camino.

   Los cuatro primeros son los datos personales. ConsultasIA es
   lo que otras personas preguntaron. Las dos vistas del final
   son las que resuelven la referencia polimórfica y llevan
   codigoUsuario junto al objeto valorado — o sea, la persona y
   su preferencia política en la misma fila. Es el dato que más
   daño haría filtrado, más que las contraseñas.
   ============================================================ */

DENY SELECT ON dbo.Usuarios                 TO cumplehn_ia;
DENY SELECT ON dbo.Valoraciones             TO cumplehn_ia;
DENY SELECT ON dbo.Comentarios              TO cumplehn_ia;
DENY SELECT ON dbo.Auditoria                TO cumplehn_ia;
DENY SELECT ON dbo.ConsultasIA              TO cumplehn_ia;
DENY SELECT ON dbo.vwAnaliticaValoraciones  TO cumplehn_ia;
DENY SELECT ON dbo.vwAnaliticaComentarios   TO cumplehn_ia;
GO

/* ============================================================
   4. Comprobación

   Deja en pantalla lo que quedó concedido, para pegarlo en el
   Manual Técnico sin tener que ir a buscarlo a SSMS.
   ============================================================ */

IF SERVERPROPERTY('IsIntegratedSecurityOnly') = 1
    PRINT 'AVISO: la instancia solo acepta autenticación de Windows. El login existe pero no podrá conectarse hasta habilitar el modo mixto.';
ELSE
    PRINT 'La instancia acepta autenticación de SQL Server.';
GO

SELECT
    p.permission_name  AS permiso,
    p.state_desc       AS estado,
    o.type_desc        AS tipoObjeto,
    o.name             AS objeto
FROM sys.database_permissions p
INNER JOIN sys.database_principals u ON u.principal_id = p.grantee_principal_id
LEFT  JOIN sys.objects o             ON o.object_id = p.major_id
WHERE u.name = N'cumplehn_ia'
ORDER BY p.state_desc, o.type_desc, o.name;
GO

/* ============================================================
   5. Comprobación del comportamiento

   Que los permisos existan no prueba que se comporten. Se
   verificaron asumiendo la identidad del usuario, que no
   necesita su contraseña:

     EXECUTE AS USER = 'cumplehn_ia';  ...  REVERT;

   Resultado, el 6 de septiembre de 2026:

     [1] spIABuscarPropuestas ......... permite
     [2] spAnaliticaParticipacion ..... permite
     [3] SELECT sobre Usuarios ........ bloquea
     [4] SELECT sobre vwAnaliticaValoraciones  bloquea
     [5] SELECT sobre Propuestas ...... bloquea
     [6] spIARegistrarConsulta ........ bloquea
     [7] UPDATE sobre Propuestas ...... bloquea

   El caso [2] es el que sostiene todo el esquema y por eso se
   probó aparte: spAnaliticaParticipacion lee
   vwAnaliticaValoraciones, que tiene DENY explícito, y aun así
   funciona. Es el encadenamiento de propiedad — con la cadena
   intacta el permiso del objeto referido no se evalúa. Sin esa
   propiedad habría que elegir entre proteger la vista o tener
   tablero, y acá se tienen las dos cosas.

   El [5] muestra la otra cara: Propuestas no lleva DENY y
   tampoco se puede leer directo, porque nunca se concedió el
   permiso. Los DENY del bloque 3 son para lo que no debe
   leerse ni por accidente, no la única defensa.
   ============================================================ */

PRINT '13_permisos_ia.sql aplicado.';
GO
