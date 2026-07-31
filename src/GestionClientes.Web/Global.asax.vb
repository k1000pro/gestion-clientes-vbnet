Imports System.Reflection
Imports log4net

Public Class Global_asax
    Inherits HttpApplication

    Private Shared ReadOnly Registro As ILog = LogManager.GetLogger(GetType(Global_asax))

    Sub Application_Start(sender As Object, e As EventArgs)
        log4net.Config.XmlConfigurator.Configure()
        Registro.Info("Aplicación iniciada.")
    End Sub

    ' Se escribe algo en la sesión para que el SessionID quede fijo desde la primera petición.
    ' Sin esto ASP.NET lo regenera en cada respuesta hasta que se almacena algo, y ViewStateUserKey
    ' —que lo usa como semilla— haría fallar la validación del ViewState en el primer postback.
    Sub Session_Start(sender As Object, e As EventArgs)
        Session("Iniciada") = True
    End Sub

    Sub Application_Error(sender As Object, e As EventArgs)
        Dim excepcion = Server.GetLastError()
        If excepcion Is Nothing Then Return

        Dim url As String = "(desconocida)"
        Try
            Dim solicitada = HttpContext.Current?.Request?.Url?.ToString()
            If Not String.IsNullOrEmpty(solicitada) Then url = solicitada
        Catch
            ' Request no está disponible en todos los contextos en los que se dispara este
            ' manejador (por ejemplo, un fallo durante el arranque). Un HttpException aquí
            ' ocultaría la excepción original.
        End Try

        ' Contenido rechazado por la validación de petición: no es un fallo del sistema sino la
        ' protección funcionando, y el usuario puede corregirlo. Se registra como advertencia.
        Dim validacion = TryCast(excepcion, HttpUnhandledException)
        Dim raiz = If(validacion IsNot Nothing AndAlso validacion.InnerException IsNot Nothing,
                      validacion.InnerException, excepcion)

        If TypeOf raiz Is HttpRequestValidationException Then
            Registro.Warn($"Contenido no permitido rechazado por la validación de petición en {url}.")
            Server.ClearError()
            Response.Redirect("~/Error.aspx?motivo=contenido", False)
            Context.ApplicationInstance.CompleteRequest()
            Return
        End If

        Registro.Error($"Excepción no controlada en {url}", excepcion)
    End Sub

End Class
