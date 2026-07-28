''' <summary>
''' Resultado de una operación de negocio. Las validaciones fallidas se comunican con esta clase
''' y no lanzando excepciones: una validación fallida es un desenlace esperado, no un error del
''' programa, y usar excepciones para control de flujo oculta la intención y cuesta rendimiento.
''' </summary>
Public Class ResultadoOperacion

    Public Property Exitoso As Boolean
    Public Property Mensajes As New List(Of String)()

    ''' <summary>Primer mensaje disponible, o cadena vacía. Cómodo para mostrar en la interfaz.</summary>
    Public ReadOnly Property PrimerMensaje As String
        Get
            If Mensajes Is Nothing OrElse Mensajes.Count = 0 Then Return String.Empty
            Return Mensajes(0)
        End Get
    End Property

    Public Shared Function Exito() As ResultadoOperacion
        Return New ResultadoOperacion With {.Exitoso = True}
    End Function

    Public Shared Function Exito(mensaje As String) As ResultadoOperacion
        Dim resultado As New ResultadoOperacion With {.Exitoso = True}
        resultado.Mensajes.Add(mensaje)
        Return resultado
    End Function

    Public Shared Function Fallo(mensaje As String) As ResultadoOperacion
        Dim resultado As New ResultadoOperacion With {.Exitoso = False}
        resultado.Mensajes.Add(mensaje)
        Return resultado
    End Function

    Public Shared Function Fallo(mensajes As IEnumerable(Of String)) As ResultadoOperacion
        Dim resultado As New ResultadoOperacion With {.Exitoso = False}
        resultado.Mensajes.AddRange(mensajes)
        Return resultado
    End Function

End Class
