Imports System.Collections
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Xml.Linq
Imports Ciloci.Flee
Imports log4net
Imports Ninject
Imports org.iringtools.adapter
Imports org.iringtools.library
Imports org.iringtools.utility
Imports System.Diagnostics
Imports VBA


Namespace iRINGTools.SDK.SPPIDDataLayer
    Public Class SPPIDDataLayer : Inherits BaseDataLayer
        Implements IDataLayer2

        Private _dataObjects As List(Of IDataObject) = Nothing
        Private _projDatasource As Llama.LMADataSource = Nothing ' SPPID DataSource
        Private _lmFilters As Llama.LMAFilter = Nothing
        Private _lmCriterion As Llama.LMACriterion = Nothing

        <Inject()>
        Public Sub New(settings As AdapterSettings, kernel As IKernel)
            _settings = settings

            ' Connect to SPPID project
            Dim siteNode As String = _settings("SPPIDSiteNode")
            Dim projectStr As String = _settings("SPPIDProjectNumber")
            projectStr += "!" & projectStr
            ' per TR-88021 in SPPID 2007 SP4
            '_projDatasource = kernel.Get<ILMADataSource>();
            _projDatasource = New Llama.LMADataSource()

            _projDatasource.ProjectNumber = projectStr

        End Sub

        Public Overrides Function GetDictionary() As DataDictionary
            Dim dataDictionary As New DataDictionary()

            ' LoadConfiguration()

            Dim dataObjects As New List(Of DataObject)()
            For Each commodity As XElement In _configuration.Elements("commodities").Elements("commodity")
                'commodity

                Dim name As String = commodity.FirstAttribute.Value
                ' string name = commodity.Element("name").Value;

                Dim dataObject As New DataObject() With { _
                  .objectName = name, _
                  .keyDelimeter = "_" _
                }

                Dim keyProperties As New List(Of KeyProperty)()
                Dim dataProperties As New List(Of DataProperty)()

                For Each attribute As XElement In commodity.Element("attributes").Elements("attribute")
                    ' Name
                    Dim attributeName As String = attribute.Attribute("name").Value

                    ' is key
                    Dim isKey As Boolean = False
                    If attribute.Attribute("isKey") IsNot Nothing Then
                        [Boolean].TryParse(attribute.Attribute("isKey").Value, isKey)
                    End If

                    ' Data type: String, Integer, Real, DateTime, Picklist, Boolean
                    Dim dataTypeName As String = attribute.Attribute("datatype").Value
                    ' string dataTypeName = attribute.Attribute("dataType").Value;

                    Dim dataType__1 As DataType = DataType.[String]
                    'Enum.TryParse<DataType>(attribute.Attribute("dataType").Value, out dataType);
                    Select Case dataTypeName
                        Case "String"
                            dataType__1 = DataType.[String]
                            Exit Select
                        Case "Integer"
                            dataType__1 = DataType.Int32
                            Exit Select
                        Case "Real"
                            dataType__1 = DataType.[Double]
                            Exit Select
                        Case "DateTime"
                            dataType__1 = DataType.DateTime
                            Exit Select
                        Case "Picklist"
                            dataType__1 = DataType.[String]
                            Exit Select
                        Case "Boolean"
                            dataType__1 = DataType.[Boolean]
                            Exit Select
                        Case Else
                            dataType__1 = DataType.[String]
                            Exit Select
                    End Select

                    ' Data length
                    Dim dataLength As Integer = 0
                    If attribute.Attribute("length") IsNot Nothing Then
                        Int32.TryParse(attribute.Attribute("length").Value, dataLength)
                    End If

                    If dataLength = 0 AndAlso dataTypeName = "Picklist" Then
                        Int32.TryParse(_settings("PicklistDataLength"), dataLength)
                    End If


                    Dim dataProperty As New DataProperty() With { _
                      .propertyName = attributeName, _
                      .dataType = dataType__1, _
                      .dataLength = dataLength, _
                      .isNullable = True, _
                      .showOnIndex = False _
                    }

                    If isKey Then
                        dataProperty.isNullable = False
                        dataProperty.showOnIndex = True

                        Dim keyProperty As New KeyProperty() With { _
                          .keyPropertyName = attributeName _
                        }

                        keyProperties.Add(keyProperty)
                    End If

                    dataProperties.Add(dataProperty)
                Next

                dataObject.keyProperties = keyProperties
                dataObject.dataProperties = dataProperties

                dataObjects.Add(dataObject)
            Next

            dataDictionary.dataObjects = dataObjects

            Return dataDictionary
        End Function

        Public Overloads Overrides Function Delete(objectType As String, filter As org.iringtools.library.DataFilter) As org.iringtools.library.Response

        End Function

        Public Overloads Overrides Function Delete(objectType As String, identifiers As System.Collections.Generic.IList(Of String)) As org.iringtools.library.Response

        End Function

        Public Overloads Overrides Function [Get](objectType As String, filter As org.iringtools.library.DataFilter, pageSize As Integer, startIndex As Integer) As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)

        End Function

        Public Overloads Overrides Function [Get](objectType As String, identifiers As System.Collections.Generic.IList(Of String)) As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)

        End Function

        Public Overrides Function GetCount(objectType As String, filter As org.iringtools.library.DataFilter) As Long

        End Function

        Public Overrides Function GetIdentifiers(objectType As String, filter As org.iringtools.library.DataFilter) As System.Collections.Generic.IList(Of String)

        End Function

        Public Overrides Function GetRelatedObjects(dataObject As org.iringtools.library.IDataObject, relatedObjectType As String) As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)

        End Function

        Public Overrides Function Post(dataObjects As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)) As org.iringtools.library.Response

        End Function
    End Class
End Namespace

