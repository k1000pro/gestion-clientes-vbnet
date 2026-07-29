Imports GestionClientes.Entidades
Imports GestionClientes.Negocio

''' <summary>Mantenimiento de clientes: consulta, alta, edición y borrado.</summary>
Public Class PaginaClientes
    Inherits PaginaBase

    Private ReadOnly _clientes As New ServicioCliente()

    ''' <summary>Tamaño de página del listado de clientes.</summary>
    Private Const TamanoPaginaClientes As Integer = 10

    ''' <summary>
    ''' Criterios actuales del listado. Viven en ViewState y no en campos de la clase porque una
    ''' instancia de página no sobrevive al postback.
    ''' </summary>
    Private Property PaginaActual As Integer
        Get
            Dim valor = ViewState("PaginaActual")
            If valor Is Nothing Then Return 1
            Return CInt(valor)
        End Get
        Set(value As Integer)
            ViewState("PaginaActual") = value
        End Set
    End Property

    Private Property OrdenActual As String
        Get
            Dim valor = TryCast(ViewState("OrdenActual"), String)
            If String.IsNullOrEmpty(valor) Then Return "Apellidos"
            Return valor
        End Get
        Set(value As String)
            ViewState("OrdenActual") = value
        End Set
    End Property

    Private Property DescendenteActual As Boolean
        Get
            Dim valor = ViewState("DescendenteActual")
            If valor Is Nothing Then Return False
            Return CBool(valor)
        End Get
        Set(value As Boolean)
            ViewState("DescendenteActual") = value
        End Set
    End Property

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            CargarClientes()
        End If
    End Sub

    Private Sub CargarClientes()
        Dim criterios As New CriteriosCliente With {
            .Busqueda = txtBusqueda.Text,
            .Orden = OrdenActual,
            .Descendente = DescendenteActual,
            .Pagina = PaginaActual,
            .TamanoPagina = TamanoPaginaClientes
        }

        Dim resultado = _clientes.Listar(criterios)

        ' Si se borró el último registro de la última página, la página actual queda fuera de
        ' rango y la rejilla saldría vacía con datos existentes detrás. Se retrocede y se repite.
        If resultado.Elementos.Count = 0 AndAlso resultado.TotalRegistros > 0 AndAlso PaginaActual > 1 Then
            PaginaActual = resultado.TotalPaginas
            criterios.Pagina = PaginaActual
            resultado = _clientes.Listar(criterios)
        End If

        gvClientes.DataSource = resultado.Elementos
        gvClientes.DataBind()

        pgClientes.Configurar(resultado)
    End Sub

    Protected Sub pgClientes_PaginaCambiada(sender As Object, e As PaginaCambiadaEventArgs) Handles pgClientes.PaginaCambiada
        PaginaActual = e.Pagina
        CargarClientes()
    End Sub

    ''' <summary>
    ''' Reordena por la columna pulsada. Pulsar la misma columna invierte la dirección; cambiar de
    ''' columna vuelve a ascendente y a la primera página, porque la fila que se estaba viendo ya
    ''' no está donde estaba.
    ''' </summary>
    Protected Sub gvClientes_Sorting(sender As Object, e As GridViewSortEventArgs) Handles gvClientes.Sorting
        If String.IsNullOrEmpty(e.SortExpression) Then Return

        If String.Equals(e.SortExpression, OrdenActual, StringComparison.Ordinal) Then
            DescendenteActual = Not DescendenteActual
        Else
            OrdenActual = e.SortExpression
            DescendenteActual = False
        End If

        PaginaActual = 1
        CargarClientes()
    End Sub

    ''' <summary>
    ''' Añade una flecha a la cabecera por la que se está ordenando. Sin esto, la rejilla reordena
    ''' pero no dice por qué columna ni en qué sentido, y el usuario solo puede deducirlo mirando
    ''' los datos. Va en RowCreated y no en RowDataBound porque la fila de encabezado no se enlaza
    ''' a datos y RowDataBound no se dispara para ella.
    ''' </summary>
    Protected Sub gvClientes_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles gvClientes.RowCreated
        If e.Row.RowType <> DataControlRowType.Header Then Return

        For Each celda As TableCell In e.Row.Cells
            If celda.Controls.Count = 0 Then Continue For

            Dim enlace = TryCast(celda.Controls(0), LinkButton)
            If enlace Is Nothing Then Continue For

            If Not String.Equals(If(enlace.CommandArgument, String.Empty), OrdenActual, StringComparison.Ordinal) Then Continue For

            Dim flecha As New Literal With {
                .Text = If(DescendenteActual, " <span class=""orden-indicador"">&#9660;</span>",
                                              " <span class=""orden-indicador"">&#9650;</span>")
            }
            celda.Controls.Add(flecha)
        Next
    End Sub

    Protected Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        PaginaActual = 1
        CargarClientes()
    End Sub

    Protected Sub btnLimpiarBusqueda_Click(sender As Object, e As EventArgs) Handles btnLimpiarBusqueda.Click
        txtBusqueda.Text = String.Empty
        PaginaActual = 1
        CargarClientes()
    End Sub

    Protected Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        LimpiarFormulario()
        litTituloFormulario.Text = "Nuevo cliente"
        pnlFormulario.Visible = True
        OcultarAviso()
    End Sub

    Protected Sub btnCancelar_Click(sender As Object, e As EventArgs) Handles btnCancelar.Click
        LimpiarFormulario()
        pnlFormulario.Visible = False
        OcultarAviso()
    End Sub

    ''' <summary>Despacha los comandos de las filas de la rejilla.</summary>
    Protected Sub gvClientes_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvClientes.RowCommand
        Dim clienteId As Integer

        If Not Integer.TryParse(Convert.ToString(e.CommandArgument), clienteId) Then Return

        Select Case e.CommandName
            Case "EditarCliente"
                CargarParaEdicion(clienteId)
        End Select
    End Sub

    Private Sub CargarParaEdicion(clienteId As Integer)
        Dim cliente = _clientes.ObtenerPorId(clienteId)

        If cliente Is Nothing Then
            MostrarAviso("El cliente ya no existe. Se actualizó el listado.", False)
            pnlFormulario.Visible = False
            CargarClientes()
            Return
        End If

        hdnClienteId.Value = cliente.ClienteId.ToString()
        hdnRowVersion.Value = If(cliente.RowVersion Is Nothing, String.Empty,
                                 Convert.ToBase64String(cliente.RowVersion))
        txtNombres.Text = cliente.Nombres
        txtApellidos.Text = cliente.Apellidos
        txtDocumento.Text = cliente.Documento
        txtEmail.Text = cliente.Email
        txtTelefono.Text = cliente.Telefono
        txtDireccion.Text = cliente.Direccion

        litTituloFormulario.Text = "Editar cliente"
        pnlFormulario.Visible = True
        OcultarAviso()
    End Sub

    Private Sub EliminarCliente(clienteId As Integer)
        Dim resultado = _clientes.Eliminar(clienteId, UsuarioIdActual, NombreUsuarioActual)

        MostrarAviso(resultado.PrimerMensaje, resultado.Exitoso)
        pnlFormulario.Visible = False
        LimpiarFormulario()
        ' No se reinicia PaginaActual aqui: si la fila borrada era la unica de la ultima
        ' pagina, el reajuste dentro de CargarClientes necesita ver la pagina fuera de rango
        ' para retroceder. Forzar la pagina 1 en cada borrado anularia ese caso de borde.
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
            .Direccion = txtDireccion.Text.Trim(),
            .RowVersion = LeerRowVersionDelFormulario()
        }

        Dim resultado = _clientes.Guardar(cliente, UsuarioIdActual, NombreUsuarioActual)

        If Not resultado.Exitoso Then
            MostrarAviso(String.Join(" ", resultado.Mensajes), False)
            pnlFormulario.Visible = True
            Return
        End If

        MostrarAviso(resultado.PrimerMensaje, True)
        LimpiarFormulario()
        pnlFormulario.Visible = False
        CargarClientes()
    End Sub

    Private Function ObtenerClienteIdDelFormulario() As Integer
        Dim clienteId As Integer
        If Integer.TryParse(hdnClienteId.Value, clienteId) Then Return clienteId
        Return 0
    End Function

    ''' <summary>
    ''' Recupera la marca de versión que se envió al formulario. Viaja en Base64 dentro de un
    ''' campo oculto y no en Session: es estado de esta página concreta, y una sesión perdida no
    ''' debe convertir una edición en una sobrescritura silenciosa.
    ''' </summary>
    Private Function LeerRowVersionDelFormulario() As Byte()
        If String.IsNullOrWhiteSpace(hdnRowVersion.Value) Then Return Nothing

        Try
            Return Convert.FromBase64String(hdnRowVersion.Value)
        Catch ex As FormatException
            ' Un valor manipulado no debe tumbar la página: se trata como versión ausente, lo que
            ' provoca el conflicto y obliga a recargar el registro.
            Return Nothing
        End Try
    End Function

    Private Sub LimpiarFormulario()
        hdnClienteId.Value = "0"
        hdnRowVersion.Value = String.Empty
        txtNombres.Text = String.Empty
        txtApellidos.Text = String.Empty
        txtDocumento.Text = String.Empty
        txtEmail.Text = String.Empty
        txtTelefono.Text = String.Empty
        txtDireccion.Text = String.Empty
    End Sub

    ''' <summary>Muestra un aviso. El texto se codifica porque puede contener datos del usuario.</summary>
    Private Sub MostrarAviso(texto As String, exitoso As Boolean)
        If String.IsNullOrWhiteSpace(texto) Then
            OcultarAviso()
            Return
        End If

        litAviso.Text = HttpUtility.HtmlEncode(texto)
        pnlAviso.CssClass = If(exitoso, "alert alert-success py-2", "alert alert-danger py-2")
        pnlAviso.Visible = True
    End Sub

    Private Sub OcultarAviso()
        pnlAviso.Visible = False
        litAviso.Text = String.Empty
    End Sub

    ''' <summary>
    ''' Confirma el borrado. El identificador llega del campo oculto que rellenó el modal, y se
    ''' valida igual que cualquier otra entrada: el diálogo es comodidad de interfaz, no control.
    ''' </summary>
    Protected Sub btnConfirmarEliminar_Click(sender As Object, e As EventArgs) Handles btnConfirmarEliminar.Click
        Dim clienteId As Integer

        If Not Integer.TryParse(hdnClienteAEliminar.Value, clienteId) Then
            MostrarAviso("No se indicó el cliente a eliminar.", False)
            Return
        End If

        hdnClienteAEliminar.Value = String.Empty
        EliminarCliente(clienteId)
    End Sub

    ''' <summary>
    ''' Prepara el nombre del cliente para viajar dentro de un atributo HTML delimitado por
    ''' comillas simples. HtmlAttributeEncode no escapa el apóstrofo, así que se hace aparte: un
    ''' apellido como O'Brien cerraría el atributo antes de tiempo.
    ''' </summary>
    Protected Function NombreParaAtributo(elemento As Object) As String
        Dim cliente = TryCast(elemento, Cliente)
        If cliente Is Nothing Then Return String.Empty

        Return HttpUtility.HtmlAttributeEncode(cliente.NombreCompleto).Replace("'", "&#39;")
    End Function

End Class
