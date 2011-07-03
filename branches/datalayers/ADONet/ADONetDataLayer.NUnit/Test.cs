using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;


namespace org.iringtools.adapter.datalayer
{
  [TestFixture]
  public class ADONetDataLayerTest
  {
    private string _baseDirectory = string.Empty;
    private NameValueCollection _settings;
    private AdapterSettings _adapterSettings;
    private ADONetDataLayer _dataLayer;

    public ADONetDataLayerTest()
    {
      _settings = new NameValueCollection();

      _settings["ProjectName"] = "12345_000";
      _settings["XmlPath"] = @".\XML";
      _settings["ApplicationName"] = "SQL";
      _settings["Scope"] = string.Format("{0}.{1}", _settings["ProjectName"], _settings["ApplicationName"]);
      _settings["TestMode"] = "UseFiles"; //UseFiles/WriteFiles

      _baseDirectory = Directory.GetCurrentDirectory();
      _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
      _settings["BaseDirectoryPath"] = _baseDirectory;
      Directory.SetCurrentDirectory(_baseDirectory);

      _adapterSettings = new AdapterSettings();
      _adapterSettings.AppendSettings(_settings);

      string appSettingsPath = Path.Combine(_adapterSettings["XmlPath"], "12345_000.SQL.config");

      if (File.Exists(appSettingsPath))
      {
        AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
        _adapterSettings.AppendSettings(appSettings);
      }

      //ADONetConfiguration config = new ADONetConfiguration();
      //config.Connection = "[Connection String]";
      //config.ConfigObjects = new List<ConfigObject>();
      //config.ConfigObjects.Add(new ConfigObject
      //{
      //  Name = "[Name]",
      //  Identifier = "[Identifier]",
      //  Select = new ConfigCommand { Query = "[Query]" }
      //});

      //string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings["XmlPath"], "adonet-configuration." + _settings["ProjectName"] + "." + _settings["ApplicationName"] + ".xml");
      //Utility.Write<ADONetConfiguration>(config, configPath, true);

      _dataLayer = new ADONetDataLayer(_adapterSettings);
    }

    //[Test]
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

    [Test]
    public void Read()
    {
      IList<string> identifiers = new List<string>() 
      { 
        "8976020", 
        "7236861"
      };

      IList<IDataObject> dataObjects = _dataLayer.Get("Documents", identifiers);

      if (!(dataObjects.Count() > 0))
      {
        throw new AssertionException("No Rows returned.");
      }

      foreach (IDataObject dataObject in dataObjects)
      {
        Assert.IsNotNull(dataObject.GetPropertyValue("DataID"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DocumentName"));
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

    [Test]
    public void Read1()
    {
      long count = _dataLayer.GetCount("Documents", null);

      Assert.Greater(count, 0);
    }
  }
}
