using org.iringtools.adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using org.iringtools.library;
using org.iringtools.utility;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Configuration;

namespace AdapterService.Tests
{
   /// <summary>
   ///This is a test class for AdapterServiceTest and is intended
   ///to contain all AdapterServiceTest Unit Tests
   ///</summary>
  [TestClass()]
  public class AdapterServiceTest
  {
    private AdapterSettings _settings = null;

    public AdapterServiceTest()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);
    }

    [TestMethod()]
    public void GetDictionaryTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      DataDictionary actual = target.GetDictionary("12345_000", "ABC");
      Assert.AreEqual(1, actual.dataObjects.Count);
    }

    [TestMethod()]
    public void GetMappingTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Mapping actual = target.GetMapping("12345_000", "ABC");
      Assert.AreNotEqual(0, actual.graphMaps.Count);
    }

    [TestMethod()]
    public void ClearStoreTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.ClearAll("12345_000", "ABC");
      if (!actual.ToString().Contains("has been deleted successfully."))
      {
        throw new AssertFailedException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.IsTrue(actual.ToString().Contains("has been deleted successfully."));
    }

    [TestMethod()]
    public void RefreshTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.RefreshGraph("12345_000", "ABC", "Lines");

      if (actual.Level == StatusLevel.Error)
      {
        throw new AssertFailedException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.IsFalse(actual.Level == StatusLevel.Error);
    }

    [TestMethod()]
    public void RefreshAllTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.RefreshAll("12345_000", "ABC");

      if (actual.Level == StatusLevel.Error)
      {
        throw new AssertFailedException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.IsFalse(actual.Level == StatusLevel.Error);
    }

    //[TestMethod()]
    //public void PullTest_ABC()
    //{
    //  AdapterProxy target = new AdapterProxy();

    //  Response prepare = target.RefreshAll("12345_000", "ABC");

    //  if (prepare.Level == StatusLevel.Error)
    //  {
    //    throw new AssertFailedException(Utility.SerializeDataContract<Response>(prepare));
    //  }

    //  Request request = new Request
    //  {

    //    {"targetUri", "http://localhost/InterfaceService/sparql/"},
    //    {"graphName", "Lines"},
    //  };

    //  Response actual = target.Pull("12345_000", "ABC", request);

    //  Assert.IsFalse(actual.Level == StatusLevel.Error);
    //}

    [TestMethod()]
    public void SparqlPull()
    {
      AdapterProxy target = new AdapterProxy();

      Request request = new Request
      {
        {"targetEndpointUri", "http://localhost:54321/InterfaceService/query"},
        {"targetGraphBaseUri", "http://localhost:54321/AdapterService/12345_000/XYZ/LINES"},
      };

      Response setup = target.RefreshGraph("12345_000", "XYZ", "LINES");

      Assert.IsFalse(setup.Level == StatusLevel.Error);

      Response actual = target.Pull("12345_000", "XYZ", "LINES", request);

      Assert.IsFalse(actual.Level == StatusLevel.Error);
    }

    [TestMethod()]
    public void UpdateMapping_ABC()
    {
      string mapping = Utility.ReadString(_settings["XmlPath"] + "Mapping.12345_000.ABC.xml");
      XElement mappingXml = XElement.Parse(mapping);
      
      AdapterProxy target = new AdapterProxy();
      Response actual = target.UpdateMapping("12345_000", "ABC", mappingXml);

      Assert.IsFalse(actual.Level == StatusLevel.Error);
    }

    [TestMethod()]
    public void GetScopes()
    {
      AdapterProxy target = new AdapterProxy();
      List<ScopeProject> scopes = target.GetScopes();
      Assert.AreNotEqual(0, scopes.Count);
    }

    //[TestMethod()]
    //public void GetManifest()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  org.iringtools.library.manifest.Manifest manifest = target.GetManifest("12345_000", "ABC");
    //  Assert.AreNotEqual(0, manifest.Graphs.Count);
    //}

   

    //[TestMethod()]
    //public void PullDTO()
    //{
    //    AdapterProxy target = new AdapterProxy();
    //    Request request = new Request();
    //    WebCredentials targetCredentials = new WebCredentials();
    //    string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
    //    string adapterServiceUri = System.Configuration.ConfigurationManager.AppSettings["AdapterServiceUri"].ToString();
    //    request.Add("targetUri", adapterServiceUri);
    //    request.Add("targetCredentials", targetCredentialsXML);
    //    request.Add("graphName", "LinesGraph");
    //    request.Add("targetGraphName", "Lines");
    //    request.Add("filter", "Tag-1");
    //    //request.Add("filter", String.Empty);
    //    request.Add("projectName", "12345_000");
    //    request.Add("applicationName", "ABC");
    //    Response actual = target.PullDTO("12345_000", "DEF", request);

    //    Assert.IsFalse(actual.Level == StatusLevel.Error);
    //}

    [TestMethod]
    public void GetXml()
    {
        AdapterProxy target = new AdapterProxy();
        XElement xElement = target.GetXml("12345_000", "ABC", "Lines", "dto");
        Assert.AreNotEqual(null, xElement);
    }

    [TestMethod]
    public void GetDataObjects()
    {
        AdapterProxy target = new AdapterProxy();
        XElement xElement = target.GetXml("12345_000", "ABC", "Lines", "dto");
        IList<IDataObject> dataObjects = target.GetDataObject("12345_000", "ABC", "Lines", "dto", xElement);
        Assert.AreNotEqual(0, dataObjects.Count);
    }

    [TestMethod]
    public void Push()
    {
        AdapterProxy target = new AdapterProxy();
        PushRequest request = new PushRequest();
        WebCredentials targetCredentials = new WebCredentials();
        string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
        string adapterServiceUri = System.Configuration.ConfigurationManager.AppSettings["AdapterServiceUri"].ToString();
        request.Add("targetUri", adapterServiceUri);
        request.Add("targetCredentials", targetCredentialsXML);
        request.Add("filter", "Tag-2");
        request.Add("targetProjectName", "12345_000");
        request.Add("targetApplicationName", "ABC");
        request.Add("targetGraphName", "Lines");
        request.Add("format", "dto");

        Response actual = target.Push("12345_000", "DEF", "LinesGraph", request);
        Assert.IsFalse(actual.Level == StatusLevel.Error);
    }

    [TestMethod]
    public void PostTest()
    {
      AdapterProxy target = new AdapterProxy();
      string linesDxo = Utility.ReadString(_settings["XmlPath"] + "DXO.12345_000.ABC.Lines.xml");
      XElement linesDxoXml = XElement.Parse(linesDxo);
      Response actual = target.Post("12345_000", "ABC", "Lines", "dxo", linesDxoXml);
      Assert.IsFalse(actual.Level == StatusLevel.Error);
    }
    
    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }      
  }
}
