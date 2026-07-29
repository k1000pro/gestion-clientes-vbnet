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
    ''' </summary>
    Sub Application_Error(sender As Object, e As EventArgs)
        Dim excepcion = Server.GetLastError()
        If excepcion Is Nothing Then Return

        Registro.Error($"Excepción no controlada en {Request.Url}", excepcion)
    End Sub

End Class
