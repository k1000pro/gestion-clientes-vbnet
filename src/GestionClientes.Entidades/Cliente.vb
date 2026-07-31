''' <summary>
''' Cliente del sistema. Contenedor de datos: sin reglas de negocio ni acceso a base de datos.
''' </summary>
Public Class Cliente
    Inherits EntidadAuditable

    Public Property ClienteId As Integer
    Public Property Nombres As String = String.Empty
    Public Property Apellidos As String = String.Empty
    Public Property Documento As String = String.Empty
    Public Property Email As String = String.Empty
    Public Property Telefono As String = String.Empty
    Public Property Direccion As String = String.Empty

    ' Mantenida por SQL Server. Detecta que otro usuario modificó el registro entre que este lo
    ' cargó y lo guardó.
    Public Property RowVersion As Byte()

    Public ReadOnly Property NombreCompleto As String
        Get
            Return (Nombres & " " & Apellidos).Trim()
        End Get
    End Property

End Class
