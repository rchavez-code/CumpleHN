<%@ Page Title="Mis consultas al asistente" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReporteAsistente.aspx.cs" Inherits="frontend.ReporteAsistente" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= Recursos.Url("~/Content/cumplehn-reporte.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container gc-rep">

        <%-- ================================================ Elegir --%>

        <asp:PlaceHolder ID="phElegir" runat="server">

            <div class="gc-pagehead">
                <div>
                    <p class="gc-eyebrow">Asistente de consulta</p>
                    <h1 style="font-size: 1.7rem; margin: 0;">Mis consultas</h1>
                </div>
                <a class="gc-btn gc-btn--ghost gc-btn--sm" href="<%: ResolveUrl("~/Analitica") %>">Volver al tablero</a>
            </div>

            <p class="gc-lead gc-rep__intro">
                Elegí las consultas que querés incluir y generá un reporte con la pregunta, la
                respuesta y la fecha de cada una. Desde el reporte lo podés imprimir o guardar como PDF.
            </p>

            <asp:PlaceHolder ID="phError" runat="server" Visible="false">
                <p class="gc-alert gc-mb"><asp:Literal ID="litError" runat="server" /></p>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phLista" runat="server">

                <div class="gc-card">
                    <div class="gc-rep__barra-lista">
                        <label class="gc-rep__todas">
                            <input type="checkbox" id="gcRepTodas" />
                            <span>Seleccionar todas</span>
                        </label>
                        <span class="gc-muted gc-small" id="gcRepCuenta"><%: TextoTotal %></span>
                    </div>

                    <ul class="gc-rep__lista">
                        <asp:Repeater ID="rptConsultas" runat="server" EnableViewState="false">
                            <ItemTemplate>
                                <li>
                                    <label class="gc-rep__op">
                                        <input type="checkbox" name="consulta"
                                            value="<%#: Eval("Codigo") %>"
                                            <%# Elegida((int)Eval("Codigo")) ? "checked" : "" %> />
                                        <span class="gc-rep__op-preg"><%#: Eval("Pregunta") %></span>
                                        <span class="gc-rep__op-fecha"><%#: Vista.FechaHora((DateTime)Eval("Fecha")) %></span>
                                    </label>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>

                <div class="gc-formfoot" style="margin-top: 18px;">
                    <asp:Button ID="btnGenerar" runat="server" CssClass="gc-btn"
                        Text="Generar reporte" OnClick="btnGenerar_Click" />
                </div>

            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
                <div class="gc-card gc-empty">
                    <h3>Todavía no tenés consultas respondidas</h3>
                    <p>Las preguntas que le hagás al asistente desde el tablero van a aparecer acá.</p>
                    <a class="gc-btn" href="<%: ResolveUrl("~/Analitica") %>">Ir al tablero</a>
                </div>
            </asp:PlaceHolder>

        </asp:PlaceHolder>

        <%-- ================================================ Reporte --%>

        <asp:PlaceHolder ID="phReporte" runat="server" Visible="false">

            <div class="gc-rep__acciones">
                <a class="gc-btn gc-btn--ghost gc-btn--sm" href="<%: ResolveUrl("~/ReporteAsistente") %>">Elegir otras consultas</a>
                <button type="button" class="gc-btn gc-btn--sm" id="gcRepImprimir">Imprimir o guardar como PDF</button>
            </div>

            <article class="gc-card gc-rep__doc">

                <header class="gc-rep__cab">
                    <p class="gc-eyebrow">CumpleHN · Asistente de consulta</p>
                    <h1>Reporte de consultas</h1>
                    <p class="gc-rep__meta">
                        <%: Sesion.Nombre %> · Generado el <%: Generado %> · <%: TextoIncluidas %>
                    </p>
                </header>

                <asp:Repeater ID="rptReporte" runat="server" EnableViewState="false">
                    <ItemTemplate>
                        <section class="gc-rep__item">
                            <div class="gc-rep__item-cab">
                                <span class="gc-rep__num"><%# Container.ItemIndex + 1 %></span>
                                <span class="gc-rep__fecha"><%#: Vista.FechaHora((DateTime)Eval("Fecha")) %></span>
                            </div>
                            <p class="gc-rep__rotulo">Pregunta</p>
                            <p class="gc-rep__preg"><%#: Eval("Pregunta") %></p>
                            <p class="gc-rep__rotulo">Respuesta</p>
                            <%-- La respuesta viaja como texto en el atributo, escapada, y
                                 cumplehn-respuesta-ia.js la reconstruye con la lista blanca.
                                 Escribirla directo en el marcado sería confiar en el modelo. --%>
                            <div class="gc-rep__resp" data-gc-respuesta="<%#: Eval("Respuesta") %>"></div>
                        </section>
                    </ItemTemplate>
                </asp:Repeater>

                <p class="gc-rep__pie">
                    Las respuestas las redactó un modelo de lenguaje con los datos registrados en
                    CumpleHN al momento de cada consulta. Pueden contener errores de redacción: las
                    cifras se pueden contrastar en el tablero de analítica.
                </p>

            </article>

        </asp:PlaceHolder>

    </div>

    <script src="<%= Recursos.Url("~/Scripts/cumplehn-respuesta-ia.js") %>"></script>
    <script>
        (function () {
            'use strict';

            var cajas = document.querySelectorAll('[data-gc-respuesta]');
            for (var i = 0; i < cajas.length; i++) {
                window.CumpleHN.pintarRespuestaIA(cajas[i], cajas[i].getAttribute('data-gc-respuesta'));
            }

            var imprimir = document.getElementById('gcRepImprimir');
            if (imprimir) {
                imprimir.addEventListener('click', function () { window.print(); });
            }

            var todas = document.getElementById('gcRepTodas');
            if (!todas) { return; }

            var casillas = document.querySelectorAll('input[name="consulta"]');

            function sincronizar() {
                var marcadas = 0;
                for (var j = 0; j < casillas.length; j++) { if (casillas[j].checked) { marcadas++; } }

                todas.checked = marcadas === casillas.length;
                todas.indeterminate = marcadas > 0 && marcadas < casillas.length;
            }

            todas.addEventListener('change', function () {
                for (var j = 0; j < casillas.length; j++) { casillas[j].checked = todas.checked; }
                sincronizar();
            });

            for (var k = 0; k < casillas.length; k++) {
                casillas[k].addEventListener('change', sincronizar);
            }

            sincronizar();
        })();
    </script>

</asp:Content>
