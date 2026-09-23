# ============================================================================
#  Versión corta: 7 láminas (portada, objetivo, tecnologías, logros x4).
#  Usa la misma librería que la versión completa.
# ============================================================================

function Build-LaminasCorta {
    param($Pres, [string]$Img)

    $c = $script:Col

    # ─────────────────────────────────────────────── 1 · Portada ───────────
    $s = New-Lamina $Pres 'claro'
    $aro = Add-Caja $s 980 96 236 236 -Borde $c.linea -GrosorBorde 1
    $aro.AutoShapeType = 9
    $arco = $s.Shapes.AddShape(9, 992 * 0.75, 108 * 0.75, 212 * 0.75, 212 * 0.75)
    $arco.Fill.Visible = 0; $arco.Line.ForeColor.RGB = $c.turquesa; $arco.Line.Weight = 2.5
    foreach ($b in @(@{y=170;w=31;col=$c.escarlata}, @{y=190;w=52;col=$c.ambarVivo}, @{y=210;w=73;col=$c.verde}, @{y=230;w=42;col=$c.turquesa})) {
        $r = Add-Caja $s 1058 $b.y $b.w 11 -Relleno $b.col -Redonda
        $r.Adjustments.Item(1) = 0.5
    }
    Add-Txt $s 64 180 900 18 'Centro Universitario Tecnológico · CEUTEC · Facultad de Ingeniería' -Size 12 -Bold -Color $c.turquesa -Espaciado 1.4 -Caps | Out-Null
    $marca = Add-Txt $s 64 202 700 92 'CumpleHN' -Size 74 -Bold -Font $script:Display -Color $c.tinta -Interlineado 1
    $marca.TextFrame.TextRange.Characters(7, 2).Font.Color.RGB = $c.escarlata
    Add-Txt $s 64 300 560 108 'Plataforma web para el seguimiento ciudadano de promesas políticas en Honduras' -Size 27 -Bold -Font $script:Display -Color $c.tinta2 -Interlineado 1.22 | Out-Null
    Add-Linea $s 64 424 1152 424 -Color $c.linea | Out-Null
    foreach ($m in @(
        @{ x = 64;  t = 'Roy Guillermo Chávez Espinal'; d = "Cuenta 31521560`rIngeniería en Informática" },
        @{ x = 300; t = 'Proyecto de graduación';       d = 'Tegucigalpa, M.D.C. · 2026' })) {
        Add-Txt $s $m.x 442 300 20 $m.t -Size 14.5 -Bold -Color $c.tinta | Out-Null
        Add-Txt $s $m.x 466 300 50 $m.d -Size 14 -Color $c.suave -Interlineado 1.4 | Out-Null
    }

    # ───────────────────────────────────────────── 2 · Objetivo ────────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Objetivo general'
    Add-Titulo $s 'Decidir con fundamentos, no con percepciones.'
    Add-Cita $s 64 150 660 244 'Facilitar el seguimiento ciudadano del cumplimiento de promesas y de la trayectoria de gestión de los candidatos y funcionarios políticos en Honduras, mediante el desarrollo de una plataforma web que centralice y visualice estadísticamente esta información, con el fin de apoyar a la ciudadanía en la toma de decisiones fundamentadas en evidencia cuantitativa al momento de votar.' `
        '' -Filo $c.azul -Size 19
    Add-Etiqueta $s 768 150 448 'Lo que CumpleHN no hace' -Color $c.escarlata
    $y = 176
    foreach ($n in @('No rankea candidaturas por calidad.', 'No recomienda por quién votar.', 'No emite un veredicto sobre quién cumple mejor.', 'No presenta como hecho comprobado lo que solo está declarado.')) {
        Add-Caja $s 768 ($y + 8) 15 2 -Relleno $c.escarlata | Out-Null
        Add-Txt $s 798 $y 418 44 $n -Size 16.5 -Color $c.tinta2 -Interlineado 1.3 | Out-Null
        $y += if ($n.Length -gt 48) { 52 } else { 32 }
    }
    Add-Cita $s 64 430 1080 76 'Entrega la evidencia organizada para que cada persona la pondere según el criterio que considere de mayor peso. Seguridad, salud, educación, economía, infraestructura o transparencia y decida por su cuenta.' -Filo $c.verde -Size 21
    Add-Pie $s 'El problema que responde está medido: en la encuesta del proyecto (n = 150), 84.7 % percibe la información dispersa y 84.7 % usaría una plataforma como esta antes de votar.'

    # ───────────────────────────────────────────── 3 · Tecnologías ─────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Tecnologías utilizadas'
    Add-Titulo $s 'Tres capas separadas, y una sola habla con la base'
    $cols = @(
        @{ h = 'Presentación'; col = $c.turquesa; filo = $null; i = @(
            @('ASP.NET Web Forms', '.NET Framework 4.7.2 · C#'),
            @('Bootstrap 5 + jQuery', 'Solo la grilla. Sin dependencias externas en tiempo de ejecución.'),
            @('CSS', 'Sistema de diseño con tokens, SVG generado en el servidor.'),
            @('IIS Express', 'Puerto 5080')) },
        @{ h = 'Servicio'; col = $c.turquesa; filo = $null; i = @(
            @('Web Service ASMX', 'SOAP y JSON desde el mismo servicio. 51 métodos.'),
            @('ADO.NET', 'Siempre con parámetros y procedimientos almacenados.'),
            @('SHA-256 criptográfico', 'Contraseñas y tokens de confirmación nunca en claro.'),
            @('SMTP', 'Confirmación de correo con contraseña de aplicación.')) },
        @{ h = 'Datos'; col = $c.turquesa; filo = $null; i = @(
            @('SQL Server Express', 'Base `BDCUMPLEHN`, 23 tablas y 35 llaves foráneas.'),
            @('Índices únicos y bajas lógicas', 'Un voto por persona, nada se borra, se usa bitácora'),
            @('Login restringido', '`cumplehn_ia`: solo EXECUTE, cero permisos de tabla.')) },
        @{ h = 'Inteligencia artificial'; col = $c.escarlata; filo = $c.escarlata; i = @(
            @('Claude Sonnet 5', 'SDK oficial de Anthropic para .NET.'),
            @('Uso de herramientas', '4 herramientas, cada una un procedimiento almacenado tipado.')) }
    )
    $x = 64
    foreach ($k in $cols) {
        Add-Caja $s $x 140 277 400 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
        if ($null -ne $k.filo) { Add-Filo $s $x 143 394 $k.filo | Out-Null }
        Add-Txt $s ($x + 18) 156 241 14 $k.h -Size 11 -Bold -Color $k.col -Espaciado 1 -Caps | Out-Null
        Add-Linea $s ($x + 18) 178 ($x + 259) 178 -Color $c.linea2 -Grosor 0.75 | Out-Null
        $y = 190
        foreach ($it in $k.i) {
            Add-Txt $s ($x + 18) $y 241 18 $it[0] -Size 14.5 -Bold -Color $c.tinta | Out-Null
            Add-Txt $s ($x + 18) ($y + 20) 241 50 $it[1] -Size 12.5 -Color $c.suave -Interlineado 1.35 | Out-Null
            $y += if ($it[1].Length -gt 44) { 88 } else { 66 }
        }
        $x += 291.5
    }
    Add-Pie $s 'La cadena de conexión vive solo en el Web Service. El frontend no la tiene y no puede consultar la base. El modelo tampoco: recibe la fila que un procedimiento le devuelve.'

    # ─────────────────────────────────────── 4 · Logros · módulos ──────────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Logros · Lo que se construyó'
    Add-Titulo $s 'Siete módulos en la plataforma'
    $mods = @(
        @{ t = 'Perfiles y catálogos';         filo = $c.turquesa;  d = 'Candidaturas, partidos y campañas con ficha pública y nivel de verificación. La administración da de alta, verifica y modera con motivo obligatorio y bitácora.' },
        @{ t = 'Propuestas con seguimiento';   filo = $c.turquesa;  d = 'Cada promesa lleva categoría, evidencia y estado de cumplimiento. La candidatura declara, la plataforma califica.' },
        @{ t = 'Consulta ciudadana';           filo = $c.turquesa;  d = 'Búsqueda y filtros por campaña, categoría, partido y departamento en todo el sitio público. Consultar es libre, participar requiere cuenta confirmada.' },
        @{ t = 'Tablero de doce indicadores';  filo = $c.azul;      d = 'La base calcula, el servicio transporta, la página dibuja. Una sola llamada para que los doce gráficos correspondan al mismo instante.' },
        @{ t = 'Participación ciudadana';      filo = $c.verde;     d = 'Valoraciones, comentarios, encuestas de percepción e iniciativas propuestas por la ciudadanía. Registro público con confirmación de correo.' },
        @{ t = 'Asistente de IA';              filo = $c.escarlata; d = 'Preguntas en español, respuestas con fuentes reales. Conectado a un modelo de lenguaje con cuota diaria y bitácora de cada consulta.' }
    )
    $x = 64; $y = 140; $i = 0
    foreach ($m in $mods) {
        Add-Tarjeta $s $x $y 374 168 $m.t $m.d -Filo $m.filo -SizeT 16 -SizeC 13
        $i++
        if ($i % 3 -eq 0) { $x = 64; $y += 180 } else { $x += 388.7 }
    }
    # Séptima tarjeta a lo ancho: la extensión, marcada como tal.
    Add-Caja $s 64 500 1152 96 -Relleno $c.blanco -Borde $c.linea -Redonda | Out-Null
    Add-Filo $s 64 503 90 $c.ambarVivo | Out-Null
    Add-Etiqueta $s 82 512 1100 'Modelo de sostenibilidad' -Color $c.ambar
    Add-Txt $s 82 532 300 22 'Espacios para organizaciones' -Size 16 -Bold -Font $script:Display -Color $c.tinta | Out-Null
    Add-Txt $s 392 530 806 60 'El mismo motor al servicio de un colegio profesional, una cooperativa o un sindicato para su propia elección: sitio propio, planillas, padrón de miembros y tablero. Aislamiento por espacio en la base, planes con vigencia y solicitud desde una página pública. CumpleHN sigue gratuito y neutral, y es un espacio más.' -Size 12.5 -Color $c.tinta2 -Interlineado 1.35 | Out-Null

    # ─────────────────────────────────────────── 5 · Logros · IA ───────────
    $s = New-Lamina $Pres 'noche'
    Add-Ceja $s 'Logros · El módulo de inteligencia artificial' -Noche
    Add-Titulo $s 'El modelo nunca ve la base. Ve la fila que un procedimiento le devuelve.' -Noche -Size 38 -H 96
    $ia = @(
        @{ n = '1'; t = 'Responde con sus fuentes, y las fuentes son reales'; d = 'Se arman con las consultas que de verdad se ejecutaron, no con lo que el modelo dice haber usado.' },
        @{ n = '2'; t = 'Nunca escribe SQL'; d = 'Recibe cuatro herramientas y cada una es un procedimiento almacenado con parámetros tipados. Lo único que conoce de la plataforma es lo que ese procedimiento le devuelve.' },
        @{ n = '3'; t = 'Un login que no puede leer tablas'; d = '`cumplehn_ia` solo tiene `EXECUTE` sobre 13 procedimientos y `DENY` sobre todo lo que vincula a una persona con su preferencia política. Verificado con `EXECUTE AS`.' },
        @{ n = '4'; t = 'Neutral por regla, no por suerte'; d = 'No rankea candidaturas, dice siempre el nivel de verificación y responde que no sabe en vez de completar. El texto que devuelven las herramientas es dato, nunca instrucción.' },
        @{ n = '5'; t = 'La respuesta no puede ejecutar nada'; d = 'Se reconstruye en el navegador con `DOMParser` y lista blanca, sin copiar un solo atributo. Sin `onerror`, `href` ni `src`, ninguna etiqueta puede hacer daño.' },
        @{ n = '6'; t = 'Límite de uso diario'; d = 'Cuota de 20 consultas por persona y día, y bitácora de cada pregunta, responda o falle.' }
    )
    $ambarClaro = C 'F0B552'
    $x = 64; $y = 186; $i = 0
    foreach ($g in $ia) {
        Add-Linea $s $x $y ($x + 34) $y -Color $ambarClaro -Grosor 2 | Out-Null
        Add-Txt $s $x ($y + 7) 34 28 $g.n -Size 24 -Bold -Font $script:Display -Color $ambarClaro | Out-Null
        Add-Txt $s ($x + 47) ($y + 4) 514 22 $g.t -Size 16.5 -Bold -Font $script:Display -Color $c.blanco | Out-Null
        Add-Txt $s ($x + 47) ($y + 30) 514 90 $g.d -Size 13.5 -Color $c.nBajada -Interlineado 1.4 | Out-Null
        $i++
        if ($i % 2 -eq 0) { $x = 64; $y += 134 } else { $x = 655 }
    }

    # ────────────────────────── 6 · Logros · neutralidad y seguridad ───────
    $s = New-Lamina $Pres 'claro'
    Add-Ceja $s 'Logros · Neutralidad y seguridad'
    Add-Titulo $s 'La neutralidad no se declara: se construye'
    $neu = @(
        @{ t = 'Verificación y cumplimiento son dos ejes distintos'; filo = $c.azul;      d = 'Una propuesta puede estar **Verificada** y seguir **Declarada**. La candidatura no se califica a sí misma: los estados de cumplimiento los asigna la plataforma con evidencia, y todo se muestra como declarado hasta que exista fuente verificable.' },
        @{ t = 'Nada se borra, y todo queda anotado';                 filo = $c.ambarVivo; d = 'Retirar es baja lógica con motivo obligatorio, nunca `DELETE`. Cada acción de administración escribe en `Auditoria`, que no tiene pantalla ni método para editarse.' }
    )
    $x = 64
    foreach ($n in $neu) {
        Add-Tarjeta $s $x 200 569 230 $n.t $n.d -Filo $n.filo -SizeT 19 -SizeC 16
        $x += 583
    }

    # ──────────────────────────────────────────── 7 · Cifras y cierre ──────
    $s = New-Lamina $Pres 'noche'
    $ambarClaro = C 'F0B552'
    $frase = Add-Txt $s 64 290 760 140 "CumpleHN no dice quién cumple mejor.`rOrdena la evidencia para que cada quien lo juzgue." -Size 44 -Bold -Font $script:Display -Color $c.blanco -Interlineado 1.25
    $ini = $frase.TextFrame.TextRange.Text.IndexOf('cada quien lo juzgue')
    if ($ini -ge 0) { $frase.TextFrame.TextRange.Characters($ini + 1, 20).Font.Color.RGB = $ambarClaro }
}
