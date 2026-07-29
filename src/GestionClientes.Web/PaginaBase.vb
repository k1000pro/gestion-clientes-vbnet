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

End Class
