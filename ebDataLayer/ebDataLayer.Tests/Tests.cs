using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using System.Data;
using System.Text;
using org.iringtools.adapter.datalayer.eb;
using log4net;

namespace org.iringtools.adaper.datalayer.test
{
  [TestFixture]
  public class Tests
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(Tests));
    private const string IDENTIFIER_PADDING = "_";
    private IDataLayer2 _dataLayer = null;
    private Scenarios _scenarios = null;

    public Tests()
    {
      string baseDir = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));
      
      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(new AppSettingsReader("App.config"));

      FileInfo log4netConfig = new FileInfo("log4net.config");
      log4net.Config.XmlConfigurator.Configure(log4netConfig);

      _dataLayer = new ebDataLayer(adapterSettings);
      _scenarios = Utility.Read<Scenarios>("Scenarios.xml");
    }

    [Test]
    public void RunTest()
    {
      #region Test dictionary
      Console.WriteLine("Testing get dictionary ...");
      DataDictionary dictionary = _dataLayer.GetDictionary();
      Assert.Greater(dictionary.dataObjects.Count, 0);
      #endregion

      #region Test refresh dictionary
      Console.WriteLine("Testing refresh dictionary ...");
      Response response = _dataLayer.RefreshAll();
      Assert.AreEqual(response.Level, StatusLevel.Success);
      #endregion

      foreach (Scenario scenario in _scenarios)
      {
        Console.WriteLine(string.Format("Executing scenario [{0}] ...", scenario.Name));

        string objectType = scenario.ObjectType;
        Properties properties = scenario.Properties;
        DataFilter dataFilter = Utility.DeserializeDataContract<DataFilter>(scenario.DataFilter);

        #region Test get count
        Console.WriteLine("Testing get count ...");
        long count = _dataLayer.GetCount(objectType, dataFilter);
        Assert.Greater(count, 0);
        #endregion

        if (count > 25) count = 25;

        #region Test get page
        Console.WriteLine("Testing get page ...");
        IList<IDataObject> dataObjects = _dataLayer.Get(objectType, dataFilter, (int)count, 0);
        Assert.Greater(dataObjects.Count, 0);
        #endregion

        #region Test get identifiers
        Console.WriteLine("Testing get identifiers ...");
        IList<string> identifiers = _dataLayer.GetIdentifiers(objectType, dataFilter);
        Assert.Greater(identifiers.Count, 0);
        #endregion

        #region Test get by identifiers
        Console.WriteLine("Testing get by identifiers ..."); 
        dataObjects = _dataLayer.Get(objectType, identifiers);
        Assert.Greater(dataObjects.Count, 0);
        #endregion

        #region Test post
        Console.WriteLine("Testing post ...");

        // Create data object by cloning an one and changing its property values
        IDataObject dataObject = dataObjects[0];
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
        string keyPropName = objDef.keyProperties[0].keyPropertyName;
        string keyPropValue = Convert.ToString(dataObject.GetPropertyValue(keyPropName)) + IDENTIFIER_PADDING;

        // Set key property
        dataObject.SetPropertyValue(keyPropName, keyPropValue);

        // Set configured properties
        foreach (Property prop in properties)
        {
          dataObject.SetPropertyValue(prop.Name, prop.Value);
        }

        response = _dataLayer.Post(new List<IDataObject>() { dataObject });
        Assert.AreEqual(response.Level, StatusLevel.Success);
        #endregion

        #region Test delete by identifiers
        Console.WriteLine("Testing delete by identifiers ...");
        response = _dataLayer.Delete(objectType, new List<string>() { keyPropValue });
        Assert.AreEqual(response.Level, StatusLevel.Success);
        #endregion

        #region Test delete by filter
        Console.WriteLine("Testing delete by filter ...");
        response = _dataLayer.Post(new List<IDataObject>() { dataObject });
        Assert.AreEqual(response.Level, StatusLevel.Success);

        dataFilter = new DataFilter()
        {
          Expressions = new List<Expression>()
          {
            new Expression()
            {
              PropertyName = keyPropName,
              RelationalOperator = RelationalOperator.EqualTo,
              Values = new Values() { keyPropValue }
            }
          }
        };

        _dataLayer.Delete(objectType, dataFilter);
        Assert.AreEqual(response.Level, StatusLevel.Success);
        #endregion
      }      
    }
  }
}
