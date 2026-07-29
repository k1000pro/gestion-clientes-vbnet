''' <summary>
''' Criterios de consulta de la bitácora. Todas las propiedades son opcionales; una propiedad
''' sin valor significa "no filtrar por este campo".
''' </summary>
Public Class FiltroBitacora

    Public Property FechaDesde As Date?
    Public Property FechaHasta As Date?
    Public Property Accion As String = String.Empty
    Public Property NombreUsuario As String = String.Empty

    ''' <summary>
    ''' Columna por la que ordenar. El procedimiento reconoce "FechaHora", "Accion" y
    ''' "NombreUsuario"; cualquier otro valor cae en el orden de desempate.
    ''' </summary>
    Public Property Orden As String = "FechaHora"

    ''' <summary>La bitácora se lee de lo más reciente a lo más antiguo por omisión.</summary>
    Public Property Descendente As Boolean = True

    Public Property Pagina As Integer = 1

    Public Property TamanoPagina As Integer = 15

End Class
