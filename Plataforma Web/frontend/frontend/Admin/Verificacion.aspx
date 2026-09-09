<%@ Page Title="Verificación" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Verificacion.aspx.cs" Inherits="frontend.Admin.Verificacion" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Verificación de contenido</h2>
            <p class="gc-muted" style="margin: 0;">
                Candidaturas, propuestas y publicaciones en una sola cola, de lo más antiguo sin revisar
                a lo más reciente.
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
                <h3>Revisar contenido</h3>
            </div>
            <div class="gc-card__body">

                <dl class="gc-datalist gc-mb">
                    <div>
                        <dt>Contenido</dt>
                        <dd>
                            <strong><%: SeleccionTitulo %></strong>
                            <div class="gc-muted gc-small"><%: SeleccionResumen %></div>
                        </dd>
                    </div>
                    <div>
                        <dt>Tipo</dt>
                        <dd><%: SeleccionTipo %></dd>
                    </div>
                    <div>
                        <dt>Declarado por</dt>
                        <dd><%: SeleccionCandidato %></dd>
                    </div>
                    <div>
                        <dt>Nivel actual</dt>
                        <dd><span class="<%= SeleccionVerificacionClase %>"><%: SeleccionVerificacion %></span></dd>
                    </div>
                </dl>

                <div class="gc-form">
                    <div class="gc-field">
                        <label for="<%= ddlNivel.ClientID %>">Nuevo nivel</label>
                        <asp:DropDownList ID="ddlNivel" runat="server" CssClass="gc-select" />
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtMotivo.ClientID %>">Fuente o motivo</label>
                        <asp:TextBox ID="txtMotivo" runat="server" CssClass="gc-input" TextMode="MultiLine" Rows="3"
                                     placeholder="Documento, medio o registro que respalda la decisión." />
                        <span class="gc-muted gc-small">
                            Obligatorio para marcar como verificado. Queda en la bitácora junto a tu nombre.
                        </span>
                    </div>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="gc-btn" Text="Guardar decisión"
                                OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="gc-btn gc-btn--quiet" Text="Cancelar"
                                OnClick="btnCancelar_Click" CausesValidation="false" />
                    <a class="gc-btn gc-btn--ghost" target="_blank" href="<%= SeleccionUrl %>">Ver el contenido</a>
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- ================================================== Filtros --%>

    <div class="gc-card gc-filters">
        <asp:DropDownList ID="ddlTipo" runat="server" CssClass="gc-select" />
        <asp:DropDownList ID="ddlPendientes" runat="server" CssClass="gc-select" />
        <asp:Button ID="btnFiltrar" runat="server" CssClass="gc-btn" Text="Filtrar" OnClick="btnFiltrar_Click" />
    </div>

    <%-- =================================================== Lista --%>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Contenido</th>
                            <th scope="col">Tipo</th>
                            <th scope="col">Declarado por</th>
                            <th scope="col">Registrado</th>
                            <th scope="col">Nivel</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptBandeja" runat="server" OnItemCommand="rptBandeja_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 260px;">
                                        <strong><%#: Eval("Titulo") %></strong>
                                        <div class="gc-muted gc-small"><%#: Eval("Resumen") %></div>
                                    </td>
                                    <td class="gc-nowrap"><%#: Eval("TipoTexto") %></td>
                                    <td><%#: Eval("Candidato") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("FechaTexto") %></td>
                                    <td class="gc-nowrap">
                                        <span class="<%#: Eval("VerificacionClase") %>"><%#: Eval("Verificacion") %></span>
                                    </td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <asp:LinkButton runat="server" CssClass="gc-small" Text="Revisar"
                                                        CommandName="revisar"
                                                        CommandArgument='<%# Eval("TipoObjeto") + "|" + Eval("CodigoObjeto") %>' />
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
            <h3><%: TituloVacio %></h3>
            <p><%: TextoVacio %></p>
        </div>
    </asp:PlaceHolder>

    <div class="gc-note" style="margin-top: 18px;">
        <span>
            El nivel de verificación no lo asigna la candidatura. Todo lo que publica queda identificado
            como declarado por ella hasta que exista una fuente que lo respalde, y esa fuente se registra
            acá. Es lo que sostiene la neutralidad de la plataforma.
        </span>
    </div>

</asp:Content>
