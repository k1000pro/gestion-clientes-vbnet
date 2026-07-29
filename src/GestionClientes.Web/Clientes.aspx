<%@ Page Language="VB" AutoEventWireup="false" MasterPageFile="~/Site.Master"
         CodeBehind="Clientes.aspx.vb" Inherits="GestionClientes.Web.PaginaClientes" %>

<asp:Content ID="contenidoClientes" ContentPlaceHolderID="Contenido" runat="server">

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h1 class="h4 mb-0 marca">Clientes</h1>
        <asp:Button ID="btnNuevo" runat="server" Text="Nuevo cliente"
                    CssClass="btn btn-primary" CausesValidation="False" />
    </div>

    <asp:Panel ID="pnlMensaje" runat="server" Visible="False" role="alert">
        <asp:Literal ID="litMensaje" runat="server" />
    </asp:Panel>

    <div class="card shadow-sm mb-4">
        <div class="card-body">

            <div class="row g-2 mb-3">
                <div class="col-sm-8 col-md-6">
                    <asp:TextBox ID="txtBusqueda" runat="server" CssClass="form-control"
                                 MaxLength="100" placeholder="Buscar por nombre, apellido o documento" />
                </div>
                <div class="col-auto">
                    <asp:Button ID="btnBuscar" runat="server" Text="Buscar"
                                CssClass="btn btn-outline-secondary" CausesValidation="False" />
                </div>
                <div class="col-auto">
                    <asp:Button ID="btnLimpiarBusqueda" runat="server" Text="Limpiar"
                                CssClass="btn btn-link" CausesValidation="False" />
                </div>
            </div>

            <asp:GridView ID="gvClientes" runat="server"
                          AutoGenerateColumns="False"
                          DataKeyNames="ClienteId"
                          AllowPaging="True"
                          PageSize="10"
                          CssClass="table table-hover tabla-datos align-middle mb-0"
                          GridLines="None"
                          EmptyDataText="No hay clientes registrados."
                          UseAccessibleHeader="True">
                <HeaderStyle CssClass="table-light" />
                <PagerStyle CssClass="pt-3" />
                <Columns>
                    <asp:BoundField DataField="Documento"     HeaderText="Documento" />
                    <asp:BoundField DataField="Nombres"       HeaderText="Nombres" />
                    <asp:BoundField DataField="Apellidos"     HeaderText="Apellidos" />
                    <asp:BoundField DataField="Email"         HeaderText="Correo" />
                    <asp:BoundField DataField="Telefono"      HeaderText="Teléfono" />
                    <asp:BoundField DataField="FechaRegistro" HeaderText="Registro"
                                    DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:TemplateField HeaderText="Acciones" ItemStyle-CssClass="text-nowrap">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkEditar" runat="server"
                                            CommandName="EditarCliente"
                                            CommandArgument='<%# Eval("ClienteId") %>'
                                            CssClass="btn btn-sm btn-outline-primary"
                                            CausesValidation="False">Editar</asp:LinkButton>

                            <asp:LinkButton ID="lnkEliminar" runat="server"
                                            CommandName="EliminarCliente"
                                            CommandArgument='<%# Eval("ClienteId") %>'
                                            CssClass="btn btn-sm btn-outline-danger"
                                            CausesValidation="False"
                                            OnClientClick="return confirm('¿Confirma que desea eliminar este cliente? Esta acción no se puede deshacer.');">Eliminar</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

        </div>
    </div>

    <asp:Panel ID="pnlFormulario" runat="server" Visible="False" CssClass="card shadow-sm">
        <div class="card-body">

            <h2 class="h5 mb-3">
                <asp:Literal ID="litTituloFormulario" runat="server" />
            </h2>

            <asp:HiddenField ID="hdnClienteId" runat="server" Value="0" />

            <div class="row g-3">

                <div class="col-md-6">
                    <label class="form-label" for="<%= txtNombres.ClientID %>">Nombres *</label>
                    <asp:TextBox ID="txtNombres" runat="server" CssClass="form-control" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="valNombres" runat="server"
                                                ControlToValidate="txtNombres" ValidationGroup="Cliente"
                                                CssClass="text-danger small" Display="Dynamic"
                                                ErrorMessage="Los nombres son obligatorios." />
                </div>

                <div class="col-md-6">
                    <label class="form-label" for="<%= txtApellidos.ClientID %>">Apellidos *</label>
                    <asp:TextBox ID="txtApellidos" runat="server" CssClass="form-control" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="valApellidos" runat="server"
                                                ControlToValidate="txtApellidos" ValidationGroup="Cliente"
                                                CssClass="text-danger small" Display="Dynamic"
                                                ErrorMessage="Los apellidos son obligatorios." />
                </div>

                <div class="col-md-4">
                    <label class="form-label" for="<%= txtDocumento.ClientID %>">Documento *</label>
                    <asp:TextBox ID="txtDocumento" runat="server" CssClass="form-control"
                                 MaxLength="20" placeholder="00000000-0" />
                    <asp:RequiredFieldValidator ID="valDocumentoRequerido" runat="server"
                                                ControlToValidate="txtDocumento" ValidationGroup="Cliente"
                                                CssClass="text-danger small" Display="Dynamic"
                                                ErrorMessage="El documento es obligatorio." />
                    <asp:RegularExpressionValidator ID="valDocumentoFormato" runat="server"
                                                    ControlToValidate="txtDocumento" ValidationGroup="Cliente"
                                                    ValidationExpression="^\d{8}-\d$"
                                                    CssClass="text-danger small" Display="Dynamic"
                                                    ErrorMessage="El documento debe tener el formato 00000000-0." />
                </div>

                <div class="col-md-4">
                    <label class="form-label" for="<%= txtEmail.ClientID %>">Correo electrónico</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                                 TextMode="Email" MaxLength="150" />
                    <asp:RegularExpressionValidator ID="valEmailFormato" runat="server"
                                                    ControlToValidate="txtEmail" ValidationGroup="Cliente"
                                                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]{2,}$"
                                                    CssClass="text-danger small" Display="Dynamic"
                                                    ErrorMessage="El correo electrónico no tiene un formato válido." />
                </div>

                <div class="col-md-4">
                    <label class="form-label" for="<%= txtTelefono.ClientID %>">Teléfono</label>
                    <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control"
                                 MaxLength="20" placeholder="0000-0000" />
                    <asp:RegularExpressionValidator ID="valTelefonoFormato" runat="server"
                                                    ControlToValidate="txtTelefono" ValidationGroup="Cliente"
                                                    ValidationExpression="^\d{4}-\d{4}$"
                                                    CssClass="text-danger small" Display="Dynamic"
                                                    ErrorMessage="El teléfono debe tener el formato 0000-0000." />
                </div>

                <div class="col-12">
                    <label class="form-label" for="<%= txtDireccion.ClientID %>">Dirección</label>
                    <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control"
                                 TextMode="MultiLine" Rows="2" MaxLength="250" />
                </div>

            </div>

            <div class="mt-4">
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar"
                            CssClass="btn btn-primary" ValidationGroup="Cliente" />
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar"
                            CssClass="btn btn-outline-secondary" CausesValidation="False" />
            </div>

        </div>
    </asp:Panel>

</asp:Content>
