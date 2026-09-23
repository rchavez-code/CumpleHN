# ============================================================================
#  Láminas 13 a 18 y las 6 de respaldo.
# ============================================================================

# Marca de esquina de las láminas que no se proyectan salvo que pregunten.
function Add-SelloRespaldo {
    param($S)
    $c = $script:Col
    Add-Caja $S 1020 28 196 22 -Relleno $c.ambarL -Borde $c.ambarVivo -Redonda | Out-Null
    Add-Txt $S 1020 34 196 14 'Lámina de respaldo' -Size 10 -Bold -Color $c.ambar -Espaciado 1.2 -Caps -Align 2 | Out-Null
}

# Fila de tabla ligera: rótulos arriba, línea fina debajo de cada fila.
function Add-FilaTabla {
    param($S, [double]$X, [double]$Y, [double[]]$Anchos, [string[]]$Celdas,
          [double]$Alto = 30, [switch]$Encabezado, $Color = $null, [double]$Size = 13)
    $c = $script:Col
    if ($null -eq $Color) { $Color = $c.tinta2 }
    $cx = $X
    for ($i = 0; $i -lt $Celdas.Length; $i++) {
        if ($Encabezado) {
            Add-Txt $S $cx $Y ($Anchos[$i] - 12) 14 $Celdas[$i] -Size 10.5 -Bold -Color $c.suave -Espaciado .9 -Caps | Out-Null
        } else {
            Add-Txt $S $cx $Y ($Anchos[$i] - 12) ($Alto - 6) $Celdas[$i] -Size $Size -Color $Color -Interlineado 1.3 | Out-Null
        }
        $cx += $Anchos[$i]
    }
    $ancho = 0; $Anchos | ForEach-Object { $ancho += $_ }
    $lineaY = $Y + $Alto - 4
    $col = if ($Encabezado) { $c.linea } else { $c.linea2 }
    Add-Linea $S $X $lineaY ($X + $ancho - 12) $lineaY -Color $col -Grosor 0.75 | Out-Null
}

function Build-LaminasB {
    param($Pres, [string]$Img)

    $c = $script:Col

    # ────────────────────────────────────────── 13 · El asistente ──────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Módulo de inteligencia artificial'
    Add-Titulo $s 'Preguntar en español, y recibir la respuesta con sus fuentes'

    Add-Caja $s 64 150 700 30 -Relleno $c.linea2 -Borde $c.linea | Out-Null
    foreach ($px in @(80, 94, 108)) { $p = Add-Caja $s $px 161 8 8 -Relleno $c.tenue; $p.AutoShapeType = 9 }
    Add-Txt $s 128 158 460 16 'cumplehn / Analitica · asistente' -Size 11 -Font $script:Mono -Color $c.suave | Out-Null
    $pic = Add-Img $s 64 180 700 488 (Join-Path $Img 'asistente.jpg')
    $pic.PictureFormat.CropBottom = (488 - 412) * 0.75

    $tarj = @(
        @{ t = 'Dijo el nivel de verificación sin que se lo pidieran'; filo = $c.verde; h = 126;
           d = 'La pregunta era cuáles propuestas de salud hay. Distinguir Verificado de Declarada es una regla del prompt, no del usuario.' },
        @{ t = 'Cerró con sus fuentes'; filo = $c.azul; h = 136;
           d = 'Y las describe por lo que devolvieron —«búsqueda entre las propuestas registradas, 2 resultados»— no por el nombre del procedimiento, que no le sirve a nadie para verificar nada.' },
        @{ t = 'Se identifica como prototipo'; filo = $c.ambarVivo; h = 128;
           d = 'Y el aviso de arriba advierte, antes de la primera pregunta, que puede equivocarse al redactar y que las cifras se contrastan con los gráficos de esa misma página.' }
    )
    $y = 150
    foreach ($t in $tarj) {
        Add-Tarjeta $s 790 $y 426 $t.h $t.t $t.d -Filo $t.filo -SizeT 16 -SizeC 13.5
        $y += $t.h + 12
    }
    Add-Pie $s 'Respuesta real, capturada de la plataforma corriendo contra la base. Tardó 12 segundos.'

    # ─────────────────────────────── 14 · Topología del asistente ──────────
    $s = New-Lamina $Pres 'noche'
    Add-Ceja $s 'Asistente · Topología' -Noche
    Add-Titulo $s 'El modelo nunca ve la base. Ve la fila que un procedimiento le devuelve.' -Noche -Size 38 -H 96

    Add-Txt $s 74 191 400 14 'Camino de la pregunta' -Size 10.5 -Bold -Color $c.nCeja -Espaciado .8 -Caps | Out-Null
    Add-Nodo $s 74  216 200 66 'Navegador'      'POST JSON, sin postback'
    Add-Nodo $s 304 216 200 66 'Asistente.ashx' 'vive en el frontend' -Estilo 'acento' -Mono
    Add-Nodo $s 534 216 200 66 'Web Service'    '3 comprobaciones previas' -Estilo 'acento'
    Add-Nodo $s 764 216 200 66 'AsistenteIA.cs' 'bucle de hasta 6 vueltas' -Mono
    Add-Nodo $s 994 216 210 66 'Modelo de lenguaje' 'claude-sonnet-5' -Estilo 'mal'
    foreach ($a in @(@(274,300), @(504,530), @(734,760), @(964,990))) {
        Add-Linea $s $a[0] 249 $a[1] 249 -Color $c.nAcentoB -Flecha -Grosor 2 | Out-Null
    }

    Add-Linea $s 1099 282 1099 330 -Color $c.nAcentoB -Grosor 2 | Out-Null
    Add-Linea $s 1099 330 874 330 -Color $c.nAcentoB -Grosor 2 | Out-Null
    Add-Linea $s 874 330 874 364 -Color $c.nAcentoB -Flecha -Grosor 2 | Out-Null
    Add-Txt $s 890 296 300 14 'Pide una herramienta' -Size 10.5 -Bold -Color $c.nCeja -Espaciado .8 -Caps | Out-Null

    Add-Txt $s 74 343 700 14 'Camino de los datos · siempre de derecha a izquierda' -Size 10.5 -Bold -Color $c.nCeja -Espaciado .8 -Caps | Out-Null
    Add-Nodo $s 774 368 200 66 '4 herramientas'   'parámetros tipados, sin SQL' -Estilo 'acento'
    Add-Nodo $s 512 368 230 66 'AsistenteDatos.cs' 'el único punto que abre esa conexión' -Estilo 'ambar' -Mono
    Add-Nodo $s 284 368 196 66 '13 procedimientos' 'los únicos con GRANT EXECUTE'
    Add-Nodo $s 74  368 180 66 'BDCUMPLEHN'        'encadenamiento de propiedad' -Estilo 'ok' -Mono
    foreach ($a in @(@(774,748), @(512,486), @(284,258))) {
        Add-Linea $s $a[0] 401 $a[1] 401 -Color $c.nBorde -Flecha -Grosor 1.6 | Out-Null
    }

    Add-Linea $s 627 434 627 452 -Color $c.nAmbarB -Punteada | Out-Null
    Add-Linea $s 627 452 424 452 -Color $c.nAmbarB -Punteada | Out-Null
    Add-Linea $s 424 452 424 466 -Color $c.nAmbarB -Punteada | Out-Null
    Add-Caja $s 64 468 700 60 -Relleno $c.nAmbar -Borde $c.nAmbarB -Redonda | Out-Null
    Add-Txt $s 84 480 660 16 'login cumplehn_ia' -Size 12 -Font $script:Mono -Color $c.nAmbarT | Out-Null
    Add-Txt $s 84 500 660 18 'sin db_datareader · cero permisos sobre tablas · DENY sobre las que identifican personas' -Size 11.5 -Color $c.nAmbarT | Out-Null

    Add-Caja $s 794 468 422 60 -Relleno $c.nCaja -Borde $c.nBorde -Redonda | Out-Null
    Add-Txt $s 814 480 382 16 'ConsultasIA' -Size 12 -Font $script:Mono -Color $c.nMono | Out-Null
    Add-Txt $s 814 500 382 18 'la escribe el Web Service, con su conexión normal' -Size 11.5 -Color $c.nBajada | Out-Null

    Add-Pie $s 'El handler vive en el frontend a propósito: abrir CORS sobre el método que gasta dinero sería dejarlo al alcance de cualquier sitio. La conexión SQL se cierra **antes** de llamar al modelo.' -Noche

    # ──────────────────────────────── 15 · Herramientas y permisos ─────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Asistente · Superficie de datos'
    Add-Titulo $s 'Lo que el asistente puede saber lo deciden las columnas, no los permisos' -Size 38 -H 96

    Add-Etiqueta $s 64 178 563 'Las cuatro herramientas · cada una es un procedimiento almacenado'
    $herr = @(
        @{ n = 'consultar_tablero'; d = 'Los diez indicadores del tablero, con los mismos filtros de la página.' },
        @{ n = 'buscar_propuestas'; d = 'Búsqueda por texto, categoría, estado, candidatura o partido. Máximo 50 filas.' },
        @{ n = 'ficha_candidato';   d = 'La ficha pública de una candidatura — sin correo ni teléfono.' },
        @{ n = 'catalogos';         d = 'Campañas, categorías, partidos y departamentos.' }
    )
    $y = 200
    foreach ($h in $herr) {
        Add-Caja $s 64 $y 563 62 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        Add-Filo $s 64 ($y + 3) 56 $c.azul | Out-Null
        Add-Txt $s 82 ($y + 11) 529 16 $h.n -Size 13 -Font $script:Mono -Color $c.azul | Out-Null
        Add-Txt $s 82 ($y + 31) 529 24 $h.d -Size 13 -Color $c.tinta2 -Interlineado 1.3 | Out-Null
        $y += 70
    }
    Add-Txt $s 64 488 563 46 'Agregar una capacidad nueva es agregar un procedimiento con su `GRANT`, nunca una consulta suelta. **Ninguno proyecta con** `SELECT *`.' `
        -Size 14 -Color $c.tinta2 -Interlineado 1.4 | Out-Null

    Add-Caja $s 653 178 563 380 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
    Add-Etiqueta $s 671 194 527 'Segunda capa · el login cumplehn_ia'
    $perm = @(
        @{ s = '✓'; col = $c.verde;     t = '`EXECUTE` sobre 13 procedimientos, y nada más. Llega a las tablas por **encadenamiento de propiedad**.' },
        @{ s = '✗'; col = $c.escarlata; t = 'No pertenece a `db_datareader` ni tiene permiso sobre ninguna tabla.' },
        @{ s = '✗'; col = $c.escarlata; t = '`DENY` sobre `Usuarios`, `Valoraciones`, `Comentarios`, `EncuestaVotos`, `Auditoria` y `ConsultasIA`.' }
    )
    $y = 218
    foreach ($p in $perm) {
        Add-Txt $s 671 $y 20 18 $p.s -Size 13 -Bold -Color $p.col | Out-Null
        $alto = if ($p.t.Length -gt 80) { 42 } else { 24 }
        Add-Txt $s 695 $y 503 $alto $p.t -Size 13.5 -Color $c.tinta2 -Interlineado 1.35 | Out-Null
        $y += $alto + 10
    }
    Add-Txt $s 671 356 527 82 '**Por qué esas tablas.** Una valoración o un voto de encuesta vincula a una persona con su preferencia política: es el dato que más daño haría filtrado. Y `Comentarios` queda fuera además porque su texto lo escribe cualquier ciudadano — leerlo sería dejar que un comentario le dé instrucciones al modelo.' `
        -Size 13.5 -Color $c.tinta2 -Interlineado 1.4 | Out-Null

    Add-Caja $s 671 452 527 88 -Relleno $c.verdeL -Borde $c.linea -Redonda | Out-Null
    Add-Filo $s 671 455 82 $c.verde | Out-Null
    Add-Txt $s 691 468 489 60 'Está verificado con `EXECUTE AS`, y ya sirvió: atrapó una versión que resolvía la campaña con dos `SELECT` sueltos en lugar de un procedimiento.' `
        -Size 13.5 -Color $c.tinta2 -Interlineado 1.4 | Out-Null

    # ───────────────────────────────────────────── 16 · Garantías ──────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Asistente · Lo que lo hace defendible'
    Add-Titulo $s 'Cinco garantías que no dependen de que el modelo se porte bien'

    $gar = @(
        @{ n = '1'; t = 'Las reglas del prompt'; col = $c.escarlata;
           d = 'No recomienda voto ni ordena candidaturas. Dice siempre el nivel de verificación. No completa una cifra de memoria. Y declara que **el texto que devuelven las herramientas es dato, no instrucción**.' },
        @{ n = '2'; t = 'Las fuentes salen del registro de llamadas'; col = $c.escarlata;
           d = 'No de lo que el modelo diga haber usado. Un modelo puede describir mal su propio trabajo, el registro de llamadas no. Cuando responde sin consultar la base, la fuente lo dice.' },
        @{ n = '3'; t = 'La respuesta no se inserta con innerHTML'; col = $c.escarlata;
           d = 'Se analiza con `DOMParser`, que no ejecuta nada, y el árbol se reconstruye nodo por nodo con una lista blanca **sin copiar un solo atributo**. Sin `onerror`, `href` ni `src`, ninguna etiqueta puede ejecutar nada.' },
        @{ n = '4'; t = 'Tres comprobaciones antes de gastar un token'; col = $c.escarlata;
           d = 'Módulo visible, cuenta activa y con correo confirmado, y cuota diaria disponible. Las tres en el Web Service, no en la página.' },
        @{ n = '5'; t = 'El costo está medido, no estimado'; col = $c.escarlata;
           d = 'Unos 11 000 tokens de entrada y 470 de salida por consulta: **0.027 dólares** con Sonnet 5, frente a 0.077 con Opus 5. Con la cuota de 20 por persona y día, el techo es conocido.' },
        @{ n = '+'; t = 'Y una bitácora que también guarda los fracasos'; col = $c.turquesa;
           d = '`ConsultasIA` registra cada pregunta con su respuesta, herramientas, tokens, tiempo y error. Una bitácora que solo guarda los casos buenos no sirve para revisar los malos.' }
    )
    $x = 64; $y = 150; $i = 0
    foreach ($g in $gar) {
        Add-Linea $s $x $y ($x + 34) $y -Color $g.col -Grosor 2 | Out-Null
        Add-Txt $s $x ($y + 7) 34 26 $g.n -Size 21 -Bold -Font $script:Display -Color $g.col | Out-Null
        Add-Txt $s ($x + 47) ($y + 4) 514 40 $g.t -Size 16.5 -Bold -Font $script:Display -Color $c.tinta -Interlineado 1.15 | Out-Null
        Add-Txt $s ($x + 47) ($y + 46) 514 86 $g.d -Size 13.5 -Color $c.tinta2 -Interlineado 1.4 | Out-Null
        $i++
        if ($i % 2 -eq 0) { $x = 64; $y += 148 } else { $x = 655 }
    }

    # ───────────────────────────────────────────── 17 · Encuestas ──────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Participación ciudadana · Encuestas de percepción'
    Add-Titulo $s 'Una encuesta que dice de qué no es evidencia'

    Add-Img $s 64 144 1152 253 (Join-Path $Img 'encuesta-card.jpg') | Out-Null

    Add-Caja $s 64 412 374 152 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
    Add-Filo $s 64 415 146 $c.turquesa | Out-Null
    Add-Txt $s 82 424 340 22 'Las reglas viven en la base' -Size 16 -Bold -Font $script:Display -Color $c.tinta | Out-Null
    $reglas = @(
        'Un voto por persona y encuesta, garantizado por un índice único.',
        'Los conteos **no se guardan**: se derivan con `COUNT`.',
        'El estado se deriva de las fechas, no de una columna que pueda mentir.'
    )
    $y = 450
    foreach ($r in $reglas) {
        $p = Add-Caja $s 84 ($y + 5) 7 7 -Relleno $c.turquesa; $p.AutoShapeType = 9
        Add-Txt $s 99 $y 323 34 $r -Size 12.5 -Color $c.tinta2 -Interlineado 1.3 | Out-Null
        $y += 36
    }

    Add-Tarjeta $s 453 412 374 152 'Solo la administración las crea' `
        'El candidato no. Quien redacta las opciones controla el marco, y una encuesta escrita por una parte interesada dentro de una plataforma que se declara neutral sería un instrumento de campaña. El ciudadano tampoco, porque no hay moderación previa.' `
        -Filo $c.escarlata -SizeT 16 -SizeC 13

    Add-Tarjeta $s 842 412 374 152 'Conteo y porcentaje, los dos' `
        'El porcentaje solo esconde de cuánta gente sale: el sesenta por ciento de cinco respuestas no es el sesenta por ciento de quinientas. El conteo solo obliga a dividir de cabeza para comparar.' `
        -Filo $c.ambarVivo -SizeT 16 -SizeC 13

    Add-Pie $s 'El resultado de estas encuestas **nunca sobrescribe** el interés medido en la encuesta metodológica del proyecto: aquella tiene n = 150 con muestreo definido, y esta es una muestra autoseleccionada.' -Y 588

    # ──────────────────────────────────────────────── 18 · Cierre ──────────
    $s = New-Lamina $Pres 'noche'
    Add-Ceja $s 'Estado del proyecto' -Noche

    $met = @(
        @{ n = '34,046'; d = "líneas de código propio`rC#, T-SQL, marcado, CSS y JS" },
        @{ n = '23';     d = "tablas, con 7 vistas, 3 funciones`ry 35 llaves foráneas" },
        @{ n = '44';     d = "procedimientos almacenados`ry 51 métodos de servicio" },
        @{ n = '6 / 6';  d = "módulos comprometidos`ren el FO-GR-013, entregados" }
    )
    $x = 64
    foreach ($m in $met) {
        Add-Txt $s $x 108 270 48 $m.n -Size 38 -Bold -Color $c.blanco -Interlineado 1 | Out-Null
        Add-Txt $s $x 162 270 44 $m.d -Size 12.5 -Color $c.nPie -Interlineado 1.35 | Out-Null
        $x += 288
    }

    Add-Etiqueta $s 64 232 560 'Lo que queda pendiente, dicho antes de que lo pregunten' -Color $c.ambarVivo
    Add-Txt $s 64 256 560 120 'El guardado del perfil y de los proyectos desde el panel del candidato valida del lado del servidor pero todavía no persiste. El código de usuario de valoraciones y comentarios lo envía el frontend, así que el servicio confía en él: se resuelve validando un token de sesión. Las contraseñas siguen sin salt.' `
        -Size 15 -Color $c.nBajada -Interlineado 1.45 | Out-Null

    Add-Etiqueta $s 656 232 560 'Trabajo futuro con argumento, no con lista de deseos' -Color $c.nCeja
    Add-Txt $s 656 256 560 120 'Verificación por SMS: evaluada y descartada por costo, no por debilidad — 0.33 dólares por mensaje, unos 50 para la muestra de 150. Desde marzo de 2025 CONATEL exige registro biométrico para una línea, así que controlar un número se acerca mucho más a la identidad que controlar un correo.' `
        -Size 15 -Color $c.nBajada -Interlineado 1.45 | Out-Null

    Add-Linea $s 64 400 1216 400 -Color $c.nBorde -Grosor 0.75 | Out-Null
    $frase = Add-Txt $s 64 428 760 140 "CumpleHN no dice quién cumple mejor.`rOrdena la evidencia para que cada quien lo juzgue." `
        -Size 34 -Bold -Font $script:Display -Color $c.blanco -Interlineado 1.25
    $t = $frase.TextFrame.TextRange.Text
    $ini = $t.IndexOf('cada quien lo juzgue')
    if ($ini -ge 0) { $frase.TextFrame.TextRange.Characters($ini + 1, 20).Font.Color.RGB = (C 'F0B552') }

    # ══════════════════════════════════ Láminas de respaldo ════════════════

    # R1 · Antecedentes
    $s = New-Lamina $Pres 'claro'; Add-SelloRespaldo $s
    Add-Ceja $s 'Respaldo · Antecedentes'
    Add-Titulo $s 'Rastreadores de promesas en el mundo' -Size 32 -H 44
    $anchos = @(210.0, 190.0, 400.0, 352.0)
    Add-FilaTabla $s 64 140 $anchos @('Iniciativa','País y año','Qué hace','Qué no cubre') -Encabezado
    $filas = @(
        @('**PolitiFact**','Estados Unidos, desde 2007 · Poynter Institute','Verificación de declaraciones públicas y rastreador de promesas con seis categorías ordenadas, incluidos estados intermedios.','De ahí se toma el esquema de estados de cumplimiento de CumpleHN.'),
        @('**Chequeado**','Argentina, desde 2010','Verificación del discurso público. Su modelo se replicó por aliados en la región.','No cubre Honduras ni el seguimiento longitudinal por candidatura.'),
        @('**Del Dicho al Hecho**','Chile · Fundación Ciudadano Inteligente','Monitoreo del cumplimiento de promesas presidenciales y legislativas.','Es el antecedente más cercano en propósito. No opera en Honduras.'),
        @('**IAIP · Portal Único de Transparencia**','Honduras · Decreto 170-2006','Acceso a información pública mediante solicitud, con 10 días hábiles prorrogables y recurso de revisión.','Es acceso a documentos, no un seguimiento comparable de promesas por candidatura.')
    )
    $y = 172
    foreach ($f in $filas) { Add-FilaTabla $s 64 $y $anchos $f -Alto 74; $y += 74 }
    Add-Txt $s 64 480 1152 60 'Honduras participa en la **Open Government Partnership** desde 2011, con cinco planes de acción, el más reciente 2023-2025. En el Índice de Percepción de la Corrupción 2024 obtuvo 22 puntos sobre 100, en la posición 154 de 180.' `
        -Size 14 -Color $c.tinta2 -Interlineado 1.45 | Out-Null

    # R2 · Metodología
    $s = New-Lamina $Pres 'claro'; Add-SelloRespaldo $s
    Add-Ceja $s 'Respaldo · Metodología'
    Add-Titulo $s 'Enfoque, muestra e instrumento' -Size 32 -H 44
    $def = @(
        @{ t = 'Enfoque cuantitativo, tipo descriptivo'; d = 'Hernández-Sampieri, Fernández-Collado y Baptista-Lucio (2014): recolección de datos para probar hipótesis con base en la medición numérica y el análisis estadístico. Secuencial y probatorio.' },
        @{ t = 'Población objetivo'; d = 'La ciudadanía hondureña habilitada para votar.' },
        @{ t = 'Población accesible'; d = 'Estudiantes universitarios hondureños mayores de 18 años: votantes habilitados, accesibles para aplicar el instrumento y alfabetizados digitalmente, que es el perfil del usuario potencial de una solución web.' },
        @{ t = 'Técnica e instrumento'; d = 'Encuesta única, cuestionario estructurado de 18 ítems cerrados con escalas Likert de 1 a 5, aplicado en línea del 12 al 21 de junio de 2026. Es la única fuente primaria: no se realizaron entrevistas.' }
    )
    $y = 150
    foreach ($d in $def) {
        Add-Txt $s 64 $y 700 20 $d.t -Size 14.5 -Bold -Color $c.tinta | Out-Null
        Add-Txt $s 64 ($y + 22) 700 76 $d.d -Size 13.5 -Color $c.tinta2 -Interlineado 1.4 | Out-Null
        $y += if ($d.d.Length -gt 120) { 108 } else { 60 }
    }
    Add-Caja $s 816 150 400 330 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
    Add-Filo $s 816 153 324 $c.azul | Out-Null
    Add-Etiqueta $s 834 166 364 'Tamaño de muestra'
    Add-Txt $s 834 188 364 30 'n = Z² · p · q / e²' -Size 22 -Bold -Font $script:Mono -Color $c.azul | Out-Null
    $mu = @(
        @('Nivel de confianza','95 % · Z = 1.96'),
        @('Probabilidades','p = q = 0.5, que maximiza la varianza'),
        @('Margen de error','8 %'),
        @('**Resultado**','**150 encuestados**, alcanzados')
    )
    $y = 232
    foreach ($m in $mu) {
        Add-Txt $s 834 $y 140 34 $m[0] -Size 13 -Color $c.tinta2 -Interlineado 1.3 | Out-Null
        Add-Txt $s 982 $y 216 34 $m[1] -Size 13 -Color $c.tinta2 -Interlineado 1.3 | Out-Null
        Add-Linea $s 834 ($y + 32) 1198 ($y + 32) -Color $c.linea2 -Grosor 0.75 | Out-Null
        $y += 40
    }
    Add-Txt $s 834 400 364 68 'Muestreo **no probabilístico por conveniencia**. Los resultados se circunscriben al grupo estudiado y no se generalizan estadísticamente a la población hondureña.' `
        -Size 13 -Color $c.tinta2 -Interlineado 1.4 | Out-Null

    # R3 · Limitaciones
    $s = New-Lamina $Pres 'claro'; Add-SelloRespaldo $s
    Add-Ceja $s 'Respaldo · Limitaciones declaradas' -Color $c.escarlata
    Add-Titulo $s 'Lo que este trabajo no logra demostrar' -Size 32 -H 44
    $lim = @(
        @{ t = 'La hipótesis no se validó empíricamente'; d = 'H1 afirma que la plataforma permitirá decidir el voto con fundamentos cuantitativos. Confirmarlo requeriría medir el uso de la herramienta una vez implementada, y esta fase midió la necesidad, no el efecto.' },
        @{ t = 'La precisión del componente de IA no se midió'; d = 'La pregunta específica 5 pide evaluar efectividad y precisión. El instrumento midió **interés y confianza declarada**, que es otra cosa. Medirla exige un conjunto de preguntas con respuesta conocida — y los registros de `ConsultasIA` ya guardan el material.' },
        @{ t = 'Las promesas recurrentes no se analizaron en el discurso'; d = 'La pregunta específica 3 se responde con el **interés declarado** de los encuestados, no con un análisis de contenido de discursos y planes de gobierno.' },
        @{ t = 'Los datos de la plataforma son de demostración'; d = 'Las candidaturas y propuestas cargadas son ficticias a propósito: usar partidos reales con candidatos inventados insinuaría afiliaciones que nadie declaró. Las cifras del tablero ilustran los indicadores, no diagnostican a la política hondureña.' }
    )
    $x = 64; $y = 150; $i = 0
    foreach ($l in $lim) {
        Add-Tarjeta $s $x $y 569 172 $l.t $l.d -Filo $c.escarlata -SizeT 16 -SizeC 13.5
        $i++
        if ($i % 2 -eq 0) { $x = 64; $y += 186 } else { $x = 647 }
    }
    Add-Txt $s 64 530 1152 24 'Declarar estas limitaciones es la misma política que sostiene la plataforma: cada cifra dice de qué no es evidencia.' `
        -Size 14 -Color $c.tinta2 | Out-Null

    # R4 · Seguridad
    $s = New-Lamina $Pres 'claro'; Add-SelloRespaldo $s
    Add-Ceja $s 'Respaldo · Seguridad y OWASP'
    Add-Titulo $s 'Resuelto, y deuda conocida' -Size 32 -H 44
    Add-Etiqueta $s 64 148 563 'Resuelto' -Color $c.verde
    $ok = @(
        '**A01 · Control de acceso roto.** La comprobación vive en la clase base `PaginaSegura` y se repite dentro de cada procedimiento de escritura. Verificado llamando al ASMX directamente.',
        '**A03 · Inyección.** Todo el acceso a datos usa `SqlCommand` con parámetros. Nunca se concatena entrada del usuario dentro del SQL.',
        '**Cross-site scripting.** El texto de usuario se rinde con una sintaxis que escapa. La respuesta del modelo se reconstruye con lista blanca y sin atributos.',
        '**Redirección abierta.** `Sesion.EsDestinoSeguro` solo acepta rutas locales en el parámetro de regreso, y vive en un solo lugar.'
    )
    $y = 170
    foreach ($o in $ok) {
        Add-Caja $s 64 $y 563 84 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        Add-Txt $s 82 ($y + 13) 529 62 $o -Size 13 -Color $c.tinta2 -Interlineado 1.4 | Out-Null
        $y += 92
    }
    Add-Etiqueta $s 653 148 563 'Deuda conocida, anotada para el anexo' -Color $c.escarlata
    $deuda = @(
        '**El servicio confía en el código de usuario que recibe.** Quien llame al ASMX directamente puede valorar o comentar en nombre de otro. Se resuelve validando un token de sesión.',
        '**Contraseñas sin salt.** SHA-256 en hexadecimal. Dos personas con la misma contraseña producen el mismo hash.',
        '**Enumeración de correos en el registro.** Decir «ese correo ya está registrado» revela que la cuenta existe. Callarlo dejaría a quien ya tiene cuenta sin saber por qué falla.',
        '**Hash con tildes al crear cuentas de candidatura.** El procedimiento cifra con la página de códigos de la base y el backend con UTF-8: una contraseña con tilde crea la cuenta y el acceso la rechaza siempre.'
    )
    $y = 170
    foreach ($d in $deuda) {
        Add-Caja $s 653 $y 563 84 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        Add-Filo $s 653 ($y + 3) 78 $c.escarlata | Out-Null
        Add-Txt $s 671 ($y + 13) 529 62 $d -Size 13 -Color $c.tinta2 -Interlineado 1.4 | Out-Null
        $y += 92
    }

    # R5 · Los doce indicadores
    $s = New-Lamina $Pres 'claro'; Add-SelloRespaldo $s
    Add-Ceja $s 'Respaldo · Los doce indicadores'
    Add-Titulo $s 'Qué calcula cada gráfico, y con qué procedimiento' -Size 32 -H 44
    $izq = @(
        @('Seis tarjetas KPI, cada una con su denominador','spAnaliticaResumen'),
        @('Principales hallazgos, calculados sobre la selección','Modelos/Analitica.cs'),
        @('Demanda ciudadana contra oferta programática','spAnaliticaCategorias'),
        @('Estado de cumplimiento, barra apilada al 100 %','spAnaliticaEstados'),
        @('Propuestas por partido, conteo bruto','spAnaliticaPartidos'),
        @('Densidad programática, propuestas por candidatura','spAnaliticaPartidos')
    )
    $der = @(
        @('Signo de la participación, dona SVG','spAnaliticaParticipacion'),
        @('Participación por tipo de contenido','spAnaliticaParticipacion'),
        @('Ranking de candidaturas por saldo','spAnaliticaCandidatos'),
        @('Actividad diaria, área SVG de dos series','spAnaliticaActividad'),
        @('Cobertura territorial, 18 departamentos','spAnaliticaTerritorio'),
        @('Verificación por tipo de contenido','spAnaliticaVerificacion')
    )
    $anchos2 = @(310.0, 253.0)
    foreach ($bloque in @(@{x=64; d=$izq}, @{x=653; d=$der})) {
        Add-FilaTabla $s $bloque.x 148 $anchos2 @('Indicador','Procedimiento') -Encabezado
        $y = 176
        foreach ($f in $bloque.d) {
            Add-Txt $s $bloque.x $y 298 32 $f[0] -Size 12.5 -Color $c.tinta2 -Interlineado 1.3 | Out-Null
            Add-Txt $s ($bloque.x + 310) $y 245 32 $f[1] -Size 11.5 -Font $script:Mono -Color $c.azul | Out-Null
            Add-Linea $s $bloque.x ($y + 32) ($bloque.x + 551) ($y + 32) -Color $c.linea2 -Grosor 0.75 | Out-Null
            $y += 40
        }
    }
    Add-Caja $s 64 434 1152 110 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
    Add-Filo $s 64 437 104 $c.ambarVivo | Out-Null
    Add-Txt $s 84 452 1112 76 '**Lo que el tablero deliberadamente no muestra:** la comparación contra el período anterior con su flecha y su porcentaje de variación. Los registros de participación abarcan pocos días — esa variación sería una cifra inventada con apariencia de dato. En su lugar cada KPI se compara contra su propio denominador: «1 de 11 verificadas», «3 de 18 departamentos».' `
        -Size 14 -Color $c.tinta2 -Interlineado 1.45 | Out-Null

    # R6 · Administración
    $s = New-Lamina $Pres 'claro'; Add-SelloRespaldo $s
    Add-Ceja $s 'Respaldo · Área de administración'
    Add-Titulo $s 'Cuatro etapas, todas con bitácora' -Size 32 -H 44
    $picAdmin = Add-Img $s 64 148 700 583 (Join-Path $Img 'admin-verificacion.jpg')
    $picAdmin.PictureFormat.CropBottom = (583 - 416) * 0.75
    $et = @(
        @{ t = 'Verificación'; filo = $c.turquesa;   d = 'Una sola cola con candidaturas, propuestas y publicaciones juntas, ordenada por lo más antiguo sin verificar. Quien revisa trabaja por antigüedad, no por tipo.' },
        @{ t = 'Moderación';   filo = $c.escarlata;  d = 'Baja lógica con motivo obligatorio. El filtro `activo = 1` vive en dos lugares y solo dos, para que una consulta nueva no pueda olvidarlo.' },
        @{ t = 'Catálogos';    filo = $c.ambarVivo;  d = 'Alta y edición de partidos, campañas y candidaturas. Nada se borra: se desactiva. Los slugs se generan solo en el alta, porque regenerarlos rompería todo enlace ya compartido.' },
        @{ t = 'Interruptores de módulos'; filo = $c.verde; d = 'Ocultar no es dar de baja: el contenido sigue en la base y quien administra lo sigue viendo, marcado como oculto al público.' }
    )
    $y = 148
    foreach ($e in $et) {
        Add-Tarjeta $s 790 $y 426 112 $e.t $e.d -Filo $e.filo -SizeT 15 -SizeC 12
        $y += 118
    }
}
