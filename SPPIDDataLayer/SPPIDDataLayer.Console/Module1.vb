Imports org.iringtools.adapter
Imports Ninject
Imports System.Collections.Specialized
Imports org.iringtools.library
Imports Ninject.Extensions.Xml
Imports System.IO
Imports System.Configuration

Module Module1
  Private _baseDirectory As String = String.Empty
  Private _kernel As IKernel = Nothing
  Private _settings As NameValueCollection
  Private _adapterSettings As AdapterSettings
  Private _sppidDataLayer As IDataLayer2

  Sub Main()

    'Dim dataLayer As iRINGTools.SDK.SPPIDDataLayer.SPPIDDataLayer = New iRINGTools.SDK.SPPIDDataLayer.SPPIDDataLayer(New AdapterSettings(), Nothing)

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
      'Dim appSettings As New AppSettingsReader(appSettingsPath)
      '_adapterSettings.AppendSettings(appSettings)
    End If

    ' and run the thing
    Dim relativePath As String = [String].Format("{0}BindingConfiguration.{1}.{2}.xml", _settings("XmlPath"), _settings("ProjectName"), _settings("ApplicationName"))

    ' Ninject Extension requires fully qualified path.
    Dim bindingConfigurationPath As String = Path.Combine(_settings("BaseDirectoryPath"), relativePath)

    _kernel.Load(bindingConfigurationPath)

    _sppidDataLayer = _kernel.[Get](Of IDataLayer2)()

  End Sub

End Module
