''' <summary>
''' Error de regla de negocio detectado en la base de datos, por ejemplo un documento duplicado.
''' La capa de datos traduce aquí los errores personalizados de los procedimientos almacenados.
''' </summary>
Public Class ReglaNegocioException
    Inherits Exception

    Public Sub New(mensaje As String)
        MyBase.New(mensaje)
    End Sub

End Class
