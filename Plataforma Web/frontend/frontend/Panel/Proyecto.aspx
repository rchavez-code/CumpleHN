<%@ Page Title="Proyecto" Language="C#" MasterPageFile="~/Panel/Panel.Master" AutoEventWireup="true" CodeBehind="Proyecto.aspx.cs" Inherits="frontend.Panel.Proyecto" %>

<asp:Content ContentPlaceHolderID="TopActions" runat="server">
    <a class="gc-btn gc-btn--ghost gc-btn--sm" href="<%= ResolveUrl("~/Panel/Proyectos") %>">Volver a mis proyectos</a>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2><%: Encabezado %></h2>
            <p class="gc-muted" style="margin: 0;">
                Un proyecto bien documentado responde tres preguntas: qué problema resuelve, qué se propone
                lograr y a quién beneficia.
            </p>
        </div>
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

                        <%-- ==================================== Identificación --%>

                        <fieldset class="gc-fieldset">
                            <legend>Identificación</legend>

                            <div class="gc-field">
                                <label for="<%= txtNombre.ClientID %>">Nombre del proyecto</label>
                                <asp:TextBox ID="txtNombre" runat="server" CssClass="gc-input" MaxLength="160" />
                                <span class="gc-hint">Concreto y verificable. Evitá consignas de campaña.</span>
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtDescripcion.ClientID %>">Descripción</label>
                                <asp:TextBox ID="txtDescripcion" runat="server" CssClass="gc-textarea" TextMode="MultiLine" />
                                <span class="gc-hint">En qué consiste el proyecto y cómo se ejecutaría.</span>
                            </div>
                        </fieldset>

                        <%-- ========================================= Sustento --%>

                        <fieldset class="gc-fieldset">
                            <legend>Sustento</legend>

                            <div class="gc-field">
                                <label for="<%= txtProblema.ClientID %>">Problema que busca solucionar</label>
                                <asp:TextBox ID="txtProblema" runat="server" CssClass="gc-textarea" TextMode="MultiLine" />
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtObjetivo.ClientID %>">Objetivo</label>
                                <asp:TextBox ID="txtObjetivo" runat="server" CssClass="gc-textarea" TextMode="MultiLine" />
                                <span class="gc-hint">
                                    Un objetivo con cifra y plazo se puede verificar después. Uno sin cifra, no.
                                </span>
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtBeneficiarios.ClientID %>">Beneficiarios</label>
                                <asp:TextBox ID="txtBeneficiarios" runat="server" CssClass="gc-input" />
                                <span class="gc-hint">A quién beneficia y, si se puede, en qué magnitud.</span>
                            </div>
                        </fieldset>

                        <%-- ===================================== Clasificación --%>

                        <fieldset class="gc-fieldset">
                            <legend>Clasificación</legend>

                            <div class="gc-row2">
                                <div class="gc-field">
                                    <label for="<%= ddlCategoria.ClientID %>">Área o categoría</label>
                                    <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="gc-select" />
                                    <span class="gc-hint">
                                        La categoría es lo que permite comparar tu propuesta con las de otras candidaturas.
                                    </span>
                                </div>
                                <div class="gc-field">
                                    <label for="<%= ddlEstado.ClientID %>">Estado declarado</label>
                                    <asp:DropDownList ID="ddlEstado" runat="server" CssClass="gc-select" />
                                    <span class="gc-hint">
                                        Los estados de cumplimiento los asigna la plataforma con evidencia.
                                    </span>
                                </div>
                            </div>

                            <div class="gc-row2">
                                <div class="gc-field">
                                    <label for="<%= txtUbicacion.ClientID %>">Ubicación</label>
                                    <asp:TextBox ID="txtUbicacion" runat="server" CssClass="gc-input"
                                        placeholder="Dejalo vacío si es de cobertura nacional" />
                                </div>
                                <div class="gc-field">
                                    <label for="<%= txtPeriodo.ClientID %>">Período estimado de ejecución</label>
                                    <asp:TextBox ID="txtPeriodo" runat="server" CssClass="gc-input"
                                        placeholder="Por ejemplo: primer año de gestión" />
                                </div>
                            </div>
                        </fieldset>

                        <%-- ======================================== Material --%>

                        <fieldset class="gc-fieldset">
                            <legend>Material y detalle</legend>

                            <div class="gc-field">
                                <label>Imagen o material de apoyo</label>
                                <div class="gc-drop">
                                    <span class="gc-quick__ico" aria-hidden="true">IMG</span>
                                    <div style="flex: 1 1 auto; min-width: 0;">
                                        <asp:FileUpload ID="fuImagen" runat="server" CssClass="gc-input" />
                                        <span class="gc-hint">JPG o PNG, hasta 2 MB. Se muestra en la tarjeta del proyecto.</span>
                                    </div>
                                </div>
                            </div>

                            <div class="gc-field">
                                <label for="<%= txtAdicional.ClientID %>">Información adicional</label>
                                <asp:TextBox ID="txtAdicional" runat="server" CssClass="gc-textarea" TextMode="MultiLine" />
                                <span class="gc-hint">
                                    Costo estimado, fuente de financiamiento, aliados o cualquier dato que sostenga la propuesta.
                                </span>
                            </div>
                        </fieldset>

                        <div class="gc-formfoot">
                            <a class="gc-btn gc-btn--quiet" href="<%= ResolveUrl("~/Panel/Proyectos") %>">Cancelar</a>
                            <asp:Button ID="btnGuardar" runat="server" CssClass="gc-btn"
                                Text="Guardar proyecto" OnClick="btnGuardar_Click" />
                        </div>

                    </div>
                </div>
            </div>
        </div>

        <%-- ================================================ Riel derecho --%>

        <div class="col-lg-4">
            <div class="gc-card gc-mb">
                <div class="gc-card__head">
                    <h3>Cómo se verá</h3>
                </div>
                <div class="gc-card__body">
                    <p class="gc-muted gc-small" style="margin: 0 0 12px;">
                        Así aparece tu proyecto en el perfil público y en los listados de la plataforma.
                    </p>

                    <div class="gc-cand__tags">
                        <span class="<%= CategoriaClase %>"><%: CategoriaVista %></span>
                        <span class="<%= EstadoClase %>"><%: EstadoVista %></span>
                    </div>

                    <h3 style="margin: 12px 0 4px; font-size: 1rem;"><%: NombreVista %></h3>
                    <p class="gc-muted gc-small" style="margin: 0;"><%: DescripcionVista %></p>
                </div>
            </div>

            <div class="gc-note gc-note--ambar">
                <span>
                    En esta etapa el formulario valida los datos pero todavía no los persiste. El guardado se
                    conecta al Web Service de propuestas del backend.
                </span>
            </div>
        </div>
    </div>

</asp:Content>
