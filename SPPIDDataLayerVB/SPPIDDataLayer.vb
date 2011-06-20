Option Compare Text
Option Explicit On

Imports System
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
Imports SPPIDDataLayerVB.iRINGTools.SDK.SPPIDDataLayer.Interfaces

Namespace iRINGTools.SDK.SPPIDDataLayerVB

    Module m1

        Public Class SPPIDDataLayer
            Inherits BaseDataLayer
            Implements IDataLayer2

            Private _dataObjects As List(Of IDataObject) = Nothing
            Private _projDataSource As Llama.LMADataSource
            'Private _projDataSource As iLMADataSource = Nothing
            Private _LMFilters As Llama.LMAFilter = Nothing
            Private _LMCriterion As Llama.LMACriterion = Nothing

#Region " Instatiation "

            ''' <summary>
            ''' Constructor
            ''' </summary>
            ''' <param name="Settings"></param>
            ''' <param name="kernel"></param>
            ''' <remarks>"Inject" is required to deliver settings to the constructor</remarks>
            <Inject()> _
            Public Sub New(Settings As AdapterSettings, kernel As IKernel)

                _settings = Settings

                ' connect to the SPPID project
                Dim siteNode As String = _settings("SPPIDSiteNote")
                Dim projectNumber As String = _settings("SPPIDProjectNumber")

                projectNumber &= "!" & projectNumber    ' required for uniqueness or sumsuch per TR-88021 in SPPID 2007 SP4
                _projDataSource = New Llama.LMADataSource ' auto-connects to the active plant
                _projDataSource.ProjectNumber = projectNumber ' forces a connection change to the desired plant; note that this can also be done by setting the SiteNode property
                ' changing the ProjectNumber or SiteNote invalidates any existing object references to the old plant; these objects must be reinitialized or destroyed since their
                ' context will be _projDataSource


            End Sub

#End Region

#Region " BaseDataLayer overridden methods "

            ''' <summary>
            ''' Load the configuration and return an iRing DataDictionary of commodities and their elements
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetDictionary() As org.iringtools.library.DataDictionary

                Dim dataDict As New DataDictionary
                Dim dataObjects As New List(Of DataObject)
                Dim attributeName As String
                Dim isKey As Boolean
                Dim dataTypeName As String
                Dim dataTyp As DataType = DataType.String
                Dim dataLength As Integer = 0
                Dim dataProp As DataProperty
                Dim keyProp As KeyProperty
                Dim name As String
                Dim dataObj As DataObject
                Dim keyProperties As List(Of KeyProperty)
                Dim dataProperties As List(Of DataProperty)

                LoadConfiguration()

                For Each commodity As XElement In _configuration.Elements("commodities").Elements("commodity")

                    ' init
                    name = commodity.FirstAttribute.Value
                    keyProperties = New List(Of KeyProperty)
                    dataProperties = New List(Of DataProperty)
                    dataObj = New DataObject With {.keyDelimeter = "_", .objectName = name}

                    For Each Attribute As XElement In commodity.Element("attributes").Elements("attribute")

                        attributeName = Attribute.Attribute("name").Value

                        If Attribute.Attribute("isKey") Is Nothing Then
                            isKey = False
                        Else : Boolean.TryParse(Attribute.Attribute("isKey").Value, isKey)
                        End If

                        dataTypeName = Attribute.Attribute("datatype").Value
                        dataTyp = GetDataTypeByName(dataTypeName)

                        If Attribute.Attribute("length") Is Nothing Then
                            dataLength = 0
                        Else : Int32.TryParse(Attribute.Attribute("length").Value, dataLength)
                        End If

                        If dataLength = 0 AndAlso dataTypeName = "PickList" Then
                            Int32.TryParse(_settings("PicklistDataLength"), dataLength)
                        End If

                        dataProp = New DataProperty With {.propertyName = attributeName, .dataType = dataTyp, .dataLength = dataLength,
                            .isNullable = True, .showOnIndex = False}

                        dataProperties.Add(dataProp)

                        If isKey Then

                            dataProp.isNullable = False
                            dataProp.showOnIndex = True
                            keyProp = New KeyProperty
                            keyProp.keyPropertyName = attributeName
                            keyProperties.Add(keyProp)

                        End If

                    Next

                    dataObj.keyProperties = keyProperties
                    dataObj.dataProperties = dataProperties
                    dataObjects.Add(dataObj)

                Next

                dataDict.dataObjects = dataObjects
                Return dataDict

            End Function

            ''' <summary>
            ''' Loads a dataDictionary and returns a subset of objects based on the input set of filtering identifiers
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <param name="identifiers"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function [Get](objectType As String, identifiers As System.Collections.Generic.IList(Of String)) _
                                                        As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)

                Dim allDataObjects As IList(Of IDataObject)
                Dim expressions As Expression(Of Func(Of IDataObject, Boolean))

                Try

                    LoadDataDictionary(objectType)
                    allDataObjects = LoadDataObjects(objectType)
                    expressions = FormMultipleKeysPredicate(identifiers)
                    If expressions IsNot Nothing Then _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList
                    Return _dataObjects

                Catch ex As Exception

                    _logger.Error("Error in GetList: " + ex.Message)
                    Throw New Exception("Error while getting a list of data objects of type [" + objectType + "].", ex.InnerException)

                End Try

            End Function

            ''' <summary>
            ''' INCOMPLETE Returns a subset of data objects based on an iRing DataFilter, page size, and start index
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <param name="filter"></param>
            ''' <param name="pageSize"></param>
            ''' <param name="startIndex"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function [Get](objectType As String, filter As org.iringtools.library.DataFilter, pageSize As Integer, startIndex As Integer) _
                                                                                As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)

                ' ToDo: Compete the converstion of the Get function
                Dim allDataObjects As IList(Of IDataObject)

                Try

                    LoadDataDictionary(objectType)
                    allDataObjects = LoadDataObjects(objectType)
                    _LMFilters = New Llama.LMAFilter

                    With _LMFilters
                        .ItemType = "Drawing"


                    End With


                Catch ex As Exception

                End Try

            End Function

            'public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
            '{
            '    try
            '    {
            '        LoadDataDictionary(objectType);

            '        IList<IDataObject> allDataObjects = LoadDataObjects(objectType);

            '        _lmFilters = new Llama.LMAFilter();
            '        //_lmCriterion = new Llama.LMACriterion();

            '        //_lmCriterion.SourceAttributeName = "TagSuffix";
            '        //_lmCriterion.set_ValueAttribute("P");
            '        //_lmCriterion.Operator = "=";
            '        _lmFilters.ItemType = "Drawing";
            '        string criteriaName = "Test";
            '        _lmFilters.get_Criteria().AddNew(criteriaName);
            '        _lmFilters.get_Criteria().get_Item(criteriaName).SourceAttributeName = "DrawingNumber";
            '        _lmFilters.get_Criteria().get_Item(criteriaName).set_ValueAttribute("d");
            '        _lmFilters.get_Criteria().get_Item(criteriaName).Operator = "!=";

            '        //_lmFilters.set_Criteria(_lmCriterion);  //TO DO Error when trying to add criteria. 

            '        // Error Retrieving the COM class factory for 
            '       //component with CLSID  failed due to the following error: 80040154 
            '       //Class not registered (Exception from HRESULT: 0x80040154 (REGDB_E_CLASSNOTREG)).

            '        Llama.LMDrawings drawings = new Llama.LMDrawings();

            '        drawings.Collect(_projDatasource, null, null, _lmFilters);

            '        Debug.WriteLine("Number of Piperuns retrieved = " + drawings.Count);

            '        Llama.LMAAttribute attr = new Llama.LMAAttribute();



            '        #region Commented code

            '        //// Apply filter
            '        //if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
            '        //{
            '        //    var predicate = filter.ToPredicate(_dataObjectDefinition);

            '        //    if (predicate != null)
            '        //    {
            '        //        _dataObjects = allDataObjects.AsQueryable().Where(predicate).ToList();
            '        //    }
            '        //}

            '        //if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
            '        //{
            '        //    throw new NotImplementedException("OrderExpressions are not supported by the SPPID DataLayer.");
            '        //}

            '        ////Page and Sort The Data
            '        //if (pageSize > _dataObjects.Count())
            '        //    pageSize = _dataObjects.Count();
            '        //_dataObjects = _dataObjects.GetRange(startIndex, pageSize);

            '     //   return _dataObjects;
            '        #endregion       

            '        return new List<IDataObject>();

            '    }
            '    catch (Exception ex)
            '    {
            '        _logger.Error("Error in GetList: " + ex);

            '        throw new Exception(
            '          "Error while getting a list of data objects of type [" + objectType + "].",
            '          ex);
            '    }
            '}

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="configuration"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Configure(configuration As System.Xml.Linq.XElement) As org.iringtools.library.Response
                Return MyBase.Configure(configuration)
                ' ToDo: complet the Configure function
            End Function

            ''' <summary>
            ''' NOT IMPLEMENTED
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <param name="identifiers"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Create(objectType As String, identifiers As System.Collections.Generic.IList(Of String)) As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)
                Return New NotImplementedException

                ' ToDo: Complete the Complete the Create function 
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <param name="filter"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Delete(objectType As String, filter As org.iringtools.library.DataFilter) As org.iringtools.library.Response
                Return New org.iringtools.library.Response

                ' ToDo: Complete the Delete(objectType, filter) function
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <param name="identifiers"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Delete(objectType As String, identifiers As System.Collections.Generic.IList(Of String)) As org.iringtools.library.Response
                Return New org.iringtools.library.Response

                ' ToDo: Complete the Delete(objectType, identifiers) function
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="obj"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Equals(obj As Object) As Boolean
                Return MyBase.Equals(obj)

                ' ToDo: Complete the Equals function
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <param name="filter"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetCount(objectType As String, filter As org.iringtools.library.DataFilter) As Long

                ' ToDo: Complete the GetCount function
                Return 0
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetHashCode() As Integer
                Return MyBase.GetHashCode()

                ' ToDo: Complete the GetHashCode function
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <param name="filter"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetIdentifiers(objectType As String, filter As org.iringtools.library.DataFilter) As System.Collections.Generic.IList(Of String)

                ' ToDo: Complete the GetIdentifiers function
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="dataObject"></param>
            ''' <param name="relatedObjectType"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetRelatedObjects(dataObject As org.iringtools.library.IDataObject, relatedObjectType As String) As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)
                ' ToDo: Complete the GetRelatedObjects function
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="dataObjects"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Post(dataObjects As System.Collections.Generic.IList(Of org.iringtools.library.IDataObject)) As org.iringtools.library.Response
                ' ToDo: Complete the Post function
            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function ToString() As String
                Return MyBase.ToString()

                ' ToDo: Complete the ToString function
            End Function

#End Region

#Region " Private Methods "

            ''' <summary>
            ''' Returns an LMAFilter object to be used as a query. The filter is constructed from the input iRing DataFilter
            ''' </summary>
            ''' <param name="iRingFilter"></param>
            ''' <returns></returns>
            ''' <remarks>Limitations: 
            ''' LMAFilters support only AND and OR operators; either all criteria must be AND or all critera must be OR
            ''' LMACriterion do not support parentheses, so parentheis counts from DataFilte objects will be
            ''' ignored.
            ''' Only these RelationalOperators are allowed for critera: Equals, NotEquals, GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo
            ''' Because the IN RelationalOperator is not used, all values in the DataFilter Values collection other than the first will be ignored.
            ''' Although LMAFilters do support Sub-filters, these are believed to be logically OR'ed together; 
            ''' the only way a DataFilter that combines results with multiple ItemTypes can be supported is
            ''' if each ItemType entry is followed by at least one criteria; each new ItemType found will create
            ''' a new LMAFilter object that is a child of the first LMAfilter   </remarks>
            Private Function GetLlamaFilter(iRingFilter As DataFilter) As Llama.LMAFilter

                Dim ir As DataFilter = iRingFilter
                Dim f As Llama.LMAFilter
                Dim curFilter As Llama.LMAFilter = Nothing
                Dim c As Llama.LMACriterion

                Dim Expressions As List(Of org.iringtools.library.Expression)
                Dim exp As org.iringtools.library.Expression
                Dim errMsg As String = ""

                Try

                    ' no expressions means no filter
                    If ir.Expressions Is Nothing Then

                        _logger.Error("Error creating LlamaFilter: The input DataFilter contains no expressions")
                        Return Nothing

                    Else : Expressions = ir.Expressions
                    End If

                    ' The DataFilter is not a neat fit for creating LMAFilters. To make it work, we will 
                    ' interpret each Expression with a PropertyName of ItemType as a new LMAFilter

                    For Each exp In Expressions

                        If exp.PropertyName = "ItemType" Then

                            curFilter = New Llama.LMAFilter
                            curFilter.ItemType = exp.Values(0)

                            If f Is Nothing Then
                                f = curFilter
                            Else

                                If f.ChildLMAFilters Is Nothing Then
                                    f.ChildLMAFilters = New Llama.LMAFilters
                                End If

                                f.ChildLMAFilters.Add(curFilter)

                            End If

                        Else ' all other entries are criteria entries

                            c = New Llama.LMACriterion
                            c.Operator = GetOperatorFromRelationalName(exp.RelationalOperator)
                            c.SourceAttributeName = exp.PropertyName
                            c.ValueAttribute = exp.Values(0)

                            ' if this is the first criteria for a filter, set the conjunctive
                            If curFilter Is Nothing Then
                                errMsg = "Error creating Llama LMAFilter: the first Expression of a DataFilter MUST have the property name ItemType"
                                Throw New Exception()

                            End If

                            If curFilter.Criteria.Count = 0 Then : c.Conjunctive = (exp.LogicalOperator = LogicalOperator.And)
                            End If

                            curFilter.Criteria.Add(c)

                        End If

                    Next

                    Return f

                Catch exc As Exception

                    _logger.Error(IIf(errMsg = "", "Error creating Llama LMAFilter: " & exc.Message, errMsg))
                    Throw New InvalidExpressionException(IIf(errMsg = "", "Error creating Llama LMAFilter: ", errMsg), exc)

                End Try

            End Function

            Private Function GetOperatorFromRelationalName(name As String) As String

                Select Case name
                    Case "EqualTo"
                        Return "="
                    Case "NotEqualTo"
                        Return "!="
                    Case "LessThan"
                        Return "<"
                    Case "LessThanOrEqualTo"
                        Return "<="
                    Case "GreaterThan"
                        Return ">"
                    Case "GreaterThanOrEqualTo"
                        Return ">="
                    Case Else
                        Throw New NotSupportedException("The DataFilter operator " & name & " is not supported for Llama LMAFilters")
                        ' ToDo: verify what operator symbols are allows for filtering

                End Select

            End Function

            ''' <summary>
            ''' Returns a datatype corresponding to a datatype name, or the String type if the datatype name has no match
            ''' </summary>
            ''' <param name="TypeString"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function GetDataTypeByName(TypeString As String) As DataType

                Select Case TypeString

                    Case "String"
                        Return DataType.String

                    Case "Integer"
                        Return DataType.Int32

                    Case "Real"
                        Return DataType.Double

                    Case "DateType"
                        Return DataType.DateTime

                    Case "PickList"
                        Return DataType.String

                    Case "Boolean"
                        Return DataType.Boolean

                    Case Else
                        Return DataType.String

                End Select


            End Function

            ''' <summary>
            ''' Load the configuration file according to the path and application name given in the _settings
            ''' </summary>
            ''' <remarks></remarks>
            Private Sub LoadConfiguration()

                If _configuration Is Nothing Then

                    Dim uri As String = String.Format("{0}Configuration.{1}.xml", _settings("XmlPath"), _settings("ApplicationName"))
                    Dim configDocument As XDocument = XDocument.Load(uri)

                    _configuration = configDocument.Element("configuration")

                End If

            End Sub

            ''' <summary>
            ''' Fetch the first Commodity configuration element for a given Attribute
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function GetCommodityConfig(objectType As String) As XElement

                If _configuration Is Nothing Then LoadConfiguration()
                Return _configuration.Elements("commodities").Elements("commodity").Where(Function(o) o.FirstAttribute.Value = objectType).First()

            End Function

            ''' <summary>
            ''' INCOMPLETE
            ''' </summary>
            ''' <param name="objectType"></param>
            ''' <returns></returns>
            ''' <remarks>Utilizes the LamaFactory to load objects; this may need to be refactored to load the objects directly from here</remarks>
            Private Function LoadDataObjects(objectType As String) As IList(Of IDataObject)

                Dim dataObjs As List(Of IDataObject)
                Try

                    dataObjs = New List(Of IDataObject)
                    ' Call some code Neha devised


                    Return dataObjs


                Catch ex As Exception

                End Try

            End Function

#End Region

        End Class

    End Module


End Namespace

