Option Explicit On
Option Compare Text

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Reflection
Imports System.Text.RegularExpressions

Public Module Common

#Region " Constants "

    Public Const nl As String = vbCrLf
    Public Const tb As String = "    "
    Public Const tb2 As String = tb & tb
    Public Const tb3 As String = tb2 & tb
    Public Const nltb As String = nl & tb
    Public Const nltb2 As String = nltb & tb
    Public Const nltb3 As String = nltb2 & tb

#End Region

#Region " Enumerations "

    Public Enum SPSchemaType

        SITE = 0
        SPAPLANT = 1
        DATA_DICTIONARY = 2
        SPPID = 3
        SPPIDDATA_DICTIONARY = 4

    End Enum

    ''' <summary>
    ''' Note: the order that these clauses are assigned is important to building a query
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum SQLClause

        TableDef = -2
        StagingName = -1
        QueryName = 0
        [Declare] = 1
        [Set] = 2
        [Select] = 3
        Into = 4
        [From] = 5
        [Where] = 6
        GroupBy = 7
        [Having] = 8
        OrderBy = 9

    End Enum

#End Region

#Region " Structures "

    ''' <summary>
    ''' Represents a uniquely defined table or column name
    ''' </summary>
    ''' <remarks>Also provided functionality for determining whether a column value should be quoted or not</remarks>
    Public Structure SQLUnique

#Region " Variables "
        Dim _type As SqlDbType
        Dim _column As String
        Dim _table As String
        Dim _schema As String
        Private IsInitialized As Boolean

#End Region

#Region " Properties "

        Public ReadOnly Property UniqueName As String
            Get
                Return _schema & "." & _table & IIf(_column = "", "", "." & _column)
            End Get
        End Property

        ''' <summary>
        ''' Returns the data type. 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>The data type for tables is 'structured'</remarks>
        Public ReadOnly Property DataType As SqlDbType
            Get
                Return _type
            End Get
        End Property

        Public ReadOnly Property ColumnName As String
            Get
                Return _column
            End Get
        End Property

        Public ReadOnly Property TableName As String
            Get
                Return _table
            End Get
        End Property

        Public ReadOnly Property SchemaName As String
            Get
                Return _schema
            End Get
        End Property

#End Region

#Region " Instantiation "

        Public Sub New(schemaName As String, tableName As String, Optional columnName As String = "", _
                       Optional DataType As SqlDbType = SqlDbType.Structured)

            _schema = schemaName
            _table = tableName
            _column = columnName
            _type = DataType

        End Sub

#End Region

#Region " Public Methods "

        Public Function IsQuotable() As Boolean

            Select Case _type

                Case SqlDbType.Text, SqlDbType.NText, SqlDbType.Char, SqlDbType.Date,
                    SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset,
                    SqlDbType.DateTimeOffset, SqlDbType.Time, SqlDbType.UniqueIdentifier,
                    SqlDbType.VarChar, SqlDbType.NVarChar, SqlDbType.Xml

                    Return True

                Case Else : Return False

            End Select

        End Function

        ''' <summary>
        ''' determines if a value of a particular type should be quoted or not
        ''' </summary>
        ''' <param name="SQLDataType"></param>
        ''' <returns></returns>
        ''' <remarks>This function is not very robust - should check to verify that the string represents something that
        ''' can actually come from information_schema.columns. If not, an error should be thrown</remarks>
        Public Shared Function IsQuotable(ByVal SQLDataType As String) As Boolean

            Dim i As Integer

            If SQLDataType.Length < 3 Then Return False

            ' the datatype name should appear at the beginning. if it contains any additional information in parens, 
            ' remove this section
            i = InStr(SQLDataType, "(")
            If i > 0 Then SQLDataType = Trim(Mid(SQLDataType, 1, i - 1))

            Select Case SQLDataType

                Case "text", "ntext", "char", "date", "datetime", "datetime2", "datetimeoffset",
                    "time", "uniqueidentifier", "varchar", "nvarchar", "xml"

                    Return True

                Case Else : Return False

            End Select

        End Function

#End Region

    End Structure

#End Region

#Region " Extension Methods "

    ''' <summary>
    ''' Assemble the query according to the order of the sql clauses defined by the SQLClause Enumeration
    ''' Empty clauses and clauses consisting of only the clause start word are skipped
    ''' </summary>
    ''' <param name="Parts"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension()> _
    Public Function BuildQuery(Parts As Dictionary(Of SQLClause, String),
                               ByRef QueryText As String,
                               Optional TextVariables As IEnumerable(Of XElement) = Nothing,
                               Optional ReplacementValues As Dictionary(Of String, String) = Nothing,
                               Optional SelectInto As Boolean = False
                               ) As String

        Dim exists As Boolean
        Dim clause As String = ""
        Dim sb As New StringBuilder
        Dim qParts As Array = [Enum].GetValues(GetType(SQLClause))
        Dim varName As String
        Dim varValue As String = ""
        Dim varKey As String = ""
        Dim pat As String

        For Each part As SQLClause In qParts

            ' skip the query names while building the query
            If part < SQLClause.Declare Then Continue For
            If part = SQLClause.Into AndAlso Not SelectInto Then Continue For

            exists = Parts.TryGetValue(part, clause)
            If exists AndAlso clause.UnpaddedCount > (Len(part.ToString) + 4) Then sb.Append(clause & nl & nl)

        Next

        ' look for any text strings requiring replacement
        QueryText = sb.ToString

        Try

            If TextVariables IsNot Nothing Then

                ' not really sure at this point whether we should indicate an error or warning if substitutions are not provided.
                ' the StagingConfiguration schema supports a "value" property where default values can be provided, so it's not 
                ' necessarily an error

                ' use the textVariables collection to replace only where necessary
                For Each e As XElement In TextVariables

                    varName = e.Attribute("name").Value
                    varKey = Parts(SQLClause.QueryName) & "." & varName
                    exists = ReplacementValues.TryGetValue(varKey, varValue)
                    varKey = "!All." & varName
                    If Not exists Then exists = ReplacementValues.TryGetValue(varKey, varValue)

                    ' use the default value if there is nothing else
                    If Not exists Then varValue = e.Attribute("value").Value

                    ' this pattern attempts to isolate the varaible to "whole word only" by screening out variable names followed
                    ' by characters legal for use in SQL identifiers.  This is really only a potential
                    ' problem if there are variable names, one of which is a subset of another, and the longer variable is not 
                    ' processed first. Since we are not ordering the replacement, this method should screen out the vast majority
                    ' of cases where this could pose a problem. 
                    pat = varName & "([^a-zA-Z0-9_\-\@\#\$])"
                    QueryText = Regex.Replace(QueryText, pat, varValue & "$1", RegexOptions.IgnoreCase)
                    'sb = sb.Replace(varName, varValue)

                Next

                ' QueryText = sb.ToString

            End If

        Catch ex As Exception
            Return "Fail: (BuildQuery) " & ex.Message
        End Try

        Return "Pass"

    End Function

    <Extension()> _
    Public Function GetDataTypeString(Row As SQLSchemaDS.SchemaColumnsRow) As String

        Dim s As String

        Select Case Row.DataType

            Case "numeric", "decimal"
                s = Row.DataType & "(" & Row.NumericPrecision & ", " & Row.NumericScale & ")"

            Case "varchar", "nvarchar", "char", "nchar", "binary", "varbinary"

                If Row.CharMaxLength > -1 Then
                    s = Row.DataType & "(" & Row.CharMaxLength & ")"
                Else
                    s = Row.DataType & "(MAX)"
                End If

            Case Else
                s = Row.DataType

        End Select

        Return s

    End Function

    <Extension()> _
    Public Function SetDeclarationValues(Parts As Dictionary(Of SQLClause, String), Declarations As IEnumerable(Of XElement), _
                                    ValueDictionary As Dictionary(Of String, String), _
                                    Optional SuppressWarnings As Boolean = False) As String

        Dim val As String = ""
        Dim s2 As String = ""
        Dim exists As Boolean
        Dim rVal As New StringBuilder
        Dim varName As String
        Dim s As New StringBuilder
        Dim quoteIt As Boolean
        Dim dType As String
        Dim qName As String

        Try

            qName = Parts(SQLClause.QueryName)

            ' use the declarations to determine what values need to be set
            For Each e As XElement In Declarations

                varName = e.Attribute("name").Value
                dType = e.Attribute("datatype").Value

                ' look for a value provided specifically for this query
                exists = ValueDictionary.TryGetValue(qName & "." & varName, val)

                If Not exists Then

                    ' look for a variable provided for any any query
                    exists = ValueDictionary.TryGetValue("!All." & varName, val)

                    ' use the default value if a value has not been provided
                    If Not exists Then val = e.Attribute("value").Value

                End If

                ' if no datatype is provided, then quote the value if it's non-numeric
                ' This basically creates a softening on the StagingConfiguration schema requirement; this may or may
                ' not be a good idea
                If dType = "" Then
                    quoteIt = Not IsNumeric(val)
                Else
                    quoteIt = SQLUnique.IsQuotable(dType)
                End If

                If Not exists Then

                    If Not SuppressWarnings Then

                        If rVal.Length < 1 Then rVal.Append("Warn: ")
                        rVal.Append(nltb & "Variable '" & varName & "' was not provided with a value; the default will be used")

                    End If

                End If

                s.Append(nltb & varName & "=" & IIf(quoteIt, "'", "") & val & IIf(quoteIt, "'", ""))

            Next

            If s.Length > 0 Then s.Insert(0, "SET ")
            exists = Parts.TryGetValue(SQLClause.Set, s2)

            If exists Then
                Parts(SQLClause.Set) = s.ToString
            Else
                Parts.Add(SQLClause.Set, s.ToString)
            End If

        Catch ex As Exception
            Return "Fail: (" & MethodBase.GetCurrentMethod.Name & "): " & ex.Message
        End Try

        Return "Pass"

    End Function

    ''' <summary>
    ''' returns the number of ascii characters a-zA-Z-_0-9 and any non-leading spaces
    ''' excludes any leading tabs, spaces, and new line characters
    ''' </summary>
    ''' <param name="CharSequence"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension()> _
    Public Function UnpaddedCount(CharSequence As String) As Integer

        Dim ct As Integer
        Dim uct As Integer = 0
        Dim seqR As IEnumerable(Of Char)

        ct = Len(CharSequence)

        ' count any leading undesirables
        For Each c As Char In CharSequence

            If c = vbCr OrElse c = vbCrLf OrElse c = " " OrElse c = vbTab Then
                uct += 1
            Else
                Exit For
            End If

        Next

        seqR = CharSequence.Reverse

        ' count any trailing undesirables
        For Each c As Char In seqR

            If c = vbCr OrElse c = vbCrLf OrElse c = " " OrElse c = vbTab Then
                uct += 1
            Else
                Exit For
            End If

        Next

        Return ct - uct

    End Function

#End Region

#Region " Common Methods "

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="QueryNode"></param>
    ''' <param name="ColumnsDV"></param>
    ''' <param name="TablesDV"></param>
    ''' <param name="SchemaSubstitutions"></param>
    ''' <param name="QueryParts"></param>
    ''' <param name="Replacements"></param>
    ''' <param name="Declarations"></param>
    ''' <param name="CommonServerName">If not blank, this is name is prepended to each schema.table source in the FROM clause</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetQueryParts(ByVal QueryNode As XElement,
                                    ByVal ColumnsDV As DataView,
                                    ByVal TablesDV As DataView,
                                    ByVal SchemaSubstitutions As Dictionary(Of String, String),
                                    ByRef QueryParts As Dictionary(Of SQLClause, String),
                                    ByRef Replacements As IEnumerable(Of XElement),
                                    ByRef Declarations As IEnumerable(Of XElement),
                                    Optional ByVal CommonServerName As String = "", Optional ByVal SiteDatabaseName As String = "") As String

        Dim dec, s, f, w, g, h, o, i, t As New StringBuilder
        Dim tabWidthAlias As Integer = 50
        Dim l As Integer = 50
        Dim queryTables As IQueryable(Of XElement)
        Dim tablesX As IEnumerable(Of XElement)
        Dim relations As IEnumerable(Of XElement)
        Dim filters As IEnumerable(Of XElement)
        Dim sorts As IEnumerable(Of XElement)
        Dim sourceAliasMap As New Dictionary(Of String, SQLUnique)
        Dim fieldAliasMap As New Dictionary(Of String, SQLUnique)
        Dim d As String = "."
        Dim q As New StringBuilder
        Dim tmpX As XElement = Nothing
        Dim tmpStr As String
        Dim source As String
        Dim colName As String
        Dim stagingFieldName As String
        Dim exists As Boolean
        Dim sourceAlias As String = ""
        Dim sourceUnique As SQLUnique = Nothing
        Dim quoteTheValue As Boolean = False
        Dim subs As Dictionary(Of String, String) = SchemaSubstitutions
        Dim conj As String = ""
        Dim svNm As String = ""
        Dim col As SQLSchemaDS.SchemaColumnsRow
        Dim lastSource As String = ""
        Dim dataType As String
        Dim colNull As String
        Dim isExpression As Boolean

        ' init
        If CommonServerName <> "" Then svNm = "[" & CommonServerName & "]."

        Try

            ' fetch the SQL Schema information
            TablesDV.Sort = ""

            ' initialize the query part strings
            s.Append("SELECT ") : f.Append("FROM ") : t.Append("CREATE TABLE ")
            tmpX = QueryNode.Element("variables")

            ' Query Names ************************************************************************************************
            QueryParts.Add(SQLClause.QueryName, QueryNode.Attribute("name").Value)
            QueryParts.Add(SQLClause.StagingName, QueryNode.Attribute("stagingDestinationName").Value)
            t.Append(QueryNode.Attribute("stagingDestinationName").Value & " (" & nltb)

            ' DELCARE clause ************************************************************************************************

            ' fetch declarations
            Declarations = _
                From var In tmpX...<declaration> _
                Select var

            For Each de As XElement In Declarations

                If de IsNot Declarations.First Then dec.Append(",")
                dec.Append(de.Attribute("name").Value & tb2)
                dec.Append(de.Attribute("datatype").Value)

                If de Is Declarations.Last Then
                    dec.Append(";")
                Else
                    dec.Append(nltb)
                End If

            Next

            If dec.Length > 0 Then dec.Insert(0, "DECLARE " & nltb)
            QueryParts.Add(SQLClause.Declare, dec.ToString)

            ' Fetch textReplacements  ***************************************************************************************

            ' fetch text replacements
            Replacements = _
                From var In tmpX...<textReplacement> _
                Select var

            ' INTO clause ***************************************************************************************************

            i.Append(QueryNode.Attribute("stagingDestinationName").Value)
            If i.Length > 0 Then i.Insert(0, "INTO ")
            QueryParts.Add(SQLClause.Into, i.ToString)


            ' FROM clause ***************************************************************************************************

            ' fetch all tables
            tablesX = _
                From tb In QueryNode...<source> _
                Select tb
            queryTables = tablesX.AsQueryable

            For Each e As XElement In tablesX

                ' look for an alias mapping for this table
                exists = sourceAliasMap.TryGetValue(e.Attribute("alias").Value, sourceUnique)

                ' create one and add it to the dictionary if it doesn't exist
                If Not exists Then

                    sourceAlias = e.Attribute("alias").Value

                    If sourceAlias = "" Then
                        sourceAlias = e.Attribute("schema").Value & d & e.Attribute("name").Value
                    End If

                    ' skip deriving the schema; it will be a textReplacement
                    If subs Is Nothing _
                        OrElse e.Attribute("schema").Value.StartsWith("!@~") _
                        OrElse e.Attribute("schema").Value.StartsWith("@") Then
                        sourceAliasMap.Add(sourceAlias, New SQLUnique(e.Attribute("schema").Value, e.Attribute("name").Value))
                    Else

                        ' derive the correct schema for the table from schema substitutions dictionary 
                        'exists = [Enum].TryParse(e.Attribute("schema").Value, True, schemaType)

                        If e.Attribute("alias").Value = "" AndAlso subs IsNot Nothing Then
                            sourceAlias = subs(e.Attribute("schema").Value) & d & e.Attribute("name").Value
                        End If

                        sourceAliasMap.Add(sourceAlias, New SQLUnique(subs(e.Attribute("schema").Value), e.Attribute("name").Value))

                    End If

                End If

                f.Append(nltb)

                If e Is tablesX.First Then
                    f.Append(tb3)
                Else
                    If tablesX.Count > 1 Then f.Append(LCase(e.Attribute("joinType").Value) & " join ")
                End If
                If (SiteDatabaseName <> "") Then
                    source = svNm & SiteDatabaseName & "." & sourceAliasMap(sourceAlias).UniqueName
                Else
                    source = svNm & sourceAliasMap(sourceAlias).UniqueName
                End If

                l = IIf((Len(source) + Len(tb)) > (tabWidthAlias + 1), Len(source) + Len(tb2), tabWidthAlias)
                f.Append(LSet(source, l))
                If e.Attribute("alias").Value <> "" Then f.Append("as " & sourceAlias)

                If e IsNot tablesX.First Then

                    f.Append(nltb2 & " on " & tb)

                    relations = _
                        From r In e...<relation>
                        Select r

                    If relations IsNot Nothing Then

                        For Each r As XElement In relations

                            conj = r.Attribute("conjunction").Value
                            If conj <> "" Then f.Append(nltb3 & conj & " ")
                            f.Append(r.Attribute("leftSource").Value & d & _
                                     r.Attribute("leftField").Value & _
                                     r.Attribute("operator").Value)

                            tmpStr = r.Attribute("rightField").Value

                            If r.Attribute("joinToText") = "True" Then

                                tmpStr = tmpStr.Replace("'", "''")
                                f.Append("'" & tmpStr & "'")

                            Else
                                f.Append(r.Attribute("rightSource").Value & "." & r.Attribute("rightField").Value & " ")
                            End If

                        Next

                    End If

                End If

            Next

            QueryParts.Add(SQLClause.From, f.ToString)

            ' SELECT and TableDef clauses ***********************************************************************************

            ' fetch selection modifiers
            Dim sels As IEnumerable(Of XElement) = QueryNode.Descendants("selection")

            ' update the selection string with any selection restrictions
            For Each e As XElement In sels : s.Append(e.Attribute("value").Value & " ") : Next
            s.Append(nltb)

            ' fetch all fields
            Dim fields As IEnumerable(Of XElement) = _
                QueryNode.Element("fields").Descendants("field")

            ' add fields to the selection string and ddl
            For Each e As XElement In fields

                source = e.Attribute("source").Value
                colName = e.Attribute("name").Value
                stagingFieldName = e.Attribute("alias").Value
                isExpression = Not (e.Attribute("expression") Is Nothing OrElse e.Attribute("expression").Value = "")

                If source <> lastSource Then

                    lastSource = source
                    If e IsNot fields.First Then s.Append(nl)

                End If

                ' update the select clause
                If isExpression Then : tmpStr = e.Attribute("expression").Value
                Else : tmpStr = source & d & "[" & colName & "]"
                End If

                If e IsNot fields.First Then tmpStr = "," & tmpStr

                l = IIf((Len(tmpStr) + Len(tb)) > (tabWidthAlias + 1), Len(tmpStr) + Len(tb2), tabWidthAlias)
                s.Append(nltb & LSet(tmpStr, l))

                If stagingFieldName = "" Then
                    stagingFieldName = colName
                Else
                    s.Append("as " & stagingFieldName)
                End If

                If isExpression Then

                    ' expressions may provide a datatype hint; if not, use nvarchar(max)
                    If e.Attribute("datatype").Value = "" Then
                        dataType = "nvarchar(MAX)"
                    Else
                        ' ToDo - it would be wise to verify this is a valid datatype here to catch typos
                        dataType = e.Attribute("datatype").Value
                    End If

                    colNull = " null"

                Else

                    ' look up the data type of the column if this is not an expression
                    exists = sourceAliasMap.TryGetValue(source, sourceUnique)

                    If exists Then

                        ColumnsDV.RowFilter = "TableSchema='" & sourceUnique.SchemaName & "' " & _
                        "and TableName='" & sourceUnique.TableName & "' " & _
                        "and ColumnName='" & colName & "'"

                        If ColumnsDV.Count <> 1 Then
                            Throw New InvalidExpressionException("The column '" & _
                                sourceUnique.UniqueName & d & colName & "' is not a " & _
                                "valid uniquely identified column; the query cannot be built")
                        End If

                        col = ColumnsDV(0).Row
                        dataType = col.GetDataTypeString
                        colNull = IIf(col.IsNullable = "Yes", " null", " not null")

                    Else : Throw New InvalidExpressionException("The source value '" & source & _
                        "' is not a valid uniquely identified table or table reference; the query cannot be built")

                    End If

                End If

                If e IsNot fields.First Then t.Append(nltb & ",")
                t.Append(stagingFieldName & " " & dataType & colNull)

            Next

            t.Append(")")
            QueryParts.Add(SQLClause.TableDef, t.ToString)
            QueryParts.Add(SQLClause.Select, s.ToString)

            ' WHERE clause ***************************************************************************************************
            filters = QueryNode.Element("filters").Descendants

            For Each fil As XElement In filters

                ' add conjunction
                If fil.Attribute("conjunction").Value <> "" Then
                    w.Append(tb & fil.Attribute("conjunction").Value & tb)
                End If

                ' add begining parentheses
                If fil.Attribute("preParenCount").Value <> "" AndAlso fil.Attribute("preParenCount").Value <> "0" Then
                    w.Append(StrDup(CInt(fil.Attribute("preParenCount").Value), "("))
                End If

                ' decide whether to quote the value or not by checking to see if it is a variable or
                ' if the field's data type makes it necessary. Variables are never quoted; however string substitutions may still be
                tmpStr = fil.Attribute("filterValue").Value

                If tmpStr.StartsWith("@") Then
                    quoteTheValue = False
                Else

                    ' look up the data type of the column
                    exists = sourceAliasMap.TryGetValue(fil.Attribute("source").Value, sourceUnique)

                    If exists Then

                        ColumnsDV.RowFilter = "TableSchema='" & sourceUnique.SchemaName & "' " & _
                        "and TableName='" & sourceUnique.TableName & "' " & _
                        "and ColumnName='" & fil.Attribute("fieldName").Value & "'"

                        If ColumnsDV.Count <> 1 Then
                            Throw New InvalidExpressionException("The column '" & _
                                sourceUnique.UniqueName & d & fil.Attribute("fieldName").Value & "' is not a " & _
                                "valid uniquely identified column; the query cannot be built")
                        End If

                        col = ColumnsDV(0).Row
                        quoteTheValue = SQLUnique.IsQuotable(col.DataType)

                    Else : Throw New InvalidExpressionException("The source value '" & fil.Attribute("source").Value & _
                        "' is not a valid uniquely identified table or table reference; the query cannot be built")

                    End If

                End If

                ' add filter clause
                tmpStr = tmpStr.Replace("'", "''")
                w.Append(fil.Attribute("source").Value & d & fil.Attribute("fieldName").Value)
                w.Append(fil.Attribute("operator").Value & IIf(quoteTheValue, "'", ""))
                w.Append(tmpStr & IIf(quoteTheValue, "'", ""))

                ' add ending parentheses
                If fil.Attribute("postParenCount").Value <> "" AndAlso fil.Attribute("postParenCount").Value <> "0" Then
                    w.Append(StrDup(CInt(fil.Attribute("postParenCount").Value), ")"))
                End If

                w.Append(nltb)

            Next

            If w.Length > 0 Then w.Insert(0, "WHERE " & nltb)
            QueryParts.Add(SQLClause.Where, w.ToString)

            ' HAVING and GROUP BY clause ****************************************************************************************
            ' NOTE: this section is not yet implemented - its unclear when or if these clauses will ever be used to
            ' extract data from SPPID

            ' ORDER BY clause ***************************************************************************************************
            sorts = QueryNode.Element("sorts").Descendants

            For Each sort As XElement In sorts

                If sort IsNot sorts.First Then o.Append(tb & ",")
                o.Append(IIf(sort.Attribute("source").Value = "", "", sort.Attribute("source").Value))
                o.Append(sort.Attribute("fieldName").Value)
                o.Append(IIf(sort.Attribute("sortDirection").Value = "", "", sort.Attribute("sortDirection").Value))

            Next

            If o.Length > 0 Then o.Insert(0, "ORDER BY " & nltb)
            QueryParts.Add(SQLClause.OrderBy, o.ToString)

        Catch ex As Exception
            Return "Fail: " & ex.Message
        End Try

        Return "Pass"

    End Function

#End Region


End Module
