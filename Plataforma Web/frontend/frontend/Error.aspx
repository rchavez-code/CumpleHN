<%@ Page Title="Algo salió mal" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="frontend.ErrorPagina" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container" style="padding: 60px 0;">
        <div class="gc-card" style="max-width: 640px; margin: 0 auto;">
            <div class="gc-card__body" style="padding: 34px;">

                <p class="gc-eyebrow">CumpleHN</p>
                <h1 style="font-size: 1.7rem;"><%: Titulo %></h1>
                <p class="gc-lead"><%: Mensaje %></p>

                <asp:PlaceHolder ID="phTexto" runat="server" Visible="false">
                    <div class="gc-note gc-note--ambar">
                        <span>
                            Los comentarios se guardan como texto. Si escribiste signos como
                            <strong>&lt;</strong> pegados a una palabra, el sistema lo interpreta como un
                            intento de insertar código y lo rechaza por seguridad. Separá el signo con un
                            espacio y volvé a intentarlo.
                        </span>
                    </div>
                </asp:PlaceHolder>

                <div class="gc-hero__actions">
                    <a class="gc-btn" href="<%= ResolveUrl("~/") %>">Volver al inicio</a>
                    <a class="gc-btn gc-btn--ghost" href="<%= ResolveUrl("~/Campanas") %>">Ver campañas</a>
                </div>

            </div>
        </div>
    </div>

</asp:Content>
