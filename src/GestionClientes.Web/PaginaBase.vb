''' <summary>
''' Página base de las pantallas autenticadas: protección anti-CSRF, cabeceras de no-caché y
''' acceso a la identidad del usuario.
''' </summary>
Public Class PaginaBase
    Inherits Page

    Protected Overrides Sub OnInit(e As EventArgs)
        ' Vincula el ViewState a la sesión. Uno capturado de otra sesión falla la validación de
        ' integridad, lo que bloquea la falsificación de peticiones entre sitios en los postbacks.
        ViewStateUserKey = Session.SessionID

        ' Las páginas autenticadas no se guardan en la caché del navegador. Sin esto, el botón
        ' Atrás sigue mostrando los datos del cliente después de cerrar sesión: la autorización
        ' impide cualquier postback, pero la información ya está en pantalla.
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        Response.Cache.SetNoStore()
        Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1))

        MyBase.OnInit(e)
    End Sub

    ' 0 si no hay identidad.
    Protected ReadOnly Property UsuarioIdActual As Integer
        Get
            Return IdentidadUsuario.ObtenerUsuarioId(User)
        End Get
    End Property

    ' Tomado del ticket de autenticación, no de la sesión.
    Protected ReadOnly Property NombreUsuarioActual As String
        Get
            If User Is Nothing OrElse User.Identity Is Nothing Then Return String.Empty
            Return User.Identity.Name
        End Get
    End Property

    ' Se invoca desde RowCreated y no desde RowDataBound: la fila de encabezado no se enlaza a
    ' datos, así que RowDataBound no se dispara para ella.
    Protected Shared Sub MarcarCabeceraOrdenada(fila As GridViewRow, orden As String, descendente As Boolean)
        If fila Is Nothing OrElse fila.RowType <> DataControlRowType.Header Then Return

        For Each celda As TableCell In fila.Cells
            If celda.Controls.Count = 0 Then Continue For

            Dim enlace = TryCast(celda.Controls(0), LinkButton)
            If enlace Is Nothing Then Continue For

            If Not String.Equals(If(enlace.CommandArgument, String.Empty), orden, StringComparison.Ordinal) Then Continue For

            celda.Attributes("aria-sort") = If(descendente, "descending", "ascending")

            celda.Controls.Add(New Literal With {
                .Text = If(descendente, "<span class=""orden-indicador"">&#9660;</span>",
                                        "<span class=""orden-indicador"">&#9650;</span>")
            })
        Next
    End Sub

End Class
