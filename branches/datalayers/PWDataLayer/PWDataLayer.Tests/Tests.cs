using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Data;
using System.Text;
using org.iringtools.adapter.datalayer;
using log4net;
using NUnit.Framework;
using StaticDust.Configuration;
using System.Runtime.Serialization;

namespace org.iringtools.adapter.datalayer.test
{
  [TestFixture]
  public class Tests
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(Tests));
    private PWDataLayer _dataLayer = null;
    private string _objectType;
      //IDataLayer _dataLayer1 = null;
      //AdapterSettings adapterSettings;
    public Tests()
    {
        //_objectType = "DTP_ENG2";
        _objectType = "Simple";
      string baseDir = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));

      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(new AppSettingsReader("app.config"));

      string pwConfigFile = String.Format("{0}{1}.{2}.config",
               adapterSettings["AppDataPath"],
               adapterSettings["ProjectName"],
               adapterSettings["ApplicationName"]
             );

      AppSettingsReader twSettings = new AppSettingsReader(pwConfigFile);
      adapterSettings.AppendSettings(twSettings);

      _dataLayer = new PWDataLayer(adapterSettings);
     
      
    }
   

    [Test]
    public void TestDictionary()
    {
      Response response = _dataLayer.RefreshAll();
      Assert.AreEqual(response.Level, StatusLevel.Success);
    }


    //[Test]
    //public void TestDocumentForFolder()
    //{
    //    List<string> identifiers = new List<string>();

    //    identifiers.Clear();
    //    identifiers.Add("DTP_ENG2");
    //    DataTable dt = _dataLayer.GetDocumentsForFolder(217, identifiers);
    //    Assert.Greater(dt.Rows.Count, 0);
    //    //Assert.AreEqual(response.Level, StatusLevel.Success);
    //}

   
   

    [Test]
    public void TestGet()
    {
        IList<string> identifiers = new List<string>();
        identifiers.Add("93");
        IList<IDataObject> dataObject = _dataLayer.Get("Folders", identifiers);
        
        //long count = _dataLayer.GetCount("DTP_ENG2", null);
        long count = _dataLayer.GetCount("Simple", null);
      Assert.Greater(count, 0);
      DataTable dt = _dataLayer.GetDataTable("Folders", string.Empty, 0, 25);
      //dt = _dataLayer.GetDataTable("DTP_ENG2", string.Empty, 0, 25);
      dt = _dataLayer.GetDataTable("Simple", string.Empty, 0, 25);
      string docGuid = dt.Rows[0]["DocumentGUID"].ToString();

      //dt = _dataLayer.GetDataTable("DTP_ENG2", new List<string> { docGuid });
      dt = _dataLayer.GetDataTable("Simple", new List<string> { docGuid });
      Assert.Greater(dt.Rows.Count, 0);
    }

    //[Test]
    //public void TestGetContents()
    //{
    //  IDictionary<string, string> idFormats = new Dictionary<string, string>()
    //  {
    //    {"8bdd1237-7e3f-415b-9e21-ad18b5d64f2b", "doc"},        
    //    {"e7ac674a-1a58-46c8-9460-c500e6776573", "pdf"}
    //  };

    //  IList<IContentObject> contentObjects = _dataLayer.GetContents("DTP_ENG2", idFormats); 

    //  Assert.Greater(contentObjects.Count, 0);
    //}
    [Test]
    public void Test_GetDataTable()
    {
        DataDictionary dictionary = _dataLayer.GetDictionary();
        IList<string> identifiers = new List<string>();
        identifiers.Add("4a44297f-3456-4e70-a073-ae0a3e92dae0");
        IList<IDataObject> dataObject = _dataLayer.Get(_objectType, identifiers);
        identifiers.Clear();
        identifiers.Add("212"); 
//        IList<IDataObject> dataObject1 = _dataLayer.Get("Documents", identifiers);

        
        Assert.AreEqual(dataObject.Count, 1);
    }

    [Test]
    public void TestCreate()
    {
      
        IList<string> identifiers = new List<string>();
        identifiers.Add("4a44297f-3456-4e70-a073-ae0a3e92dae0");
        IList<IDataObject> dataObjects = _dataLayer.Create(_objectType, identifiers);
        Assert.AreNotEqual(dataObjects, null);
    }
   // [Test]
    public void TestGetIdentifiers()
    {

        string docGUID = "d730d646-fccc-47b1-b974-97ec9c0ddb90";
        IList<string> GetID = _dataLayer.GetIdentifiers(_objectType, docGUID);
        //Assert.AreEqual(response.Level, StatusLevel.Success);
    }

     [Test]
    public void TestPostContents()
    {

        string docGUID = "4a44297f-3456-4e70-a073-ae0a3e92dae0";
       // IList<string> GetID = _dataLayer.PostContents(_objectType, docGUID);
        //Assert.AreEqual(response.Level, StatusLevel.Success);
    }
    //[Test]
    public void TestPost()
    {
      IDataObject contentObject = new GenericDataObject()
      {
        ObjectType = "DTP_ENG2"
      };

      contentObject.SetPropertyValue("DWGNO", "DWG-" + DateTime.Now.Ticks);

      _dataLayer.Post(new List<IDataObject>() { contentObject });
    }

    //[Test]
    public void TestDelete()
    {
      string docGUID = "d730d646-fccc-47b1-b974-97ec9c0ddb90";
      Response response = _dataLayer.Delete("DTP_ENG2", new List<string> { docGUID });
      Assert.AreEqual(response.Level, StatusLevel.Success);
    }
    //[Test]
    public void TestDeleteDataTable()
    {
        string docGUID = "d730d646-fccc-47b1-b974-97ec9c0ddb90";
        Response response = _dataLayer.DeleteDataTable("DTP_ENG2", new List<string> { docGUID });
        Assert.AreEqual(response.Level, StatusLevel.Success);
    }
    [Test]
    public void TestPostWithContent()
    {
      GenericContentObject contentObject = new GenericContentObject()
      {
        ObjectType = "DTP_ENG2"
      };

     // contentObject.Content = Utility.ReadStream(@"C:\iring-tools\datalayers\pw\samples\sample.pdf");
      contentObject.Content = Utility.ReadStream(@"C:\PW\samples\sample.pdf");
      contentObject.ContentType = "pdf";

      contentObject.SetPropertyValue("DWGNO", "DWG-" + DateTime.Now.Ticks);
      
      _dataLayer.Post(new List<IDataObject>() { contentObject });
    }
   

    //[Test]
    //public void TestGetFolders()
    //{
    //    SortedList<int, string> folders = _dataLayer.GetTopLevelFolders();

    //    foreach (var pair in folders)
    //    {
    //        SortedList<int, string> childFolders = _dataLayer.GetChildFolders(pair.Key);

    //        foreach (var grandChildFolder in childFolders)
    //        {
    //            SortedList<int, string> grandChildFolders = _dataLayer.GetChildFolders(grandChildFolder.Key);
    //        }
    //    }
    //}
  }
}