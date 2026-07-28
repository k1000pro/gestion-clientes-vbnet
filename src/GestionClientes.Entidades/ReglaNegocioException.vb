''' <summary>
''' Error de regla de negocio detectado en la base de datos (por ejemplo, documento duplicado).
''' La capa de datos traduce los errores de SQL Server con número 50001 y 50002 a esta excepción,
''' de modo que la capa de negocio no necesite conocer SqlException ni códigos de error del motor.
''' </summary>
Public Class ReglaNegocioException
    Inherits Exception

    Public Sub New(mensaje As String)
        MyBase.New(mensaje)
    End Sub

End Class
