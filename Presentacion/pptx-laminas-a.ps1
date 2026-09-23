# ============================================================================
#  Láminas 1 a 12 de la versión editable.
#  Coordenadas en el espacio de 1280x720 del deck HTML.
# ============================================================================

function Build-LaminasA {
    param($Pres, [string]$Img)

    $c = $script:Col

    # ─────────────────────────────────────────────── 1 · Portada ───────────
    $s = New-Lamina $Pres 'claro'

    # Sello: aro con el arco de la guacamaya y cuatro barras que son una
    # promesa avanzando de declarada a verificada.
    $aro = Add-Caja $s 980 96 236 236 -Borde $c.linea -GrosorBorde 1
    $aro.AutoShapeType = 9                                   # 9 = óvalo
    $arco = $s.Shapes.AddShape(9, 992 * 0.75, 108 * 0.75, 212 * 0.75, 212 * 0.75)
    $arco.Fill.Visible = 0
    $arco.Line.ForeColor.RGB = $c.turquesa
    $arco.Line.Weight = 2.5
    $barras = @(
        @{ y = 170; w = 31; col = $c.escarlata },
        @{ y = 190; w = 52; col = $c.ambarVivo },
        @{ y = 210; w = 73; col = $c.verde },
        @{ y = 230; w = 42; col = $c.turquesa }
    )
    foreach ($b in $barras) {
        $r = Add-Caja $s 1058 $b.y $b.w 11 -Relleno $b.col -Redonda
        $r.Adjustments.Item(1) = 0.5
    }

    Add-Txt $s 64 180 900 18 'Centro Universitario Tecnológico · CEUTEC · Facultad de Ingeniería' `
        -Size 12 -Bold -Color $c.turquesa -Espaciado 1.4 -Caps | Out-Null

    $marca = Add-Txt $s 64 202 700 92 'CumpleHN' -Size 74 -Bold -Font $script:Display -Color $c.tinta -Interlineado 1
    $marca.TextFrame.TextRange.Characters(7, 2).Font.Color.RGB = $c.escarlata

    Add-Txt $s 64 300 560 108 'Plataforma web para el seguimiento ciudadano de promesas políticas en Honduras' `
        -Size 27 -Bold -Font $script:Display -Color $c.tinta2 -Interlineado 1.22 | Out-Null

    Add-Linea $s 64 424 1152 424 -Color $c.linea | Out-Null

    $meta = @(
        @{ x = 64;  t = 'Roy Guillermo Chávez Espinal'; d = "Cuenta 31521560`rIngeniería en Informática" },
        @{ x = 300; t = 'Asesor';                       d = 'Ing. Rafael Cerrato' },
        @{ x = 470; t = 'Defensa de proyecto de graduación'; d = 'Tegucigalpa, M.D.C. · 2026' }
    )
    foreach ($m in $meta) {
        Add-Txt $s $m.x 442 300 20 $m.t -Size 14.5 -Bold -Color $c.tinta | Out-Null
        Add-Txt $s $m.x 466 300 50 $m.d -Size 14 -Color $c.suave -Interlineado 1.4 | Out-Null
    }

    # ────────────────────────────────────────────── 2 · El problema ────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'El problema'
    Add-Titulo $s 'La información existe. Está dispersa.'
    Add-Bajada $s 'Lo que un candidato promete queda registrado en muchos lugares a la vez, y en ninguno de forma comparable. No hay un esquema común de clasificación, ni indicadores de cumplimiento, ni una manera de contrastar lo dicho con lo hecho.' 134 940 62

    $fuentes = @(
        @{ x = 87;  y = 246; w = 207; t = 'Discursos de campaña' },
        @{ x = 444; y = 232; w = 314; t = 'Entrevistas en radio y televisión' },
        @{ x = 882; y = 266; w = 158; t = 'Redes sociales' },
        @{ x = 168; y = 328; w = 100; t = 'Debates' },
        @{ x = 421; y = 358; w = 363; t = 'Planes de gobierno inscritos ante el CNE' },
        @{ x = 790; y = 344; w = 223; t = 'Medios de comunicación' },
        @{ x = 225; y = 418; w = 232; t = 'Comparecencias públicas' },
        @{ x = 709; y = 428; w = 281; t = 'Portales oficiales del Estado' }
    )
    foreach ($f in $fuentes) {
        Add-Caja $s $f.x $f.y $f.w 34 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        $p = Add-Caja $s ($f.x + 14) ($f.y + 14) 7 7 -Relleno $c.tenue
        $p.AutoShapeType = 9
        Add-Txt $s ($f.x + 29) ($f.y + 9) ($f.w - 40) 18 $f.t -Size 14.5 -Color $c.tinta2 | Out-Null
    }

    Add-Cita $s 64 496 1080 96 'Reunir lo que un solo candidato prometió obliga a un esfuerzo individual que casi nadie hace, y esa dificultad empuja la decisión hacia la percepción.' 'Capítulo II · Planteamiento del problema' -Size 22
    Add-Pie $s 'Restricción de alcance acordada con el asesor: la plataforma documenta la **dispersión** de la información, sin pronunciarse sobre la existencia o suficiencia de los mecanismos institucionales.'

    # ─────────────────────────────────────── 3 · No es una intuición ───────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Evidencia · Encuesta del proyecto'
    Add-Titulo $s 'No es una intuición. Está medido.'
    Add-Bajada $s 'Instrumento de 18 ítems cerrados aplicado en junio de 2026 a 150 personas. Escala Likert de 1 a 5, sin valores faltantes.' 134 940 40

    $datos = @(
        @{ n = '84.7 %'; d = 'dice que la información sobre promesas y gestión está **dispersa entre muchas fuentes**'; p = 84.7; col = $c.azul;      filo = $null },
        @{ n = '75.3 %'; d = 'no logra encontrar **si un candidato cumplió** lo que prometió antes';                   p = 75.3; col = $c.azul;      filo = $null },
        @{ n = '74.0 %'; d = 'no la encuentra **organizada ni fácil de comparar**';                                    p = 74.0; col = $c.azul;      filo = $null },
        @{ n = '11.3 %'; d = '— y solo esa fracción **sabe dónde buscar** información confiable';                      p = 11.3; col = $c.escarlata; filo = $c.escarlata }
    )
    $x = 64
    foreach ($d in $datos) {
        Add-Caja $s $x 200 277 200 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        if ($null -ne $d.filo) { Add-Filo $s $x 203 194 $d.filo | Out-Null }
        Add-Txt $s ($x + 18) 216 241 74 $d.n -Size 60 -Bold -Color $d.col -Interlineado 1 | Out-Null
        $barCol = if ($null -ne $d.filo) { $c.escarlata } else { $c.turquesa }
        Add-Barra $s ($x + 18) 300 241 $d.p $barCol | Out-Null
        Add-Txt $s ($x + 18) 316 241 74 $d.d -Size 14 -Color $c.tinta2 -Interlineado 1.35 | Out-Null
        $x += 291.5
    }

    Add-Txt $s 64 428 1080 30 '**64.0 %** no sabía que existen mecanismos institucionales de seguimiento a la gestión pública, y **51.3 %** no puede nombrar una sola promesa específica de la última campaña.' `
        -Size 17 -Color $c.tinta2 -Interlineado 1.4 | Out-Null

    Add-Pie $s 'n = 150 · junio de 2026 · muestreo no probabilístico por conveniencia sobre estudiantes universitarios hondureños mayores de 18 años. Los resultados describen a quienes respondieron, no a la población hondureña.'

    # ──────────────────────────────────────────── 4 · Hay demanda ──────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Evidencia · Encuesta del proyecto'
    Add-Titulo $s 'Y hay demanda de la herramienta.'

    $dem = @(
        @{ e = 'La usaría antes de votar'; n = '84.7 %'; col = $c.verde; filo = $c.verde;
           d = 'usaría una plataforma que muestre estadísticamente el cumplimiento por candidato. Media de 4.31 sobre 5, **la más alta de todo el instrumento**.' },
        @{ e = 'Quiere preguntar en lenguaje natural'; n = '78.7 %'; col = $c.azul; filo = $c.azul;
           d = 'quiere poder preguntar «¿qué candidatos cumplieron sus promesas de educación?» y recibir una respuesta basada en datos.' },
        @{ e = 'Confiaría en la IA — con una condición'; n = '77.3 %'; col = $c.ambar; filo = $c.ambarVivo;
           d = 'confiaría en las respuestas de un asistente de inteligencia artificial **si estuvieran respaldadas por fuentes verificables**.' }
    )
    $x = 64
    foreach ($d in $dem) {
        Add-Caja $s $x 152 374 218 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        Add-Filo $s $x 155 212 $d.filo | Out-Null
        Add-Etiqueta $s ($x + 18) 168 338 $d.e
        Add-Txt $s ($x + 18) 190 338 52 $d.n -Size 42 -Bold -Color $d.col -Interlineado 1 | Out-Null
        Add-Txt $s ($x + 18) 250 338 108 $d.d -Size 14 -Color $c.tinta2 -Interlineado 1.4 | Out-Null
        $x += 388.7
    }

    Add-Cita $s 64 400 1080 96 'Esa condición no es un detalle del instrumento: es el requisito que gobierna todo el diseño del módulo de IA.' 'Por eso el asistente responde siempre con su lista de fuentes, y esas fuentes se arman con las consultas que realmente se ejecutaron.' -Size 22
    Add-Pie $s 'Ítems 14, 15 y 16 del instrumento · acuerdo = respuestas 4 y 5 en la escala Likert · n = 150.'

    # ───────────────────────────────────────────── 5 · Objetivo ────────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Capítulo III · Objetivo general'
    Add-Titulo $s 'Decidir con fundamentos, no con percepciones.'

    Add-Cita $s 64 150 660 244 'Facilitar el seguimiento ciudadano del cumplimiento de promesas y de la trayectoria de gestión de los candidatos y funcionarios políticos en Honduras, mediante el desarrollo de una plataforma web que centralice y visualice estadísticamente esta información, con el fin de apoyar a la ciudadanía en la toma de decisiones fundamentadas en evidencia cuantitativa al momento de votar.' `
        'Objetivo general, texto íntegro' -Filo $c.azul -Size 19

    Add-Etiqueta $s 768 150 448 'Y lo que CumpleHN no hace' -Color $c.escarlata
    $no = @(
        'No rankea candidaturas por calidad.',
        'No recomienda por quién votar.',
        'No emite un veredicto sobre quién cumple mejor.',
        'No deja que la candidatura se asigne su propio estado de cumplimiento.',
        'No presenta como hecho comprobado lo que solo está declarado.'
    )
    $y = 176
    foreach ($n in $no) {
        Add-Caja $s 768 ($y + 8) 15 2 -Relleno $c.escarlata | Out-Null
        Add-Txt $s 798 $y 418 44 $n -Size 16.5 -Color $c.tinta2 -Interlineado 1.3 | Out-Null
        $y += if ($n.Length -gt 48) { 52 } else { 32 }
    }

    Add-Cita $s 64 430 1080 76 'Entrega la evidencia organizada para que cada persona la pondere según el criterio que considere de mayor peso — seguridad, salud, educación, economía, infraestructura o transparencia — y decida por su cuenta.' `
        -Filo $c.verde -Size 21

    # ────────────────────────────────────────── 6 · La plataforma ──────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'El producto'
    Add-Titulo $s 'Seis módulos, una sola fuente de consulta'

    Add-Caja $s 64 150 702 30 -Relleno $c.linea2 -Borde $c.linea | Out-Null
    foreach ($px in @(80, 94, 108)) {
        $p = Add-Caja $s $px 161 8 8 -Relleno $c.tenue; $p.AutoShapeType = 9
    }
    Add-Txt $s 128 158 400 16 'cumplehn · portada pública' -Size 11 -Font $script:Mono -Color $c.suave | Out-Null
    Add-Img $s 64 180 702 440 (Join-Path $Img 'portada.jpg') | Out-Null

    $mods = @(
        @{ n = '01'; t = '**Perfiles** de candidaturas y partidos'; col = $c.turquesa },
        @{ n = '02'; t = '**Propuestas** con categoría, evidencia y estado de cumplimiento'; col = $c.turquesa },
        @{ n = '03'; t = '**Consulta ciudadana** con búsqueda y filtros'; col = $c.turquesa },
        @{ n = '04'; t = '**Tablero estadístico** de doce indicadores'; col = $c.turquesa },
        @{ n = '05'; t = '**Participación**: valoraciones, comentarios y encuestas'; col = $c.turquesa },
        @{ n = '06'; t = '**Asistente de IA** en lenguaje natural'; col = $c.escarlata }
    )
    $y = 150
    foreach ($m in $mods) {
        Add-Caja $s 792 $y 424 62 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        Add-Filo $s 792 ($y + 3) 56 $m.col | Out-Null
        Add-Txt $s 808 ($y + 11) 30 16 $m.n -Size 11 -Font $script:Mono -Color $c.tenue | Out-Null
        Add-Txt $s 838 ($y + 10) 366 44 $m.t -Size 13.5 -Color $c.tinta -Interlineado 1.3 | Out-Null
        $y += 70
    }
    Add-Pie $s 'Los seis módulos comprometidos en el FO-GR-013 aprobado están construidos y funcionando. Las cifras de la portada se leen en vivo de la base de datos.'

    # ───────────────────────────────────────────── 7 · Topología ───────────
    $s = New-Lamina $Pres 'noche'
    Add-Ceja $s 'Arquitectura · Topología' -Noche
    Add-Titulo $s 'Tres capas, y una sola habla con la base' -Noche

    $capas = @(
        @{ x = 74;  w = 228; r = 'CLIENTE';                  t = 'Navegador';        est = 'normal';
           l = @('HTML renderizado en el servidor', 'Bootstrap solo para la grilla', 'Sin dependencias externas'); m = 'jQuery · 387 líneas de JS' },
        @{ x = 372; w = 228; r = 'PRESENTACIÓN';             t = 'Frontend';         est = 'acento';
           l = @('ASP.NET Web Forms · C#', '29 páginas · 8 controles', 'IIS Express · puerto 5080'); m = 'IContenidoServicio' },
        @{ x = 670; w = 228; r = 'LÓGICA Y ACCESO A DATOS';  t = 'Web Service ASMX'; est = 'acento';
           l = @('51 métodos · SOAP y JSON', 'ADO.NET siempre con parámetros', 'IIS Express · puerto 44359'); m = 'WebServiceGlobal.asmx' },
        @{ x = 968; w = 248; r = 'PERSISTENCIA';             t = 'SQL Server Express'; est = 'ok';
           l = @('23 tablas · 7 vistas · 3 funciones', '44 procedimientos almacenados', '35 llaves foráneas'); m = 'BDCUMPLEHN' }
    )
    foreach ($k in $capas) {
        Add-Txt $s $k.x 176 $k.w 14 $k.r -Size 10.5 -Bold -Color $c.nCeja -Espaciado .8 | Out-Null
        switch ($k.est) {
            'acento' { $rel = $c.nAcento; $bor = $c.nAcentoB }
            'ok'     { $rel = $c.nOk;     $bor = $c.nOkB }
            default  { $rel = $c.nCaja;   $bor = $c.nBorde }
        }
        Add-Caja $s $k.x 196 $k.w 158 -Relleno $rel -Borde $bor -Redonda | Out-Null
        Add-Txt $s ($k.x + 20) 214 ($k.w - 36) 22 $k.t -Size 15 -Bold -Color $c.blanco | Out-Null
        $yy = 244
        foreach ($linea in $k.l) {
            Add-Txt $s ($k.x + 20) $yy ($k.w - 32) 18 $linea -Size 11.5 -Color $c.nBajada | Out-Null
            $yy += 20
        }
        Add-Txt $s ($k.x + 20) 322 ($k.w - 32) 18 $k.m -Size 11 -Font $script:Mono -Color $c.nMono | Out-Null
    }
    $flechas = @(
        @{ x1 = 302; x2 = 366; t = 'HTTPS' },
        @{ x1 = 600; x2 = 664; t = 'SOAP · HTTPS' },
        @{ x1 = 898; x2 = 962; t = 'ADO.NET' }
    )
    foreach ($f in $flechas) {
        Add-Linea $s $f.x1 275 $f.x2 275 -Color $c.nAcentoB -Flecha -Grosor 2 | Out-Null
        Add-Txt $s ($f.x1 - 12) 252 90 14 $f.t -Size 10.5 -Bold -Color $c.nCeja -Align 2 | Out-Null
    }

    Add-Linea $s 486 368 486 404 -Color $c.nAmbarB -Punteada | Out-Null
    Add-Linea $s 486 404 1086 404 -Color $c.nAmbarB -Punteada | Out-Null
    Add-Linea $s 1086 404 1086 368 -Color $c.nAmbarB -Punteada | Out-Null
    Add-Caja $s 468 414 636 74 -Relleno $c.nAmbar -Borde $c.nAmbarB -Redonda | Out-Null
    Add-Txt $s 490 430 600 20 'La cadena de conexión vive solo en el Web Service.' -Size 13.5 -Bold -Color $c.nAmbarT | Out-Null
    Add-Txt $s 490 452 600 34 'El frontend no la tiene, no referencia System.Data y no puede consultar la base ni por error.' -Size 11.5 -Color $c.nAmbarT -Interlineado 1.3 | Out-Null

    Add-Pie $s 'Dos soluciones de Visual Studio independientes más una capa de base de datos por scripts numerados, que se ejecutan en orden con `sqlcmd`.' -Noche

    # ─────────────────────────────────────── 8 · Modelo de datos ───────────
    $s = New-Lamina $Pres 'noche'
    Add-Ceja $s 'Arquitectura · Modelo de datos' -Noche
    Add-Titulo $s '23 tablas, y una relación que no lleva llave foránea' -Noche

    $grupos = @(
        @{ h = 'Catálogos · 6';     t = @('Roles','Cargos','Departamentos','Categorias','EstadosPropuesta','NivelesVerificacion') },
        @{ h = 'Entidades · 6';     t = @('Campanas','Partidos','Candidatos','Propuestas','Publicaciones','Usuarios') },
        @{ h = 'Participación · 6'; t = @('TiposObjeto','Valoraciones','Comentarios','Encuestas','EncuestaOpciones','EncuestaVotos') },
        @{ h = 'Gobierno · 5';      t = @('Auditoria','Modulos','ConsultasIA','DominiosCorreo','ConfirmacionesCorreo') }
    )
    $x = 64
    foreach ($g in $grupos) {
        Add-Txt $s $x 152 166 14 $g.h -Size 10.5 -Bold -Color $c.nCeja -Espaciado .7 -Caps | Out-Null
        $y = 174
        foreach ($t in $g.t) {
            $clave = ($t -eq 'TiposObjeto')
            $rel = if ($clave) { $c.nAmbar } else { $c.nCaja }
            $bor = if ($clave) { $c.ambarVivo } else { $c.nBorde }
            $col = if ($clave) { $c.nAmbarT } else { $c.nBajada }
            Add-Caja $s $x $y 166 27 -Relleno $rel -Borde $bor -Redonda | Out-Null
            Add-Txt $s ($x + 9) ($y + 7) 150 14 $t -Size 11.5 -Font $script:Mono -Color $col | Out-Null
            $y += 33
        }
        $x += 180.5
    }

    Add-Caja $s 858 152 298 54 -Relleno $c.nCaja -Borde $c.nBorde -Redonda | Out-Null
    Add-Txt $s 874 162 270 16 'Valoraciones · Comentarios · Auditoria' -Size 12.5 -Bold -Color $c.blanco | Out-Null
    Add-Txt $s 874 182 270 16 'apuntan a cualquier objeto de la plataforma' -Size 11 -Color $c.nBajada | Out-Null

    Add-Linea $s 1007 206 1007 240 -Color $c.nBorde -Flecha | Out-Null
    Add-Caja $s 874 244 266 34 -Relleno $c.nAmbar -Borde $c.nAmbarB -Redonda | Out-Null
    Add-Txt $s 874 253 266 16 '(codigoTipoObjeto, codigoObjeto)' -Size 11.5 -Font $script:Mono -Color $c.nAmbarT -Align 2 | Out-Null

    Add-Linea $s 1007 278 1007 306 -Color $c.nBorde -Flecha | Out-Null
    Add-Caja $s 916 308 182 38 -Relleno $c.nAcento -Borde $c.nAcentoB -Redonda | Out-Null
    Add-Txt $s 916 319 182 16 'TiposObjeto' -Size 12 -Font $script:Mono -Color $c.blanco -Align 2 | Out-Null

    Add-Linea $s 1007 346 1007 362 -Color $c.nAmbarB -Punteada | Out-Null
    Add-Linea $s 806 362 1208 362 -Color $c.nAmbarB -Punteada | Out-Null
    $destinos = @('Candidatos','Propuestas','Publicaciones','Partidos','Encuestas')
    $dx = 798
    foreach ($d in $destinos) {
        Add-Linea $s ($dx + 40) 362 ($dx + 40) 376 -Color $c.nAmbarB -Punteada | Out-Null
        Add-Caja $s $dx 378 80 26 -Relleno $c.nCaja -Borde $c.nBorde -Redonda | Out-Null
        Add-Txt $s ($dx + 3) 385 74 14 $d -Size 9 -Font $script:Mono -Color $c.nBajada -Align 2 | Out-Null
        $dx += 84
    }
    Add-Txt $s 798 418 418 56 'Sin llave foránea: el destino cambia según el tipo. La existencia la valida el Web Service, y la alternativa era repetir ocho tablas iguales.' `
        -Size 11.5 -Color $c.nBajada -Interlineado 1.35 -Align 2 | Out-Null

    Add-Pie $s 'Los contadores de «me gusta» y de comentarios **no se guardan**: se derivan con subconsultas, para que el número mostrado no pueda contradecir a las filas reales.' -Noche

    # ────────────────────────────────────────── 9 · Neutralidad ────────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Neutralidad'
    Add-Titulo $s 'La neutralidad no se declara: se construye'

    Add-Caja $s 64 140 569 176 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
    Add-Filo $s 64 143 170 $c.azul | Out-Null
    Add-Txt $s 82 154 533 24 'Verificación y cumplimiento son dos ejes distintos' -Size 17.5 -Bold -Font $script:Display -Color $c.tinta | Out-Null
    Add-Txt $s 82 182 533 38 'Se llaman parecido y no son lo mismo. Una propuesta puede estar **Verificada** y seguir **Declarada**, y eso no es una contradicción.' -Size 14 -Color $c.tinta2 -Interlineado 1.4 | Out-Null

    Add-Txt $s 82 228 96 14 'Verificación' -Size 10.5 -Bold -Color $c.suave -Caps | Out-Null
    $ejes1 = @(@{t='Declarado';on=$false}, @{t='En revisión';on=$false}, @{t='Verificado';on=$true})
    $ex = 186
    foreach ($e in $ejes1) {
        $rel = if ($e.on) { $c.verdeL } else { $c.linea2 }
        $col = if ($e.on) { $c.verde } else { $c.suave }
        $bor = if ($e.on) { $c.verde } else { $null }
        Add-Caja $s $ex 224 137 24 -Relleno $rel -Borde $bor -Redonda | Out-Null
        Add-Txt $s $ex 230 137 14 $e.t -Size 11.5 -Bold -Color $col -Align 2 | Out-Null
        $ex += 143
    }
    Add-Txt $s 82 262 96 14 'Cumplimiento' -Size 10.5 -Bold -Color $c.suave -Caps | Out-Null
    $ejes2 = @(@{t='Declarada';on=$true}, @{t='En proceso';on=$false}, @{t='Cumplida';on=$false}, @{t='Incumplida';on=$false})
    $ex = 186
    foreach ($e in $ejes2) {
        $rel = if ($e.on) { $c.ambarL } else { $c.linea2 }
        $col = if ($e.on) { $c.ambar } else { $c.suave }
        $bor = if ($e.on) { $c.ambarVivo } else { $null }
        Add-Caja $s $ex 258 100 24 -Relleno $rel -Borde $bor -Redonda | Out-Null
        Add-Txt $s $ex 264 100 14 $e.t -Size 11 -Bold -Color $col -Align 2 | Out-Null
        $ex += 107
    }

    Add-Tarjeta $s 647 140 569 176 'La candidatura no se califica a sí misma' `
        'El candidato solo declara su propuesta o la marca en proceso. Los estados de cumplimiento —esquema de PolitiFact, siete niveles con su ponderación— los asigna la plataforma con evidencia. Como el candidato es autor de su propio contenido, todo se muestra identificado como **declarado** hasta que exista fuente verificable.' `
        -Filo $c.escarlata

    Add-Tarjeta $s 64 330 569 158 'Nada se borra, y todo queda anotado' `
        'Retirar una publicación es baja lógica con motivo obligatorio, nunca `DELETE`. Cada acción de administración escribe en `Auditoria`, que conserva la fila aunque el objeto desaparezca. No hay pantalla ni método para editar esa bitácora, y no debe haberlos.' `
        -Filo $c.ambarVivo

    Add-Tarjeta $s 647 330 569 158 'Cada cifra declara de qué no es evidencia' `
        'La cuadrícula de departamentos dice que no es un mapa. La tarjeta de encuesta dice que describe a quienes respondieron y no a la población hondureña. El tablero omite la comparación contra el período anterior porque tres días de registro no la sostienen.' `
        -Filo $c.verde

    Add-Cita $s 64 506 1080 96 '«Esta sí está Verificada, lo que significa que hay una fuente registrada que respalda que la propuesta existe, aunque su estado de cumplimiento sigue siendo Declarada.»' `
        'Respuesta real del asistente de CumpleHN, sin que se le preguntara por la diferencia entre los dos ejes.' -Filo $c.turquesa -Size 17

    # ──────────────────────────────────── 10 · Control de acceso ───────────
    $s = New-Lamina $Pres 'noche'
    Add-Ceja $s 'Seguridad · Control de acceso' -Noche
    Add-Titulo $s 'Lo que el frontend sabe decide qué botones muestra, nunca qué se permite' -Noche -Size 38 -H 96

    Add-Txt $s 74 186 400 14 'Camino normal' -Size 10.5 -Bold -Color $c.nCeja -Espaciado .8 -Caps | Out-Null
    Add-Nodo $s 74 204 176 70 'Navegador' 'sesión en Session'
    Add-Linea $s 250 239 314 239 -Color $c.nBorde -Flecha -Grosor 1.6 | Out-Null
    Add-Nodo $s 318 204 238 70 'PaginaSegura.OnPreInit' 'antes de que exista un solo control' -Estilo 'acento' -Mono
    Add-Linea $s 556 239 620 239 -Color $c.nBorde -Flecha -Grosor 1.6 | Out-Null
    Add-Nodo $s 624 204 200 70 'Su propia área' 'Admin · Panel · portada' -Estilo 'ok'

    Add-Txt $s 858 186 358 14 'Tres roles' -Size 10.5 -Bold -Color $c.nCeja -Espaciado .8 -Caps | Out-Null
    Add-Caja $s 858 204 358 70 -Relleno $c.nCaja -Borde $c.nBorde -Redonda | Out-Null
    Add-Txt $s 878 220 320 16 'Administrador · Candidato · Ciudadano' -Size 12.5 -Bold -Color $c.blanco | Out-Null
    Add-Txt $s 878 242 320 20 'La correspondencia rol → área vive solo en un lugar.' -Size 11 -Color $c.nBajada | Out-Null

    Add-Txt $s 74 316 700 14 'Llamada directa al servicio, saltándose las páginas' -Size 10.5 -Bold -Color $c.nMalB -Espaciado .8 -Caps | Out-Null
    Add-Nodo $s 74 334 238 74 'Cliente SOAP propio' 'envía el código de usuario que quiera' -Estilo 'mal'
    Add-Linea $s 312 371 380 371 -Color $c.nMalB -Flecha -Grosor 1.6 | Out-Null

    Add-Caja $s 384 334 252 74 -Relleno $c.nAcento -Borde $c.nAcentoB -Redonda | Out-Null
    Add-Txt $s 402 348 220 14 'Barrera 1 · Web Service' -Size 10 -Bold -Color $c.nCeja -Caps | Out-Null
    Add-Txt $s 402 370 220 18 'EsAdministrador(conn, codigo)' -Size 12 -Font $script:Mono -Color $c.blanco | Out-Null
    Add-Linea $s 636 371 704 371 -Color $c.nMalB -Flecha -Grosor 1.6 | Out-Null

    Add-Caja $s 708 334 252 74 -Relleno $c.nAcento -Borde $c.nAcentoB -Redonda | Out-Null
    Add-Txt $s 726 348 220 14 'Barrera 2 · dentro de la base' -Size 10 -Bold -Color $c.nCeja -Caps | Out-Null
    Add-Txt $s 726 370 220 18 'fnEsAdministrador(@codigo)' -Size 12 -Font $script:Mono -Color $c.blanco | Out-Null
    Add-Linea $s 960 371 1016 371 -Color $c.nMalB -Flecha -Grosor 1.6 | Out-Null
    Add-Txt $s 1022 356 194 18 'Rechazado' -Size 13.5 -Bold -Color $c.nMalT | Out-Null
    Add-Txt $s 1022 376 194 18 'con motivo, en las dos capas' -Size 11 -Color $c.nMalT | Out-Null

    Add-Caja $s 64 432 1152 92 -Relleno $c.nOk -Borde $c.nOkB -Redonda | Out-Null
    Add-Txt $s 88 448 1104 20 'La protección va en la clase base, no repetida en cada página.' -Size 13.5 -Bold -Color $c.nOkT | Out-Null
    Add-Txt $s 88 472 1104 44 'Antes cada página del panel repetía el mismo bloque, así que una página nueva que olvidara copiarlo quedaba abierta — el fallo A01 de OWASP. Con la clase base, olvidarse significa no compilar. Y las plantillas maestras no protegen nada: se aplican cuando la página ya empezó su ciclo de vida.' `
        -Size 11.5 -Color $c.nOkT -Interlineado 1.35 | Out-Null

    Add-Pie $s 'Está verificado llamando al ASMX directamente con el código de un ciudadano y con el de un candidato: ninguno de los dos consigue verificar ni moderar.' -Noche

    # ─────────────────────────────────────────── 11 · Tablero ──────────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Módulo de analítica'
    Add-Titulo $s 'Doce indicadores en una sola llamada'

    Add-Caja $s 64 150 740 30 -Relleno $c.linea2 -Borde $c.linea | Out-Null
    foreach ($px in @(80, 94, 108)) { $p = Add-Caja $s $px 161 8 8 -Relleno $c.tenue; $p.AutoShapeType = 9 }
    Add-Txt $s 128 158 400 16 'cumplehn / Analitica' -Size 11 -Font $script:Mono -Color $c.suave | Out-Null
    $pic = Add-Img $s 64 180 740 740 (Join-Path $Img 'analitica.jpg')
    $pic.PictureFormat.CropBottom = (740 - 396) * 0.75

    $capasR = @(
        @{ q = 'base de datos';   v = 'calcula';    d = '5 vistas de detalle y 10 procedimientos' },
        @{ q = 'Web Service';     v = 'transporta'; d = 'sin una línea de SQL escrita a mano' },
        @{ q = 'Modelos/';        v = 'interpreta'; d = 'KPI, hallazgos y proporciones' },
        @{ q = 'Analitica.aspx';  v = 'dibuja';     d = 'anchos de barra, clases y SVG en el servidor' }
    )
    $y = 150
    foreach ($k in $capasR) {
        Add-Caja $s 828 $y 388 62 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        Add-Filo $s 828 ($y + 3) 56 $c.turquesa | Out-Null
        Add-Txt $s 846 ($y + 12) 152 16 $k.q -Size 12.5 -Font $script:Mono -Color $c.azul | Out-Null
        Add-Txt $s 846 ($y + 32) 152 20 $k.v -Size 17 -Bold -Font $script:Display -Color $c.tinta | Out-Null
        Add-Txt $s 1004 ($y + 14) 196 40 $k.d -Size 12.5 -Color $c.tinta2 -Interlineado 1.3 | Out-Null
        $y += 70
    }
    Add-Caja $s 828 434 388 118 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
    Add-Filo $s 828 437 112 $c.ambarVivo | Out-Null
    Add-Txt $s 846 450 354 90 'Un indicador nuevo se agrega **creando primero su procedimiento almacenado**, para que el cálculo quede documentado en la base y no escondido en el servicio.' `
        -Size 14 -Color $c.tinta2 -Interlineado 1.4 | Out-Null

    Add-Pie $s 'Todo el tablero viaja en una sola llamada. No es solo por costo: garantiza que los doce gráficos correspondan al mismo instante de los datos.'

    # ──────────────────────────────────────────── 12 · La brecha ───────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'El indicador central'
    Add-Titulo $s 'Lo que la ciudadanía prioriza contra lo que las candidaturas proponen' -Size 38 -H 96

    $ley = @(
        @{ x = 64;  col = $c.ambarVivo; t = 'Interés ciudadano — encuesta del proyecto, n = 150' },
        @{ x = 430; col = $c.turquesa;  t = 'Propuestas registradas en la plataforma' }
    )
    foreach ($l in $ley) {
        Add-Caja $s $l.x 184 11 11 -Relleno $l.col -Redonda | Out-Null
        Add-Txt $s ($l.x + 18) 181 340 16 $l.t -Size 13 -Color $c.tinta2 | Out-Null
    }
    Add-Txt $s 740 181 476 16 'Las dos medidas en porcentaje sobre su propio total.' -Size 13 -Color $c.suave | Out-Null

    $brecha = @(
        @{ e = 'Seguridad y orden público';   n = '1 propuesta';  i = 25.3; o = 9.1  },
        @{ e = 'Salud';                       n = '2 propuestas'; i = 23.3; o = 18.2 },
        @{ e = 'Educación';                   n = '1 propuesta';  i = 18.0; o = 9.1  },
        @{ e = 'Economía y empleo';           n = '2 propuestas'; i = 14.7; o = 18.2 },
        @{ e = 'Infraestructura';             n = '2 propuestas'; i = 14.0; o = 18.2 },
        @{ e = 'Transparencia y gobernanza';  n = '3 propuestas'; i = 4.7;  o = 27.3 }
    )
    $y = 212
    $pistaW = 858
    foreach ($b in $brecha) {
        Add-Txt $s 64 ($y + 2) 178 18 $b.e -Size 14.5 -Color $c.tinta | Out-Null
        Add-Txt $s 64 ($y + 21) 178 14 $b.n -Size 11.5 -Color $c.suave | Out-Null
        Add-Caja $s 258 ($y + 4) ($pistaW * $b.i / 27.3) 11 -Relleno $c.ambarVivo | Out-Null
        Add-Caja $s 258 ($y + 19) ($pistaW * $b.o / 27.3) 11 -Relleno $c.turquesa | Out-Null
        Add-Txt $s 1132 ($y + 2) 84 18 ("{0} %" -f $b.i) -Size 15 -Bold -Color $c.ambar -Align 3 | Out-Null
        Add-Txt $s 1132 ($y + 21) 84 14 ("{0} %" -f $b.o) -Size 12.5 -Color $c.turquesa -Align 3 | Out-Null
        $y += 46
    }

    Add-Cita $s 64 500 1080 86 '«La brecha más grande está en seguridad y orden público: concentra 25.3 % del interés ciudadano y solo 9.1 % de las propuestas registradas, 16.2 puntos porcentuales de diferencia.»' `
        'Hallazgo redactado por la propia plataforma. Se calcula sobre la selección activa y cambia con los filtros.' -Size 18

    Add-Pie $s 'La columna `Categorias.interesEncuesta` guarda el interés medido en la encuesta, de modo que el contraste sea data-driven y no un texto fijo. La oferta corresponde a los datos de demostración.'
}
