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
using log4net;
using org.iringtools.test;
using iringtools.sdk.sp3ddatalayer;

namespace org.iringtools.adapter.datalayer.test
{
  [TestFixture]
  public class Tests
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(Tests));
    private const string project = "test";
    private const string app = "sppid";

    private IDataLayer2 _dataLayer = null;
    private Scenarios _scenarios = null;

    public Tests()
    {
      string baseDir = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));

      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings["ProjectName"] = project;
      adapterSettings["ApplicationName"] = app;

      string appConfigPath = string.Format("App_Data\\{0}.{1}.config", project, app);
      adapterSettings.AppendSettings(new AppSettingsReader(appConfigPath));

      _dataLayer = new SP3DDataLayer(adapterSettings);
      _scenarios = org.iringtools.utility.Utility.Read<Scenarios>("Scenarios.xml");
    }

    [Test]
    public void TestDataObjects()
    {
      int MAX_ITEMS = 25;
      string result = string.Empty;
      Response response = null;
      IList<IDataObject> dataObjects;

      #region Test dictionary
      Console.WriteLine("\nTesting get dictionary ...");
      DataDictionary dictionary = _dataLayer.GetDictionary();
      Assert.Greater(dictionary.dataObjects.Count, 0);
      result = (dictionary.dataObjects.Count > 0) ? "passed." : "failed.";
      _logger.Info("Test get dictionary " + result);
      #endregion

      #region Test refresh dictionary
      Console.WriteLine("Testing refresh dictionary ...");
      response = _dataLayer.RefreshAll();
      Assert.AreEqual(response.Level, StatusLevel.Success);
      result = (dictionary.dataObjects.Count > 0) ? "passed." : "failed.";
      _logger.Info("Test refresh dictionary " + result);
      #endregion

      foreach (Scenario scenario in _scenarios)
      {
        Console.WriteLine(string.Format("\nExecuting scenario [{0}] ...", scenario.Name));

        string objectType = scenario.ObjectType;
        string padding = scenario.IdentifierPadding;
        org.iringtools.test.Properties properties = scenario.Properties;
        DataFilter filter = !string.IsNullOrEmpty(scenario.DataFilter)
          ? org.iringtools.utility.Utility.DeserializeDataContract<DataFilter>(scenario.DataFilter)
          : new DataFilter();

        #region Test get count
        Console.WriteLine("Testing get count ...");
        long count = _dataLayer.GetCount(objectType, filter);
        Assert.Greater(count, 0);
        result = (count > 0) ? "passed." : "failed.";
        _logger.Info("Test get count " + result);
        #endregion

        if (count > MAX_ITEMS) count = MAX_ITEMS;

        #region Test get page
        Console.WriteLine("Testing get page ...");
        dataObjects = _dataLayer.Get(objectType, filter, (int)count, 0);
        Assert.Greater(dataObjects.Count, 0);
        result = (dataObjects.Count > 0) ? "passed." : "failed.";
        _logger.Info("Test get page " + result);
        #endregion

        #region Test get identifiers
        Console.WriteLine("Testing get identifiers ...");
        IList<string> identifiers = _dataLayer.GetIdentifiers(objectType, filter);
        Assert.Greater(identifiers.Count, 0);
        result = (identifiers.Count > 0) ? "passed." : "failed.";
        _logger.Info("Test get identifires " + result);
        #endregion

        #region Test get by identifiers
        Console.WriteLine("Testing get by identifiers ...");
        if (identifiers.Count > MAX_ITEMS)
          dataObjects = _dataLayer.Get(objectType, (((List<string>)identifiers).GetRange(0, MAX_ITEMS - 1)));
        else
          dataObjects = _dataLayer.Get(objectType, identifiers);
        Assert.Greater(dataObjects.Count, 0);
        result = (dataObjects.Count > 0) ? "passed." : "failed.";
        _logger.Info("Test get by identifires " + result);
        #endregion

        //
        // Create a data object to post
        //
        dataObjects = _dataLayer.Get(objectType, new DataFilter(), 2, 1);
        IDataObject clonedDataObject = dataObjects[0];
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
        string keyPropName = objDef.keyProperties[0].keyPropertyName;
        string keyPropValue = Convert.ToString(clonedDataObject.GetPropertyValue(keyPropName)) + padding;

        // Set key property
        clonedDataObject.SetPropertyValue(keyPropName, keyPropValue);

        // Set configured properties
        foreach (Property prop in properties)
        {
          string value = Guid.NewGuid().ToString("N");
          clonedDataObject.SetPropertyValue(prop.Name, value);

          // change another data object if available to test mixed of new and updated list
          if (dataObjects.Count > 1)
          {
            dataObjects[1].SetPropertyValue(prop.Name, value);
          }
        }

        #region Test post
        Console.WriteLine("Testing post ...");
        IList<IDataObject> postDataObjects = new List<IDataObject>() { clonedDataObject };

        if (dataObjects.Count > 1)
        {
          postDataObjects.Add(dataObjects[1]);
        }

        response = _dataLayer.Post(postDataObjects);
        Assert.AreEqual(response.Level, StatusLevel.Success);
        result = (response.Level == StatusLevel.Success) ? "passed." : "failed.";
        _logger.Info("Test post " + result);
        #endregion

        #region Test delete by identifiers
        Console.WriteLine("Testing delete by identifiers ...");
        response = _dataLayer.Delete(objectType, new List<string>() { keyPropValue });
        Assert.AreEqual(response.Level, StatusLevel.Success);
        result = (response.Level == StatusLevel.Success) ? "passed." : "failed.";
        _logger.Info("Test delete by identifiers " + result);
        #endregion

        #region Test create
        Console.WriteLine("Testing create ...");
        IDataObject newDataObject = _dataLayer.Create(objectType, null)[0];
        Assert.AreNotEqual(newDataObject, null);
        result = (newDataObject != null) ? "passed." : "failed.";
        _logger.Info("Test create " + result);
        #endregion

        #region Test delete by filter
        Console.WriteLine("Testing delete by filter ...");
        // Prepare data object to post
        foreach (DataProperty prop in objDef.dataProperties)
        {
          newDataObject.SetPropertyValue(prop.propertyName, clonedDataObject.GetPropertyValue(prop.propertyName));
        }

        // Post it
        response = _dataLayer.Post(new List<IDataObject>() { newDataObject });
        Assert.AreEqual(response.Level, StatusLevel.Success);

        // Prepare filter to delete
        filter = new DataFilter()
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

        // Execute delete
        _dataLayer.Delete(objectType, filter);
        Assert.AreEqual(response.Level, StatusLevel.Success);
        result = (response.Level == StatusLevel.Success) ? "passed." : "failed.";
        _logger.Info("Test delete by filter " + result);
        #endregion
      }
    }
  }
}
