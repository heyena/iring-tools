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
    Function UpdateConfig(scope As String, application As String, datalayer As String, configurations As SPPIDConfiguration) As List(Of String)
    Function GetConfiguration(scope As String, application As String) As SPPIDConfiguration
    Function GetDBDictionary(scope As String, application As String) As DatabaseDictionary
    Function GetDBObjects(scope As String, application As String, dbProvider As String, dbServer As String, dbInstance As String, dbName As String, _
     dbSchema As String, dbUserName As String, dbPassword As String, tableNames As String, portNumber As String, serName As String) As List(Of JsonTreeNode)
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
    Private Property _hibernateclient() As WebHttpClient
        Get
            Return m__hibernateclient
        End Get
        Set(value As WebHttpClient)
            m__hibernateclient = value
        End Set
    End Property
    Private m__hibernateclient As WebHttpClient

    <Inject()> _
    Public Sub New()
        _settings = New AdapterSettings()
        _settings.AppendSettings(ConfigurationSettings.AppSettings)
        _client = New WebHttpClient(_settings("AdapterServiceUri"))
        _hibernateclient = New WebHttpClient(_settings("NHibernateServiceURI"))
    End Sub

    Private Function InitializeProvider(_configuration As SPPIDConfiguration) As SPPIDProvider
        If _provider Is Nothing Then

            _provider = New SPPIDProvider(_configuration)
        End If

        Return _provider
    End Function

    Public Function UpdateConfig(scope As String, application As String, datalayer As String, configurations As SPPIDConfiguration) As List(Of String) Implements ISPPIDRepository.UpdateConfig
        Dim requestMessages As New List(Of MultiPartMessage)()

        requestMessages.Add(New MultiPartMessage() With { _
          .name = "DataLayer", _
          .message = datalayer, _
          .type = MultipartMessageType.FormData _
        })

        requestMessages.Add(New MultiPartMessage() With { _
          .name = "Configuration", _
          .message = Utility.Serialize(Of XElement)(Utility.SerializeToXElement(configurations), True), _
          .type = MultipartMessageType.FormData _
        })


        Dim retVal As String = _client.PostMultipartMessage(String.Format("/{0}/{1}/configure", scope, application), requestMessages, True)

        Dim objTableName As List(Of String) = Nothing
        If retVal.Contains("Success") Then
            objTableName = GetTableName(configurations.StagingConnectionString)
        Else
            objTableName = New List(Of String)
            objTableName.Add("ERROR")
            objTableName.Add(retVal)
        End If

        Return objTableName
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

    ' use appropriate icons especially node with children
    Public Function GetDBObjects(scope As String, application As String, dbProvider As String, dbServer As String, dbInstance As String, dbName As String, _
     dbSchema As String, dbUserName As String, dbPassword As String, tableNames As String, portNumber As String, serName As String) As List(Of JsonTreeNode) Implements ISPPIDRepository.GetDBObjects
        Dim dbObjectNodes As New List(Of JsonTreeNode)()


        Dim obj As New SPPIDDataLayer(_settings)

        Dim hasDBDictionary = False

        Dim uri = [String].Format("/{0}/{1}/objects", scope, application)

        Dim request As New Request()
        request.Add("dbProvider", dbProvider)
        request.Add("dbServer", dbServer)
        request.Add("portNumber", portNumber)
        request.Add("dbInstance", dbInstance)
        request.Add("dbName", dbName)
        request.Add("dbSchema", dbSchema)
        request.Add("dbUserName", dbUserName)
        request.Add("dbPassword", dbPassword)
        request.Add("tableNames", tableNames)
        request.Add("serName", serName)

        Dim dataObjects As List(Of DataObject) = _hibernateclient.Post(Of Request, List(Of DataObject))(uri, request, True)


        Try
            Dim dbDictionary As DatabaseDictionary = GetDBDictionary(scope, application)

            If dbDictionary IsNot Nothing Then
                If dbDictionary.dataObjects.Count > 0 Then
                    hasDBDictionary = True
                End If
            End If
        Catch generatedExceptionName As Exception
            hasDBDictionary = False
        End Try

        For Each dataObject As DataObject In dataObjects
            Dim keyPropertiesNode As New JsonTreeNode() With { _
              .text = "Keys", _
              .type = "keys", _
              .expanded = True, _
              .iconCls = "folder", _
              .leaf = False, _
              .children = New List(Of JsonTreeNode)() _
            }

            Dim dataPropertiesNode As New JsonTreeNode() With { _
              .text = "Properties", _
              .type = "properties", _
              .expanded = True, _
              .iconCls = "folder", _
              .leaf = False, _
              .children = New List(Of JsonTreeNode)() _
            }

            Dim relationshipsNode As New JsonTreeNode() With { _
              .text = "Relationships", _
              .type = "relationships", _
              .expanded = True, _
              .iconCls = "folder", _
              .leaf = False, _
              .children = New List(Of JsonTreeNode)() _
            }

            ' create data object node
            Dim dataObjectNode As New JsonTreeNode() With { _
              .text = dataObject.tableName, _
              .type = "dataObject", _
              .iconCls = "treeObject", _
              .leaf = False, _
              .children = New List(Of JsonTreeNode)() From { _
              keyPropertiesNode, _
              dataPropertiesNode, _
              relationshipsNode _
             }, _
              .properties = New Dictionary(Of String, String)() From { _
              {"objectNamespace", "org.iringtools.adapter.datalayer.proj_" & scope & "." & application}, _
              {"objectName", dataObject.objectName}, _
              {"keyDelimiter", dataObject.keyDelimeter} _
             } _
            }

            ' add /data property nodes
            For Each dataProperty As DataProperty In dataObject.dataProperties
                Dim properties As New Dictionary(Of String, String)() From { _
                 {"columnName", dataProperty.columnName}, _
                 {"propertyName", dataProperty.propertyName}, _
                 {"dataType", dataProperty.dataType.ToString()}, _
                 {"dataLength", dataProperty.dataLength.ToString()}, _
                 {"nullable", dataProperty.isNullable.ToString()}, _
                 {"showOnIndex", dataProperty.showOnIndex.ToString()}, _
                 {"numberOfDecimals", dataProperty.numberOfDecimals.ToString()}, _
                 {"isHidden", dataProperty.isHidden.ToString()} _
                }

                If dataObject.isKeyProperty(dataProperty.propertyName) AndAlso Not hasDBDictionary Then
                    properties.Add("keyType", dataProperty.keyType.ToString())

                    Dim keyPropertyNode As New JsonTreeNode() With { _
                      .text = dataProperty.columnName, _
                      .type = "keyProperty", _
                      .properties = properties, _
                      .iconCls = "treeKey", _
                      .leaf = True _
                    }

                    keyPropertiesNode.children.Add(keyPropertyNode)
                Else
                    Dim dataPropertyNode As New JsonTreeNode() With { _
                      .text = dataProperty.columnName, _
                      .type = "dataProperty", _
                      .iconCls = "treeProperty", _
                      .leaf = True, _
                      .hidden = True, _
                      .properties = properties _
                    }

                    dataPropertiesNode.children.Add(dataPropertyNode)
                End If
            Next

            dbObjectNodes.Add(dataObjectNode)
        Next

        Return dbObjectNodes
    End Function

#Region "Private Methods"

    Private Function GetTableName(_stageConStr As String) As List(Of String)

        Dim obj As New SPPIDDataLayer(_settings)
        Dim tablename As List(Of String) = Nothing

        Try
            tablename = obj.LoadDataTable(_stageConStr)

        Catch generatedExceptionName As Exception

        End Try

        Return tablename

    End Function

#End Region
End Class
