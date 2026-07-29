Imports GestionClientes.Entidades
Imports GestionClientes.Negocio

''' <summary>Mantenimiento de clientes: consulta, alta, edición y borrado.</summary>
Public Class PaginaClientes
    Inherits PaginaBase

    Private ReadOnly _clientes As New ServicioCliente()

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            CargarClientes()
        End If
    End Sub

    ''' <summary>Carga la rejilla aplicando el texto de búsqueda actual.</summary>
    Private Sub CargarClientes()
        gvClientes.DataSource = _clientes.Listar(txtBusqueda.Text)
        gvClientes.DataBind()
    End Sub

    Protected Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        gvClientes.PageIndex = 0
        CargarClientes()
    End Sub

    Protected Sub btnLimpiarBusqueda_Click(sender As Object, e As EventArgs) Handles btnLimpiarBusqueda.Click
        txtBusqueda.Text = String.Empty
        gvClientes.PageIndex = 0
        CargarClientes()
    End Sub

    Protected Sub gvClientes_PageIndexChanging(sender As Object, e As GridViewPageEventArgs) Handles gvClientes.PageIndexChanging
        gvClientes.PageIndex = e.NewPageIndex
        CargarClientes()
    End Sub

    Protected Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        LimpiarFormulario()
        litTituloFormulario.Text = "Nuevo cliente"
        pnlFormulario.Visible = True
        OcultarMensaje()
    End Sub

    Protected Sub btnCancelar_Click(sender As Object, e As EventArgs) Handles btnCancelar.Click
        LimpiarFormulario()
        pnlFormulario.Visible = False
        OcultarMensaje()
    End Sub

    ''' <summary>Despacha los comandos de las filas de la rejilla.</summary>
    Protected Sub gvClientes_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvClientes.RowCommand
        Dim clienteId As Integer

        If Not Integer.TryParse(Convert.ToString(e.CommandArgument), clienteId) Then Return

        Select Case e.CommandName
            Case "EditarCliente"
                CargarParaEdicion(clienteId)
            Case "EliminarCliente"
                EliminarCliente(clienteId)
        End Select
    End Sub

    Private Sub CargarParaEdicion(clienteId As Integer)
        Dim cliente = _clientes.ObtenerPorId(clienteId)

        If cliente Is Nothing Then
            MostrarMensaje("El cliente ya no existe. Se actualizó el listado.", False)
            pnlFormulario.Visible = False
            CargarClientes()
            Return
        End If

        hdnClienteId.Value = cliente.ClienteId.ToString()
        txtNombres.Text = cliente.Nombres
        txtApellidos.Text = cliente.Apellidos
        txtDocumento.Text = cliente.Documento
        txtEmail.Text = cliente.Email
        txtTelefono.Text = cliente.Telefono
        txtDireccion.Text = cliente.Direccion

        litTituloFormulario.Text = "Editar cliente"
        pnlFormulario.Visible = True
        OcultarMensaje()
    End Sub

    Private Sub EliminarCliente(clienteId As Integer)
        Dim resultado = _clientes.Eliminar(clienteId, UsuarioIdActual, NombreUsuarioActual)

        MostrarMensaje(resultado.PrimerMensaje, resultado.Exitoso)
        pnlFormulario.Visible = False
        LimpiarFormulario()
        CargarClientes()
    End Sub

    Protected Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        ' La validación del navegador ya se ejecutó, pero se vuelve a comprobar en el servidor:
        ' un cliente HTTP puede enviar el formulario sin ejecutar el script de validación.
        If Not Page.IsValid Then
            pnlFormulario.Visible = True
            Return
        End If

        Dim cliente As New Cliente With {
            .ClienteId = ObtenerClienteIdDelFormulario(),
            .Nombres = txtNombres.Text.Trim(),
            .Apellidos = txtApellidos.Text.Trim(),
            .Documento = txtDocumento.Text.Trim(),
            .Email = txtEmail.Text.Trim(),
            .Telefono = txtTelefono.Text.Trim(),
            .Direccion = txtDireccion.Text.Trim()
        }

        Dim resultado = _clientes.Guardar(cliente, UsuarioIdActual, NombreUsuarioActual)

        If Not resultado.Exitoso Then
            MostrarMensaje(String.Join(" ", resultado.Mensajes), False)
            pnlFormulario.Visible = True
            Return
        End If

        MostrarMensaje(resultado.PrimerMensaje, True)
        LimpiarFormulario()
        pnlFormulario.Visible = False
        CargarClientes()
    End Sub

    Private Function ObtenerClienteIdDelFormulario() As Integer
        Dim clienteId As Integer
        If Integer.TryParse(hdnClienteId.Value, clienteId) Then Return clienteId
        Return 0
    End Function

    Private Sub LimpiarFormulario()
        hdnClienteId.Value = "0"
        txtNombres.Text = String.Empty
        txtApellidos.Text = String.Empty
        txtDocumento.Text = String.Empty
        txtEmail.Text = String.Empty
        txtTelefono.Text = String.Empty
        txtDireccion.Text = String.Empty
    End Sub

    ''' <summary>Muestra un aviso. El texto se codifica porque puede contener datos del usuario.</summary>
    Private Sub MostrarMensaje(texto As String, exitoso As Boolean)
        If String.IsNullOrWhiteSpace(texto) Then
            OcultarMensaje()
            Return
        End If

        litMensaje.Text = HttpUtility.HtmlEncode(texto)
        pnlMensaje.CssClass = If(exitoso, "alert alert-success py-2", "alert alert-danger py-2")
        pnlMensaje.Visible = True
    End Sub

    Private Sub OcultarMensaje()
        pnlMensaje.Visible = False
        litMensaje.Text = String.Empty
    End Sub

End Class
