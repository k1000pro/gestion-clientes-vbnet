<%@ Control Language="VB" AutoEventWireup="false" CodeBehind="Paginador.ascx.vb" Inherits="GestionClientes.Web.Paginador" %>

<asp:Panel ID="pnlPaginador" runat="server" Visible="False"
           CssClass="d-flex justify-content-between align-items-center flex-wrap gap-2 pt-3">

    <span class="text-muted small">
        <asp:Literal ID="litResumen" runat="server" />
    </span>

    <nav aria-label="Paginación">
        <ul class="pagination pagination-sm mb-0">

            <li class="page-item">
                <asp:LinkButton ID="lnkAnterior" runat="server" CssClass="page-link"
                                CommandName="Ir" CausesValidation="False">Anterior</asp:LinkButton>
            </li>

            <asp:Repeater ID="rptPaginas" runat="server">
                <ItemTemplate>
                    <li class="page-item">
                        <asp:LinkButton ID="lnkPagina" runat="server" CssClass="page-link"
                                        CommandName="Ir"
                                        CommandArgument='<%# Container.DataItem %>'
                                        Text='<%# Container.DataItem %>'
                                        CausesValidation="False" />
                    </li>
                </ItemTemplate>
            </asp:Repeater>

            <li class="page-item">
                <asp:LinkButton ID="lnkSiguiente" runat="server" CssClass="page-link"
                                CommandName="Ir" CausesValidation="False">Siguiente</asp:LinkButton>
            </li>

        </ul>
    </nav>

</asp:Panel>
