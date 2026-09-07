/* ==========================================================================
   CumpleHN — Tablero analítico
   --------------------------------------------------------------------------
   Tres comportamientos, nada más. Todo lo demás —cifras, barras, SVG,
   hallazgos— lo genera el servidor y llega ya dibujado en el HTML.

   1. Pestañas. Los cuatro paneles vienen renderizados en la misma respuesta,
      así que cambiar de pestaña no necesita ir al servidor. El estado se
      guarda en un campo oculto para que un postback de filtros devuelva a la
      persona a la pestaña donde estaba.

   2. Velo de carga. Filtrar es un postback, y un postback sin respuesta
      visible parece que no hizo nada.

   3. Asistente. La consulta al modelo tarda entre diez y veinte segundos, y
      en un postback eso congelaría la página entera y perdería la pestaña
      activa. Se pide contra Asistente.ashx y solo el bloque del asistente
      muestra que está trabajando.

   Sin dependencias. No usa jQuery aunque esté disponible.
   ========================================================================== */

(function () {
    'use strict';

    function listo(fn) {
        if (document.readyState === 'loading')
            document.addEventListener('DOMContentLoaded', fn);
        else
            fn();
    }

    listo(function () {

        // ---------------------------------------------------- Pestañas

        var pista = document.querySelector('.gc-pest'),
            tabs = document.querySelectorAll('.gc-pest__b'),
            paneles = document.querySelectorAll('.gc-vista'),
            guardado = document.getElementById('gcPestana');

        /*  En pantalla angosta las cuatro pestañas no caben y la pista
            desplaza. Se mueve solo el scroll de la pista, nunca el de la
            página: scrollIntoView haría saltar la vista vertical al cargar.

            Importa al volver de un postback de filtros estando en la última
            pestaña — sin esto, la persona vuelve a una pista que muestra la
            primera y parece que perdió su selección.                        */
        function acercar(tab) {
            if (!pista || !tab) return;

            var p = pista.getBoundingClientRect(),
                t = tab.getBoundingClientRect();

            if (t.left < p.left) pista.scrollLeft -= (p.left - t.left) + 8;
            else if (t.right > p.right) pista.scrollLeft += (t.right - p.right) + 8;
        }

        function activar(nombre) {
            var i, coincide, activa = null;

            for (i = 0; i < paneles.length; i++) {
                coincide = paneles[i].getAttribute('data-panel') === nombre;
                paneles[i].hidden = !coincide;
            }

            for (i = 0; i < tabs.length; i++) {
                coincide = tabs[i].getAttribute('data-panel') === nombre;
                tabs[i].classList.toggle('is-activa', coincide);
                tabs[i].setAttribute('aria-selected', coincide ? 'true' : 'false');
                tabs[i].setAttribute('tabindex', coincide ? '0' : '-1');

                if (coincide) activa = tabs[i];
            }

            acercar(activa);

            if (guardado) guardado.value = nombre;
        }

        for (var i = 0; i < tabs.length; i++) {
            tabs[i].addEventListener('click', function (e) {
                e.preventDefault();
                activar(this.getAttribute('data-panel'));
            });

            // Flechas para recorrer las pestañas, como pide el patrón de tablist.
            tabs[i].addEventListener('keydown', function (e) {
                var paso = e.key === 'ArrowRight' ? 1 : e.key === 'ArrowLeft' ? -1 : 0;
                if (!paso) return;

                e.preventDefault();

                var lista = Array.prototype.slice.call(tabs),
                    pos = lista.indexOf(this),
                    destino = lista[(pos + paso + lista.length) % lista.length];

                destino.focus();
                activar(destino.getAttribute('data-panel'));
            });
        }

        // El servidor ya marcó la pestaña correcta en el HTML. Solo se
        // reactiva cuando el campo oculto trae una distinta, que es el caso
        // de volver de un postback de filtros.
        if (guardado && guardado.value) activar(guardado.value);

        // ----------------------------------------------- Velo de carga

        var form = document.querySelector('form'),
            velo = document.getElementById('gcCargando');

        if (!form || !velo) return;

        function esperar() {
            document.body.classList.add('gc-esperando');
        }

        // Los botones normales envían el formulario y disparan «submit».
        form.addEventListener('submit', esperar);

        // Los desplegables con AutoPostBack y los LinkButton no: pasan por
        // __doPostBack, que internamente llama a form.submit(), y ese método
        // no dispara el evento. Como los filtros son justamente esos
        // controles, sin envolver __doPostBack el velo no aparecería nunca en
        // el caso que más importa.
        if (typeof window.__doPostBack === 'function') {
            var original = window.__doPostBack;

            window.__doPostBack = function () {
                esperar();
                return original.apply(this, arguments);
            };
        }

        // Volver con el botón «atrás» del navegador restaura la página desde
        // la caché con el velo encendido. Apagarlo al restaurar evita dejar
        // la pantalla bloqueada.
        window.addEventListener('pageshow', function () {
            document.body.classList.remove('gc-esperando');
        });
    });

    // ======================================================================
    //  3. Asistente
    // ======================================================================

    document.addEventListener('DOMContentLoaded', function () {

        var hilo   = document.getElementById('gcIaHilo');
        var caja   = document.getElementById('gcIaPregunta');
        var boton  = document.getElementById('gcIaEnviar');
        var estado = document.getElementById('gcIaEstado');

        // Sin hilo no está el bloque: el módulo puede estar apagado. Sin caja
        // está pero la persona no inició sesión, y entonces solo se lee.
        if (!hilo || !caja || !boton) { return; }

        var ocupado = false;

        // ------------------------------------------------------------------
        //  Pintado
        // ------------------------------------------------------------------

        function burbuja(clase, inicial) {
            var div = document.createElement('div');
            div.className = 'gc-msg ' + clase;

            var ico = document.createElement('span');
            ico.className = 'gc-msg__ico';
            ico.textContent = inicial;

            var cuerpo = document.createElement('div');
            cuerpo.className = 'gc-msg__b';

            div.appendChild(ico);
            div.appendChild(cuerpo);
            hilo.appendChild(div);

            div.scrollIntoView({ block: 'nearest' });
            return cuerpo;
        }

        /* La respuesta del modelo llega como HTML, y dentro puede venir
           reflejado texto que escribió una candidatura. Insertarla con
           innerHTML sería confiar en que el modelo nunca repita una etiqueta
           peligrosa, y esa no es una garantía que se pueda dar.

           Se analiza con DOMParser, que no ejecuta scripts ni descarga nada,
           y de ahí se toman únicamente los párrafos, como texto. El formato
           que sobrevive es el único que el prompt pide, y no hay atributo que
           pueda colarse. */
        function pintarRespuesta(contenedor, html) {
            var doc = new DOMParser().parseFromString(html || '', 'text/html');
            var parrafos = doc.body.querySelectorAll('p');
            var i;

            if (parrafos.length === 0) {
                var suelto = document.createElement('p');
                suelto.textContent = doc.body.textContent.trim();
                contenedor.appendChild(suelto);
                return;
            }

            for (i = 0; i < parrafos.length; i++) {
                var p = document.createElement('p');
                p.textContent = parrafos[i].textContent.trim();
                if (p.textContent) { contenedor.appendChild(p); }
            }
        }

        function pintarFuentes(contenedor, fuentes) {
            if (!fuentes || !fuentes.length) { return; }

            var caja = document.createElement('div');
            caja.className = 'gc-fuentes';

            var titulo = document.createElement('div');
            titulo.className = 'gc-fuentes__t';
            titulo.textContent = 'Fuentes consultadas';

            var lista = document.createElement('ol');

            for (var i = 0; i < fuentes.length; i++) {
                var li = document.createElement('li');
                li.textContent = fuentes[i];
                lista.appendChild(li);
            }

            caja.appendChild(titulo);
            caja.appendChild(lista);
            contenedor.appendChild(caja);
        }

        // ------------------------------------------------------------------
        //  Envío
        // ------------------------------------------------------------------

        function preguntar(texto) {
            if (ocupado) { return; }

            texto = (texto || '').trim();
            if (!texto) { caja.focus(); return; }

            ocupado = true;
            boton.disabled = true;
            caja.disabled = true;
            caja.value = '';

            burbuja('gc-msg--yo', boton.getAttribute('data-gc-iniciales') || 'TÚ')
                .appendChild(document.createTextNode(texto));

            var respuesta = burbuja('gc-msg--ia', 'IA');
            var espera = document.createElement('p');
            espera.className = 'gc-muted';
            espera.textContent = 'Consultando la base…';
            respuesta.appendChild(espera);

            if (estado) { estado.textContent = 'Consultando…'; }

            var campana = document.getElementById('gcCampanaSlug');

            fetch('Asistente.ashx', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    pregunta: texto,
                    campanaSlug: campana ? campana.value : ''
                })
            })
            .then(function (r) {
                if (!r.ok) { throw new Error('HTTP ' + r.status); }
                return r.json();
            })
            .then(function (d) {
                respuesta.innerHTML = '';

                if (d.ok) {
                    pintarRespuesta(respuesta, d.respuesta);
                    pintarFuentes(respuesta, d.fuentes);

                    if (estado) {
                        estado.textContent = d.restantes === 1
                            ? 'Te queda 1 consulta hoy.'
                            : 'Te quedan ' + d.restantes + ' consultas hoy.';
                    }

                    // Sin cuota, la caja deja de tener sentido.
                    if (d.restantes <= 0) { caja.placeholder = 'Sin consultas por hoy.'; }
                    else { caja.disabled = false; boton.disabled = false; }
                } else {
                    var aviso = document.createElement('p');
                    aviso.textContent = d.mensaje || 'No se pudo responder.';
                    respuesta.appendChild(aviso);

                    if (estado) { estado.textContent = ''; }
                    caja.disabled = false;
                    boton.disabled = false;
                }
            })
            .catch(function () {
                respuesta.innerHTML = '';
                var aviso = document.createElement('p');
                aviso.textContent = 'No se pudo consultar al asistente. '
                                  + 'Los gráficos de esta página siguen disponibles.';
                respuesta.appendChild(aviso);

                if (estado) { estado.textContent = ''; }
                caja.disabled = false;
                boton.disabled = false;
            })
            .then(function () {
                ocupado = false;
                if (!caja.disabled) { caja.focus(); }
            });
        }

        boton.addEventListener('click', function () { preguntar(caja.value); });

        // La caja vive dentro del formulario de Web Forms. Sin esto, Enter lo
        // envía y recarga la página entera, que es justo lo que se evitó.
        caja.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                preguntar(caja.value);
            }
        });

        var sugerencias = document.querySelectorAll('[data-gc-pregunta]');

        for (var i = 0; i < sugerencias.length; i++) {
            sugerencias[i].addEventListener('click', function () {
                preguntar(this.getAttribute('data-gc-pregunta'));
            });
        }
    });

})();
