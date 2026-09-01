<%@ Page Title="Acceder" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Acceso.aspx.cs" Inherits="frontend.Acceso" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-auth">

        <%-- ============================================ Columna de valor --%>

        <aside class="gc-auth__aside">
            <p class="gc-eyebrow" style="color: rgba(255,255,255,.65);">Acceso a CumpleHN</p>

            <h2>Consultar es libre. Participar necesita cuenta.</h2>

            <p>
                Toda la información de campañas, candidaturas y propuestas es pública y no requiere registro.
                La cuenta se necesita solo para participar o para administrar una candidatura.
            </p>

            <ul class="gc-auth__list">
                <li><span aria-hidden="true">&mdash;</span><span>Apoyar publicaciones y participar en votaciones de percepción</span></li>
                <li><span aria-hidden="true">&mdash;</span><span>Comentar en las publicaciones de las candidaturas</span></li>
                <li><span aria-hidden="true">&mdash;</span><span>Guardar candidaturas y propuestas de tu interés</span></li>
                <li><span aria-hidden="true">&mdash;</span><span>Administrar tu perfil y tus proyectos si sos candidato</span></li>
            </ul>
        </aside>

        <%-- ================================================= Formulario --%>

        <div class="gc-auth__form">
            <div class="gc-auth__box">

                <h1 style="font-size: 1.7rem;">Acceder</h1>
                <p class="gc-muted" style="margin-bottom: 24px;">
                    Ingresá con la cuenta que usás en la plataforma.
                </p>

                <asp:PlaceHolder ID="phError" runat="server" Visible="false">
                    <p class="gc-alert gc-mb"><asp:Literal ID="litError" runat="server" /></p>
                </asp:PlaceHolder>

                <div class="gc-form">
                    <div class="gc-field">
                        <label for="<%= txtCorreo.ClientID %>">Usuario o correo electrónico</label>
                        <asp:TextBox ID="txtCorreo" runat="server" CssClass="gc-input"
                            placeholder="usuario o tu@correo.hn" />
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtClave.ClientID %>">Contraseña</label>
                        <asp:TextBox ID="txtClave" runat="server" CssClass="gc-input" TextMode="Password" />
                        <span class="gc-hint">Mínimo ocho caracteres.</span>
                    </div>

                    <div style="display: flex; align-items: center; justify-content: space-between; gap: 12px;">
                        <label style="display: flex; align-items: center; gap: 8px; font-size: .88rem; font-weight: 500;">
                            <asp:CheckBox ID="chkRecordar" runat="server" />
                            Mantener la sesión abierta
                        </label>
                        <a class="gc-small" href="<%= ResolveUrl("~/Acceso") %>">Olvidé mi contraseña</a>
                    </div>

                    <asp:Button ID="btnEntrar" runat="server" CssClass="gc-btn gc-btn--lg gc-btn--block"
                        Text="Entrar" OnClick="btnEntrar_Click" />

                    <div class="gc-sep">o</div>

                    <a class="gc-btn gc-btn--ghost gc-btn--block" href="<%= ResolveUrl("~/Registro") %>">
                        Crear una cuenta ciudadana
                    </a>

                    <a class="gc-btn gc-btn--quiet gc-btn--block" href="<%= ResolveUrl("~/Registro?tipo=candidato") %>">
                        Registrarme como candidato
                    </a>
                </div>

                <div class="gc-note" style="margin-top: 22px;">
                    <span>
                        El acceso se valida contra la base de datos a través del Web Service del backend.
                        Si el servidor no está en marcha, el ingreso no va a funcionar.
                    </span>
                </div>

                <p class="gc-muted gc-small" style="margin-top: 20px;">
                    No necesitás cuenta para consultar la plataforma.
                    <a href="<%= ResolveUrl("~/Campanas") %>">Explorar sin registrarme</a>.
                </p>

            </div>
        </div>

    </div>

</asp:Content>
