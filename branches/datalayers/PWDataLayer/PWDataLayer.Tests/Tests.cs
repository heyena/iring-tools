﻿using System;
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

    public Tests()
    {
      string baseDir = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));

      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(new AppSettingsReader("app.config"));

      _dataLayer = new PWDataLayer(adapterSettings);
    }

    //[Test]
    public void TestDictionary()
    {
      Response response = _dataLayer.RefreshAll();
      Assert.AreEqual(response.Level, StatusLevel.Success);
    }

    //[Test]
    public void TestGet()
    {
      long count = _dataLayer.GetCount("DTP_ENG2", null);
      Assert.Greater(count, 0);

      DataTable dt = _dataLayer.GetDataTable("DTP_ENG2", string.Empty, 0, 25);
      string docGuid = dt.Rows[0]["DocumentGUID"].ToString();

      dt = _dataLayer.GetDataTable("DTP_ENG2", new List<string> { docGuid });
      Assert.Greater(dt.Rows.Count, 0);
    }

    [Test]
    public void TestGetContents()
    {
      IDictionary<string, string> pairs = new Dictionary<string, string>()
      {
        {"8bdd1237-7e3f-415b-9e21-ad18b5d64f2b", "doc"},
        {"c3831c9b-a2de-447e-96c6-fae0a26c6ac9", "pdf"}
      };

      IList<IContentObject> objects = _dataLayer.GetContents("DTP_ENG2", pairs);

      Assert.Greater(objects.Count, 0);
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
    public void TestPostWithContent()
    {
      GenericContentObject contentObject = new GenericContentObject()
      {
        ObjectType = "DTP_ENG2"
      };

      contentObject.Content = Utility.ReadStream(@"C:\iring-tools\datalayers\pw\samples\sample.pdf");
      contentObject.ContentType = "pdf";

      contentObject.SetPropertyValue("DWGNO", "DWG-" + DateTime.Now.Ticks);
      
      _dataLayer.Post(new List<IDataObject>() { contentObject });
    }

    //[Test]
    //public void TestGetFolders()
    //{
    //  SortedList<int, string> folders = _dataLayer.GetTopLevelFolders();

    //  foreach (var pair in folders)
    //  {
    //    SortedList<int, string>  childFolders = _dataLayer.GetChildFolders(pair.Key);

    //    foreach (var grandChildFolder in childFolders)
    //    {
    //      SortedList<int, string> grandChildFolders = _dataLayer.GetChildFolders(grandChildFolder.Key);
    //    }
    //  }
    //}
  }
}