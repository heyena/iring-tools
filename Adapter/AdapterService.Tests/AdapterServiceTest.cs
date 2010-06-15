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

namespace AdapterService.Tests
{
   /// <summary>
   ///This is a test class for AdapterServiceTest and is intended
   ///to contain all AdapterServiceTest Unit Tests
   ///</summary>
  [TestClass()]
  public class AdapterServiceTest
  {
    [TestMethod()]
    public void UpdateDatabaseDictionaryTest_ABC()
    {

      string bindingConfigPath = @"C:\iring-tools\Adapter\AdapterService.Tests\XML\BindingConfiguration.12345_000.ABC.xml";
      string bindingConfigTestPath = @"C:\iring-tools\Adapter\AdapterService.Tests\XML\BindingConfiguration.12345_000.ABC.Test.xml";
      string nhMappingPath = @"C:\iring-tools\Adapter\AdapterService.Tests\XML\nh-mapping.12345_000.ABC.xml";
      string nhMappingTestPath = @"C:\iring-tools\Adapter\AdapterService.Tests\XML\nh-mapping.12345_000.ABC.Test.xml";

      string dbDictionaryPath = @"C:\iring-tools\Adapter\AdapterService.Tests\XML\DatabaseDictionary.12345_000.ABC.xml";

      AdapterProxy target = new AdapterProxy();
      DatabaseDictionary databaseDictionary = Utility.Read<DatabaseDictionary>(dbDictionaryPath);
      Response actual = target.UpdateDatabaseDictionary("12345_000", "ABC", databaseDictionary);
      Assert.AreEqual("Entities generated successfully.", actual[0]);

      File.Copy(bindingConfigTestPath, bindingConfigPath, true);
      File.Copy(nhMappingTestPath, nhMappingPath, true);
    }

    //[TestMethod()]
    //public void GenerateTest_ABC()
    //{
    //  string bindingConfigPath = @"C:\iring-tools\Adapter\AdapterService.Tests\XML\BindingConfiguration.12345_000.ABC.xml";
    //  string bindingConfigTestPath = @"C:\iring-tools\Adapter\AdapterService.Tests\XML\BindingConfiguration.12345_000.ABC.Test.xml";

    //  AdapterProxy target = new AdapterProxy();
    //  Response actual = target.Generate("12345_000", "ABC");
    //  Assert.AreEqual("DTO Model generated successfully.", actual[0]);

    //  File.Copy(bindingConfigTestPath, bindingConfigPath, true);
    //}

    [TestMethod()]
    public void GetDictionaryTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      DataDictionary actual = target.GetDictionary("12345_000", "ABC");
      Assert.AreEqual(2, actual.dataObjects.Count);
    }

    [TestMethod()]
    public void GetMappingTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Mapping actual = target.GetMapping("12345_000", "ABC");
      Assert.AreNotEqual(0, actual.graphMaps.Count);
    }

    //[TestMethod()]
    //public void GetTest_ABC()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  XElement xml = target.Get("12345_000", "ABC", "Lines", "1-AB-L-001");
    //  Assert.AreNotEqual(0, xml.Elements().Count());
    //}

    //[TestMethod()]
    //public void GetListTest_ABC()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  XElement xml = target.GetList("12345_000", "ABC", "Lines");
    //  Assert.AreNotEqual(0, xml.Elements().Count());
    //}

    [TestMethod()]
    public void ClearStoreTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.ClearAll("12345_000", "ABC");
      if ("Graph[Valves] cleared successfully." != actual[0])
      {
        throw new AssertFailedException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.AreEqual("Graph[Valves] cleared successfully.", actual[0].ToString());
    }

    [TestMethod()]
    public void RefreshTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.RefreshGraph("12345_000", "ABC", "Lines");

      if (actual[0].ToUpper().Contains("ERROR"))
      {
        throw new AssertFailedException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.AreEqual(false, actual[0].ToUpper().Contains("ERROR"));
    }

    [TestMethod()]
    public void RefreshAllTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.RefreshAll("12345_000", "ABC");

      if (actual[0].ToUpper().Contains("ERROR"))
      {
        throw new AssertFailedException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.AreEqual(false, actual[0].ToUpper().Contains("ERROR"));
    }

    [TestMethod()]
    public void PullTest_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Request request = new Request();
      WebCredentials targetCredentials = new WebCredentials();
      string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
      request.Add("targetUri", "http://localhost/InterfaceService/sparql/");
      request.Add("targetCredentials", targetCredentialsXML);
      request.Add("filter", "");
      Response actual = target.Pull("12345_000", "ABC", "Lines", request);
      bool isError = false;
      for (int i = 0; i < actual.Count; i++)
      {
        if (actual[i].ToUpper().Contains("ERROR"))
        {
          isError = true;
          break;
        }
      }
      Assert.AreEqual(false, isError);
    }

    [TestMethod()]
    public void UpdateMapping_ABC()
    {
      AdapterProxy target = new AdapterProxy();
      Mapping mapping = Utility.Read<Mapping>(@"C:\iring-tools\Adapter\AdapterService.Tests\XML\Mapping.12345_000.ABC.xml", true);
      Response actual = target.UpdateMapping("12345_000", "ABC", mapping);
      bool isError = false;
      for (int i = 0; i < actual.Count; i++)
      {
        if (actual[i].ToUpper().Contains("ERROR"))
        {
          isError = true;
          break;
        }
      }
      Assert.AreEqual(false, isError);
    }

    [TestMethod()]
    public void GetScopes()
    {
      AdapterProxy target = new AdapterProxy();
      List<ScopeProject> scopes = target.GetScopes();
      Assert.AreNotEqual(0, scopes.Count);
    }

    [TestMethod()]
    public void GetManifest()
    {
      AdapterProxy target = new AdapterProxy();
      Manifest manifest = target.GetManifest("12345_000", "ABC");
      Assert.AreNotEqual(0, manifest.Graphs.Count);
    }

    //[TestMethod()]
    //public void UpdateDatabaseDictionaryTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  string dbDictionaryPath = System.Environment.CurrentDirectory + @"\XML\DatabaseDictionary.12345_000.DEF.xml";
    //  DatabaseDictionary databaseDictionary = Utility.Read<DatabaseDictionary>(dbDictionaryPath);
    //  Response actual = target.UpdateDatabaseDictionary(databaseDictionary, "12345_000", "DEF");
    //  Assert.AreEqual("Entities generated successfully.", actual[0]);
    //}

    //[TestMethod()]
    //public void GenerateTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  Response actual = target.Generate("12345_000", "DEF");
    //  Assert.AreEqual("DTO Model generated successfully.", actual[0]);
    //}

    //[TestMethod()]
    //public void GetDictionaryTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  DataDictionary actual = target.GetDictionary("12345_000", "DEF");
    //  Assert.AreEqual(1, actual.dataObjects.Count);
    //}

    //[TestMethod()]
    //public void GetMappingTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  Mapping actual = target.GetMapping("12345_000", "DEF");
    //  Assert.AreEqual(1, actual.graphMaps.Count);
    //}

    //[TestMethod()]
    //public void GetTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  Envelope envelope = target.Get("12345_000", "DEF", "Lines", "1-AB-L126");
    //  Assert.AreNotEqual(0, envelope.Payload.Count);
    //}

    //[TestMethod()]
    //public void GetListTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  Envelope envelope = target.GetList("12345_000", "DEF", "Lines");
    //  Assert.AreNotEqual(0, envelope.Payload.Count);
    //}

    //[TestMethod()]
    //public void ClearStoreTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  Response actual = target.ClearStore("12345_000", "DEF");
    //  Assert.AreEqual("Store cleared successfully.", actual[0]);
    //}

    //[TestMethod()]
    //public void RefreshTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  Response actual = target.RefreshGraph("12345_000", "DEF", "Lines");
    //  Assert.AreEqual(false, actual[0].ToUpper().Contains("ERROR"));
    //}

    //[TestMethod()]
    //public void RefreshAllTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  Response actual = target.RefreshAll("12345_000", "DEF");
    //  Assert.AreEqual(false, actual[0].ToUpper().Contains("ERROR"));
    //}

    //[TestMethod()]
    //public void PullTest_DEF()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  Request request = new Request();
    //  WebCredentials targetCredentials = new WebCredentials();
    //  string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
    //  request.Add("targetUri", "http://localhost/InterfaceService/sparql");
    //  request.Add("targetCredentials", targetCredentialsXML);
    //  request.Add("graphName", "Lines");
    //  request.Add("filter", "");
    //  Response actual = target.Pull("12345_000", "DEF", request);
    //  bool isError = false;
    //  for (int i = 0; i < actual.Count; i++)
    //  {
    //    if (actual[i].ToUpper().Contains("ERROR"))
    //    {
    //      isError = true;
    //      break;
    //    }
    //  }
    //  Assert.AreEqual(false, isError);
    //}

    //[TestMethod()]
    //public void PullDTO()
    //{
    //    AdapterProxy target = new AdapterProxy();
    //    Request request = new Request();
    //    WebCredentials targetCredentials = new WebCredentials();
    //    string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
    //    request.Add("targetUri", "http://localhost/AdapterService/Service.svc");
    //    request.Add("targetCredentials", targetCredentialsXML);
    //    request.Add("graphName", "Lines");
    //    request.Add("filter", "1-AB-L-003");
    //    //request.Add("filter", String.Empty);
    //    request.Add("projectName", "12345_000");
    //    request.Add("applicationName", "ABC");
    //    Response actual = target.PullDTO("12345_000", "ABC", request);
    //    bool isError = false;
    //    for (int i = 0; i < actual.Count; i++)
    //    {
    //        if (actual[i].ToUpper().Contains("ERROR"))
    //        {
    //            isError = true;
    //            break;
    //        }
    //    }
    //    Assert.AreEqual(false, isError);
    //}

    //[TestMethod()]
    //public void PullDTOPrototype()
    //{
    //  AdapterProxy target = new AdapterProxy();
    //  WebHttpClient httpClient = new WebHttpClient(@"http://localhost:52786/Service.svc");
    //  string dtoListString = httpClient.GetMessage(@"/12345_000/DEF/Lines/2-SC-L069");
    //  File.WriteAllText(System.Environment.CurrentDirectory + @"\XML\DTOString.xml", dtoListString, System.Text.Encoding.UTF8);
    //  #region Deserialization code
    //  //StringReader input = new StringReader(dtoListString);
    //  //XmlReaderSettings settings = new XmlReaderSettings();
    //  //settings.ProhibitDtd = false;
    //  //XmlReader reader = XmlDictionaryReader.Create(input, settings);

    //  //System.Type[] extraTypes = { typeof(org.iringtools.adapter.proj_12345_000.DEF.Lines) };

    //  //XmlSerializer serializer = new XmlSerializer(typeof(List<DataTransferObject>), extraTypes);

    //  //List<DataTransferObject> dtoList = (List<DataTransferObject>)serializer.Deserialize(reader);

    //  //List<DataTransferObject> dtoList = Utility.Deserialize<List<org.iringtools.adapter.DataTransferObject>>(dtoListString, false);

    //  //foreach (DataTransferObject dto in dtoList)
    //  //{
    //  //  org.iringtools.adapter.proj_12345_000.DEF.Line line = (org.iringtools.adapter.proj_12345_000.DEF.Line)dto.GetDataObject();
    //  //}
    //  #endregion

    //  XDocument xmlFile = XDocument.Load(System.Environment.CurrentDirectory + @"\XML\DTOString.xml");
    //  XNamespace propertyNS = "http://dto.iringtools.org";
    //  XNamespace projectNS = "http://DEF.bechtel.com/12345_000/data#";

    //  List<org.iringtools.adapter.proj_12345_000.ABC.Lines> lineList = new List<org.iringtools.adapter.proj_12345_000.ABC.Lines>();

    //  var query1 = from c in xmlFile.Elements(propertyNS + "Envelope").Elements(propertyNS + "Payload").Elements(propertyNS + "DataTransferObject")
    //               select c;
    //  foreach (var dto in query1)
    //  {
    //    var query2 = from c in dto.Elements(propertyNS + "Properties").Elements(propertyNS + "Property")
    //                 select c;

    //    org.iringtools.adapter.proj_12345_000.ABC.Lines line = new org.iringtools.adapter.proj_12345_000.ABC.Lines();

    //    foreach (var dtoProperty in query2)
    //    {
    //      if (dtoProperty.Attribute("name").Value == "tpl_PipingNetworkSystemName_identifier")
    //        line.tpl_PipingNetworkSystemName_tpl_identifier = dtoProperty.Attribute("value").Value.ToString();
    //      if (dtoProperty.Attribute("name").Value == "tpl_SystemPipingNetworkSystemAssembly_hasClassOfWhole_rdl_System_tpl_SystemName_identifier")
    //        line.tpl_SystemPipingNetworkSystemAssembly_tpl_hasClassOfWhole_rdl_System_tpl_SystemName_tpl_identifier = dtoProperty.Attribute("value").Value.ToString();
    //    }
    //    lineList.Add(line);
    //  }
    //}
    
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

    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //}
    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test
    //[TestInitialize()]
    //public void MyTestInitialize()
    //{
    //}
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //
    #endregion
      
  }
}
