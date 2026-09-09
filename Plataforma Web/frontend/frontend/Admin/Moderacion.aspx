<%@ Page Title="Moderación" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Moderacion.aspx.cs" Inherits="frontend.Admin.Moderacion" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Moderación de publicaciones</h2>
            <p class="gc-muted" style="margin: 0;">
                Retirar una publicación la saca de la consulta pública. No la borra: la fila se conserva
                con el motivo y el responsable, y se puede restaurar.
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

                <dl class="gc-datalist gc-mb">
                    <div>
                        <dt>Publicación</dt>
                        <dd><%: SeleccionTexto %></dd>
                    </div>
                    <div>
                        <dt>Autor</dt>
                        <dd><%: SeleccionCandidato %></dd>
                    </div>
                    <div>
                        <dt>Participación registrada</dt>
                        <dd><%: SeleccionParticipacion %></dd>
                    </div>
                </dl>

                <asp:PlaceHolder ID="phAvisoRetiro" runat="server" Visible="false">
                    <div class="gc-note gc-note--ambar gc-mb">
                        <span>
                            Al retirarla, sus valoraciones y sus comentarios dejan de contar en el tablero de
                            analítica. Las filas no se borran: vuelven a contar si la publicación se restaura.
                        </span>
                    </div>
                </asp:PlaceHolder>

                <div class="gc-field">
                    <label for="<%= txtMotivo.ClientID %>">Motivo de la decisión</label>
                    <asp:TextBox ID="txtMotivo" runat="server" CssClass="gc-input" TextMode="MultiLine" Rows="3"
                                 placeholder="Por qué se toma esta decisión sobre la publicación." />
                    <span class="gc-muted gc-small">
                        Obligatorio. Queda en la bitácora junto a tu nombre y la fecha.
                    </span>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnConfirmar" runat="server" CssClass="gc-btn" Text="Confirmar"
                                OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="gc-btn gc-btn--quiet" Text="Cancelar"
                                OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- ================================================== Filtros --%>

    <div class="gc-card gc-filters">
        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="gc-select" />
        <asp:Button ID="btnFiltrar" runat="server" CssClass="gc-btn" Text="Filtrar" OnClick="btnFiltrar_Click" />
    </div>

    <%-- =================================================== Lista --%>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Publicación</th>
                            <th scope="col">Autor</th>
                            <th scope="col">Fecha</th>
                            <th scope="col">Participación</th>
                            <th scope="col">Estado</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptPublicaciones" runat="server" OnItemCommand="rptPublicaciones_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 280px;">
                                        <%#: Eval("Extracto") %>
                                        <asp:PlaceHolder runat="server" Visible='<%# !(bool)Eval("Activa") %>'>
                                            <div class="gc-muted gc-small" style="margin-top: 6px;">
                                                Retirada por <%#: Eval("RetiradaPor") %>: <%#: Eval("MotivoBaja") %>
                                            </div>
                                        </asp:PlaceHolder>
                                    </td>
                                    <td><%#: Eval("Candidato") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("FechaTexto") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("Participacion") %></td>
                                    <td class="gc-nowrap">
                                        <span class="<%# EstadoClase(Eval("Activa")) %>"><%#: EstadoTexto(Eval("Activa")) %></span>
                                    </td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <asp:LinkButton runat="server" CssClass="gc-small"
                                                        Text='<%# (bool)Eval("Activa") ? "Retirar" : "Restaurar" %>'
                                                        CommandName="moderar"
                                                        CommandArgument='<%# Eval("CodigoPublicacion") %>' />
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
        <div class="gc-card gc-empty">
            <h3>No hay publicaciones que mostrar</h3>
            <p>Cambiá el filtro de estado para ver las publicaciones activas o las retiradas.</p>
        </div>
    </asp:PlaceHolder>

    <div class="gc-note" style="margin-top: 18px;">
        <span>
            La moderación no juzga la posición política de una publicación. Retirar contenido por su
            contenido político convertiría a la plataforma en parte de la disputa que documenta.
        </span>
    </div>

</asp:Content>
