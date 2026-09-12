<%@ Page Title="Espacios" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Espacios.aspx.cs" Inherits="frontend.Admin.EspaciosPagina" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Espacios de clientes</h2>
            <p class="gc-muted" style="margin: 0;">
                Cada espacio es una organización que usa la plataforma para su propio proceso electoral,
                con sus campañas, candidaturas y participación aparte del sitio público. No se borran:
                se retiran, y su contenido queda fuera de toda consulta sin desaparecer de la base.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
        <div class="<%= ClaseMensaje %>" role="status"><%: Mensaje %></div>
    </asp:PlaceHolder>

    <%-- ================================================ Formulario --%>

    <asp:PlaceHolder ID="phForm" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3><%: TituloForm %></h3>
            </div>
            <div class="gc-card__body">

                <div class="gc-form">
                    <div class="gc-field">
                        <label for="<%= txtNombre.ClientID %>">Nombre del espacio</label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="gc-input" MaxLength="160" />
                        <span class="gc-muted gc-small">
                            Es lo que se ve en la barra del sitio del espacio. Su dirección pública se deriva
                            del nombre al registrarlo y después no cambia aunque el nombre se corrija.
                        </span>
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtOrganizacion.ClientID %>">Organización</label>
                        <asp:TextBox ID="txtOrganizacion" runat="server" CssClass="gc-input" MaxLength="200" />
                        <span class="gc-muted gc-small">
                            Quién administra el espacio. El sitio lo declara al pie: la plataforma es la
                            herramienta, y el contenido responde a la organización.
                        </span>
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtDescripcion.ClientID %>">Descripción</label>
                        <asp:TextBox ID="txtDescripcion" runat="server" CssClass="gc-input"
                                     TextMode="MultiLine" Rows="3" MaxLength="1000" />
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtTermino.ClientID %>">Cómo llama a las agrupaciones de candidaturas</label>
                        <asp:TextBox ID="txtTermino" runat="server" CssClass="gc-input" MaxLength="40" />
                        <span class="gc-muted gc-small">
                            «Partido» en una elección pública, «Planilla» o «Lista» en una interna. Cambia
                            los rótulos del sitio del espacio, no el modelo.
                        </span>
                    </div>

                    <div class="gc-field">
                        <asp:CheckBox ID="chkPadron" runat="server" Text=" Padrón cerrado: solo participa quien esté en la lista de miembros del espacio" />
                        <span class="gc-muted gc-small">
                            Con el padrón abierto participa cualquier cuenta con correo confirmado, como en el
                            sitio público. En una elección interna casi siempre va cerrado.
                        </span>
                    </div>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="gc-btn" Text="Guardar"
                                OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="gc-btn gc-btn--quiet" Text="Cancelar"
                                OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- ============================================== Baja / alta --%>

    <asp:PlaceHolder ID="phEstado" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3><%: TituloEstado %></h3>
            </div>
            <div class="gc-card__body">

                <p class="gc-muted" style="margin-top: 0;"><%: TextoEstado %></p>

                <div class="gc-field">
                    <label for="<%= txtMotivo.ClientID %>">Motivo</label>
                    <asp:TextBox ID="txtMotivo" runat="server" CssClass="gc-input"
                                 TextMode="MultiLine" Rows="2" />
                    <span class="gc-muted gc-small">Queda en la bitácora junto a tu nombre.</span>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnConfirmarEstado" runat="server" CssClass="gc-btn" Text="Confirmar"
                                OnClick="btnConfirmarEstado_Click" />
                    <asp:Button ID="btnCancelarEstado" runat="server" CssClass="gc-btn gc-btn--quiet"
                                Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- ============================================ Cuenta de acceso --%>

    <asp:PlaceHolder ID="phCuenta" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3>Cuenta de administración para <%: NombreSeleccion %></h3>
            </div>
            <div class="gc-card__body">

                <p class="gc-muted" style="margin-top: 0;">
                    Con esta cuenta el cliente administra su espacio: campañas, candidaturas, encuestas,
                    verificación y moderación de lo suyo, y nada de lo demás. La primera que se crea queda
                    como propietaria. Nace confirmada, porque la contraseña se entrega en mano.
                </p>

                <div class="gc-form">
                    <div class="gc-field">
                        <label for="<%= txtLogin.ClientID %>">Usuario</label>
                        <asp:TextBox ID="txtLogin" runat="server" CssClass="gc-input" MaxLength="60" />
                    </div>
                    <div class="gc-field">
                        <label for="<%= txtNombreCuenta.ClientID %>">Nombre de quien administra</label>
                        <asp:TextBox ID="txtNombreCuenta" runat="server" CssClass="gc-input" MaxLength="160" />
                    </div>
                    <div class="gc-field">
                        <label for="<%= txtCorreo.ClientID %>">Correo</label>
                        <asp:TextBox ID="txtCorreo" runat="server" CssClass="gc-input" MaxLength="160" TextMode="Email" />
                    </div>
                    <div class="gc-field">
                        <label for="<%= txtClave.ClientID %>">Contraseña</label>
                        <asp:TextBox ID="txtClave" runat="server" CssClass="gc-input" MaxLength="200" TextMode="Password" />
                        <span class="gc-muted gc-small">
                            Al menos ocho caracteres. No se guarda ni se muestra en ningún lado: entregala
                            al cliente por un medio seguro.
                        </span>
                    </div>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnCrearCuenta" runat="server" CssClass="gc-btn" Text="Crear cuenta"
                                OnClick="btnCrearCuenta_Click" />
                    <asp:Button ID="btnCancelarCuenta" runat="server" CssClass="gc-btn gc-btn--quiet"
                                Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- =================================================== Lista --%>

    <div class="gc-card gc-filters">
        <asp:Button ID="btnNuevo" runat="server" CssClass="gc-btn" Text="Registrar espacio"
                    OnClick="btnNuevo_Click" CausesValidation="false" />
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Espacio</th>
                            <th scope="col">Organización</th>
                            <th scope="col">Campañas</th>
                            <th scope="col">Candidaturas</th>
                            <th scope="col">Cuenta</th>
                            <th scope="col">Estado</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptEspacios" runat="server" OnItemCommand="rptEspacios_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 200px;">
                                        <strong><%#: Eval("Nombre") %></strong>
                                        <div class="gc-muted gc-small"><%#: Eval("Slug") %></div>
                                    </td>
                                    <td><%#: Eval("Organizacion") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("Campanas") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("Candidaturas") %></td>
                                    <td class="gc-nowrap">
                                        <%# (bool)Eval("EsPlataforma")
                                                ? "<span class=\"gc-muted\">—</span>"
                                                : (string.IsNullOrEmpty((string)Eval("PropietarioLogin"))
                                                    ? "<span class=\"gc-chip gc-chip--proceso\">Sin cuenta</span>"
                                                    : "<code>" + HttpUtility.HtmlEncode((string)Eval("PropietarioLogin")) + "</code>") %>
                                    </td>
                                    <td class="gc-nowrap">
                                        <span class="<%# EstadoClase((string)Eval("Estado")) %>"><%#: Eval("Estado") %></span>
                                    </td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <asp:LinkButton runat="server" CssClass="gc-small" Text="Administrar"
                                                        CommandName="administrar" CommandArgument='<%# Eval("Codigo") %>'
                                                        Visible='<%# (bool)Eval("Activo") %>' />
                                        <asp:PlaceHolder runat="server" Visible='<%# !(bool)Eval("EsPlataforma") %>'>
                                            <span class="gc-faint" aria-hidden="true"> · </span>
                                            <asp:LinkButton runat="server" CssClass="gc-small" Text="Editar"
                                                            CommandName="editar" CommandArgument='<%# Eval("Codigo") %>' />
                                            <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("Activo") && string.IsNullOrEmpty((string)Eval("PropietarioLogin")) %>'>
                                                <span class="gc-faint" aria-hidden="true"> · </span>
                                                <asp:LinkButton runat="server" CssClass="gc-small" Text="Crear cuenta"
                                                                CommandName="cuenta" CommandArgument='<%# Eval("Codigo") %>' />
                                            </asp:PlaceHolder>
                                            <span class="gc-faint" aria-hidden="true"> · </span>
                                            <asp:LinkButton runat="server" CssClass="gc-small"
                                                            Text='<%# (bool)Eval("Activo") ? "Retirar" : "Restaurar" %>'
                                                            CommandName="estado" CommandArgument='<%# Eval("Codigo") %>' />
                                        </asp:PlaceHolder>
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
            <h3>No hay espacios registrados</h3>
            <p>Registrá el primer espacio para que una organización pueda usar la plataforma.</p>
        </div>
    </asp:PlaceHolder>

</asp:Content>
