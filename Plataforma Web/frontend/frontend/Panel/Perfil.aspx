<%@ Page Title="Mi perfil" Language="C#" MasterPageFile="~/Panel/Panel.Master" AutoEventWireup="true" CodeBehind="Perfil.aspx.cs" Inherits="frontend.Panel.Perfil" %>

<asp:Content ContentPlaceHolderID="TopActions" runat="server">
    <a class="gc-btn gc-btn--ghost gc-btn--sm" href="<%= UrlPerfilPublico %>">Ver perfil público</a>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Mi perfil</h2>
            <p class="gc-muted" style="margin: 0;">
                Esta es la información que verá cualquier persona que visite tu perfil público.
            </p>
        </div>
        <span class="<%= VerificacionClase %>"><%: VerificacionTexto %></span>
    </div>

    <asp:PlaceHolder ID="phOk" runat="server" Visible="false">
        <p class="gc-ok gc-mb"><asp:Literal ID="litOk" runat="server" /></p>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phError" runat="server" Visible="false">
        <p class="gc-alert gc-mb"><asp:Literal ID="litError" runat="server" /></p>
    </asp:PlaceHolder>

    <div class="row">
        <div class="col-lg-8">
            <div class="gc-card gc-mb">
                <div class="gc-card__body">
                    <div class="gc-form">

                        <%-- ==================================== Fotografía --%>

                        <fieldset class="gc-fieldset">
                            <legend>Fotografía</legend>

                            <div class="gc-drop">
                                <span class="gc-avatar gc-avatar--lg" style="<%= EstiloAvatar %>" aria-hidden="true"><%: Iniciales %></span>
                                <div style="flex: 1 1 auto; min-width: 0;">
                                    <asp:FileUpload ID="fuFoto" runat="server" CssClass="gc-input" />
                                    <span class="gc-hint">
                                        Formato JPG o PNG, mínimo 400 × 400 píxeles. Se recorta en forma circular.
                                    </span>
                                </div>
                            </div>
                        </fieldset>

                        <%-- ====================================== Identidad --%>

                        <fieldset class="gc-fieldset">
                            <legend>Identidad</legend>

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
                        </fieldset>

                        <%-- ===================================== Candidatura --%>

                        <fieldset class="gc-fieldset">
                            <legend>Candidatura</legend>

                            <div class="gc-row2">
                                <div class="gc-field">
                                    <label for="<%= ddlCargo.ClientID %>">Cargo al que aspirás</label>
                                    <asp:DropDownList ID="ddlCargo" runat="server" CssClass="gc-select" />
                                </div>
                                <div class="gc-field">
                                    <label for="<%= txtPartido.ClientID %>">Partido político</label>
                                    <asp:TextBox ID="txtPartido" runat="server" CssClass="gc-input"
                                        placeholder="Vacío si es independiente" />
                                </div>
                            </div>

                            <div class="gc-row2">
                                <div class="gc-field">
                                    <label for="<%= ddlDepartamento.ClientID %>">Departamento</label>
                                    <asp:DropDownList ID="ddlDepartamento" runat="server" CssClass="gc-select" />
                                </div>
                                <div class="gc-field">
                                    <label for="<%= txtMunicipio.ClientID %>">Municipio</label>
                                    <asp:TextBox ID="txtMunicipio" runat="server" CssClass="gc-input"
                                        placeholder="Solo si tu cargo es municipal" />
                                </div>
                            </div>
                        </fieldset>

                        <%-- ==================================== Presentación --%>

                        <fieldset class="gc-fieldset">
                            <legend>Presentación</legend>

                            <div class="gc-field">
                                <label for="<%= txtTitular.ClientID %>">Titular</label>
                                <asp:TextBox ID="txtTitular" runat="server" CssClass="gc-input" MaxLength="140" />
                                <span class="gc-hint">
                                    Una sola línea que resuma tu candidatura. Es lo primero que se lee en tu perfil.
                                </span>
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtBiografia.ClientID %>">Biografía</label>
                                <asp:TextBox ID="txtBiografia" runat="server" CssClass="gc-textarea" TextMode="MultiLine" />
                                <span class="gc-hint">Tu trayectoria personal y pública.</span>
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtProfesional.ClientID %>">Información profesional</label>
                                <asp:TextBox ID="txtProfesional" runat="server" CssClass="gc-textarea" TextMode="MultiLine" />
                                <span class="gc-hint">Formación académica y experiencia laboral relevante.</span>
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtCandidatura.ClientID %>">Descripción de tu candidatura</label>
                                <asp:TextBox ID="txtCandidatura" runat="server" CssClass="gc-textarea" TextMode="MultiLine" />
                                <span class="gc-hint">Qué proponés y por qué. Los proyectos concretos se registran aparte.</span>
                            </div>
                        </fieldset>

                        <%-- ================================ Contacto público --%>

                        <fieldset class="gc-fieldset">
                            <legend>Contacto público</legend>

                            <div class="gc-note">
                                <span>
                                    Solo lo que llenés en esta sección se publica. Dejá vacío lo que no querás
                                    que sea visible para cualquier visitante del sitio.
                                </span>
                            </div>

                            <div class="gc-row2">
                                <div class="gc-field">
                                    <label for="<%= txtCorreo.ClientID %>">Correo de contacto</label>
                                    <asp:TextBox ID="txtCorreo" runat="server" CssClass="gc-input" TextMode="Email" />
                                </div>
                                <div class="gc-field">
                                    <label for="<%= txtTelefono.ClientID %>">Teléfono</label>
                                    <asp:TextBox ID="txtTelefono" runat="server" CssClass="gc-input" />
                                </div>
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtSitio.ClientID %>">Sitio web</label>
                                <asp:TextBox ID="txtSitio" runat="server" CssClass="gc-input" placeholder="https://" />
                            </div>
                        </fieldset>

                        <%-- =================================== Redes sociales --%>

                        <fieldset class="gc-fieldset">
                            <legend>Redes sociales</legend>

                            <div class="gc-row3">
                                <div class="gc-field">
                                    <label for="<%= txtFacebook.ClientID %>">Facebook</label>
                                    <asp:TextBox ID="txtFacebook" runat="server" CssClass="gc-input" placeholder="usuario" />
                                </div>
                                <div class="gc-field">
                                    <label for="<%= txtX.ClientID %>">X</label>
                                    <asp:TextBox ID="txtX" runat="server" CssClass="gc-input" placeholder="usuario" />
                                </div>
                                <div class="gc-field">
                                    <label for="<%= txtInstagram.ClientID %>">Instagram</label>
                                    <asp:TextBox ID="txtInstagram" runat="server" CssClass="gc-input" placeholder="usuario" />
                                </div>
                            </div>
                        </fieldset>

                        <div class="gc-formfoot">
                            <a class="gc-btn gc-btn--quiet" href="<%= ResolveUrl("~/Panel/") %>">Cancelar</a>
                            <asp:Button ID="btnGuardar" runat="server" CssClass="gc-btn"
                                Text="Guardar cambios" OnClick="btnGuardar_Click" />
                        </div>

                    </div>
                </div>
            </div>
        </div>

        <%-- ================================================ Riel derecho --%>

        <div class="col-lg-4">
            <div class="gc-card gc-mb">
                <div class="gc-card__body">
                    <h3 style="margin-bottom: 10px;">Avance del perfil</h3>
                    <div class="gc-meter" role="img" aria-label="Avance del perfil: <%: PerfilCompleto %> por ciento">
                        <div class="gc-meter__fill" style="width: <%: PerfilCompleto %>%;"></div>
                    </div>
                    <p class="gc-muted gc-small" style="margin: 10px 0 0;">
                        <%: PerfilCompleto %> % de los campos del perfil tienen contenido.
                    </p>
                </div>
            </div>

            <div class="gc-note gc-note--ambar">
                <span>
                    En esta etapa el formulario todavía no persiste los cambios. El guardado se conecta al
                    Web Service de candidatos del backend, que es el que escribe en la base de datos.
                </span>
            </div>
        </div>
    </div>

</asp:Content>
