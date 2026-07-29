''' <summary>
''' Página base de las pantallas autenticadas. Aporta la protección anti-CSRF y el acceso a los
''' datos del usuario en sesión, para que las páginas no repitan ese código.
''' </summary>
Public Class PaginaBase
    Inherits Page

    ''' <summary>
    ''' Vincula el ViewState a la sesión del usuario. Un ViewState capturado de otra sesión falla
    ''' la validación de integridad, lo que bloquea los ataques de falsificación de petición
    ''' entre sitios sobre los postbacks.
    ''' </summary>
    Protected Overrides Sub OnInit(e As EventArgs)
        ViewStateUserKey = Session.SessionID

        ' Las páginas autenticadas no se guardan en la caché del navegador. Sin esto, el botón
        ' Atrás sigue mostrando los datos del cliente después de cerrar sesión: la autorización
        ' impide cualquier postback, pero la información ya está en pantalla.
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        Response.Cache.SetNoStore()
        Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1))

        MyBase.OnInit(e)
    End Sub

    ''' <summary>Identificador del usuario autenticado, o 0 si no hay identidad.</summary>
    Protected ReadOnly Property UsuarioIdActual As Integer
        Get
            Return IdentidadUsuario.ObtenerUsuarioId(User)
        End Get
    End Property

    ''' <summary>Nombre de usuario autenticado, tomado del ticket de autenticación.</summary>
    Protected ReadOnly Property NombreUsuarioActual As String
        Get
            If User Is Nothing OrElse User.Identity Is Nothing Then Return String.Empty
            Return User.Identity.Name
        End Get
    End Property

    ''' <summary>
    ''' Marca la cabecera por la que se está ordenando con una flecha de dirección. Vive aquí y no
    ''' en cada página porque las dos rejillas necesitan exactamente el mismo comportamiento.
    '''
    ''' Se invoca desde RowCreated y no desde RowDataBound: la fila de encabezado no se enlaza a
    ''' datos, así que RowDataBound no se dispara para ella.
    ''' </summary>
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
