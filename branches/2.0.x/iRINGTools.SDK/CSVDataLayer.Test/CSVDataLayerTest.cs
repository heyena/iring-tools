using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;

namespace Bechtel.CSVDataLayer.API
{
  /// <summary>
  /// Summary description for UnitTest1
  /// </summary>
  [TestClass]
  public class CSVDataLayerTest
  {
    private CSVDataLayer _csvDataLayer;

    public CSVDataLayerTest()
    {
      NameValueCollection settings = new NameValueCollection();
      settings["BaseDirectoryPath"] = String.Empty;
      settings["XmlPath"] = @"C:\iring-tools\CSVDataLayer\CSVDataLayer\";
      settings["ProxyCredentialToken"] = String.Empty;
      settings["ProxyHost"] = String.Empty;
      settings["ProxyPort"] = String.Empty;
      settings["UseSemweb"] = "False";
      settings["TripleStoreConnectionString"] = String.Empty;
      settings["InterfaceService"] = String.Empty;
      settings["InterfaceServicePath"] = String.Empty;
      settings["TargetCredentialToken"] = String.Empty;
      settings["TrimData"] = "False";
      settings["BinaryPath"] = String.Empty;
      settings["CodePath"] = String.Empty;

      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(settings);

      _csvDataLayer = new CSVDataLayer(adapterSettings);
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

    [TestMethod]
    public void Create()
    {
      IList<string> identifiers = new List<string>() 
      { 
        "Equip-001", 
        "Equip-002", 
        "Equip-003", 
        "Equip-004" 
      };

      Random random = new Random();

      IList<IDataObject> dataObjects = _csvDataLayer.Create("Equipment", identifiers);
      foreach (IDataObject dataObject in dataObjects)
      {
        dataObject.SetPropertyValue("PumpType", "PT-" + random.Next(2, 10));
        dataObject.SetPropertyValue("PumpDriverType", "PDT-" + random.Next(2, 10));
        dataObject.SetPropertyValue("DesignTemp", (double)random.Next(2, 10));
        dataObject.SetPropertyValue("DesignPressure", (double)random.Next(2, 10));
        dataObject.SetPropertyValue("Capacity", (double)random.Next(2, 10));
        dataObject.SetPropertyValue("SpecificGravity", (double)random.Next(2, 10));
        dataObject.SetPropertyValue("DifferentialPressure", (double)random.Next(2, 10));
      }
      Response actual = _csvDataLayer.Post(dataObjects);

      if (actual.Level != StatusLevel.Success)
      {
        throw new AssertFailedException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.IsTrue(actual.Level == StatusLevel.Success);
    }

    [TestMethod]
    public void Read()
    {
      IList<string> identifiers = new List<string>() 
      { 
        "Equip-001", 
        "Equip-002", 
        "Equip-003", 
        "Equip-004" 
      };

      IList<IDataObject> dataObjects = _csvDataLayer.Get("Equipment", identifiers);
      
      if (!(dataObjects.Count() > 0))
      {
        throw new AssertFailedException("No Rows returned.");
      }

      foreach (IDataObject dataObject in dataObjects)
      {
        Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
        Assert.IsNotNull(dataObject.GetPropertyValue("PumpDriverType"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
        Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
        Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
      }
    }

    [TestMethod]
    public void GetDictionary()
    {
      DataDictionary dictionary = _csvDataLayer.GetDictionary();

      Assert.IsNotNull(dictionary);

      Utility.Write<DataDictionary>(dictionary, @"C:\iring-tools\Adapter\CSVDataLayer.Test\DataDictionary.xml", true);
    }
  }
}
