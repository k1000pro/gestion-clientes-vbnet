''' <summary>
''' Una página de resultados junto con el total de registros que cumplen el criterio.
'''
''' El total viaja con la página porque la interfaz necesita ambas cosas para dibujar el
''' paginador, y obtenerlo con una segunda consulta abriría una ventana en la que el conteo y
''' los datos podrían no corresponderse.
''' </summary>
''' <typeparam name="T">Tipo de los elementos de la página.</typeparam>
Public Class ResultadoPaginado(Of T)

    Public Property Elementos As New List(Of T)()

    ''' <summary>Registros que cumplen el filtro, no los que caben en esta página.</summary>
    Public Property TotalRegistros As Integer

    Public Property Pagina As Integer = 1

    Public Property TamanoPagina As Integer = 10

    ''' <summary>
    ''' Páginas necesarias para mostrar todos los registros. Devuelve cero si el tamaño de página
    ''' no es válido, en lugar de dividir entre cero.
    ''' </summary>
    Public ReadOnly Property TotalPaginas As Integer
        Get
            If TamanoPagina <= 0 Then Return 0
            Return CInt(Math.Ceiling(TotalRegistros / CDbl(TamanoPagina)))
        End Get
    End Property

End Class
