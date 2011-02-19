using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Configuration;
using NUnit.Framework;
using System.Collections.Specialized;

namespace NUnit.Tests
{
  [TestFixture]
  public class DataTest
  {
    private AdapterProvider _adapterProvider = null;
    private AdapterSettings _settings = null;

    public DataTest()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);

      _settings["BaseDirectoryPath"] = @"C:\iring-tools-2.0.x\Tests\NUnit.Tests";
      _settings["ProjectName"] = "12345_000";
      _settings["ApplicationName"] = "ABC";
      _settings["GraphName"] = "Lines";
      _settings["Identifier"] = "66015-O";

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      _adapterProvider = new AdapterProvider(_settings);
    }

    [Test]
    public void GetIndividual()
    {
      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"], _settings["Identifier"],
          "Data", false
        );

      string path = String.Format(
         "{0}GetIndividual.xml",
         _settings["XmlPath"]
       );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetIndex()
    {
      XDocument xDocument = 
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"], 
          _settings["GraphName"], 
          "Data", 
          0, 0, null, null, 
          false, 
          null
        );

      string path = String.Format(
          "{0}GetIndex.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetFullIndex()
    {
      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          0, 0, null, null,
          true,
          null
        );

      string path = String.Format(
          "{0}GetFullIndex.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetPageIndex()
    {
      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          10, 5, null, null,
          false,
          null
        );

      string path = String.Format(
          "{0}GetPageIndex.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetPageFull()
    {
      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          10, 5, null, null,
          true,
          null
        );

      string path = String.Format(
          "{0}GetPageFull.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetFilterIndex()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("System", "SC"); 

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          0, 0, null, null,
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetFilterIndex.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetFilterFull()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("System", "SC");

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          0, 0, null, null,
          true,
          parameters
        );

      string path = String.Format(
          "{0}GetFilterFull.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetSortIndex()
    {
      NameValueCollection parameters = new NameValueCollection();

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          0, 0, "Asc", "System",
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetSortIndex.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetSortFull()
    {
      NameValueCollection parameters = new NameValueCollection();

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          0, 0, "Asc", "System",
          true,
          parameters
        );

      string path = String.Format(
          "{0}GetSortFull.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetFilterPageSort()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("Area", "90");

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          10, 5, "Desc", "System",
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetFilterPageSort.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }

    [Test]
    public void GetFilterPageSortFull()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("Area", "90");

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Data",
          10, 5, "Desc", "System",
          true,
          parameters
        );

      string path = String.Format(
          "{0}GetFilterPageSortFull.xml",
          _settings["XmlPath"]
        );

      xDocument.Save(path);

      Assert.AreNotEqual(null, xDocument);
    }
  }
}

