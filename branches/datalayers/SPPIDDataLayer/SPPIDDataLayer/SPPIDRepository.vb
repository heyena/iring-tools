Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Configuration
Imports StaticDust.Configuration

Imports org.iringtools.adapter
Imports org.iringtools.library
Imports org.iringtools.utility
Imports Ninject
Imports System.Xml.Linq
Imports System.Collections.Specialized
Imports System.IO
Imports System.Net



Public Interface ISPPIDRepository
    Function UpdateConfig(scope As String, application As String, datalayer As String, configurations As SPPIDConfiguration) As String
    Function GetConfiguration(scope As String, application As String) As SPPIDConfiguration
    Function GetDBDictionary(scope As String, application As String) As DatabaseDictionary
End Interface

Public Class SPPIDRepository
    Implements ISPPIDRepository
    Private Property _settings() As AdapterSettings
        Get
            Return m__settings
        End Get
        Set(value As AdapterSettings)
            m__settings = value
        End Set
    End Property
    Private m__settings As AdapterSettings
    Private Property _provider() As SPPIDProvider
        Get
            Return m__provider
        End Get
        Set(value As SPPIDProvider)
            m__provider = value
        End Set
    End Property
    Private m__provider As SPPIDProvider
    Private Property _client() As WebHttpClient
        Get
            Return m__client
        End Get
        Set(value As WebHttpClient)
            m__client = value
        End Set
    End Property
    Private m__client As WebHttpClient

    <Inject()> _
    Public Sub New()
        _settings = New AdapterSettings()
        _settings.AppendSettings(ConfigurationSettings.AppSettings)
        _client = New WebHttpClient(_settings("AdapterServiceUri"))
    End Sub

    Private Function InitializeProvider(_configuration As SPPIDConfiguration) As SPPIDProvider
        If _provider Is Nothing Then

            _provider = New SPPIDProvider(_configuration)
        End If

        Return _provider
    End Function

    Public Function UpdateConfig(scope As String, application As String, datalayer As String, configurations As SPPIDConfiguration) As String Implements ISPPIDRepository.UpdateConfig
        Dim requestMessages As New List(Of MultiPartMessage)()

        requestMessages.Add(New MultiPartMessage() With { _
          .name = "DataLayer", _
          .message = datalayer, _
          .type = MultipartMessageType.FormData _
        })

        'requestMessages.Add(New MultiPartMessage() With { _
        '  .name = "SiteConnectionString", _
        '  .message = siteConString, _
        '  .type = MultipartMessageType.FormData _
        '})
        'requestMessages.Add(New MultiPartMessage() With { _
        '  .name = "PlantConnectionString", _
        '  .message = plantConString, _
        '  .type = MultipartMessageType.FormData _
        '})
        'requestMessages.Add(New MultiPartMessage() With { _
        '  .name = "StagingConnectionString", _
        '  .message = stageConString, _
        '  .type = MultipartMessageType.FormData _
        '})

        requestMessages.Add(New MultiPartMessage() With { _
          .name = "Configuration", _
          .message = Utility.Serialize(Of XElement)(Utility.SerializeToXElement(configurations), True), _
          .type = MultipartMessageType.FormData _
        })



        _client.PostMultipartMessage(String.Format("/{0}/{1}/configure", scope, application), requestMessages)

        Return "SUCCESS"
    End Function

    Public Function GetConfiguration(scope As String, application As String) As SPPIDConfiguration Implements ISPPIDRepository.GetConfiguration

        Dim obj As SPPIDConfiguration = Nothing

        Try
            Dim element As XElement = _client.[Get](Of XElement)(String.Format("/{0}/{1}/configuration", scope, application))
            If Not element.IsEmpty Then
                obj = Utility.DeserializeFromXElement(Of SPPIDConfiguration)(element)
            End If

        Catch generatedExceptionName As Exception

        End Try

        Return obj

    End Function

    Public Function GetDBDictionary(scope As String, application As String) As DatabaseDictionary Implements ISPPIDRepository.GetDBDictionary


        Dim dbDictionary As DatabaseDictionary = New DatabaseDictionary()

        Return dbDictionary
    End Function
End Class