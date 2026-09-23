# ============================================================================
#  Arma CumpleHN-Defensa-editable.pptx con formas y texto reales.
#
#  A diferencia de CumpleHN-Defensa.pptx, que lleva cada lámina como imagen,
#  acá cada título, cada tarjeta y cada caja de diagrama es un objeto de
#  PowerPoint que se puede mover, reescribir y recolorear. Las únicas
#  imágenes son las capturas de la plataforma, que son fotos por naturaleza.
#
#  Uso:  powershell -NoProfile -ExecutionPolicy Bypass -File armar-pptx-editable.ps1
# ============================================================================

$ErrorActionPreference = 'Stop'
$Raiz = Split-Path -Parent $MyInvocation.MyCommand.Path

. (Join-Path $Raiz 'pptx-lib.ps1')
. (Join-Path $Raiz 'pptx-laminas-a.ps1')
. (Join-Path $Raiz 'pptx-laminas-b.ps1')

$Img   = Join-Path $Raiz 'imagenes'
$Notas = Join-Path $Raiz 'notas-presentador.txt'
$Dest  = Join-Path $Raiz 'CumpleHN-Defensa-editable.pptx'

if (-not (Test-Path $Img))   { throw "Falta la carpeta de imágenes: $Img" }
if (-not (Test-Path $Notas)) { throw "Falta el archivo de notas: $Notas" }

$notas = Get-Content $Notas -Encoding UTF8

$pp = New-Object -ComObject PowerPoint.Application
try {
    $pres = $pp.Presentations.Add($false)          # sin ventana visible
    $pres.PageSetup.SlideWidth  = 960              # 13.333 pulgadas
    $pres.PageSetup.SlideHeight = 540              # 7.5 pulgadas

    Build-LaminasA $pres $Img
    Build-LaminasB $pres $Img

    # PowerPoint deja una diapositiva vacía al crear la presentación.
    while ($pres.Slides.Count -gt 24) { $pres.Slides($pres.Slides.Count).Delete() }

    for ($k = 1; $k -le $pres.Slides.Count; $k++) {
        $txt = if ($k -le $notas.Count -and $notas[$k-1] -ne '-') { $notas[$k-1] } else { '' }
        if (-not $txt) { continue }
        foreach ($sh in $pres.Slides($k).NotesPage.Shapes) {
            if ($sh.Type -eq 14 -and $sh.PlaceholderFormat.Type -eq 2) {
                $sh.TextFrame.TextRange.Text = $txt
                break
            }
        }
    }

    if (Test-Path $Dest) { Remove-Item $Dest -Force }
    $pres.SaveAs($Dest, 24)                        # 24 = ppSaveAsOpenXMLPresentation
    Write-Output ("diapositivas: " + $pres.Slides.Count)
    $pres.Close()
} finally {
    $pp.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($pp) | Out-Null
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
}

$f = Get-Item $Dest
Write-Output ("archivo: " + $f.Name + "  " + [Math]::Round($f.Length/1MB,2) + " MB")
