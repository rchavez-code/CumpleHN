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
- `Plataforma Web/Data Base/BDCUMPLEHN/Scripts/` → scripts T-SQL numerados (no hay proyecto `.sqlproj` porque falta el workload SSDT). Se ejecutan en orden: `01_crear_base`, `02_tablas`, `03_catalogos`, `04_datos_demo`, `05_interaccion`, `06_datos_interaccion`, `07_vistas`, `08_analitica`.

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

**Importante al verificar:** MSBuild **no** compila el marcado de los `.aspx`. Un build limpio no garantiza que las páginas funcionen. La forma confiable de comprobarlo es levantar el sitio y recorrer las rutas. (`aspnet_compiler.exe` sirve para lo mismo, pero en esta máquina Application Control de Windows bloquea los DLL que genera y falla con `0x800711C7`.)

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

El asistente de IA de `Analitica.aspx` es una **maqueta declarada como tal en pantalla**: las respuestas se arman con las cifras reales del tablero en pantalla y citan el procedimiento del que sale cada cifra, pero las preguntas se reconocen por palabras clave, no con un modelo de lenguaje. Cuando no reconoce una pregunta lo dice, en lugar de inventar. Al conectar el modelo se conserva la forma de la respuesta con sus fuentes, que es lo que la encuesta dejó como condición de confianza (77.4 %).

### Pendiente

Alta de usuarios desde el registro, guardado del perfil y de los proyectos (los formularios ya validan del lado del servidor pero todavía no persisten), moderación de comentarios, registro de evidencias para poder mover los estados de cumplimiento, y conectar el asistente a la API de un modelo de lenguaje.

**Deuda de seguridad conocida, para el anexo OWASP:** el `codigoUsuario` de las valoraciones y los comentarios lo envía el frontend desde su sesión, y el Web Service solo comprueba que la cuenta exista y esté activa. Quien llame al servicio directamente puede opinar en nombre de otro usuario. Se resuelve cuando el backend valide un token de sesión en lugar de confiar en el código recibido. Sigue pendiente también el salt en las contraseñas.
