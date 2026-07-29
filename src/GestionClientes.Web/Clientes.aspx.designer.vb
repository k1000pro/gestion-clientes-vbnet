Option Strict On
Option Explicit On

Partial Public Class PaginaClientes

    Protected WithEvents btnNuevo As Global.System.Web.UI.WebControls.Button

    Protected WithEvents pnlMensaje As Global.System.Web.UI.WebControls.Panel

    Protected WithEvents litMensaje As Global.System.Web.UI.WebControls.Literal

    Protected WithEvents txtBusqueda As Global.System.Web.UI.WebControls.TextBox

    Protected WithEvents btnBuscar As Global.System.Web.UI.WebControls.Button

    Protected WithEvents btnLimpiarBusqueda As Global.System.Web.UI.WebControls.Button

    Protected WithEvents gvClientes As Global.System.Web.UI.WebControls.GridView

    Protected WithEvents pnlFormulario As Global.System.Web.UI.WebControls.Panel

    Protected WithEvents litTituloFormulario As Global.System.Web.UI.WebControls.Literal

    Protected WithEvents hdnClienteId As Global.System.Web.UI.WebControls.HiddenField

    Protected WithEvents txtNombres As Global.System.Web.UI.WebControls.TextBox

    Protected WithEvents valNombres As Global.System.Web.UI.WebControls.RequiredFieldValidator

    Protected WithEvents txtApellidos As Global.System.Web.UI.WebControls.TextBox

    Protected WithEvents valApellidos As Global.System.Web.UI.WebControls.RequiredFieldValidator

    Protected WithEvents txtDocumento As Global.System.Web.UI.WebControls.TextBox

    Protected WithEvents valDocumentoRequerido As Global.System.Web.UI.WebControls.RequiredFieldValidator

    Protected WithEvents valDocumentoFormato As Global.System.Web.UI.WebControls.RegularExpressionValidator

    Protected WithEvents txtEmail As Global.System.Web.UI.WebControls.TextBox

    Protected WithEvents valEmailFormato As Global.System.Web.UI.WebControls.RegularExpressionValidator

    Protected WithEvents txtTelefono As Global.System.Web.UI.WebControls.TextBox

    Protected WithEvents valTelefonoFormato As Global.System.Web.UI.WebControls.RegularExpressionValidator

    Protected WithEvents txtDireccion As Global.System.Web.UI.WebControls.TextBox

    Protected WithEvents btnGuardar As Global.System.Web.UI.WebControls.Button

    Protected WithEvents btnCancelar As Global.System.Web.UI.WebControls.Button

End Class
