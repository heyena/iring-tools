Imports System.Web.Mvc
Imports System.Collections.Specialized
Imports System.Collections.Generic
Imports System.Configuration
Imports iRINGTools.SDK.SPPIDDataLayer.SPPIDRepository
Imports org.iringtools.library

Public Class SPPIDController
    Inherits Controller
    Private _settings As NameValueCollection = Nothing
    Private _keyFormat As String = "Configuration.{0}.{1}"
    Private Property _repository() As ISPPIDRepository
        Get
            Return m__repository
        End Get
        Set(value As ISPPIDRepository)
            m__repository = value
        End Set
    End Property

    Private m__repository As ISPPIDRepository
    'private string _keyFormat = "Configuration.{0}.{1}";

    Public Sub New()
        Me.New(New SPPIDRepository())
    End Sub
    Public Sub New(repository As ISPPIDRepository)
        _settings = ConfigurationSettings.AppSettings
        _repository = repository
    End Sub
    '
    ' GET: /SPPID/

    Public Function Index() As ActionResult
        Return View()
    End Function
    Public Function UpdateConfig(form As FormCollection) As JsonResult

        ''Variable Declration ---------------------
        Dim _siteConnDataSource As String = String.Empty
        Dim _plantConnDataSource As String = String.Empty
        Dim _staggConnDataSource As String = String.Empty
        Dim connStr As String = String.Empty
        Dim plantConnStr As String = String.Empty
        Dim stageConnStr As String = String.Empty
        Dim _plantDicConnOracle As String = String.Empty
        Dim _PIDDicConnOracle As String = String.Empty
        Dim _PIDConnStr As String = String.Empty

        ''Set Value of Data Source ---------------------
        If form("dbInstance").ToUpper() = "DEFAULT" Then
            _siteConnDataSource = form("dbServer")
        Else
            _siteConnDataSource = String.Format("{0}\{1}", form("dbServer"), form("dbInstance"))
        End If
        If form("dbplantInstance").ToUpper() = "DEFAULT" Then
            _plantConnDataSource = form("dbplantServer")
        Else
            _plantConnDataSource = String.Format("{0}\{1}", form("dbplantServer"), form("dbplantInstance"))
        End If
        If form("dbstageInstance").ToUpper() = "DEFAULT" Then
            _staggConnDataSource = form("dbstageServer")
        Else
            _staggConnDataSource = String.Format("{0}\{1}", form("dbstageServer"), form("dbstageInstance"))
        End If


        ''Create connection string for PID Database ---------------------
        If form("dbProvider").ToUpper().Contains("MSSQL") Then
            connStr = [String].Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", _siteConnDataSource, form("dbName"), form("dbUserName"), form("dbPassword"))
            plantConnStr = [String].Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", _plantConnDataSource, form("dbplantName"), form("dbplantUserName"), form("dbplantPassword"))
        Else
            connStr = [String].Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}", form("dbServer"), form("portNumber"), form("serName"), form("dbInstance"), form("dbUserName"), _
             form("dbPassword"))

            plantConnStr = [String].Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}", form("dbServer"), form("portNumber"), form("serName"), form("dbInstance"), form("dbOraPlantUserName"), _
             form("dbOraPlantPassword"))
            _plantDicConnOracle = [String].Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}", form("dbServer"), form("portNumber"), form("serName"), form("dbInstance"), form("dbPlantDataDicUserName"), _
             form("dbPlantDataDicPassword"))



            _PIDConnStr = [String].Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}", form("dbServer"), form("portNumber"), form("serName"), form("dbInstance"), form("dbPIDUserName"), _
             form("dbPIDPassword"))
            _PIDDicConnOracle = [String].Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}", form("dbServer"), form("portNumber"), form("serName"), form("dbInstance"), form("dbPIDDataDicUserName"), _
             form("dbPIDDataDicPassword"))

            connStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=NDHST5005)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=NDHPTST)));User ID=FOS_SITE;Password=FOS_SITE"
            plantConnStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=NDHST5005)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=NDHPTST)));User ID=RUSSELCITY_PILOT;Password=RUSSELCITY_PILOT"
            _plantDicConnOracle = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=NDHST5005)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=NDHPTST)));User ID=RUSSELCITY_PILOTD;Password=RUSSELCITY_PILOTD"

            _PIDConnStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=NDHST5005)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=NDHPTST)));User ID=RUSSELCITY_PILOTPID;Password=RUSSELCITY_PILOTPID"
            _PIDDicConnOracle = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=NDHST5005)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=NDHPTST)));User ID=RUSSELCITY_PILOTPIDD;Password=RUSSELCITY_PILOTPIDD"


        End If



        ''Create connection string for Staging Database ---------------------
        stageConnStr = [String].Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", _staggConnDataSource, form("dbstageName"), form("dbstageUserName"), form("dbstagePassword"))
        stageConnStr = "Data Source=NDHD06670\SQLEXPRESSW;Initial Catalog=PW_iRing_Staging;User ID=sa;Password=manager"

        Dim configurations As New SPPIDConfiguration With {.PlantConnectionString = plantConnStr, .SiteConnectionString = connStr, .StagingConnectionString = stageConnStr, .PIDConnectionString = _PIDConnStr, .PIDDataDicConnectionString = _PIDDicConnOracle, .PlantDataDicConnectionString = _plantDicConnOracle}


        ''Call repository method which gave call to service---------------------
        Dim response As List(Of String) = _repository.UpdateConfig(form("scope"), form("app"), form("_datalayer"), configurations)


        If response.Contains("ERROR") Then
            Dim inds As Integer = response(1).LastIndexOf("<message>")
            Dim msg As String = response(1).Substring(inds).Replace("<message>", "").Replace("</message>", Environment.NewLine).Replace("</messages></response>", "")
            Return Json(Convert.ToString(New With { _
              .success = False _
            }) & msg, JsonRequestBehavior.AllowGet)
        End If

        Return Json(New With {.success = response}, JsonRequestBehavior.AllowGet)
    End Function

    Public Function GetConfiguration(form As FormCollection) As ActionResult

        Dim key As String = String.Format(_keyFormat, form("scope"), form("app"))

        If Session(key) Is Nothing Then
            Session(key) = _repository.GetConfiguration(form("scope"), form("app"))
        End If
        Dim SPPIDConfiguration As SPPIDConfiguration = DirectCast(Session(key), SPPIDConfiguration)

        Return Json(SPPIDConfiguration, JsonRequestBehavior.AllowGet)

    End Function

    Public Function DBDictionary(form As FormCollection) As ActionResult
        Try
            Dim dbDict As DatabaseDictionary = _repository.GetDBDictionary(form("scope"), form("app"))
            Return Json(dbDict, JsonRequestBehavior.AllowGet)
        Catch e As Exception
            '_logger.[Error](e.ToString())
            Throw e
        End Try
    End Function

    Public Function DBObjects(form As FormCollection) As ActionResult
        Try
            Dim dbObjects__1 As List(Of JsonTreeNode) = _repository.GetDBObjects(form("scope"), form("app"), form("dbProvider"), form("dbServer"), form("dbInstance"), form("dbName"), _
             form("dbSchema"), form("dbUserName"), form("dbPassword"), form("tableNames"), form("portNumber"), form("serName"))

            Return Json(dbObjects__1, JsonRequestBehavior.AllowGet)
        Catch e As Exception
            ' _logger.[Error](e.ToString())
            Throw e
        End Try
    End Function

    Public Function Trees(form As FormCollection) As ActionResult
        Try
            Dim response As String = String.Empty

            response = _repository.SaveDBDictionary(form("scope"), form("app"), form("tree"))

            If response IsNot Nothing AndAlso response.ToUpper().Contains("ERROR") Then
                Dim inds As Integer = response.ToUpper().IndexOf("<MESSAGE>")
                Dim inde As Integer = response.ToUpper().IndexOf(";")
                Dim msg As String = response.Substring(inds + 9, inde - inds - 13)
                Return Json(Convert.ToString(New With { _
                 Key .success = False _
                }) & msg, JsonRequestBehavior.AllowGet)
            End If
            Return Json(New With { _
             Key .success = True _
            }, JsonRequestBehavior.AllowGet)
        Catch e As Exception

            Throw e
        End Try
    End Function
End Class