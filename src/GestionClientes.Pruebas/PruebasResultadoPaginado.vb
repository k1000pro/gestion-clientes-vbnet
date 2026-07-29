Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports GestionClientes.Entidades

<TestClass()>
Public Class PruebasResultadoPaginado

    <TestMethod()>
    Public Sub TotalPaginas_EsCeroCuandoNoHayRegistros()
        Dim resultado = New ResultadoPaginado(Of String) With {.TotalRegistros = 0, .TamanoPagina = 10}

        Assert.AreEqual(0, resultado.TotalPaginas)
    End Sub

    <TestMethod()>
    Public Sub TotalPaginas_EsUnaCuandoLosRegistrosCabenJustos()
        Dim resultado = New ResultadoPaginado(Of String) With {.TotalRegistros = 10, .TamanoPagina = 10}

        Assert.AreEqual(1, resultado.TotalPaginas)
    End Sub

    <TestMethod()>
    Public Sub TotalPaginas_RedondeaHaciaArribaConUnResiduo()
        Dim resultado = New ResultadoPaginado(Of String) With {.TotalRegistros = 11, .TamanoPagina = 10}

        Assert.AreEqual(2, resultado.TotalPaginas)
    End Sub

    <TestMethod()>
    Public Sub TotalPaginas_RedondeaHaciaArribaConMuchosResiduos()
        Dim resultado = New ResultadoPaginado(Of String) With {.TotalRegistros = 95, .TamanoPagina = 10}

        Assert.AreEqual(10, resultado.TotalPaginas)
    End Sub

    <TestMethod()>
    Public Sub TotalPaginas_EsCeroSiElTamanoDePaginaEsInvalido()
        Dim resultado = New ResultadoPaginado(Of String) With {.TotalRegistros = 50, .TamanoPagina = 0}

        Assert.AreEqual(0, resultado.TotalPaginas,
            "Un tamaño de página de cero no debe provocar una división por cero.")
    End Sub

    <TestMethod()>
    Public Sub Elementos_EstaInicializadaYNoEsNothing()
        Dim resultado = New ResultadoPaginado(Of String)()

        Assert.IsNotNull(resultado.Elementos)
        Assert.AreEqual(0, resultado.Elementos.Count)
    End Sub

    <TestMethod()>
    Public Sub Pagina_ArrancaEnUnoPorOmision()
        Dim resultado = New ResultadoPaginado(Of String)()

        Assert.AreEqual(1, resultado.Pagina)
    End Sub

End Class
