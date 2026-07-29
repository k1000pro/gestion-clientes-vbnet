''' <summary>
''' Representa a un cliente del sistema. Es un contenedor de datos: no contiene reglas de
''' negocio ni acceso a base de datos.
'''
''' Hereda de EntidadAuditable los campos de creación, modificación y borrado lógico, que son
''' comunes a cualquier entidad persistida y no describen nada propio de un cliente.
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

    ''' <summary>
    ''' Marca de versión de la fila, mantenida por SQL Server. Se usa para detectar que otro
    ''' usuario modificó el registro entre que este lo cargó y lo guardó.
    ''' </summary>
    Public Property RowVersion As Byte()

    ''' <summary>Nombre y apellidos concatenados, para mostrar en la interfaz.</summary>
    Public ReadOnly Property NombreCompleto As String
        Get
            Return (Nombres & " " & Apellidos).Trim()
        End Get
    End Property

End Class
