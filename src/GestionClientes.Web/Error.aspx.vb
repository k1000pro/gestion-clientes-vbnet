''' <summary>
''' Página de error genérica. No muestra el detalle de la excepción: exponer un stack trace revela
''' rutas, nombres de ensamblado y versiones que facilitan un ataque dirigido.
'''
''' El único caso que se distingue es el contenido no permitido en un campo, porque no es un fallo
''' del sistema sino un dato que el usuario puede corregir, y merece decírselo.
''' </summary>
Public Class PaginaError
    Inherits Page

    Private Const MensajeGenerico As String =
        "No fue posible completar la operación. El detalle quedó registrado para su revisión."

    Private Const MensajeContenidoNoPermitido As String =
        "El texto ingresado contiene caracteres que no se aceptan por seguridad, como < o >. " &
        "Corrija el dato y vuelva a intentarlo."

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Misma política de caché que las páginas autenticadas: esta se alcanza desde ellas y no
        ' debe quedar en el historial del navegador.
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        Response.Cache.SetNoStore()
        Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1))

        ' El motivo solo se acepta cuando la página se alcanzó por redirección explícita. En la
        ' ruta de customErrors, que usa ResponseRewrite, esta página se ejecuta dentro de la
        ' petición original y vería su cadena de consulta, no la nuestra.
        Dim esRedireccionExplicita = Server.GetLastError() Is Nothing

        Dim esContenidoNoPermitido =
            esRedireccionExplicita AndAlso
            String.Equals(Request.QueryString("motivo"), "contenido", StringComparison.Ordinal)

        litMensaje.Text = HttpUtility.HtmlEncode(
            If(esContenidoNoPermitido, MensajeContenidoNoPermitido, MensajeGenerico))
    End Sub

End Class
