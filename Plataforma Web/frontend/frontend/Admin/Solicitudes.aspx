<%@ Page Title="Solicitudes" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Solicitudes.aspx.cs" Inherits="frontend.Admin.SolicitudesPagina" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Solicitudes de espacio</h2>
            <p class="gc-muted" style="margin: 0;">
                Lo que las organizaciones dejan en la página «Para organizaciones». De acá sale cada
                espacio nuevo: se responde a la organización, se recibe el pago y se crea el espacio a
                partir de la solicitud. Nada se borra: una solicitud descartada sigue diciendo quién pidió
                qué y cuándo.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
        <div class="<%= ClaseMensaje %>" role="status"><%: Mensaje %></div>
    </asp:PlaceHolder>

    <%-- ============================================== Descartar --%>

    <asp:PlaceHolder ID="phDescartar" runat="server" Visible="false">
        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3>Descartar la solicitud de <%: NombreSeleccion %></h3>
            </div>
            <div class="gc-card__body">
                <div class="gc-field">
                    <label for="<%= txtNotas.ClientID %>">Motivo</label>
                    <asp:TextBox ID="txtNotas" runat="server" CssClass="gc-input" TextMode="MultiLine" Rows="2" MaxLength="500" />
                    <span class="gc-muted gc-small">Queda en la solicitud y en la bitácora junto a tu nombre.</span>
                </div>
                <div class="gc-formfoot">
                    <asp:Button ID="btnDescartar" runat="server" CssClass="gc-btn" Text="Descartar" OnClick="btnDescartar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="gc-btn gc-btn--quiet" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
    </asp:PlaceHolder>

    <%-- ================================================= Filtro --%>

    <div class="gc-card gc-filters">
        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="gc-input gc-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstado_Changed">
            <asp:ListItem Text="Nuevas" Value="Nueva" Selected="True" />
            <asp:ListItem Text="Atendidas" Value="Atendida" />
            <asp:ListItem Text="Descartadas" Value="Descartada" />
            <asp:ListItem Text="Todas" Value="" />
        </asp:DropDownList>
    </div>

    <%-- ================================================== Lista --%>

    <asp:PlaceHolder ID="phLista" runat="server">
        <asp:Repeater ID="rptSolicitudes" runat="server" OnItemCommand="rptSolicitudes_ItemCommand">
            <ItemTemplate>
                <div class="gc-card gc-mb">
                    <div class="gc-card__body">
                        <div style="display: flex; justify-content: space-between; gap: 16px; flex-wrap: wrap; align-items: flex-start;">
                            <div style="min-width: 0;">
                                <h3 style="margin: 0 0 4px;"><%#: Eval("Organizacion") %></h3>
                                <div class="gc-muted gc-small">
                                    <%#: Eval("Contacto") %> · <a href="mailto:<%#: Eval("Correo") %>"><%#: Eval("Correo") %></a>
                                    <%# string.IsNullOrEmpty((string)Eval("Telefono")) ? "" : " · " + HttpUtility.HtmlEncode((string)Eval("Telefono")) %>
                                </div>
                            </div>
                            <div class="gc-nowrap" style="text-align: right;">
                                <span class="<%# Eval("EstadoClase") %>"><%#: Eval("Estado") %></span>
                                <div class="gc-muted gc-small" style="margin-top: 4px;">Recibida el <%#: ((DateTime)Eval("FechaRegistro")).ToString("d MMM yyyy, HH:mm") %></div>
                            </div>
                        </div>

                        <dl class="gc-datalist" style="margin-top: 14px;">
                            <div><dt>Qué eligen</dt><dd><%#: Eval("Proceso") %></dd></div>
                            <div><dt>Fecha aproximada</dt><dd><%#: Eval("FechaAproximadaTexto") %></dd></div>
                            <div><dt>Plan de interés</dt><dd><%#: string.IsNullOrEmpty((string)Eval("Plan")) ? "sin definir" : Eval("Plan") %></dd></div>
                            <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrEmpty((string)Eval("Mensaje")) %>'>
                                <div><dt>Mensaje</dt><dd><%#: Eval("Mensaje") %></dd></div>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder runat="server" Visible='<%# !(bool)Eval("Nueva") %>'>
                                <div><dt>Resolución</dt><dd>
                                    <%#: Eval("AtendidaPor") %> · <%#: ((DateTime)Eval("FechaAtencion")).ToString("d MMM yyyy") %>
                                    <%# string.IsNullOrEmpty((string)Eval("Espacio")) ? "" : " · espacio <strong>" + HttpUtility.HtmlEncode((string)Eval("Espacio")) + "</strong>" %>
                                    <%# string.IsNullOrEmpty((string)Eval("Notas")) ? "" : "<div class=\"gc-muted gc-small\">" + HttpUtility.HtmlEncode((string)Eval("Notas")) + "</div>" %>
                                </dd></div>
                            </asp:PlaceHolder>
                        </dl>

                        <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("Nueva") %>'>
                            <div class="gc-formfoot" style="margin-top: 10px;">
                                <asp:LinkButton runat="server" CssClass="gc-btn gc-btn--sm" Text="Crear el espacio"
                                                CommandName="crear" CommandArgument='<%# Eval("Codigo") %>' />
                                <asp:LinkButton runat="server" CssClass="gc-btn gc-btn--quiet gc-btn--sm" Text="Descartar"
                                                CommandName="descartar" CommandArgument='<%# Eval("Codigo") %>' />
                            </div>
                        </asp:PlaceHolder>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
        <div class="gc-card gc-empty">
            <h3>No hay solicitudes en este estado</h3>
            <p>Las nuevas llegan desde la página «Para organizaciones» del sitio público.</p>
        </div>
    </asp:PlaceHolder>

</asp:Content>
