<%@ Page Title="Confirmar cuenta" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Confirmar.aspx.cs" Inherits="frontend.Confirmar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container" style="max-width: 640px; padding-top: 54px; padding-bottom: 72px;">

        <div class="gc-auth__box">

            <p class="gc-eyebrow">Cuenta ciudadana</p>

            <h1 style="font-size: 1.6rem;"><asp:Literal ID="litTitulo" runat="server" /></h1>

            <p class="gc-muted" style="margin-bottom: 24px;">
                <asp:Literal ID="litMensaje" runat="server" />
            </p>

            <%-- El destino cambia según cómo haya salido: quien confirmó sigue
                 a lo suyo, quien no puede pedir otro enlace. --%>

            <asp:PlaceHolder ID="phSeguir" runat="server" Visible="false">
                <a class="gc-btn gc-btn--lg gc-btn--block" href="<%= ResolveUrl("~/") %>">
                    Ir a la plataforma
                </a>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phEntrar" runat="server" Visible="false">
                <a class="gc-btn gc-btn--lg gc-btn--block" href="<%= ResolveUrl("~/Acceso") %>">
                    Acceder
                </a>
                <p class="gc-muted gc-small" style="margin: 14px 0 0;">
                    Al entrar vas a poder pedir un enlace nuevo desde el aviso de tu cuenta.
                </p>
            </asp:PlaceHolder>

        </div>

    </div>

</asp:Content>
