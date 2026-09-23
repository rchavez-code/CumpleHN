# ============================================================================
#  Librería de dibujo para armar CumpleHN-Defensa-editable.pptx con formas y
#  texto reales de PowerPoint, no capturas.
#
#  Las coordenadas se escriben en el espacio de 1280x720 del deck HTML y se
#  convierten aquí a los 960x540 puntos de la diapositiva. Así el port de cada
#  lámina es una transcripción y no un rediseño.
# ============================================================================

$script:K = 0.75          # 1280 px -> 960 pt

# Tipografías: Georgia y Calibri vienen con Windows y con Office, así que el
# archivo se abre igual en cualquier máquina. Georgia es además la serif que
# usa la propia plataforma.
$script:Display = 'Georgia'
$script:Sans    = 'Calibri'
$script:Mono    = 'Consolas'

# --------------------------------------------------------------- Colores ---
# PowerPoint quiere el entero en orden R + G*256 + B*65536.
function C {
    param([string]$Hex)
    $h = $Hex.TrimStart('#')
    [int]("0x" + $h.Substring(0,2)) + ([int]("0x" + $h.Substring(2,2)) * 256) + ([int]("0x" + $h.Substring(4,2)) * 65536)
}

$script:Col = @{
    tinta     = C 'FFFFFF'   # se reasigna abajo; placeholder para el orden
}
$script:Col = @{
    tinta     = C '14171C'
    tinta2    = C '39404A'
    suave     = C '6A7078'
    tenue     = C '9AA0A8'
    linea     = C 'E3E1DD'
    linea2    = C 'EEECE8'
    papel     = C 'F7F7F5'
    blanco    = C 'FFFFFF'
    escarlata = C 'C3372C'
    escarlataL= C 'FDF2F1'
    ambar     = C 'B8791A'
    ambarVivo = C 'D3901F'
    ambarL    = C 'FDF6E9'
    verde     = C '2C7A58'
    verdeL    = C 'EEF7F2'
    turquesa  = C '17708A'
    turquesaL = C 'EEF6F8'
    azul      = C '1B4F9C'
    azulL     = C 'EFF3FA'
    noche     = C '123054'
    noche2    = C '0A1728'
    # Sobre fondo noche
    nTexto    = C 'E8EEF6'
    nBajada   = C 'BACADD'
    nPie      = C '8BA3BF'
    nCeja     = C '7FC4D8'
    nMono     = C '9ED4E6'
    nCaja     = C '1B3A5E'
    nBorde    = C '3E5F86'
    nAcento   = C '17475F'
    nAcentoB  = C '4FA8C4'
    nOk       = C '1B4838'
    nOkB      = C '5AAB84'
    nMal      = C '5A2420'
    nMalB     = C 'E0685C'
    nAmbar    = C '4A3A18'
    nAmbarB   = C 'A07A3C'
    nAmbarT   = C 'F0CF90'
    nMalT     = C 'F2B3AC'
    nOkT      = C '9ED9BD'
}

# ------------------------------------------------------------- Diapositiva --

function New-Lamina {
    param($Pres, [string]$Tipo = 'claro')

    $s = $Pres.Slides.Add($Pres.Slides.Count + 1, 12)   # 12 = ppLayoutBlank
    $s.FollowMasterBackground = 0
    $s.Background.Fill.Visible = -1

    if ($Tipo -eq 'noche') {
        $s.Background.Fill.TwoColorGradient(1, 1)        # lineal, variante 1
        $s.Background.Fill.ForeColor.RGB = $script:Col.noche
        $s.Background.Fill.BackColor.RGB = $script:Col.noche2
        $s.Background.Fill.GradientAngle = 315
    } else {
        $s.Background.Fill.Solid()
        $s.Background.Fill.ForeColor.RGB = $script:Col.papel
    }

    # Filo guacamaya: el mismo remate que llevan las tarjetas de la plataforma.
    $anchos = @($script:Col.escarlata, $script:Col.ambarVivo, $script:Col.verde, $script:Col.turquesa)
    for ($i = 0; $i -lt 4; $i++) {
        $r = $s.Shapes.AddShape(1, $i * 240, 0, 240, 3.75)
        $r.Fill.Solid(); $r.Fill.ForeColor.RGB = $anchos[$i]
        $r.Line.Visible = 0
        $r.Name = "filo$i"
    }
    return $s
}

# ------------------------------------------------------------------ Texto --
#
#  Admite un marcado mínimo dentro del texto:
#     **negrita**      y      `monoespaciado`
#  Se resuelve aplicando formato por rango de caracteres, que es lo que
#  mantiene el texto editable en vez de partirlo en cajas separadas.

function Add-Txt {
    param(
        $S, [double]$X, [double]$Y, [double]$W, [double]$H, [string]$T,
        [double]$Size = 14, [string]$Font = $null, $Color = $null,
        [switch]$Bold, [int]$Align = 1, [double]$Interlineado = 1.25,
        [double]$Espaciado = 0, [switch]$Caps, [int]$Anclaje = 1
    )
    if (-not $Font)  { $Font  = $script:Sans }
    if ($null -eq $Color) { $Color = $script:Col.tinta }

    # Se recogen los rangos marcados antes de limpiar los marcadores.
    $negritas = New-Object System.Collections.ArrayList
    $monos    = New-Object System.Collections.ArrayList
    $limpio = ''
    $i = 0
    while ($i -lt $T.Length) {
        if ($i + 1 -lt $T.Length -and $T.Substring($i,2) -eq '**') {
            $fin = $T.IndexOf('**', $i + 2)
            if ($fin -lt 0) { $limpio += $T.Substring($i,2); $i += 2; continue }
            $trozo = $T.Substring($i + 2, $fin - $i - 2)
            [void]$negritas.Add(@(($limpio.Length + 1), $trozo.Length))
            $limpio += $trozo
            $i = $fin + 2
        } elseif ($T[$i] -eq '`') {
            $fin = $T.IndexOf('`', $i + 1)
            if ($fin -lt 0) { $limpio += $T[$i]; $i++; continue }
            $trozo = $T.Substring($i + 1, $fin - $i - 1)
            [void]$monos.Add(@(($limpio.Length + 1), $trozo.Length))
            $limpio += $trozo
            $i = $fin + 1
        } else {
            $limpio += $T[$i]; $i++
        }
    }
    if ($Caps) { $limpio = $limpio.ToUpper() }

    $sh = $S.Shapes.AddTextbox(1, $X * $script:K, $Y * $script:K, $W * $script:K, $H * $script:K)
    $tf = $sh.TextFrame
    $tf.MarginLeft = 0; $tf.MarginRight = 0; $tf.MarginTop = 0; $tf.MarginBottom = 0
    $tf.WordWrap = -1
    $tf.AutoSize = 0
    $tf.VerticalAnchor = $Anclaje
    $tr = $tf.TextRange
    $tr.Text = $limpio
    $tr.Font.Name = $Font
    $tr.Font.Size = $Size * $script:K
    $tr.Font.Color.RGB = $Color
    $tr.Font.Bold = if ($Bold) { -1 } else { 0 }
    if ($Espaciado -ne 0) { $sh.TextFrame2.TextRange.Font.Spacing = $Espaciado }
    $tr.ParagraphFormat.Alignment = $Align
    $pf = $sh.TextFrame2.TextRange.ParagraphFormat
    $pf.LineRuleWithin = -1
    $pf.SpaceWithin = $Interlineado

    foreach ($n in $negritas) { $tr.Characters($n[0], $n[1]).Font.Bold = -1 }
    foreach ($m in $monos) {
        $c = $tr.Characters($m[0], $m[1])
        $c.Font.Name = $script:Mono
        $c.Font.Size = $Size * $script:K * 0.9
    }
    return $sh
}

# ------------------------------------------------------------------ Cajas --

function Add-Caja {
    param(
        $S, [double]$X, [double]$Y, [double]$W, [double]$H,
        $Relleno = $null, $Borde = $null, [switch]$Redonda,
        [double]$GrosorBorde = 1, [double]$Transparencia = 0
    )
    $tipo = if ($Redonda) { 5 } else { 1 }
    $sh = $S.Shapes.AddShape($tipo, $X * $script:K, $Y * $script:K, $W * $script:K, $H * $script:K)
    if ($Redonda) { $sh.Adjustments.Item(1) = 0.06 }
    if ($null -eq $Relleno) {
        $sh.Fill.Visible = 0
    } else {
        $sh.Fill.Solid(); $sh.Fill.ForeColor.RGB = $Relleno
        if ($Transparencia -gt 0) { $sh.Fill.Transparency = $Transparencia }
    }
    if ($null -eq $Borde) {
        $sh.Line.Visible = 0
    } else {
        $sh.Line.Visible = -1; $sh.Line.ForeColor.RGB = $Borde; $sh.Line.Weight = $GrosorBorde
    }
    $sh.Shadow.Visible = 0
    return $sh
}

# Filo de color en el canto izquierdo de una tarjeta. En este deck el filo
# codifica el tipo de tarjeta, así que se usa por rol y no en todas.
function Add-Filo {
    param($S, [double]$X, [double]$Y, [double]$H, [int]$Color, [double]$Ancho = 3)
    $sh = $S.Shapes.AddShape(1, $X * $script:K, $Y * $script:K, $Ancho * $script:K, $H * $script:K)
    $sh.Fill.Solid(); $sh.Fill.ForeColor.RGB = $Color
    $sh.Line.Visible = 0
    return $sh
}

function Add-Linea {
    param(
        $S, [double]$X1, [double]$Y1, [double]$X2, [double]$Y2,
        $Color = $null, [switch]$Flecha, [switch]$Punteada, [double]$Grosor = 1.2
    )
    if ($null -eq $Color) { $Color = $script:Col.linea }
    $sh = $S.Shapes.AddLine($X1 * $script:K, $Y1 * $script:K, $X2 * $script:K, $Y2 * $script:K)
    $sh.Line.ForeColor.RGB = $Color
    $sh.Line.Weight = $Grosor
    if ($Punteada) { $sh.Line.DashStyle = 4 }
    if ($Flecha) { $sh.Line.EndArrowheadStyle = 3; $sh.Line.EndArrowheadWidth = 1; $sh.Line.EndArrowheadLength = 1 }
    return $sh
}

function Add-Img {
    param($S, [double]$X, [double]$Y, [double]$W, [double]$H, [string]$Ruta)
    if (-not (Test-Path $Ruta)) { throw "No existe la imagen $Ruta" }
    $sh = $S.Shapes.AddPicture($Ruta, $false, $true, $X * $script:K, $Y * $script:K, $W * $script:K, $H * $script:K)
    $sh.Line.Visible = -1
    $sh.Line.ForeColor.RGB = $script:Col.linea
    $sh.Line.Weight = 0.75
    return $sh
}

# --------------------------------------------------- Piezas repetidas ------

function Add-Ceja {
    param($S, [string]$T, [switch]$Noche, $Color = $null)
    if ($null -eq $Color) { $Color = if ($Noche) { $script:Col.nCeja } else { $script:Col.turquesa } }
    Add-Txt $S 64 52 1000 18 $T -Size 12 -Bold -Color $Color -Espaciado 1.4 -Caps | Out-Null
}

function Add-Titulo {
    param($S, [string]$T, [switch]$Noche, [double]$Size = 38, [double]$Y = 74, [double]$H = 52)
    $c = if ($Noche) { $script:Col.blanco } else { $script:Col.tinta }
    Add-Txt $S 64 $Y 1080 $H $T -Size $Size -Bold -Font $script:Display -Color $c -Interlineado 1.1 | Out-Null
}

function Add-Bajada {
    param($S, [string]$T, [double]$Y, [double]$W = 900, [double]$H = 50, [switch]$Noche, [double]$Size = 18.5)
    $c = if ($Noche) { $script:Col.nBajada } else { $script:Col.tinta2 }
    Add-Txt $S 64 $Y $W $H $T -Size $Size -Color $c -Interlineado 1.45 | Out-Null
}

function Add-Pie {
    param($S, [string]$T, [switch]$Noche, [double]$Y = 636)
    $c = if ($Noche) { $script:Col.nPie } else { $script:Col.suave }
    Add-Txt $S 64 $Y 1152 34 $T -Size 13 -Color $c -Interlineado 1.35 | Out-Null
}

# Tarjeta con filo de color, título y cuerpo. Devuelve la caja por si hay que
# apilarle algo encima.
function Add-Tarjeta {
    param(
        $S, [double]$X, [double]$Y, [double]$W, [double]$H,
        [string]$Titulo = '', [string]$Cuerpo = '', $Filo = $null,
        [switch]$Noche, [double]$SizeT = 17.5, [double]$SizeC = 14
    )
    $relleno = if ($Noche) { $script:Col.nCaja } else { $script:Col.blanco }
    $borde   = if ($Noche) { $script:Col.nBorde } else { $script:Col.linea }
    Add-Caja $S $X $Y $W $H -Relleno $relleno -Borde $borde -Redonda | Out-Null
    if ($null -ne $Filo) { Add-Filo $S $X ($Y + 3) ($H - 6) $Filo | Out-Null }

    $ct = if ($Noche) { $script:Col.blanco } else { $script:Col.tinta }
    $cc = if ($Noche) { $script:Col.nBajada } else { $script:Col.tinta2 }
    $yy = $Y + 14
    if ($Titulo) {
        $altoT = if ($Titulo.Length -gt 34) { 44 } else { 24 }
        Add-Txt $S ($X + 18) $yy ($W - 34) $altoT $Titulo -Size $SizeT -Bold -Font $script:Display -Color $ct -Interlineado 1.12 | Out-Null
        $yy += $altoT + 6
    }
    if ($Cuerpo) {
        Add-Txt $S ($X + 18) $yy ($W - 34) ($H - ($yy - $Y) - 12) $Cuerpo -Size $SizeC -Color $cc -Interlineado 1.4 | Out-Null
    }
}

# Etiqueta pequeña en versalitas, la que rotula bloques dentro de una lámina.
function Add-Etiqueta {
    param($S, [double]$X, [double]$Y, [double]$W, [string]$T, $Color = $null, [switch]$Noche)
    if ($null -eq $Color) { $Color = if ($Noche) { $script:Col.nPie } else { $script:Col.suave } }
    Add-Txt $S $X $Y $W 16 $T -Size 10.5 -Bold -Color $Color -Espaciado 1 -Caps | Out-Null
}

# Cita con filo, la pieza que remata varias láminas.
function Add-Cita {
    param(
        $S, [double]$X, [double]$Y, [double]$W, [double]$H,
        [string]$T, [string]$Fuente = '', $Filo = $null, [switch]$Noche, [double]$Size = 21
    )
    if ($null -eq $Filo) { $Filo = $script:Col.ambarVivo }
    Add-Filo $S $X $Y $H $Filo | Out-Null
    $c  = if ($Noche) { $script:Col.nTexto } else { $script:Col.tinta }
    $cf = if ($Noche) { $script:Col.nPie }   else { $script:Col.suave }
    $altoT = if ($Fuente) { $H - 26 } else { $H }
    Add-Txt $S ($X + 20) $Y ($W - 20) $altoT $T -Size $Size -Font $script:Display -Color $c -Interlineado 1.3 | Out-Null
    if ($Fuente) {
        Add-Txt $S ($X + 20) ($Y + $altoT + 2) ($W - 20) 24 $Fuente -Size 13 -Color $cf -Interlineado 1.3 | Out-Null
    }
}

# Caja del diagrama: rectángulo redondeado con título y subtítulo dentro.
function Add-Nodo {
    param(
        $S, [double]$X, [double]$Y, [double]$W, [double]$H,
        [string]$T, [string]$Sub = '', [string]$Estilo = 'normal', [switch]$Mono
    )
    switch ($Estilo) {
        'acento'  { $r = $script:Col.nAcento; $b = $script:Col.nAcentoB }
        'ok'      { $r = $script:Col.nOk;     $b = $script:Col.nOkB }
        'mal'     { $r = $script:Col.nMal;    $b = $script:Col.nMalB }
        'ambar'   { $r = $script:Col.nAmbar;  $b = $script:Col.nAmbarB }
        default   { $r = $script:Col.nCaja;   $b = $script:Col.nBorde }
    }
    Add-Caja $S $X $Y $W $H -Relleno $r -Borde $b -Redonda | Out-Null
    $ct = switch ($Estilo) {
        'mal'   { $script:Col.nMalT }
        'ambar' { $script:Col.nAmbarT }
        default { $script:Col.blanco }
    }
    $cs = switch ($Estilo) {
        'mal'   { $script:Col.nMalT }
        'ambar' { $script:Col.nAmbarT }
        default { $script:Col.nBajada }
    }
    $f = if ($Mono) { $script:Mono } else { $script:Sans }
    $sz = if ($Mono) { 12.5 } else { 13.5 }
    Add-Txt $S ($X + 16) ($Y + 14) ($W - 30) 20 $T -Size $sz -Bold -Font $f -Color $ct | Out-Null
    if ($Sub) {
        Add-Txt $S ($X + 16) ($Y + 38) ($W - 30) 34 $Sub -Size 11.5 -Color $cs -Interlineado 1.25 | Out-Null
    }
}

# Barra de dato con su pista, para las láminas de cifras.
function Add-Barra {
    param($S, [double]$X, [double]$Y, [double]$W, [double]$Pct, [int]$Color, [double]$Alto = 6)
    Add-Caja $S $X $Y $W $Alto -Relleno $script:Col.linea -Redonda | Out-Null
    if ($Pct -gt 0) {
        Add-Caja $S $X $Y ($W * $Pct / 100) $Alto -Relleno $Color -Redonda | Out-Null
    }
}
