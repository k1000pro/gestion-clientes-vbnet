<%@ Page Language="VB" AutoEventWireup="false" MasterPageFile="~/Site.Master"
         CodeBehind="Bitacora.aspx.vb" Inherits="GestionClientes.Web.PaginaBitacora" %>

<%@ Register TagPrefix="gc" TagName="Paginador" Src="~/Paginador.ascx" %>

<asp:Content ID="contenidoBitacora" ContentPlaceHolderID="Contenido" runat="server">

    <h1 class="h4 mb-1 marca">Bitácora de acciones</h1>
    <p class="text-muted mb-3">
        Registro de auditoría de todos los cambios realizados sobre los clientes. Es de solo lectura.
    </p>

    <div class="card shadow-sm mb-4">
        <div class="card-body">

            <div class="row g-2 align-items-end mb-3">

                <div class="col-sm-6 col-md-3">
                    <label class="form-label" for="<%= txtFechaDesde.ClientID %>">Desde</label>
                    <asp:TextBox ID="txtFechaDesde" runat="server" CssClass="form-control" TextMode="Date" />
                </div>

                <div class="col-sm-6 col-md-3">
                    <label class="form-label" for="<%= txtFechaHasta.ClientID %>">Hasta</label>
                    <asp:TextBox ID="txtFechaHasta" runat="server" CssClass="form-control" TextMode="Date" />
                </div>

                <div class="col-sm-6 col-md-2">
                    <label class="form-label" for="<%= ddlAccion.ClientID %>">Acción</label>
                    <asp:DropDownList ID="ddlAccion" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Todas"   Value="" />
                        <asp:ListItem Text="Agregar" Value="AGREGAR" />
                        <asp:ListItem Text="Editar"  Value="EDITAR" />
                        <asp:ListItem Text="Eliminar" Value="ELIMINAR" />
                    </asp:DropDownList>
                </div>

                <div class="col-sm-6 col-md-2">
                    <label class="form-label" for="<%= ddlUsuario.ClientID %>">Usuario</label>
                    <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-select" />
                </div>

                <div class="col-md-2 d-flex gap-2">
                    <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar"
                                CssClass="btn btn-primary" CausesValidation="False" />
                    <asp:Button ID="btnLimpiarFiltro" runat="server" Text="Limpiar"
                                CssClass="btn btn-link px-0" CausesValidation="False" />
                </div>

            </div>

            <asp:Panel ID="pnlAviso" runat="server" Visible="False" CssClass="alert alert-warning py-2">
                <asp:Literal ID="litAviso" runat="server" />
            </asp:Panel>

            <asp:GridView ID="gvBitacora" runat="server"
                          AutoGenerateColumns="False"
                          AllowSorting="True"
                          CssClass="table table-hover tabla-datos align-middle mb-0"
                          GridLines="None"
                          EmptyDataText="No hay registros que coincidan con los filtros."
                          UseAccessibleHeader="True">
                <HeaderStyle CssClass="table-light" />
                <Columns>
                    <asp:BoundField DataField="BitacoraId"    HeaderText="#" />
                    <asp:BoundField DataField="FechaHora"     HeaderText="Fecha y hora"
                                    DataFormatString="{0:dd/MM/yyyy HH:mm:ss}" SortExpression="FechaHora" />
                    <asp:BoundField DataField="Accion"        HeaderText="Acción" SortExpression="Accion" />
                    <asp:BoundField DataField="ClienteId"     HeaderText="Cliente" />
                    <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" SortExpression="NombreUsuario" />
                    <asp:BoundField DataField="Detalle"       HeaderText="Detalle"
                                    ItemStyle-CssClass="detalle-bitacora" />
                </Columns>
            </asp:GridView>

            <gc:Paginador ID="pgBitacora" runat="server" />

        </div>
    </div>

</asp:Content>
