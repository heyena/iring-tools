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
Imports System.Reflection

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
        _settings("ExecutingAssemblyName") = Assembly.GetExecutingAssembly().GetName().Name

        'TO-Do Temporary settings
        ' _settings("ExecutingAssemblyName") = "SPPIDDataLayer"

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

    _sppidDataLayer = New iRINGTools.SDK.SPPIDDataLayer.SPPIDDataLayer(_adapterSettings)
    '_sppidDataLayer = _kernel.[Get](Of IDataLayer2)()
    End Sub
  '<Test()>
  'Public Sub Create()
  '    Dim identifiers As IList(Of String) = New List(Of String)() From { _
  ' "E5E3A74C7A0F431AB5069EA1BCD0407D"
  '}

  '    Dim random As New Random()
  '    Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Create("Equipment", identifiers)

  '    For Each dataObject As IDataObject In dataObjects
  '        dataObject.SetPropertyValue("Name", "PT-" & random.[Next](2, 10))
  '        dataObject.SetPropertyValue("Drawing_DateCreated", DateTime.Today)
  '    Next

  '    Dim actual As Response = _sppidDataLayer.Post(dataObjects)

  '    If actual.Level <> StatusLevel.Success Then
  '        Throw New AssertionException(Utility.SerializeDataContract(Of Response)(actual))
  '    End If

  '    Assert.IsTrue(actual.Level = StatusLevel.Success)

    'End Sub

  <Test()>
  Public Sub GetObjects()
    'THIS ID IS DIFFERENT FOR EACH TEST DATABASE!
    Dim identifiers As IList(Of String) = New List(Of String)() From { _
 "27D058CBC5CB4ABB8B256D9B10193313"
}

        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Get("Equipment", identifiers)

        Console.WriteLine("Object Count: " + dataObjects.Count().ToString())
        For Each dataObject As IDataObject In dataObjects
            Debug.WriteLine(dataObject.GetPropertyValue("SP_ID"), "Equipment")
        Next


  End Sub

    <Test()>
    Public Sub GetCountWithFilters()

    Dim dataFilter As New DataFilter() With {.Expressions = New List(Of Expression)() From { _
          New Expression() With { _
            .PropertyName = "Drawing_DocumentCategory", _
            .RelationalOperator = RelationalOperator.EqualTo, _
            .Values = New Values() From { _
            "PipingDocuments" _
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

    <Test()>
    Public Sub GetDictionary()
        Dim benchmark As DataDictionary = Nothing

        Dim dictionary As DataDictionary = _sppidDataLayer.GetDictionary()

        Assert.IsNotNull(dictionary)

        Dim path As String = [String].Format("{0}DataDictionary.{1}.{2}.xml", _adapterSettings("XmlPath"), _settings("ProjectName"), _adapterSettings("ApplicationName"))

        Utility.Write(Of DataDictionary)(dictionary, path, True)
    End Sub
End Class

