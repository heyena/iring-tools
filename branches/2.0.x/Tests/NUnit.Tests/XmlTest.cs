﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;

namespace NUnit.Tests
{
  [TestFixture]
  public class XmlTest
  {
    private AdapterProvider _adapterProvider = null;
    private AdapterSettings _settings = null;
    private string _baseDirectory = string.Empty;

    public XmlTest()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);

     // _settings["BaseDirectoryPath"] = @"E:\iring-tools\branches\2.0.x\Tests\NUnit.Tests";
      _settings["ProjectName"] = "12345_000";
      _settings["ApplicationName"] = "ABC";
      _settings["GraphName"] = "Lines";
      _settings["Identifier"] = "66015-O";
			_settings["TestMode"] = "UseFiles"; //UseFiles/WriteFiles

      _baseDirectory = Directory.GetCurrentDirectory();
      _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
      _settings["BaseDirectoryPath"] = _baseDirectory;
      Directory.SetCurrentDirectory(_baseDirectory);

      _adapterProvider = new AdapterProvider(_settings);
    }

    [Test]
    public void GetIndividual()
    {
      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"], _settings["ClassName"],
          _settings["Identifier"],
          "Xml", false
        );

      string path = String.Format(
         "{0}GetXmlIndividual.xml",
         _settings["XmlPath"]
       );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetIndex()
    {
      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 0, null, null,
          false,
          null
        );

      string path = String.Format(
          "{0}GetXmlIndex.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetFull()
    {
      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 0, null, null,
          true,
          null
        );

      string path = String.Format(
          "{0}GetXmlFull.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetPageIndex()
    {
      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 5, null, null,
          false,
          null
        );

      string path = String.Format(
          "{0}GetXmlPageIndex.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }

      int total = 0;
      
      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        xDocument =
         _adapterProvider.GetDataProjection(
           _settings["ProjectName"], _settings["ApplicationName"],
           _settings["GraphName"],
           "Xml",
           i, 5, null, null,
           false,
           null
         );

        path = String.Format(
            "{0}GetXmlPageIndex.{1}.xml",
            _settings["XmlPath"],
            (i / 5) + 1
          );

        if (_settings["TestMode"].ToLower() != "usefiles")
        {
          xDocument.Save(path);
          Assert.AreNotEqual(null, xDocument);
        }
        else
        {
          benchmark = XDocument.Load(path);
          Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
        }
      }
    }

    [Test]
    public void GetPageFull()
    {
      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 5, null, null,
          true,
          null
        );

      string path = String.Format(
          "{0}GetXmlPageFull.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }

      int total = 0;
      
      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        xDocument =
         _adapterProvider.GetDataProjection(
           _settings["ProjectName"], _settings["ApplicationName"],
           _settings["GraphName"],
           "Xml",
           i, 5, null, null,
           true,
           null
         );

        path = String.Format(
            "{0}GetXmlPageFull.{1}.xml",
            _settings["XmlPath"],
            (i / 5) + 1
          );

        if (_settings["TestMode"].ToLower() != "usefiles")
        {
          xDocument.Save(path);
          Assert.AreNotEqual(null, xDocument);
        }
        else
        {
          benchmark = XDocument.Load(path);
          Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
        }
      }
    }

    [Test]
    public void GetFilterIndex()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("System", "SC");

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 0, null, null,
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetXmlFilterIndex.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetFilterFull()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("System", "SC");

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 0, null, null,
          true,
          parameters
        );

      string path = String.Format(
          "{0}GetXmlFilterFull.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetSortIndex()
    {
      NameValueCollection parameters = new NameValueCollection();

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 0, "Asc", "System",
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetXmlSortIndex.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetSortFull()
    {
      NameValueCollection parameters = new NameValueCollection();

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 0, "Asc", "System",
          true,
          parameters
        );

      string path = String.Format(
          "{0}GetXmlSortFull.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetFilterPageSortIndex()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("Area", "90");

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 5, "Desc", "System",
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetXmlFilterPageSortIndex.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }

      int total = 0;
      
      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        xDocument =
         _adapterProvider.GetDataProjection(
           _settings["ProjectName"], _settings["ApplicationName"],
           _settings["GraphName"],
           "Xml",
           i, 5, "Desc", "System",
           false,
           parameters
         );

        path = String.Format(
            "{0}GetXmlFilterPageSortIndex.{1}.xml",
            _settings["XmlPath"],
            (i / 5) + 1
          );

        if (_settings["TestMode"].ToLower() != "usefiles")
        {
          xDocument.Save(path);
          Assert.AreNotEqual(null, xDocument);
        }
        else
        {
          benchmark = XDocument.Load(path);
          Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
        }
      }
    }

    [Test]
    public void GetFilterPageSortFull()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("Area", "90");

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          "Xml",
          0, 5, "Desc", "System",
          true,
          parameters
        );

      string path = String.Format(
          "{0}GetXmlFilterPageSortFull.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }

      int total = 0;

      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        xDocument =
         _adapterProvider.GetDataProjection(
           _settings["ProjectName"], _settings["ApplicationName"],
           _settings["GraphName"],
           "Xml",
           i, 5, "Desc", "System",
           true,
           parameters
         );

        path = String.Format(
            "{0}GetXmlFilterPageSortFull.{1}.xml",
            _settings["XmlPath"],
            (i / 5) + 1
          );

        if (_settings["TestMode"].ToLower() != "usefiles")
        {
          xDocument.Save(path);
          Assert.AreNotEqual(null, xDocument);
        }
        else
        {
          benchmark = XDocument.Load(path);
          Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
        }
      }
    }

    [Test]
    public void GetDataFilterIndex()
    {
      DataFilter filter = new DataFilter();

      Expression expression = new Expression
      {
        PropertyName = "System",
        Values = new Values
         {
           "SC"
         },
        RelationalOperator = RelationalOperator.EqualTo
      };

      filter.Expressions.Add(expression);

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          filter, "Xml",
          0, 0,
          false
        );

      string path = String.Format(
          "{0}GetXmlDataFilterIndex.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetDataFilterFull()
    {
      DataFilter filter = new DataFilter();

      Expression expression = new Expression
      {
        PropertyName = "System",
        Values = new Values
         {
           "SC"
         },
        RelationalOperator = RelationalOperator.EqualTo
      };

      filter.Expressions.Add(expression);

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          filter, "Xml",
          0, 0,
          true
        );

      string path = String.Format(
          "{0}GetXmlDataFilterFull.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }
    }

    [Test]
    public void GetDataFilterPageSortIndex()
    {
      DataFilter filter = new DataFilter();

      Expression expression = new Expression
      {
        PropertyName = "Area",
        Values = new Values
         {
           "90"
         },
        RelationalOperator = RelationalOperator.EqualTo
      };

      filter.Expressions.Add(expression);

      OrderExpression orderExpression = new OrderExpression
      {
        PropertyName = "System",
        SortOrder = SortOrder.Desc
      };

      filter.OrderExpressions.Add(orderExpression);

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          filter, "Xml",
          0, 5,
          false
        );

      string path = String.Format(
          "{0}GetXmlDataFilterPageSortIndex.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }

      int total = 0;

      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        //the Provider clears the orderExpressions for getCount
        //not a problem for webservice, but we need to handle it here.
        filter.OrderExpressions.Add(orderExpression);

        xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          filter, "Xml",
          i, 5,
          false
        );

        path = String.Format(
            "{0}GetXmlDataFilterPageSortIndex.{1}.xml",
            _settings["XmlPath"],
            (i / 5) + 1
          );

        if (_settings["TestMode"].ToLower() != "usefiles")
        {
          xDocument.Save(path);
          Assert.AreNotEqual(null, xDocument);
        }
        else
        {
          benchmark = XDocument.Load(path);
          Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
        }
      }
    }

    [Test]
    public void GetDataFilterPageSortFull()
    {
      DataFilter filter = new DataFilter();

      Expression expression = new Expression
      {
        PropertyName = "Area",
        Values = new Values
         {
           "90"
         },
        RelationalOperator = RelationalOperator.EqualTo
      };

      filter.Expressions.Add(expression);

      OrderExpression orderExpression = new OrderExpression
      {
        PropertyName = "System",
        SortOrder = SortOrder.Desc
      };

      filter.OrderExpressions.Add(orderExpression);

      XDocument benchmark = null;

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          filter, "Xml",
          10, 5,
          true
        );

      string path = String.Format(
          "{0}GetXmlDataFilterPageSortFull.xml",
          _settings["XmlPath"]
        );

      if (_settings["TestMode"].ToLower() != "usefiles")
      {
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
      }
      else
      {
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      }

      int total = 0;

      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        //the Provider clears the orderExpressions for getCount
        //not a problem for webservice, but we need to handle it here.
        filter.OrderExpressions.Add(orderExpression);

        xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          filter, "Xml",
          i, 5,
          true
        );

        path = String.Format(
            "{0}GetXmlDataFilterPageSortFull.{1}.xml",
            _settings["XmlPath"],
            (i / 5) + 1
          );

        if (_settings["TestMode"].ToLower() != "usefiles")
        {
          xDocument.Save(path);
          Assert.AreNotEqual(null, xDocument);
        }
        else
        {
          benchmark = XDocument.Load(path);
          Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
        }
      }
    }
  }
}
