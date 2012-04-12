﻿using System;
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
using org.iringtools.adapter.datalayer;

namespace bechtel.eb.datalayer.test
{
  [TestFixture]
  public class Tests
  {
    private IDataLayer2 _dataLayer;
    private string _objectType;
    private DataObject _objectDefinition;
    private string _modifiedProperty;
    private DataFilter _filter;

    public Tests()
    {
      string baseDir = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));

      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(new AppSettingsReader("app.config"));

      FileInfo log4netConfig = new FileInfo("log4net.config");
      log4net.Config.XmlConfigurator.Configure(log4netConfig);

      _dataLayer = new ebDataLayer(adapterSettings);
      _dataLayer.GetDictionary();
    }

    [Test]
    public void TestCreate()
    {
      IList<IDataObject> dataObjects = _dataLayer.Create(_objectType, null);
      Assert.AreNotEqual(dataObjects, null);
    }

    [Test]
    public void TestGetCount()
    {
      long count = _dataLayer.GetCount(_objectType, new DataFilter());
      Assert.Greater(count, 0);
    }

    [Test]
    public void TestGetPage()
    {
      IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, new DataFilter(), 5, 0);
      Assert.Greater(dataObjects.Count, 0);
    }

    [Test]
    public void TestGetWithIdentifiers()
    {
      IList<string> identifiers = _dataLayer.GetIdentifiers(_objectType, new DataFilter());
      IList<string> identifier = ((List<string>)identifiers).GetRange(1, 1);
      IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, identifier);
      Assert.Greater(dataObjects.Count, 0);
    }

    [Test]
    public void TestGetCountWithFilter()
    {
      long count = _dataLayer.GetCount(_objectType, _filter);
      Assert.Greater(count, 0);
    }

    [Test]
    public void TestGetPageWithFilter()
    {
      IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, _filter, 5, 0);
      Assert.Greater(dataObjects.Count, 0);
    }

    [Test]
    public void TestGetIdentifiersWithFilter()
    {
      IList<string> identifiers = _dataLayer.GetIdentifiers(_objectType, _filter);
      Assert.Greater(identifiers.Count, 0);
    }

    [Test]
    public void TestPostWithUpdate()
    {
      IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, new DataFilter(), 1, 1);
      string orgIdentifier = GetIdentifier(dataObjects[0]);    
      string orgPropValue = Convert.ToString(dataObjects[0].GetPropertyValue(_modifiedProperty)) ?? String.Empty;
      string newPropValue = GenerateStringValue();
      
      // post data object with modified property
      dataObjects[0].SetPropertyValue(_modifiedProperty, newPropValue);
      Response response = _dataLayer.Post(dataObjects);
      Assert.AreEqual(response.Level, StatusLevel.Success);
      
      // verify post result
      dataObjects = _dataLayer.Get(_objectType, new List<string> { orgIdentifier });
      Assert.AreEqual(dataObjects[0].GetPropertyValue(_modifiedProperty), newPropValue);

      // reset property to its orginal value
      dataObjects[0].SetPropertyValue(_modifiedProperty, orgPropValue);
      response = _dataLayer.Post(dataObjects);
      Assert.AreEqual(response.Level, StatusLevel.Success);
    }

    [Test]
    public void TestPostWithAddAndDeleteByIdentifier()
    {
      //
      // create a new data object by getting an existing one and change its identifier
      //
      IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, new DataFilter(), 1, 1);
      string orgIdentifier = GetIdentifier(dataObjects[0]);

      string newIdentifier = GenerateStringValue();
      SetIdentifier(dataObjects[0], newIdentifier);  

      // post the new data object
      Response response = _dataLayer.Post(dataObjects);
      Assert.AreEqual(response.Level, StatusLevel.Success);

      //
      // delete the new data object by its identifier
      //
      response = _dataLayer.Delete(_objectType, new List<string> { newIdentifier });
      Assert.AreEqual(response.Level, StatusLevel.Success);
    }

    [Test]
    public void TestPostWithAddAndDeleteByFilter()
    {
      //
      // create new data object by getting an existing one and change its identifier
      //
      IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, new DataFilter(), 1, 1);
      string orgIdentifier = GetIdentifier(dataObjects[0]);

      string newIdentifier = GenerateStringValue();
      SetIdentifier(dataObjects[0], newIdentifier);

      // post new data object
      Response response = _dataLayer.Post(dataObjects);
      Assert.AreEqual(response.Level, StatusLevel.Success);

      //
      // delete the new data object with a filter
      //
      DataFilter filter = new DataFilter();

      filter.Expressions.Add(
        new Expression()
        {
          PropertyName = "szTagNo",
          RelationalOperator = org.iringtools.library.RelationalOperator.EqualTo,
          Values = new Values() { newIdentifier }
        }
      );

      response = _dataLayer.Delete(_objectType, filter);
      Assert.AreEqual(response.Level, StatusLevel.Success);
    }

    [Test]
    public void TestRefresh()
    {
      Response response = _dataLayer.RefreshAll();
      Assert.AreEqual(response.Level, StatusLevel.Success);
    }

    #region helper methods
    private string GenerateStringValue()
    {
      return DateTime.Now.ToUniversalTime().Ticks.ToString();
    }

    private DataObject GetObjectDefinition(string objectType)
    {
      DataDictionary dictionary = _dataLayer.GetDictionary();

      if (dictionary.dataObjects != null)
      {
        foreach (DataObject dataObject in dictionary.dataObjects)
        {
          if (dataObject.objectName.ToLower() == objectType.ToLower())
          {
            return dataObject;
          }
        }
      }

      return null;
    }

    private string GetIdentifier(IDataObject dataObject)
    {
      string[] identifierParts = new string[_objectDefinition.keyProperties.Count];

      int i = 0;
      foreach (KeyProperty keyProperty in _objectDefinition.keyProperties)
      {
        identifierParts[i] = dataObject.GetPropertyValue(keyProperty.keyPropertyName).ToString();
        i++;
      }

      return String.Join(_objectDefinition.keyDelimeter, identifierParts);
    }

    private IList<string> GetKeyProperties()
    {
      IList<string> keyProperties = new List<string>();

      foreach (DataProperty dataProp in _objectDefinition.dataProperties)
      {
        foreach (KeyProperty keyProp in _objectDefinition.keyProperties)
        {
          if (dataProp.propertyName == keyProp.keyPropertyName)
          {
            keyProperties.Add(dataProp.propertyName);
          }
        }
      }

      return keyProperties;
    }

    private void SetIdentifier(IDataObject dataObject, string identifier)
    {
      IList<string> keyProperties = GetKeyProperties();

      if (keyProperties.Count == 1)
      {
        dataObject.SetPropertyValue(keyProperties[0], identifier);
      }
      else if (keyProperties.Count > 1)
      {
        StringBuilder identifierBuilder = new StringBuilder();

        foreach (string keyProperty in keyProperties)
        {
          dataObject.SetPropertyValue(keyProperty, identifier);

          if (identifierBuilder.Length > 0)
          {
            identifierBuilder.Append(_objectDefinition.keyDelimeter);
          }

          identifierBuilder.Append(identifier);
        }

        identifier = identifierBuilder.ToString();
      }
    }
    #endregion
  }
}
