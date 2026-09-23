/* ==========================================================================
   CumpleHN — Respuesta del asistente
   --------------------------------------------------------------------------
   El único lugar que convierte el HTML redactado por el modelo en nodos de
   la página. Lo usan el tablero (cumplehn-analitica.js), al recibir una
   respuesta, y el reporte de consultas (ReporteAsistente.aspx), al volver a
   dibujar las guardadas.

   Vive aparte y no copiado en los dos porque es la garantía de seguridad del
   módulo: una copia que se quede atrás no se nota, la página sigue
   funcionando igual y solo deja de estar protegida.

   Expone una sola función, CumpleHN.pintarRespuestaIA(contenedor, html).
   Sin dependencias.
   ========================================================================== */

(function () {
    'use strict';

    /* Etiquetas que el asistente puede usar. El prompt pide las mismas,
       pero la lista que manda es esta: un prompt es una instrucción y no
       una garantía. */
    var PERMITIDAS = {
        P: 1, BR: 1, STRONG: 1, EM: 1,
        UL: 1, OL: 1, LI: 1,
        TABLE: 1, THEAD: 1, TBODY: 1, TR: 1, TH: 1, TD: 1
    };

    /* La respuesta del modelo llega como HTML, y dentro puede venir
       reflejado texto que escribió una candidatura. Insertarla con
       innerHTML sería confiar en que el modelo nunca repita una etiqueta
       peligrosa, y esa no es una garantía que se pueda dar.

       Se analiza con DOMParser, que no ejecuta scripts ni descarga nada,
       y de ahí el árbol se reconstruye nodo por nodo: cada etiqueta
       permitida se vuelve a crear desde cero y **no se copia ni un solo
       atributo**. Ahí está la garantía, y no en la lista de etiquetas —
       sin atributos no hay onerror, ni href, ni src, así que ninguna
       etiqueta puede ejecutar nada aunque se permita.

       Una etiqueta que no está en la lista no se descarta con su
       contenido: se descarta ella y su texto sigue subiendo. Perder la
       mitad de una frase porque el modelo la envolvió en algo raro sería
       peor que perder el formato. */
    function limpiar(origen, destino) {
        for (var i = 0; i < origen.childNodes.length; i++) {
            var n = origen.childNodes[i];

            if (n.nodeType === 3) {
                destino.appendChild(document.createTextNode(n.nodeValue));
                continue;
            }

            if (n.nodeType !== 1) { continue; }

            if (!PERMITIDAS[n.tagName]) { limpiar(n, destino); continue; }

            var nuevo = document.createElement(n.tagName);

            // Una tabla ancha se desborda, así que va en su propia caja con
            // desplazamiento. La clase la pone el script, nunca el modelo.
            if (n.tagName === 'TABLE') {
                nuevo.className = 'gc-table';
                var caja = document.createElement('div');
                caja.className = 'gc-ia__tabla';
                caja.appendChild(nuevo);
                limpiar(n, nuevo);
                destino.appendChild(caja);
                continue;
            }

            limpiar(n, nuevo);
            destino.appendChild(nuevo);
        }
    }

    function pintarRespuestaIA(contenedor, html) {
        var doc = new DOMParser().parseFromString(html || '', 'text/html');
        var frag = document.createDocumentFragment();

        limpiar(doc.body, frag);

        // Sin ninguna etiqueta permitida queda texto suelto, que sin
        // envolver no hereda el espaciado de un párrafo.
        if (!frag.firstElementChild) {
            var p = document.createElement('p');
            p.textContent = doc.body.textContent.trim();
            contenedor.appendChild(p);
            return;
        }

        contenedor.appendChild(frag);
    }

    window.CumpleHN = window.CumpleHN || {};
    window.CumpleHN.pintarRespuestaIA = pintarRespuestaIA;
})();
