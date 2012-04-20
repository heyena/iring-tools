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

namespace bechtel.eb.datalayer.test
{
  [TestFixture]
  public class Tests
  {
    private IDataLayer2 _dataLayer = null;
    private string _objectType = string.Empty;
    private DataObject _objectDefinition = null;
    private string _modifiedProperty = string.Empty;
    private DataFilter _filter = null;

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

    //[Test]
    public void TestCreate()
    {
      IList<IDataObject> dataObjects = _dataLayer.Create(_objectType, null);
      Assert.AreNotEqual(dataObjects, null);
    }

    //[Test]
    public void TestGetCount()
    {
      long count = _dataLayer.GetCount(_objectType, new DataFilter());
      Assert.Greater(count, 0);
    }

    [Test]
    public void TestGetPage()
    {
      IList<IDataObject> dataObjects = _dataLayer.Get("Mechanical(Tag)",new DataFilter(), 5, 0);
      Assert.Greater(dataObjects.Count, 0);
    }

    [Test]
    public void TestGetIdentifiers()
    {
        IList<string> identifiers =new List<string>();
        identifiers.Add("MECH-ME-01-001");
        identifiers.Add("MECH-ME-01-002");

        IList<IDataObject> dataObjects = _dataLayer.Get("Mechanical(Tag)", identifiers);
        Assert.Greater(dataObjects.Count, 0);
    }

    //[Test]
    public void TestGetWithIdentifiers()
    {
        IList<string> identifiers = _dataLayer.GetIdentifiers(_objectType, new DataFilter());
      IList<string> identifier = ((List<string>)identifiers).GetRange(1, 1);
      IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, identifier);
      Assert.Greater(dataObjects.Count, 0);
    }

    //[Test]
    public void TestGetCountWithFilter()
    {
      long count = _dataLayer.GetCount(_objectType, _filter);
      Assert.Greater(count, 0);
    }

    //[Test]
    public void TestGetPageWithFilter()
    {
      IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, _filter, 5, 0);
      Assert.Greater(dataObjects.Count, 0);
    }

    //[Test]
    public void TestGetIdentifiersWithFilter()
    {
      IList<string> identifiers = _dataLayer.GetIdentifiers(_objectType, _filter);
      Assert.Greater(identifiers.Count, 0);
    }

    //[Test]
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

    //[Test]
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

    //[Test]
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
    public void TestPost()
    {
      IDataObject dataObject = new GenericDataObject() { ObjectType = "Mechanical(Tag)" };
      dataObject.SetPropertyValue("ApprDiameter", "");
      dataObject.SetPropertyValue("ApprDPress", "");
      dataObject.SetPropertyValue("ApprDTemp", "");
      dataObject.SetPropertyValue("ApprInsulMaterial", "");
      dataObject.SetPropertyValue("ApprInsulationThickness", "");
      dataObject.SetPropertyValue("ApprLineClass", "");
      dataObject.SetPropertyValue("ApprovalStatus", "");
      dataObject.SetPropertyValue("ApprovedAQClass", "");
      dataObject.SetPropertyValue("ApprovedDesignPressure", "");
      dataObject.SetPropertyValue("ApprovedDesignTemperature", "");
      dataObject.SetPropertyValue("ApprovedEstLoad", "");
      dataObject.SetPropertyValue("ApprovedLoad", "");
      dataObject.SetPropertyValue("ApprovedNon-PressureBoundryQualityGroup", "");
      dataObject.SetPropertyValue("ApprovedPressureBoundryQualityGroup", "NA");
      dataObject.SetPropertyValue("ApprovedQualityClass", "");
      dataObject.SetPropertyValue("ApprovedSeismicClass", "SC-II");
      dataObject.SetPropertyValue("ChangeControlled", "");
      dataObject.SetPropertyValue("ClassCode", "");
      dataObject.SetPropertyValue("ClassId", "");
      dataObject.SetPropertyValue("Code", "01-N-CCW-ME-001Z");    // Tag Number
      dataObject.SetPropertyValue("Description", "CCW HEAT EXCHANGER");
      dataObject.SetPropertyValue("CommGrpCode", "ME");
      dataObject.SetPropertyValue("CommodityCode", "MES0");
      dataObject.SetPropertyValue("Unit", "01");
      dataObject.SetPropertyValue("ComponentFunctionName(CFN)", "1-N-CCW-HX-ZZ");
      dataObject.SetPropertyValue("DateEffective", "");
      dataObject.SetPropertyValue("DateObsolete", "");
      dataObject.SetPropertyValue("DateEffective", "");
      dataObject.SetPropertyValue("Discipline", "");
      dataObject.SetPropertyValue("DocumentStatus", "");
      dataObject.SetPropertyValue("EquipmentRating", "18200000");
      dataObject.SetPropertyValue("HP(Est/Act/Calc)", "");
      dataObject.SetPropertyValue("Name", "TEST-01-N-CCW-ME-001Z");
      dataObject.SetPropertyValue("Manufacturer", "");
      dataObject.SetPropertyValue("Middle", "");
      dataObject.SetPropertyValue("ModelNo", "");
      dataObject.SetPropertyValue("PowerReqd", "");
      dataObject.SetPropertyValue("Q-List(Y/N)", "N");
      dataObject.SetPropertyValue("PowerReqd", "N");
      dataObject.SetPropertyValue("ReasonforIssue", "");
      dataObject.SetPropertyValue("RegulatoryRequirement", "");
      dataObject.SetPropertyValue("Remark", "");
      dataObject.SetPropertyValue("Revision", "");
      dataObject.SetPropertyValue("SafetyDesignator", "");
      dataObject.SetPropertyValue("Synopsis", "");
      dataObject.SetPropertyValue("VendorSupplied(Y/N)", "");

      IList<IDataObject> dataObjects = new List<IDataObject>() { dataObject };
      Response response = _dataLayer.Post(dataObjects);

      Assert.AreEqual(response.Level, StatusLevel.Success);
    }

    //[Test]
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
