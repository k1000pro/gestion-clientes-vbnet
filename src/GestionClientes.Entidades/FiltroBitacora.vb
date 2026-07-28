''' <summary>
''' Criterios de consulta de la bitácora. Todas las propiedades son opcionales; una propiedad
''' sin valor significa "no filtrar por este campo".
''' </summary>
Public Class FiltroBitacora

    Public Property FechaDesde As Date?
    Public Property FechaHasta As Date?
    Public Property Accion As String = String.Empty
    Public Property NombreUsuario As String = String.Empty

End Class
