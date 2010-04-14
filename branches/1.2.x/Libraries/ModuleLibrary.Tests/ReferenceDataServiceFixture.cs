using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleLibrary.Tests.Base;
using ModuleLibrary.LayerDAL;
using ModuleLibrary.Events;
using org.ids_adi.iring.referenceData;

namespace ModuleLibrary.Tests
{
  /// <summary>
  /// Summary description for ReferenceDataServiceFixture
  /// </summary>
  [TestClass]
  public class ReferenceDataServiceFixture : TestServiceBase
  {
    public ReferenceDataServiceFixture() { }

    [TestInitialize]
    public void InitTests()
    {
      // Initialize with port and TestContext directory
      InitializeTest("51382", TestContext.TestDir);
      Data = null;
    }


    #region Generated Code
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
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion
    #endregion

    /// <summary>
    /// Determines whether this instance [can retrieve search results from SOAP service].
    /// </summary>
    [TestMethod]
    public void CanRetrieveSearchResultsFromSOAPService()
    {
      IReferenceData referenceData = Container.Resolve<IReferenceData>("ReferenceDataDAL");
      referenceData.OnDataArrived += new EventHandler<EventArgs>(OnDataArrivedHandler);
      referenceData.Search("pump");

      SleepForSeconds(5);

      // Cast for asserts
      RefDataEntities data = (RefDataEntities)Data;

      Assert.IsNotNull(data, "Could not retrieve results for Search(Pump)");
      KeyValuePair<string,Entity> entity = data.FirstOrDefault(e => e.Key == "ACTIVATED SLUDGE PUMP");
      Assert.IsNotNull(entity, "Could not find ACTIVATED SLUDGE PUMP in search results");
      Assert.AreEqual("ACTIVATED SLUDGE PUMP", entity.Value.label);
    }

    void OnDataArrivedHandler(object sender, EventArgs e)
    {
      Data = ((CompletedEventArgs)e).Data;
    }
  }
}
