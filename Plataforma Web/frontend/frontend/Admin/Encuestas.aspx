<%@ Page Title="Encuestas" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Encuestas.aspx.cs" Inherits="frontend.Admin.EncuestasPagina" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Encuestas de percepción</h2>
            <p class="gc-muted" style="margin: 0;">
                La pregunta que aparece en la portada. Una sola encuesta abierta por campaña: con dos
                compitiendo, ninguna junta respuestas suficientes.
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
                        <label for="<%= ddlCampana.ClientID %>">Campaña</label>
                        <asp:DropDownList ID="ddlCampana" runat="server" CssClass="gc-input" />
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtPregunta.ClientID %>">Pregunta</label>
                        <asp:TextBox ID="txtPregunta" runat="server" CssClass="gc-input" MaxLength="300" />
                        <span class="gc-muted gc-small">
                            Es lo que se lee más grande en la tarjeta. Una sola pregunta, redactada de
                            modo que ninguna de las opciones quede sugerida en el enunciado.
                        </span>
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtDescripcion.ClientID %>">Descripción</label>
                        <asp:TextBox ID="txtDescripcion" runat="server" CssClass="gc-input"
                                     TextMode="MultiLine" Rows="3" MaxLength="600" />
                        <span class="gc-muted gc-small">
                            Opcional. Para qué sirve la respuesta y qué se hace con ella.
                        </span>
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtOpciones.ClientID %>">Opciones, una por línea</label>
                        <asp:TextBox ID="txtOpciones" runat="server" CssClass="gc-input"
                                     TextMode="MultiLine" Rows="8" />
                        <span class="gc-muted gc-small">
                            Entre dos y ocho. Conviene que cubran todas las respuestas posibles: sin una
                            salida del tipo «ninguna en particular», quien no priorice nada queda obligado
                            a elegir algo que no piensa.
                        </span>
                    </div>

                    <asp:PlaceHolder ID="phOpcionesBloqueadas" runat="server" Visible="false">
                        <div class="gc-field">
                            <p class="gc-alert" style="margin: 0;">
                                Esta encuesta ya tiene respuestas, así que las opciones no se pueden cambiar.
                                La pregunta, la descripción y las fechas sí. Cambiar las opciones ahora
                                dejaría respuestas apuntando a algo que nadie respondió.
                            </p>
                        </div>
                    </asp:PlaceHolder>

                    <div class="gc-field">
                        <label for="<%= txtInicio.ClientID %>">Abre el</label>
                        <asp:TextBox ID="txtInicio" runat="server" CssClass="gc-input" TextMode="Date" />
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtCierre.ClientID %>">Cierra el</label>
                        <asp:TextBox ID="txtCierre" runat="server" CssClass="gc-input" TextMode="Date" />
                        <span class="gc-muted gc-small">
                            Opcional. Sin fecha de cierre sigue abierta hasta que la cierres a mano.
                        </span>
                    </div>

                    <div class="gc-field">
                        <label for="<%= ddlCategoria.ClientID %>">Categoría temática</label>
                        <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="gc-input" />
                        <span class="gc-muted gc-small">
                            Opcional. Sirve para contrastar la respuesta con las propuestas registradas de
                            esa misma área.
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

    <%-- ================================================== Decisión --%>

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
                                 TextMode="MultiLine" Rows="2" MaxLength="300" />
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

    <%-- =================================================== Resultado --%>

    <asp:PlaceHolder ID="phResultado" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3>Resultado</h3>
                <span class="gc-muted gc-small"><%: ResumenResultado %></span>
            </div>
            <div class="gc-card__body">

                <div class="gc-tablewrap">
                    <table class="gc-table">
                        <thead>
                            <tr>
                                <th scope="col">Opción</th>
                                <th scope="col">Respuestas</th>
                                <th scope="col">Proporción</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptResultado" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td style="min-width: 220px;"><%#: Eval("Texto") %></td>
                                        <td class="gc-nowrap"><%#: Eval("Votos") %></td>
                                        <td class="gc-nowrap"><%# Proporcion(Container.DataItem) %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnCerrarResultado" runat="server" CssClass="gc-btn gc-btn--quiet"
                                Text="Cerrar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- ======================================================= Lista --%>

    <div class="gc-card gc-filters">
        <asp:Button ID="btnNueva" runat="server" CssClass="gc-btn" Text="Registrar encuesta"
                    OnClick="btnNueva_Click" CausesValidation="false" />
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Pregunta</th>
                            <th scope="col">Campaña</th>
                            <th scope="col">Vigencia</th>
                            <th scope="col">Respuestas</th>
                            <th scope="col">Estado</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptEncuestas" runat="server" OnItemCommand="rptEncuestas_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 260px;">
                                        <strong><%#: Eval("Pregunta") %></strong>
                                        <div class="gc-muted gc-small"><%#: Eval("Opciones") %> opciones</div>
                                    </td>
                                    <td class="gc-muted"><%#: Eval("Campana") %></td>
                                    <td class="gc-muted gc-nowrap gc-small">
                                        <%#: Eval("FechaInicioTexto") %>
                                        <div><%#: Eval("FechaCierreTexto") %></div>
                                    </td>
                                    <td class="gc-nowrap"><%#: Eval("Votos") %></td>
                                    <td class="gc-nowrap">
                                        <span class="<%# Eval("EstadoClase") %>"><%#: Eval("Estado") %></span>
                                    </td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <asp:LinkButton runat="server" CssClass="gc-small" Text="Editar"
                                                        CommandName="editar" CommandArgument='<%# Eval("Codigo") %>' />
                                        <span class="gc-faint" aria-hidden="true"> · </span>
                                        <asp:LinkButton runat="server" CssClass="gc-small" Text="Resultado"
                                                        CommandName="resultado" CommandArgument='<%# Eval("Codigo") %>' />
                                        <span class="gc-faint" aria-hidden="true"> · </span>
                                        <asp:LinkButton runat="server" CssClass="gc-small"
                                                        Text='<%# (bool)Eval("EstaAbierta") ? "Cerrar" : "Reabrir" %>'
                                                        CommandName="cierre" CommandArgument='<%# Eval("Codigo") %>'
                                                        Visible='<%# (bool)Eval("Activo") %>' />
                                        <span class="gc-faint" aria-hidden="true"> · </span>
                                        <asp:LinkButton runat="server" CssClass="gc-small"
                                                        Text='<%# (bool)Eval("Activo") ? "Retirar" : "Restaurar" %>'
                                                        CommandName="baja" CommandArgument='<%# Eval("Codigo") %>' />
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
            <h3>No hay encuestas registradas</h3>
            <p>
                La portada no muestra el bloque mientras no exista una encuesta abierta, así que hasta
                acá no hay nada que el público esté viendo.
            </p>
        </div>
    </asp:PlaceHolder>

</asp:Content>
