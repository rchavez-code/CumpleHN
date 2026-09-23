# ============================================================================
#  Arma los decks HTML a partir de sus fuentes. Equivalente en PowerShell de
#  armar.sh y armar-corta.sh, para no depender de Git Bash.
#
#    .\armar-html.ps1              arma las dos versiones
#    .\armar-html.ps1 corta        solo la corta
#    .\armar-html.ps1 completa     solo la completa
#
#  Fuentes:
#    deck.src.html      cabecera, tokens y estilos base (común a las dos)
#    _p2/_p3/_p4.html   láminas de la completa (sus <style> también sirven a la corta)
#    _p5.html           HUD, paneles y motor (común)
#    _corta.html        láminas de la corta
#    imagenes/*.jpg     se embeben como data: URI donde haya {{IMG:nombre}}
# ============================================================================
param([ValidateSet('corta', 'completa', 'todas')][string]$Version = 'todas')

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $raiz

$utf8 = New-Object System.Text.UTF8Encoding($false)   # sin BOM, como el original

function Leer([string]$archivo) { [IO.File]::ReadAllText((Join-Path $raiz $archivo), $utf8) }

# Solo los bloques <style> de un archivo de láminas.
function Estilos([string]$archivo) {
    $t = Leer $archivo
    [regex]::Matches($t, '(?s)<style>.*?</style>') | ForEach-Object { $_.Value + "`n" }
}

# Sustituye {{IMG:nombre}} por el data: URI de imagenes/nombre.jpg.
function Embeber([string]$html) {
    [regex]::Replace($html, '\{\{IMG:([a-z0-9-]+)\}\}', {
        param($m)
        $ruta = Join-Path $raiz ("imagenes\" + $m.Groups[1].Value + ".jpg")
        if (-not (Test-Path $ruta)) { throw "Falta la imagen $ruta" }
        'data:image/jpeg;base64,' + [Convert]::ToBase64String([IO.File]::ReadAllBytes($ruta))
    })
}

function Escribir([string]$archivo, [string]$html) {
    [IO.File]::WriteAllText((Join-Path $raiz $archivo), $html, $utf8)
    $kb = [Math]::Round((Get-Item (Join-Path $raiz $archivo)).Length / 1KB)
    Write-Output ("{0} — {1} KB" -f $archivo, $kb)
}

if ($Version -in 'completa', 'todas') {
    $html = (Leer 'deck.src.html') + (Leer '_p2.html') + (Leer '_p3.html') + (Leer '_p4.html') + (Leer '_p5.html')
    Escribir 'cumplehn-defensa.html' (Embeber $html)
}

if ($Version -in 'corta', 'todas') {
    $cab = (Leer 'deck.src.html') -replace '<title>Defensa CumpleHN</title>', '<title>CumpleHN en cinco minutos</title>'
    $html = $cab + (Estilos '_p2.html') + (Estilos '_p3.html') + (Estilos '_p4.html') + (Leer '_corta.html') + (Leer '_p5.html')
    Escribir 'cumplehn-defensa-corta.html' (Embeber $html)
}
