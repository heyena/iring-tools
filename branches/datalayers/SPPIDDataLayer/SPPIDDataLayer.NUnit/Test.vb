Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.IO
Imports System.Linq
Imports NUnit.Framework
Imports org.iringtools.adapter
Imports org.iringtools.library
Imports org.iringtools.utility
Imports StaticDust.Configuration
Imports Ninject
Imports Ninject.Extensions.Xml

<TestFixture()>
Public Class Test
    Private _baseDirectory As String = String.Empty
    Private _kernel As IKernel = Nothing
    Private _settings As NameValueCollection
    Private _adapterSettings As AdapterSettings
    Private _sppidDataLayer As IDataLayer2
    Public Sub Test()
        ' N inject magic

        Dim ninjectSettings = New NinjectSettings() With {.LoadExtensions = False}
        _kernel = New StandardKernel(ninjectSettings)

        _kernel.Load(New XmlExtensionModule())

        _kernel.Bind(Of AdapterSettings)().ToSelf().InSingletonScope()
        _adapterSettings = _kernel.[Get](Of AdapterSettings)()

        ' Start with some generic settings
        _settings = New NameValueCollection()

        _settings("XmlPath") = ".\12345_000\"
        _settings("ProjectName") = "12345_000"
        _settings("ApplicationName") = "SPPID"

        _baseDirectory = Directory.GetCurrentDirectory()
        _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\bin"))
        _settings("BaseDirectoryPath") = _baseDirectory
        Directory.SetCurrentDirectory(_baseDirectory)

        _adapterSettings.AppendSettings(_settings)

        ' Add our specific settings
        Dim appSettingsPath As String = [String].Format("{0}12345_000.SPPID.config", _adapterSettings("XmlPath"))

        If File.Exists(appSettingsPath) Then
            Dim appSettings As New AppSettingsReader(appSettingsPath)
            _adapterSettings.AppendSettings(appSettings)
        End If

        ' and run the thing
        Dim relativePath As String = [String].Format("{0}BindingConfiguration.{1}.{2}.xml", _settings("XmlPath"), _settings("ProjectName"), _settings("ApplicationName"))

        ' Ninject Extension requires fully qualified path.
        Dim bindingConfigurationPath As String = Path.Combine(_settings("BaseDirectoryPath"), relativePath)

        _kernel.Load(bindingConfigurationPath)

        _sppidDataLayer = _kernel.[Get](Of IDataLayer2)()
    End Sub
    <Test()>
    Public Sub Create()
        Dim identifiers As IList(Of String) = New List(Of String)() From { _
     "Equip-003", _
     "Equip-004" _
    }

        Dim random As New Random()
        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Create("Equipment", identifiers)

        Dim actual As Response = _sppidDataLayer.Post(dataObjects)

        If actual.Level <> StatusLevel.Success Then
            Throw New AssertionException(Utility.SerializeDataContract(Of Response)(actual))
        End If

        Assert.IsTrue(actual.Level = StatusLevel.Success)

    End Sub

End Class
