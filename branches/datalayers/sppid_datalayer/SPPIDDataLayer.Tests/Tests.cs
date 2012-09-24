using System.Collections.Generic;
using NUnit.Framework;
using org.iringtools.library;
using IU=org.iringtools.utility;
using StaticDust.Configuration;
using System.IO;
using System;

namespace org.iringtools.adapter.datalayer.sppid.test
{
  [TestFixture]
  public class Tests
  {
    private IDataLayer2 _dataLayer;
    private AdapterSettings _settings;

    public Tests()
    {
      string baseDir = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));

      _settings = new AdapterSettings();
      _settings.AppendSettings(new AppSettingsReader("app.config"));

      _dataLayer = new SPPIDDataLayer(_settings);
      _dataLayer.GetDictionary();
    }

    //[Test]
    public void TestRefresh()
    {
      Response response = _dataLayer.Refresh(string.Empty);
      Assert.AreEqual(response.Level, StatusLevel.Success);
    }

    [Test]
    public void TestCount()
    {
      long count = _dataLayer.GetCount("Equipment", new DataFilter());
      Assert.Greater(count, 0);
    }

    [Test]
    public void TestGetPage()
    {
      IList<IDataObject> dataObjects = _dataLayer.Get("Equipment", new DataFilter(), 5, 0);
      Assert.Greater(dataObjects.Count, 0);
    }

    [Test]
    public void TestGetWithIdentifiers()
    {
      IList<string> identifiers = new List<string> 
        { "C7215C9E75DD4A85BB5779C3D29A883D", "51BD7C1DAD8743D99C813083322C14D8" };
      IList<IDataObject> dataObjects = _dataLayer.Get("Equipment", identifiers);
      Assert.Greater(dataObjects.Count, 0);
    }

    [Test]
    public void TestGetIdentifiers()
    {
      string filterPath = string.Format("{0}{1}.{2}.filter.xml",
        _settings[Constants.DATA_PATH],
        _settings[Constants.PROJECT],
        _settings[Constants.APPLICATION]);

      DataFilter filter = IU.Utility.Read<DataFilter>(filterPath);
      IList<string> identifiers = _dataLayer.GetIdentifiers("Equipment", filter);
      Assert.Greater(identifiers.Count, 0);
    }

    [Test]
    public void TestCreateWithNoIdentifier()
    {
      IList<string> identifiers = new List<string>();
      IList<IDataObject> dataObjects = _dataLayer.Create("Equipment", identifiers);
      Assert.Greater(dataObjects.Count, 0);
    }

    [Test]
    public void TestCreateWithExistAndNonExistingIdentifiers()
    {
      IList<string> identifiers = new List<string> { 
        "C7215C9E75DD4A85BB5779C3D29A883D", 
        "12345",
        "" 
      };

      IList<IDataObject> dataObjects = _dataLayer.Create("Equipment", identifiers);
      Assert.Greater(dataObjects.Count, 0);
    }

    //[Test]
    public void TestDeleteWithIdentifiers()
    {
      IList<string> identifiers = new List<string> { 
        "C7215C9E75DD4A85BB5779C3D29A883D",
        "12345"
      };

      Response response = _dataLayer.Delete("Equipment", identifiers);
      Assert.AreNotEqual(response.Level, StatusLevel.Error);
    }

    //[Test]
    public void TestDeleteWithFilter()
    {
      string filterPath = string.Format("{0}{1}.{2}.filter.xml",
        _settings[Constants.DATA_PATH],
        _settings[Constants.PROJECT],
        _settings[Constants.APPLICATION]);

      DataFilter filter = IU.Utility.Read<DataFilter>(filterPath);
      Response response = _dataLayer.Delete("Equipment", filter);
      Assert.AreNotEqual(response.Level, StatusLevel.Error);
    }

    [Test]
    public void TestAddAndUpdate()
    {
      IList<IDataObject> dataObjects = _dataLayer.Get("Equipment",
        new List<string> { "12345" });

      foreach (IDataObject dataObject in dataObjects)
      {
        dataObject.SetPropertyValue("SupplyByID", Guid.NewGuid().ToString());
      }

      // create new data object
      IDataObject newDataObject = _dataLayer.Create("Equipment", null)[0];
      DataDictionary dictionary = _dataLayer.GetDictionary();
      DataObject objDef = dictionary.dataObjects.Find(x => x.objectName == "Equipment");

      foreach (DataProperty prop in objDef.dataProperties)
      {
        string propName = prop.propertyName;
        newDataObject.SetPropertyValue(propName, dataObjects[0].GetPropertyValue(propName));
      }

      newDataObject.SetPropertyValue("SP_ID", "123456");
      newDataObject.SetPropertyValue("SupplyByID", Guid.NewGuid().ToString());
      dataObjects.Add(newDataObject);

      Response response = _dataLayer.Post(dataObjects);
      Assert.AreNotEqual(response.Level, StatusLevel.Error);
    }
  }
}
