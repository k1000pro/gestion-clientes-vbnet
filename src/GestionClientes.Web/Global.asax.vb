Imports System.IO

''' <summary>Eventos de ciclo de vida de la aplicación.</summary>
Public Class Global_asax
    Inherits HttpApplication

    ''' <summary>
    ''' Se escribe un valor en la sesión al iniciarla para que el identificador de sesión quede
    ''' fijo desde la primera petición. Sin esto, ASP.NET genera un SessionID nuevo en cada
    ''' respuesta hasta que algo se almacena, y ViewStateUserKey (que lo usa como semilla) haría
    ''' fallar la validación del ViewState en el primer postback.
    ''' </summary>
    Sub Session_Start(sender As Object, e As EventArgs)
        Session("Iniciada") = True
    End Sub

    ''' <summary>
    ''' Registra las excepciones no controladas en App_Data\errores.log. Con customErrors en
    ''' RemoteOnly, un cliente remoto solo ve la página de error genérica; en la máquina local se
    ''' conserva el detalle para diagnosticar. El registro es la fuente fiable en ambos casos.
    ''' </summary>
    Sub Application_Error(sender As Object, e As EventArgs)
        Dim excepcion = Server.GetLastError()
        If excepcion Is Nothing Then Return

        Try
            Dim carpeta = Server.MapPath("~/App_Data")
            If Not Directory.Exists(carpeta) Then Directory.CreateDirectory(carpeta)

            Dim linea As String =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {Request.Url}" & Environment.NewLine &
                excepcion.ToString() & Environment.NewLine & New String("-"c, 80) & Environment.NewLine

            File.AppendAllText(Path.Combine(carpeta, "errores.log"), linea)

        Catch
            ' Si no se puede registrar el error, no tiene sentido lanzar otro desde el manejador
            ' de errores: se pierde el registro pero la aplicación sigue respondiendo.
        End Try
    End Sub

End Class
