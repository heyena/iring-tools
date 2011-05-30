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
Imports Llama



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

        LoadConfiguration()

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
        Return New Response
    End Function

    Public Overloads Overrides Function Delete(objectType As String, identifiers As System.Collections.Generic.IList(Of String)) As org.iringtools.library.Response
        Return New Response
    End Function

    Public Overloads Overrides Function [Get](objectType As String, filter As org.iringtools.library.DataFilter, pageSize As Integer, startIndex As Integer) As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)

        Try
            LoadDataDictionary(objectType)

            Dim allDataObjects As IList(Of IDataObject) = LoadDataObjects(objectType)

            ' Apply filter
            If filter IsNot Nothing AndAlso filter.Expressions IsNot Nothing AndAlso filter.Expressions.Count > 0 Then
                Dim predicate = filter.ToPredicate(_dataObjectDefinition)

                If predicate IsNot Nothing Then
                    _dataObjects = allDataObjects.AsQueryable().Where(predicate).ToList()
                End If
            End If

            If filter IsNot Nothing AndAlso filter.OrderExpressions IsNot Nothing AndAlso filter.OrderExpressions.Count > 0 Then
                Throw New NotImplementedException("OrderExpressions are not supported by the CSV DataLayer.")
            End If

            'Page and Sort The Data
            If pageSize > _dataObjects.Count() Then
                pageSize = _dataObjects.Count()
            End If
            _dataObjects = _dataObjects.GetRange(startIndex, pageSize)

            ' Return _dataObjects
        Catch ex As Exception
            _logger.[Error]("Error in GetList: " & ex.ToString())

            Throw New Exception("Error while getting a list of data objects of type [" & objectType & "].", ex)
        End Try

        Return New System.Collections.Generic.List(Of org.iringtools.library.IDataObject)
    End Function

    Public Overloads Overrides Function [Get](objectType As String, identifiers As System.Collections.Generic.IList(Of String)) As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)
        Try

            LoadDataDictionary(objectType)

            Dim allDataObjects As IList(Of IDataObject) = LoadDataObjects(objectType)

            Dim expressions = FormMultipleKeysPredicate(identifiers)

            If expressions IsNot Nothing Then
                _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList()
            End If

            Return _dataObjects
        Catch ex As Exception
            _logger.[Error]("Error in GetList: " & ex.ToString())
            Throw New Exception("Error while getting a list of data objects of type [" & objectType & "].", ex)
        End Try

    End Function

    Public Overrides Function GetCount(objectType As String, filter As org.iringtools.library.DataFilter) As Long

        Dim dataObjects As IList(Of IDataObject) = [Get](objectType, filter, 0, 0)

        Return dataObjects.Count()
    End Function

    Public Overrides Function GetIdentifiers(objectType As String, filter As org.iringtools.library.DataFilter) As System.Collections.Generic.IList(Of String)
        Return New System.Collections.Generic.List(Of String)
    End Function

    Public Overrides Function GetRelatedObjects(dataObject As org.iringtools.library.IDataObject, relatedObjectType As String) As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)
        Return New System.Collections.Generic.List(Of org.iringtools.library.IDataObject)
    End Function

    Public Overrides Function Post(dataObjects As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)) As org.iringtools.library.Response
        Dim response As New Response()
        Dim objectType As String = [String].Empty

        If dataObjects Is Nothing OrElse dataObjects.Count = 0 Then
            Dim status As New Status()
            status.Level = StatusLevel.Warning
            status.Messages.Add("Nothing to update.")
            response.Append(status)
            Return response
        End If

        Try
            objectType = DirectCast(dataObjects.FirstOrDefault(), GenericDataObject).ObjectType

            LoadDataDictionary(objectType)

            Dim existingDataObjects As IList(Of IDataObject) = LoadDataObjects(objectType)

            For Each dataObject As IDataObject In dataObjects
                Dim existingDataObject As IDataObject = Nothing

                Dim identifier As String = GetIdentifier(dataObject)
                Dim predicate = FormKeyPredicate(identifier)

                If predicate IsNot Nothing Then
                    existingDataObject = existingDataObjects.AsQueryable().Where(predicate).FirstOrDefault()
                End If

                If existingDataObject IsNot Nothing Then
                    existingDataObjects.Remove(existingDataObject)
                End If

                'TODO: Should this be per property?  Will it matter?
                existingDataObjects.Add(dataObject)
            Next

            response = SaveDataObjects(objectType, existingDataObjects)

            Return response
        Catch ex As Exception
            _logger.[Error]("Error in Post: " & ex.ToString())

            Throw New Exception("Error while posting dataObjects of type [" & objectType & "].", ex)
        End Try

    End Function

    Private Function LoadDataObjects(objectType As String) As IList(Of IDataObject)
        Try

            Dim dataObject As IDataObject = New GenericDataObject() With { _
  .ObjectType = objectType _
}
            Dim dataObjects As New List(Of IDataObject)()

            LoadConfiguration()

            Dim commodityElement As XElement = GetCommodityConfig(objectType)
            Dim attributeElements As IEnumerable(Of XElement) = commodityElement.Element("attributes").Elements("attribute")
            Dim appSource As String = String.Empty
            Dim strDrawing As String
            Dim objEquipments As LMEquipments
            Dim objEquipment As LMEquipment
            Dim intCount As Long
            Dim name As String
            Dim value As String

            _lmFilters = New LMAFilter
            _lmCriterion = New LMACriterion


            Dim criteriaName As String = "getEquipments"
            _lmFilters.ItemType = "Drawing"
            _lmFilters.Criteria.AddNew(criteriaName)
            _lmFilters.Criteria.Item(criteriaName).SourceAttributeName = "Name"
            _lmFilters.Criteria.Item(criteriaName).ValueAttribute = "Test303" ' selected drawing
            _lmFilters.Criteria.Item(criteriaName).Operator = "="

            Dim drawings As New LMDrawings
            drawings.Collect(_projDatasource, Filter:=_lmFilters)
            Debug.WriteLine("Number of drawings filtered = " & drawings.Count)

            Dim drawing As LMDrawing
            For Each drawing In drawings

                strDrawing = drawing.Attributes("Name").Value
                _lmCriterion = New LMACriterion

                _lmFilters = New LMAFilter

                _lmCriterion.SourceAttributeName = "Representation.Drawing.Name"

                _lmCriterion.ValueAttribute = strDrawing

                _lmCriterion.Operator = "="

                _lmFilters.ItemType = "Equipment"

                _lmFilters.Criteria.Add(_lmCriterion)

                objEquipments = New LMEquipments

                objEquipments.Collect(_projDatasource, Filter:=_lmFilters)

                intCount = 1
                If Not objEquipments Is Nothing Then

                    For Each objEquipment In objEquipments

                        _projDatasource.BeginTransaction()
                        For Each attr In drawing.Attributes
                            name = attr.name
                            If Not IsDBNull(attr.Value) Then
                                value = attr.Value
                            Else
                                value = "Null"
                            End If
                            dataObject.SetPropertyValue(name, value)
                        Next attr
                    Next
                End If
            Next drawing
            If Not IsDBNull(dataObjects) Then
                dataObjects.Add(dataObject)
            End If

            Return dataObjects
        Catch ex As Exception
            _logger.[Error]("Error in LoadDataObjects: " & ex.ToString())
            Throw New Exception("Error while loading data objects of type [" & objectType & "].", ex)
        End Try
    End Function
    Private Function FormDataObject(objectType As String, csvRow As String) As IDataObject
        Try
            Dim dataObject As IDataObject = New GenericDataObject() With { _
             .ObjectType = objectType _
            }

            Dim commodityElement As XElement = GetCommodityConfig(objectType)

            If Not [String].IsNullOrEmpty(csvRow) Then
                Dim attributeElements As IEnumerable(Of XElement) = commodityElement.Element("attributes").Elements("attribute")

                Dim csvValues As String() = csvRow.Split(","c)
                Dim index As Integer = 0
                For Each attributeElement In attributeElements

                    Dim name As String = attributeElement.Attribute("name").Value
                    Dim dataType As String = attributeElement.Attribute("datatype").Value.ToLower()
                    'string dataType = attributeElement.Attribute("dataType").Value.ToLower();

                    Dim value As String = csvValues(System.Math.Max(System.Threading.Interlocked.Increment(index), index - 1)).Trim()

                    ' if data type is not nullable, make sure it has a value
                    If Not (dataType.EndsWith("?") AndAlso value = [String].Empty) Then
                        If dataType.Contains("bool") Then
                            If value.ToUpper() = "TRUE" OrElse value.ToUpper() = "YES" Then
                                value = "1"
                            Else
                                value = "0"
                            End If
                        ElseIf value = [String].Empty AndAlso (dataType.StartsWith("int") OrElse dataType = "double" OrElse dataType = "single" OrElse dataType = "float" OrElse dataType = "decimal") Then
                            value = "0"
                        End If
                    End If

                    dataObject.SetPropertyValue(name, value)
                Next
            End If

            Return dataObject
        Catch ex As Exception
            _logger.[Error]("Error in FormDataObject: " & ex.ToString())

            Throw New Exception("Error while forming a dataObject of type [" & objectType & "] from SPPID.", ex)
        End Try
    End Function
    Private Function SaveDataObjects(objectType As String, dataObjects As IList(Of IDataObject)) As Response
        Try
            Dim response As New Response()

            ' Create data object directory in case it does not exist
            Directory.CreateDirectory(_settings("XMLPath"))
            'Directory.CreateDirectory(_settings["SPPIDFolderPath"]);

            Dim path As String = [String].Format("{0}\{1}.csv", _settings("XMLPath"), objectType)

            'TODO: Need to update file, not replace it!
            Dim writer As TextWriter = New StreamWriter(path)

            For Each dataObject As IDataObject In dataObjects
                Dim status As New Status()

                Try
                    Dim identifier As String = GetIdentifier(dataObject)
                    status.Identifier = identifier

                    Dim csvRow As List(Of String) = FormCSVRow(objectType, dataObject)

                    writer.WriteLine([String].Join(", ", csvRow.ToArray()))
                    status.Messages.Add("Record [" & identifier & "] has been saved successfully.")
                Catch ex As Exception
                    status.Level = StatusLevel.[Error]

                    Dim message As String = [String].Format("Error while posting dataObject [{0}]. {1}", dataObject.GetPropertyValue("Tag"), ex.ToString())

                    status.Messages.Add(message)
                End Try

                response.Append(status)
            Next

            writer.Close()

            Return response
        Catch ex As Exception
            _logger.[Error]("Error in LoadDataObjects: " & ex.ToString())
            Throw New Exception("Error while loading data objects of type [" & objectType & "].", ex)
        End Try
    End Function
    Private Function FormCSVRow(objectType As String, dataObject As IDataObject) As List(Of String)
        Try
            Dim csvRow As New List(Of String)()

            Dim commodityElement As XElement = GetCommodityConfig(objectType)

            Dim attributeElements As IEnumerable(Of XElement) = commodityElement.Element("attributes").Elements("attribute")
            Dim value As String = String.Empty
            For Each attributeElement In attributeElements
                Dim name As String = attributeElement.Attribute("name").Value
                value = Convert.ToString(dataObject.GetPropertyValue(name))
                csvRow.Add(value)
            Next

            Return csvRow
        Catch ex As Exception
            _logger.[Error]("Error in FormSPPIDRow: " & ex.ToString())

            Throw New Exception("Error while forming a CSV row of type [" & objectType & "] from a DataObject.", ex)
        End Try
    End Function

    Private Sub LoadConfiguration()
        If _configuration Is Nothing Then
            Dim uri As String = [String].Format("{0}Configuration.{1}.xml", _settings("XmlPath"), _settings("ApplicationName"))

            Dim configDocument As XDocument = XDocument.Load(uri)
            _configuration = configDocument.Element("configuration")
        End If
    End Sub
    Private Function GetCommodityConfig(objectType As String) As XElement
        If _configuration Is Nothing Then
            LoadConfiguration()
        End If

        Dim commodityConfig As XElement = _configuration.Elements("commodities").Elements("commodity").Where(Function(o) o.FirstAttribute.Value = objectType).First()

        Return commodityConfig
    End Function



End Class


