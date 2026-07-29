Imports System.Globalization
Imports GestionClientes.Entidades
Imports GestionClientes.Negocio

''' <summary>Consulta de solo lectura de la bitácora de auditoría.</summary>
Public Class PaginaBitacora
    Inherits PaginaBase

    Private ReadOnly _bitacora As New ServicioBitacora()

    ''' <summary>Tamaño de página de la bitácora.</summary>
    Private Const TamanoPaginaBitacora As Integer = 15

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
            If String.IsNullOrEmpty(valor) Then Return "FechaHora"
            Return valor
        End Get
        Set(value As String)
            ViewState("OrdenActual") = value
        End Set
    End Property

    Private Property DescendenteActual As Boolean
        Get
            Dim valor = ViewState("DescendenteActual")
            If valor Is Nothing Then Return True
            Return CBool(valor)
        End Get
        Set(value As Boolean)
            ViewState("DescendenteActual") = value
        End Set
    End Property

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            CargarUsuarios()
            CargarBitacora()
        End If
    End Sub

    ''' <summary>Puebla el filtro de usuarios con los que efectivamente aparecen en la bitácora.</summary>
    Private Sub CargarUsuarios()
        ddlUsuario.Items.Clear()
        ddlUsuario.Items.Add(New ListItem("Todos", String.Empty))

        For Each nombreUsuario In _bitacora.ObtenerUsuarios()
            ddlUsuario.Items.Add(New ListItem(nombreUsuario, nombreUsuario))
        Next
    End Sub

    Private Sub CargarBitacora()
        Dim filtro = ConstruirFiltro()
        If filtro Is Nothing Then Return

        filtro.Orden = OrdenActual
        filtro.Descendente = DescendenteActual
        filtro.Pagina = PaginaActual
        filtro.TamanoPagina = TamanoPaginaBitacora

        Dim resultado = _bitacora.Listar(filtro)

        gvBitacora.DataSource = resultado.Elementos
        gvBitacora.DataBind()

        pgBitacora.Configurar(resultado)
    End Sub

    Protected Sub pgBitacora_PaginaCambiada(sender As Object, e As PaginaCambiadaEventArgs) Handles pgBitacora.PaginaCambiada
        PaginaActual = e.Pagina
        CargarBitacora()
    End Sub

    Protected Sub gvBitacora_Sorting(sender As Object, e As GridViewSortEventArgs) Handles gvBitacora.Sorting
        If String.IsNullOrEmpty(e.SortExpression) Then Return

        If String.Equals(e.SortExpression, OrdenActual, StringComparison.Ordinal) Then
            DescendenteActual = Not DescendenteActual
        Else
            OrdenActual = e.SortExpression
            DescendenteActual = False
        End If

        PaginaActual = 1
        CargarBitacora()
    End Sub

    ''' <summary>
    ''' Arma el filtro a partir de los controles. Devuelve Nothing y muestra un aviso si el rango
    ''' de fechas está invertido, en lugar de consultar sabiendo que no devolverá nada.
    ''' </summary>
    Private Function ConstruirFiltro() As FiltroBitacora
        Dim filtro As New FiltroBitacora With {
            .Accion = ddlAccion.SelectedValue,
            .NombreUsuario = ddlUsuario.SelectedValue
        }

        Dim desde As Date
        If Date.TryParse(txtFechaDesde.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, desde) Then
            filtro.FechaDesde = desde
        End If

        Dim hasta As Date
        If Date.TryParse(txtFechaHasta.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, hasta) Then
            filtro.FechaHasta = hasta
        End If

        If filtro.FechaDesde.HasValue AndAlso filtro.FechaHasta.HasValue AndAlso
           filtro.FechaDesde.Value > filtro.FechaHasta.Value Then

            MostrarAviso("La fecha inicial no puede ser posterior a la fecha final.")
            Return Nothing
        End If

        OcultarAviso()
        Return filtro
    End Function

    Protected Sub btnFiltrar_Click(sender As Object, e As EventArgs) Handles btnFiltrar.Click
        PaginaActual = 1
        CargarBitacora()
    End Sub

    Protected Sub btnLimpiarFiltro_Click(sender As Object, e As EventArgs) Handles btnLimpiarFiltro.Click
        txtFechaDesde.Text = String.Empty
        txtFechaHasta.Text = String.Empty
        ddlAccion.SelectedIndex = 0
        ddlUsuario.SelectedIndex = 0
        PaginaActual = 1
        CargarBitacora()
    End Sub

    Private Sub MostrarAviso(texto As String)
        litAviso.Text = HttpUtility.HtmlEncode(texto)
        pnlAviso.Visible = True
    End Sub

    Private Sub OcultarAviso()
        pnlAviso.Visible = False
        litAviso.Text = String.Empty
    End Sub

End Class
