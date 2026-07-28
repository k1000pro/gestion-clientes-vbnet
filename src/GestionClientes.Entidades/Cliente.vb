''' <summary>
''' Representa a un cliente del sistema. Es un contenedor de datos: no contiene reglas de
''' negocio ni acceso a base de datos.
''' </summary>
Public Class Cliente

    Public Property ClienteId As Integer
    Public Property Nombres As String = String.Empty
    Public Property Apellidos As String = String.Empty
    Public Property Documento As String = String.Empty
    Public Property Email As String = String.Empty
    Public Property Telefono As String = String.Empty
    Public Property Direccion As String = String.Empty
    Public Property FechaRegistro As DateTime

    ''' <summary>Nombre y apellidos concatenados, para mostrar en la interfaz.</summary>
    Public ReadOnly Property NombreCompleto As String
        Get
            Return (Nombres & " " & Apellidos).Trim()
        End Get
    End Property

End Class
