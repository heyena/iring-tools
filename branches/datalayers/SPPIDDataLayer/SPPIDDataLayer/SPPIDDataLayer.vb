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
Imports ISPClientData3



Public Class SPPIDDataLayer : Inherits BaseDataLayer
    Implements IDataLayer2

    Private _dataObjects As List(Of IDataObject) = Nothing
    Private _projDatasource As Llama.LMADataSource = Nothing ' SPPID DataSource
    Private _lmFilters As Llama.LMAFilter = Nothing
    Private _lmCriterion As Llama.LMACriterion = Nothing
    Private m_skipInternalAttributes As Boolean  ' ignore internal attributes
    Private m_skipNoDisplayAttributes As Boolean  ' ignore non-displayed attributes

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
              .keyDelimeter = "_", _
              .tableName = "tbl" + name
            }

            Dim keyProperties As New List(Of KeyProperty)()
            Dim dataProperties As New List(Of DataProperty)()

            For Each attribute As XElement In commodity.Element("attributes").Elements("attribute")
                ' Name
                Dim attributeName As String = attribute.Attribute("name").Value

                'Column Name
                Dim columnName As String = attribute.Attribute("name").Value

                ' is key
                Dim isKey As Boolean = False
                If attribute.Attribute("isKey") IsNot Nothing Then
                    [Boolean].TryParse(attribute.Attribute("isKey").Value, isKey)
                End If

                ' Data type: String, Integer, Real, DateTime, Picklist, Boolean
                Dim dataTypeName As String = attribute.Attribute("datatype").Value


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
                  .showOnIndex = False, _
                  .columnName = columnName
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

            Dim dataObjects As New List(Of IDataObject)()

            LoadConfiguration()

            Dim commodityElement As XElement = GetCommodityConfig(objectType)
            Dim attributeElements As IEnumerable(Of XElement) = commodityElement.Element("attributes").Elements("attribute")

            Dim appSource As String = String.Empty
            Dim objEquipments As LMEquipments
            Dim objEquipment As LMEquipment

            _lmCriterion = New LMACriterion
            _lmFilters = New LMAFilter

            Dim criteriaName As String = "getEquipments"

            _lmFilters.Criteria.AddNew(criteriaName)
            _lmCriterion = New LMACriterion
            _lmFilters = New LMAFilter

            _lmFilters.ItemType = "Equipment"

            objEquipments = New LMEquipments
            objEquipments.Collect(_projDatasource, Filter:=_lmFilters)

            If Not objEquipments Is Nothing Then
                For Each objEquipment In objEquipments
                    Dim dataObject As IDataObject = New GenericDataObject() With { _
.ObjectType = objectType _
}
                    fetchEquipment(objEquipment, dataObject, objectType)
                    If Not IsDBNull(dataObject) Then
                        dataObjects.Add(dataObject)
                    End If
                Next
            End If

            If Not IsDBNull(dataObjects) Then
                For Each obj In dataObjects
                    For Each attr In attributeElements
                        Dim s = obj.GetType()
                        Dim n As String = attr.Attribute("name").Value
                        Try
                            Dim name = obj.GetPropertyValue(n).GetType()
                        Catch ex As Exception
                            obj.SetPropertyValue(attr.Attribute("name").Value, "Null")
                        End Try
                    Next
                Next
            End If

            Return dataObjects
        Catch ex As Exception
            _logger.[Error]("Error in LoadDataObjects: " & ex.ToString())
            Throw New Exception("Error while loading data objects of type [" & objectType & "].", ex)
        End Try
    End Function

    Private Function SaveDataObjects(objectType As String, dataObjects As IList(Of IDataObject)) As Response
        Try
            Dim response As New Response()
            Dim equips As LMEquipments
            Dim equip As LMEquipment
            Dim equipExchanger As LMExchanger
            Dim equipMechanical As LMMechanical
            Dim equipVessel As LMVessel
            Dim equipOther As LMEquipmentOther
            Dim equipType As String
            Dim attSubclass As String
            Dim compUpdated As Boolean
            Dim rep As LMRepresentation
            Dim attr As LMAAttribute
            Dim newValue As String
            Dim attrUpdated As Boolean
            Dim i As Long
            Dim attrName As String
            Dim onlyIfNull As Boolean
            Dim skip As Boolean
            Dim numAttsUpdated As Long
            Dim spId As String
            Dim commodity As String
            Dim m_updateIfDwgOpen As Boolean

            'To-Do I don't know how to get Opened drawing, so setting default value as false.
            m_updateIfDwgOpen = False


            For Each dataobject In dataObjects
                equipType = dataobject.GetPropertyValue("ItemTypeName")
                spId = dataobject.GetPropertyValue("SP_ID")
              

                Select Case equipType
                    Case "Exchanger"
                        commodity = equipType
                        equipExchanger = _projDatasource.GetExchanger(spId)

                        '' Check for an open drawing.
                        'If Not m_updateIfDwgOpen And equipExchanger.Representations.Count > 0 Then
                        '    rep = equipExchanger.Representations.Nth(1)
                        '    skip = skipDwg(rep, errMsgs)
                        'End If

                        'If Not skip Then
                        '    ' Update each attribute from the XML if changed.
                        '    For i = 0 To updates.length - 1
                        '        getRecvAttr(updates, i, attrName, newValue, attSubclass, onlyIfNull)
                        '        writeLog(2, "Recvd attr " & CStr(i) & ": subclass '" & attSubclass & "', " & attrName & " = '" & newValue & "'")

                        '        ' See if attribute is applicable
                        '        If attSubclass = "" Or attSubclass = commodity Then
                        '            attr = equipExchanger.Attributes(attrName)

                        '            If attr Is Nothing Then
                        '                errMsgs.add("Specified " & commodity & " attribute """ & attrName & """ not found")
                        '            Else
                        '                attrUpdated = updateAttribute(attr, newValue, onlyIfNull, m_projDatasource, errMsgs)
                        '                If attrUpdated Then
                        '                    compUpdated = True
                        '                    numAttsUpdated = numAttsUpdated + 1
                        '                End If
                        '            End If
                        '        End If
                        '    Next i
                        'End If

                        'If compUpdated Then
                        '    equipExchanger.Commit()
                        'End If
                        'equipExchanger = Nothing

                    Case "Mechanical"
                        commodity = equipType
                        equipMechanical = _projDatasource.GetMechanical(spId)

                        ' Check for an open drawing.
                        If Not m_updateIfDwgOpen And equipMechanical.Representations.Count > 0 Then
                            rep = equipMechanical.Representations.Nth(1)
                            skip = skipDwg(rep, "")
                        End If

                        'If Not skip Then
                        '    ' Update each attribute from the XML if changed.
                        '    For i = 0 To updates.length - 1
                        '        getRecvAttr(updates, i, attrName, newValue, attSubclass, onlyIfNull)
                        '        writeLog(2, "Recvd attr " & CStr(i) & ": subclass '" & attSubclass & "', " & attrName & " = '" & newValue & "'")

                        '        ' See if attribute is applicable
                        '        If attSubclass = "" Or attSubclass = commodity Then
                        '            attr = equipMechanical.Attributes(attrName)

                        '            If attr Is Nothing Then
                        '                errMsgs.add("Specified " & commodity & " attribute """ & attrName & """ not found")
                        '            Else
                        '                attrUpdated = updateAttribute(attr, newValue, onlyIfNull, m_projDatasource, errMsgs)
                        '                If attrUpdated Then
                        '                    compUpdated = True
                        '                    numAttsUpdated = numAttsUpdated + 1
                        '                End If
                        '            End If
                        '        End If
                        '    Next i
                        'End If

                        'If compUpdated Then
                        '    equipMechanical.Commit()
                        'End If
                        'equipMechanical = Nothing

                    Case "Vessel"
                        commodity = equipType
                        equipVessel = _projDatasource.GetVessel(spId)

                        ' Check for an open drawing.
                        If Not m_updateIfDwgOpen And equipVessel.Representations.Count > 0 Then
                            rep = equipVessel.Representations.Nth(1)
                            ' skip = skipDwg(rep, errMsgs)
                        End If

                        'If Not skip Then
                        '    ' Update each attribute from the XML if changed.
                        '    For i = 0 To updates.length - 1
                        '        getRecvAttr(updates, i, attrName, newValue, attSubclass, onlyIfNull)
                        '        writeLog(2, "Recvd attr " & CStr(i) & ": subclass '" & attSubclass & "', " & attrName & " = '" & newValue & "'")

                        '        ' See if attribute is applicable
                        '        If attSubclass = "" Or attSubclass = commodity Then
                        '            attr = equipVessel.Attributes(attrName)

                        '            If attr Is Nothing Then
                        '                errMsgs.add("Specified " & commodity & " attribute """ & attrName & """ not found")
                        '            Else
                        '                attrUpdated = updateAttribute(attr, newValue, onlyIfNull, m_projDatasource, errMsgs)
                        '                If attrUpdated Then
                        '                    compUpdated = True
                        '                    numAttsUpdated = numAttsUpdated + 1
                        '                End If
                        '            End If
                        '        End If
                        '    Next i
                        'End If

                        'If compUpdated Then
                        '    equipVessel.Commit()
                        'End If
                        'equipVessel = Nothing

                    Case "EquipmentOther"
                        commodity = equipType
                        equipOther = _projDatasource.GetEquipmentOther(spId)

                        ' Check for an open drawing.
                        If Not m_updateIfDwgOpen And equipOther.Representations.Count > 0 Then
                            rep = equipOther.Representations.Nth(1)
                            ' skip = skipDwg(rep, errMsgs)
                        End If

                        'If Not skip Then
                        '    ' Update each attribute from the XML if changed.
                        '    For i = 0 To updates.length - 1
                        '        getRecvAttr(updates, i, attrName, newValue, attSubclass, onlyIfNull)
                        '        writeLog(2, "Recvd attr " & CStr(i) & ": subclass '" & attSubclass & "', " & attrName & " = '" & newValue & "'")

                        '        ' See if attribute is applicable
                        '        If attSubclass = "" Or attSubclass = commodity Then
                        '            attr = equipOther.Attributes(attrName)

                        '            If attr Is Nothing Then
                        '                errMsgs.add("Specified " & commodity & " attribute """ & attrName & """ not found")
                        '            Else
                        '                attrUpdated = updateAttribute(attr, newValue, onlyIfNull, m_projDatasource, errMsgs)
                        '                If attrUpdated Then
                        '                    compUpdated = True
                        '                    numAttsUpdated = numAttsUpdated + 1
                        '                End If
                        '            End If
                        '        End If
                        '    Next i
                        'End If

                        'If compUpdated Then
                        '    equipOther.Commit()
                        'End If
                        'equipOther = Nothing
                    Case "Nozzle"
                        Dim temp = _projDatasource.GetNozzle(spId)

                        Dim SP_EquipmentID As String = ""
                        For Each attr In temp.Attributes
                            If attr.Name = "SP_EquipmentID" Then
                                SP_EquipmentID = attr.Value
                            End If
                        Next
                        equip = _projDatasource.GetEquipment(SP_EquipmentID)
                        If Not m_updateIfDwgOpen And equip.Representations.Count > 0 Then
                            rep = equip.Representations.Nth(1)
                            skip = skipDwg(rep, "sd")
                        End If
                        If Not skip Then
                            ' Update each attribute from the XML if changed.
                            For i = 0 To dataObjects.LongCount() - 1
                                ' getRecvAttr(updates, i, attrName, newValue, attSubclass, onlyIfNull)
                            Next
                        End If
                    Case Else   ' shouldn't be anything else
                        ' Check for an open drawing.
                       
                        '  If Not m_updateIfDwgOpen And equip.Representations.Count > 0 Then
                        '  rep = equip.Representations.Nth(1)
                        '  skip = skipDwg(rep, "sd")
                        '  End If

                        ' If Not skip Then
                        ' Update each attribute from the XML if changed.
                        'For i = 0 To dataobject.length - 1
                        '    getRecvAttr(updates, i, attrName, newValue, attSubclass, onlyIfNull)
                        '    writeLog(2, "Recvd attr " & CStr(i) & ": subclass '" & attSubclass & "', " & attrName & " = '" & newValue & "'")

                        '    ' See if attribute is applicable
                        '    If attSubclass = "" Or attSubclass = commodity Then
                        '        attr = equip.Attributes(attrName)

                        '        If attr Is Nothing Then
                        '            errMsgs.add("Specified " & commodity & " attribute """ & attrName & """ not found")
                        '        Else
                        '            attrUpdated = updateAttribute(attr, newValue, onlyIfNull, m_projDatasource, errMsgs)
                        '            If attrUpdated Then
                        '                compUpdated = True
                        '                numAttsUpdated = numAttsUpdated + 1
                        '            End If
                        '        End If
                        '    End If
                        ' Next i
                        '  End If

                        If compUpdated Then
                            '  equip.Commit()
                        End If

                End Select
            Next



            Return response
        Catch ex As Exception
            _logger.[Error]("Error in LoadDataObjects: " & ex.ToString())
            Throw New Exception("Error while loading data objects of type [" & objectType & "].", ex)
        End Try
    End Function


    Private Function skipDwg( _
          ByRef rep As LMRepresentation, _
          ByRef errMsgs As String) As Boolean

        Dim dwg As LMDrawing
        Dim dwgNo As String
        Dim filespec As String
        Dim m_plantPath As String = getPlantPath()

        skipDwg = False

        ' Get the drawing filename. If no drawing it's in the project stockpile.
        dwg = rep.DrawingObject
        If Not dwg Is Nothing Then
            filespec = m_plantPath & dwg.Attributes("Path").Name

            ' See if file is open
            If isFileLocked(filespec) Then
                dwgNo = dwg.Attributes("DrawingNumber").Value
                'errMsgs.add("Drawing " & dwgNo & " is open")
                skipDwg = True
            End If
        End If
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

    Private Function fetchEquipment(objEquipment As LMEquipment, DataObject As IDataObject, objectType As String) As Boolean

    

        Dim fetchEquioment As Boolean
        Dim rep As LMRepresentation
        Dim drawing As LMDrawing
        Dim attr As LMAAttribute
        Dim inStockpile As Boolean
        Dim dwgId As String
        Dim spId As String
        Dim CantPossiblyBeARealName As String = "toastandjam"

        fetchEquioment = True

        ' Skip if no Representation
        If objEquipment.Representations.Count = 0 Then
            fetchEquipment = False
            Exit Function
        End If

        rep = objEquipment.Representations.Nth(1)
        drawing = rep.DrawingObject

        ' See if it's in the project or drawing stockpile.
        attr = rep.Attributes("InStockpile")
        inStockpile = attr.Value = "True"

        If inStockpile Then
            If Not drawing Is Nothing Then
                'If Not m_exposeDwgStockpile("Equipment") Then
                '    fetchEquipment = False
                '    Exit Function
                'End If
            End If
        End If

        ' Drawing attributes
        dwgId = rep.DrawingID

        ' Skip this component if querying by dwg and it's not on the first dwg.
        Dim m_queriedDrawingId = getDrawingID(dwgId)
        Dim _attr = objEquipment.Attributes("toastandjam")
        'If m_queriedByDrawing And dwgId <> m_queriedDrawingId Then
        '    fetchEquipment = False
        '    Exit Function
        'End If

        ' Representation
        For Each attr In rep.Attributes
            addAttrSP(DataObject, attr, , "Representation", , objectType)
        Next attr

        ' Commodity-specific attributes
        ' First find the subclass of this equipment
        Dim equipType As String
        equipType = objEquipment.Attributes("ItemTypeName").Value

        spId = objEquipment.Id
        ' Don't think you can expand the case attributes for just the base equipment
        Select Case equipType
            Case "Exchanger"
                Dim equipExchanger As LMExchanger
                equipExchanger = _projDatasource.GetExchanger(spId)

                ' Expand Attributes collection to include all Case properties
                attr = equipExchanger.Attributes("toastandjam")

                For Each attr In equipExchanger.Attributes
                    addAttrSP(DataObject, attr, , equipType, , objectType)
                Next attr

                equipExchanger = Nothing
            Case "Mechanical"
                Dim equipMechanical As LMMechanical
                equipMechanical = _projDatasource.GetMechanical(spId)

                ' Expand Attributes collection to include all Case properties
                attr = equipMechanical.Attributes(CantPossiblyBeARealName)

                For Each attr In equipMechanical.Attributes
                    addAttrSP(DataObject, attr, , equipType, , objectType)
                Next attr

                equipMechanical = Nothing
            Case "Vessel"
                Dim equipVessel As LMVessel
                equipVessel = _projDatasource.GetVessel(spId)

                ' Expand Attributes collection to include all Case properties
                attr = equipVessel.Attributes(CantPossiblyBeARealName)

                For Each attr In equipVessel.Attributes
                    addAttrSP(DataObject, attr, , equipType, , objectType)
                Next attr

                equipVessel = Nothing
            Case "EquipmentOther"
                Dim equipOther As LMEquipmentOther
                equipOther = _projDatasource.GetEquipmentOther(spId)

                ' Expand Attributes collection to include all Case properties
                attr = equipOther.Attributes(CantPossiblyBeARealName)

                For Each attr In equipOther.Attributes
                    addAttrSP(DataObject, attr, , equipType, , objectType)
                Next attr

                equipOther = Nothing
            Case "EquipComponent"
                'If m_skipEquipComponents Then
                '    fetchEquipment = False
                '    Exit Function
                'Else
                ' Expand Attributes collection to include all Case properties

                attr = objEquipment.Attributes(CantPossiblyBeARealName)

                For Each attr In objEquipment.Attributes
                    addAttrSP(DataObject, attr, , equipType, , objectType)
                Next attr
                'End If
            Case Else   ' shouldn't be anything else
                fetchEquipment = False
                Exit Function
        End Select

        ' Get the drawing attributes. If no drawing it's in the project stockpile.
        If drawing Is Nothing Then
            ' Fake the drawing number
            'addAttr(xmlDoc, DrawingNumberTag, StockpileTag, , TagDrawing)
            'addAttr(xmlDoc, NameTag, StockpileTag, , TagDrawing)
            'addAttr(xmlDoc, DescriptionTag, StockpileTag, , TagDrawing)
            'addAttr(xmlDoc, TitleTag, StockpileTag, , TagDrawing)
        Else
            For Each attr In drawing.Attributes
                addAttrSP(DataObject, attr, , "Drawing", True, objectType)
            Next attr
        End If

        ' Symbol
        Dim symbol = _projDatasource.GetSymbol(rep.Id)
        For Each attr In symbol.Attributes
            addAttrSP(DataObject, attr, , "Symbol", True, objectType)
        Next attr
        symbol = Nothing

        rep = Nothing

        ' Nozzle
        If objEquipment.Nozzles.Count > 0 Then
            Dim nozzle As LMNozzle
            nozzle = objEquipment.Nozzles.Nth(1)
            For Each attr In nozzle.Attributes
                addAttrSP(DataObject, attr, , "Nozzle", , objectType)
            Next attr
            nozzle = Nothing
        End If

        ' Parent Tag
        Dim parentTag As String
        If Not objEquipment.PartOfPlantItemObject Is Nothing Then
            If Not IsDBNull(objEquipment.PartOfPlantItemObject.Attributes("ItemTag").Value) Then
                parentTag = objEquipment.PartOfPlantItemObject.Attributes("ItemTag").Value
                ' addAttrSP(DataObject, "Parent", parentTag, , "Adapter")
                addAttrSP(DataObject, attr, , "Adapter", , objectType)
            End If
        End If


        Return fetchEquioment
    End Function

    Sub addAttrSP(dataObject As IDataObject, attr As LMAAttribute, Optional subclass As String = "", Optional src As String = "", _
        Optional ByVal displayedOnly As Boolean = False, Optional objectType As String = "")


        Dim useAltValue As Boolean
        Dim enumAttrs As ISPEnumeratedAttributes
        Dim attrValue As Object
        Dim intCount As Integer
        Dim value As String

        Dim commodityElement As XElement = GetCommodityConfig(objectType)
        Dim attributeElements As IEnumerable(Of XElement) = commodityElement.Element("attributes").Elements("attribute")

        attrValue = attr.Value

        Debug.WriteLine(src & "--->" & attr.Name)
        ' Skip hidden attributes
        If Not skipAttribute(attr, displayedOnly) Then
            '  If isAttrRequested(attr.Name, subclass, src, useAltValue) Then
            If useAltValue Then
                ' See if attribute has a select list.
                enumAttrs = attr.ISPAttribute.Attribution.ISPEnumAtts
                If Not enumAttrs Is Nothing Then
                    ' .Name is long value, .Description is short value
                    attrValue = enumAttrs.Item(CStr(attr.Index)).Description    ' Bin Lin 11/10/2008
                End If
            End If

            'If Not IsDBNull(attrValue) Then
            '    dataObject.SetPropertyValue(attr.Name, attrValue)
            'End If
            'End If

            '---------------
            '' Get Equipment Attributes------------------
            For Each attributeElement In attributeElements  'xml
                intCount = 0
                If (attributeElement.Attribute("name").Value = attr.Name Or attributeElement.Attribute("nativeName").Value = attr.Name) Then
                    intCount = 1
                    If Not IsDBNull(attr.Value) Then
                        value = attr.Value
                    Else
                        value = "Null"
                    End If
                    dataObject.SetPropertyValue(attributeElement.Attribute("name").Value, value)
                    Exit Sub
                End If

                'If (intCount = 0) Then
                '    dataObject.SetPropertyValue(attributeElement.Attribute("name").Value, "Null")
                'End If
            Next
        End If
        'If Not IsDBNull(DataObjects) Then
        '    DataObjects.Add(DataObjects)
        'End If
        '---------------

    End Sub

    Private Function skipAttribute( _
        ByRef attr As LMAAttribute, _
        Optional ByVal displayedOnly As Boolean = False) As Boolean

        skipAttribute = False

        Select Case attr.ISPAttribute.Attribution.Displayable.ToString()
            Case "spInternalAtt"
                skipAttribute = displayedOnly Or m_skipInternalAttributes
            Case "spNoDisplayAtt"
                skipAttribute = displayedOnly Or m_skipNoDisplayAttributes
        End Select
    End Function

    Private Function getDrawingID( _
        ByVal dwgNo As String)

        Const funcName As String = "getDrawingID"

        Dim dwgFilter As New LMAFilter
        Dim criteriaName As String

        dwgFilter.ItemType = "Drawing"

        criteriaName = "dwg"
        dwgFilter.Criteria.AddNew(criteriaName)
        dwgFilter.Criteria.Item(criteriaName).SourceAttributeName = "SP_ID"
        dwgFilter.Criteria.Item(criteriaName).ValueAttribute = dwgNo
        dwgFilter.Criteria.Item(criteriaName).Operator = "="
        dwgFilter.Criteria.Item(criteriaName).Conjunctive = True

        Dim drawings As New LMDrawings
        drawings.Collect(_projDatasource, Filter:=dwgFilter)
        If drawings.Count <> 1 Then
            Err.Raise(vbObjectError + 1, funcName, "Drawing " & dwgNo & " not found")
        End If

        getDrawingID = drawings.Nth(1).Id

        dwgFilter = Nothing
        drawings = Nothing
    End Function

    Private Function isFileLocked( _
        ByRef filespec As String) As Boolean

        ' If the file is already opened by another process and the specified type of access
        ' is not allowed the Open operation fails and an error occurs.
        On Error Resume Next
        isFileLocked = False

        Dim f As Integer
        f = FreeFile

        '  Open filespec For Binary Access Read Lock Read Write As #f

        ' Check for "Permission Denied"
        If Err.Number = 70 Then
            isFileLocked = True
        End If

        ' Close #f
    End Function

    Private Function getPlantPath()
        ' Get "Plant Path" from PlantSettings


        Dim pathFilter As New LMAFilter
        Dim criterion As New LMACriterion

        criterion.SourceAttributeName = "Name"
        criterion.ValueAttribute = "Plant Path"
        criterion.Operator = "="

        pathFilter.ItemType = "PlantSetting"
        pathFilter.Criteria.Add(criterion)

        Dim plantSettings As New LMPlantSettings
        Dim plantSetting As LMPlantSetting
        plantSettings.Collect(_projDatasource, Filter:=pathFilter)

        plantSetting = plantSettings.Nth(1)

        getPlantPath = plantSetting.Attributes("Value")

        pathFilter = Nothing
        criterion = Nothing
        plantSetting = Nothing
        plantSettings = Nothing


        Exit Function

    End Function

End Class


