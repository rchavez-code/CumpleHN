<%@ Page Title="Módulos" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Modulos.aspx.cs" Inherits="frontend.Admin.ModulosPagina" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Módulos de la plataforma</h2>
            <p class="gc-muted" style="margin: 0;">
                Qué ve quien consulta el sitio. Lo que ocultes acá desaparece del menú y de la dirección
                directa, y vos lo seguís viendo con un aviso.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
        <div class="<%= ClaseMensaje %>" role="status"><%: Mensaje %></div>
    </asp:PlaceHolder>

    <%-- ================================================ Decisión --%>

    <asp:PlaceHolder ID="phDecision" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3><%: TituloDecision %></h3>
            </div>
            <div class="gc-card__body">

                <p class="gc-muted" style="margin-top: 0;"><%: TextoDecision %></p>

                <asp:PlaceHolder ID="phMotivo" runat="server">
                    <div class="gc-field">
                        <label for="<%= txtMotivo.ClientID %>">Motivo</label>
                        <asp:TextBox ID="txtMotivo" runat="server" CssClass="gc-input"
                                     TextMode="MultiLine" Rows="2" />
                        <span class="gc-muted gc-small">
                            Obligatorio para ocultar. Queda en la bitácora junto a tu nombre.
                        </span>
                    </div>
                </asp:PlaceHolder>

                <div class="gc-formfoot">
                    <asp:Button ID="btnConfirmar" runat="server" CssClass="gc-btn" Text="Confirmar"
                                OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="gc-btn gc-btn--quiet" Text="Cancelar"
                                OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- =================================================== Lista --%>

    <asp:Repeater ID="rptGrupos" runat="server" OnItemDataBound="rptGrupos_ItemDataBound">
        <ItemTemplate>

            <div class="gc-card gc-mb">
                <div class="gc-card__head">
                    <h3><%#: Eval("Nombre") %></h3>
                </div>
                <div class="gc-tablewrap">
                    <table class="gc-table">
                        <thead>
                            <tr>
                                <th scope="col">Elemento</th>
                                <th scope="col">Estado</th>
                                <th scope="col">Último cambio</th>
                                <th scope="col"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptModulos" runat="server" OnItemCommand="rptModulos_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td style="min-width: 260px;">
                                            <strong><%#: Eval("Nombre") %></strong>
                                            <div class="gc-muted gc-small"><%#: Eval("Descripcion") %></div>
                                        </td>
                                        <td class="gc-nowrap">
                                            <span class="<%# Eval("EstadoClase") %>"><%#: Eval("EstadoTexto") %></span>
                                        </td>
                                        <td class="gc-muted gc-small gc-nowrap"><%#: Eval("UltimoCambio") %></td>
                                        <td class="gc-nowrap" style="text-align: right;">
                                            <asp:LinkButton runat="server" CssClass="gc-small"
                                                            Text='<%# (bool)Eval("Habilitado") ? "Ocultar" : "Mostrar" %>'
                                                            CommandName="cambiar"
                                                            CommandArgument='<%# Eval("Clave") %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </div>

        </ItemTemplate>
    </asp:Repeater>

    <div class="gc-note">
        <span>
            El tablero de analítica es lo que sostiene el propósito de la plataforma: que cada persona
            decida con evidencia organizada. Ocultarlo tiene sentido para preparar algo antes de
            publicarlo, no como estado permanente.
        </span>
    </div>

</asp:Content>
