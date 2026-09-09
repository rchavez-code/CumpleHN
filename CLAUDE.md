# CumpleHN — Contexto del proyecto

Proyecto de graduación de Roy Chávez (cuenta 31521560), Ingeniería en Informática, CEUTEC. Asesor: Ing. Rafael Cerrato. Entrega: mayo 2026.

## Qué es CumpleHN

Plataforma web para el seguimiento ciudadano de promesas políticas en Honduras. Registra, clasifica, consulta y da seguimiento a promesas políticas, con asistente IA y visualizaciones estadísticas. Enfoque neutral: no promueve ni ataca candidatos.

**Propósito declarado:** la plataforma debe permitir que cada persona tome decisiones (electorales y de participación ciudadana) basadas en fundamentos estadísticos y cuantitativos, ponderando según el criterio que cada quien considere de mayor peso (educación, seguridad, salud, infraestructura, transparencia, etc.). CumpleHN no emite un veredicto sobre quién "cumple mejor"; entrega evidencia organizada para que el usuario juzgue por sí mismo. Este framing aplica al Resumen Ejecutivo, Justificación, Objetivos (dashboard y asistente IA) y Marco Teórico.

## Estructura de la carpeta

- Documentos del avance de tesis (`Avance #1.docx`, `Avance#2*.docx`, `Avance#3*.docx`, etc.) — versión más reciente de cada capítulo. `Avance#3 Roy Chavez 31521560.docx` es la más reciente al momento de escribir esto.
- `Marco_Teorico_CumpleHN.docx`, `Instrumentos_CumpleHN.docx`, `Encuesta.pdf`, `Respuestas a encuesta.xlsx` — insumos de investigación.
- `Portafolio RoyChavez 31521560.docx/.pdf` — portafolio del proyecto.
- `FO-GR-013_Descripcion_de_Proyecto_de_Graduacion...docx` — descripción original aprobada.
- `Plataforma Web/` — **código de la plataforma**. Es acá donde se programa.
- `app/` — andamiaje inicial, obsoleto. Quedó vacío cuando el código pasó a `Plataforma Web/`. Se puede eliminar.

## Restricciones de alcance acordadas con el docente

1. **No afirmar que no existe un mecanismo oficial de rendición de cuentas en Honduras.** Redactar solo que la información sobre promesas políticas está **dispersa** entre múltiples fuentes (discursos, entrevistas, redes sociales, debates, planes de gobierno, medios). No afirmar si existe o no algo oficial.
2. **Población objetivo:** ciudadanía hondureña habilitada para votar. **Población accesible/de estudio (propuesta al asesor, aún sin aprobar formalmente):** estudiantes universitarios hondureños mayores de 18 años, sin acotar a una sola universidad. Hasta que el asesor confirme el cambio, las preguntas de investigación, hipótesis y variables deben quedar redactadas hacia la ciudadanía hondureña en general — la delimitación a estudiantes solo se menciona como propuesta metodológica en la Justificación y en Metodología.

## Metodología (Capítulo V)

- Enfoque puramente cuantitativo (no mixto). Tipo de investigación: descriptivo, según Hernández-Sampieri et al. (2014).
- No se realizan entrevistas. La encuesta es el único instrumento y la única fuente primaria.
- Muestra: no probabilística por conveniencia, meta ~150 encuestados (95% confianza, 8% error, fórmula n=Z²pq/e²).

## Reglas de redacción y trabajo

- Toda afirmación factual debe llevar cita APA con fuente verificable.
- No usar punto y coma (";") en las redacciones.
- El texto de cuerpo del documento final va en formato APA (justificado, interlineado 2.0, sangría); los títulos no se tocan.
- Las redacciones nuevas se entregan primero en el chat para revisión. No editar los .docx directamente hasta que Roy los apruebe.
- Roy indica paso a paso qué sección rehacer — no adelantarse a rehacer secciones sin que las pida.

## Control de versiones

El proyecto es un repositorio git desde el 31 de agosto de 2026, con la raíz en la carpeta completa: los documentos de tesis y `Plataforma Web/` juntos. Rama `main`, remoto privado en `https://github.com/rchavez-code/CumpleHN.git`.

El `.gitignore` deja fuera `bin`, `obj`, `.vs` y `packages`, que son resultado de compilar y se regeneran solos. Eran 540 de los 759 archivos de la carpeta y 172 de los 182 MB. Los `packages.config` sí se versionan, y con ellos NuGet reconstruye `packages` al abrir la solución.

**Flujo acordado con Roy:** antes de empezar una tanda de cambios el árbol debe estar limpio y confirmado, para que exista un punto de retorno. Roy revisa lo que se hizo y, si no le convence, pide restaurar.

- Sin commit todavía: `git restore .` para lo modificado y `git clean -fd` para lo nuevo. **Siempre `git clean -nd` antes**, que es el simulacro y avisa qué se borraría.
- Ya con commit: `git revert HEAD`, que deshace conservando el historial. No usar `reset --hard` sin avisarle.
- Los cambios grandes o riesgosos van en rama aparte con `git switch -c`, nunca directo en `main`.
- Cerrar cada sesión de trabajo con `git push`. Es el único paso que constituye respaldo fuera de OneDrive.

Las rutas del proyecto llevan espacios (`Plataforma Web`, `Data Base`), así que en cualquier comando de git van entre comillas dobles.

`core.autocrlf` está en `false` a propósito, para que git no reescriba los bytes de los archivos guardados en UTF-8 sin BOM. La identidad de git se configuró solo con `--local`, no en toda la máquina.

## Código de la plataforma (`Plataforma Web/`)

### Stack

- **ASP.NET Web Application (.NET Framework 4.7.2), C#.** Frontend y backend separados.
- **Frontend:** Web Forms con Bootstrap 5 (solo la grilla) y jQuery. Sin dependencias externas en tiempo de ejecución.
- **Backend:** Web Service **ASMX** (SOAP).
- **Base de datos:** **SQL Server Express** — instancia `localhost\SQLEXPRESS`, base `BDCUMPLEHN`.
- Acceso a datos previsto: **ADO.NET con procedimientos almacenados**, sin Entity Framework, porque el Manual Técnico del capítulo IX exige documentar stored procedures, triggers y restricciones.

### Estructura

Tres soluciones `.slnx` independientes:

- `Plataforma Web/frontend/` → `frontend.csproj`. IIS Express en el puerto 5080.
- `Plataforma Web/backend/` → `backend.csproj`. Vacío por ahora, ya referencia `System.Web.Services`.
- `Plataforma Web/Data Base/BDCUMPLEHN/Scripts/` → scripts T-SQL numerados (no hay proyecto `.sqlproj` porque falta el workload SSDT). Se ejecutan en orden: `01_crear_base`, `02_tablas`, `03_catalogos`, `04_datos_demo`, `05_interaccion`, `06_datos_interaccion`, `07_vistas`, `08_analitica`, `09_administracion`, `10_catalogos_admin`, `11_modulos`, `12_asistente`, `13_permisos_ia`, `14_encuestas`, `15_datos_encuestas`, `16_registro`.

Los scripts están en UTF-8 **sin BOM**, así que por línea de comandos hay que pasarle la página de códigos a sqlcmd o las tildes entran corruptas:

```bash
sqlcmd -S "localhost\SQLEXPRESS" -d BDCUMPLEHN -E -C -b -f 65001 -i 07_vistas.sql
```

Dentro del frontend:

- `Content/cumplehn.css` — sistema de diseño completo con tokens. Base neutra, acentos inspirados en la guacamaya (escarlata, ámbar, verde, turquesa) usados solo en acciones e indicadores.
- `Modelos/` — modelos de vista y `Vista.cs` con todo el formato (fechas, plurales, clases CSS por categoría y estado).
- `Servicios/` — `IContenidoServicio` es el contrato único por el que las páginas obtienen datos. Hoy lo implementa `ContenidoDemo` (datos en memoria). **Al conectar el ASMX se cambia una sola línea en `Contenido.cs` y ninguna página se toca.**
- `Controles/` — controles reutilizables (`CampanaCard`, `CandidatoCard`, `PropuestaCard`, `PublicacionCard`), registrados en `Web.config` para que ninguna página los declare.
- `Panel/` — área privada del candidato, con su propia plantilla `Panel.Master`.
- `Admin/` — área privada del administrador de la plataforma, con su propia plantilla `Admin.Master`.

### Roles y control de acceso

Tres roles en el catálogo `Roles`: **Administrador**, **Candidato** y **Ciudadano**. El ciudadano participa en las páginas públicas y no tiene área privada, así que `Autorizacion.InicioDe` lo manda a la portada.

Cada rol entra a su propia área. La correspondencia rol → área vive **solo** en `Autorizacion.InicioDe`, que usan tanto `Acceso.aspx.cs` al entrar como las páginas al rechazar a quien no corresponde. Si estuviera en los dos lados podrían discrepar.

La protección **no puede** hacerse con `<authorization>` del Web.config: ese mecanismo se apoya en la autenticación de formularios de ASP.NET, y CumpleHN guarda la sesión en `Session` después de validar contra el Web Service. Se resuelve en código, en clases base de página de `Servicios/Autorizacion.cs`:

- `PaginaSegura` comprueba el rol en `OnPreInit`, el primer paso del ciclo de vida, antes de que exista un solo control. Sin sesión manda a `~/Acceso` conservando el destino. Con sesión pero con otro rol devuelve a su propia área, que no es lo mismo: pedirle acceso a quien ya entró es un rodeo sin salida.
- `PaginaPanel` exige rol Candidato y además expone `CandidatoActual` ya resuelto y **nunca nulo**. Todas las páginas de `Panel/` heredan de ella.
- `PaginaAdmin` exige rol Administrador. Todas las páginas de `Admin/` heredan de ella.

**La protección va en la clase base, no en cada página.** Antes cada página del panel repetía el mismo bloque de comprobación, así que una página nueva que olvidara copiarlo quedaba abierta — que es el fallo de control de acceso del A01 de OWASP. Con la clase base, olvidarse significa no compilar contra `CandidatoActual`.

Las plantillas `Panel.Master` y `Admin.Master` **no protegen nada**: una plantilla se aplica después de que la página ya empezó su ciclo de vida. `Panel.Master` toma la candidatura de la página en lugar de volver a pedirla, para no gastar una segunda llamada al Web Service por carga.

Del lado del backend, `EsAdministrador(conn, codigoUsuario)` confirma rol y cuenta activa contra la base. **Toda acción de administración tiene que pasar por ahí antes de escribir.** Lo que el frontend sabe de su sesión decide qué botones muestra, nunca qué se permite: quien llame al Web Service directamente envía el código de usuario que quiera.

### Decisiones de modelo que no se deben romper

1. **`Propuesta` es a la vez el proyecto de campaña y la promesa política del marco teórico.** Nace `Declarada` por el candidato y después admite estado de cumplimiento y evidencias. No duplicar en dos entidades: el seguimiento de cumplimiento del capítulo IX depende de que sea una sola.
2. **`NivelVerificacion`** (Declarado / En revisión / Verificado) en candidatos, propuestas y publicaciones. Como el candidato es autor de su propio contenido, la plataforma lo muestra siempre identificado como declarado hasta que exista fuente verificable. Es lo que sostiene la neutralidad.
3. **El candidato no asigna estados de cumplimiento.** Solo declara `Declarada` o `En proceso`. Los estados de cumplimiento (esquema PolitiFact) los asigna la plataforma con evidencia.
4. **Categorías temáticas:** taxonomía COFOG del Marco Teórico, ordenadas por el interés medido en la encuesta (seguridad, salud, educación, economía, infraestructura, transparencia).

### Compilar y correr

```
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "Plataforma Web\frontend\frontend\frontend.csproj" /p:Configuration=Debug
```

Para levantarlo hay un `.claude/launch.json` con la entrada `cumplehn-frontend` (IIS Express, puerto 5080).

**Importante al verificar:** MSBuild **no** compila el marcado de los `.aspx`. Un build limpio no garantiza que las páginas funcionen. Para eso está `aspnet_compiler.exe`, que valida todo el marcado en segundos:

```
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -v / -p "Plataforma Web\frontend\frontend" "$env:TEMP\verif"
```

Código de salida 0 significa que todo el marcado compila. No sustituye levantar el sitio para revisar lo visual, pero atrapa errores de marcado antes de abrir el navegador.

Hasta el 31 de agosto de 2026 esta herramienta fallaba con `0x800711C7`, porque Smart App Control de Windows 11 bloqueaba los DLL recién generados. Ese día se desactivó. Si el error reaparece conviene saber que también afecta al `frontend.dll` que genera Visual Studio, y que entonces el síntoma es un 500 en todo el sitio con `FileLoadException`. Se comprueba leyendo `VerifiedAndReputablePolicyState` en `HKLM\SYSTEM\CurrentControlSet\Control\CI\Policy`, donde 0 es desactivado y 1 es aplicando.

Las páginas están guardadas en UTF-8 sin BOM, así que `Web.config` necesita `<globalization fileEncoding="utf-8" ... culture="es-HN" />`. Sin eso las tildes salen corruptas.

### Base de datos y Web Service

La base `BDCUMPLEHN` existe y tiene 15 tablas: `Roles`, `Usuarios`, `Campanas`, `Cargos`, `Departamentos`, `Categorias`, `EstadosPropuesta`, `NivelesVerificacion`, `Candidatos`, `Propuestas`, `Publicaciones`, `Partidos`, `TiposObjeto`, `Valoraciones`, `Comentarios`.

### Módulo de interacción ciudadana

`Valoraciones` y `Comentarios` son **polimórficas**: apuntan a cualquier objeto con el par `(codigoTipoObjeto, codigoObjeto)`, donde el tipo viene del catálogo `TiposObjeto` (Publicacion, Candidato, Partido, Propuesta). Ese par **no lleva llave foránea** porque el destino cambia según el tipo — la existencia del objeto la valida `ExisteObjeto` en el Web Service, con una consulta fija por tipo. La alternativa era repetir ocho tablas iguales.

Reglas que no se deben romper:

1. **Un voto por persona y objeto.** Lo garantiza `UQ_Valoraciones_unaPorUsuario`, no solo el código. Repetir el mismo voto lo retira, votar lo contrario lo cambia.
2. **Los contadores no se guardan.** `meGusta`, `noMeGusta` y `comentarios` se derivan con subconsultas. Las columnas `apoyos`/`comentarios` de `Publicaciones` se eliminaron justamente para que el número mostrado no pueda contradecir a las filas reales.
3. **Consultar es público, participar requiere cuenta.** Los contadores y los comentarios se ven siempre. Al intentar votar o comentar sin sesión, `Sesion.UrlAccesoDeVuelta()` manda al acceso y regresa a la misma página. `Acceso.aspx` solo acepta rutas locales en `?volver=` para no ser un redirector abierto.
4. **`Partidos` está normalizado.** Antes el partido era texto dentro de `Candidatos`. Sin tabla propia no hay entidad a la que darle "me gusta".

En el frontend: `Controles/Participacion/` tiene `Interaccion.ascx` (botones) y `Comentarios.ascx` (hilo). Están en esa subcarpeta porque **ASP.NET no permite que un control registrado en Web.config se use desde otro control del mismo directorio**, y las tarjetas de `Controles/` los necesitan.

**Al mostrar texto de usuario en marcado usar siempre `<%#: %>` o `<%: %>`, nunca `<%# %>`**, que no escapa HTML. Cualquier página con repetidor que contenga tarjetas con botones debe enlazar sus datos **en cada carga**, también en los postbacks, o la tarjeta se queda sin modelo al procesar el clic.

**El backend es el único que toca la base.** La cadena `CnxCumpleHN` está solo en el Web.config del backend. El frontend no tiene cadena de conexión: obtiene todo por `WebServiceGlobal.asmx`.

Patrón de acceso a datos, igual en todos los métodos: `ConfigurationManager.ConnectionStrings` → `using (SqlConnection)` → `SqlCommand` **siempre con parámetros** → `SqlDataReader` → `List<T>`. Nunca se concatena entrada del usuario dentro del SQL.

Cada método lleva `[WebMethod]` y `[ScriptMethod(ResponseFormat = ResponseFormat.Json)]`, y la clase lleva `[ScriptService]`. Así el mismo servicio responde por SOAP (Service Reference desde el code-behind) y por JSON (llamada desde script).

Contraseñas: SHA-256 en hexadecimal minúscula, con `EncriptarSHA256` en el backend. El script de datos usa `HASHBYTES('SHA2_256', ...)` y produce el mismo hash — si se cambia uno hay que cambiar el otro o ningún login entra.

Usuarios de prueba, todos con contraseña `CumpleHN2026`: `admin` (Administrador) y uno por candidato (`amolina`, `rnunez`, `cvilleda`, `hpaz`, `gfonseca`, `mcastellanos`).

Para levantar los dos servidores hay dos entradas en `.claude/launch.json`: `cumplehn-backend` (IIS Express, `https://localhost:44359`) y `cumplehn-frontend` (puerto 5080). **El frontend necesita el backend en marcha**, si no las páginas salen vacías.

Si se agregan métodos al Web Service, hay que regenerar el proxy del frontend: en Visual Studio, clic derecho sobre el Service Reference `webservices` → *Update Service Reference*.

Desde la línea de comandos también se puede, con el backend levantado. **El sitio del backend expone además HTTP en el puerto 51720**, y conviene usar ese: el certificado de desarrollo de HTTPS hace fallar tanto a `Invoke-WebRequest` como a SvcUtil.

```bash
SvcUtil.exe "http://localhost:51720/WebServiceGlobal.asmx?wsdl" /language:cs /namespace:"*,frontend.webservices" /out:Reference.cs /noconfig /serializer:XmlSerializer
```

SvcUtil está en `C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\`. **Después hay que corregir a mano una línea**: SvcUtil genera `ConfigurationName="frontend.webservices.WebServiceGlobalSoap"` y tiene que quedar `"webservices.WebServiceGlobalSoap"`, que es lo que dice el `contract` del `<client>` en el Web.config. Si no coincide, ninguna página recibe datos.

### Módulo de analítica

Reparto estricto de responsabilidades, y **no se debe romper**:

```
base de datos  calcula      (vistas de detalle + procedimientos almacenados)
Web Service    transporta   (sin una línea de SQL escrita a mano)
Modelos/       interpreta   (KPI, hallazgos, proporciones)
Analitica.aspx dibuja       (anchos de barra, clases y SVG)
```

**Vistas de detalle** (`07_vistas.sql`): `vwAnaliticaCandidaturas`, `vwAnaliticaPropuestas`, `vwAnaliticaPublicaciones`, `vwAnaliticaValoraciones`, `vwAnaliticaComentarios`. Cada una devuelve **una fila por hecho**, ya cruzada con todas sus dimensiones (campaña, categoría, partido, departamento, nivel de gobierno, fecha). Las dos últimas resuelven la referencia polimórfica con una UNION de cuatro ramas, una por tipo de objeto.

**Procedimientos** (`08_analitica.sql`): diez, uno por indicador más `spAnaliticaCatalogos`. Se reducen a un `WHERE` y un `GROUP BY` sobre las vistas. Todos los parámetros son opcionales con el patrón `(@p IS NULL OR columna = @p)`, donde NULL significa «sin filtrar». **Un indicador nuevo se agrega creando primero su procedimiento**, para que el cálculo quede documentado en la base y no escondido en el Web Service — y de paso cubre la exigencia del Manual Técnico del capítulo IX de documentar stored procedures.

Las vistas ya agregadas del diseño anterior (`vwPropuestasPorCategoria` y las otras seis) se eliminan en `07_vistas.sql`: no aceptaban parámetros, así que filtrar habría obligado a concatenar el `WHERE` dentro del Web Service.

Todo el tablero viaja en **una sola llamada**, `obtenerAnalitica(FiltroAnalitica)`. No es solo por costo: garantiza que los nueve gráficos correspondan al mismo instante de los datos.

`Categorias.interesEncuesta` guarda el interés medido en la encuesta (n = 150), de modo que el gráfico que contrasta demanda ciudadana con oferta de propuestas sea data-driven y no texto fijo.

**Lo que el tablero deliberadamente NO muestra:** comparación contra el período anterior con su flecha y su porcentaje de variación. Los registros de participación abarcan tres días — esa variación sería una cifra inventada con apariencia de dato. En su lugar cada KPI se compara contra **su propio denominador** («0 de 11 verificadas», «3 de 18 departamentos») y las comparaciones entre entidades hacen el resto. Si algún día la base acumula meses de registro, ahí se agrega la comparación temporal.

Los **hallazgos** se calculan en `Modelos/Analitica.cs` (propiedad `Hallazgos`), no en SQL: son interpretación, y así la redacción vive junto al cálculo que la sostiene. Cuando una comparación no tiene sustento, el hallazgo **no se agrega** en lugar de mostrarse vacío.

Estructura de `Analitica.aspx`: encabezado + filtros + KPI siempre visibles, y cuatro pestañas (`hallazgos`, `oferta`, `participacion`, `cobertura`). Los cuatro paneles se renderizan en la misma respuesta y `cumplehn-analitica.js` los alterna sin volver al servidor. La pestaña activa se conserva en el input oculto `gcPestana` para sobrevivir a los postbacks de filtros.

**Cuidado con tres cosas del lado del cliente:**

1. **`cumplehn.css` ya ocupa varios nombres que parecen libres.** Como se carga primero y en todas las páginas, cualquier clase repetida en el tablero hereda estilos que no se pidieron, y el síntoma no se parece a la causa. Ya pasó dos veces:
   - `.gc-panel` es el layout **flex** del área privada del candidato. Al usarla en los paneles de pestaña, las columnas Bootstrap de adentro colapsaban a 190 px. Por eso los paneles son **`.gc-vista`**.
   - `.gc-tabs` y `.gc-tabs a` son las pestañas de `Candidato.aspx` y `Partido.aspx`, con `padding: 12px 0 14px`. Como `.gc-tabs a` (clase + elemento) gana por especificidad a `.gc-tab`, anulaba el padding horizontal y las cuatro etiquetas se leían pegadas: «HallazgosOferta programáticaParticipación». Por eso son **`.gc-pest`** y **`.gc-pest__b`**.

   Antes de nombrar una clase nueva del tablero, `grep` en `cumplehn.css`.

2. El velo de carga envuelve `window.__doPostBack`, no solo el evento `submit`. Los desplegables con AutoPostBack y los LinkButton llaman a `form.submit()` internamente, y ese método **no dispara** el evento `submit`.

3. El CSS y el JS del tablero se enlazan con `Recurso("~/…")`, que le pega la fecha de modificación del archivo como `?v=`. Sin esa huella el navegador sirve la versión cacheada y se depuran errores que ya estaban corregidos — pasó dos veces durante el desarrollo. Si se agregan más archivos estáticos a la página, enlazarlos igual.

Los estilos del tablero viven en `Content/cumplehn-analitica.css`, enlazado desde el `HeadContent` de la página y no desde el bundle, porque ninguna otra página los necesita.

**Todo archivo estático se enlaza con `Recursos.Url("~/…")`, que le pega la fecha de modificación como `?v=`.** Las tres plantillas (`Site.Master`, `Admin.Master`, `Panel.Master`) enlazan así `bootstrap.css` y `cumplehn.css`, en lugar del `<webopt:bundlereference path="~/Content/css" />` que usaban antes. Ese paquete, con las optimizaciones apagadas —que es como corre en desarrollo—, no combina ni versiona nada: escribe un enlace por archivo con la ruta pelada, y el navegador sirve para siempre la copia que ya tenía. Es la tercera vez que el proyecto pierde tiempo con esto, y la peor: una tanda entera de rediseño de la tarjeta de encuestas se veía con los estilos anteriores, así que parecía que el trabajo no se había hecho. `Bundle.config` se eliminó, porque un archivo que parece gobernar las hojas de estilo y no las gobierna es peor que no tenerlo.

Las pestañas son un **control segmentado** (pista gris con la activa en superficie blanca elevada), no una fila de títulos subrayados: con cuatro etiquetas largas, el subrayado solo no comunicaba que fueran opciones intercambiables. La activa lleva además un punto de color, para no depender solo de la elevación en alto contraste o al imprimir.

Reglas de los gráficos, aplicadas a mano porque no hay node para correr el validador de paleta:

1. **Una sola escala por gráfico. Nunca dos ejes.** Para comparar el interés (porcentaje) con las propuestas (conteo), el conteo se convierte a porcentaje del total. Por eso el conteo por partido y la densidad programática son **dos gráficos separados**: tienen unidades distintas.
2. **Cada valor se etiqueta de forma directa** y además existe la tabla equivalente en un `<details>`. Ningún dato depende solo del tooltip.
3. **Color según el trabajo del dato**: un tono (turquesa) para magnitud, par frío/cálido (turquesa + ámbar) para identidad, verde/ámbar/gris para estado, verde/escarlata para polaridad. Los pares verde/escarlata **siempre llevan etiqueta**, nunca identifican solo por color.
4. La cifra grande de los KPI usa la **sans** del cuerpo, no la serif de los títulos.
5. El valor de cada barra tiene **columna propia** en la cuadrícula, para que una barra al máximo de la escala no empuje su etiqueta fuera de la pista. Corolario: el texto de esa columna se mantiene corto, porque no envuelve.
6. **El azul de la bandera (`--gc-azul`) es identidad, no dato.** Viste encabezado y pestañas. Junto al turquesa en un gráfico, los dos azules no se separan bajo deuteranopia.
7. **Un solo gráfico circular en todo el tablero**, la dona de a favor / en contra, y lo es porque cumple la única condición que lo justifica: dos partes de un mismo todo. Con tres o más categorías siempre gana una barra ordenada.
8. El SVG (dona y actividad diaria) **se genera en el servidor**. Los tooltips son elementos `<title>` nativos: no necesitan JavaScript y los lee el lector de pantalla.
9. La cuadrícula de departamentos **no es un mapa** y la página lo dice. Trazar fronteras aproximadas a mano produciría un mapa falso, y la pregunta que el indicador responde es cuáles tienen cobertura, no dónde quedan.

El asistente de IA de `Analitica.aspx` ya está conectado a un modelo de lenguaje. Conserva la forma de respuesta que tenía la maqueta —cuerpo redactado más lista de fuentes—, y esa continuidad es deliberada: es lo que la encuesta dejó como condición de confianza (77.4 %). Se detalla más abajo, en «Módulo del asistente».

### Módulo de administración

Las dos facultades del rol Administrador: verificar contenido y moderar publicaciones. Mismo reparto de responsabilidades que la analítica — la base calcula y valida, el Web Service transporta, los modelos interpretan, la página dibuja.

**Retirar una publicación es baja lógica, nunca `DELETE`.** `Publicaciones` lleva `activo`, `motivoBaja`, `fechaBaja` y `codigoUsuarioBaja`. Un borrado real se llevaría las valoraciones y los comentarios de la publicación, y el tablero quedaría contando totales que no cuadran con las filas que quedan. El filtro `activo = 1` va en **dos lugares y solo dos**: la vista `vwAnaliticaPublicaciones` (de ahí se propaga a valoraciones y comentarios, que se unen a ella) y la constante `SelectPublicacion` del Web Service, que comparten todas las consultas públicas. Poner el filtro en cada método sería la manera de que una consulta nueva se olvide.

**Ninguna acción de administración queda sin registro.** La tabla `Auditoria` guarda quién, qué, sobre qué objeto, cuándo y con qué motivo, con el mismo par polimórfico `(codigoTipoObjeto, codigoObjeto)` de `Valoraciones` y `Comentarios`. A diferencia de aquellas, la fila se conserva aunque el objeto desaparezca: una bitácora que se borra sola no sirve como bitácora. No hay pantalla ni método para editarla o borrarla, y no debe haberlos.

**El motivo es obligatorio** al marcar como verificado y en las dos direcciones de la moderación. Ahí es donde queda anotada la fuente que respalda la decisión. Lo exige el procedimiento almacenado, no la página: una validación que solo vive en el formulario se salta llamando al servicio.

**El rol se comprueba dos veces**, en el Web Service (`EsAdministrador`) y dentro de cada procedimiento de escritura (`fnEsAdministrador`). No es descuido: el control de acceso no debe depender de un solo punto, y la comprobación de la base queda documentada en el Manual Técnico del capítulo IX. Está verificado que llamar al ASMX directamente con el código de un ciudadano o de un candidato no permite verificar ni moderar.

Procedimientos de `09_administracion.sql`: `spAdminBandejaVerificacion`, `spAdminCambiarVerificacion`, `spAdminPublicaciones`, `spAdminModerarPublicacion` y `spAdminAuditoria`, más la función `fnEsAdministrador`. Los de escritura devuelven una fila con `ok` y `mensaje` que el Web Service transporta tal cual, para que el texto del rechazo viva en un solo lugar.

La bandeja de verificación es **una sola cola** con candidaturas, propuestas y publicaciones juntas, ordenada por lo más antiguo sin verificar. Quien revisa trabaja por antigüedad, no por tipo.

**Cuidado con el ciclo de vida en las páginas con panel de decisión.** `Page_Load` corre **antes** que el evento del botón. Preseleccionar ahí el valor de un desplegable pisa lo que la persona acaba de elegir, y se guarda siempre el valor anterior. Pasó en `Verificacion.aspx`: el nivel se preselecciona solo cuando `!IsPostBack` o al elegir otro contenido, nunca en el postback que guarda.

### Catálogos administrados

Alta y edición de partidos, campañas y candidaturas, más la creación de la cuenta de acceso de una candidatura (`10_catalogos_admin.sql`). Mismas reglas del script 09: `fnEsAdministrador` en cada escritura, bitácora en cada acción, y una fila con `ok` y `mensaje` de vuelta.

**Los slugs los genera `fnSlug`, y solo al dar de alta.** Un slug es la dirección pública de la ficha: regenerarlo al editar rompería todo enlace ya compartido. Además los slugs existentes no siempre coinciden con lo que la función derivaría del nombre — la campaña «Elecciones Generales 2029» tiene el slug `generales-2029` —, así que corregir un nombre mal escrito mudaría la ficha de dirección. Los tres procedimientos de guardado comprueban la unicidad del slug **solo en el alta**.

**Nada se borra.** `Partidos` y `Candidatos` se desactivan con su columna `activo`, y las campañas se cierran con su `estado`. Un partido con candidaturas activas no se puede desactivar: escondería de la consulta pública a gente que sí se presentó por él. Al retirar una candidatura, **su cuenta de acceso se desactiva con ella** — una candidatura retirada que todavía puede publicar sería una puerta abierta sin ficha detrás.

**`Candidatos` conserva las columnas de texto `partido` y `partidoSiglas` junto a `codigoPartido`.** Están desnormalizadas desde antes y las consultas del Web Service las leen. `spAdminGuardarCandidato` las sincroniza desde `Partidos` en la misma operación, y `spAdminGuardarPartido` las actualiza en todas sus candidaturas al renombrarse. Como esos procedimientos son los únicos que escriben, no pueden quedar en desacuerdo.

Las contraseñas de las cuentas nuevas se cifran con `HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), @clave))` en hexadecimal minúscula, que produce exactamente el mismo hash que `EncriptarSHA256` del backend y que el script de datos demo. Está verificado con un acceso real. La contraseña no aparece en la respuesta ni en la bitácora: quien la crea es quien la entrega.

**`SET QUOTED_IDENTIFIER ON` al inicio de los scripts 09 y 10.** Un procedimiento guarda para siempre el valor que esa opción tenía al crearse, y **sqlcmd la trae apagada** (a diferencia de SSMS). Con la opción apagada, cualquier escritura sobre una tabla con índice filtrado falla con el error 1934 — `Campanas` tiene `UQ_Campanas_unicaActual`, el índice que garantiza una sola campaña destacada. Sin esa línea, los procedimientos compilan bien y fallan solo al ejecutarse.

### Interruptores de módulos

La administración puede ocultar del sitio público un módulo entero o un gráfico concreto del tablero (`11_modulos.sql`). **Ocultar no es dar de baja**: el contenido sigue en la base, y el rol Administrador lo sigue viendo con un aviso — así se puede preparar algo antes de publicarlo, o retirar lo que no está listo sin perder la capacidad de revisarlo.

**Un solo mecanismo para dos niveles de detalle.** `Modulos` tiene una fila por cada cosa apagable, y `clavePadre` expresa la jerarquía: apagar `analitica` apaga sus doce gráficos aunque cada uno tenga su interruptor encendido. La resolución vive en `vwModulosEfectivos`, que distingue `habilitado` (el interruptor propio) de `visible` (ya con el padre aplicado). Si cada página lo calculara por su cuenta, bastaría con que una lo hiciera distinto para que el tablero se contradijera.

**Solo se registran elementos que el código consulta de verdad.** No hay interruptor para «consulta ciudadana con filtros» de los seis módulos comprometidos: no es una página sino una capacidad repartida por el sitio, y apagarla no tendría efecto verificable. Un interruptor que no apaga nada es peor que no tenerlo, porque en la pantalla se ve igual que los que sí funcionan.

**Ocultar el enlace del menú nunca alcanza.** La dirección se puede escribir a mano. Cada página pública hereda de `PaginaDeModulo`, que comprueba su módulo en `OnPreInit`, y el Web Service comprueba `ModuloVisible` antes de aceptar una valoración o un comentario: cerrar la participación desde el frontend se saltaría llamando al ASMX directamente.

**Al cerrar la participación, lo ya registrado sigue visible** y solo se impide participar de nuevo. Los contadores se muestran, los botones quedan deshabilitados con su explicación, y el hilo de comentarios se lee. Esconder lo que ya se dijo sería reescribir el pasado, y además el tablero lo seguiría contando.

`Servicios/Modulos.cs` consulta el estado **una vez por petición** (caché en `HttpContext.Items`). Sin eso, una página con doce gráficos abriría doce llamadas al Web Service para responder doce veces la misma pregunta. Ante una clave desconocida o un backend caído devuelve visible: un error de comunicación no debe vaciar la plataforma.

Los bloques del tablero se marcan con `runat="server"` sobre su propio `div` en lugar de envolverse en un `PlaceHolder` — son divs anidados y envolverlos habría sido frágil. En las dos parejas de columnas el atributo va en el `col-*`, no en la tarjeta, para que no quede media fila vacía. El bloque que solo ve el administrador lleva la clase **`.gc-oculto`** (borde ámbar punteado y etiqueta «Oculto al público»): sin esa marca, quien administra confundiría lo que ve él con lo que ve el resto, que es el error que vuelve inútil un interruptor.

### Módulo del asistente

Mismo reparto de responsabilidades que la analítica, con una capa más: **la base calcula y además decide qué puede leerse**, el Web Service transporta y comprueba, el modelo redacta, la página dibuja. El modelo **nunca escribe SQL**. Recibe cuatro herramientas y cada una es un procedimiento almacenado con parámetros tipados, así que lo único que conoce de la plataforma es la fila que ese procedimiento le devuelve.

Las herramientas son `consultar_tablero` (los diez `spAnalitica*`), `buscar_propuestas`, `ficha_candidato` y `catalogos`. Agregar una capacidad nueva es agregar un procedimiento con su `GRANT`, **nunca una consulta suelta**.

**Lo que el asistente puede saber lo deciden las columnas, no los permisos.** Ningún procedimiento del script 12 proyecta con `*`. Quedan fuera a propósito el correo y el teléfono de las candidaturas, toda columna de `Usuarios`, y la participación individual: `Valoraciones` vincula a una persona con su preferencia política, que es el dato que más daño haría filtrado. `Comentarios` queda fuera además porque su texto lo escribe cualquier ciudadano, y leerlo sería dejar que un comentario le dé instrucciones al modelo.

**El login `cumplehn_ia` es la segunda capa** (`13_permisos_ia.sql`). No pertenece a `db_datareader` ni tiene permiso sobre ninguna tabla: solo `EXECUTE` sobre los procedimientos concedidos, y llega a las tablas por **encadenamiento de propiedad**. Por eso los `DENY` sobre `Usuarios`, `Valoraciones`, `Comentarios`, `Auditoria`, `ConsultasIA` y las dos vistas que llevan `codigoUsuario` bloquean la consulta directa sin romper los procedimientos. Está verificado con `EXECUTE AS`, y ya sirvió: atrapó una versión que resolvía la campaña con dos `SELECT` sueltos.

`AsistenteDatos` es **el único punto que abre esa conexión**, por la misma razón por la que el control de acceso vive en `PaginaSegura` y no repetido en cada página. Si se abriera en cada método que la necesita, bastaría con que uno nuevo se olvidara para perder la garantía, y el síntoma sería que todo funciona.

**Las fuentes se arman con las herramientas que se ejecutaron**, no con las que el modelo diga haber usado. Un modelo puede describir mal su propio trabajo, el registro de llamadas no.

Y **describen lo que la herramienta devolvió, no el procedimiento que la resolvió**: «7 candidaturas y 11 propuestas registradas en la plataforma», no «spAnaliticaResumen». Nombrar el procedimiento es cierto pero no le sirve a nadie para verificar nada, que es justamente lo que la encuesta pedía. La traza técnica no se pierde: los nombres de las herramientas siguen guardándose en `ConsultasIA`, que es donde hacen falta. Cuando el modelo responde sin consultar la base —un pedido de ranking, por ejemplo— la fuente lo dice, porque callarlo dejaría una respuesta sin respaldo con el mismo aspecto que una respaldada.

**Todo lo que entra por herramientas es dato, nunca instrucción.** El prompt lo declara y el resultado viaja delimitado. Del lado del navegador, la respuesta **no se inserta con `innerHTML`**: se analiza con `DOMParser`, que no ejecuta nada, y el árbol se reconstruye nodo por nodo con una lista blanca de etiquetas (`p`, `br`, `strong`, `em`, listas y tabla) **sin copiar ni un solo atributo**. La garantía está en los atributos y no en la lista de etiquetas: sin `onerror`, `href` ni `src`, ninguna etiqueta puede ejecutar nada aunque se permita. Las clases de la tabla las pone el script, nunca el modelo.

Una etiqueta fuera de la lista se descarta ella sola y su texto sigue subiendo. Perder media frase porque el modelo la envolvió en algo raro sería peor que perder el formato. **El prompt pide las mismas etiquetas que la lista blanca acepta, pero la que manda es la lista**: un prompt es una instrucción, no una garantía.

**Tres comprobaciones antes de gastar un token**, todas en el Web Service: módulo visible, cuenta activa y cuota diaria (`spIACuotaDisponible`, 20 por persona y día). El frontend decide qué muestra, nunca qué se permite. `ConsultasIA` registra cada pregunta con su respuesta, herramientas, tokens y tiempo, responda o falle — una bitácora que solo guarda los casos buenos no sirve para revisar los malos. La escribe el backend con su conexión normal, no el asistente.

La conversación **no va por postback**: la consulta tarda de diez a veinte segundos y eso congelaría la página y perdería la pestaña activa. Va contra `Asistente.ashx`, que vive en el frontend y no expone el backend al navegador — abrir CORS sobre el método que gasta dinero sería dejarlo al alcance de cualquier sitio.

El modelo y el esfuerzo son ajustes del `Web.config` (`AsistenteModelo`, `AsistenteEsfuerzo`), no del código. En desarrollo va `claude-sonnet-5`, que cuesta alrededor de la tercera parte que Opus 5. La elección definitiva se decide comparando lo guardado en `ConsultasIA`, y lo que hay que mirar no es si la respuesta suena bien sino si sostiene las tres reglas que hacen defendible el módulo: negarse a rankear candidaturas, decir el nivel de verificación sin que se lo pidan, y responder que no sabe en vez de completar.

La clave de la API y la cadena de `cumplehn_ia` viven en `secrets.config`, fuera de git, enganchado con el atributo `file` de `appSettings` — que complementa en vez de reemplazar y se ignora en silencio si falta, así el backend arranca en una máquina recién clonada. `secrets.config.ejemplo` sí se versiona.

El SDK `Anthropic` arrastra trece dependencias y exige `LangVersion 9.0`, porque declara sus propiedades con establecedores `init`. Las redirecciones de enlace usan la versión del **ensamblado** dentro de cada paquete, que no coincide con la del paquete: `System.Memory` 4.6.3 contiene el ensamblado 4.0.5.0. Sin ellas el sitio compila y falla al ejecutarse.

**Costo medido:** unos 11.000 tokens de entrada y 470 de salida por consulta, alrededor de $0.027 con Sonnet 5 y $0.077 con Opus 5. Casi toda la entrada es que `consultar_tablero` devuelve los diez indicadores aunque la pregunta necesite uno — dejarle al modelo elegir cuáles pedir es la optimización pendiente, y es mejor idea que bajar de modelo.

### Módulo de encuestas de percepción

La tercera pata del módulo de participación ciudadana. El FO-GR-013 aprobado describe ese módulo como «comentarios, valoraciones **o votaciones de percepción**»: las dos primeras están en el script 05, esta es la tercera. No es alcance nuevo, así que no hay que reabrir la descripción aprobada con el asesor.

**Solo el rol Administrador crea encuestas** (`Admin/Encuestas.aspx`). El candidato no: quien redacta las opciones controla el marco, y una encuesta escrita por una parte interesada dentro de una plataforma que se declara neutral sería un instrumento de campaña. El ciudadano tampoco, porque no hay moderación previa para preguntas escritas por cualquiera. El contenido de la pregunta queda **a criterio del administrador**.

**No se reusan las `Valoraciones`.** Aquella tabla guarda −1 o 1, y una encuesta es una opción entre N. Forzarla ahí ensuciaría el gráfico de signo de la participación, que hoy significa una sola cosa. Tres tablas nuevas en `14_encuestas.sql`: `Encuestas`, `EncuestaOpciones` y `EncuestaVotos`.

Reglas que no se deben romper:

1. **Un voto por persona y encuesta**, garantizado por `UQ_EncuestaVotos_unoPorUsuario` y no solo por el código, igual que `UQ_Valoraciones_unaPorUsuario`. Cambiar de opción mientras sigue abierta actualiza la fila, nunca agrega otra. Elegir la misma opción de nuevo no hace nada: a diferencia del me gusta, retirar el voto dejaría a la persona sin ver un resultado que ya vio.
2. **`EncuestaVotos` referencia el par `(codigoEncuesta, codigoOpcion)` con llave foránea compuesta**, apoyada en `UQ_EncuestaOpciones_par`. Esa restricción única parece redundante con la llave primaria y no lo es: sin ella un voto podría guardar una opción de otra encuesta y nada lo impediría.
3. **Los conteos no se guardan.** Se derivan con `COUNT` sobre `EncuestaVotos`. Una columna `votos` en la opción es la misma contradicción que se eliminó de `Publicaciones`.
4. **El estado se deriva de las fechas**, en `vwEncuestas`: Retirada, Programada, Cerrada o Abierta. No hay columna de estado — una que dijera «abierta» sobre una encuesta cuya fecha de cierre ya pasó es una mentira esperando a ocurrir.
5. **Nada se borra.** Retirar es baja lógica con `activo`, `motivoBaja`, `fechaBaja` y `codigoUsuarioBaja`, como en `Publicaciones`. Un `DELETE` se llevaría los votos.
6. **Las opciones solo se pueden cambiar mientras la encuesta no tenga votos.** Con votos, la pregunta y las fechas se corrigen y las opciones no: cambiarlas dejaría respuestas apuntando a algo que nadie respondió. Lo impide `spAdminGuardarEncuesta`, no el formulario.
7. **Varias encuestas pueden estar abiertas a la vez.** Hubo una restricción que lo impedía, con el argumento de que dos compitiendo se reparten la participación. Se quitó al pasar la portada a mostrar varias: era una regla de negocio haciendo el trabajo de una decisión de presentación.

**El resultado se muestra siempre, se haya respondido o no.** Hubo una versión que lo reservaba hasta después de votar, con una columna `revelar`, para no empujar a nadie hacia la opción que iba ganando. Se quitó a pedido de Roy: esconder el resultado hasta que participes convierte el dato en un peaje, y la plataforma existe para mostrar evidencia. Lo que queda de aquella decisión es el aviso del pie de la tarjeta, que dice a quién describe el resultado — el sesgo de arrastre no desaparece porque se muestre el reparto, pero quien lee sabe de qué muestra sale.

Del usuario solo se usa **qué eligió en cada encuesta**, para marcar su respuesta. `spEncuestasVigentesOpciones` lo resuelve con un `LEFT JOIN` a sus votos.

**Cada opción muestra el conteo y el porcentaje, no uno de los dos.** El porcentaje solo esconde de cuánta gente sale —el sesenta por ciento de cinco respuestas no es el sesenta por ciento de quinientas— y el conteo solo obliga a dividir de cabeza para comparar dos opciones.

**Cerrar y retirar no son lo mismo.** La cerrada terminó su votación y su resultado se consulta desde administración. La retirada, además, sale del alcance de cualquier consulta pública. Las dos acciones exigen motivo, y lo exige `spAdminEstadoEncuesta`.

**La portada solo muestra las abiertas.** Dejar una cerrada junto a las que sí admiten respuesta obligaría a quien mira a distinguir cuáles puede contestar antes de intentarlo. Hubo una versión que las mantenía un mes y se descartó por eso.

**El instante de cierre se trunca al segundo, no se redondea.** `DATETIME2(0)` redondea al segundo más cercano, así que guardar `SYSDATETIME()` directo puede dejar la fecha de cierre medio segundo en el futuro, y durante esa fracción `vwEncuestas` sigue diciendo «Abierta». La pantalla que acababa de confirmar el cierre mostraba «Abierta» en la misma respuesta. Se corta con `CONVERT(VARCHAR(19), SYSDATETIME(), 126)`.

**La tarjeta declara de qué no es evidencia.** El pie de `EncuestaCard` dice que participa quien decide hacerlo, así que el resultado describe a quienes respondieron y no a la población hondureña. Es la misma clase de aviso que el de la cuadrícula de departamentos del tablero: sin él quedaría una cifra con apariencia de encuesta representativa, que es justo lo que este proyecto no debería producir.

Una sola llamada trae las encuestas con sus opciones dentro (`listarEncuestasVigentes`): la portada dibuja varias tarjetas en la misma respuesta, así que pedir las opciones por encuesta serían tantos viajes como preguntas publicadas. Después de responder vuelven solo las de la encuesta tocada, que es lo único que cambió.

En el frontend, la tarjeta vive en `Controles/EncuestaCard.ascx` y se coloca en `Default.aspx`, entre la campaña destacada y las candidaturas. **Las mismas filas sirven para elegir y para leer el resultado**, con un solo repetidor: si las opciones se movieran al votar se perdería la relación entre lo que se eligió y lo que salió. La opción propia se identifica por el punto relleno, el borde y la negrita además del color, porque un solo canal de codificación deja fuera a quien no distingue el tono.

**Cada encuesta ocupa una fila entera**, y en pantalla ancha el cuerpo de la tarjeta se parte en dos columnas: la pregunta a la izquierda y las opciones a la derecha. A una sola columna, con el contenedor en 1140 px, cada opción quedaba de mil píxeles con la etiqueta y su cifra en extremos opuestos y nada en medio. Debajo de 992 vuelve a una columna, que es donde también se apilan las columnas de Bootstrap.

**La portada muestra dos encuestas y esconde el resto tras «Ver más».** Las escondidas se dibujan igual en la misma respuesta y el botón solo les quita el `display: none`: si pidiera otra carga al servidor, desplegar la lista costaría un viaje para mostrar algo ya consultado. El estado del despliegue viaja en un input oculto —igual que la pestaña activa del tablero— para sobrevivir al postback de responder una encuesta. **Cuántas se ven es constante de la página** (`EncuestasALaVista`), no de la base: el procedimiento devuelve todas las abiertas.

**Cada opción lleva su color, de una paleta de ocho** (`gc-enc__op--c1` a `c8`, tantos como el máximo de opciones, así que ninguna encuesta repite). No contradice la regla del tablero de usar un solo tono para magnitud: allá el color codifica el dato y hay que compararlo entre barras, mientras que acá las opciones son categorías nominales de una sola pregunta y el dato lo lleva la cifra escrita al lado. Queda fuera `--gc-azul`, que es identidad y no dato. El color va por clase y no por estilo en línea para que la paleta se pueda revisar entera en un archivo.

El filo de color en el canto izquierdo y el punto coloreado existen para que la tarjeta tenga color aunque una encuesta todavía no tenga respuestas y todas las barras midan cero.

**El círculo de selección desaparece cuando la encuesta ya no admite respuestas** (`is-cerrada`). Ese círculo es lo que dice «esto se puede elegir», así que dejarlo en una encuesta cerrada sería una invitación falsa. El de la opción propia se queda, que ahí sí informa.

Estilos en `cumplehn.css` con prefijo **`gc-enc`**, verificado con grep antes de nombrarlos. `.gc-enc--oculta` **no reusa** `.gc-oculto` del tablero: aquella vive en `cumplehn-analitica.css`, que la portada no carga, y definirla dos veces sería la manera de que acaben distintas. El hover se excluye con `:not(.aspNetDisabled)`, porque ASP.NET rinde un LinkButton apagado como un `<a>` sin href y no como un `<span>`. La tarjeta es una columna flexible con el pie empujado por `margin-top: auto`, que es lo que empareja dos encuestas de distinto largo en la misma fila.

**Interruptor propio `encuestas` en `Modulos`**, y `ModuloVisible` comprobado en el Web Service antes de aceptar un voto: cerrar solo la página se saltaría llamando al ASMX directamente. Está verificado con una llamada directa. `Encuesta` se registró en `TiposObjeto`, lo que deja las encuestas listas para recibir valoraciones y comentarios cuando se quiera — el backend ya las acepta en `TipoValido` y `ExisteObjeto`, que es la única de las cinco ramas que además filtra por `activo`.

**`EncuestaVotos` lleva `DENY SELECT` para `cumplehn_ia`**, junto con `Encuestas`. El login no pertenece a `db_datareader` y no tiene permiso sobre ninguna tabla, así que ya quedaban fuera de alcance, pero un voto vincula a una persona con una preferencia política y ese es el dato que más daño haría filtrado. No se concede `EXECUTE` sobre ningún procedimiento del script 14: cuando el asistente responda sobre encuestas será con un procedimiento propio que devuelva solo agregados.

`15_datos_encuestas.sql` deja una encuesta de demostración con sus votos repartidos entre las cuentas ciudadanas del script 06. Es prescindible, y **da el alta llamando al procedimiento en lugar de con un INSERT**, para que el script de datos pase por las mismas validaciones que la pantalla.

Una corrección de raíz que salió de acá: `LeerRespuesta` y `LeerGuardado` **no cerraban su `SqlDataReader`**. Nunca se notó porque cada método soltaba la conexión enseguida, pero `votarEncuesta` ejecuta más consultas sobre la misma conexión y falló con «ya hay un DataReader abierto». Ahora los dos lo cierran con `using`.

### Registro público de cuentas ciudadanas

`Registro.aspx` con `?tipo=ciudadano` crea la cuenta de verdad, contra `RegistrarCiudadano` del Web Service y `spRegistrarCiudadano` del script `16_registro.sql`. Era el prerrequisito de las encuestas y de todo el módulo de participación: hasta acá las únicas cuentas que podían opinar eran las diez de prueba del script 06.

**La contraseña se cifra en el backend, no en la base.** Es la diferencia con `spAdminCrearCuentaCandidato`, que la recibe en texto y la cifra con `HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), @clave))`. Ese `CONVERT` pasa por la página de códigos de la base —`Modern_Spanish_CI_AI`, la 1252— mientras que `EncriptarSHA256` trabaja sobre UTF-8. Con una contraseña de solo ASCII los dos coinciden, pero en cuanto lleva una tilde o una eñe dan hashes distintos y la cuenta **se crea bien y el acceso la rechaza siempre**. Como acá la contraseña la elige cualquiera, se cifra en un solo lugar, el mismo que después la valida. Está comprobado con `Contraseña1`: registro y acceso posterior con la misma clave.

**El formulario no pide nombre de usuario.** Pedirlo obliga a inventarlo y a chocar con los ocupados antes de poder registrarse, cuando el correo ya identifica la cuenta y `ValidarLogin` acepta los dos. El login lo deriva el procedimiento con la misma forma que tienen las cuentas sembradas —inicial del nombre más el primer apellido, `jlopez`— pasando por `fnSlug` para las tildes, y le agrega un número si ya está ocupado.

**El alta devuelve la misma `RespuestaLogin` que el acceso**, y con ella `Registro.aspx.cs` abre la sesión. Mandar al formulario de acceso justo después de crear la cuenta sería pedir que se compruebe algo que se acaba de comprobar.

**No escribe en `Auditoria` a propósito.** Esa bitácora registra lo que hace quien administra, y es lo que se lee para revisar decisiones de verificación y moderación. Registrarse no es una acción de administración, y mezclarlo la llenaría de filas que nadie revisa. Cuándo se creó la cuenta ya lo guarda `Usuarios.fechaRegistro`.

**El destino (`?volver=`) se conserva por toda la cadena**, del acceso al registro y del registro de vuelta a la página. Quien quiso comentar y no tenía cuenta terminaba el registro en la portada, buscando otra vez la publicación donde estaba. `Sesion.EsDestinoSeguro` es la comprobación de redirección abierta, y vive en `Sesion` —no copiada en cada página— porque una copia que se quede atrás no se nota: la página sigue funcionando igual, solo deja de estar protegida.

**`Registro.aspx?tipo=candidato` no crea nada.** La candidatura la da de alta la administración con su cuenta (script 10). Una ficha pública que nadie revisó antes de publicarse es lo contrario de lo que sostiene la neutralidad del sitio, así que el formulario recoge los datos y remite al contacto.

Decir «ese correo ya está registrado» revela que existe una cuenta con ese correo, y el acceso a propósito no lo revela. Sin un paso de confirmación por correo no hay alternativa: callarlo dejaría a quien ya tiene cuenta sin saber por qué falla. Queda anotado para el anexo OWASP.

### Pendiente

Guardado del perfil y de los proyectos desde el panel del candidato: los formularios ya validan del lado del servidor pero todavía no persisten.

**`spAdminCrearCuentaCandidato` arrastra el problema del hash descrito arriba.** Una cuenta de candidatura creada con una contraseña que lleve tilde o eñe no puede entrar nunca. Se arregla igual que en el registro ciudadano — que el backend mande el hash y el procedimiento reciba `CHAR(64)` en lugar de la clave en texto.

De las encuestas quedan tres cosas anotadas y ninguna urgente: que el candidato pueda abrir encuestas sobre sus propias propuestas —marcadas como declaradas y fuera del tablero, porque una encuesta escrita por una parte interesada no mide nada—, un gráfico `analitica.encuestas` que contraste el resultado con la oferta programática, y comentarios sobre la encuesta, que el backend ya acepta y la tarjeta todavía no muestra. **El resultado de las encuestas no debe sobrescribir `Categorias.interesEncuesta`**: aquella cifra viene de la encuesta metodológica del proyecto (n = 150) y esta de una muestra autoseleccionada, así que juntarlas mezclaría dos cosas con validez distinta. Si algún día se muestran en el mismo gráfico, van como series separadas y etiquetadas.

Del asistente quedan tres decisiones anotadas y ninguna urgente: que una consulta fallida no consuma cuota, un techo diario para toda la plataforma además del tope por persona, y que `consultar_tablero` deje elegir indicadores. Gráficos y PDF generados por el modelo **se descartaron a propósito**: la página ya dibuja los doce gráficos en el servidor y el backend puede armar un informe sin gastar un token, mientras que un gráfico dibujado por el modelo podría contradecir al del tablero.

Las cuatro etapas del área de administración están hechas: roles y control de acceso, verificación y moderación, catálogos con sus cuentas, e interruptores de módulos. Cada facultad tiene su procedimiento almacenado, su método en el Web Service, su sección en `Admin/` y su registro en bitácora, y el permiso se comprueba en las dos capas.

Mientras una sección no exista, aparece **deshabilitada** en el menú de `Admin/` en lugar de mostrar una pantalla que no guarda.

**Deuda de seguridad conocida, para el anexo OWASP:** el `codigoUsuario` de las valoraciones y los comentarios lo envía el frontend desde su sesión, y el Web Service solo comprueba que la cuenta exista y esté activa. Quien llame al servicio directamente puede opinar en nombre de otro usuario. Se resuelve cuando el backend valide un token de sesión en lugar de confiar en el código recibido. Sigue pendiente también el salt en las contraseñas.
