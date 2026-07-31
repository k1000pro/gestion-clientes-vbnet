''' <summary>
''' Criterios de consulta del listado de clientes: qué buscar, cómo ordenar y qué página traer.
''' </summary>
Public Class CriteriosCliente

    Public Property Busqueda As String = String.Empty

    ' El procedimiento reconoce "Documento", "Nombres", "Apellidos" y "FechaRegistro". Cualquier
    ' otro valor cae en el orden de desempate.
    Public Property Orden As String = "Apellidos"

    Public Property Descendente As Boolean

    Public Property Pagina As Integer = 1

    Public Property TamanoPagina As Integer = 10

End Class
