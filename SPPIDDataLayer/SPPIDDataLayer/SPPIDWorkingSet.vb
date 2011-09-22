Option Explicit On
Option Compare Text

Imports System.Data.SqlClient

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

#End Region

#Region " Properties "

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

#End Region

#Region " Constructors "

    Public Sub New(ProjectConnection As SqlConnection,
                   SiteConnection As SqlConnection,
                   StagingConnection As SqlConnection,
                   SiteDataQuery As XElement)

        'SPQueries = New SmartPlantDBQueries
        _CommonDataDS = New SQLSchemaDS
        _StagingServerInCommon = (ProjectConnection.DataSource = StagingConnection.DataSource)
        _CommonServerName = IIf(_StagingServerInCommon, ProjectConnection.DataSource, "")

        _SchemaSubstitutions = New Dictionary(Of String, String)
        _siteDataTA = New SQLSchemaDSTableAdapters.SiteDataTableAdapter
        _tablesTA = New SQLSchemaDSTableAdapters.SchemaTablesTableAdapter
        _columnsTA = New SQLSchemaDSTableAdapters.SchemaColumnsTableAdapter

        GetBaselineSchema(SiteConnection, ProjectConnection.Database, SiteDataQuery)

        _tablesTA.Connection = ProjectConnection
        _columnsTA.Connection = ProjectConnection

        _tablesDT = _CommonDataDS.SchemaTables
        _tablesTA.Fill(_tablesDT)
        _tablesDV = _tablesDT.DefaultView

        _columnsDT = _CommonDataDS.SchemaColumns
        _columnsTA.fill(_columnsDT)
        _columnsDV = _columnsDT.DefaultView

    End Sub

#End Region


#Region " Public Methods "

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
            '_siteDataDV = _siteDataDT.DefaultView

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
                      queryParts, replacements, declarations)

        ' the only SET necessary should be the database name
        setClause = "SET @ProjectDBName='" & ProjectDBname & "'" & nltb
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
            SetSchemaType(schemaTp, SiteDR.Username)
            SchemaSubstitutions.Add(SiteDR.SP_Schema_Type, SiteDR.Username)

        Next

    End Sub

#End Region




End Class
