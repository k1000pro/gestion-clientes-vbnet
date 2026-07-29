Imports System.Reflection
Imports log4net

''' <summary>Eventos de ciclo de vida de la aplicación.</summary>
Public Class Global_asax
    Inherits HttpApplication

    Private Shared ReadOnly Registro As ILog = LogManager.GetLogger(GetType(Global_asax))

    ''' <summary>Carga la configuración de log4net declarada en Web.config.</summary>
    Sub Application_Start(sender As Object, e As EventArgs)
        log4net.Config.XmlConfigurator.Configure()
        Registro.Info("Aplicación iniciada.")
    End Sub

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
    ''' Registra las excepciones no controladas. Con customErrors en RemoteOnly, un cliente remoto
    ''' solo ve la página de error genérica; en la máquina local se conserva el detalle para
    ''' diagnosticar. El registro es la fuente fiable en ambos casos.
    '''
    ''' Ya no hace falta envolver la escritura en un Try mudo: log4net no propaga los fallos de su
    ''' appender al llamador, que es exactamente lo que aquel Try imitaba a mano.
    '''
    ''' La URL se obtiene aparte, dentro de su propio Try: este manejador puede dispararse en
    ''' contextos donde Request no está disponible (típicamente un fallo durante el arranque de la
    ''' aplicación), y ahí Request.Url lanza HttpException. Si eso ocurriera dentro de la
    ''' interpolación del mensaje, el manejador de errores fallaría y ocultaría la excepción
    ''' original, que es justo lo que existe para evitar.
    ''' </summary>
    Sub Application_Error(sender As Object, e As EventArgs)
        Dim excepcion = Server.GetLastError()
        If excepcion Is Nothing Then Return

        ' La validación de petición de ASP.NET rechaza contenido que parece marcado. No es un
        ' fallo del sistema: es la protección funcionando, y el usuario puede corregirlo. Se
        ' registra como advertencia y se le responde con un mensaje entendible, sin desactivar ni
        ' relajar la validación.
        Dim validacion = TryCast(excepcion, HttpUnhandledException)
        Dim raiz = If(validacion IsNot Nothing AndAlso validacion.InnerException IsNot Nothing,
                      validacion.InnerException, excepcion)

        If TypeOf raiz Is HttpRequestValidationException Then
            Registro.Warn("Contenido no permitido rechazado por la validación de petición.")
            Server.ClearError()
            Response.Redirect("~/Error.aspx?motivo=contenido", False)
            Context.ApplicationInstance.CompleteRequest()
            Return
        End If

        Dim url As String = "(desconocida)"
        Try
            url = HttpContext.Current?.Request?.Url?.ToString()
        Catch
            ' Request no está disponible en todos los contextos en los que se dispara este
            ' manejador, y un fallo aquí ocultaría la excepción original.
        End Try

        Registro.Error($"Excepción no controlada en {If(url, "(desconocida)")}", excepcion)
    End Sub

End Class
