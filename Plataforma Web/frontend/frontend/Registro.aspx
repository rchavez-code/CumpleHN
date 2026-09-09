<%@ Page Title="Crear cuenta" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Registro.aspx.cs" Inherits="frontend.Registro" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-auth">

        <aside class="gc-auth__aside">
            <p class="gc-eyebrow" style="color: rgba(255,255,255,.65);">Crear cuenta</p>

            <h2><%: TituloAside %></h2>

            <p><%: TextoAside %></p>

            <ul class="gc-auth__list">
                <asp:Repeater ID="rptBeneficios" runat="server">
                    <ItemTemplate>
                        <li><span aria-hidden="true">&mdash;</span><span><%#: Container.DataItem %></span></li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </aside>

        <div class="gc-auth__form">
            <div class="gc-auth__box">

                <h1 style="font-size: 1.7rem;">Crear una cuenta</h1>
                <p class="gc-muted" style="margin-bottom: 22px;">
                    Elegí el tipo de cuenta que necesitás.
                </p>

                <%-- ======================================= Tipo de cuenta --%>

                <div class="gc-pick gc-mb">
                    <a class="gc-pick__opt <%= ClaseTipo("ciudadano") %>" href="<%= UrlConDestino("~/Registro?tipo=ciudadano") %>">
                        <div>
                            <strong>Cuenta ciudadana</strong>
                            <span>Para apoyar publicaciones, comentar, participar en votaciones de percepción y guardar lo que te interesa.</span>
                        </div>
                    </a>

                    <a class="gc-pick__opt <%= ClaseTipo("candidato") %>" href="<%= UrlConDestino("~/Registro?tipo=candidato") %>">
                        <div>
                            <strong>Cuenta de candidato</strong>
                            <span>Para registrar tu candidatura en la campaña actual y administrar tu perfil, tus proyectos y tus publicaciones.</span>
                        </div>
                    </a>
                </div>

                <asp:PlaceHolder ID="phError" runat="server" Visible="false">
                    <p class="gc-alert gc-mb"><asp:Literal ID="litError" runat="server" /></p>
                </asp:PlaceHolder>

                <%-- ============================================ Formulario --%>

                <div class="gc-form">

                    <div class="gc-row2">
                        <div class="gc-field">
                            <label for="<%= txtNombres.ClientID %>">Nombres</label>
                            <asp:TextBox ID="txtNombres" runat="server" CssClass="gc-input" />
                        </div>
                        <div class="gc-field">
                            <label for="<%= txtApellidos.ClientID %>">Apellidos</label>
                            <asp:TextBox ID="txtApellidos" runat="server" CssClass="gc-input" />
                        </div>
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtCorreo.ClientID %>">Correo electrónico</label>
                        <asp:TextBox ID="txtCorreo" runat="server" CssClass="gc-input" TextMode="Email"
                            placeholder="tu@correo.hn" />
                    </div>

                    <div class="gc-row2">
                        <div class="gc-field">
                            <label for="<%= txtClave.ClientID %>">Contraseña</label>
                            <asp:TextBox ID="txtClave" runat="server" CssClass="gc-input" TextMode="Password" />
                            <span class="gc-hint">Mínimo ocho caracteres.</span>
                        </div>
                        <div class="gc-field">
                            <label for="<%= txtClave2.ClientID %>">Repetir contraseña</label>
                            <asp:TextBox ID="txtClave2" runat="server" CssClass="gc-input" TextMode="Password" />
                        </div>
                    </div>

                    <%-- ================================= Datos de candidato --%>

                    <asp:PlaceHolder ID="phCandidato" runat="server" Visible="false">
                        <fieldset class="gc-fieldset">
                            <legend>Datos de la candidatura</legend>

                            <div class="gc-field">
                                <label for="<%= ddlCampana.ClientID %>">Campaña electoral</label>
                                <asp:DropDownList ID="ddlCampana" runat="server" CssClass="gc-select" />
                                <span class="gc-hint">Solo se listan las campañas con registro abierto.</span>
                            </div>

                            <div class="gc-row2">
                                <div class="gc-field">
                                    <label for="<%= ddlCargo.ClientID %>">Cargo al que aspirás</label>
                                    <asp:DropDownList ID="ddlCargo" runat="server" CssClass="gc-select" />
                                </div>
                                <div class="gc-field">
                                    <label for="<%= ddlDepartamento.ClientID %>">Departamento</label>
                                    <asp:DropDownList ID="ddlDepartamento" runat="server" CssClass="gc-select" />
                                </div>
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtPartido.ClientID %>">Partido político</label>
                                <asp:TextBox ID="txtPartido" runat="server" CssClass="gc-input"
                                    placeholder="Dejalo vacío si tu candidatura es independiente" />
                            </div>
                        </fieldset>

                        <div class="gc-note gc-note--ambar">
                            <span>
                                La candidatura no se publica sola: la registra la administración de la
                                plataforma con estos datos y te entrega la cuenta de acceso. El perfil
                                aparece identificado como declarado por la candidatura, porque la
                                plataforma no avala su contenido hasta que exista una fuente verificable
                                que lo respalde.
                            </span>
                        </div>
                    </asp:PlaceHolder>

                    <label style="display: flex; align-items: flex-start; gap: 9px; font-size: .87rem; font-weight: 500;">
                        <asp:CheckBox ID="chkTerminos" runat="server" />
                        <span>
                            Confirmo que la información que registre es veraz y acepto que se publique de
                            forma pública en la plataforma.
                        </span>
                    </label>

                    <asp:Button ID="btnCrear" runat="server" CssClass="gc-btn gc-btn--lg gc-btn--block"
                        Text="Crear cuenta" OnClick="btnCrear_Click" />

                    <p class="gc-muted gc-small" style="margin: 0;">
                        ¿Ya tenés cuenta? <a href="<%= UrlConDestino("~/Acceso") %>">Acceder</a>.
                    </p>

                </div>

            </div>
        </div>

    </div>

</asp:Content>
