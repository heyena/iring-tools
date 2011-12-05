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
        Dim connStr, plantConnStr, stageConnStr As String

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


        ''Create connection string for Site Database ---------------------
        If form("dbProvider").ToUpper().Contains("MSSQL") Then
            connStr = [String].Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", _siteConnDataSource, form("dbName"), form("dbUserName"), form("dbPassword"))
        Else
            connStr = [String].Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}", form("dbServer"), form("portNumber"), form("serName"), form("dbInstance"), form("dbUserName"), _
             form("dbPassword"))
        End If


        ''Create connection string for Plant Database ---------------------
        If form("dbplantProvider").ToUpper().Contains("MSSQL") Then
            plantConnStr = [String].Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", _plantConnDataSource, form("dbplantName"), form("dbplantUserName"), form("dbplantPassword"))
        Else
            plantConnStr = [String].Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}", form("dbplantServer"), form("dbplantportNumber"), form("dbplantserName"), form("dbplantInstance"), form("dbplantUserName"), _
             form("dbplantPassword"))
        End If

        ''Create connection string for Plant Database ---------------------
        If form("dbstageProvider").ToUpper().Contains("MSSQL") Then
            stageConnStr = [String].Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", _staggConnDataSource, form("dbstageName"), form("dbstageUserName"), form("dbstagePassword"))
        Else
            stageConnStr = [String].Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}", form("dbstageServer"), form("dbstageportNumber"), form("dbstageserName"), form("dbstageInstance"), form("dbstageUserName"), _
             form("dbstagePassword"))
        End If


        ''Set connection string parameter need to be passed to datalayer for configuration---------------------
        plantConnStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=NDHST5005)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=NDHPTST)));User ID=RUSSELCITY_PILOTPID;Password=RUSSELCITY_PILOTPID"
        connStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=NDHST5005)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=NDHPTST)));User ID=FOS_SITE;Password=FOS_SITE"
        stageConnStr = "Data Source=NDHD06670\SQLEXPRESSW;Initial Catalog=PW_iRing_Staging;User ID=sa;Password=manager"

        '  plantConnStr = "Data Source=NDHD06670\SQLEXPRESSW;Initial Catalog=SPPID_Project_Plant;User ID=sa;Password=manager"
        '  connStr = "Data Source=NDHD06670\SQLEXPRESSW;Initial Catalog=SPPID_Project;User ID=sa;Password=manager"

        Dim configurations As New SPPIDConfiguration With {.PlantConnectionString = plantConnStr, .SiteConnectionString = connStr, .StagingConnectionString = stageConnStr}


        ''Call repository method which gave call to service---------------------
        Dim success As String = _repository.UpdateConfig(form("scope"), form("app"), form("_datalayer"), configurations)


        Return Json(New With {.success = True}, JsonRequestBehavior.AllowGet)
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

End Class