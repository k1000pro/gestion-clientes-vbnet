''' <summary>
''' Una página de resultados junto con el total de registros que cumplen el criterio. El total
''' viaja con la página para que el conteo y los datos no puedan corresponder a estados distintos.
''' </summary>
''' <typeparam name="T">Tipo de los elementos de la página.</typeparam>
Public Class ResultadoPaginado(Of T)

    Public Property Elementos As New List(Of T)()

    ' Registros que cumplen el filtro, no los que caben en esta página.
    Public Property TotalRegistros As Integer

    Public Property Pagina As Integer = 1

    Public Property TamanoPagina As Integer = 10

    ' Cero si el tamaño de página no es válido, en lugar de dividir entre cero.
    Public ReadOnly Property TotalPaginas As Integer
        Get
            If TamanoPagina <= 0 Then Return 0
            Return CInt(Math.Ceiling(TotalRegistros / CDbl(TamanoPagina)))
        End Get
    End Property

End Class
