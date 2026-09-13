<%@ Page Title="Para organizaciones" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Organizaciones.aspx.cs" Inherits="frontend.OrganizacionesPagina" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= Recursos.Url("~/Content/cumplehn-organizaciones.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <%-- ======================================================= Hero --%>

    <section class="gc-org__hero">
        <div class="container">
            <div class="gc-org__hero-in">
                <div>
                    <p class="gc-eyebrow">Para organizaciones</p>
                    <h1>Tu elección interna, con la seriedad de una elección pública.</h1>
                    <p class="gc-org__lead">
                        Un espacio propio en CumpleHN para la junta directiva de tu colegio, cooperativa,
                        sindicato o asociación: planillas, propuestas, participación de los miembros y un
                        tablero con resultados, con la neutralidad que ya sostiene el sitio público.
                    </p>
                    <div class="gc-org__cta">
                        <a class="gc-btn gc-btn--lg" href="#planes">Ver planes y precios</a>
                        <asp:HyperLink ID="lnkDemo" runat="server" CssClass="gc-btn gc-btn--ghost gc-btn--lg" Target="_blank">Ver un espacio de ejemplo</asp:HyperLink>
                    </div>
                    <p class="gc-org__fine">Sin tarjeta. Sin instalación. Se activa en un día hábil.</p>
                </div>

                <%-- Una tarjeta que imita la portada de un espacio: dice más que un
                     párrafo sobre qué se compra. --%>
                <div class="gc-org__mock" aria-hidden="true">
                    <div class="gc-org__mock-bar">
                        <span class="gc-org__mock-brand">Colegio de Ingenieros<small>con CumpleHN</small></span>
                        <span class="gc-org__mock-nav"><i></i><i></i><i></i><i></i></span>
                    </div>
                    <div class="gc-org__mock-body">
                        <div class="gc-org__mock-title">Junta Directiva 2027</div>
                        <div class="gc-org__mock-row"><span class="gc-org__mock-chip gc-org__mock-chip--a">Planilla Azul</span><span class="gc-org__mock-chip gc-org__mock-chip--v">Planilla Verde</span></div>
                        <div class="gc-org__mock-bars">
                            <div><b style="width: 62%"></b><span>Infraestructura del colegio</span></div>
                            <div><b style="width: 47%"></b><span>Formación continua</span></div>
                            <div><b style="width: 31%"></b><span>Finanzas y transparencia</span></div>
                        </div>
                        <div class="gc-org__mock-foot">248 miembros en el padrón · 173 han participado</div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <%-- =================================================== Para quién --%>

    <section class="gc-org__sec">
        <div class="container">
            <div class="gc-org__head">
                <h2>Hecho para organizaciones que eligen a su gente</h2>
                <p>Donde hoy hay un grupo de mensajería, un formulario y una hoja de cálculo.</p>
            </div>
            <div class="row gc-org__quien">
                <div class="col-lg-3 col-md-6"><div class="gc-org__quien-it"><span class="gc-org__ico gc-org__ico--t">&#9670;</span><strong>Colegios profesionales</strong><span>Juntas directivas y tribunales de honor con cientos de agremiados.</span></div></div>
                <div class="col-lg-3 col-md-6"><div class="gc-org__quien-it"><span class="gc-org__ico gc-org__ico--v">&#9670;</span><strong>Cooperativas</strong><span>Consejos de administración y juntas de vigilancia elegidos en asamblea.</span></div></div>
                <div class="col-lg-3 col-md-6"><div class="gc-org__quien-it"><span class="gc-org__ico gc-org__ico--a">&#9670;</span><strong>Sindicatos y gremios</strong><span>Directivas seccionales y centrales con afiliados en varias sedes.</span></div></div>
                <div class="col-lg-3 col-md-6"><div class="gc-org__quien-it"><span class="gc-org__ico gc-org__ico--e">&#9670;</span><strong>Asociaciones y universidades</strong><span>Patronatos, asociaciones de estudiantes y consejos que se renuevan cada año.</span></div></div>
            </div>
        </div>
    </section>

    <%-- ================================================== Qué incluye --%>

    <section class="gc-org__sec gc-org__sec--alt">
        <div class="container">
            <div class="gc-org__head">
                <h2>Todo lo que hace CumpleHN, dentro de tu espacio</h2>
                <p>El mismo motor del sitio público, con tu nombre, tu padrón y tus reglas.</p>
            </div>
            <div class="row gc-org__feats">
                <div class="col-lg-4 col-md-6"><div class="gc-org__feat"><h3>Sitio propio</h3><p>Tu portada en <code>cumplehn.hn/e/tu-organizacion</code>, con el nombre de la organización como marca y CumpleHN como herramienta.</p></div></div>
                <div class="col-lg-4 col-md-6"><div class="gc-org__feat"><h3>Planillas y candidaturas</h3><p>Cada planilla con su ficha, cada candidatura con su perfil y sus propuestas, identificadas como declaradas hasta que la comisión las verifique.</p></div></div>
                <div class="col-lg-4 col-md-6"><div class="gc-org__feat"><h3>Padrón cerrado</h3><p>Cargás la lista de correos de tus miembros. Solo ellos participan. Quien no está en la lista puede mirar, pero no votar.</p></div></div>
                <div class="col-lg-4 col-md-6"><div class="gc-org__feat"><h3>Participación</h3><p>Apoyos, comentarios y encuestas de percepción, con una respuesta por persona garantizada por la base de datos, no por el formulario.</p></div></div>
                <div class="col-lg-4 col-md-6"><div class="gc-org__feat"><h3>Tablero de analítica</h3><p>Propuestas por área, participación por día, cobertura y verificación, calculado en vivo y con cada cifra etiquetada.</p></div></div>
                <div class="col-lg-4 col-md-6"><div class="gc-org__feat"><h3>Moderación con bitácora</h3><p>Tu comisión verifica y modera lo suyo, con motivo obligatorio y registro de quién hizo qué. Nada se borra: todo se retira con rastro.</p></div></div>
            </div>
        </div>
    </section>

    <%-- ================================================ Cómo funciona --%>

    <section class="gc-org__sec">
        <div class="container">
            <div class="gc-org__head">
                <h2>Cómo funciona</h2>
                <p>Cuatro pasos y un día hábil.</p>
            </div>
            <ol class="gc-org__pasos">
                <li><span>1</span><strong>Solicitás el espacio</strong><p>Con el formulario de abajo. Te escribimos el mismo día con los datos para el pago.</p></li>
                <li><span>2</span><strong>Pagás por transferencia</strong><p>Al recibirlo, activamos el espacio por el período del plan y te entregamos la cuenta de administración.</p></li>
                <li><span>3</span><strong>Cargás tu proceso</strong><p>Campaña, planillas, candidaturas y el padrón de correos de tus miembros. Cada candidatura publica sus propuestas.</p></li>
                <li><span>4</span><strong>Tus miembros participan</strong><p>Se registran con el correo del padrón, confirman y participan. Vos mirás el tablero.</p></li>
            </ol>
        </div>
    </section>

    <%-- ======================================================= Planes --%>

    <section class="gc-org__sec gc-org__sec--alt" id="planes">
        <div class="container">
            <div class="gc-org__head">
                <h2>Planes</h2>
                <p>Precios en lempiras, por período de vigencia. Sin cargos ocultos ni renovación automática.</p>
            </div>

            <asp:PlaceHolder ID="phPlanes" runat="server">
                <div class="row gc-org__planes">
                    <asp:Repeater ID="rptPlanes" runat="server">
                        <ItemTemplate>
                            <div class="col-lg-4 col-md-6">
                                <div class="gc-org__plan <%# (bool)Eval("Destacado") ? "is-destacado" : "" %>">
                                    <%# (bool)Eval("Destacado") ? "<span class=\"gc-org__plan-tag\">El más elegido</span>" : "" %>
                                    <h3><%#: Eval("Nombre") %></h3>
                                    <p class="gc-org__plan-lema"><%#: Eval("Lema") %></p>
                                    <div class="gc-org__plan-precio"><%#: Eval("PrecioTexto") %><small>por <%#: Eval("DuracionTexto") %></small></div>
                                    <ul>
                                        <li><%#: Eval("DuracionTexto") %> de vigencia</li>
                                        <li><%#: Eval("MiembrosTexto") %></li>
                                        <li>Sitio propio, planillas y candidaturas</li>
                                        <li>Encuestas, participación y tablero</li>
                                        <li>Verificación y moderación con bitácora</li>
                                    </ul>
                                    <p class="gc-org__plan-desc"><%#: Eval("Descripcion") %></p>
                                    <a class="gc-btn <%# (bool)Eval("Destacado") ? "" : "gc-btn--ghost" %> gc-btn--block" href="#solicitud" data-plan="<%# Eval("Codigo") %>">Solicitar <%#: Eval("Nombre") %></a>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phSinPlanes" runat="server" Visible="false">
                <div class="gc-note"><span>Los planes no están disponibles en este momento. Escribinos con el formulario y te los enviamos.</span></div>
            </asp:PlaceHolder>

            <div class="gc-org__pago">
                <strong>Cómo se paga.</strong> Por transferencia o depósito bancario, con el número de referencia.
                No procesamos tarjetas y no guardamos datos de pago: al recibir el comprobante, registramos
                el pago y el espacio queda vigente por el período del plan. Renovar es volver a pagar. Nada se
                cobra solo.
            </div>
        </div>
    </section>

    <%-- ==================================================== Solicitud --%>

    <section class="gc-org__sec" id="solicitud">
        <div class="container">
            <div class="row">
                <div class="col-lg-5">
                    <div class="gc-org__head gc-org__head--left">
                        <h2>Solicitá tu espacio</h2>
                        <p>
                            Contanos qué van a elegir y te escribimos el mismo día hábil con los pasos para
                            activarlo. Sin compromiso: el espacio se activa cuando se registra el pago.
                        </p>
                    </div>
                    <dl class="gc-org__faq">
                        <dt>¿Quién administra el contenido?</dt>
                        <dd>Tu organización, con la cuenta que te entregamos. CumpleHN es la herramienta y lo declara en cada página del espacio.</dd>
                        <dt>¿Los miembros necesitan cuenta?</dt>
                        <dd>Sí, una cuenta gratuita con el correo que está en tu padrón. La confirman con un enlace y ya pueden participar.</dd>
                        <dt>¿Qué pasa cuando vence?</dt>
                        <dd>El espacio sigue en línea para consulta, con un aviso. Nadie participa ni administra hasta el siguiente pago. Nada se borra.</dd>
                        <dt>¿Puedo ver los datos de mis miembros?</dt>
                        <dd>Ves quién está en el padrón y quién participó. No ves qué votó cada persona: eso no lo ve nadie.</dd>
                    </dl>
                </div>
                <div class="col-lg-7">
                    <div class="gc-card gc-org__form">
                        <div class="gc-card__body">

                            <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
                                <div class="<%= ClaseMensaje %>" role="status"><%: Mensaje %></div>
                            </asp:PlaceHolder>

                            <asp:PlaceHolder ID="phForm" runat="server">
                                <div class="gc-form">
                                    <div class="gc-row2">
                                        <div class="gc-field">
                                            <label for="<%= txtOrganizacion.ClientID %>">Organización</label>
                                            <asp:TextBox ID="txtOrganizacion" runat="server" CssClass="gc-input" MaxLength="200" placeholder="Colegio de Ingenieros Civiles de Honduras" />
                                        </div>
                                        <div class="gc-field">
                                            <label for="<%= txtContacto.ClientID %>">Con quién nos comunicamos</label>
                                            <asp:TextBox ID="txtContacto" runat="server" CssClass="gc-input" MaxLength="160" placeholder="Nombre y cargo" />
                                        </div>
                                    </div>
                                    <div class="gc-row2">
                                        <div class="gc-field">
                                            <label for="<%= txtCorreo.ClientID %>">Correo</label>
                                            <asp:TextBox ID="txtCorreo" runat="server" CssClass="gc-input" MaxLength="160" TextMode="Email" placeholder="comision@organizacion.hn" />
                                        </div>
                                        <div class="gc-field">
                                            <label for="<%= txtTelefono.ClientID %>">Teléfono <span class="gc-muted">(opcional)</span></label>
                                            <asp:TextBox ID="txtTelefono" runat="server" CssClass="gc-input" MaxLength="40" placeholder="+504 9999-9999" />
                                        </div>
                                    </div>
                                    <div class="gc-row2">
                                        <div class="gc-field">
                                            <label for="<%= ddlPlan.ClientID %>">Plan que te interesa</label>
                                            <asp:DropDownList ID="ddlPlan" runat="server" CssClass="gc-input gc-select" />
                                        </div>
                                        <div class="gc-field">
                                            <label for="<%= txtFecha.ClientID %>">Fecha aproximada de la elección</label>
                                            <asp:TextBox ID="txtFecha" runat="server" CssClass="gc-input" TextMode="Date" />
                                        </div>
                                    </div>
                                    <div class="gc-field">
                                        <label for="<%= txtProceso.ClientID %>">Qué van a elegir</label>
                                        <asp:TextBox ID="txtProceso" runat="server" CssClass="gc-input" MaxLength="300" placeholder="Junta directiva 2027, tres planillas, unos 800 agremiados" />
                                    </div>
                                    <div class="gc-field">
                                        <label for="<%= txtMensaje.ClientID %>">Algo más que debamos saber <span class="gc-muted">(opcional)</span></label>
                                        <asp:TextBox ID="txtMensaje" runat="server" CssClass="gc-input" TextMode="MultiLine" Rows="3" MaxLength="1000" />
                                    </div>
                                    <%-- Trampa para bots: un campo que ninguna persona ve ni llena. --%>
                                    <div class="gc-org__hp" aria-hidden="true">
                                        <asp:TextBox ID="txtSitio" runat="server" TabIndex="-1" autocomplete="off" />
                                    </div>
                                </div>
                                <div class="gc-formfoot">
                                    <asp:Button ID="btnEnviar" runat="server" CssClass="gc-btn gc-btn--lg" Text="Solicitar el espacio" OnClick="btnEnviar_Click" />
                                    <span class="gc-muted gc-small">Te respondemos al correo que indiques.</span>
                                </div>
                            </asp:PlaceHolder>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <script>
        // Al elegir un plan desde su tarjeta, queda seleccionado en el formulario.
        (function () {
            var sel = document.getElementById('<%= ddlPlan.ClientID %>');
            if (!sel) return;
            var enlaces = document.querySelectorAll('a[data-plan]');
            for (var i = 0; i < enlaces.length; i++) {
                enlaces[i].addEventListener('click', function () {
                    sel.value = this.getAttribute('data-plan');
                });
            }
        })();
    </script>

</asp:Content>
