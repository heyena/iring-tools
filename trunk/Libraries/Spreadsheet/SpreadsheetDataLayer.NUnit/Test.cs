using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;

using StaticDust.Configuration;
using NUnit.Framework;

using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;

namespace iRINGTools.SDK.SpreadsheetDataLayer.NUnit
{
  [TestFixture]
  public class SpreadsheetDataLayerTest
  {
    private string _baseDirectory = string.Empty;
    private NameValueCollection _settings;
    private AdapterSettings _adapterSettings;
    private SpreadsheetDataLayer _dataLayer;

    public SpreadsheetDataLayerTest()
    {
      _settings = new NameValueCollection();

      _settings["ProjectName"] = "12345_000";
      _settings["XmlPath"] = @"XML";
      _settings["ApplicationName"] = "SPDS";
      _settings["Scope"] = string.Format("{0}.{1}", _settings["ProjectName"], _settings["ApplicationName"]);
      _settings["TestMode"] = "UseFiles"; //"WriteFiles";

      _baseDirectory = Directory.GetCurrentDirectory();
      _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
      _settings["BaseDirectoryPath"] = _baseDirectory;
      Directory.SetCurrentDirectory(_baseDirectory);

      _adapterSettings = new AdapterSettings();
      _adapterSettings.AppendSettings(_settings);

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        SpreadsheetConfiguration configuration = new SpreadsheetConfiguration { Location = @"XML\Equipment.xlsx", Generate = true };
        Utility.Write<SpreadsheetConfiguration>(configuration, @"XML\spreadsheet-configuration.12345_000.SPDS.xml");
      }      

      _dataLayer = new SpreadsheetDataLayer(_adapterSettings);
    }

    [Test]
    public void Create()
    {
      IList<string> identifiers = new List<string>() { 
                "Equip-001", 
                "Equip-002",
                "Equip-003", 
                "Equip-004"
            };

      Random random = new Random();

      IList<IDataObject> dataObjects = _dataLayer.Create("Equipment", identifiers);
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
      Response actual = _dataLayer.Post(dataObjects);

      if (actual.Level != StatusLevel.Success)
      {
        throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.IsTrue(actual.Level == StatusLevel.Success);
    }

    //[Test]
    public void Read()
    {
      IList<string> identifiers = new List<string>() 
            { 
                "Equip-001", 
                "Equip-002", 
                "Equip-003", 
                "Equip-004" 
            };

      IList<IDataObject> dataObjects = _dataLayer.Get("Equipment", identifiers);

      if (!(dataObjects.Count() > 0))
      {
        throw new AssertionException("No Rows returned.");
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

    //[Test]
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

      IList<IDataObject> dataObjects = _dataLayer.Get("Equipment", dataFilter, 2, 0);

      if (!(dataObjects.Count() > 0))
      {
        throw new AssertionException("No Rows returned.");
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

    [Test]
    public void A_Delete()
    {
      IList<string> identifiers = new List<string>() 
            { 
                "Equip-001", 
                "Equip-002", 
                "Equip-003", 
                "Equip-004" 
            };

      Response response = _dataLayer.Delete("Equipment", identifiers);

      if (!(response.StatusList.Count() > 0))
      {
        throw new AssertionException("No Rows returned.");
      }

      IList<IDataObject> dataObjects =_dataLayer.Get("Equipment", identifiers);

      Assert.AreEqual(dataObjects.Count, 0);
    }

    [Test]
    public void GetDictionary()
    {
      DataDictionary benchmark = null;

      DataDictionary dictionary = _dataLayer.GetDictionary();

      Assert.IsNotNull(dictionary);

      string path = Path.Combine(_adapterSettings["XmlPath"], string.Format("DataDictionary.{0}.xml", _adapterSettings["Scope"]));

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        Utility.Write<DataDictionary>(dictionary, path);
        Assert.AreNotEqual(null, dictionary);
      }
      else
      {
        benchmark = Utility.Read<DataDictionary>(path);
        Assert.AreEqual(
          Utility.SerializeDataContract<DataDictionary>(benchmark),
          Utility.SerializeDataContract<DataDictionary>(dictionary)
        );
      }
    }
  }
}
