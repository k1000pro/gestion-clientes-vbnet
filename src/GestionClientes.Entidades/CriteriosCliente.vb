''' <summary>
''' Criterios de consulta del listado de clientes: qué buscar, cómo ordenar y qué página traer.
'''
''' Se agrupan en una clase en lugar de pasarse como cinco parámetros sueltos para que agregar un
''' criterio nuevo no obligue a cambiar la firma de cada método de cada capa.
''' </summary>
Public Class CriteriosCliente

    Public Property Busqueda As String = String.Empty

    ''' <summary>
    ''' Columna por la que ordenar. Los valores que el procedimiento reconoce son "Documento",
    ''' "Nombres", "Apellidos" y "FechaRegistro"; cualquier otro cae en el orden de desempate.
    ''' </summary>
    Public Property Orden As String = "Apellidos"

    Public Property Descendente As Boolean

    Public Property Pagina As Integer = 1

    Public Property TamanoPagina As Integer = 10

End Class
