Imports GestionClientes.Entidades

''' <summary>Argumentos del evento que notifica un cambio de página.</summary>
Public Class PaginaCambiadaEventArgs
    Inherits EventArgs

    Public Sub New(pagina As Integer)
        Me.Pagina = pagina
    End Sub

    Public ReadOnly Property Pagina As Integer

End Class

''' <summary>
''' Paginador reutilizable con aspecto de Bootstrap. No se usa el de GridView porque asume que la
''' rejilla tiene todas las filas en memoria, que es justo lo que se dejó de hacer.
''' </summary>
Public Class Paginador
    Inherits UserControl

    ' Números de página que se muestran alrededor de la actual.
    Private Const VentanaDePaginas As Integer = 5

    Public Event PaginaCambiada As EventHandler(Of PaginaCambiadaEventArgs)

    Public Property PaginaActual As Integer = 1
    Public Property TotalRegistros As Integer
    Public Property TamanoPagina As Integer = 10

    Public ReadOnly Property TotalPaginas As Integer
        Get
            If TamanoPagina <= 0 Then Return 0
            Return CInt(Math.Ceiling(TotalRegistros / CDbl(TamanoPagina)))
        End Get
    End Property

    ''' <summary>
    ''' Ajusta el paginador a un resultado y lo dibuja. Se oculta cuando todo cabe en una página.
    ''' </summary>
    Public Sub Configurar(Of T)(resultado As ResultadoPaginado(Of T))
        If resultado Is Nothing Then
            pnlPaginador.Visible = False
            Return
        End If

        PaginaActual = resultado.Pagina
        TotalRegistros = resultado.TotalRegistros
        TamanoPagina = resultado.TamanoPagina

        Dibujar()
    End Sub

    Private Sub Dibujar()
        If TotalPaginas <= 1 Then
            pnlPaginador.Visible = False
            Return
        End If

        pnlPaginador.Visible = True

        Dim primero = ((PaginaActual - 1) * TamanoPagina) + 1
        Dim ultimo = Math.Min(PaginaActual * TamanoPagina, TotalRegistros)
        litResumen.Text = HttpUtility.HtmlEncode($"Mostrando {primero}–{ultimo} de {TotalRegistros}")

        lnkAnterior.Enabled = PaginaActual > 1
        lnkAnterior.CssClass = If(PaginaActual > 1, "page-link", "page-link disabled")
        lnkAnterior.CommandArgument = (PaginaActual - 1).ToString()

        lnkSiguiente.Enabled = PaginaActual < TotalPaginas
        lnkSiguiente.CssClass = If(PaginaActual < TotalPaginas, "page-link", "page-link disabled")
        lnkSiguiente.CommandArgument = (PaginaActual + 1).ToString()

        rptPaginas.DataSource = CalcularVentana()
        rptPaginas.DataBind()
    End Sub

    ' Ventana centrada en la página actual, recortada a los extremos para ofrecer siempre tantas
    ' opciones como páginas haya.
    Private Function CalcularVentana() As List(Of Integer)
        Dim mitad = VentanaDePaginas \ 2
        Dim inicio = Math.Max(1, PaginaActual - mitad)
        Dim fin = Math.Min(TotalPaginas, inicio + VentanaDePaginas - 1)

        inicio = Math.Max(1, Math.Min(inicio, fin - VentanaDePaginas + 1))

        Dim paginas As New List(Of Integer)()
        For numero = inicio To fin
            paginas.Add(numero)
        Next

        Return paginas
    End Function

    Protected Sub rptPaginas_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles rptPaginas.ItemDataBound
        If e.Item.ItemType <> ListItemType.Item AndAlso e.Item.ItemType <> ListItemType.AlternatingItem Then Return

        Dim enlace = TryCast(e.Item.FindControl("lnkPagina"), LinkButton)
        If enlace Is Nothing Then Return

        Dim numero As Integer
        If Not Integer.TryParse(enlace.CommandArgument, numero) Then Return

        If numero = PaginaActual Then
            enlace.Enabled = False
            enlace.CssClass = "page-link active"
        End If
    End Sub

    ' Traduce cualquier clic del paginador en un único evento hacia la página.
    Protected Sub Paginador_ItemCommand(sender As Object, e As CommandEventArgs) Handles lnkAnterior.Command, lnkSiguiente.Command, rptPaginas.ItemCommand
        If e.CommandName <> "Ir" Then Return

        Dim pagina As Integer
        If Not Integer.TryParse(Convert.ToString(e.CommandArgument), pagina) Then Return
        If pagina < 1 Then Return

        PaginaActual = pagina
        RaiseEvent PaginaCambiada(Me, New PaginaCambiadaEventArgs(pagina))
    End Sub

End Class
