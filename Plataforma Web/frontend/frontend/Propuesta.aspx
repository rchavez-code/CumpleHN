<%@ Page Title="Propuesta" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Propuesta.aspx.cs" Inherits="frontend.PropuestaPagina" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="gc-chead">
        <div class="container">
            <nav class="gc-crumbs" aria-label="Ruta de navegación">
                <a href="<%= ResolveUrl("~/") %>">Inicio</a>
                <span aria-hidden="true">/</span>
                <a href="<%= ResolveUrl("~/Propuestas") %>">Propuestas</a>
                <span aria-hidden="true">/</span>
                <span><%: Item.Nombre %></span>
            </nav>

            <div class="gc-cand__tags gc-mb">
                <span class="<%= Item.CategoriaClase %>"><%: Item.Categoria %></span>
                <span class="<%= Item.EstadoClase %>"><%: Item.EstadoTexto %></span>
                <span class="<%= Item.VerificacionClase %>"><%: Item.VerificacionTexto %></span>
            </div>

            <h1><%: Item.Nombre %></h1>
            <p class="gc-lead" style="max-width: 70ch;"><%: Item.Descripcion %></p>

            <div class="gc-card__inter" style="border: 0; padding: 0;">
                <gc:Interaccion ID="interPropuesta" runat="server" />
            </div>

            <div style="height: 26px;"></div>
        </div>
    </section>

    <div class="container" style="padding-top: 26px;">
        <div class="gc-layout">

            <div>
                <div class="gc-pob gc-mb">
                    <div>
                        <div class="gc-pob__l">Problema que atiende</div>
                        <div><%: Item.Problema %></div>
                    </div>
                    <div>
                        <div class="gc-pob__l">Objetivo</div>
                        <div><%: Item.Objetivo %></div>
                    </div>
                    <div>
                        <div class="gc-pob__l">Beneficiarios</div>
                        <div><%: Item.Beneficiarios %></div>
                    </div>
                </div>

                <asp:PlaceHolder ID="phAdicional" runat="server">
                    <div class="gc-card gc-mb">
                        <div class="gc-card__head">
                            <h3>Información adicional</h3>
                        </div>
                        <div class="gc-card__body">
                            <p><%: Item.InformacionAdicional %></p>
                        </div>
                    </div>
                </asp:PlaceHolder>

                <div class="gc-card gc-mb">
                    <div class="gc-card__head">
                        <h3>Seguimiento del cumplimiento</h3>
                    </div>
                    <div class="gc-card__body">
                        <p class="gc-muted" style="margin-bottom: 14px;">
                            Esta propuesta está registrada como <strong><%: Item.EstadoTexto %></strong>.
                            El estado avanza únicamente cuando existe evidencia documentada que respalde el
                            cambio, y cada actualización queda registrada con su fuente y su fecha.
                        </p>

                        <div class="gc-cand__tags">
                            <span class="gc-chip gc-chip--declarada">Declarada</span>
                            <span class="gc-chip gc-chip--proceso">En proceso</span>
                            <span class="gc-chip gc-chip--estancada">Estancada</span>
                            <span class="gc-chip gc-chip--proceso">Cumplida a medias</span>
                            <span class="gc-chip gc-chip--cumplida">Cumplida</span>
                            <span class="gc-chip gc-chip--incumplida">Incumplida</span>
                        </div>
                    </div>
                    <div class="gc-card__foot">
                        <span class="gc-muted gc-small">
                            Todavía no hay evidencias asociadas a esta propuesta.
                        </span>
                    </div>
                </div>

                <%-- ==================================== Opiniones --%>

                <div class="gc-card gc-mb">
                    <div class="gc-card__head">
                        <h3>Opiniones de la ciudadanía</h3>
                    </div>
                    <div class="gc-card__body">
                        <gc:Comentarios ID="comentsPropuesta" runat="server" />
                    </div>
                </div>

                <p class="gc-neutral">
                    Las valoraciones y los comentarios expresan la opinión de quien los publica.
                    No son verificados por CumpleHN ni intervienen en el estado de cumplimiento
                    de la propuesta, que se asigna únicamente con evidencia documentada.
                </p>
            </div>

            <aside class="gc-rail">
                <asp:PlaceHolder ID="phAutor" runat="server">
                    <div class="gc-card gc-mb">
                        <div class="gc-card__head">
                            <h3>Candidatura</h3>
                        </div>
                        <div class="gc-card__body">
                            <a href="<%= UrlCandidato %>" style="display: flex; align-items: center; gap: 12px; color: inherit;">
                                <span class="gc-avatar" aria-hidden="true"><%: InicialesAutor %></span>
                                <span>
                                    <strong style="display: block; font-size: .93rem;"><%: NombreAutor %></strong>
                                    <span class="gc-muted gc-small"><%: CargoAutor %></span>
                                </span>
                            </a>
                        </div>
                        <div class="gc-card__foot">
                            <a class="gc-small" href="<%= UrlCandidato %>">Ver perfil completo &rarr;</a>
                        </div>
                    </div>
                </asp:PlaceHolder>

                <div class="gc-card gc-mb">
                    <div class="gc-card__head">
                        <h3>Ficha</h3>
                    </div>
                    <div class="gc-card__body">
                        <dl class="gc-datalist">
                            <div>
                                <dt>Categoría</dt>
                                <dd><%: Item.Categoria %></dd>
                            </div>
                            <div>
                                <dt>Ubicación</dt>
                                <dd><%: Item.UbicacionTexto %></dd>
                            </div>
                            <div>
                                <dt>Período</dt>
                                <dd><%: Item.PeriodoEjecucion %></dd>
                            </div>
                            <div>
                                <dt>Estado</dt>
                                <dd><%: Item.EstadoTexto %></dd>
                            </div>
                            <div>
                                <dt>Registrada</dt>
                                <dd><%: FechaRegistro %></dd>
                            </div>
                        </dl>
                    </div>
                </div>

                <div class="gc-note gc-note--ambar">
                    <span>
                        Esta propuesta fue declarada por la candidatura. CumpleHN todavía no ha asociado
                        fuentes que la respalden, por lo que no debe leerse como un hecho verificado.
                    </span>
                </div>
            </aside>

        </div>
    </div>

</asp:Content>
