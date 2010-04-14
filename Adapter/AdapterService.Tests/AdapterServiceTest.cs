using org.iringtools.adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using org.iringtools.library;
using org.iringtools.utility;

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
    public void RefreshDictionaryTest()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.RefreshDictionary();
      Assert.AreEqual("Data dictionary created successfully.", actual[0]);
     
    }    

    [TestMethod()]
    public void GenerateTest()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.Generate();
      Assert.AreEqual("IAdapterService.DTO.cs, IDataService.DTO.cs and ModelDTO.cs generated successfully", actual[0]);
    }

    [TestMethod()]
    public void GetDictionaryTest()
    {
      AdapterProxy target = new AdapterProxy();
      DataDictionary actual = target.GetDictionary();
      Assert.AreEqual(4, actual.dataObjects.Count);      
    }

    [TestMethod()]
    public void GetMappingTest()
    {
      AdapterProxy target = new AdapterProxy();
      Mapping actual = target.GetMapping();
      Assert.AreEqual(4, actual.graphMaps.Count);      
    }

    [TestMethod()]
    public void GetTest()
    {
      AdapterProxy target = new AdapterProxy();
      Envelope envelope = target.Get("Valves", "1-AB-PV-001");
      Assert.AreNotEqual(0, envelope.Payload.Count);

    }

    [TestMethod()]
    public void GetListTest()
    {
      AdapterProxy target = new AdapterProxy();
      Envelope envelope = target.GetList("Vessels");
      Assert.AreNotEqual(0, envelope.Payload.Count);
    }

    [TestMethod()]
    public void ClearStoreTest()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.ClearStore();
      Assert.AreEqual("Store cleared successfully.", actual[0]);
    }

    [TestMethod()]
    public void RefreshTest()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.RefreshGraph("Valves");
      Assert.AreNotEqual(0, actual.Count);
    }

    [TestMethod()]
    public void RefreshAllTest()
    {
      AdapterProxy target = new AdapterProxy();
      Response actual = target.RefreshAll();
      Assert.AreNotEqual(0, actual.Count);
    }

    [TestMethod()]
    public void PullTest()
    {
      AdapterProxy target = new AdapterProxy();
      Request request = new Request();
      WebCredentials targetCredentials = new WebCredentials();
      string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
      request.Add("targetUri", "http://localhost:2222/iring");
      request.Add("targetCredentials", targetCredentialsXML);
      request.Add("graphName", "Valves");
      request.Add("filter", "");
      Response actual = target.Pull(request);
      Assert.AreNotEqual(0, actual.Count);
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
