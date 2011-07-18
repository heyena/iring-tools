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

Public Class LamaFactory

    Private _lmFilters As Llama.LMAFilter = Nothing
    Private _lmCriterion As Llama.LMACriterion = Nothing

    'Public Shared Function Llama.LMADataSource CreateDataSource(string siteNode, string projectStr)
    '    Dim ds As Llama.LMADataSource = New Llama.LMADataSource()
    '    ds.ProjectNumber = projectStr;
    '    ds.set_SiteNode(siteNode);
    '    Return ds

    Public Shared Function CreateDataSource(ByVal siteNode As String, ByVal projectStr As String) As LMADataSourceSubstitute
        Dim ds As LMADataSourceSubstitute = New LMADataSourceSubstitute()
        ds.ProjectNumber = projectStr
        ds.set_SiteNode(siteNode)
        Return ds
    End Function

    Public Function LoadDataObjects(objectType As String, commodityElement As XElement, projDatasource As Llama.LMADataSource) As IList(Of IDataObject)
        Try

            Dim dataObject As IDataObject = New GenericDataObject() With {.ObjectType = objectType}
            Dim dataObjects As New List(Of IDataObject)()

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
            drawings.Collect(projDatasource, Filter:=_lmFilters)
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
            '_logger.[Error]("Error in LoadDataObjects: " & ex.ToString())
            Throw New Exception("Error while loading data objects of type [" & objectType & "].", ex)
        End Try
    End Function
End Class

'Had to use this as I did not have the Llama.dll available
'Should be the Llama.LMADataSource instead
Public Class LMADataSourceSubstitute

    Public Property ProjectNumber() As String

    Public Sub set_SiteNode(ByVal siteNode As String)

    End Sub

End Class