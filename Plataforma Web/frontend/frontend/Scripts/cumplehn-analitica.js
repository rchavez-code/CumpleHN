/* ==========================================================================
   CumpleHN — Tablero analítico
   --------------------------------------------------------------------------
   Dos comportamientos, nada más. Todo lo demás —cifras, barras, SVG,
   hallazgos— lo genera el servidor y llega ya dibujado en el HTML.

   1. Pestañas. Los cuatro paneles vienen renderizados en la misma respuesta,
      así que cambiar de pestaña no necesita ir al servidor. El estado se
      guarda en un campo oculto para que un postback de filtros devuelva a la
      persona a la pestaña donde estaba.

   2. Velo de carga. Filtrar es un postback, y un postback sin respuesta
      visible parece que no hizo nada.

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

})();
