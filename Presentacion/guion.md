# Guion de la defensa — CumpleHN

18 láminas · **15 min a paso completo**, 12 min con los recortes marcados al final.
Las láminas de respaldo se abren con **B** y no se proyectan salvo que la terna pregunte.

## Manejo

| Tecla | Qué hace |
|---|---|
| `→` `Espacio` `AvPág` | avanza un paso dentro de la lámina, o pasa a la siguiente |
| `←` `RePág` | retrocede |
| `↓` `↑` | salta de lámina sin pasar por los pasos |
| `F` | pantalla completa |
| `N` | guion de la lámina actual, en pantalla |
| `B` | láminas de respaldo, y otra vez `B` para volver donde estabas |
| `1`…`9` | salto directo |

Los clickers mandan AvPág y RePág, así que funcionan sin configurar nada. El archivo es autocontenido: **no necesita internet**. Sin red, la tipografía cae a Georgia y se ve como la propia plataforma.

---

## Antes de entrar

1. Abrir `cumplehn-defensa.html` y pulsar `F`. Comprobar que se ve completo, sin barras.
2. Si vas a hacer la demo en vivo: levantar backend y frontend **antes**, entrar como `admin`, y dejar la pestaña del tablero abierta en otra ventana. Tener la pregunta escrita y ensayada.
3. Plan B de la demo: no hacerla. La lámina 13 ya muestra una respuesta real.

---

## Lámina por lámina

### 1 · Portada — 40 s
Presentarte y enunciar la tesis en una sola frase: *la información sobre promesas políticas existe, pero está dispersa, y esa dispersión tiene un costo medible sobre cómo se decide el voto*. No leer la portada: ya la están leyendo.

### 2 · El problema — 50 s
Las fichas que flotan son las fuentes donde queda registrada una promesa. El punto visual es que **no convergen**.
> **Cuidado:** no digas «no existe un mecanismo oficial de rendición de cuentas». Es la restricción que acordaste con el asesor. Di **dispersión**: la información está distribuida en muchas fuentes sin un esquema común que permita compararla.

### 3 · No es una intuición — 55 s
Cuatro cifras. Deja que los contadores terminen antes de hablar de cada uno.
Cierra con la línea del pie: **declara el muestreo tú mismo**, antes de que lo pregunten. Es la misma política que la plataforma aplica a sus propias cifras, y quita el argumento más fácil que tiene la terna.

### 4 · Y hay demanda — 50 s
El remate es la tercera cifra: 77.3 % confiaría en la IA **si cita fuentes**. Esa condición es literalmente la especificación del módulo. Anúncialo: «a esa condición vuelvo en la lámina 16».

### 5 · Objetivo — 55 s
Lee el objetivo general (es texto íntegro del capítulo III), y después pasa a la columna de la derecha. Si la terna solo se lleva una idea de toda la defensa, que sea esta: **la plataforma no emite veredicto, entrega evidencia ordenada**.

### 6 · La plataforma — 40 s
Es la primera prueba de que existe. No te detengas en la interfaz: las cifras de la portada se leen en vivo de la base, dilo y sigue.

### 7 · Topología — 55 s
Va paso a paso: navegador, frontend, servicio, base. La frase que importa es la del recuadro ámbar: **la cadena de conexión vive solo en el Web Service**, el frontend no la tiene y no referencia `System.Data`, así que no puede consultar la base ni por error.

### 8 · Modelo de datos — 55 s
Los cuatro grupos primero, el diagrama después.
> Si preguntan por qué la relación polimórfica no lleva llave foránea: porque el destino cambia según el tipo. La existencia la valida el Web Service con una consulta fija por tipo, y la alternativa era repetir ocho tablas iguales. Es la decisión más discutible del modelo — defiéndela, no la escondas.

### 9 · Neutralidad por diseño — 60 s
Las cuatro tarjetas, y **termina en la cita**: es una respuesta real del asistente que distingue los dos ejes sin que nadie se lo pidiera. Es la prueba de que la regla se cumple del modelo de datos al prompt.

### 10 · Control de acceso — 50 s
Primero el camino normal, después el del atacante. Remata con el recuadro verde: la protección va en la clase base, no repetida en cada página, y por eso olvidarse significa no compilar.
> Si preguntan por `<authorization>` del Web.config: ese mecanismo se apoya en la autenticación de formularios de ASP.NET, y acá la sesión se valida contra el Web Service.

### 11 · Tablero analítico — 50 s
El reparto de las cuatro capas es lo que hay que decir. Añade: **no hay librería de gráficos** — todo son repetidores, CSS y SVG generado en el servidor, y los tooltips son elementos `<title>` nativos que lee el lector de pantalla.

### 12 · La brecha — 55 s
Deja que las barras crezcan. La cita del final la redacta la propia plataforma y cambia con los filtros.
**Aclara sin que te lo pregunten:** la oferta son datos de demostración, así que la brecha ilustra el indicador, no diagnostica a la política hondureña.

### 13 · El asistente — 50 s
Respuesta real, 12 segundos.
**Aquí va la demo en vivo si la haces.** Una sola pregunta, ensayada. Si algo falla, sigue: la captura ya lo demostró.

### 14 · Topología del asistente — 75 s
La lámina técnica central. Diez pasos, uno por pulsación, sin apurar.
El argumento en una frase: **el modelo nunca ve la base, ve la fila que un procedimiento le devuelve**.

### 15 · Herramientas y permisos — 55 s
El argumento no es «le pusimos permisos». Es que **la superficie de datos se diseñó columna por columna**, y el permiso del login es la red de abajo. Termina con el `EXECUTE AS`: la comprobación ya atrapó una versión que resolvía la campaña con dos `SELECT` sueltos.

### 16 · Las cinco garantías — 60 s
Vuelve a enganchar con la lámina 4. Cierra diciendo qué se evalúa: no si la respuesta suena bien, sino si sostiene las tres reglas — negarse a rankear, decir el nivel de verificación, y responder que no sabe en vez de completar.

### 17 · Encuestas — 45 s
Es la tercera pata del módulo de participación que el FO-GR-013 ya contemplaba («comentarios, valoraciones **o votaciones de percepción**»), así que **no es alcance nuevo** — dilo, evita la pregunta.
El pie de la tarjeta declara de qué no es evidencia. Es el mismo criterio de la lámina 3.

### 18 · Cierre — 40 s
Las cifras, lo pendiente dicho por ti, el trabajo futuro con su argumento.
Lee la frase final, **calla, y espera las preguntas**.

---

## Las láminas de respaldo (tecla `B`)

| Si preguntan por… | Lámina |
|---|---|
| «¿Esto no existe ya?» | 1 · Antecedentes — PolitiFact, Chequeado, Del Dicho al Hecho, IAIP |
| Muestra, fórmula, tipo de investigación | 2 · Metodología |
| Validez, generalización, la hipótesis | 3 · Limitaciones declaradas |
| Seguridad, OWASP, contraseñas | 4 · Seguridad y deuda conocida |
| Qué calcula cada gráfico | 5 · Los doce indicadores |
| El área de administración | 6 · Cuatro etapas con bitácora |

**La lámina 3 de respaldo es la más importante de las seis.** Si la terna encuentra una debilidad que tú ya declaraste, deja de ser una debilidad y pasa a ser rigor. Las cuatro que están ahí son: la hipótesis no se validó empíricamente, la precisión de la IA no se midió (se midió confianza declarada), las promesas recurrentes no se analizaron en el discurso, y los datos de la plataforma son de demostración.

---

## Recorte para 12 minutos

Si el tiempo aprieta, en este orden:

1. **Lámina 6** (la plataforma) — pásala en 15 s, solo señalando la captura.
2. **Lámina 11** (tablero) — di el reparto de capas y pasa. Quita el detalle de los repetidores y el SVG.
3. **Lámina 17** (encuestas) — deja solo la primera tarjeta y el aviso del pie.
4. **Lámina 12** — no leas la cita completa, resume: la mayor brecha está en seguridad, 16.2 puntos.

**No recortes** las láminas 3, 5, 9, 14 y 16: son la evidencia, la neutralidad y el módulo de IA, que es donde pusiste el énfasis.
