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
using iringtools.sdk.sp3ddatalayer;

namespace iringtools.sdk.sp3ddatalayer.test
{
  [TestFixture]
  public class Tests
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(Tests));
    private const string project = "12345_000";
    private const string app = "sp3d";
    private SP3DProvider sp3dProvider = null;

    public Tests()
    {     
      string baseDir = Directory.GetCurrentDirectory();
      baseDir = baseDir.Substring(0, baseDir.LastIndexOf("\\bin"));
      Directory.SetCurrentDirectory(baseDir);

      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings["ProjectName"] = project;
      adapterSettings["ApplicationName"] = app;
      adapterSettings["DataLayerPath"] = baseDir + "\\App_Data\\";

      sp3dProvider = new SP3DProvider(adapterSettings);
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
      DataDictionary dictionary = sp3dProvider.GetDictionary();
      Assert.Greater(dictionary.dataObjects.Count, 0);
      result = (dictionary.dataObjects.Count > 0) ? "passed." : "failed.";
      _logger.Info("Test get dictionary " + result);
      #endregion

      #region Test refresh dictionary
      Console.WriteLine("Testing refresh dictionary ...");
      response = sp3dProvider.RefreshCachingTables("");
      Assert.AreEqual(response.Level, StatusLevel.Success);
      result = (dictionary.dataObjects.Count > 0) ? "passed." : "failed.";
      _logger.Info("Test refresh dictionary " + result);
      #endregion     

      string objectType = "Equipment";

      DataFilter filter = new DataFilter();
      filter.Expressions.Add(
        new Expression
        {
          PropertyName = "oid",
          Values = new Values
            {
              "00004e2e-0000-0000-0000-1c55c44d2e04"
            },
          RelationalOperator = RelationalOperator.EqualTo
        }
      );

      filter.OrderExpressions.Add(
        new OrderExpression
        {
          PropertyName = "oid",
          SortOrder = SortOrder.Asc
        }
      );      

      #region Test get count
      Console.WriteLine("Testing get count ...");
      long count = sp3dProvider.GetCountSP3D(objectType, filter);
      Assert.Greater(count, 0);
      result = (count > 0) ? "passed." : "failed.";
      _logger.Info("Test get count " + result);
      #endregion

      if (count > MAX_ITEMS) count = MAX_ITEMS;

      #region Test get page
      Console.WriteLine("Testing get page ...");
      dataObjects = sp3dProvider.Get(objectType, filter, (int)count, 0);
      Assert.Greater(dataObjects.Count, 0);
      result = (dataObjects.Count > 0) ? "passed." : "failed.";
      _logger.Info("Test get page " + result);
      #endregion

      #region Test get identifiers
      Console.WriteLine("Testing get identifiers ...");
      IList<string> identifiers = sp3dProvider.GetIdentifiers(objectType, filter);
      Assert.Greater(identifiers.Count, 0);
      result = (identifiers.Count > 0) ? "passed." : "failed.";
      _logger.Info("Test get identifires " + result);
      #endregion

      #region Test get by identifiers
      Console.WriteLine("Testing get by identifiers ...");
      if (identifiers.Count > MAX_ITEMS)
        dataObjects = sp3dProvider.Get(objectType, (((List<string>)identifiers).GetRange(0, MAX_ITEMS - 1)));
      else
        dataObjects = sp3dProvider.Get(objectType, identifiers);
      Assert.Greater(dataObjects.Count, 0);
      result = (dataObjects.Count > 0) ? "passed." : "failed.";
      _logger.Info("Test get by identifires " + result);
      #endregion

      //
      // Create a data object to post
      //
      dataObjects = sp3dProvider.Get(objectType, new DataFilter(), 2, 1);
      IDataObject clonedDataObject = dataObjects[0];
      DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
      string keyPropName = objDef.keyProperties[0].keyPropertyName;
      string keyPropValue = Convert.ToString(clonedDataObject.GetPropertyValue(keyPropName));

      clonedDataObject.SetPropertyValue(keyPropName, keyPropValue);
      string propertyName = objDef.dataProperties[2].propertyName;
      clonedDataObject.SetPropertyValue(propertyName, "posting a value");      

      #region Test post
      Console.WriteLine("Testing post ...");
      IList<IDataObject> postDataObjects = new List<IDataObject>() { clonedDataObject };

      if (dataObjects.Count > 1)
      {
        postDataObjects.Add(dataObjects[1]);
      }

      response = sp3dProvider.PostSP3DBusinessObjects(postDataObjects);
      Assert.AreEqual(response.Level, StatusLevel.Success);
      result = (response.Level == StatusLevel.Success) ? "passed." : "failed.";
      _logger.Info("Test post " + result);
      #endregion

      #region Test delete by identifiers
      Console.WriteLine("Testing delete by identifiers ...");

      response = sp3dProvider.DeleteSP3DIdentifiers(objectType, new List<string>() { keyPropValue });
      Assert.AreEqual(response.Level, StatusLevel.Success);
      result = (response.Level == StatusLevel.Success) ? "passed." : "failed.";
      _logger.Info("Test delete by identifiers " + result);
      #endregion

      #region Test create
      Console.WriteLine("Testing create ...");
      IDataObject newDataObject = sp3dProvider.Create(objectType, null)[0];
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
      response = sp3dProvider.PostSP3DBusinessObjects(new List<IDataObject>() { newDataObject });
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
      sp3dProvider.DeleteSP3DBusinessObjects(objectType, filter);
      Assert.AreEqual(response.Level, StatusLevel.Success);
      result = (response.Level == StatusLevel.Success) ? "passed." : "failed.";
      _logger.Info("Test delete by filter " + result);
      #endregion
      
    }
  }
}
