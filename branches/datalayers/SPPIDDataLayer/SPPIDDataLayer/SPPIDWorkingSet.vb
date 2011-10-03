﻿Option Explicit On
Option Compare Text

Imports System.Data.SqlClient
Imports System.IO
Imports log4net

Public Class SPPIDWorkingSet

#Region " Variables "

    Private _conn As SqlConnection

    Private _CommonDataDS As SQLSchemaDS
    Private _siteDataDT As SQLSchemaDS.SiteDataDataTable
    Private _siteDataTA As SQLSchemaDSTableAdapters.SiteDataTableAdapter
    Private _siteDataDV As DataView
    Private _tablesDT As SQLSchemaDS.SchemaTablesDataTable
    Private _tablesTA As SQLSchemaDSTableAdapters.SchemaTablesTableAdapter
    Private _tablesDV As DataView
    Private _columnsDT As SQLSchemaDS.SchemaColumnsDataTable
    Private _columnsTA As SQLSchemaDSTableAdapters.SchemaColumnsTableAdapter
    Private _columnsDV As DataView
    Private _SPAPLANTSchemaName As String
    Private _DATA_DICTIONARYSchemaName As String
    Private _SPPIDSchemaName As String
    Private _SPPIDDATA_DICTIONARYSchemaName As String
    Private _SITESchemaName As String
    'Private SPQueries As SmartPlantDBQueries
    Private _SchemaSubstitutions As Dictionary(Of String, String)
    Private _StagingServerInCommon As Boolean
    Private _CommonServerName As String
    Private _StagingConfigurationDoc As XDocument
    Private _textReplacementsMap As Dictionary(Of String, String)
    Private _queryVariablesMap As Dictionary(Of String, String)
    Private _logger As ILog
    Private _StagingDataBaseName As String

#End Region

#Region " Properties "

    Public ReadOnly Property TextReplacementMap As Dictionary(Of String, String)
        Get
            Return _textReplacementsMap
        End Get
    End Property

    Public ReadOnly Property QueryVariableMap As Dictionary(Of String, String)
        Get
            Return _queryVariablesMap
        End Get
    End Property

    Public ReadOnly Property CommonServerName() As String
        Get
            Return _CommonServerName
        End Get
    End Property

    Public ReadOnly Property StagingServerInCommon() As Boolean
        Get
            Return _StagingServerInCommon
        End Get
    End Property

    Public ReadOnly Property Connection As SqlConnection
        Get
            Return _conn
        End Get
    End Property

    Public ReadOnly Property Tables As SQLSchemaDS.SchemaTablesDataTable
        Get
            Return _tablesDT
        End Get
    End Property

    Public ReadOnly Property Columns As SQLSchemaDS.SchemaColumnsDataTable
        Get
            Return _columnsDT
        End Get
    End Property

    Public ReadOnly Property TablesView As DataView
        Get
            Return _tablesDV
        End Get
    End Property

    Public ReadOnly Property ColumnsView(Schema As String, Table As String) As DataView
        Get
            Return New DataView(_columnsDT _
                                , "TableSchema='" & Schema & "' and TableName='" & Table & "'" _
                                , "ColumnName" _
                                , DataViewRowState.CurrentRows)

        End Get
    End Property

    Public ReadOnly Property ColumnsView() As DataView
        Get
            Return _columnsDT.DefaultView
        End Get
    End Property

    Public ReadOnly Property SchemaName(SchemaType As SPSchemaType) As String
        Get
            Select Case SchemaType
                Case SPSchemaType.SPAPLANT
                    Return _SPAPLANTSchemaName
                Case SPSchemaType.DATA_DICTIONARY
                    Return _DATA_DICTIONARYSchemaName
                Case SPSchemaType.SPPID
                    Return _SPPIDSchemaName
                Case SPSchemaType.SPPIDDATA_DICTIONARY
                    Return _SPPIDDATA_DICTIONARYSchemaName
                Case SPSchemaType.SITE
                    Return _SITESchemaName
                Case Else : Return ""
            End Select
        End Get
    End Property

    Public ReadOnly Property SchemaSubstitutions() As Dictionary(Of String, String)
        Get
            Return _SchemaSubstitutions
        End Get
    End Property

    Public ReadOnly Property SiteDatabaseName() As String
        Get
            Return _StagingDataBaseName
        End Get
    End Property

#End Region

#Region " Constructors "

    Public Sub New(ProjectConnection As SqlConnection,
                   SiteConnection As SqlConnection,
                   StagingConnection As SqlConnection,
                   StagingConfigurationPath As String,
                   ByRef Logger As ILog, PlantConnection As SqlConnection)

        Dim SiteDataQuery As XElement = Nothing


        'SPQueries = New SmartPlantDBQueries
        _CommonDataDS = New SQLSchemaDS
        _StagingServerInCommon = (ProjectConnection.DataSource = StagingConnection.DataSource)
        _CommonServerName = IIf(_StagingServerInCommon, ProjectConnection.DataSource, "")
        _logger = Logger

        _SchemaSubstitutions = New Dictionary(Of String, String)
        _siteDataTA = New SQLSchemaDSTableAdapters.SiteDataTableAdapter
        _tablesTA = New SQLSchemaDSTableAdapters.SchemaTablesTableAdapter
        _columnsTA = New SQLSchemaDSTableAdapters.SchemaColumnsTableAdapter
        _StagingConfigurationDoc = XDocument.Load(StagingConfigurationPath)
        _textReplacementsMap = New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
        _queryVariablesMap = New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
        SetProjectVariablesAndReplacements(ProjectConnection)

        GetQueryByName("!SiteData", SiteDataQuery)
        GetBaselineSchema(SiteConnection, PlantConnection.Database, SiteDataQuery)

        '_tablesTA.Connection = ProjectConnection
        '_columnsTA.Connection = ProjectConnection

        _tablesTA.Connection = PlantConnection
        _columnsTA.Connection = PlantConnection

        _tablesDT = _CommonDataDS.SchemaTables
        _tablesTA.Fill(_tablesDT)
        _tablesDV = _tablesDT.DefaultView

        _columnsDT = _CommonDataDS.SchemaColumns
        _columnsTA.Fill(_columnsDT)
        _columnsDV.RowFilter = Nothing
        _columnsDV = _columnsDT.DefaultView

    End Sub

#End Region


#Region " Public Methods "

    ''' <summary>
    ''' Fetch the named query from the staging configuration XDocument
    ''' </summary>
    ''' <param name="QueryName"></param>
    ''' <param name="QueryNode"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetQueryByName(ByVal QueryName As String, ByRef QueryNode As XElement) As String

        Dim doc As XDocument = _StagingConfigurationDoc

        Try

            ' fetch the site data query
            Dim q As IEnumerable(Of XElement) = _
            From el In doc...<query>
            Select el
            Where el.Attribute("name").Value = QueryName

            QueryNode = q.First

        Catch ex As Exception
            Return "Fail: " & ex.Message
        End Try

        Return "Pass"

    End Function

#End Region

#Region " Private Methods "

    Private Sub SetSchemaType(SchemaType As SPSchemaType, Value As String)

        Select Case SchemaType
            Case SPSchemaType.SPAPLANT
                _SPAPLANTSchemaName = Value
            Case SPSchemaType.DATA_DICTIONARY
                _DATA_DICTIONARYSchemaName = Value
            Case SPSchemaType.SPPID
                _SPPIDSchemaName = Value
            Case SPSchemaType.SPPIDDATA_DICTIONARY
                _SPPIDDATA_DICTIONARYSchemaName = Value
            Case SPSchemaType.SITE
                _SITESchemaName = Value
        End Select

    End Sub

    ''' <summary>
    ''' Derive the site schema, and from that, query the site to get the schema for the given database schema information
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GetBaselineSchema(SiteConnection As SqlConnection, ProjectDBname As String, ByVal SiteDataQuery As XElement)

        Dim SiteDR As SQLSchemaDS.SiteDataRow
        Dim TablesDR As SQLSchemaDS.SchemaTablesRow
        Dim drv As DataRowView
        Dim queryText As String = ""
        Dim schemaTp As SPSchemaType
        Dim rVal As Boolean
        Dim queryParts As New Dictionary(Of SQLClause, String)
        Dim replacements As IEnumerable(Of XElement) = Nothing
        Dim declarations As IEnumerable(Of XElement) = Nothing
        Dim setClause As String

        Try

            ' fetch the table information for the SITE database
            _tablesTA.Connection = SiteConnection
            _tablesDT = _CommonDataDS.SchemaTables
            _tablesTA.Fill(_tablesDT)
            _tablesDV = _tablesDT.DefaultView
            _StagingDataBaseName = _tablesTA.Connection.Database

            ' fetch the column information fro the SITE database
            _columnsTA.Connection = SiteConnection
            _columnsDT = _CommonDataDS.SchemaColumns
            _columnsTA.Fill(_columnsDT)
            _columnsDV = _columnsDT.DefaultView

            ' fetch the site data specifying information about all site projects
            _siteDataTA.Connection = SiteConnection
            _siteDataDT = _CommonDataDS.SiteData
            _siteDataTA.Fill(_siteDataDT) ' note that his fetches no data; it just forces the SelectCommand to initialize so the CommandText can be
            ' set properly
            ' _siteDataDV = _siteDataDT.DefaultView

        Catch ex As InvalidOperationException
            Throw ex
        Catch ex As Exception
            Throw ex
            ' to do: Exit ungracefully if the connection string does
        End Try

        ' exit if the table is not found
        Try
            TablesDR = _tablesDT.Select("Name='T_DB_Data'").First
        Catch ex As Exception
            Throw New KeyNotFoundException("The table 'T_DB_Data' cannot be found in the SPPID site database using " & _
                                       "connection string '" & SiteConnection.ConnectionString & "'")
        End Try

        ' otherwise use the schema to form the correct query to get the SPPID schema types
        _SITESchemaName = TablesDR.Schema
        _SchemaSubstitutions.Clear()
        _SchemaSubstitutions.Add(SPSchemaType.SITE.ToString, _SITESchemaName)
        GetQueryParts(SiteDataQuery, ColumnsView, TablesView, _SchemaSubstitutions,
                      queryParts, replacements, declarations, _queryVariablesMap)

        ' the only SET necessary should be the database name
        setClause = "SET @ProjectDBName='" & ProjectDBname & "'" & nltb
        _StagingDataBaseName = ProjectDBname
        queryParts.Add(SQLClause.Set, setClause)
        queryParts.BuildQuery(queryText)

        ' fetch the site data
        Try
            ' warning to implementers - you can set the command text, but do not attempt to replace the command itself;
            ' setting the command will NOT update the private CommandCollection, and the command will automatically be
            ' overridden when Fill (or the default GetData) is called.
            _siteDataTA.Adapter.SelectCommand.CommandText = queryText

        Catch ex As Exception
            Throw New InvalidExpressionException("The site data query '" & queryText & "' is malformed")
        End Try

        _siteDataTA.ClearBeforeFill = True
        _CommonDataDS.EnforceConstraints = False
        _siteDataTA.Fill(_siteDataDT)
        _siteDataDV = New DataView(_siteDataDT, "", "SP_Schema_Type", DataViewRowState.CurrentRows)

        ' set the project-specific schema set for this project
        For Each drv In _siteDataDV

            SiteDR = drv.Row
            rVal = [Enum].TryParse(SiteDR.SP_Schema_Type, True, schemaTp)
            SetSchemaType(schemaTp, SiteDR.UserName)
            SchemaSubstitutions.Add(SiteDR.SP_Schema_Type, SiteDR.UserName)

        Next

    End Sub

    ''' <summary>
    ''' Fetches the variable values and text replacement values from the Staging configuration
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function SetProjectVariablesAndReplacements(ProjectConn As SqlConnection) As String

        Dim vars As IEnumerable(Of XElement)
        Dim key As String
        Dim v As String = ""
        Dim n As String
        Dim found As Boolean

        Try

            ' This function makes the assumption that project variables and replacements are provided within the StagingConfiguration file
            vars = _StagingConfigurationDoc...<projectVariables>...<assignment>

            For Each var As XElement In vars

                v = var.Attribute("value").Value
                n = var.Attribute("name").Value

                ' ignore variables where !NoValue is set (indicating that, although the variable is listed, a value is not provided
                ' and ignore "empty lines" in the variable set. An empty line is indicated by an assignment with no name
                If v = "!NoValue" OrElse n = "" Then Continue For

                key = "!" & var.Attribute("query").Value & "." & var.Attribute("name").Value
                found = _queryVariablesMap.TryGetValue(key, v)
                If Not found Then _queryVariablesMap.Add("!" & var.Attribute("query").Value & "." & var.Attribute("name").Value, var.Attribute("value").Value)

            Next

            ' The variable @ProjectDBName should generally be set from the Project Connection information; if it already exists, then it is 
            ' being overridden from the calling function
            found = _queryVariablesMap.TryGetValue("!!All.@ProjectDBName", v)

            If Not found Then : _queryVariablesMap.Add("!!All.@ProjectDBName", ProjectConn.Database)
            Else

                If v <> ProjectConn.Database Then
                    _logger.Warn("The project connection database name differs from the default database name used by by the staging configuration queries")
                End If

            End If

            vars = _StagingConfigurationDoc...<projectReplacements>...<assignment>

            For Each var As XElement In vars

                v = var.Attribute("value").Value
                n = var.Attribute("name").Value
                If v = "!NoValue" OrElse n = "" Then Continue For

                key = "!" & var.Attribute("query").Value & "." & var.Attribute("name").Value
                found = _textReplacementsMap.TryGetValue(key, v)
                _textReplacementsMap.Add("!" & var.Attribute("query").Value & "." & var.Attribute("name").Value, var.Attribute("value").Value)

            Next

        Catch ex As Exception
            Return "Fail: " & ex.Message
        End Try

        Return "Pass"

    End Function

#End Region




End Class
