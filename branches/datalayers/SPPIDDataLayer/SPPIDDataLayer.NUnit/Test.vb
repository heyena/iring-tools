﻿Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.IO
Imports System.Linq
Imports NUnit.Framework
Imports org.iringtools.adapter
Imports org.iringtools.library
Imports org.iringtools.utility
Imports StaticDust.Configuration
Imports Ninject
Imports Ninject.Extensions.Xml

<TestFixture()>
Public Class Test
    Private _baseDirectory As String = String.Empty
    Private _kernel As IKernel = Nothing
    Private _settings As NameValueCollection
    Private _adapterSettings As AdapterSettings
  Private _sppidDataLayer As IDataLayer2
  Public Sub New()
    ' N inject magic

    Dim ninjectSettings = New NinjectSettings() With {.LoadExtensions = False}
    _kernel = New StandardKernel(ninjectSettings)

    _kernel.Load(New XmlExtensionModule())

    _kernel.Bind(Of AdapterSettings)().ToSelf().InSingletonScope()
    _adapterSettings = _kernel.[Get](Of AdapterSettings)()

    ' Start with some generic settings
    _settings = New NameValueCollection()

    _settings("XmlPath") = ".\12345_000\"
    _settings("ProjectName") = "12345_000"
    _settings("ApplicationName") = "SPPID"

    _baseDirectory = Directory.GetCurrentDirectory()
    _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\bin"))
    _settings("BaseDirectoryPath") = _baseDirectory
    Directory.SetCurrentDirectory(_baseDirectory)

    _adapterSettings.AppendSettings(_settings)

    ' Add our specific settings
    Dim appSettingsPath As String = [String].Format("{0}12345_000.SPPID.config", _adapterSettings("XmlPath"))

    If File.Exists(appSettingsPath) Then
      Dim appSettings As New AppSettingsReader(appSettingsPath)
      _adapterSettings.AppendSettings(appSettings)
    End If

    ' and run the thing
    Dim relativePath As String = [String].Format("{0}BindingConfiguration.{1}.{2}.xml", _settings("XmlPath"), _settings("ProjectName"), _settings("ApplicationName"))

    ' Ninject Extension requires fully qualified path.
    Dim bindingConfigurationPath As String = Path.Combine(_settings("BaseDirectoryPath"), relativePath)

    _kernel.Load(bindingConfigurationPath)

    Dim dataLayer As IDataLayer2 = New iRINGTools.SDK.SPPIDDataLayer.SPPIDDataLayer(_adapterSettings, Nothing)
    _sppidDataLayer = _kernel.[Get](Of IDataLayer2)()
  End Sub
    <Test()>
    Public Sub Create()
        Dim identifiers As IList(Of String) = New List(Of String)() From { _
     "7C73D75A0F4C4A3F8FD99D962C733BCC"
    }

        Dim random As New Random()
        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Create("Equipment", identifiers) ' Returning 28 items in a object

        For Each dataObject As IDataObject In dataObjects
            dataObject.SetPropertyValue("Adapter_ParentTag", "PT-" & random.[Next](2, 10))
            dataObject.SetPropertyValue("Drawing_DateCreated", DateTime.Today)
        Next

        Dim actual As Response = _sppidDataLayer.Post(dataObjects)

        If actual.Level <> StatusLevel.Success Then
            Throw New AssertionException(Utility.SerializeDataContract(Of Response)(actual))
        End If

        Assert.IsTrue(actual.Level = StatusLevel.Success)

    End Sub
    <Test()>
    Public Sub GetCountWithFilters()

        Dim dataFilter As New DataFilter() With {.Expressions = New List(Of Expression)() From { _
              New Expression() With { _
                .PropertyName = "Drawing_DocumentType", _
                .RelationalOperator = RelationalOperator.EqualTo, _
                .Values = New Values() From { _
                "PDT-8" _
   }
  }
 }
}


        Dim dataObjects As Long = _sppidDataLayer.GetCount("Equipment", dataFilter)

        If dataObjects = -1 Then
            Assert.IsNotNull(dataObjects)
        End If

        Assert.IsTrue(True, dataObjects.ToString())

    End Sub
End Class

