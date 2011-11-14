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
        Dim _siteConnDataSource As String = String.Empty
        Dim _plantConnDataSource As String = String.Empty
        Dim _staggConnDataSource As String = String.Empty

        If form("dbInstance").ToUpper() = "DEFAULT" Then
            _siteConnDataSource = form("dbServer")
        Else
            _siteConnDataSource = String.Format("{0}\{1}", form("dbServer"), form("dbInstance"))
        End If

        If form("dbplantInstance").ToUpper() = "DEFAULT" Then
            _plantConnDataSource = form("dbServer")
        Else
            _plantConnDataSource = String.Format("{0}\{1}", form("dbplantServer"), form("dbplantInstance"))
        End If

        If form("dbstageInstance").ToUpper() = "DEFAULT" Then
            _staggConnDataSource = form("dbServer")
        Else
            _staggConnDataSource = String.Format("{0}\{1}", form("dbstageServer"), form("dbstageInstance"))
        End If



        Dim siteConn As String = String.Format("user id={0};password={1};Data Source={2};Initial Catalog={3}", form("dbUserName"), form("dbPassword"), _siteConnDataSource, form("dbName"))
        Dim plantConn As String = String.Format("user id={0};password={1};Data Source={2};Initial Catalog={3}", form("dbplantUserName"), form("dbplantPassword"), _plantConnDataSource, form("dbplantName"))
        Dim staggConn As String = String.Format("user id={0};password={1};Data Source={2};Initial Catalog={3}", form("dbstageUserName"), form("dbstagePassword"), _staggConnDataSource, form("dbstageName"))

        Dim configurations As New SPPIDConfiguration With {.PlantConnectionString = plantConn, .SiteConnectionString = siteConn, .StagingConnectionString = staggConn}


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