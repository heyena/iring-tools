using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using iRINGTools.SDK.CSVDataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.iringtools.library;
using org.iringtools.utility;
using System.IO;
using StaticDust.Configuration;

namespace org.iringtools.adapter.datalayer.csv.test
{
  [TestClass]
  public class CSVDataLayerTest
  {
    private CustomDataLayer _csvDataLayer;

    public CSVDataLayerTest()
    {
      NameValueCollection settings = new NameValueCollection();
      settings["BaseDirectoryPath"] = AppDomain.CurrentDomain.BaseDirectory;
      settings["XmlPath"] = @".\";
      settings["ProjectName"] = "12345_000";
      settings["ApplicationName"] = "CSV";

      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(settings);

      string appSettingsPath = String.Format("{0}12345_000.CSV.config",
            adapterSettings["XmlPath"]
          );

      if (File.Exists(appSettingsPath))
      {
        AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
        adapterSettings.AppendSettings(appSettings);
      }

      _csvDataLayer = new CustomDataLayer(adapterSettings);
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
    public void ReadWithFilter()
    {
      DataFilter dataFilter = new DataFilter
      {
        Expressions = new List<Expression>
        {
          new Expression
          {
            PropertyName = "PumpDriverType",
            RelationalOperator = RelationalOperator.EqualTo,
            Values = new Values
            {
              "PDT-8",
            }
          }
        }
      };

      IList<IDataObject> dataObjects = _csvDataLayer.Get("Equipment", dataFilter, 2, 0);
      
      if (!(dataObjects.Count() > 0))
      {
        throw new AssertFailedException("No Rows returned.");
      }

      Assert.AreEqual(dataObjects.Count(), 2);

      foreach (IDataObject dataObject in dataObjects)
      {
        Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
        Assert.AreEqual(dataObject.GetPropertyValue("PumpDriverType"), "PDT-8");
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

      Utility.Write<DataDictionary>(dictionary, @"C:\Users\rpdecarl\iring-tools-2.0.x\iRINGTools.SDK\CSVDataLayer\DataDictionary.xml", true);
    }
  }
}
